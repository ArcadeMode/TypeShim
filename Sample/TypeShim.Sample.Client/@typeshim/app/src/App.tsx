import { useMemo, useState } from 'react';
import Home from './pages/Home';
import People from './pages/People';
import CapabilitiesPage from './pages/Capabilities';

import { launchWasmRuntime, TypeShimInitializer } from '@typeshim/wasm-exports';

type Page = 'home' | 'people' | 'capabilities';

function App() {
  const [currentPage, setCurrentPage] = useState<Page>('home');

  const exportsPromise: Promise<void> = useMemo(async () => {
    const exports = await launchWasmRuntime();
    TypeShimInitializer.initialize({ exports });
  }, []);

  return (
    <div>
      <header style={{
        background: '#111',
        color: '#fff',
        padding: '0.75rem 1rem',
        display: 'flex',
        gap: '1rem'
      }}>
        <strong style={{ marginRight: '1rem' }}>@typeshim/app</strong>
        <nav style={{ display: 'flex', gap: '1rem' }}>
          <a
            href="#"
            onClick={(e) => { e.preventDefault(); setCurrentPage('home'); }}
            style={{ color: currentPage === 'home' ? '#61dafb' : '#ccc' }}
          >
            Home
          </a>
          <a
            href="#"
            onClick={(e) => { e.preventDefault(); setCurrentPage('people'); }}
            style={{ color: currentPage === 'people' ? '#61dafb' : '#ccc' }}
          >
            People
          </a>
          <a
            href="#"
            onClick={(e) => { e.preventDefault(); setCurrentPage('capabilities'); }}
            style={{ color: currentPage === 'capabilities' ? '#61dafb' : '#ccc' }}
          >
            Capabilities
          </a>
        </nav>
      </header>
      <main style={{ padding: '1rem', maxWidth: 800, margin: '0 auto' }}>
        {currentPage === 'home' && <Home />}
        {currentPage === 'people' && <People />}
        {currentPage === 'capabilities' && <CapabilitiesPage />}
      </main>
    </div>
  );
}

export default App;
