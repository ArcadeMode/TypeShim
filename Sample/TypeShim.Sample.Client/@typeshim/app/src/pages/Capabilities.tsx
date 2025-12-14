import { PrimitivesDemoComponent, ArraysDemoComponent } from '@typeshim/capabilities-ui';
import type { AssemblyExports } from '@typeshim/wasm-exports';

export default function Capabilities({ exportsPromise }: { exportsPromise?: Promise<AssemblyExports> }) {
    return (
    <div>
        <p>
            This page demonstrates EndToEnd capabilities of TypeShim for different Types, Methods, and Properties.
            All state lives in the dotnet runtime, and is accessed through interop calls.
            Try clicking the "Invoke" buttons below to create new demo class instances.
        </p>
        <p
            style={{
                padding: '1em 1.5em',
                margin: '1.5em 0',
                borderLeft: '5px solid rgba(0, 0, 0, 0.2)',
                borderRadius: '1px',
                fontStyle: 'italic',
                color: '#333',
            }}
        >
            This page is doubles as an E2E test of sorts and is very much a work-in-progress. Not all code-patterns are
            implemented yet on this page.
        </p>
        <PrimitivesDemoComponent exportsPromise={exportsPromise} />
        <div style={{ margin: '0.5em'}}></div>
        <ArraysDemoComponent exportsPromise={exportsPromise} />
    </div>
    );
}
