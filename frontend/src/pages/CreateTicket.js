import React, { useState } from 'react';

function CreateTicket() {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    try {
      const response = await fetch('http://localhost:5254/api/Tickets', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          title,
          description,
          priority: 'medium', // Default priority
          status: 'open'     // Initial status
        })
      });

      if (!response.ok) {
        throw new Error('Failed to create ticket');
      }

      // Clear form on success
      setTitle('');
      setDescription('');
      setError('');
      alert('Ticket created successfully!');

    } catch (err) {
      setError(err.message);
      console.error('Error creating ticket:', err);
    }
  };

  return (
    <div className="create-ticket">
      <h2>Create New Ticket</h2>
      {error && <p className="error">{error}</p>}
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label>Title:</label>
          <input 
            type="text" 
            value={title} 
            onChange={(e) => setTitle(e.target.value)} 
            required 
          />
        </div>
        <div className="form-group">
          <label>Description:</label>
          <textarea 
            value={description} 
            onChange={(e) => setDescription(e.target.value)} 
            required 
          />
        </div>
        <button type="submit">Submit Ticket</button>
      </form>
    </div>
  );
}

export default CreateTicket;