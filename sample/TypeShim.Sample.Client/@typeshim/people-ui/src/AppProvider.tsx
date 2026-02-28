import { MyApp } from '@typeshim/wasm-exports';
import { useMemo, ReactNode } from 'react';
import AppContext from './appContext';

export interface AppProviderProps {
    children: ReactNode;
}

export function AppProvider({ children }: AppProviderProps) {
    const myApp = new MyApp(document.baseURI);
    const value = useMemo(() => (myApp), [myApp]);
    return <AppContext.Provider value={value}> {children} </AppContext.Provider>;
}