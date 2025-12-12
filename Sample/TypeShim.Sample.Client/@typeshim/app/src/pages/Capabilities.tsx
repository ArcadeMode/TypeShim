import { PrimitivesDemoComponent } from '@typeshim/capabilities-ui';
import type { AssemblyExports } from '@typeshim/wasm-exports';

export default function Capabilities({ exportsPromise }: { exportsPromise?: Promise<AssemblyExports> }) {
    return (
    <div>
        <p>
            This page demonstrates EndToEnd capabilities of TypeShim for different Types, Methods, and Properties.
            While interesting to play with, this page is mostly intended as an E2E test of sorts.
        </p>
        <PrimitivesDemoComponent exportsPromise={exportsPromise} />
    </div>
    );
}
