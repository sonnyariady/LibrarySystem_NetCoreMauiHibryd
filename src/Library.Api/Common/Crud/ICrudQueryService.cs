using Library.Api.Common.Api;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Common.Crud;

public interface ICrudQueryService<TDbContext, TEntity, TKey>
    where TDbContext : DbContext
    where TEntity : class
{
    Task<TEntity?> GetAsync(TKey id, CancellationToken ct = default);

    Task<PagedResultGeneral<TEntity>> SearchAsync(
        string? q,
        IDictionary<string, string?>? filters,
        PagedRequestGeneral paging,
        CancellationToken ct = default);

    Task<IReadOnlyList<TEntity>> SearchRawAsync(
        string? q,
        IDictionary<string, string?>? filters,
        string? sortBy,
        bool sortDesc,
        CancellationToken ct = default);

    Task<TEntity> UpsertAsync(TEntity entity, Func<TEntity, TKey> keySelector, CancellationToken ct = default);
    Task<bool> DeleteAsync(TKey id, CancellationToken ct = default);
}
