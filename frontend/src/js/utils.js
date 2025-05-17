// Authentication and User Management
const auth = {
    // Check if user is logged in
    isAuthenticated: () => {
        const token = localStorage.getItem('token');
        const userData = JSON.parse(localStorage.getItem('userData'));
        return !!(token && userData);
    },

    // Get current user data
    getCurrentUser: () => {
        return JSON.parse(localStorage.getItem('userData'));
    },

    // Get auth token
    getToken: () => {
        return localStorage.getItem('token');
    },

    // Check if user is admin
    isAdmin: () => {
        const userData = JSON.parse(localStorage.getItem('userData'));
        return userData?.roleId === 1;
    },

    // Handle logout
    logout: () => {
        localStorage.removeItem('token');
        localStorage.removeItem('userData');
        window.location.href = 'login.html';
    },

    // Redirect if not authenticated
    requireAuth: () => {
        if (!auth.isAuthenticated()) {
            window.location.href = 'login.html';
            return false;
        }
        return true;
    },

    // Redirect if not admin
    requireAdmin: () => {
        if (!auth.isAuthenticated() || !auth.isAdmin()) {
            window.location.href = 'user-home.html';
            return false;
        }
        return true;
    }
};

// API Calls
const api = {
    // Base URL for API calls
    baseUrl: 'https://ahmedhamdy-areeb-api.runasp.net/api',

    // Common fetch wrapper with auth header
    fetch: async (endpoint, options = {}) => {
        const token = auth.getToken();
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                ...(token ? { 'Authorization': `Bearer ${token}` } : {})
            }
        };

        try {
            const response = await fetch(`${api.baseUrl}${endpoint}`, {
                ...defaultOptions,
                ...options,
                headers: {
                    ...defaultOptions.headers,
                    ...options.headers
                }
            });

            // Handle 404 for bookings endpoint specifically
            if (response.status === 404 && endpoint.includes('/booking/user/')) {
                const data = await response.json();
                if (data.message === "No bookings found for the given User ID") {
                    return { bookings: [] };
                }
            }

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: 'Request failed' }));
                throw new Error(errorData.message || 'Request failed');
            }

            return await response.json();
        } catch (error) {
            // Don't log 404 errors for bookings endpoint
            if (!(error.message === "No bookings found for the given User ID" && endpoint.includes('/booking/user/'))) {
                console.error('API Error:', error);
            }
            throw error;
        }
    },

    // Common API endpoints
    endpoints: {
        login: '/user/login',
        register: '/user/register',
        events: '/event',
        eventDetails: (id) => `/event/${id}`,
        bookings: '/booking',
        userBookings: (userId) => `/booking/user/${userId}`,
        cancelBooking: (bookingId) => `/booking/${bookingId}`,
        users: '/user'
    }
};

// Date Formatting
const dateUtils = {
    formatDate: (dateString) => {
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', {
            weekday: 'long',
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    },

    formatTime: (dateString) => {
        const date = new Date(dateString);
        return date.toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit'
        });
    }
};

// UI Utilities
const uiUtils = {
    // Debounce function for search inputs
    debounce: (func, wait) => {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    },

    // Show error message
    showError: (element, message) => {
        if (element) {
            element.textContent = message;
            element.classList.add('show');
            if (element.previousElementSibling) {
                element.previousElementSibling.classList.add('error');
            }
        }
    },

    // Hide error message
    hideError: (element) => {
        if (element) {
            element.textContent = '';
            element.classList.remove('show');
            if (element.previousElementSibling) {
                element.previousElementSibling.classList.remove('error');
            }
        }
    }
};

// Export all utilities
window.utils = {
    auth,
    api,
    dateUtils,
    uiUtils
}; 