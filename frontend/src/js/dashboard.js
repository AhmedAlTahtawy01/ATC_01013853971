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

    // Function to fetch total events count
    async function fetchTotalEvents() {
        try {
            const response = await fetch('https://ahmedhamdy-areeb-api.runasp.net/api/event/count', {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Accept': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Failed to fetch event count');
            }

            const data = await response.json();
            const totalEventsElement = document.querySelector('.stat-card:nth-child(1) .stat-number');
            totalEventsElement.textContent = data.count;
        } catch (error) {
            console.error('Error fetching total events:', error);
            const totalEventsElement = document.querySelector('.stat-card:nth-child(1) .stat-number');
            totalEventsElement.textContent = 'Error';
        }
    }

    // Function to fetch total users count
    async function fetchTotalUsers() {
        try {
            const response = await fetch('https://ahmedhamdy-areeb-api.runasp.net/api/user/count', {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Accept': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Failed to fetch user count');
            }

            const data = await response.json();
            const totalUsersElement = document.querySelector('.stat-card:nth-child(2) .stat-number');
            totalUsersElement.textContent = data.count;
        } catch (error) {
            console.error('Error fetching total users:', error);
            const totalUsersElement = document.querySelector('.stat-card:nth-child(2) .stat-number');
            totalUsersElement.textContent = 'Error';
        }
    }

    // Function to fetch total bookings count
    async function fetchTotalBookings() {
        try {
            const response = await fetch('https://ahmedhamdy-areeb-api.runasp.net/api/booking/count', {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Accept': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Failed to fetch booking count');
            }

            const data = await response.json();
            const totalBookingsElement = document.querySelector('.stat-card:nth-child(3) .stat-number');
            totalBookingsElement.textContent = data.count;
        } catch (error) {
            console.error('Error fetching total bookings:', error);
            const totalBookingsElement = document.querySelector('.stat-card:nth-child(3) .stat-number');
            totalBookingsElement.textContent = 'Error';
        }
    }

    // Add click event listeners to navigation links
    const navLinks = document.querySelectorAll('.nav-links a');
    navLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            // Remove active class from all links
            navLinks.forEach(l => l.parentElement.classList.remove('active'));
            // Add active class to clicked link
            this.parentElement.classList.add('active');
        });
    });

    // Call fetch functions when dashboard loads
    fetchTotalEvents();
    fetchTotalUsers();
    fetchTotalBookings();

    // Update date and time
    function updateDateTime() {
        const dateTimeElement = document.getElementById('dateTime');
        const now = new Date();
        dateTimeElement.textContent = now.toLocaleString();
    }
    updateDateTime();
    setInterval(updateDateTime, 1000);

    // Handle logout
    document.getElementById('logoutBtn').addEventListener('click', function() {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        window.location.href = 'http://127.0.0.1:5500/frontend/public/login.html';
    });
}); 