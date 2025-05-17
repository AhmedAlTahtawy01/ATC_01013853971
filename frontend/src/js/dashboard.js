document.addEventListener('DOMContentLoaded', function() {
    // Check if user is logged in
    const token = localStorage.getItem('token');
    const user = JSON.parse(localStorage.getItem('user'));

    if (!token || !user) {
        window.location.href = '/login.html';
        return;
    }

    // Update user information
    const userNameElement = document.getElementById('userName');
    const userRoleElement = document.getElementById('userRole');
    
    userNameElement.textContent = user.username || 'User';
    userRoleElement.textContent = user.roleId === 1 ? 'Admin' : 'User';

    // Update date and time
    function updateDateTime() {
        const dateTimeElement = document.getElementById('dateTime');
        const now = new Date();
        dateTimeElement.textContent = now.toLocaleString();
    }
    updateDateTime();
    setInterval(updateDateTime, 1000);

    // Handle logout
    const logoutBtn = document.getElementById('logoutBtn');
    logoutBtn.addEventListener('click', function() {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        window.location.href = '/login.html';
    });

    // Add click event listeners to navigation links
    const navLinks = document.querySelectorAll('.nav-links a');
    navLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            // Remove active class from all links
            navLinks.forEach(l => l.parentElement.classList.remove('active'));
            // Add active class to clicked link
            this.parentElement.classList.add('active');
        });
    });
}); 