import axios from 'axios';

const API_BASE_URL = 'https://localhost:7296/api';

const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
    withCredentials: true
});

export default api; 