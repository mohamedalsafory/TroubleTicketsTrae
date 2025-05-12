import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Link } from 'react-router-dom';
import './Tickets.css';

const Tickets = () => {
  const [tickets, setTickets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchTickets = async () => {
      try {
        // Add explicit API base URL
        const response = await axios.get('http://localhost:5254/api/tickets', {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          }
        });
        console.log('API Response:', response);
        setTickets(response.data);
        setLoading(false);
      } catch (err) {
        console.error('API Error:', err);
        setError(`Failed to load tickets: ${err.message}`);
        if (err.response) {
          console.error('Response data:', err.response.data);
          console.error('Status:', err.response.status);
        }
        setLoading(false);
      }
    };
    fetchTickets();
  }, []);

  if (loading) return <div className="loading">Loading tickets...</div>;
  if (error) return <div className="error">{error}</div>;

  return (
    <div className="tickets-container">
      <h1>Ticket Dashboard</h1>
      <Link to="/tickets/create" className="btn">+ Create New Ticket</Link>
      
      <div className="tickets-list">
        {tickets.map(ticket => (
          <div key={ticket.id} className="ticket-card">
            <h3>{ticket.title}</h3>
            <p>{ticket.description}</p>
            <div className="ticket-meta">
              <span className={`status ${ticket.status}`}>{ticket.status}</span>
              <Link to={`/tickets/${ticket.id}`} className="view-link">View Details →</Link>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default Tickets;
