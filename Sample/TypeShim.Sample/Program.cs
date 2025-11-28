using System;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

Console.WriteLine("TypeShim.Sample entered Main method in browser.");

while (true)
{
    await Task.Delay(10_000); // just keep the program alive
}

