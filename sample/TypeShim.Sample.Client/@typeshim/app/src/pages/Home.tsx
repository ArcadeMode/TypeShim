
export default function Home() {
    return (
    <div>
        <h1>Welcome to @typeshim/app</h1>
        <p>This is a sample React app that demonstrates how TypeShim extends .NET &lt;&gt; Javascript interop with:</p>
        <ul>
            <li>Natural object semantics like:</li>
            <ul>
                <li>Accessing .NET class properties</li>
                <li>Accessing .NET class member methods</li>
            </ul>
            <li>Cross-language Type information through generated TypeScript and C#</li>
            <li>Minimizing fault-sensitive interop code</li>
        </ul>
            
        <p>Visit the 'People' page to see demonstrations of:</p>
        <ul>
            <li>Pulling typed data from an ASP.NET backend through WASM shared code</li>
            <li>Accessing object properties through interop calls</li>
            <li>Seeing performance of making ~1500 interop calls during page rendering</li>
        </ul>
        <p></p>
    </div>
    );
}
