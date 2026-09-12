using TypeShim;

namespace TypeShim.E2E.External;

/// <summary>
/// A simple exported class that lives in a different namespace than the rest of the E2E suite,
/// so that it can be referenced across namespaces to exercise the qualification logic.
/// </summary>
[TSExport]
public class ExternalClass
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    public string Describe() => $"{Id}:{Name}";
}
