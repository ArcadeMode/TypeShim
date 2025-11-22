import React from 'react';
import { PeopleList, Person } from '@typeshim/people-ui';

const samplePeople: Person[] = [
  { id: '1', name: 'Ada Lovelace', role: 'Mathematician' },
  { id: '2', name: 'Grace Hopper', role: 'Computer Scientist' },
  { id: '3', name: 'Alan Turing', role: 'Logician' }
];

export default function PeoplePage() {
  return (
    <div>
      <h1>People</h1>
      <p>These components are imported from <code>@typeshim/people-ui</code>.</p>
      <PeopleList people={samplePeople} emptyText="No people yet." />
    </div>
  );
}
