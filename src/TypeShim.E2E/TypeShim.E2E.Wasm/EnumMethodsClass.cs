using System.Threading.Tasks;

namespace TypeShim.E2E.Wasm;

[TSExport]
public enum Priority
{
    Low,
    Medium,
    High
}

[TSExport]
public enum SpecialByte : byte
{
    Null = 0,
    All = 255
}

[TSExport]
public enum Season : short
{
    Spring = 1,
    Summer = 2,
    Autumn = 3,
    Winter = 4
}

/// <summary>
/// A long-backed enum with a member at the JS safe-integer boundary (2^53 - 1).
/// </summary>
[TSExport]
public enum Magnitude : long
{
    Zero = 0,
    Max = 9007199254740991
}

[TSExport]
public class EnumMethodsClass
{
    public Priority PriorityProperty { get; set; }
    public SpecialByte SpecialByteProperty { get; set; }
    public Priority? NullablePriorityProperty { get; set; }
    public Priority[] PriorityArrayProperty { get; set; } = [];
    public Season SeasonProperty { get; set; }

    public Priority EchoPriority(Priority priority) => priority;
    public SpecialByte EchoSpecialByte(SpecialByte specialByte) => specialByte;
    public Priority? EchoNullablePriority(Priority? priority) => priority;
    public Priority[] EchoPriorityArray(Priority[] priorities) => priorities;
    public Season EchoSeason(Season season) => season;
    public Magnitude EchoMagnitude(Magnitude magnitude) => magnitude;

    public Task<Priority> HighestPriorityTask() => Task.FromResult(Priority.High);

    public Priority NextPriority(Priority priority) => priority == Priority.High ? Priority.High : priority + 1;

    public static Priority HighestPriority() => Priority.High;
}
