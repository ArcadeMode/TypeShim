import React from 'react';
import type { Person } from '@typeshim/people-exports';

export interface PersonCardProps {
  person: Person;
}

export const PersonCard: React.FC<PersonCardProps> = ({ person }) => {
  const pet = person.GetPet();
  return (
      <div
          title={`${person.GetName()} (Id: ${person.GetId()})`}
          style={{
              border: '1px solid #ddd',
              borderRadius: 6,
              padding: '0.75rem',
              marginBottom: '0.5rem',
              background: '#fff',
              display: 'flex',
              flexDirection: 'row',
              justifyContent: 'space-between',
          }}>
      <div>
        <h3 style={{ margin: '0 0 0.25rem 0' }}>{person.GetName()}</h3>
        <p style={{ margin: 0, fontSize: '0.875rem', color: '#555' }}>Age: {person.GetAge()}</p>
      </div>
      {pet && (
        <div>
          <span style={{
            display: 'inline-block',
            marginTop: '0.5rem',
            padding: '0.25rem 0.5rem',
            borderRadius: 9999,
            background: '#eef6ff',
            color: '#1e3a8a',
            fontSize: '0.75rem',
            border: '1px solid #cfe0ff'
          }}>
            Pet: {pet.GetName()} ({pet.GetBreed()}) - {pet.Bark()}
          </span>
        </div>
      )}
    </div>
  );
};
