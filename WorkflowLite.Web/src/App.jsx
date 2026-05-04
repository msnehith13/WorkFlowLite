import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/Login';
import Register from './pages/Register';
import Dashboard from './pages/Dashboard';
import CreateWorkflow from './pages/CreateWorkflow';
import WorkflowDetail from './pages/WorkflowDetail';
import Layout from './components/Layout';
import ProtectedRoute from './components/ProtectedRoute';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/" element={
          <ProtectedRoute>
            <Layout><Dashboard /></Layout>
          </ProtectedRoute>
        } />
        <Route path="/workflows/create" element={
          <ProtectedRoute>
            <Layout><CreateWorkflow /></Layout>
          </ProtectedRoute>
        } />
        <Route path="/workflows/:id" element={
          <ProtectedRoute>
            <Layout><WorkflowDetail /></Layout>
          </ProtectedRoute>
        } />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;