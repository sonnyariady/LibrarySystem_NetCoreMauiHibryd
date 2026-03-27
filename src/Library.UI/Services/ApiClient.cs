using System.Net.Http.Headers;
using System.Net.Http.Json;
using Library.UI.Models;

namespace Library.UI.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<PagedResultGeneral<T>> SearchAsync<T>(
        string endpoint,
        string? q = null,
        int page = 1,
        int pageSize = 20,
        string? sortBy = null,
        bool sortDesc = false,
        Dictionary<string, string?>? filters = null)
    {
        try
        {
            var req = new CrudSearchRequest(q, page, pageSize, sortBy, sortDesc, filters);

            var res = await _http.PostAsJsonAsync($"api/{endpoint}/search", req);
            res.EnsureSuccessStatusCode();

            var payload = await res.Content.ReadFromJsonAsync<ApiResponseGeneral<PagedResultGeneral<T>>>();

            if (payload is null)
                return new PagedResultGeneral<T>(Array.Empty<T>(), 0, page, pageSize, 0);

            if (!payload.Success)
                return new PagedResultGeneral<T>(Array.Empty<T>(), 0, page, pageSize, 0);

            return payload.Data ?? new PagedResultGeneral<T>(Array.Empty<T>(), 0, page, pageSize, 0);
        }
        catch (OperationCanceledException exx)
        {
            return new PagedResultGeneral<T>(Array.Empty<T>(), 0, page, pageSize, 0);
        }
        catch (Exception ex)
        {
            return new PagedResultGeneral<T>(Array.Empty<T>(), 0, page, pageSize, 0);
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint, int id) where T : class
    {
        try
        {
            var payload = await _http.GetFromJsonAsync<ApiResponseGeneral<T>>($"api/{endpoint}/{id}");

            if (payload is null || !payload.Success)
                return null;

            return payload.Data;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        
    }

    public async Task<T?> UpsertAsync<T>(string endpoint, T data)
    {
        try
        {
            var res = await _http.PostAsJsonAsync($"api/{endpoint}/upsert", data);
            res.EnsureSuccessStatusCode();

            var payload = await res.Content.ReadFromJsonAsync<ApiResponseGeneral<T>>();

            if (payload is null || !payload.Success)
                return default;

            return payload.Data;
        }
        catch (OperationCanceledException)
        {
            return default;
        }
       
    }

    public async Task DeleteAsync(string endpoint, int id)
    {
        try
        {
            var res = await _http.DeleteAsync($"api/{endpoint}/{id}");
            res.EnsureSuccessStatusCode();
        }
        catch (OperationCanceledException)
        {
        }
        
    }

    public async Task<ExportFileResult> ExportExcelAsync(string endpoint, ExcelExportRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/{endpoint}/export", request);
        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var contentType = response.Content.Headers.ContentType?.ToString()
                          ?? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        var fileName = "export.xlsx";

        if (response.Content.Headers.ContentDisposition is ContentDispositionHeaderValue disposition)
        {
            fileName = disposition.FileNameStar ?? disposition.FileName ?? fileName;
        }
        else if (response.Content.Headers.TryGetValues("Content-Disposition", out var values))
        {
            var raw = values.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(raw))
            {
                var parsed = ContentDispositionHeaderValue.Parse(raw);
                fileName = parsed.FileNameStar ?? parsed.FileName ?? fileName;
            }
        }

        fileName = fileName.Trim('"');

        return new ExportFileResult
        {
            FileName = fileName,
            ContentType = contentType,
            Bytes = bytes
        };
    }
}