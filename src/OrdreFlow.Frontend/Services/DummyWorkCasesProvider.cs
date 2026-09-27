using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

// Backend has no work cases/work-items endpoint yet, so this stands in until it does.
// Order ids match the WorkItemIds used before, so creating time entries still works against the API.
// Swap this registration for a real API-backed IWorkCasesProvider once that endpoint exists.
public class DummyWorkCasesProvider : IWorkCasesProvider
{
    private static readonly IReadOnlyList<WorkCase> WorkCases =
    [
        new WorkCase(1, "Netto Vestergade",
        [
            new Order(1, "Install shelving unit")
        ]),
        new WorkCase(2, "VS Automatic",
        [
            new Order(2, "Repair conveyor belt (Plant 2)"),
            new Order(4, "Replace hydraulic pump (Plant 1)")
        ]),
        new WorkCase(3, "Bilka Logistikcenter",
        [
            new Order(3, "Quarterly maintenance check")
        ])
    ];

    public Task<IReadOnlyList<WorkCase>> GetWorkCasesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(WorkCases);
}
