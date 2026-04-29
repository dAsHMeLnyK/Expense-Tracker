import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '5s', target: 20 },
        { duration: '15s', target: 100 }, // Стрибок до 100 користувачів (стрес)
        { duration: '10s', target: 100 },
        { duration: '10s', target: 0 },
    ],

    thresholds: {
        http_req_failed: ['rate<0.01'], // помилок має бути менше 1%
        http_req_duration: ['p(95)<500'], // 95% запитів мають бути швидшими за 500мс
    },
};

const BASE_URL = 'http://localhost:5191';

export default function () {
    const payload = JSON.stringify({
        userId: Math.floor(Math.random() * 10) + 1,
        amount: (Math.random() * 100) + 1,
        description: 'Stress Test Expense',
        categoryId: 1, // Припускаємо, що категорія з Id 1 існує (Seeder її створить)
        date: new Date().toISOString(),
        paymentMethod: 1 // Card
    });

    const params = {
        headers: {
            'Content-Type': 'application/json',
        },
    };

    const res = http.post(`${BASE_URL}/api/expenses`, payload, params);

    check(res, {
        'is status 201 (Created)': (r) => r.status === 201,
    });

    sleep(0.5);
}