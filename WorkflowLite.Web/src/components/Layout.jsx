import { NavLink, useNavigate } from 'react-router-dom';

export default function Layout({ children }) {
  const nav = useNavigate();
  const user = JSON.parse(localStorage.getItem('user') || '{}');

  const logout = () => {
    localStorage.clear();
    nav('/login');
  };

  return (
    <div className="app-layout">
      <aside className="sidebar">
        <div className="sidebar-logo">WorkflowLite</div>
        <nav>
          <NavLink to="/" end className={({ isActive }) => isActive ? 'active' : ''}>
            Workflows
          </NavLink>
          <NavLink to="/workflows/create" className={({ isActive }) => isActive ? 'active' : ''}>
            + New Workflow
          </NavLink>
        </nav>
        <div className="sidebar-footer">
          <div style={{ fontSize: 12, color: 'rgba(255,255,255,0.5)', marginBottom: 10 }}>
            {user.fullName || user.email}
          </div>
          <button onClick={logout}>Sign out</button>
        </div>
      </aside>
      <main className="main-content">{children}</main>
    </div>
  );
}