// File: /components/SubscriptionPieChart.jsx

import React, { useState, useEffect } from 'react';
import { Pie } from 'react-chartjs-2';
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from 'chart.js';
import { useSession } from '/src/hooks/useSession';
import { API_GATEWAY_URL } from '/src/config';
import Skeleton from 'react-loading-skeleton';
import 'react-loading-skeleton/dist/skeleton.css';

// Đăng ký các thành phần cần thiết cho Chart.js
ChartJS.register(ArcElement, Tooltip, Legend);

function SubscriptionPieChart() {
    const [chartData, setChartData] = useState(null);
    const [loading, setLoading] = useState(true);
    const { getAuthHeaders } = useSession();

    useEffect(() => {
        const fetchData = async () => {
            try {
                setLoading(true);
                const response = await fetch(`${API_GATEWAY_URL}/api/subscriptions/stats-by-type`, {
                    headers: getAuthHeaders()
                });
                if (!response.ok) throw new Error('Could not fetch chart data');
                const data = await response.json();
                
                // Chuyển đổi dữ liệu từ API sang định dạng Chart.js yêu cầu
                const formattedData = {
                    labels: data.map(item => item.type),
                    datasets: [
                        {
                            label: '# of Subscriptions',
                            data: data.map(item => item.count),
                            backgroundColor: [
                                'rgba(255, 99, 132, 0.7)',
                                'rgba(54, 162, 235, 0.7)',
                                'rgba(255, 206, 86, 0.7)',
                                'rgba(75, 192, 192, 0.7)',
                                'rgba(153, 102, 255, 0.7)',
                                'rgba(255, 159, 64, 0.7)',
                            ],
                            borderColor: [
                                'rgba(255, 99, 132, 1)',
                                'rgba(54, 162, 235, 1)',
                                'rgba(255, 206, 86, 1)',
                                'rgba(75, 192, 192, 1)',
                                'rgba(153, 102, 255, 1)',
                                'rgba(255, 159, 64, 1)',
                            ],
                            borderWidth: 1,
                        },
                    ],
                };
                setChartData(formattedData);
            } catch (error) {
                console.error("Chart Error:", error);
            } finally {
                setLoading(false);
            }
        };
        fetchData();
    }, [getAuthHeaders]);
    
    if (loading) {
        return <div style={{width: 250, height: 250, margin: 'auto'}}><Skeleton circle height="100%" /></div>;
    }

    if (!chartData || chartData.labels.length === 0) {
        return <p>Không có dữ liệu để vẽ biểu đồ.</p>;
    }

    return <div style={{maxWidth: '350px', margin: 'auto'}}><Pie data={chartData} /></div>;
}

export default SubscriptionPieChart;