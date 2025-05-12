import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import './App.css';
import Login from './pages/Login';
import Tickets from './pages/Tickets';
import CreateTicket from './pages/CreateTicket';
import TicketDetails from './pages/TicketDetails';
import Analytics from './pages/Analytics';

function App() {
  // In a real app, you would check authentication state here
  const isAuthenticated = localStorage.getItem('token');

  return (
    <Router>
      <Routes>
        <Route 
          path="/" 
          element={isAuthenticated ? <Tickets /> : <Navigate to="/login" />} 
        />
        <Route path="/login" element={<Login />} />
        <Route path="/tickets" element={<Tickets />} />
        <Route path="/tickets/create" element={<CreateTicket />} />
        <Route path="/tickets/:id" element={<TicketDetails />} />
        <Route path="/analytics" element={<Analytics />} />
      </Routes>
    </Router>
  );
}

export default App;
