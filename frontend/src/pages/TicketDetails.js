import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import axios from 'axios';
import './TicketDetails.css';

const TicketDetails = () => {
  const { id } = useParams();
  const [ticket, setTicket] = useState(null);
  const [messages, setMessages] = useState([]);
  const [newMessage, setNewMessage] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        setError('');
        
        const [ticketRes, messagesRes] = await Promise.all([
          axios.get(`http://localhost:5254/api/tickets/${id}`),
          axios.get(`http://localhost:5254/api/messages/tickets/${id}/messages`)
        ]);
        
        if (!ticketRes.data) {
          throw new Error('Ticket data not found');
        }
        
        setTicket(ticketRes.data);
        setMessages(messagesRes.data || []);
      } catch (err) {
        console.error('Error loading ticket:', err);
        setError(err.response?.data?.message || err.message || 'Failed to load ticket details');
      } finally {
        setLoading(false);
      }
    };
    
    fetchData();
  }, [id]);

  const handleSendMessage = async (e) => {
    e.preventDefault();
    try {
      const response = await axios.post('http://localhost:5254/api/messages', {
        ticketId: id,
        content: newMessage
      });
      setMessages([...messages, response.data]);
      setNewMessage('');
    } catch (err) {
      setError('Failed to send message');
    }
  };

  if (loading) return <div>Loading ticket details...</div>;
  if (error) return <div className="error">{error}</div>;
  if (!ticket) return <div>No ticket data available</div>;

  return (
    <div className="ticket-details">
      <div className="ticket-properties">
        <div className="label-value">
          <span className="label-value__label">Ticket ID:</span>
          <span className="label-value__value">{ticket.id}</span>
        </div>
        <div className="label-value">
          <span className="label-value__label">Title:</span>
          <span className="label-value__value">{ticket.title}</span>
        </div>
        <div className="label-value">
          <span className="label-value__label">Status:</span>
          <span className="label-value__value status-badge status-{ticket.status}">{ticket.status}</span>
        </div>
        <div className="label-value">
          <span className="label-value__label">Priority:</span>
          <span className={`label-value__value priority-${ticket.priority}`}>{ticket.priority}</span>
        </div>
        <div className="label-value">
          <span className="label-value__label">Category:</span>
          <span className="label-value__value">{ticket.categoryId || '-'}</span>
        </div>
        <div className="label-value">
          <span className="label-value__label">Created By:</span>
          <span className="label-value__value">{ticket.createdById || '-'}</span>
        </div>
        <div className="label-value">
          <span className="label-value__label">Assigned To:</span>
          <span className="label-value__value">{ticket.assignedToId || '-'}</span>
        </div>
        <div className="label-value">
          <span className="label-value__label">Created At:</span>
          <span className="label-value__value">{ticket.createdAt ? new Date(ticket.createdAt).toLocaleString() : '-'}</span>
        </div>
        <div className="label-value">
          <span className="label-value__label">Updated At:</span>
          <span className="label-value__value">{ticket.updatedAt ? new Date(ticket.updatedAt).toLocaleString() : '-'}</span>
        </div>
        <div className="label-value ticket-description">
          <span className="label-value__label">Description:</span>
          <span className="label-value__value">{ticket.description}</span>
        </div>
      </div>

      <div className="messages-section">
        <h3>Messages</h3>
        {messages.map(message => (
          <div key={message.id} className="message">
            <div className="message-meta">
              <span><b>Sender:</b> {message.userId || '-'}</span>
              <span><b>At:</b> {message.createdAt ? new Date(message.createdAt).toLocaleString() : '-'}</span>
            </div>
            <div className="message-content">{message.content}</div>
          </div>
        ))}
        <form onSubmit={handleSendMessage}>
          <textarea 
            value={newMessage}
            onChange={(e) => setNewMessage(e.target.value)}
            placeholder="Type your message..."
            required
          />
          <button type="submit">Send Message</button>
        </form>
      </div>
    </div>
  );
};

export default TicketDetails;
