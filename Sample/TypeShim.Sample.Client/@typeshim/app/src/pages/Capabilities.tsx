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
        <p>This page is doubles as an E2E test of sorts.</p>
        <PrimitivesDemoComponent exportsPromise={exportsPromise} />
        <div style={{ margin: '0.5em'}}></div>
        <ArraysDemoComponent exportsPromise={exportsPromise} />
    </div>
    );
}
