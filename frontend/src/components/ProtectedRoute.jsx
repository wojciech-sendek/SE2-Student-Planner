import { Navigate } from 'react-router-dom'
import { getToken } from '../lib/authStorage.js'

export default function ProtectedRoute({ children }) {
  if (!getToken()) {
    return <Navigate to="/login" replace />
  }
  return children
}
