document.addEventListener('DOMContentLoaded', () => {
    // Check if user is logged in
    if (!utils.auth.requireAuth()) return;

    // Update user information
    const userNameElement = document.getElementById('userName');
    if (userNameElement) {
        userNameElement.textContent = utils.auth.getCurrentUser().username;
    }

    // DOM Elements
    const bookingsList = document.getElementById('bookingsList');
    const pagination = document.getElementById('pagination');
    const statusFilter = document.getElementById('statusFilter');
    const logoutBtn = document.getElementById('logoutBtn');

    // State variables
    let currentPage = 1;
    const bookingsPerPage = 5;
    let currentStatus = 'all';

    // Event Listeners
    statusFilter.addEventListener('change', handleStatusChange);
    logoutBtn.addEventListener('click', utils.auth.logout);

    // Initial load
    loadBookings();

    // Functions
    async function loadBookings() {
        try {
            const currentUser = utils.auth.getCurrentUser();
            if (!currentUser || !currentUser.id) {
                throw new Error('User information not available');
            }

            const response = await utils.api.fetch(utils.api.endpoints.userBookings(currentUser.id), {
                method: 'GET'
            });

            // Handle case where no bookings are found
            if (!response || !response.bookings || response.bookings.length === 0) {
                bookingsList.innerHTML = `
                    <div class="empty-state">
                        <i class="fas fa-calendar-times"></i>
                        <p>You haven't made any bookings yet.</p>
                        <a href="user-events.html" class="action-btn view-btn" style="display: inline-block; margin-top: 20px;">
                            Browse Events
                        </a>
                    </div>
                `;
                pagination.innerHTML = '';
                return;
            }

            // Filter bookings based on status
            let filteredBookings = response.bookings;
            if (currentStatus !== 'all') {
                const now = new Date();
                filteredBookings = response.bookings.filter(booking => {
                    const eventDate = new Date(booking.eventDate);
                    return currentStatus === 'upcoming' ? eventDate > now : eventDate < now;
                });
            }

            // Calculate pagination
            const totalPages = Math.ceil(filteredBookings.length / bookingsPerPage);
            const startIndex = (currentPage - 1) * bookingsPerPage;
            const endIndex = startIndex + bookingsPerPage;
            const paginatedBookings = filteredBookings.slice(startIndex, endIndex);

            // Display bookings
            displayBookings(paginatedBookings);
            renderPagination(totalPages);

        } catch (error) {
            console.error('Error loading bookings:', error);
            bookingsList.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-exclamation-circle"></i>
                    <p>Failed to load bookings. Please try again later.</p>
                </div>
            `;
            pagination.innerHTML = '';
        }
    }

    function displayBookings(bookings) {
        if (!bookings || bookings.length === 0) {
            bookingsList.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-calendar-times"></i>
                    <p>No bookings found.</p>
                </div>
            `;
            return;
        }

        bookingsList.innerHTML = bookings.map(booking => {
            return `
                <div class="booking-card">
                    <div class="booking-info">
                        <div class="booking-header">
                            <h3 class="booking-title">Booking #${booking.id}</h3>
                            <span class="booking-status status-upcoming">Active</span>
                        </div>
                        <div class="booking-details">
                            <div class="detail-item">
                                <i class="fas fa-calendar"></i>
                                <span>Booked on: ${utils.dateUtils.formatDate(booking.bookedAt)}</span>
                            </div>
                            <div class="detail-item">
                                <i class="fas fa-clock"></i>
                                <span>Time: ${utils.dateUtils.formatTime(booking.bookedAt)}</span>
                            </div>
                            <div class="detail-item">
                                <i class="fas fa-user"></i>
                                <span>User ID: ${booking.userId}</span>
                            </div>
                            <div class="detail-item">
                                <i class="fas fa-ticket-alt"></i>
                                <span>Event ID: ${booking.eventId}</span>
                            </div>
                        </div>
                    </div>
                    <div class="booking-actions">
                        <button class="action-btn view-btn" onclick="viewEventDetails(${booking.eventId})">
                            View Event
                        </button>
                        <button class="action-btn cancel-btn" onclick="cancelBooking(${booking.id})">
                            Cancel Booking
                        </button>
                    </div>
                </div>
            `;
        }).join('');
    }

    function renderPagination(totalPages) {
        if (totalPages <= 1) {
            pagination.innerHTML = '';
            return;
        }

        let paginationHTML = `
            <button class="page-btn" ${currentPage === 1 ? 'disabled' : ''} onclick="changePage(${currentPage - 1})">
                Previous
            </button>
        `;

        for (let i = 1; i <= totalPages; i++) {
            paginationHTML += `
                <button class="page-btn ${i === currentPage ? 'active' : ''}" onclick="changePage(${i})">
                    ${i}
                </button>
            `;
        }

        paginationHTML += `
            <button class="page-btn" ${currentPage === totalPages ? 'disabled' : ''} onclick="changePage(${currentPage + 1})">
                Next
            </button>
        `;

        pagination.innerHTML = paginationHTML;
    }

    function handleStatusChange(e) {
        currentStatus = e.target.value;
        currentPage = 1;
        loadBookings();
    }

    // Make functions available globally
    window.changePage = (page) => {
        currentPage = page;
        loadBookings();
    };

    window.viewEventDetails = (eventId) => {
        window.location.href = `user-event-details.html?id=${eventId}`;
    };

    window.cancelBooking = async (bookingId) => {
        if (!confirm('Are you sure you want to cancel this booking?')) {
            return;
        }

        try {
            const response = await utils.api.fetch(utils.api.endpoints.cancelBooking(bookingId), {
                method: 'DELETE'
            });

            if (response && response.message === "Booking deleted successfully") {
                alert('Booking cancelled successfully');
                loadBookings();
            } else {
                throw new Error(response?.message || 'Failed to cancel booking');
            }
        } catch (error) {
            console.error('Error cancelling booking:', error);
            alert(error.message || 'Failed to cancel booking. Please try again later.');
        }
    };
}); 