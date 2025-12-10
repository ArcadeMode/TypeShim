using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypeShim.Sample;

[TSExport]
public class People(Person[] people)
{
    public Person[] All => people;
}
