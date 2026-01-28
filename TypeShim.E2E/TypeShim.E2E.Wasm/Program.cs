using System;
using System.Threading.Tasks;

namespace TypeShim.E2E.Wasm;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Wasm module runtime entered Main");
    }
}
