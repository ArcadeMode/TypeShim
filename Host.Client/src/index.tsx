import React from 'react';
import ReactDOM from 'react-dom/client';

import App from './App';

console.log("Starting React app...");
ReactDOM.createRoot(document.querySelector('#root')!).render(
   <App/>
);
