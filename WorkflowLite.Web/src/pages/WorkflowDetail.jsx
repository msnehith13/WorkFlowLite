import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getWorkflow, triggerWorkflow } from '../api/workflows';

const STATUS_LABELS = { 1: 'Pending', 2: 'Running', 3: 'Completed', 4: 'Failed', 5: 'Cancelled' };
const STATUS_BADGE = { 1: 'badge-pending', 2: 'badge-running', 3: 'badge-completed', 4: 'badge-failed', 5: 'badge-inactive' };
const STEP_TYPE_LABELS = { 1: 'Assign to User', 2: 'Send Email', 3: 'Mark Complete', 4: 'Approval' };
const OPERATOR_LABELS = { 1: 'equals', 2: 'not equals', 3: '>', 4: '<', 5: 'contains' };
const LOG_BADGE = { 1: 'badge-pending', 2: 'badge-running', 3: 'badge-completed', 4: 'badge-failed', 5: 'badge-inactive' };
const LOG_LABELS = { 1: 'Pending', 2: 'Running', 3: 'Completed', 4: 'Failed', 5: 'Skipped' };

export default function WorkflowDetail() {
  const { id } = useParams();
  const nav = useNavigate();
  const [workflow, setWorkflow] = useState(null);
  const [contextJson, setContextJson] = useState('{"status":"approved"}');
  const [result, setResult] = useState(null);
  const [triggering, setTriggering] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    getWorkflow(id).then(r => setWorkflow(r.data)).catch(() => nav('/'));
  }, [id]);

  const trigger = async () => {
    setError('');
    setResult(null);
    setTriggering(true);
    try {
      const res = await triggerWorkflow(id, contextJson);
      setResult(res.data);
    } catch (err) {
      setError(err.response?.data?.error || 'Trigger failed.');
    } finally {
      setTriggering(false);
    }
  };

  if (!workflow) return <p style={{ color: '#888', padding: 32 }}>Loading...</p>;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>{workflow.name}</h1>
          <p>{workflow.description}</p>
        </div>
        <button className="btn btn-ghost" onClick={() => nav('/')}>← Back</button>
      </div>

      {/* Steps */}
      <div className="card">
        <div className="card-title" style={{ marginBottom: 16 }}>
          Steps ({workflow.steps.length})
        </div>
        {workflow.steps.map((step, i) => (
          <div key={step.id} style={{ marginBottom: 16, paddingBottom: 16, borderBottom: i < workflow.steps.length - 1 ? '1px solid #f0f0f0' : 'none' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 6 }}>
              <span className="step-num">Step {step.order}</span>
              <span style={{ fontWeight: 500 }}>{step.name}</span>
              <span style={{ fontSize: 12, color: '#888', background: '#f4f4f4', padding: '2px 8px', borderRadius: 10 }}>
                {STEP_TYPE_LABELS[step.type]}
              </span>
            </div>
            {step.description && <div style={{ fontSize: 13, color: '#666', marginBottom: 8 }}>{step.description}</div>}
            {step.rules.length > 0 && (
              <div style={{ fontSize: 12, color: '#888' }}>
                Rules: {step.rules.map((r, ri) => (
                  <span key={ri} style={{ background: '#f0f4ff', color: '#5060c0', padding: '2px 8px', borderRadius: 10, marginRight: 6 }}>
                    {r.field} {OPERATOR_LABELS[r.operator]} "{r.value}"
                  </span>
                ))}
              </div>
            )}
          </div>
        ))}
      </div>

      {/* Trigger */}
      <div className="card">
        <div className="card-title" style={{ marginBottom: 6 }}>Trigger Workflow</div>
        <div style={{ fontSize: 13, color: '#666', marginBottom: 16 }}>
          Pass context as JSON key-value pairs. Rules will be evaluated against these values.
        </div>
        {error && <div className="alert alert-error">{error}</div>}
        <div className="form-group">
          <label>Context JSON</label>
          <textarea
            value={contextJson}
            onChange={e => setContextJson(e.target.value)}
            style={{ fontFamily: 'monospace', fontSize: 13 }}
            rows={3}
          />
        </div>
        <button className="btn btn-primary" onClick={trigger} disabled={triggering}>
          {triggering ? 'Running...' : '▶ Trigger Now'}
        </button>

        {result && (
          <div className="trigger-result">
            <h4>
              Execution result &nbsp;
              <span className={`badge ${STATUS_BADGE[result.status]}`}>
                {STATUS_LABELS[result.status]}
              </span>
            </h4>
            <div style={{ fontSize: 12, color: '#888', marginBottom: 14 }}>
              Started {new Date(result.startedAt).toLocaleTimeString()}
              {result.completedAt && ` · Completed ${new Date(result.completedAt).toLocaleTimeString()}`}
            </div>
            {result.stepLogs.map((log, i) => (
              <div className="log-row" key={i}>
                <div className="log-step-name">{log.stepName}</div>
                <span className={`badge ${LOG_BADGE[log.status]}`} style={{ flexShrink: 0 }}>
                  {LOG_LABELS[log.status]}
                </span>
                <div className="log-msg">{log.message}</div>
                <div className="log-time">{new Date(log.executedAt).toLocaleTimeString()}</div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}