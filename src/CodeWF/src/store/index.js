import { createStore, applyMiddleware } from '@reduxjs/toolkit';
import rootReducer from './reducers'; // 导入 Reducer 文件
import { thunk } from 'redux-thunk';



const store = createStore(rootReducer, applyMiddleware(thunk));



export default store;

