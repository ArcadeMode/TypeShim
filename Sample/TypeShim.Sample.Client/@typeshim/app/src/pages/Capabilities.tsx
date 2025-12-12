import { StringCapabilities } from '@typeshim/capabilities-ui';
import type { AssemblyExports } from '@typeshim/wasm-exports';

export default function Home({ exportsPromise }: { exportsPromise?: Promise<AssemblyExports> }) {
    return (
    <div>
        <p>This page demonstrates the types of types, methods, properties etc that
            TypeShim can construct for you.
        </p>
        <StringCapabilities exportsPromise={exportsPromise} />
    </div>
    );
}
