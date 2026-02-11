using System.Threading.Tasks;

namespace TypeShim.E2E.Wasm;

[TSExport]
public class TaskReturnMethodsClass
{
    public Task VoidTaskMethod() => Task.CompletedTask;
    public Task<int> Int32TaskMethod() => Task.FromResult(42);
    public Task<bool> BoolTaskMethod() => Task.FromResult(true);
    public Task<string> StringTaskMethod() => Task.FromResult("Hello, from .NET Task");

    public Task<ExportedClass> ExportedClassTaskMethod() => Task.FromResult(new ExportedClass() { Id = 420 });
}
