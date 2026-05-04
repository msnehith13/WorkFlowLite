import client from './client';

export const getWorkflows = () => client.get('/workflows');
export const getWorkflow = (id) => client.get(`/workflows/${id}`);
export const createWorkflow = (data) => client.post('/workflows', data);
export const updateWorkflow = (id, data) => client.put(`/workflows/${id}`, data);
export const deleteWorkflow = (id) => client.delete(`/workflows/${id}`);
export const triggerWorkflow = (id, contextJson) =>
  client.post(`/workflows/${id}/trigger`, { contextJson });