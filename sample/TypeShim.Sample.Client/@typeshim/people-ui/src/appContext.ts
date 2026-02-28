import { PeopleApp } from '@typeshim/wasm-exports';
import React from 'react';

const AppContext = React.createContext<PeopleApp>(undefined!);
export default AppContext;
