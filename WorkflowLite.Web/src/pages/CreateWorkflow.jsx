import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { createWorkflow } from '../api/workflows';

const STEP_TYPES = [
  { value: 1, label: 'Assign to User' },
  { value: 2, label: 'Send Email' },
  { value: 3, label: 'Mark Complete' },
  { value: 4, label: 'Approval' },
];

const OPERATORS = [
  { value: 1, label: 'Equals' },
  { value: 2, label: 'Not Equals' },
  { value: 3, label: 'Greater Than' },
  { value: 4, label: 'Less Than' },
  { value: 5, label: 'Contains' },
];

const emptyRule = () => ({ field: '', operator: 1, value: '' });
const emptyStep = (order) => ({
  name: '', description: '', type: 1, order,
  configJson: '{}', rules: [],
});

export default function CreateWorkflow() {
  const nav = useNavigate();
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [steps, setSteps] = useState([emptyStep(1)]);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const updateStep = (i, field, val) => {
    const s = [...steps];
    s[i] = { ...s[i], [field]: val };
    setSteps(s);
  };

  const addStep = () => setSteps([...steps, emptyStep(steps.length + 1)]);
  const removeStep = (i) => setSteps(steps.filter((_, idx) => idx !== i).map((s, idx) => ({ ...s, order: idx + 1 })));

  const addRule = (i) => {
    const s = [...steps];
    s[i].rules = [...s[i].rules, emptyRule()];
    setSteps(s);
  };

  const updateRule = (si, ri, field, val) => {
    const s = [...steps];
    s[si].rules[ri] = { ...s[si].rules[ri], [field]: val };
    setSteps(s);
  };

  const removeRule = (si, ri) => {
    const s = [...steps];
    s[si].rules = s[si].rules.filter((_, idx) => idx !== ri);
    setSteps(s);
  };

  const submit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await createWorkflow({ name, description, steps });
      nav('/');
    } catch (err) {
      setError(err.response?.data?.error || 'Failed to create workflow.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Create Workflow</h1>
          <p>Define steps and rules for your automation</p>
        </div>
        <button className="btn btn-ghost" onClick={() => nav('/')}>Cancel</button>
      </div>

      {error && <div className="alert alert-error">{error}</div>}

      <form onSubmit={submit}>
        <div className="card">
          <div className="card-title" style={{ marginBottom: 18 }}>Workflow details</div>
          <div className="form-group">
            <label>Name</label>
            <input value={name} onChange={e => setName(e.target.value)} required placeholder="e.g. Employee Onboarding" />
          </div>
          <div className="form-group">
            <label>Description</label>
            <textarea value={description} onChange={e => setDescription(e.target.value)} placeholder="What does this workflow do?" />
          </div>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', margin: '24px 0 14px' }}>
          <div style={{ fontWeight: 600, fontSize: 16 }}>Steps</div>
          <button type="button" className="btn btn-ghost" onClick={addStep}>+ Add Step</button>
        </div>

        {steps.map((step, si) => (
          <div className="step-block" key={si}>
            <div className="step-block-header">
              <span className="step-num">Step {si + 1}</span>
              {steps.length > 1 && (
                <button type="button" className="btn btn-danger" style={{ padding: '4px 12px', fontSize: 12 }} onClick={() => removeStep(si)}>
                  Remove
                </button>
              )}
            </div>

            <div className="form-row">
              <div className="form-group">
                <label>Step name</label>
                <input value={step.name} onChange={e => updateStep(si, 'name', e.target.value)} required placeholder="e.g. Send approval email" />
              </div>
              <div className="form-group">
                <label>Type</label>
                <select value={step.type} onChange={e => updateStep(si, 'type', Number(e.target.value))}>
                  {STEP_TYPES.map(t => <option key={t.value} value={t.value}>{t.label}</option>)}
                </select>
              </div>
            </div>

            <div className="form-group">
              <label>Description</label>
              <input value={step.description} onChange={e => updateStep(si, 'description', e.target.value)} placeholder="Optional" />
            </div>

            {step.rules.length > 0 && (
              <div style={{ marginBottom: 12 }}>
                <div style={{ fontSize: 13, fontWeight: 500, color: '#555', marginBottom: 8 }}>Rules (all must pass)</div>
                {step.rules.map((rule, ri) => (
                  <div className="rule-row" key={ri}>
                    <input placeholder="Field e.g. status" value={rule.field} onChange={e => updateRule(si, ri, 'field', e.target.value)} required />
                    <select value={rule.operator} onChange={e => updateRule(si, ri, 'operator', Number(e.target.value))}>
                      {OPERATORS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
                    </select>
                    <input placeholder="Value e.g. approved" value={rule.value} onChange={e => updateRule(si, ri, 'value', e.target.value)} required />
                    <button type="button" className="btn btn-danger" style={{ padding: '6px 10px' }} onClick={() => removeRule(si, ri)}>✕</button>
                  </div>
                ))}
              </div>
            )}

            <button type="button" className="btn btn-ghost" style={{ fontSize: 12, padding: '5px 12px' }} onClick={() => addRule(si)}>
              + Add Rule
            </button>
          </div>
        ))}

        <div className="btn-row" style={{ marginTop: 24 }}>
          <button className="btn btn-primary" disabled={loading}>
            {loading ? 'Creating...' : 'Create Workflow'}
          </button>
        </div>
      </form>
    </div>
  );
}