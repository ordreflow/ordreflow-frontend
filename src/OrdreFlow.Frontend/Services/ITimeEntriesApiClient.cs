using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public interface ITimeEntriesApiClient
{
    Task<ApiResult<ViewTimeEntriesResponse>> GetEntriesAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    // Entries from the signed-in manager's employees (Manager or Admin role).
    Task<ApiResult<ViewTimeEntriesResponse>> GetManagerEntriesAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    Task<ApiResult<TimeEntryResponse>> GetAsync(Guid id, CancellationToken cancellationToken = default);

    // Decision is "accept" or "return"; a reason is required when returning.
    Task<ApiResult<bool>> ReviewAsync(
        Guid id,
        ReviewTimeEntryRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<ViewTimeEntryReviewsResponse>> GetReviewsAsync(
        Guid timeEntryId,
        CancellationToken cancellationToken = default);

    Task<ApiResult<bool>> CreateAsync(
        CreateTimeEntryRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<bool>> UpdateAsync(
        Guid id,
        UpdateTimeEntryRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
