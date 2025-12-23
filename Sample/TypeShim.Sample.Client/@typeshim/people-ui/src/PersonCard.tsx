import React, { useState } from 'react';
import { Person, Dog } from '@typeshim/wasm-exports';
import { PetChip } from './PetChip';

export interface PersonCardProps {
  initPerson: Person.Proxy;
}

export const PersonCard: React.FC<PersonCardProps> = ({ initPerson }) => {
  const [wrapper, setPerson] = useState<{person: Person.Proxy}>({person: initPerson});
  const person = wrapper.person;
  return (
    <div
        style={{
            border: '1px solid #ddd',
            borderRadius: 6,
            padding: '0.75rem',
            marginBottom: '0.5rem',
            background: '#fff',
        }}>
          <div style={{display: 'flex',
            flexDirection: 'row',
            justifyContent: 'space-between'}}>
              <div title={`${person.Name} (Id: ${person.Id})`} style={{ padding: '0.5rem' }}>
                <h3 style={{ margin: '0 0 0.25rem 0' }}>
                  {person.Name}
                </h3>
                <p style={{ margin: 0, fontSize: '0.875rem', color: '#555' }}>
                  Age: {person.Age}
                </p>
              </div>
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginTop: '0.5rem' }}>
                <button
                  style={{
                    display: 'inline-block',
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
                    setPerson({ ...wrapper });
                  }}
                >
                  Adopt a .NET pet!
                </button>
                <button
                  style={{
                    display: 'inline-block',
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
                    person.Pets = [...person.Pets, { Name: "JS-Doggo", Age: 2, Breed: "Golden Retriever" }];
                    setPerson({ ...wrapper });
                  }}
                >
                  Adopt a JS pet!
                </button>
              </div>
          </div>
            
      <div>
        {person.Pets.length > 0 ? (
          <div>
            {person.Pets.map((p, idx) => (
              <PetChip
                key={idx}
                petProxy={p}
                onClick={(pet) => {
                  pet.Age += 1;
                  setPerson({ ...wrapper });
                }}
              />
            ))}
          </div>
        ) : (<div></div>)}
        
      </div>
    </div>
  );
};
