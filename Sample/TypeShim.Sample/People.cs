using TypeShim;

namespace TypeShim.Sample;

[TSExport]
[TSModule]
public class People(Person[] people)
{
    public Person[] All => people;
}
