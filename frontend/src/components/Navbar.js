import React from 'react';
import { Link } from 'react-router-dom';
import './Navbar.css';

const Navbar = () => {
  const isAuthenticated = localStorage.getItem('user') !== null;
  
  const handleLogout = () => {
    localStorage.removeItem('user');
    window.location.href = '/login';
  };

  return (
    <nav className="navbar">
      <div className="navbar-container">
        <Link to="/" className="navbar-logo">
          TroubleTicket
        </Link>
        
        <ul className="nav-menu">
          {isAuthenticated && (
            <>
              <li className="nav-item">
                <Link to="/tickets" className="nav-links">Tickets</Link>
              </li>
              <li className="nav-item">
                <Link to="/analytics" className="nav-links">Analytics</Link>
              </li>
            </>
          )}
        </ul>
        
        <div className="nav-auth">
          {isAuthenticated ? (
            <button 
              className="nav-links"
              onClick={handleLogout}
            >
              Logout
            </button>
          ) : (
            <Link to="/login" className="nav-links">Login</Link>
          )}
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
