import React from 'react'
import { createBrowserRouter } from 'react-router-dom'

import About from '@/pages/About/index'
import Home from '@/pages/Home/index'
import Layouts from '@/layouts/index'

const router = createBrowserRouter(
    [
        {
            path: '/',
            element: <Layouts />,
            children: [
                {
                    path: '/',
                    
                    element: <Home />
                },
            ]
        },
        {
            path: '/About',
            element: <Layouts />,
            children: [
                {
                    path: '/About',
                    element: <About />
                },
            ]
        },
    ]
)





export default router
