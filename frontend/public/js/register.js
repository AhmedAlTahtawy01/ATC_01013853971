document.addEventListener('DOMContentLoaded', function() {
    const registerForm = document.getElementById('registerForm');
    const usernameInput = document.getElementById('username');
    const nameInput = document.getElementById('name');
    const emailInput = document.getElementById('email');
    const passwordInput = document.getElementById('password');
    const confirmPasswordInput = document.getElementById('confirmPassword');
    const toast = document.getElementById('notificationToast');
    const toastBody = toast.querySelector('.toast-body');
    const closeBtn = toast.querySelector('.close-btn');
    const themeToggle = document.getElementById('themeToggle');
    const themeIcon = themeToggle.querySelector('i');

    // Theme handling
    function initTheme() {
        const savedTheme = localStorage.getItem('theme') || 'light';
        document.documentElement.setAttribute('data-theme', savedTheme);
        updateThemeIcon(savedTheme);
    }

    function updateThemeIcon(theme) {
        themeIcon.className = theme === 'dark' ? 'fas fa-sun' : 'fas fa-moon';
    }

    themeToggle.addEventListener('click', () => {
        const currentTheme = document.documentElement.getAttribute('data-theme');
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        
        document.documentElement.setAttribute('data-theme', newTheme);
        localStorage.setItem('theme', newTheme);
        updateThemeIcon(newTheme);
    });

    // Initialize theme
    initTheme();

    // Show notification function
    function showNotification(message, isError = false) {
        toastBody.textContent = message;
        toastBody.className = 'toast-body ' + (isError ? 'text-danger' : 'text-success');
        toast.classList.add('show');
        
        // Auto hide after 3 seconds
        setTimeout(() => {
            toast.classList.remove('show');
        }, 3000);
    }

    // Close toast
    closeBtn.addEventListener('click', () => {
        toast.classList.remove('show');
    });

    // Function to get or create error element
    function getErrorElement(inputElement) {
        const errorId = inputElement.id + '-error';
        let errorElement = document.getElementById(errorId);
        
        if (!errorElement) {
            errorElement = document.createElement('span');
            errorElement.id = errorId;
            errorElement.className = 'error-message';
            inputElement.parentNode.appendChild(errorElement);
        }
        
        return errorElement;
    }

    // Function to show error message
    function showError(element, message) {
        const errorElement = getErrorElement(element);
        errorElement.textContent = message;
        errorElement.classList.add('show');
        element.classList.add('error');
    }

    // Function to hide error message
    function hideError(element) {
        const errorElement = getErrorElement(element);
        errorElement.classList.remove('show');
        element.classList.remove('error');
    }

    // Function to validate username
    function validateUsername() {
        const username = usernameInput.value.trim();
        if (!username) {
            showError(usernameInput, 'Username is required');
            return false;
        }
        if (username.length < 3) {
            showError(usernameInput, 'Username must be at least 3 characters long');
            return false;
        }
        hideError(usernameInput);
        return true;
    }

    // Function to validate name
    function validateName() {
        const name = nameInput.value.trim();
        if (!name) {
            showError(nameInput, 'Full name is required');
            return false;
        }
        hideError(nameInput);
        return true;
    }

    // Function to validate email
    function validateEmail() {
        const email = emailInput.value.trim();
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!email || !emailRegex.test(email)) {
            showError(emailInput, 'Please enter a valid email address');
            return false;
        }
        hideError(emailInput);
        return true;
    }

    // Function to validate password
    function validatePassword() {
        const password = passwordInput.value.trim();
        if (!password) {
            showError(passwordInput, 'Password is required');
            return false;
        }
        if (password.length < 8) {
            showError(passwordInput, 'Password must be at least 8 characters long');
            return false;
        }
        hideError(passwordInput);
        return true;
    }

    // Function to validate confirm password
    function validateConfirmPassword() {
        const password = passwordInput.value.trim();
        const confirmPassword = confirmPasswordInput.value.trim();
        if (password !== confirmPassword) {
            showError(confirmPasswordInput, 'Passwords do not match');
            return false;
        }
        hideError(confirmPasswordInput);
        return true;
    }

    // Function to validate all fields
    function validateAll() {
        const isUsernameValid = validateUsername();
        const isNameValid = validateName();
        const isEmailValid = validateEmail();
        const isPasswordValid = validatePassword();
        const isConfirmPasswordValid = validateConfirmPassword();

        return isUsernameValid && isNameValid && isEmailValid && 
               isPasswordValid && isConfirmPasswordValid;
    }

    // Function to handle registration API call
    async function handleRegistration(formData) {
        try {
            const data = await utils.api.fetch(utils.api.endpoints.register, {
                method: 'POST',
                body: JSON.stringify(formData)
            });
            
            showNotification('Registration successful! Redirecting to login...');
            localStorage.setItem('userId', data.userId);
            setTimeout(() => {
                window.location.href = 'login.html';
            }, 2000);
        } catch (error) {
            console.error('Registration error:', error);
            showNotification(error.message || 'Registration failed. Please try again.', true);
        }
    }

    // Handle form submission
    registerForm.addEventListener('submit', async function(e) {
        e.preventDefault();
        
        if (validateAll()) {
            const formData = {
                username: usernameInput.value.trim(),
                name: nameInput.value.trim(),
                email: emailInput.value.trim(),
                passwordHash: passwordInput.value.trim(),
                roleId: 2 // Default role for regular users
            };
            
            // Show loading state
            const registerButton = registerForm.querySelector('.register-button');
            const originalButtonText = registerButton.textContent;
            registerButton.textContent = 'Registering...';
            registerButton.disabled = true;

            try {
                await handleRegistration(formData);
            } finally {
                // Reset button state
                registerButton.textContent = originalButtonText;
                registerButton.disabled = false;
            }
        }
    });

    // Real-time validation for confirm password
    confirmPasswordInput.addEventListener('input', validateConfirmPassword);
}); 