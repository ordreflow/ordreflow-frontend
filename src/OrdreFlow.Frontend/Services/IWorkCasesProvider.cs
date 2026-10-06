using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

public interface IWorkCasesProvider
{
    Task<IReadOnlyList<WorkCase>> GetWorkCasesAsync(CancellationToken cancellationToken = default);
}
