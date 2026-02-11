import React from 'react';
import { Dog } from '@typeshim/wasm-exports';

export interface PetChipProps {
  petProxy: Dog;
  onClick?: (pet: Dog) => void;
}

export const PetChip: React.FC<PetChipProps> = ({ petProxy, onClick }) => {
  const name = petProxy.Name;
  const breed = petProxy.Breed;
  const age = petProxy.Age;
  const humanAge = petProxy.GetAge(true);
  const bark = petProxy.Bark();

  return (
    <span
      role="button"
      tabIndex={0}
      style={{
        display: 'inline-block',
        marginTop: '0.5rem',
        marginRight: '0.5rem',
        padding: '0.25rem 0.5rem',
        borderRadius: 9999,
        background: '#eef6ff',
        color: '#1e3a8a',
        fontSize: '0.75rem',
        border: '1px solid #cfe0ff',
        cursor: 'pointer',
        userSelect: 'none',
      }}
      title={`Pet: ${name} (Breed: ${breed})`}
      onClick={() => onClick?.(petProxy)}
    >
      Pet: {name} {age}/{humanAge} ({breed}) - {bark}
    </span>
  );
};
