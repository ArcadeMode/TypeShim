"use client"

import React, { useEffect, useState } from 'react';
import { AssemblyExports, CapabilitiesProvider, PrimitivesDemo, CapabilitiesModule } from '@typeshim/wasm-exports/typeshim';

export interface PrimitivesDemoProps {
  exportsPromise?: Promise<AssemblyExports>;
}

export const PrimitivesDemoComponent: React.FC<PrimitivesDemoProps> = ({ exportsPromise }) => {
  const [cap, setCap] = useState<CapabilitiesProvider.Proxy | null>(null);
  const [newBaseInput, setNewBaseInput] = useState('Hello');
  const [demos, setDemos] = useState<Array<{ instance: PrimitivesDemo.Proxy; concatA: string; concatB: string; multiplyCount: number; multiplyResult?: string }>>([]);

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
      const capabilities = CapabilitiesModule.Proxy.GetCapabilitiesProvider();
      setCap(capabilities);
    })().catch(console.error);
  }, [exportsPromise]);

  const createDemo = () => {
    if (!cap) return;
    const instance = cap.GetPrimitivesDemo(newBaseInput);
    setDemos(prev => [{ instance, concatA: 'foo', concatB: 'bar', multiplyCount: 2, multiplyResult: 'Void' }, ...prev]);
  };

  const handleMultiplyInvoke = (idx: number) => {
    setDemos(prev => {
      const current = prev[idx];
      let result = 'Void';
      try {
        current.instance.MultiplyString(current.multiplyCount)
        
      } catch (err: any) {
        const message = err?.message ? String(err.message) : String(err);
        debugger;
        result = `Error: ${message}`;        
      }
      const next = [...prev];
      next[idx] = { ...current, multiplyResult: result };
      return next;
    });
  };

  return (
    <div style={{ border: '1px solid #ddd', borderRadius: 8, padding: '1rem' }}>
      <h2 style={{ margin: 0, paddingLeft: '0.5em'}}>Primitives</h2>
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
        capabilities.GetPrimitivesDemo(<input value={newBaseInput} onChange={e => setNewBaseInput(e.target.value)} style={{ width: '60px' }} />)
        <button onClick={createDemo} disabled={!cap} style={{ padding: '0.25rem 0.5rem', margin: '0 0.5rem', borderRadius: 4, borderWidth: 1 }}>Invoke</button>
      </div>      

      <div style={{ display: 'grid', gap: '1rem', gridTemplateColumns: '1fr' }}>
        {demos.map((demo, idx) => (
          <div key={idx} style={{ border: '1px solid #eee', background: '#f0f0f0', borderRadius: 8, padding: '0.75rem' }}>
            <div style={{ fontFamily: 'monospace', background: '#f7f7f7', padding: '0.5rem', borderRadius: 6, marginBottom: '0.5rem' }}>
              <span style={{ color: '#555' }}>const instance</span>
              <span> = capabilities.GetPrimitivesDemo("{demo.instance.InitialStringProperty}")</span>
              <span style={{ marginLeft: 8, color: 'rebeccapurple' }}>→ PrimitivesDemo</span>
            </div>

            <div style={{ fontFamily: 'monospace', background: '#f7f7f7', padding: '0.5rem', borderRadius: 6, marginBottom: '0.5rem' }}>
              <div style={{ marginTop: 6 }}>
                <span>instance.StringProperty</span>
                <span style={{ marginLeft: 8, color: 'rebeccapurple' }}>→ "{demo.instance.StringProperty}"</span>
              </div>
            </div>
            <div style={{ fontFamily: 'monospace', background: '#f7f7f7', padding: '0.5rem', borderRadius: 6, marginBottom: '0.5rem' }}>
              <span>instance.StringProperty = <input value={demo.instance.StringProperty} 
                                                     onChange={e => { demo.instance.StringProperty = e.target.value; setDemos(prev => prev.map((d, i) => i === idx ? { ...d } : d))}}
                                                     style={{ width: '60px' }}/></span>
              <span style={{ marginLeft: 8, color: 'rebeccapurple' }}>→ Void</span>
            </div>
            <div style={{ fontFamily: 'monospace', background: '#f7f7f7', padding: '0.5rem', borderRadius: 6, marginBottom: '0.5rem' }}>
              <div>
                <span>instance.GetStringLength()</span>
                <span style={{ marginLeft: 8, color: 'rebeccapurple' }}>→ {demo.instance.GetStringLength()}</span>
              </div>
            </div>
            <div style={{ fontFamily: 'monospace', background: '#f7f7f7', padding: '0.5rem', borderRadius: 6, marginBottom: '0.5rem' }}>
              <div>
                <span>instance.ContainsUpperCase()</span>
                <span style={{ marginLeft: 8, color: 'rebeccapurple' }}>→ {demo.instance.ContainsUpperCase() ? 'true' : 'false'}</span>
              </div>
            </div>
            <div style={{ fontFamily: 'monospace', background: '#f7f7f7', padding: '0.5rem', borderRadius: 6, marginBottom: '0.5rem' }}>
              <div>
                <span>instance.ToUpperCase()</span>
                <span style={{ marginLeft: 8, color: 'rebeccapurple' }}>→ {demo.instance.ToUpperCase()}</span>
              </div>
            </div>
            <div style={{ fontFamily: 'monospace', background: '#f7f7f7', padding: '0.5rem', borderRadius: 6, marginBottom: '0.5rem' }}>
              <div>
                <span>instance.ResetBaseString()</span>
                <button onClick={() => { demo.instance.ResetBaseString(); setDemos(prev => prev.map((d, i) => i === idx ? { ...d } : d)) }} 
                        style={{ padding: '0.25rem 0.5rem', borderRadius: 4, borderWidth: 1 }}>Invoke</button>
                <span style={{ marginLeft: 8, color: 'rebeccapurple' }}>→ Void</span>
              </div>
            </div>
            <div style={{ fontFamily: 'monospace', background: '#f7f7f7', padding: '0.5rem', borderRadius: 6, marginBottom: '0.5rem' }}>
              <div>
                <span>instance.ConcatStrings(str1: <input value={demo.concatA} onChange={e => setDemos(prev => prev.map((d, i) => i === idx ? { ...d, concatA: e.target.value } : d))} style={{ width: '60px' }} />, str2: <input value={demo.concatB} onChange={e => setDemos(prev => prev.map((d, i) => i === idx ? { ...d, concatB: e.target.value } : d))} style={{ width: '60px' }} />)</span>
                <span style={{ marginLeft: 8, color: 'rebeccapurple' }}>→ "{demo.instance.Concat(demo.concatA, demo.concatB)}"</span>
              </div>
            </div>

            <div style={{ fontFamily: 'monospace', background: '#f7f7f7', padding: '0.5rem', borderRadius: 6, marginBottom: '0.5rem' }}>
              <span>instance.MultiplyString(count: 
                <input type="number" value={demo.multiplyCount}
                  onChange={e => {
                    const val = Number(e.target.value);
                    setDemos(prev => prev.map((d, i) => i === idx ? { ...d, multiplyCount: Number.isFinite(val) ? val : 0 } : d))
                  }}
                  style={{ width: '40px' }} />)</span>
              <button onClick={() => handleMultiplyInvoke(idx)}
                      style={{ padding: '0.25rem 0.5rem', margin: '0 0.5rem', borderRadius: 4, borderWidth: 1 }}>Invoke</button>
              <span style={{ marginLeft: 8, color: (demo.multiplyResult?.startsWith('Error:') ? 'red' : 'rebeccapurple') }}>→ {demo.multiplyResult}</span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};
