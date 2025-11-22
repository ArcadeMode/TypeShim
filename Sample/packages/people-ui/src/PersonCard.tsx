import React from 'react';
import type { Person } from './types';

export interface PersonCardProps {
  person: Person;
}

export const PersonCard: React.FC<PersonCardProps> = ({ person }) => {
  return (
    <div style={{
      border: '1px solid #ddd',
      borderRadius: 6,
      padding: '0.75rem',
      marginBottom: '0.5rem',
      background: '#fff'
    }}>
      <h3 style={{ margin: '0 0 0.25rem 0' }}>{person.name}</h3>
      <p style={{ margin: 0, fontSize: '0.875rem', color: '#555' }}>{person.role}</p>
    </div>
  );
};
