document.addEventListener('DOMContentLoaded', function() {
    const loginForm = document.getElementById('loginForm');
    const usernameInput = document.getElementById('username');
    const passwordInput = document.getElementById('password');
    const usernameError = document.getElementById('username-error');
    const passwordError = document.getElementById('password-error');

    // Function to validate username
    function validateUsername() {
        const username = usernameInput.value.trim();
        if (!username) {
            utils.uiUtils.showError(usernameError, 'Username is required');
            return false;
        }
        utils.uiUtils.hideError(usernameError);
        return true;
    }

    // Function to validate password
    function validatePassword() {
        const password = passwordInput.value.trim();
        if (!password) {
            utils.uiUtils.showError(passwordError, 'Password is required');
            return false;
        }
        if (password.length < 8) {
            utils.uiUtils.showError(passwordError, 'Password must be at least 8 characters long');
            return false;
        }
        utils.uiUtils.hideError(passwordError);
        return true;
    }

    // Function to validate all fields
    function validateAll() {
        // Hide all errors first
        utils.uiUtils.hideError(usernameError);
        utils.uiUtils.hideError(passwordError);

        // Validate all fields
        const isUsernameValid = validateUsername();
        const isPasswordValid = validatePassword();

        return isUsernameValid && isPasswordValid;
    }

    // Function to handle login API call
    async function handleLogin(username, password) {
        try {
            const data = await utils.api.fetch(utils.api.endpoints.login, {
                method: 'POST',
                body: JSON.stringify({
                    username: username,
                    password: password
                })
            });
            
            if (!data.token) {
                throw new Error('Invalid response from server');
            }

            // Store the token and user data in localStorage
            localStorage.setItem('token', data.token);
            localStorage.setItem('userData', JSON.stringify(data.user));

            // Redirect based on user role
            if (data.user.roleId === 1) {
                // Admin user - redirect to dashboard
                window.location.href = 'dashboard.html';
            } else if (data.user.roleId === 2) {
                // Normal user - redirect to user home
                window.location.href = 'user-home.html';
            } else {
                throw new Error('Invalid user role');
            }
        } catch (error) {
            console.error('Login error:', error);
            if (error.message.includes('Failed to fetch')) {
                utils.uiUtils.showError(usernameError, 'Unable to connect to the server. Please try again later.');
            } else {
                utils.uiUtils.showError(usernameError, error.message || 'Invalid username or password');
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