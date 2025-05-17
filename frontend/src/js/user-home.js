document.addEventListener('DOMContentLoaded', function() {
    // Check if user is logged in
    const token = localStorage.getItem('token');
    const user = JSON.parse(localStorage.getItem('user'));

    if (!token || !user) {
        window.location.href = 'login.html';
        return;
    }

    // Update user information
    const userNameElement = document.getElementById('userName');
    userNameElement.textContent = user.username || 'User';

    // Handle logout
    document.getElementById('logoutBtn').addEventListener('click', function() {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        window.location.href = 'login.html';
    });

    // Function to fetch featured events
    async function fetchFeaturedEvents() {
        try {
            const response = await fetch('https://localhost:7107/api/event?pageNumber=1&pageSize=3', {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Accept': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Failed to fetch events');
            }

            const data = await response.json();
            return data.events;
        } catch (error) {
            console.error('Error fetching featured events:', error);
            return [];
        }
    }

    // Function to format date
    function formatDate(dateString) {
        const options = { 
            year: 'numeric', 
            month: 'long', 
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        };
        return new Date(dateString).toLocaleDateString('en-US', options);
    }

    // Function to render featured events
    function renderFeaturedEvents(events) {
        const eventsContainer = document.getElementById('featuredEvents');
        eventsContainer.innerHTML = '';

        events.forEach(event => {
            const eventCard = document.createElement('div');
            eventCard.className = 'event-card';
            eventCard.innerHTML = `
                <div class="event-image">
                    <img src="${event.imageUrl || '../images/default-event.jpg'}" alt="${event.name}">
                </div>
                <div class="event-details">
                    <h4>${event.name}</h4>
                    <p class="event-date">
                        <i class="fas fa-calendar"></i>
                        ${formatDate(event.dateTime)}
                    </p>
                    <p class="event-venue">
                        <i class="fas fa-map-marker-alt"></i>
                        ${event.venue}
                    </p>
                    <p class="event-price">
                        <i class="fas fa-ticket-alt"></i>
                        $${event.price}
                    </p>
                    <button onclick="window.location.href='event-details.html?id=${event.id}'">
                        View Details
                    </button>
                </div>
            `;
            eventsContainer.appendChild(eventCard);
        });
    }

    // Load featured events
    async function loadFeaturedEvents() {
        const events = await fetchFeaturedEvents();
        renderFeaturedEvents(events);
    }

    // Initialize the page
    loadFeaturedEvents();
}); 