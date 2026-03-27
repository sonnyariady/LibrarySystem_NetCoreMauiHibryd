using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Library.Api.Common.Api;
using Library.Api.Common.Crud;
using Library.Api.Common.Excel;

namespace Library.Api.Common.Controllers;

[ApiController]
public abstract class CrudControllerBase<TDbContext, TEntity, TKey> : ControllerBase
    where TDbContext : DbContext
    where TEntity : class
{
    private readonly ICrudQueryService<TDbContext, TEntity, TKey> _svc;
    private readonly IExcelExporter _excel;
    private readonly Func<TEntity, TKey> _keySelector;

    protected CrudControllerBase(
        ICrudQueryService<TDbContext, TEntity, TKey> svc,
        IExcelExporter excel,
        Func<TEntity, TKey> keySelector)
    {
        _svc = svc;
        _excel = excel;
        _keySelector = keySelector;
    }

    [HttpGet("{id}")]
    public async Task<ApiResponseGeneral<TEntity>> Get(TKey id, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetAsync(id, ct);
            return data is null
                ? ApiResponseGeneral<TEntity>.Fail("Not found")
                : ApiResponseGeneral<TEntity>.Ok(data);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return ApiResponseGeneral<TEntity>.Fail("Request was cancelled");
        }
    }

    [HttpPost("search")]
    public async Task<ApiResponseGeneral<PagedResultGeneral<TEntity>>> SearchPost(
        [FromBody] CrudSearchRequest req,
        CancellationToken ct = default)
    {
        try
        {
            var page = req.Page < 1 ? 1 : req.Page;
            var pageSize = req.PageSize is < 1 or > 500 ? 20 : req.PageSize;

            var paging = new PagedRequestGeneral(page, pageSize, req.SortBy, req.SortDesc);
            var data = await _svc.SearchAsync(req.Q, req.Filters, paging, ct);

            return ApiResponseGeneral<PagedResultGeneral<TEntity>>.Ok(data);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return ApiResponseGeneral<PagedResultGeneral<TEntity>>.Ok(
                new PagedResultGeneral<TEntity>(Array.Empty<TEntity>(), 0, 1, 20, 0));
        }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("search")]
    public async Task<ApiResponseGeneral<PagedResultGeneral<TEntity>>> SearchGet(
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        [FromQuery] Dictionary<string, string?>? filters = null,
        CancellationToken ct = default)
    {
        try
        {
            var paging = new PagedRequestGeneral(page, pageSize, sortBy, sortDesc);
            var data = await _svc.SearchAsync(q, filters, paging, ct);

            return ApiResponseGeneral<PagedResultGeneral<TEntity>>.Ok(data);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return ApiResponseGeneral<PagedResultGeneral<TEntity>>.Ok(
                new PagedResultGeneral<TEntity>(Array.Empty<TEntity>(), 0, 1, 20, 0));
        }
    }

    [HttpPost("upsert")]
    public async Task<ApiResponseGeneral<TEntity>> Upsert([FromBody] TEntity entity, CancellationToken ct)
    {
        try
        {
            var saved = await _svc.UpsertAsync(entity, _keySelector, ct);
            return ApiResponseGeneral<TEntity>.Ok(saved);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return ApiResponseGeneral<TEntity>.Fail("Request was cancelled");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponseGeneral<bool>> Delete(TKey id, CancellationToken ct)
    {
        try
        {
            var ok = await _svc.DeleteAsync(id, ct);
            return ok
                ? ApiResponseGeneral<bool>.Ok(true)
                : ApiResponseGeneral<bool>.Fail("Not found");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return ApiResponseGeneral<bool>.Fail("Request was cancelled");
        }
    }

    [HttpPost("export")]
    public async Task<IActionResult> ExportExcel([FromBody] ExcelExportRequest req, CancellationToken ct)
    {
        try
        {
            var rows = await _svc.SearchRawAsync(req.Q, req.Filters, req.SortBy, req.SortDesc, ct);

            var sheet = string.IsNullOrWhiteSpace(req.SheetName) ? "Sheet1" : req.SheetName!;
            var bytes = _excel.ExportRawToXlsx(rows, req.HeaderMap, sheet);

            var fileName = string.IsNullOrWhiteSpace(req.FileName)
                ? $"{typeof(TEntity).Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx"
                : (req.FileName!.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
                    ? req.FileName!
                    : req.FileName! + ".xlsx");

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return NoContent();
        }
    }
}