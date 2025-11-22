import React from 'react';
import { PersonCard } from './PersonCard';
import type { Person } from './types';

export interface PeopleListProps {
  people: Person[];
  emptyText?: string;
}

export const PeopleList: React.FC<PeopleListProps> = ({ people, emptyText = 'No people found.' }) => {
  if (!people.length) return <div>{emptyText}</div>;
  return (
    <div>
      {people.map(p => <PersonCard key={p.id} person={p} />)}
    </div>
  );
};
