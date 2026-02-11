using System;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace TypeShim.Sample.Capabilities;

[TSExport]
public class PrimitivesDemo
{
    public required string InitialStringProperty { get; init; }
    public required string StringProperty { get; set; }

    public int GetStringLength()
    {
        return StringProperty.Length;
    }

    public string ToUpperCase()
    {
        return StringProperty.ToUpper();
    }

    public string Concat(string str1, string str2)
    {
        return string.Concat(StringProperty, str1, str2);
    }

    public bool ContainsUpperCase()
    {
        return StringProperty.Equals(StringProperty.ToLowerInvariant(), StringComparison.CurrentCultureIgnoreCase);
    }
    
    public void ResetBaseString()
    {
        StringProperty = InitialStringProperty;
    }

    public void MultiplyString(int times)
    {
        if (times < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(times), "times must be non-negative");
        }
        if (times * InitialStringProperty.Length > 100_000)
        {
            throw new InvalidOperationException("Resulting string is too long");
        }
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < times; i++)
        {
            sb.Append(StringProperty);
        }
        StringProperty = sb.ToString();
    }
}
