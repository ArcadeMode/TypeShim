"use client"

import React, { useEffect, useState } from 'react';
import { PersonCard } from './PersonCard';
import type { Person } from '@typeshim/wasm-exports';
import { PeopleRepository } from './PeopleRepository';

export interface PeopleListProps {
  emptyText?: string;
  repository: PeopleRepository;
}

export const PeopleList: React.FC<PeopleListProps> = ({ emptyText = 'No people found.', repository }) => {
  const [people, setPeople] = useState<Person.Proxy[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

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
    <div>
      <div>
        {people.map((p, index) => <PersonCard key={index} initPerson={p} />)}
      </div>
    </div>
  );
};