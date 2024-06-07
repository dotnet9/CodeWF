import React from 'react'
import { createBrowserRouter } from 'react-router-dom'

import About from '@/pages/About/index'
import Home from '@/pages/Home/index'
// import Home from '@/pages/Home/index'

import App from '../App'

const router = createBrowserRouter(
    [
        {
            path: '/',
            element: <Home />
        },
        {
            path: '/About',
            element: <About />
        }
    ]
)





export default router
