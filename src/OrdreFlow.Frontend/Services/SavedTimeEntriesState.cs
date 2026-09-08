using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

// Backend has no endpoint yet to list saved time entries, so this just tracks what was
// successfully saved during this session. Swap this out once a real "list entries" endpoint exists.
public class SavedTimeEntriesState
{
    private readonly List<SavedTimeEntry> entries = [];

    public IReadOnlyList<SavedTimeEntry> Entries => entries;

    public event Action? Changed;

    public void Add(SavedTimeEntry entry)
    {
        entries.Add(entry);
        Changed?.Invoke();
    }

    public decimal WeeklyTotalHours(DateTime referenceDate)
    {
        var (weekStart, weekEnd) = GetWeekRange(referenceDate);

        return entries
            .Where(e => e.Entry.Date.Date >= weekStart && e.Entry.Date.Date <= weekEnd)
            .Sum(e => e.Entry.Hours);
    }

    private static (DateTime Start, DateTime End) GetWeekRange(DateTime referenceDate)
    {
        var date = referenceDate.Date;
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var start = date.AddDays(-diff);
        return (start, start.AddDays(6));
    }
}
