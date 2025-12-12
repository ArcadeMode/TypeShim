"use client"

import React, { useEffect, useState } from 'react';
import { AssemblyExports, Capabilities, PrimitivesCapability, CapabilitiesModule } from '@typeshim/wasm-exports/typeshim';

export interface StringCapabilitiesProps {
  exportsPromise?: Promise<AssemblyExports>;
}

export const StringCapabilities: React.FC<StringCapabilitiesProps> = ({ exportsPromise }) => {
  const [cap, setCap] = useState<Capabilities | null>(null);
  const [baseInput, setBaseInput] = useState('Hello');
  const [stringCap, setStringCap] = useState<PrimitivesCapability | null>(null);
  const [upperInput, setUpperInput] = useState('world');
  const [concatA, setConcatA] = useState('foo');
  const [concatB, setConcatB] = useState('bar');

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
      const capabilities = module.Capabilities;
      setCap(capabilities);
      const sc = capabilities.GetStringCapability(baseInput);
      setStringCap(sc);
    })().catch(console.error);
  }, [exportsPromise]);

  useEffect(() => {
    if (!cap) return;
    const sc = cap.GetStringCapability(baseInput);
    setStringCap(sc);
  }, [baseInput, cap]);

  return (
    <div style={{ border: '1px solid #ddd', borderRadius: 8, padding: '1rem' }}>
      <h2>String Capabilities</h2>
      <div style={{ fontFamily: 'monospace', background: '#f7f7f7', padding: '0.75rem', borderRadius: 6, marginBottom: '1rem' }}>
        <div>
          <span style={{ color: '#555' }}>const module</span>
          <span> = new CapabilitiesModule(exports)</span>
          <span style={{ marginLeft: 8 }}>→ CapabilitiesModule</span>
          <br/>
          <span style={{ color: '#555' }}>const capabilities</span>
          <span> = module.Capabilities</span>
          <span style={{ marginLeft: 8 }}>→ Capabilities</span>
        </div>
      </div>
      <div style={{ display: 'grid', gap: '1rem', gridTemplateColumns: '1fr' }}>
        <div style={{ fontFamily: 'monospace', background: '#f7f7f7', padding: '0.75rem', borderRadius: 6 }}>
          <div style={{ marginBottom: 6 }}>
            <label style={{ marginRight: 8 }}>baseInput:</label>
            <input value={baseInput} onChange={e => setBaseInput(e.target.value)} style={{ width: '60%' }} />
          </div>
          <div>
            <span style={{ color: '#555' }}>const instance</span>
            <span> = capabilities.ConstructPrimitivesCapability(baseInput)</span>
            <span style={{ marginLeft: 8 }}>
              → PrimitivesCapability
            </span>
          </div>
          <div>
            <span style={{ color: '#555' }}>length</span>
            <span> = instance.GetStringLength()</span>
            <span style={{ marginLeft: 8 }}>
              → {stringCap ? stringCap.GetStringLength() : '-'}
            </span>
          </div>
          <div>
            <span style={{ color: '#555' }}>upper</span>
            <span> = instance.ToUpperCase(upperInput)</span>
            <span style={{ marginLeft: 8 }}>
              → {stringCap ? stringCap.ToUpperCase() : '-'}
            </span>
          </div>
          <div>
            <span style={{ color: '#555' }}>baseString</span>
            <span> = instance.BaseString</span>
            <span style={{ marginLeft: 8 }}>
              → {stringCap ? stringCap.BaseString : '-'}
            </span>
          </div>
        </div>

        <div style={{ fontFamily: 'monospace', background: '#f7f7f7', padding: '0.75rem', borderRadius: 6 }}>
          <div style={{ marginBottom: 6 }}>
            <label style={{ marginRight: 8 }}>concatA:</label>
            <input value={concatA} onChange={e => setConcatA(e.target.value)} style={{ width: '30%' }} />
            <br/>
            <label style={{ marginRight: 8 }}>concatB:</label>
            <input value={concatB} onChange={e => setConcatB(e.target.value)} style={{ width: '30%' }} />
          </div>
          <div>
            <span style={{ color: '#555' }}>concat</span>
            <span> = instance.ConcatStrings(concatA, concatB)</span>
            <span style={{ marginLeft: 8 }}>
              → {stringCap ? stringCap.Concat(concatA, concatB) : '-'}
            </span>
          </div>
        </div>

      </div>
    </div>
  );
};
