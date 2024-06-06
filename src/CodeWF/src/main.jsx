import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.jsx'
import './index.css'



import { Provider } from 'react-redux';
import storeData from '@/store/index';

ReactDOM.createRoot(document.getElementById('root')).render(

  <Provider store={storeData}>
    {/* <React.StrictMode> */}
      <App />
    {/* </React.StrictMode> */}
  </Provider>,
)
