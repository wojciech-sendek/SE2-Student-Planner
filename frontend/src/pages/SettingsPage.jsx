import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

function SettingsPage() {
    const [isDeleting, setIsDeleting] = useState(false);
    const [error, setError] = useState(null);
    const navigate = useNavigate();

    const handleDeleteAccount = async () => {
        if (!window.confirm("Are you sure you want to delete your account? This action cannot be undone.")) {
            return;
        }

        setIsDeleting(true);
        setError(null);

        try {
            const token = localStorage.getItem('token');
            const response = await fetch('/api/Auth/delete-account', {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (response.ok) {
                localStorage.removeItem('token');
                alert("Account deleted successfully.");
                navigate('/login');
            } else {
                const data = await response.json();
                setError(data.message || "Failed to delete account.");
            }
        } catch (err) {
            setError("An error occurred. Please try again later.");
        } finally {
            setIsDeleting(false);
        }
    };

    return (
        <div style={{ padding: '20px', maxWidth: '600px', margin: '0 auto' }}>
            <h1>User Settings</h1>
            <div style={{ border: '1px solid #ccc', padding: '20px', borderRadius: '8px', marginTop: '20px' }}>
                <h2 style={{ color: 'red' }}>Danger Zone</h2>
                <p>Once you delete your account, there is no going back. Please be certain.</p>
                {error && <p style={{ color: 'red' }}>{error}</p>}
                <button
                    onClick={handleDeleteAccount}
                    disabled={isDeleting}
                    style={{
                        backgroundColor: '#ff4d4f',
                        color: 'white',
                        border: 'none',
                        padding: '10px 20px',
                        borderRadius: '4px',
                        cursor: isDeleting ? 'not-allowed' : 'pointer'
                    }}
                >
                    {isDeleting ? 'Deleting...' : 'Delete My Account'}
                </button>
            </div>
        </div>
    );
}

export default SettingsPage;
