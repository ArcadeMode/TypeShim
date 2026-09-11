using TypeShim;

namespace Top.A;

[TSExport]
public class SpikeA
{
    public int Id { get; set; }
    public Top.B.SpikeB Partner { get; set; } = new();

    public string Describe() => $"{Id}:{Partner.Name}";
}
