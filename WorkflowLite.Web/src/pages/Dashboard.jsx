import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { getWorkflows, deleteWorkflow } from '../api/workflows';

export default function Dashboard() {
  const [workflows, setWorkflows] = useState([]);
  const [loading, setLoading] = useState(true);
  const nav = useNavigate();

  const load = async () => {
    try {
      const res = await getWorkflows();
      setWorkflows(res.data);
    } catch {
      // handled by interceptor
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleDelete = async (id, name) => {
    if (!confirm(`Delete workflow "${name}"?`)) return;
    await deleteWorkflow(id);
    setWorkflows(workflows.filter(w => w.id !== id));
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Workflows</h1>
          <p>All your automated business processes</p>
        </div>
        <button className="btn btn-primary" onClick={() => nav('/workflows/create')}>
          + New Workflow
        </button>
      </div>

      {loading && <p style={{ color: '#888' }}>Loading...</p>}

      {!loading && workflows.length === 0 && (
        <div className="empty-state">
          <h3>No workflows yet</h3>
          <p>Create your first workflow to get started</p>
          <button className="btn btn-primary" onClick={() => nav('/workflows/create')}>
            + Create Workflow
          </button>
        </div>
      )}

      {workflows.map(w => (
        <div className="card" key={w.id}>
          <div className="card-header">
            <div>
              <div className="card-title">{w.name}</div>
              <div className="card-desc">{w.description}</div>
            </div>
            <span className={`badge ${w.isActive ? 'badge-active' : 'badge-inactive'}`}>
              {w.isActive ? 'Active' : 'Inactive'}
            </span>
          </div>
          <div style={{ fontSize: 13, color: '#888', marginBottom: 14 }}>
            {w.steps.length} step{w.steps.length !== 1 ? 's' : ''} &nbsp;·&nbsp;
            Created {new Date(w.createdAt).toLocaleDateString()}
          </div>
          <div className="btn-row">
            <Link className="btn btn-ghost" to={`/workflows/${w.id}`}>View & Trigger</Link>
            <button className="btn btn-danger" onClick={() => handleDelete(w.id, w.name)}>
              Delete
            </button>
          </div>
        </div>
      ))}
    </div>
  );
}