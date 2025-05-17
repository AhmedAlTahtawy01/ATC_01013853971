document.addEventListener('DOMContentLoaded', function() {
    // Check if user is logged in
    const token = localStorage.getItem('token');
    const userData = JSON.parse(localStorage.getItem('userData'));

    if (!token || !userData) {
        window.location.href = 'login.html';
        return;
    }

    // Update user information
    const userNameElement = document.getElementById('userName');
    const userRoleElement = document.getElementById('userRole');
    
    userNameElement.textContent = userData.username || 'User';
    userRoleElement.textContent = userData.roleId === 1 ? 'Admin' : 'User';

    // Modal elements
    const bookingDetailsModal = document.getElementById('bookingDetailsModal');
    const closeBtn = bookingDetailsModal.querySelector('.close');
    const cancelBtn = bookingDetailsModal.querySelector('.cancel-btn');

    // Close modal
    function closeModal() {
        bookingDetailsModal.style.display = "none";
    }

    closeBtn.onclick = closeModal;
    cancelBtn.onclick = closeModal;

    // Close modal when clicking outside
    window.onclick = function(event) {
        if (event.target == bookingDetailsModal) {
            closeModal();
        }
    }

    // Pagination state
    let currentPage = 1;
    const pageSize = 10;

    // Function to format date
    function formatDate(dateString) {
        try {
            if (!dateString) return '-';
            const date = new Date(dateString);
            if (isNaN(date.getTime())) return '-';
            return date.toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'short',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        } catch (error) {
            console.error('Error formatting date:', error);
            return '-';
        }
    }

    // Function to fetch bookings
    async function fetchBookings(page = 1) {
        try {
            const response = await fetch(`https://ahmedhamdy-areeb-api.runasp.net/api/booking?pageNumber=${page}&pageSize=${pageSize}`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Accept': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Failed to fetch bookings');
            }

            const data = await response.json();
            return data;
        } catch (error) {
            console.error('Error fetching bookings:', error);
            throw error;
        }
    }

    // Function to render bookings
    function renderBookings(bookings) {
        const bookingsGrid = document.getElementById('bookingsTableBody');
        bookingsGrid.innerHTML = '';

        if (!bookings || bookings.length === 0) {
            bookingsGrid.innerHTML = '<p class="no-bookings">No bookings found</p>';
            return;
        }

        bookings.forEach(booking => {
            // Ensure booking has required properties with defaults
            const bookingData = {
                id: booking.id || 0,
                userId: booking.userId || 0,
                eventId: booking.eventId || 0,
                bookedAt: booking.bookedAt || new Date().toISOString()
            };

            const card = document.createElement('div');
            card.className = 'booking-card';
            card.onclick = () => showBookingDetails(bookingData);
            card.innerHTML = `
                <div class="booking-header">
                    <h3 class="event-name">Event ID: ${bookingData.eventId}</h3>
                    <span class="booking-status">
                        Booked
                    </span>
                </div>
                <div class="booking-details">
                    <div class="booking-detail">
                        <i class="fas fa-user"></i>
                        <span>User ID: ${bookingData.userId}</span>
                    </div>
                    <div class="booking-detail">
                        <i class="fas fa-calendar-alt"></i>
                        <span>Booked: ${formatDate(bookingData.bookedAt)}</span>
                    </div>
                    <div class="booking-detail">
                        <i class="fas fa-hashtag"></i>
                        <span>Booking ID: ${bookingData.id}</span>
                    </div>
                </div>
            `;
            bookingsGrid.appendChild(card);
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
                loadBookings();
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
                loadBookings();
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
                loadBookings();
            }
        };
        pagination.appendChild(nextButton);
    }

    // Function to load bookings
    async function loadBookings() {
        try {
            const data = await fetchBookings(currentPage);
            if (!data || !data.bookings) {
                throw new Error('Invalid response format');
            }
            renderBookings(data.bookings);
            renderPagination(Math.ceil(data.totalCount / pageSize));
        } catch (error) {
            console.error('Error loading bookings:', error);
            const bookingsGrid = document.getElementById('bookingsTableBody');
            bookingsGrid.innerHTML = '<p class="error-message">Failed to load bookings. Please try again.</p>';
        }
    }

    // Function to show booking details
    function showBookingDetails(booking) {
        // Ensure booking has required properties with defaults
        const bookingData = {
            id: booking.id || 0,
            userId: booking.userId || 0,
            eventId: booking.eventId || 0,
            bookedAt: booking.bookedAt || new Date().toISOString()
        };

        document.getElementById('modalEventName').textContent = `Event ID: ${bookingData.eventId}`;
        document.getElementById('modalUserName').textContent = `User ID: ${bookingData.userId}`;
        document.getElementById('modalBookingDate').textContent = formatDate(bookingData.bookedAt);
        document.getElementById('modalStatus').textContent = 'Booked';
        document.getElementById('modalPrice').textContent = `Booking ID: ${bookingData.id}`;

        bookingDetailsModal.style.display = "block";
    }

    // Handle logout
    document.getElementById('logoutBtn').addEventListener('click', function() {
        localStorage.removeItem('token');
        localStorage.removeItem('userData');
        window.location.href = 'login.html';
    });

    // Update date and time
    function updateDateTime() {
        const dateTimeElement = document.getElementById('dateTime');
        const now = new Date();
        dateTimeElement.textContent = now.toLocaleString();
    }
    updateDateTime();
    setInterval(updateDateTime, 1000);

    // Initial load of bookings
    loadBookings();
}); 