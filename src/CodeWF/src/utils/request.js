import axios from 'axios';


const baseURL = '/';


const instance = axios.create({
    baseURL,
    timeout: 1000 * 60 * 1000,
});


instance.interceptors.request.use(config => {

    /** */
    return config;
});


instance.interceptors.response.use(
    response => {
        return response.data;
    },
    r => {
        let resultError = { response: r };

        return Promise.reject(resultError);
    },
);
export default instance


