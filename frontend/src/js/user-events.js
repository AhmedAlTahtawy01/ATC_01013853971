document.addEventListener('DOMContentLoaded', () => {
    // Check if user is logged in
    if (!utils.auth.requireAuth()) return;

    // Theme handling
    const themeToggle = document.getElementById('themeToggle');
    const themeIcon = themeToggle.querySelector('i');

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

    // Update user information
    const userNameElement = document.getElementById('userName');
    if (userNameElement) {
        userNameElement.textContent = utils.auth.getCurrentUser().username;
    }

    // DOM Elements
    const eventsList = document.getElementById('eventList');
    const pagination = document.getElementById('pagination');
    const categoryFilter = document.getElementById('categoryFilter');
    const searchInput = document.getElementById('searchInput');
    const logoutBtn = document.getElementById('logoutBtn');

    // State variables
    let currentPage = 1;
    const eventsPerPage = 6;
    let currentCategory = 'all';
    let searchQuery = '';
    let userBookings = [];

    // Event Listeners
    categoryFilter.addEventListener('change', handleCategoryChange);
    searchInput.addEventListener('input', utils.uiUtils.debounce(handleSearch, 300));
    logoutBtn.addEventListener('click', utils.auth.logout);

    // Initial load
    loadUserBookings();
    loadEvents();

    // Functions
    async function loadUserBookings() {
        try {
            const currentUser = utils.auth.getCurrentUser();
            if (!currentUser || !currentUser.id) {
                throw new Error('User information not available');
            }

            const response = await utils.api.fetch(utils.api.endpoints.userBookings(currentUser.id));
            if (response && response.bookings) {
                userBookings = response.bookings;
            }
        } catch (error) {
            console.error('Error loading user bookings:', error);
            handleAuthError(error);
        }
    }

    function isEventBooked(eventId) {
        return userBookings.some(booking => booking.eventId === eventId);
    }

    async function loadEvents() {
        try {
            if (!eventsList) {
                throw new Error('Events list container not found');
            }

            const response = await utils.api.fetch(utils.api.endpoints.events, {
                method: 'GET',
                params: {
                    pageNumber: currentPage,
                    pageSize: eventsPerPage,
                    category: currentCategory,
                    search: searchQuery
                }
            });

            if (!response || !response.events) {
                throw new Error('Invalid response format from API');
            }

            displayEvents(response.events);
            renderPagination(response.totalPages);

        } catch (error) {
            console.error('Error loading events:', error);
            handleAuthError(error);
        }
    }

    function handleAuthError(error) {
        // Check if the error is due to authentication issues
        if (error.message.includes('401') || error.message.includes('Unauthorized') || 
            error.message.includes('token') || error.message.includes('expired')) {
            
            // Show error message
            if (eventsList) {
                eventsList.innerHTML = `
                    <div class="error-message">
                        <i class="fas fa-exclamation-circle"></i>
                        <p>Your session has expired. Please log in again.</p>
                        <button onclick="window.location.href='login.html'" class="action-btn">
                            Go to Login
                        </button>
                    </div>
                `;
            }

            // Clear pagination
            if (pagination) {
                pagination.innerHTML = '';
            }

            // Clear user data and redirect after a delay
            setTimeout(() => {
                utils.auth.logout();
            }, 3000);
        } else {
            // Handle other errors
            if (eventsList) {
                eventsList.innerHTML = `
                    <div class="error-message">
                        <i class="fas fa-exclamation-circle"></i>
                        <p>Failed to load events. Please try again later.</p>
                    </div>
                `;
            }
            if (pagination) {
                pagination.innerHTML = '';
            }
        }
    }

    function displayEvents(events) {
        if (!eventsList) return;

        if (!events || events.length === 0) {
            eventsList.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-calendar-times"></i>
                    <p>No events found.</p>
                </div>
            `;
            return;
        }

        eventsList.innerHTML = events.map(event => {
            const isBooked = isEventBooked(event.id);
            return `
                <div class="event-card">
                    <div class="event-image">
                        <img src="${event.imageUrl || 'https://via.placeholder.com/300x200?text=Event+Image'}" alt="${event.name}">
                        ${isBooked ? '<span class="booked-label">Booked</span>' : ''}
                    </div>
                    <div class="event-info">
                        <h3 class="event-title">${event.name}</h3>
                        <div class="event-meta">
                            <p><i class="fas fa-calendar"></i> ${utils.dateUtils.formatDate(event.date)}</p>
                            <p><i class="fas fa-clock"></i> ${utils.dateUtils.formatTime(event.date)}</p>
                            <p><i class="fas fa-map-marker-alt"></i> ${event.venue || 'Venue not specified'}</p>
                        </div>
                        <div class="event-price">
                            <p>${event.price ? `$${event.price.toFixed(2)}` : 'Free'}</p>
                        </div>
                        ${isBooked ? 
                            `<button class="booked-btn" disabled>Booked</button>` :
                            `<button class="book-now-btn" onclick="viewEventDetails(${event.id})">Book Now</button>`
                        }
                    </div>
                </div>
            `;
        }).join('');
    }

    function renderPagination(totalPages) {
        if (!pagination) return;

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

    function handleCategoryChange(e) {
        currentCategory = e.target.value;
        currentPage = 1;
        loadEvents();
    }

    function handleSearch(e) {
        searchQuery = e.target.value;
        currentPage = 1;
        loadEvents();
    }

    // Make functions available globally
    window.changePage = (page) => {
        currentPage = page;
        loadEvents();
    };

    window.viewEventDetails = (eventId) => {
        window.location.href = `user-event-details.html?id=${eventId}`;
    };
}); 