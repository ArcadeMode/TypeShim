import React from 'react';
import Link from 'next/link';
import './globals.css';

export const metadata = {
  title: '@typeshim/app Demo',
  description: 'Demo app using @typeshim/people-ui components'
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body style={{ fontFamily: 'system-ui, sans-serif', margin: 0 }}>
        <header style={{
          background: '#111',
          color: '#fff',
          padding: '0.75rem 1rem',
          display: 'flex',
          gap: '1rem'
        }}>
          <strong style={{ marginRight: '1rem' }}>@typeshim/app</strong>
          <nav style={{ display: 'flex', gap: '1rem' }}>
            <Link href="/">Home</Link>
            <Link href="/people">People</Link>
          </nav>
        </header>
        <main style={{ padding: '1rem', maxWidth: 800, margin: '0 auto' }}>
          {children}
        </main>
      </body>
    </html>
  );
}
