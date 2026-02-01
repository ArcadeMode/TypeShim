using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace TypeShim.E2E.Wasm;

[TSExport]
public class TaskPropertiesClass
{
    public Task TaskProperty { get; set; } = Task.CompletedTask;
    public Task<nint> TaskOfNIntProperty { get; set; } = Task.FromResult((nint)0);
    public Task<short> TaskOfShortProperty { get; set; } = Task.FromResult((short)0);
    public Task<int> TaskOfIntProperty { get; set; } = Task.FromResult(0);
    public Task<long> TaskOfLongProperty { get; set; } = Task.FromResult(0L);
    public Task<bool> TaskOfBoolProperty { get; set; } = Task.FromResult(false);
    public Task<byte> TaskOfByteProperty { get; set; } = Task.FromResult((byte)0);
    public Task<char> TaskOfCharProperty { get; set; } = Task.FromResult('\0');
    public Task<string> TaskOfStringProperty { get; set; } = Task.FromResult(string.Empty);
    public Task<double> TaskOfDoubleProperty { get; set; } = Task.FromResult(0.0);
    public Task<float> TaskOfFloatProperty { get; set; } = Task.FromResult(0.0f);
    public Task<DateTime> TaskOfDateTimeProperty { get; set; } = Task.FromResult(DateTime.MinValue);
    public Task<DateTimeOffset> TaskOfDateTimeOffsetProperty { get; set; } = Task.FromResult(DateTimeOffset.MinValue);
    public Task<object> TaskOfObjectProperty { get; set; } = Task.FromResult(new object());
    public Task<ExportedClass> TaskOfExportedClassProperty { get; set; } = Task.FromResult(new ExportedClass());
    public Task<JSObject> TaskOfJSObjectProperty { get; set; } = Task.FromException<JSObject>(new Exception("biem"));
}
