import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '10s', target: 50 },
        { duration: '30s', target: 50 },
        { duration: '10s', target: 0 },
    ],
};

const BASE_URL = 'http://localhost:5191';

export default function () {
    const month = new Date().getMonth() + 1;
    const year = new Date().getFullYear();
    
    const monthlyRes = http.get(`${BASE_URL}/api/reports/monthly?month=${month}&year=${year}`);
    check(monthlyRes, {
        'monthly report status is 200': (r) => r.status === 200,
    });
    
    const summaryRes = http.get(`${BASE_URL}/api/reports/summary`);
    check(summaryRes, {
        'summary status is 200': (r) => r.status === 200,
    });

    sleep(1);
}