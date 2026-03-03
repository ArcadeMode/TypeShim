import { PeopleApp } from '@client/wasm-exports';
import { useMemo, ReactNode } from 'react';
import AppContext from './appContext';

export interface AppProviderProps {
    children: ReactNode;
}

export function AppProvider({ children }: AppProviderProps) {
    // TypeShim automatically map the object literal to an PeopleAppOptions instance required for the PeopleApp constructor
    const peopleApp = new PeopleApp({ BaseAddress: document.baseURI}); 
    const value = useMemo(() => (peopleApp), [peopleApp]);
    return <AppContext.Provider value={value}> {children} </AppContext.Provider>;
}