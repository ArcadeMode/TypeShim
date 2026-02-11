import { describe, bench } from 'vitest';
import { ArrayPropertiesClass, ExportedClass } from '@typeshim/e2e-wasm-lib';
import { printSample, measureMemoryDelta } from './bench-utils';

describe('bench: array-properties', () => {
  bench('memory: constructing ArrayPropertiesClass (single sample)', async () => {
    // Put some pressure on memory by constructing a lot of objects, and see how much memory is used.
    printSample(measureMemoryDelta(() => {
      for (let i = 0; i < 10_000; i++) {
        makeObject();
      }
    }));
    
  }, { time: 10_000});

  bench('memory: constructing ExportedClass (single sample)', async () => {
    // Put some pressure on memory by constructing a lot of objects, and see how much memory is used.
    printSample(measureMemoryDelta(() => {
      for (let i = 0; i < 10_000; i++) {
        const exported = new ExportedClass({ Id: 3 });
        (globalThis as any).fakeMethod?.(exported); // no-op that ensures not optimized away
      }
    }));
    
  }, { time: 10_000});
});

function makeObject() {
  const exported = new ExportedClass({ Id: 3 });
  // ArrayPropertiesClass is used as a more memory-costly object to amplify memory measurement signal
  const obj = new ArrayPropertiesClass({
    ByteArrayProperty: [1, 2, 3],
    IntArrayProperty: [7, 8, 9],
    StringArrayProperty: ['one', 'two', 'three'],
    DoubleArrayProperty: [1.1, 2.2, 3.3],
    JSObjectArrayProperty: [{ a: 1 }, { b: 2 }, { c: 3 }],
    ObjectArrayProperty: [exported.instance],
    ExportedClassArrayProperty: [exported],
  });
  return { obj, exported };
}