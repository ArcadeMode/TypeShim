import { MyApp } from '@typeshim/wasm-exports';
import React from 'react';

const AppContext = React.createContext<MyApp>(undefined!);
export default AppContext;
