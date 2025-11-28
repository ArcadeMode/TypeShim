import { useState } from 'react';
import Home from './pages/Home';
import People from './pages/People';

type Page = 'home' | 'people';

function App() {
  const [currentPage, setCurrentPage] = useState<Page>('home');

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
        </nav>
      </header>
      <main style={{ padding: '1rem', maxWidth: 800, margin: '0 auto' }}>
        {currentPage === 'home' && <Home />}
        {currentPage === 'people' && <People />}
      </main>
    </div>
  );
}

export default App;
