import React, { useState } from 'react';
import { Person, Dog } from '@typeshim/wasm-exports';

export interface PersonCardProps {
  initPerson: Person.Proxy;
}

export const PersonCard: React.FC<PersonCardProps> = ({ initPerson }) => {
  const [wrapper, setPerson] = useState<{person: Person.Proxy}>({person: initPerson});
  const personSnapshot = Person.snapshot(wrapper.person);
  const petSnapshot = personSnapshot.Pet;
  const personProxy = wrapper.person;
  const petProxy = personProxy.Pet;
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
      <div title={`${personSnapshot.Name} (Id: ${personSnapshot.Id})`} style={{ padding: '0.5rem' }}>
        <h3 style={{ margin: '0 0 0.25rem 0' }}>{personSnapshot.Name}</h3>
        <p style={{ margin: 0, fontSize: '0.875rem', color: '#555' }}>Age: {personSnapshot.Age}</p>
      </div>
      {petSnapshot && petProxy && (
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
          }} title={`Pet: ${petSnapshot.Name} (Breed: ${petSnapshot.Breed})`}>
            Pet: {petSnapshot.Name} {petProxy.GetAge(false)} years/{petProxy.GetAge(true)} years ({petSnapshot.Breed}) - {petProxy.Bark()}
          </span>
        </div>
      )}
      {!petSnapshot && (
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
              personProxy.Adopt({ Name: "New Pet", Breed: "Unknown", Age: 1 } as Dog.Snapshot as any); 
              setPerson({...wrapper}); 
            }}>
            Adopt a pet!
          </button>
        </div>
      )}
    </div>
  );
};
