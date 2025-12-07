using TypeShim;

namespace TypeShim.Sample;

[TsExport]
public class People(Person[] people)
{
    public Person[] All => people;
}
