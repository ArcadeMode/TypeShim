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
    console.log("Starting E2E CompilationTest...");
    const exportedClass = new ExportedClass({ Id: 1 });
    const testObject = new CompilationTest({
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
        TaskOfCharProperty: Promise.resolve('B'),
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

    console.log("E2E CompilationTest instance:", testObject, CompilationTest.materialize(testObject));

    testObject.CharProperty = 'Z';
    if (testObject.CharProperty !== 'Z') {
        console.error(`E2E CharProperty value mismatch. Expected 'Z', got '${testObject.CharProperty}'`);
    }
    testObject.TaskOfCharProperty = Promise.resolve('Y');
    testObject.TaskOfCharProperty.then(value => {
        if (value === 'Y') return;
        console.error(`E2E TaskOfCharProperty value mismatch. Expected 'Y', got '${value}'`);
    }).catch(err => {
        console.error("E2E TaskOfCharProperty error:", err);
    });

    testObject.StringProperty = "Updated Test";
    if (testObject.StringProperty !== "Updated Test") {
        console.error(`E2E StringProperty value mismatch. Expected 'Updated Test', got '${testObject.StringProperty}'`);
    }
    testObject.TaskOfStringProperty = Promise.resolve("Updated Task String");
    testObject.TaskOfStringProperty.then(value => {
        if (value === "Updated Task String") return;
        console.error(`E2E TaskOfStringProperty value mismatch. Expected 'Updated Task String', got '${value}'`);
    }).catch(err => {
        console.error("E2E TaskOfStringProperty error:", err);
    });

    testObject.ExportedClassProperty.Id = 99;
    if (testObject.ExportedClassProperty.Id !== 99) {
        console.error(`E2E ExportedClassProperty.Id value mismatch. Expected 99, got '${testObject.ExportedClassProperty.Id}'`);
    }
    const newExport = new ExportedClass({ Id: 100 });
    testObject.ExportedClassProperty = newExport;
    if (testObject.ExportedClassProperty.Id !== 100) {
        console.error(`E2E ExportedClassProperty reassignment value mismatch. Expected 100, got '${testObject.ExportedClassProperty.Id}'`);
    }

    testObject.TaskOfLongProperty.then(value => {
        console.log("E2E TaskOfLongProperty value:", value);
        if (value === 45) return;
        console.error(`E2E TaskOfLongProperty value mismatch. Expected 45, got '${value}'`);
    }).catch(err => {
        console.error("E2E TaskOfLongProperty error:", err);
    });
    console.log("E2E CompilationTest completed.");
}