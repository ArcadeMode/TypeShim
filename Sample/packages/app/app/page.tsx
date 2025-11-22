import React from 'react';
import Link from 'next/link';

export default function HomePage() {
  return (
    <div>
      <h1>Welcome to @typeshim/app</h1>
      <p>This is the demo Next.js application in the monorepo.</p>
      <p>Navigate to the <Link href="/people">People</Link> page to view shared UI components.</p>
    </div>
  );
}
