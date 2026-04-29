import React from 'react'
import { Navigate } from 'react-router-dom'
import { getToken } from '../lib/authStorage.js'

export default function RootRedirect() {
  return <Navigate to={getToken() ? '/app' : '/login'} replace />
}
