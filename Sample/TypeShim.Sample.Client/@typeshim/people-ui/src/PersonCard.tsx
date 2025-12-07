import React, { useState } from 'react';
import type { Person } from '@typeshim/people-exports';

export interface PersonCardProps {
  initPerson: Person;
}

export const PersonCard: React.FC<PersonCardProps> = ({ initPerson }) => {
  const [wrapper, setPerson] = useState<{person: Person}>({person: initPerson});
  const person = wrapper.person;
  const pet = person.Pet;
  return (
      <div
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
      <div title={`${person.Name} (Id: ${person.Id})`} style={{ padding: '0.5rem' }}>
        <h3 style={{ margin: '0 0 0.25rem 0' }}>{person.Name}</h3>
        <p style={{ margin: 0, fontSize: '0.875rem', color: '#555' }}>Age: {person.Age}</p>
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
            Pet: {pet.Name} ({pet.Breed}) - {pet.Bark()}
          </span>
        </div>
      )}
      {!pet && (
        <div style={{ display: 'flex', alignItems: 'center' }}>
          <button 
            style={{
              display: 'inline-block',
              marginTop: '0.5rem',
              padding: '.5rem 0.75rem',
              borderRadius: 4,
              background: 'rgb(255 238 238)',
              color: 'rgb(138 30 30)',
              fontSize: '0.75rem',
              border: '1px solid rgb(255 207 207)',
              boxShadow: 'rgba(0, 0, 0, 0.1) 1px 1px 4px 0px',
              cursor: 'pointer'
            }}
            onClick={() => {
              person.AdoptPet(); 
              setPerson({...wrapper}); 
            }}>
            Adopt a pet!
          </button>
        </div>
      )}
    </div>
  );
};
