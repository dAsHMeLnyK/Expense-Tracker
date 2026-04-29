import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '10s', target: 50 }, // Розгін до 50 користувачів
        { duration: '30s', target: 50 }, // Тримаємо навантаження
        { duration: '10s', target: 0 },  // Зниження
    ],
};

const BASE_URL = 'http://localhost:5191'; // Замініть на реальний порт вашого API під час запуску

export default function () {
    const month = new Date().getMonth() + 1;
    const year = new Date().getFullYear();

    // Запит 1: Місячний звіт
    const monthlyRes = http.get(`${BASE_URL}/api/reports/monthly?month=${month}&year=${year}`);
    check(monthlyRes, {
        'monthly report status is 200': (r) => r.status === 200,
    });

    // Запит 2: Загальний підсумок
    const summaryRes = http.get(`${BASE_URL}/api/reports/summary`);
    check(summaryRes, {
        'summary status is 200': (r) => r.status === 200,
    });

    sleep(1); // Пауза в 1 секунду між ітераціями
}