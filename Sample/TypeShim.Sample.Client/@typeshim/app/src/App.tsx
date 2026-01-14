import { useMemo, useState } from 'react';
import Home from './pages/Home';
import People from './pages/People';
import CapabilitiesPage from './pages/Capabilities';

import { createWasmRuntime, MyApp, TypeShimInitializer } from '@typeshim/wasm-exports';
import { CompilationTest, ExportedClass } from "@typeshim/wasm-exports";

type Page = 'home' | 'people' | 'capabilities';

function App() {
  const [currentPage, setCurrentPage] = useState<Page>('home');

  useMemo(async () => {
    const runtimeInfo = await createWasmRuntime();
    TypeShimInitializer.initialize(runtimeInfo);
    MyApp.Initialize(document.baseURI);

    E2E();
  }, []);

  return (
    <div>
      <header style={{
        background: '#111',
        color: '#fff',
        padding: '0.75rem 1rem',
        display: 'flex',
        gap: '1rem'
      }}>
        <strong style={{ marginRight: '1rem' }}>@typeshim/app</strong>
        <nav style={{ display: 'flex', gap: '1rem' }}>
          <a
            href="#"
            onClick={(e) => { e.preventDefault(); setCurrentPage('home'); }}
            style={{ color: currentPage === 'home' ? '#61dafb' : '#ccc' }}
          >
            Home
          </a>
          <a
            href="#"
            onClick={(e) => { e.preventDefault(); setCurrentPage('people'); }}
            style={{ color: currentPage === 'people' ? '#61dafb' : '#ccc' }}
          >
            People
          </a>
          <a
            href="#"
            onClick={(e) => { e.preventDefault(); setCurrentPage('capabilities'); }}
            style={{ color: currentPage === 'capabilities' ? '#61dafb' : '#ccc' }}
          >
            Capabilities
          </a>
        </nav>
      </header>
      <main style={{ padding: '1rem', maxWidth: 800, margin: '0 auto' }}>
        {currentPage === 'home' && <Home />}
        {currentPage === 'people' && <People />}
        {currentPage === 'capabilities' && <CapabilitiesPage />}
      </main>
    </div>
  );
}

export default App;

function E2E() {
    const exportedClass = new ExportedClass({ Id: 1 });
    const t = new CompilationTest({
        NIntProperty: 1,
        ByteProperty: 2,
        ShortProperty: 3,
        IntProperty: 4,
        LongProperty: 5,
        BoolProperty: true,
        StringProperty: "Test",
        CharProperty: 'A',
        DoubleProperty: 6.7,
        FloatProperty: 8.9,
        DateTimeProperty: new Date(),
        DateTimeOffsetProperty: new Date(),
        ExportedClassProperty: exportedClass,
        ObjectProperty: exportedClass.instance,
        JSObjectProperty: { foo: "bar" },
        TaskProperty: Promise.resolve(),
        TaskOfByteProperty: Promise.resolve(22),
        TaskOfNIntProperty: Promise.resolve(42),
        TaskOfShortProperty: Promise.resolve(43),
        TaskOfIntProperty: Promise.resolve(44),
        TaskOfLongProperty: Promise.resolve(45),
        TaskOfBoolProperty: Promise.resolve(true),
        TaskOfCharProperty: Promise.resolve(1 as unknown as string),
        TaskOfStringProperty: Promise.resolve("Task String"),
        TaskOfDoubleProperty: Promise.resolve(67.8),
        TaskOfFloatProperty: Promise.resolve(89.0),
        TaskOfDateTimeProperty: Promise.resolve(new Date()),
        TaskOfDateTimeOffsetProperty: Promise.resolve(new Date()),
        TaskOfObjectProperty: Promise.resolve(exportedClass.instance),
        TaskOfExportedClassProperty: Promise.resolve(exportedClass),
        TaskOfJSObjectProperty: Promise.resolve({ baz: "qux" }),
        ByteArrayProperty: [1, 2, 3],
        IntArrayProperty: [7, 8, 9],
        StringArrayProperty: ["one", "two", "three"],
        DoubleArrayProperty: [1.1, 2.2, 3.3],
        JSObjectArrayProperty: [{ a: 1 }, { b: 2 }, { c: 3 }],
        ObjectArrayProperty: [exportedClass.instance],
        ExportedClassArrayProperty: [exportedClass],
    });

    console.log("E2E CompilationTest instance:", t, CompilationTest.materialize(t));
}