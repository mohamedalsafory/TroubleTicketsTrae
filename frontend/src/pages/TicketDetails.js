import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import axios from 'axios';

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
        const [ticketRes, messagesRes] = await Promise.all([
          axios.get(`http://localhost:5254/api/tickets/${id}`),
          axios.get(`http://localhost:5254/api/messages?ticketId=${id}`)
        ]);
        setTicket(ticketRes.data);
        setMessages(messagesRes.data);
        setLoading(false);
      } catch (err) {
        setError('Failed to load ticket details');
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

  if (loading) return <div>Loading...</div>;
  if (error) return <div>{error}</div>;
  if (!ticket) return <div>Ticket not found</div>;

  return (
    <div className="ticket-details">
      <h2>{ticket.title}</h2>
      <p>Status: {ticket.status}</p>
      <p>{ticket.description}</p>
      
      <div className="messages-section">
        <h3>Messages</h3>
        {messages.map(message => (
          <div key={message.id} className="message">
            <p>{message.content}</p>
            <small>{new Date(message.createdAt).toLocaleString()}</small>
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
