import React from 'react';
import { createRoot } from 'react-dom/client';
import App from './App';
import './index.css';

import { setAuthTokenGetter } from '@workspace/api-client-react';
import { tokenRef } from './contexts/AuthContext';

setAuthTokenGetter(() => tokenRef.current);

const root = document.getElementById('root');
if (root) {
  createRoot(root).render(
    <React.StrictMode>
      <App />
    </React.StrictMode>
  );
}
