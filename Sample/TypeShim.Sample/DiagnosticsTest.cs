using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TypeShim.Sample;

//[TSExport]
////[TSModule]
//internal class DiagnosticsTest
//{
//    public Task<int[]> X() { return Task.FromResult(new int[] { 1, 2, 3 }); }
//    public Task<int?> Z() { return Task.FromResult<int?>(1); }

//    public Span<string> Y() { return new Span<string>(); }
//    public Span<int> BA() { return new Span<int>(); }
//    public IEnumerable<int> BBA() { return []; }
//    public IDictionary<string, int> C() { return new Dictionary<string, int>(); }
//    public IReadOnlyList<string> E() { return []; }
//    public Dictionary<int, Person> D() { return new Dictionary<int, Person>(); }

//    public ArraySegment<string> W() { return new ArraySegment<string>(new string[] { "a", "b", "c" }); }

//    public Func<int, string> F() { return (i) => i.ToString(); }
//}
