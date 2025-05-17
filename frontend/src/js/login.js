document.addEventListener('DOMContentLoaded', function() {
    const loginForm = document.getElementById('loginForm');
    const usernameInput = document.getElementById('username');
    const passwordInput = document.getElementById('password');
    const usernameError = document.getElementById('username-error');
    const passwordError = document.getElementById('password-error');

    // Function to show error message
    function showError(element, message) {
        if (element) {
            element.textContent = message;
            element.classList.add('show');
            if (element.previousElementSibling) {
                element.previousElementSibling.classList.add('error');
            }
        }
    }

    // Function to hide error message
    function hideError(element) {
        if (element) {
            element.textContent = '';
            element.classList.remove('show');
            if (element.previousElementSibling) {
                element.previousElementSibling.classList.remove('error');
            }
        }
    }

    // Function to validate username
    function validateUsername() {
        const username = usernameInput.value.trim();
        if (!username) {
            showError(usernameError, 'Username is required');
            return false;
        }
        hideError(usernameError);
        return true;
    }

    // Function to validate password
    function validatePassword() {
        const password = passwordInput.value.trim();
        if (!password) {
            showError(passwordError, 'Password is required');
            return false;
        }
        if (password.length < 8) {
            showError(passwordError, 'Password must be at least 8 characters long');
            return false;
        }
        hideError(passwordError);
        return true;
    }

    // Function to validate all fields
    function validateAll() {
        // Hide all errors first
        hideError(usernameError);
        hideError(passwordError);

        // Validate all fields
        const isUsernameValid = validateUsername();
        const isPasswordValid = validatePassword();

        return isUsernameValid && isPasswordValid;
    }

    // Function to handle login API call
    async function handleLogin(username, password) {
        try {
            const response = await fetch('https://localhost:7107/api/user/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify({
                    username: username,
                    password: password
                })
            });

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: 'Login failed' }));
                throw new Error(errorData.message || 'Login failed');
            }

            const data = await response.json();
            
            if (!data.token) {
                throw new Error('Invalid response from server');
            }

            // Store the token in localStorage
            localStorage.setItem('token', data.token);
            localStorage.setItem('user', JSON.stringify(data.user));

            // Redirect to dashboard or home page
            window.location.href = 'http://127.0.0.1:5500/frontend/public/dashboard.html';
        } catch (error) {
            console.error('Login error:', error);
            if (error.message.includes('Failed to fetch')) {
                showError(usernameError, 'Unable to connect to the server. Please try again later.');
            } else {
                showError(usernameError, error.message || 'Invalid username or password');
            }
        }
    }

    // Handle form submission
    loginForm.addEventListener('submit', async function(e) {
        e.preventDefault();
        
        if (validateAll()) {
            const username = usernameInput.value.trim();
            const password = passwordInput.value.trim();
            
            // Show loading state
            const loginButton = loginForm.querySelector('.login-button');
            const originalButtonText = loginButton.textContent;
            loginButton.textContent = 'Logging in...';
            loginButton.disabled = true;

            try {
                await handleLogin(username, password);
            } finally {
                // Reset button state
                loginButton.textContent = originalButtonText;
                loginButton.disabled = false;
            }
        }
    });
}); 