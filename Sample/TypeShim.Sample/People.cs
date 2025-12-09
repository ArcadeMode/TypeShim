using TypeShim;

namespace TypeShim.Sample;

[TSExport]
public class People(Person[] people)
{
    public Person[] All => people;
}
