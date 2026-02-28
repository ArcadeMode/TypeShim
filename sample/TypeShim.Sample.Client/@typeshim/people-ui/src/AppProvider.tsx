import { PeopleApp } from '@typeshim/wasm-exports';
import { useMemo, ReactNode } from 'react';
import AppContext from './appContext';

export interface AppProviderProps {
    children: ReactNode;
}

export function AppProvider({ children }: AppProviderProps) {
    const peopleApp = new PeopleApp(document.baseURI);
    const value = useMemo(() => (peopleApp), [peopleApp]);
    return <AppContext.Provider value={value}> {children} </AppContext.Provider>;
}