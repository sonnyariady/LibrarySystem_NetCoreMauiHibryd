using Microsoft.EntityFrameworkCore;
using Library.Api.Common.Api;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Library.Api.Common.Crud;

public interface ISearchableEntity
{
    static abstract IReadOnlyList<string> SearchableColumns { get; }
}

public class EfCrudQueryService<TDbContext, TEntity, TKey> : ICrudQueryService<TDbContext, TEntity, TKey>
    where TDbContext : DbContext
    where TEntity : class
{
    private readonly TDbContext _db;
    private readonly DbSet<TEntity> _set;

    public EfCrudQueryService(TDbContext db)
    {
        _db = db;
        _set = db.Set<TEntity>();
    }

    public Task<TEntity?> GetAsync(TKey id, CancellationToken ct = default)
        => _set.FindAsync([id], ct).AsTask();

    public async Task<TEntity> UpsertAsync(TEntity entity, Func<TEntity, TKey> keySelector, CancellationToken ct = default)
    {
        var key = keySelector(entity);
        var existing = await _set.FindAsync([key], ct);

        if (existing is null || EqualityComparer<TKey>.Default.Equals(key, default!))
        {
            _set.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        _db.Entry(existing).CurrentValues.SetValues(entity);
        await _db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteAsync(TKey id, CancellationToken ct = default)
    {
        var existing = await _set.FindAsync([id], ct);
        if (existing is null) return false;

        _set.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<PagedResultGeneral<TEntity>> SearchAsync(
        string? q,
        IDictionary<string, string?>? filters,
        PagedRequestGeneral paging,
        CancellationToken ct = default)
    {
        try
        {
            var query = BuildSearchQuery(q, filters);

            var page = paging.Page < 1 ? 1 : paging.Page;
            var pageSize = paging.PageSize is < 1 or > 500 ? 20 : paging.PageSize;

            var total = await query.LongCountAsync(ct);

            query = ApplySorting(query, paging.SortBy, paging.SortDesc);

            var totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResultGeneral<TEntity>(items, total, page, pageSize, totalPages);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            var page = paging.Page < 1 ? 1 : paging.Page;
            var pageSize = paging.PageSize is < 1 or > 500 ? 20 : paging.PageSize;

            return new PagedResultGeneral<TEntity>(Array.Empty<TEntity>(), 0, page, pageSize, 0);
        }
    }

    public async Task<IReadOnlyList<TEntity>> SearchRawAsync(
        string? q,
        IDictionary<string, string?>? filters,
        string? sortBy,
        bool sortDesc,
        CancellationToken ct = default)
    {
        try
        {
            var query = BuildSearchQuery(q, filters);
            query = ApplySorting(query, sortBy, sortDesc);
            return await query.ToListAsync(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Array.Empty<TEntity>();
        }
    }

    private IQueryable<TEntity> BuildSearchQuery(string? q, IDictionary<string, string?>? filters)
    {
        IQueryable<TEntity> query = _set.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q) && typeof(ISearchableEntity).IsAssignableFrom(typeof(TEntity)))
            query = ApplyFreeTextSearch(query, q);

        if (filters is not null && filters.Count > 0)
            query = ApplyColumnFilters(query, filters);

        return query;
    }

    private static Expression BuildCaseInsensitiveLikeContains(Expression memberString, string value)
    {
        var toLower = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
        var notNull = Expression.NotEqual(memberString, Expression.Constant(null, typeof(string)));

        var lowerMember = Expression.Call(memberString, toLower);

        var functions = Expression.Property(null, typeof(EF).GetProperty(nameof(EF.Functions))!);

        var likeMethod = typeof(DbFunctionsExtensions)
            .GetMethods()
            .Single(m => m.Name == nameof(DbFunctionsExtensions.Like)
                         && m.GetParameters().Length == 3
                         && m.GetParameters()[1].ParameterType == typeof(string)
                         && m.GetParameters()[2].ParameterType == typeof(string));

        var pattern = Expression.Constant($"%{value.ToLowerInvariant()}%");
        var likeCall = Expression.Call(likeMethod, functions, lowerMember, pattern);

        return Expression.AndAlso(notNull, likeCall);
    }

    private static IQueryable<TEntity> ApplyFreeTextSearch(IQueryable<TEntity> query, string q)
    {
        var colsProp = typeof(TEntity).GetProperty("SearchableColumns");
        var cols = colsProp?.GetValue(null) as IReadOnlyList<string>;
        if (cols is null || cols.Count == 0) return query;

        var param = Expression.Parameter(typeof(TEntity), "x");
        Expression? body = null;

        foreach (var col in cols)
        {
            var member = Expression.PropertyOrField(param, col);
            if (member.Type != typeof(string)) continue;

            var expr = BuildCaseInsensitiveLikeContains(member, q);
            body = body is null ? expr : Expression.OrElse(body, expr);
        }

        if (body is null) return query;

        var lambda = Expression.Lambda<Func<TEntity, bool>>(body, param);
        return query.Where(lambda);
    }

    private static IQueryable<TEntity> ApplyColumnFilters(IQueryable<TEntity> query, IDictionary<string, string?> filters)
    {
        var param = Expression.Parameter(typeof(TEntity), "x");
        Expression? body = null;

        var rangeGroups = new Dictionary<string, (string? Start, string? End)>(StringComparer.OrdinalIgnoreCase);

        foreach (var (rawKey, rawValue) in filters)
        {
            if (string.IsNullOrWhiteSpace(rawKey) || string.IsNullOrWhiteSpace(rawValue))
                continue;

            var key = rawKey.Trim();
            var value = rawValue.Trim();

            if (TrySplitRangeKey(key, out var propName, out var bound))
            {
                if (!rangeGroups.TryGetValue(propName, out var ex))
                    ex = (null, null);

                if (bound == RangeBoundKind.Start) ex.Start = value;
                else ex.End = value;

                rangeGroups[propName] = ex;
                continue;
            }

            var pred = BuildSimplePredicate(param, key, value);
            if (pred is null) continue;

            body = body is null ? pred : Expression.AndAlso(body, pred);
        }

        foreach (var (propName, bounds) in rangeGroups)
        {
            var pred = BuildDateRangePredicate(param, propName, bounds.Start, bounds.End);
            if (pred is null) continue;

            body = body is null ? pred : Expression.AndAlso(body, pred);
        }

        if (body is null) return query;

        var lambda = Expression.Lambda<Func<TEntity, bool>>(body, param);
        return query.Where(lambda);
    }

    private enum RangeBoundKind { Start, End }

    private static bool TrySplitRangeKey(string key, out string propName, out RangeBoundKind kind)
    {
        propName = "";
        kind = RangeBoundKind.Start;

        if (key.EndsWith("Start", StringComparison.OrdinalIgnoreCase))
        {
            propName = key[..^5];
            kind = RangeBoundKind.Start;
            return !string.IsNullOrWhiteSpace(propName);
        }
        if (key.EndsWith("End", StringComparison.OrdinalIgnoreCase))
        {
            propName = key[..^3];
            kind = RangeBoundKind.End;
            return !string.IsNullOrWhiteSpace(propName);
        }
        if (key.EndsWith("From", StringComparison.OrdinalIgnoreCase))
        {
            propName = key[..^4];
            kind = RangeBoundKind.Start;
            return !string.IsNullOrWhiteSpace(propName);
        }
        if (key.EndsWith("To", StringComparison.OrdinalIgnoreCase))
        {
            propName = key[..^2];
            kind = RangeBoundKind.End;
            return !string.IsNullOrWhiteSpace(propName);
        }

        return false;
    }

    private static Expression? BuildSimplePredicate(ParameterExpression param, string key, string value)
    {
        var prop = typeof(TEntity).GetProperty(
            key,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
        );

        if (prop is null) return null;

        var member = Expression.Property(param, prop);
        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

        if (value.IndexOfAny([',', '|']) >= 0)
        {
            var parts = value
                .Split([',', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (parts.Length == 0) return null;

            if (targetType == typeof(string))
            {
                Expression? orChain = null;

                foreach (var p in parts)
                {
                    var rightString = Expression.Constant(p, typeof(string));
                    var eq = Expression.Equal(member, rightString);
                    orChain = orChain is null ? eq : Expression.OrElse(orChain, eq);
                }

                return orChain;
            }

            var typedValues = new List<object>(parts.Length);
            foreach (var p in parts)
            {
                if (!TryConvert(p, targetType, out var typed)) return null;
                typedValues.Add(typed!);
            }

            var array = Array.CreateInstance(targetType, typedValues.Count);
            for (var i = 0; i < typedValues.Count; i++)
                array.SetValue(typedValues[i], i);

            Expression left = member;
            if (member.Type != targetType)
                left = Expression.Convert(member, targetType);

            var listConst = Expression.Constant(array, array.GetType());

            var contains = typeof(Enumerable).GetMethods()
                .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2)
                .MakeGenericMethod(targetType);

            return Expression.Call(contains, listConst, left);
        }

        if (targetType == typeof(string))
            return BuildCaseInsensitiveLikeContains(member, value);

        if (!TryConvert(value, targetType, out var typedSingle)) return null;

        Expression leftSingle = member;
        if (member.Type != targetType)
            leftSingle = Expression.Convert(member, targetType);

        var right = Expression.Constant(typedSingle, targetType);
        return Expression.Equal(leftSingle, right);
    }

    private static Expression? BuildDateRangePredicate(ParameterExpression param, string propName, string? start, string? end)
    {
        var prop = typeof(TEntity).GetProperty(propName);
        if (prop is null) return null;

        var member = Expression.Property(param, prop);
        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

        var isDate =
            targetType == typeof(DateOnly) ||
            targetType == typeof(DateTime) ||
            targetType == typeof(DateTimeOffset);

        if (!isDate) return null;

        Expression? rangeBody = null;

        if (!string.IsNullOrWhiteSpace(start))
        {
            if (!TryConvertDate(start!, targetType, out var startObj)) return null;

            Expression left = member.Type == targetType ? member : Expression.Convert(member, targetType);
            var right = Expression.Constant(startObj, targetType);
            var ge = Expression.GreaterThanOrEqual(left, right);
            rangeBody = ge;
        }

        if (!string.IsNullOrWhiteSpace(end))
        {
            if (!TryConvertDate(end!, targetType, out var endObj)) return null;

            Expression left = member.Type == targetType ? member : Expression.Convert(member, targetType);
            var right = Expression.Constant(endObj, targetType);
            var le = Expression.LessThanOrEqual(left, right);
            rangeBody = rangeBody is null ? le : Expression.AndAlso(rangeBody, le);
        }

        return rangeBody;
    }

    private static bool TryConvert(string value, Type targetType, out object? converted)
    {
        try
        {
            if (targetType == typeof(Guid))
            {
                converted = Guid.Parse(value);
                return true;
            }

            if (targetType == typeof(DateOnly))
            {
                converted = DateOnly.Parse(value, CultureInfo.InvariantCulture);
                return true;
            }

            if (targetType.IsEnum)
            {
                converted = Enum.Parse(targetType, value, ignoreCase: true);
                return true;
            }

            converted = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            converted = null;
            return false;
        }
    }

    private static bool TryConvertDate(string value, Type targetType, out object? converted)
    {
        try
        {
            if (targetType == typeof(DateOnly))
            {
                converted = DateOnly.Parse(value, CultureInfo.InvariantCulture);
                return true;
            }

            if (targetType == typeof(DateTime))
            {
                converted = DateTime.Parse(value, CultureInfo.InvariantCulture);
                return true;
            }

            if (targetType == typeof(DateTimeOffset))
            {
                converted = DateTimeOffset.Parse(value, CultureInfo.InvariantCulture);
                return true;
            }

            converted = null;
            return false;
        }
        catch
        {
            converted = null;
            return false;
        }
    }

    private static IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, string? sortBy, bool sortDesc)
    {
        if (string.IsNullOrWhiteSpace(sortBy)) return query;

        var prop = typeof(TEntity).GetProperty(
            sortBy,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (prop is null) return query;

        var param = Expression.Parameter(typeof(TEntity), "x");
        var body = Expression.Property(param, prop);
        var keySelector = Expression.Lambda(body, param);

        var methodName = sortDesc ? "OrderByDescending" : "OrderBy";

        var call = Expression.Call(
            typeof(Queryable),
            methodName,
            [typeof(TEntity), prop.PropertyType],
            query.Expression,
            Expression.Quote(keySelector));

        return query.Provider.CreateQuery<TEntity>(call);
    }
}