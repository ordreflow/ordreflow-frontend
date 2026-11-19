using System.Globalization;
using System.Net.Http.Json;
using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public sealed class ExportsApiClient(HttpClient httpClient)
{
    public Task<ApiResult<ExportPageResponse<ExportEmployeeResponse>>> GetEmployeesAsync(
        string? search = null,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var query = $"?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            query += $"&search={Uri.EscapeDataString(search)}";

        return ApiResponse.ReadAsync<ExportPageResponse<ExportEmployeeResponse>>(
            () => httpClient.GetAsync($"api/exports/employees{query}", cancellationToken), cancellationToken);
    }

    public Task<ApiResult<ExportPageResponse<ExportSummaryResponse>>> GetExportsAsync(
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default) =>
        ApiResponse.ReadAsync<ExportPageResponse<ExportSummaryResponse>>(
            () => httpClient.GetAsync($"api/exports?page={page}&pageSize={pageSize}", cancellationToken), cancellationToken);

    public async Task<ApiResult<DownloadedFile>> CreateAndDownloadAsync(
        DateOnly fromDate,
        DateOnly toDate,
        Guid? employeeId = null,
        CancellationToken cancellationToken = default)
    {
        var create = await ApiResponse.ReadAsync<ExportDetailsResponse>(
            () => httpClient.PostAsJsonAsync("api/exports",
                new CreateExportRequest(Guid.NewGuid(), fromDate, toDate, employeeId), cancellationToken),
            cancellationToken);
        if (!create.IsSuccess)
            return ApiResult<DownloadedFile>.Failure(create.Errors);

        var fileName = $"time-entries-{fromDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-{toDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.csv";
        return await ApiResponse.ReadFileAsync(
            () => httpClient.GetAsync($"api/exports/{create.Value!.Export.Id:D}/download", cancellationToken),
            fileName,
            cancellationToken);
    }

    public Task<ApiResult<bool>> CloseTimeEntriesAsync(Guid exportId, CancellationToken cancellationToken = default) =>
        ApiResponse.WriteAsync(
            () => httpClient.PostAsync($"api/exports/{exportId:D}/close-time-entries", content: null, cancellationToken), cancellationToken);
}
