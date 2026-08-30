using System.Threading.Tasks;

namespace TypeShim.E2E.Wasm;

[TSExport]
public enum Priority
{
    Low,
    Medium,
    High
}

/// <summary>
/// A uint-backed enum: exercises the widening path where the enum crosses the boundary as a long.
/// </summary>
[TSExport]
public enum Season : uint
{
    Spring = 1,
    Summer = 2,
    Autumn = 3,
    Winter = 4
}

[TSExport]
public class EnumMethodsClass
{
    public Priority PriorityProperty { get; set; }
    public Priority? NullablePriorityProperty { get; set; }
    public Priority[] PriorityArrayProperty { get; set; } = [];
    public Season SeasonProperty { get; set; }

    public Priority EchoPriority(Priority priority) => priority;
    public Priority? EchoNullablePriority(Priority? priority) => priority;
    public Priority[] EchoPriorityArray(Priority[] priorities) => priorities;
    public Season EchoSeason(Season season) => season;

    public Task<Priority> HighestPriorityTask() => Task.FromResult(Priority.High);

    public Priority NextPriority(Priority priority) => priority == Priority.High ? Priority.High : priority + 1;

    public static Priority HighestPriority() => Priority.High;
}
