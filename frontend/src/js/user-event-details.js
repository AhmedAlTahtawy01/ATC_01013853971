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

    // Get event ID from URL
    const urlParams = new URLSearchParams(window.location.search);
    const eventId = urlParams.get('id');

    if (!eventId) {
        window.location.href = 'user-events.html';
        return;
    }

    // DOM Elements
    const eventImage = document.getElementById('eventImage');
    const eventTitle = document.getElementById('eventTitle');
    const eventDate = document.getElementById('eventDate');
    const eventTime = document.getElementById('eventTime');
    const eventLocation = document.getElementById('eventLocation');
    const eventCategory = document.getElementById('eventCategory');
    const eventPrice = document.getElementById('eventPrice');
    const eventDescription = document.getElementById('eventDescription');
    const locationDetails = document.getElementById('locationDetails');
    const bookNowBtn = document.getElementById('bookNowBtn');
    const bookingModal = document.getElementById('bookingModal');
    const successModal = document.getElementById('successModal');
    const closeModal = document.querySelector('.close');
    const bookingForm = document.getElementById('bookingForm');
    const logoutBtn = document.getElementById('logoutBtn');

    // Modal elements
    const modalEventName = document.getElementById('modalEventName');
    const modalEventDate = document.getElementById('modalEventDate');
    const modalEventPrice = document.getElementById('modalEventPrice');

    let currentEvent = null;
    let isEventBooked = false;

    // Load event details and check booking status
    loadEventDetails();
    checkBookingStatus();

    // Event Listeners
    bookNowBtn.addEventListener('click', openBookingModal);
    closeModal.addEventListener('click', closeBookingModal);
    bookingForm.addEventListener('submit', handleBooking);
    logoutBtn.addEventListener('click', utils.auth.logout);

    // Close modal when clicking outside
    window.addEventListener('click', (e) => {
        if (e.target === bookingModal) {
            closeBookingModal();
        }
    });

    // Functions
    async function checkBookingStatus() {
        try {
            const currentUser = utils.auth.getCurrentUser();
            if (!currentUser || !currentUser.id) {
                throw new Error('User information not available');
            }

            const response = await utils.api.fetch(utils.api.endpoints.userBookings(currentUser.id));
            
            if (response && response.bookings) {
                isEventBooked = response.bookings.some(booking => booking.eventId === parseInt(eventId));
                updateBookButtonState();
            }
        } catch (error) {
            console.error('Error checking booking status:', error);
        }
    }

    function updateBookButtonState() {
        if (!currentEvent) return;

        if (!currentEvent.isActive) {
            bookNowBtn.textContent = 'Event Not Available';
            bookNowBtn.disabled = true;
            bookNowBtn.style.backgroundColor = '#999';
        } else if (isEventBooked) {
            bookNowBtn.textContent = 'Booked';
            bookNowBtn.disabled = true;
            bookNowBtn.style.backgroundColor = '#4CAF50';
        } else {
            bookNowBtn.textContent = 'Book Now';
            bookNowBtn.disabled = false;
            bookNowBtn.style.backgroundColor = '#007bff';
        }
    }

    async function loadEventDetails() {
        try {
            const response = await utils.api.fetch(utils.api.endpoints.eventDetails(eventId));
            console.log('API Response:', response); // Debug log

            if (!response || !response.eventData) {
                throw new Error('Invalid response format from API');
            }

            currentEvent = response.eventData;
            displayEventDetails(currentEvent);
            initializeMap(currentEvent.venue);
            updateBookButtonState();
        } catch (error) {
            console.error('Error loading event details:', error);
            const eventDetailsContainer = document.querySelector('.event-details-container');
            if (eventDetailsContainer) {
                eventDetailsContainer.innerHTML = `
                    <div class="error-message">
                        <i class="fas fa-exclamation-circle"></i>
                        <p>Failed to load event details. Please try again later.</p>
                    </div>
                `;
            }
        }
    }

    function displayEventDetails(event) {
        eventImage.src = event.imageUrl || 'https://via.placeholder.com/800x400?text=Event+Image';
        eventImage.alt = event.name;
        eventTitle.textContent = event.name;
        
        eventDate.textContent = utils.dateUtils.formatDate(event.date);
        eventTime.textContent = utils.dateUtils.formatTime(event.date);
        
        eventLocation.textContent = event.venue || 'Venue not specified';
        eventCategory.textContent = event.category || 'Uncategorized';
        eventPrice.textContent = event.price ? `$${event.price.toFixed(2)}` : 'Free';
        eventDescription.textContent = event.description || 'No description available.';

        // Update location details
        locationDetails.textContent = event.venue || 'Location details not available.';
    }

    function initializeMap(location) {
        const mapContainer = document.getElementById('map');
        mapContainer.innerHTML = `
            <div class="map-placeholder">
                <i class="fas fa-map-marker-alt"></i>
                <p>Map for ${location || 'Event Location'}</p>
            </div>
        `;
    }

    function openBookingModal() {
        if (!currentEvent.isActive || isEventBooked) {
            return;
        }

        // Update modal content
        modalEventName.textContent = currentEvent.name;
        modalEventDate.textContent = `${utils.dateUtils.formatDate(currentEvent.date)} at ${utils.dateUtils.formatTime(currentEvent.date)}`;
        modalEventPrice.textContent = currentEvent.price ? `$${currentEvent.price.toFixed(2)}` : 'Free';

        bookingModal.style.display = 'block';
    }

    function closeBookingModal() {
        bookingModal.style.display = 'none';
        bookingForm.reset();
    }

    function showSuccessModal() {
        bookingModal.style.display = 'none';
        successModal.style.display = 'block';
    }

    async function handleBooking(e) {
        e.preventDefault();

        try {
            // Get current user ID from auth
            const currentUser = utils.auth.getCurrentUser();
            if (!currentUser || !currentUser.id) {
                throw new Error('User information not available');
            }

            // Prepare booking data according to the Booking model
            const bookingData = {
                userId: currentUser.id,
                eventId: currentEvent.id
            };

            // Show loading state
            const submitButton = bookingForm.querySelector('button[type="submit"]');
            const originalButtonText = submitButton.textContent;
            submitButton.disabled = true;
            submitButton.textContent = 'Processing...';

            // Make API call
            const response = await utils.api.fetch(utils.api.endpoints.bookings, {
                method: 'POST',
                body: JSON.stringify(bookingData)
            });

            // Handle successful booking
            if (response && response.bookingId) {
                // Update booking status
                isEventBooked = true;
                updateBookButtonState();

                // Show success modal
                showSuccessModal();
            } else {
                throw new Error('Invalid response from server');
            }
        } catch (error) {
            console.error('Error creating booking:', error);
            
            // Show error message
            const errorMessage = document.createElement('div');
            errorMessage.className = 'error-message';
            errorMessage.innerHTML = `
                <i class="fas fa-exclamation-circle"></i>
                <p>${error.message || 'Failed to create booking. Please try again later.'}</p>
            `;
            bookingForm.insertBefore(errorMessage, bookingForm.firstChild);

            // Reset button state
            const submitButton = bookingForm.querySelector('button[type="submit"]');
            submitButton.disabled = false;
            submitButton.textContent = 'Confirm Booking';
        }
    }
}); 