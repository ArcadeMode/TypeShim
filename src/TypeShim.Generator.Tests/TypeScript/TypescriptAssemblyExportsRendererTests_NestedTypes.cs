using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypescriptAssemblyExportsRendererTests_NestedTypes
{
    [Test]
    public void AssemblyExports_NestedInteropClasses_AreNestedUnderContainer()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class Outer
            {
                private Outer() {}
                public static Inner Make(Kind k) => null;

                public enum Kind { A, B }

                public class Inner
                {
                    private Inner() {}
                    public static int Ping() => 1;
                }
            }

            [TSExport]
            public class Consumer
            {
                private Consumer() {}
                public static Outer.Inner Direct() => null;
                public static Outer.Inner[] Arr() => null;
                public static Outer.Inner? Nily() => null;
                public static Task<Outer.Inner> Asy() => null;
                public static Action<Outer.Inner> Del() => null;
                public static Outer.Kind EnumRef() => Outer.Kind.A;
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exported = [.. symbolExtractor.ExtractAllExportedSymbols()];
        InteropTypeInfoCache typeCache = new();
        List<ClassInfo> classes = [.. exported.Select(s => new ClassInfoBuilder(s, typeCache).Build())];

        ModuleHierarchyInfo hierarchyInfo = ModuleHierarchyInfo.FromClasses(classes);
        RenderContext renderCtx = new(null, [.. classes], RenderOptions.TypeScript);
        new TypescriptAssemblyExportsRenderer(hierarchyInfo, renderCtx).Render();

        AssertEx.EqualOrDiff(renderCtx.ToString(), """
// TypeShim generated TypeScript module exports interface
export interface AssemblyExports{
  N1: {
    OuterInterop: {
      Make(k: number): ManagedObject;
      InnerInterop: {
        Ping(): number;
      };
    };
    ConsumerInterop: {
      Direct(): ManagedObject;
      Arr(): Array<ManagedObject>;
      Nily(): ManagedObject | null;
      Asy(): Promise<ManagedObject>;
      Del(): (arg0: ManagedObject) => void;
      EnumRef(): number;
    };
  };
}

""");
    }
}
