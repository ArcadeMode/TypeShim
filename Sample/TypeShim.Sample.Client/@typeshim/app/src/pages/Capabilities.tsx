import { StringCapabilities } from '@typeshim/capabilities-ui';
import type { AssemblyExports } from '@typeshim/wasm-exports';

export default function Home({ exportsPromise }: { exportsPromise?: Promise<AssemblyExports> }) {
    return (
    <div>
        <h1>TypeShim Capabilities</h1>
        <StringCapabilities exportsPromise={exportsPromise} />
    </div>
    );
}
