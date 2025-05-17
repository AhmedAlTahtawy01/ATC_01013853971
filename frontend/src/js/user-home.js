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

    // Initialize logout button
    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', utils.auth.logout);
    }

    // Load featured events
    loadFeaturedEvents();

    async function loadFeaturedEvents() {
        try {
            const response = await utils.api.fetch(`${utils.api.endpoints.events}?pageNumber=1&pageSize=3`);
            console.log('API Response:', response); // Debug log

            // Check if response has the expected structure
            if (!response || !response.events || !Array.isArray(response.events)) {
                throw new Error('Invalid response format from API');
            }

            renderFeaturedEvents(response.events);
        } catch (error) {
            console.error('Error loading featured events:', error);
            const featuredEventsContainer = document.getElementById('featuredEvents');
            if (featuredEventsContainer) {
                featuredEventsContainer.innerHTML = `
                    <div class="error-message">
                        <i class="fas fa-exclamation-circle"></i>
                        <p>Failed to load featured events. Please try again later.</p>
                    </div>
                `;
            }
        }
    }

    function renderFeaturedEvents(events) {
        const featuredEventsContainer = document.getElementById('featuredEvents');
        if (!featuredEventsContainer) return;

        if (!events || events.length === 0) {
            featuredEventsContainer.innerHTML = `
                <div class="no-events">
                    <i class="fas fa-calendar-times"></i>
                    <p>No featured events available at the moment.</p>
                </div>
            `;
            return;
        }

        featuredEventsContainer.innerHTML = events.map(event => createEventCard(event)).join('');
    }

    function createEventCard(event) {
        const formattedDate = utils.dateUtils.formatDate(event.date);
        const formattedTime = utils.dateUtils.formatTime(event.date);

        return `
            <div class="event-card">
                <div class="event-image">
                    <img src="${event.imageUrl || 'https://via.placeholder.com/300x200?text=Event+Image'}" alt="${event.name}">
                </div>
                <div class="event-details">
                    <h4>${event.name}</h4>
                    <div class="event-info">
                        <p><i class="fas fa-calendar"></i> ${formattedDate}</p>
                        <p><i class="fas fa-clock"></i> ${formattedTime}</p>
                        <p><i class="fas fa-map-marker-alt"></i> ${event.venue}</p>
                        <p><i class="fas fa-tag"></i> ${event.category}</p>
                    </div>
                    <button onclick="window.location.href='user-event-details.html?id=${event.id}'">
                        View Details
                    </button>
                </div>
            </div>
        `;
    }
}); 