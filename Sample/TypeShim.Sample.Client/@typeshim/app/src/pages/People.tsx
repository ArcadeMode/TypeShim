import React, { useState } from 'react';
import { PeopleList, PeopleGrid } from '@typeshim/people-ui';

export default function People() {
    const [view, setView] = useState<'list' | 'grid'>('list');
    const toggle = () => setView(v => (v === 'list' ? 'grid' : 'list'));
    return (
    <div>
        <h1>People</h1>
            <p>
                All data on this screen is accessed through interop calls to the dotnet runtime. Getting the array of people, getting a <code>Person</code>'s name,
                getting their (optional) <code>Pet</code> or getting the <code>Pet</code>'s name, these are all examples of interop calls.
            </p>
            <p>
                There are ~1500 interop calls to methods on about 400 dotnet object instances made to render this page.
                The impact of this many calls is not noticable (<i>credits to the dotnet/runtime team!</i>) so you should use your browser's devtools to profile this app.
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
                Try switching between grid and list, so you can see the isolated effects of interop. The WASM layer caches the data, so only the initial render is affected by data fetching.
            </p>

            <button
                style={{
                    background: 'rgb(115 115 115)',
                    color: '#fff',
                    border: 'none',
                    borderRadius: '4px',
                    padding: '0.6em 1.4em',
                    marginBottom: '0.5em',
                    fontSize: '1rem',
                    fontWeight: 500,
                    boxShadow: '0 2px 6px rgba(100, 116, 139, 0.15)',
                    cursor: 'pointer',
                }}
                onClick={toggle}
            >
                {view === 'list' ? 'Switch to Grid' : 'Switch to List'}
            </button>
        {view === 'list' ? (
        <PeopleList emptyText="No people yet." />
        ) : (
        <PeopleGrid emptyText="No people yet." />
        )}
    </div>
    );
}
