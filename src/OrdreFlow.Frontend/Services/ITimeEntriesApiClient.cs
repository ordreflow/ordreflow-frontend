using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public interface ITimeEntriesApiClient
{
    Task<ApiResult<TimeEntry>> CreateAsync(
        CreateTimeEntryRequest request,
        CancellationToken cancellationToken = default);
}
