import React from 'react'
import { createBrowserRouter } from 'react-router-dom'

import About from '@/pages/About/index'
import Home from '@/pages/Home/index'
import Layouts from '@/layouts/index'

const router = createBrowserRouter(
    [
        {
            path: '/Home',
            element: <Layouts />,
            children: [
                {
                    path: '/Home',
                    
                    element: <Home />
                },
                {
                    path: '/Home/About',
                    element: <About />
                },
            ]
        },
        {
            path: '/',
            element: <Layouts />,
    
        },

    ]
)





export default router
