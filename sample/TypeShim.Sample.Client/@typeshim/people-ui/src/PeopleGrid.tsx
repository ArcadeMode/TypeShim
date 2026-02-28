"use client"

import React, { useEffect, useState } from 'react';
import { PersonCard } from './PersonCard';
import type { Person } from '@typeshim/wasm-exports';
import { PeopleRepository } from './PeopleRepository';
import AppContext from './appContext';

export interface PeopleGridProps {
  emptyText?: string;
}

export const PeopleGrid: React.FC<PeopleGridProps> = ({ emptyText = 'No people found.' }) => {
  const [people, setPeople] = useState<Person[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const app = React.useContext(AppContext);
  const repository = new PeopleRepository(app.GetPeopleProvider());
  useEffect(() => {
    const fetchPeople = async () => {
      try {
        const data = await repository.getAllPeople();
        setPeople(data);
      } catch (err) {
        console.error(err);
        setError('Failed to load people.');
      } finally {
        setLoading(false);
      }
    };

    fetchPeople();
  }, []);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>{error}</div>;
  if (!people.length) return <div>{emptyText}</div>;

  return (
    <div
      style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))',
        gap: '0.5rem'
      }}
    >
      {people.map((p, index) => (
        <PersonCard key={index} initPerson={p} />
      ))}
    </div>
  );
};
