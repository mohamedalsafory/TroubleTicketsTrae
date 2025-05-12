import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import './theme.css';
import './App.css';
import Navbar from './components/Navbar';
import Footer from './components/Footer';
import Login from './pages/Login';
import Tickets from './pages/Tickets';
import CreateTicket from './pages/CreateTicket';
import TicketDetails from './pages/TicketDetails';
import Analytics from './pages/Analytics';

const PrivateRoute = ({ children }) => {
  const isAuthenticated = localStorage.getItem('user') !== null;
  return isAuthenticated ? children : <Navigate to="/login" />;
};

function App() {
  return (
    <Router>
      <Navbar />
      <div className="main-content">
        <Routes>
          <Route 
            path="/" 
            element={<PrivateRoute><Tickets /></PrivateRoute>} 
          />
          <Route path="/login" element={<Login />} />
          <Route 
            path="/tickets" 
            element={
              <PrivateRoute>
                <Tickets />
              </PrivateRoute>
            } 
          />
          <Route 
            path="/tickets/create" 
            element={
              <PrivateRoute>
                <CreateTicket />
              </PrivateRoute>
            } 
          />
          <Route 
            path="/tickets/:id" 
            element={
              <PrivateRoute>
                <TicketDetails />
              </PrivateRoute>
            } 
          />
          <Route 
            path="/analytics" 
            element={
              <PrivateRoute>
                <Analytics />
              </PrivateRoute>
            } 
          />
        </Routes>
      </div>
      <Footer />
    </Router>
  );
}

export default App;
