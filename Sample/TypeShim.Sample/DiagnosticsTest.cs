using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TypeShim.Sample;

//[TSExport]
//internal class PublicAccessibilityTest
//{
//}

//[TSExport]
//public class PropertyAccessibilityTest
//{
//    public int PublicProperty { get; set; }
//    private int PrivateProperty { get; set; }

//    public required int PublicRequiredProperty { get; set; }
//    public int PublicGetPrivateSetProperty { get; private set; }
//}

//[TSExport]
//public class PropertyTypeSupportTest
//{
//    public Dictionary<int, Person> P { get; set; } = [];
//}

//[TSExport]
//public class OverloadMethodTest
//{
//    public int P() => 0; 
//    public int P(int i) => 0;
//    public int P(int i, int j) => 0;
//}
//[TSExport]
//public class OverloadMethodTest2
//{
//    public int P() => 0; 
//    internal int P(int i) => 0;
//    public int P(int i, int j) => 0;
//}

//[TSExport]
//public class OverloadConstructorTest()
//{
//    public OverloadConstructorTest(int i)
//    {

//    }
//}

//[TSExport]
//public class OverloadConstructorTest2
//{
//    public OverloadConstructorTest2(int i)
//    {

//    }
//    public OverloadConstructorTest2(bool i)
//    {

//    }
//}

//[TSExport]
//public class OverloadConstructorTest3
//{
//    public OverloadConstructorTest3(int i)
//    {

//    }
//    internal OverloadConstructorTest3(bool i)
//    {

//    }
//}

//[TSExport]
//public class MethodTypeSupportTest
//{ 
//    public Task<int[]> X() { return Task.FromResult(new int[] { 1, 2, 3 }); }
//    public Task<int?> Z() { return Task.FromResult<int?>(1); }
//    public Span<string> Y() { return new Span<string>(); }
//    public Span<int> BA() { return new Span<int>(); }
//    public IEnumerable<int> BBA() { return []; }  
//    public IDictionary<string, int> C() { return new Dictionary<string, int>(); }
//    public IReadOnlyList<string> E() { return []; }
//    public Dictionary<int, Person> D() { return new Dictionary<int, Person>(); }
//    public void DIn(Dictionary<int, Person> p) { }
    
//    public ArraySegment<string> W() { return new ArraySegment<string>(new string[] { "a", "b", "c" }); }

//    public Func<int, string> F() { return (i) => i.ToString(); }
//}

//[TSExport]
//public class FieldTest
//{
//    public int PublicField;
//    private int PrivateField;
    
//    public required int InternalRequiredField;   
//}
