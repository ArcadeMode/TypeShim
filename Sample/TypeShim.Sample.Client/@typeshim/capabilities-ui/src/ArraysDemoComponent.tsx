"use client"

import React, { useEffect, useState } from 'react';
import { AssemblyExports, CapabilitiesModule, CapabilitiesProvider, ArraysDemo } from '@typeshim/wasm-exports/typeshim';

export interface ArraysDemoProps {
  exportsPromise?: Promise<AssemblyExports>;
}

type ArraysDemoState = {
  instance: ArraysDemo;
  appendValue: number;
  setValue: number;
};

export const ArraysDemoComponent: React.FC<ArraysDemoProps> = ({ exportsPromise }) => {
  const [cap, setCap] = useState<CapabilitiesProvider | null>(null);
  const [demos, setDemos] = useState<ArraysDemoState[]>([]);

  useEffect(() => {
    (async () => {
      let exports: AssemblyExports;
      if (exportsPromise) {
        exports = await exportsPromise;
      } else {
        const starter: any = (window as any).wasmModuleStarter;
        if (!starter) throw new Error('wasmModuleStarter not found. Ensure dotnet-start.js is loaded.');
        exports = await (starter.exports as Promise<AssemblyExports>);
      }
      const module = new CapabilitiesModule(exports);
      const provider = module.GetCapabilitiesProvider();
      setCap(provider);
    })().catch(console.error);
  }, [exportsPromise]);

  const createDemo = () => {
    if (!cap) return;
    const instance: ArraysDemo = cap.GetArraysDemo();
    setDemos(prev => [{ instance, appendValue: 0, setValue: 0 }, ...prev]);
  };

  return (
    <div style={{ border: '1px solid #ddd', borderRadius: 8, padding: '1rem' }}>
      <h2 style={{ margin: 0, paddingLeft: '0.5em'}}>Arrays</h2>
      <div style={{ fontFamily: 'monospace', background: '#f7f7f7', padding: '0.75rem', borderRadius: 6, marginBottom: '1rem' }}>
        <div>
          <span style={{ color: '#555' }}>const module</span>
          <span> = new CapabilitiesModule(exports)</span>
          <span style={{ marginLeft: 8, color: 'rebeccapurple' }}>→ CapabilitiesModule</span>
          <br/>
          <span style={{ color: '#555' }}>const capabilities</span>
          <span> = module.GetCapabilitiesProvider()</span>
          <span style={{ marginLeft: 8, color: 'rebeccapurple' }}>→ CapabilitiesProvider</span>
        </div>
      </div>

      <div style={{ marginBottom: '1rem', padding: '0.75rem' }}>
        capabilities.GetArraysDemo()
        <button onClick={createDemo} disabled={!cap} style={{ padding: '0.25rem 0.5rem', margin: '0 0.5rem', borderRadius: 4, borderWidth: 1 }}>Invoke</button>
      </div>

      <div style={{ display: 'grid', gap: '1rem', gridTemplateColumns: '1fr' }}>
        {demos.map((demo, idx) => (
          <div key={idx} style={{ border: '1px solid #eee', background: '#f0f0f0', borderRadius: 8, padding: '0.75rem' }}>
            <div style={{ fontFamily: 'monospace', background: '#f7f7f7', padding: '0.5rem', borderRadius: 6, marginBottom: '0.5rem' }}>
              <span style={{ color: '#555' }}>const instance</span>
              <span> = capabilities.GetArraysDemo([...])</span>
              <span style={{ marginLeft: 8, color: 'rebeccapurple' }}>→ ArraysDemo</span>
            </div>

            <div style={{ fontFamily: 'monospace', background: '#f7f7f7', padding: '0.5rem', borderRadius: 6, marginBottom: '0.5rem' }}>
              <div>
                <span>instance.IntArrayProperty</span>
                <span style={{ marginLeft: 8, color: 'rebeccapurple' }}>→ [{demo.instance.IntArrayProperty.join(', ')}]</span>
              </div>
            </div>

            <div style={{ fontFamily: 'monospace', background: '#f7f7f7', padding: '0.5rem', borderRadius: 6, marginBottom: '0.5rem' }}>
              <div>
                <span>instance.SumIntArray()</span>
                <span style={{ marginLeft: 8, color: 'rebeccapurple' }}>→ {demo.instance.SumIntArray()}</span>
              </div>
            </div>

            <div style={{ fontFamily: 'monospace', background: '#f7f7f7', padding: '0.5rem', borderRadius: 6, marginBottom: '0.5rem' }}>
              <span>instance.AppendToIntArray(value: 
                <input type="number" value={demo.appendValue}
                  onChange={e => {
                    const val = Number(e.target.value);
                    setDemos(prev => prev.map((d, i) => i === idx ? { ...d, appendValue: Number.isFinite(val) ? val : 0 } : d))
                  }}
                  style={{ width: '60px' }} />)</span>
              <button onClick={() => { demo.instance.AppendToIntArray(demo.appendValue); setDemos(prev => prev.map((d, i) => i === idx ? { ...d } : d)) }} 
                      style={{ padding: '0.25rem 0.5rem', margin: '0 0.5rem', borderRadius: 4, borderWidth: 1 }}>Invoke</button>
              <span style={{ marginLeft: 8, color: 'rebeccapurple' }}>→ Void</span>
            </div>

            <div style={{ fontFamily: 'monospace', background: '#f7f7f7', padding: '0.5rem', borderRadius: 6, marginBottom: '0.5rem' }}>
              <span>instance.IntArrayProperty[0] =  
                <input type="number" value={demo.setValue}
                  onChange={e => {
                    const val = Number(e.target.value);
                    setDemos(prev => prev.map((d, i) => i === idx ? { ...d, setValue: Number.isFinite(val) ? val : 0 } : d))
                  }}
                  style={{ width: '60px' }} />
                </span>
              <button onClick={() => { demo.instance.IntArrayProperty[0] = demo.setValue; setDemos(prev => prev.map((d, i) => i === idx ? { ...d } : d)) }} 
                      style={{ padding: '0.25rem 0.5rem', margin: '0 0.5rem', borderRadius: 4, borderWidth: 1 }}>Invoke</button>
              <span style={{ marginLeft: 8, color: 'rebeccapurple' }}>→ Void</span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};