document.addEventListener('DOMContentLoaded', function() {
    // Check if user is logged in
    const token = localStorage.getItem('token');
    const user = JSON.parse(localStorage.getItem('user'));

    if (!token || !user) {
        window.location.href = 'http://127.0.0.1:5500/frontend/public/login.html';
        return;
    }

    // Update user information
    const userNameElement = document.getElementById('userName');
    const userRoleElement = document.getElementById('userRole');
    
    userNameElement.textContent = user.username || 'User';
    userRoleElement.textContent = user.roleId === 1 ? 'Admin' : 'User';

    // Modal elements
    const addModal = document.getElementById('addUserModal');
    const addUserBtn = document.getElementById('addUserBtn');
    const addCloseBtn = addModal.querySelector('.close');
    const addCancelBtn = addModal.querySelector('.cancel-btn');
    const addUserForm = document.getElementById('addUserForm');

    // Show add modal
    addUserBtn.onclick = function() {
        addModal.style.display = "block";
    }

    // Close modal
    function closeAddModal() {
        addModal.style.display = "none";
        addUserForm.reset();
    }

    addCloseBtn.onclick = closeAddModal;
    addCancelBtn.onclick = closeAddModal;

    // Close modal when clicking outside
    window.onclick = function(event) {
        if (event.target == addModal) {
            closeAddModal();
        }
    }

    // Pagination state
    let currentPage = 1;
    const pageSize = 10;

    // Function to fetch users
    async function fetchUsers(page = 1) {
        try {
            const response = await fetch(`https://ahmedhamdy-areeb-api.runasp.net/api/user?pageNumber=${page}&pageSize=${pageSize}`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Accept': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Failed to fetch users');
            }

            const data = await response.json();
            return data;
        } catch (error) {
            console.error('Error fetching users:', error);
            throw error;
        }
    }

    // Function to render users
    function renderUsers(users) {
        const usersGrid = document.getElementById('usersTableBody');
        usersGrid.innerHTML = '';

        users.forEach(user => {
            const card = document.createElement('div');
            card.className = 'user-card';
            card.innerHTML = `
                <div class="user-header">
                    <h3 class="user-name">${user.username}</h3>
                    <span class="user-role ${user.roleId === 1 ? 'admin' : 'user'}">
                        ${user.roleId === 1 ? 'Admin' : 'User'}
                    </span>
                </div>
                <div class="user-details">
                    <div class="user-detail">
                        <i class="fas fa-envelope"></i>
                        <span>${user.email}</span>
                    </div>
                </div>
                <div class="user-actions">
                    <button class="edit-btn" onclick="editUser(${user.id})">
                        <i class="fas fa-edit"></i> Edit
                    </button>
                    <button class="delete-btn" onclick="deleteUser(${user.id})">
                        <i class="fas fa-trash"></i> Delete
                    </button>
                </div>
            `;
            usersGrid.appendChild(card);
        });
    }

    // Function to render pagination
    function renderPagination(totalPages) {
        const pagination = document.getElementById('pagination');
        pagination.innerHTML = '';

        // Previous button
        const prevButton = document.createElement('button');
        prevButton.textContent = 'Previous';
        prevButton.disabled = currentPage === 1;
        prevButton.onclick = () => {
            if (currentPage > 1) {
                currentPage--;
                loadUsers();
            }
        };
        pagination.appendChild(prevButton);

        // Page numbers
        for (let i = 1; i <= totalPages; i++) {
            const pageButton = document.createElement('button');
            pageButton.textContent = i;
            pageButton.classList.toggle('active', i === currentPage);
            pageButton.onclick = () => {
                currentPage = i;
                loadUsers();
            };
            pagination.appendChild(pageButton);
        }

        // Next button
        const nextButton = document.createElement('button');
        nextButton.textContent = 'Next';
        nextButton.disabled = currentPage === totalPages;
        nextButton.onclick = () => {
            if (currentPage < totalPages) {
                currentPage++;
                loadUsers();
            }
        };
        pagination.appendChild(nextButton);
    }

    // Function to load users
    async function loadUsers() {
        try {
            const data = await fetchUsers(currentPage);
            renderUsers(data.users);
            renderPagination(Math.ceil(data.totalCount / pageSize));
        } catch (error) {
            console.error('Error loading users:', error);
            alert('Failed to load users. Please try again.');
        }
    }

    // Handle add user form submission
    addUserForm.addEventListener('submit', async function(e) {
        e.preventDefault();

        // Get form values
        const username = document.getElementById('username').value.trim();
        const name = document.getElementById('name').value.trim();
        const email = document.getElementById('email').value.trim();
        const password = document.getElementById('password').value;
        const roleId = parseInt(document.getElementById('roleId').value);

        // Validate form data
        const errors = [];

        // Username validation
        if (username.length === 0) {
            errors.push('Username is required');
        } else if (username.length > 50) {
            errors.push('Username must be 50 characters or less');
        }

        // Name validation
        if (name.length === 0) {
            errors.push('Name is required');
        } else if (name.length > 100) {
            errors.push('Name must be 100 characters or less');
        }

        // Email validation
        if (email.length === 0) {
            errors.push('Email is required');
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
            errors.push('Please enter a valid email address');
        }

        // Password validation
        if (password.length === 0) {
            errors.push('Password is required');
        } else if (password.length < 8) {
            errors.push('Password must be at least 8 characters long');
        }

        // Role validation
        if (!roleId) {
            errors.push('Please select a role');
        }

        // If there are validation errors, show them and return
        if (errors.length > 0) {
            alert(errors.join('\n'));
            return;
        }

        const formData = {
                username,
                name,
                email,
                passwordHash: password,
                roleId
            };

        try {
            const response = await fetch('https://ahmedhamdy-areeb-api.runasp.net/api/user/register', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(formData)
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Failed to create user');
            }

            alert('User created successfully!');
            closeAddModal();
            loadUsers(); // Reload the users list
        } catch (error) {
            console.error('Error creating user:', error);
            alert(error.message || 'Failed to create user. Please try again.');
        }
    });

    // Handle user deletion
    window.deleteUser = async function(userId) {
        if (!confirm('Are you sure you want to delete this user?')) {
            return;
        }

        try {
            const response = await fetch(`https://ahmedhamdy-areeb-api.runasp.net/api/user/${userId}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Accept': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Failed to delete user');
            }

            alert('User deleted successfully');
            loadUsers(); // Reload the users list
        } catch (error) {
            console.error('Error deleting user:', error);
            alert('Failed to delete user. Please try again.');
        }
    };

    // Handle user editing
    window.editUser = async function(userId) {
        // TODO: Implement edit functionality
        alert('Edit functionality coming soon!');
    };

    // Handle logout
    document.getElementById('logoutBtn').addEventListener('click', function() {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        window.location.href = 'http://127.0.0.1:5500/frontend/public/login.html';
    });

    // Update date and time
    function updateDateTime() {
        const dateTimeElement = document.getElementById('dateTime');
        const now = new Date();
        dateTimeElement.textContent = now.toLocaleString();
    }
    updateDateTime();
    setInterval(updateDateTime, 1000);

    // Initial load of users
    loadUsers();
}); 