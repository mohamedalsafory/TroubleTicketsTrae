import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend } from 'recharts';

const Analytics = () => {
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const response = await axios.get('/api/analytics');
        setStats(response.data);
        setLoading(false);
      } catch (err) {
        setError('Failed to load analytics');
        setLoading(false);
      }
    };
    fetchStats();
  }, []);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>{error}</div>;
  if (!stats) return <div>No analytics data available</div>;

  return (
    <div className="analytics-container">
      <h2>Analytics Dashboard</h2>
      
      <div className="chart-container">
        <h3>Tickets by Status</h3>
        <BarChart width={600} height={300} data={stats.ticketsByStatus}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="status" />
          <YAxis />
          <Tooltip />
          <Legend />
          <Bar dataKey="count" fill="#8884d8" />
        </BarChart>
      </div>
      
      <div className="stats-grid">
        <div className="stat-card">
          <h4>Total Tickets</h4>
          <p>{stats.totalTickets}</p>
        </div>
        <div className="stat-card">
          <h4>Open Tickets</h4>
          <p>{stats.openTickets}</p>
        </div>
        <div className="stat-card">
          <h4>Avg Resolution Time</h4>
          <p>{stats.avgResolutionTime} days</p>
        </div>
      </div>
    </div>
  );
};

export default Analytics;
