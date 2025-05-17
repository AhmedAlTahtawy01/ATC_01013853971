document.addEventListener('DOMContentLoaded', function() {
    // Check if user is logged in
    const token = localStorage.getItem('token');
    const user = JSON.parse(localStorage.getItem('user'));

    if (!token || !user) {
        window.location.href = 'http://127.0.0.1:5500/frontend/public/login.html';
        return;
    }

    // Check for edit parameter in URL
    const urlParams = new URLSearchParams(window.location.search);
    const editEventId = urlParams.get('edit');
    if (editEventId) {
        // Remove the edit parameter from URL without refreshing
        window.history.replaceState({}, document.title, window.location.pathname);
        // Open edit modal for the specified event
        editEvent(parseInt(editEventId));
    }

    // Update user information
    const userNameElement = document.getElementById('userName');
    const userRoleElement = document.getElementById('userRole');
    
    userNameElement.textContent = user.username || 'User';
    userRoleElement.textContent = user.roleId === 1 ? 'Admin' : 'User';

    // Modal elements
    const addModal = document.getElementById('addEventModal');
    const editModal = document.getElementById('editEventModal');
    const addEventBtn = document.getElementById('addEventBtn');
    const addCloseBtn = addModal.querySelector('.close');
    const editCloseBtn = editModal.querySelector('.close');
    const addCancelBtn = addModal.querySelector('.cancel-btn');
    const editCancelBtn = editModal.querySelector('.cancel-btn');
    const addEventForm = document.getElementById('addEventForm');
    const editEventForm = document.getElementById('editEventForm');

    // Show add modal
    addEventBtn.onclick = function() {
        addModal.style.display = "block";
    }

    // Close modals
    function closeAddModal() {
        addModal.style.display = "none";
        addEventForm.reset();
    }

    function closeEditModal() {
        editModal.style.display = "none";
        editEventForm.reset();
    }

    addCloseBtn.onclick = closeAddModal;
    editCloseBtn.onclick = closeEditModal;
    addCancelBtn.onclick = closeAddModal;
    editCancelBtn.onclick = closeEditModal;

    // Close modals when clicking outside
    window.onclick = function(event) {
        if (event.target == addModal) {
            closeAddModal();
        }
        if (event.target == editModal) {
            closeEditModal();
        }
    }

    // Handle add form submission
    addEventForm.addEventListener('submit', async function(e) {
        e.preventDefault();

        const formData = {
            name: document.getElementById('eventName').value,
            description: document.getElementById('eventDescription').value,
            category: document.getElementById('eventCategory').value,
            venue: document.getElementById('eventVenue').value,
            date: document.getElementById('eventDate').value,
            price: parseFloat(document.getElementById('eventPrice').value),
            imageUrl: document.getElementById('eventImageUrl').value,
            isActive: document.getElementById('eventIsActive').checked,
            createdBy: user.id
        };

        try {
            const response = await fetch('https://ahmedhamdy-areeb-api.runasp.net/api/event', {
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
                throw new Error(errorData.message || 'Failed to create event');
            }

            const data = await response.json();
            alert('Event created successfully!');
            closeAddModal();
            loadEvents(); // Reload the events list
        } catch (error) {
            console.error('Error creating event:', error);
            alert(error.message || 'Failed to create event. Please try again.');
        }
    });

    // Handle edit form submission
    editEventForm.addEventListener('submit', async function(e) {
        e.preventDefault();

        const eventId = document.getElementById('editEventId').value;
        const formData = {
            id: parseInt(eventId),
            name: document.getElementById('editEventName').value,
            description: document.getElementById('editEventDescription').value,
            category: document.getElementById('editEventCategory').value,
            venue: document.getElementById('editEventVenue').value,
            date: document.getElementById('editEventDate').value,
            price: parseFloat(document.getElementById('editEventPrice').value),
            imageUrl: document.getElementById('editEventImageUrl').value,
            isActive: document.getElementById('editEventIsActive').checked,
            createdBy: user.id
        };

        try {
            const response = await fetch(`https://ahmedhamdy-areeb-api.runasp.net/api/event/${eventId}`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(formData)
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Failed to update event');
            }

            alert('Event updated successfully!');
            closeEditModal();
            loadEvents(); // Reload the events list
        } catch (error) {
            console.error('Error updating event:', error);
            alert(error.message || 'Failed to update event. Please try again.');
        }
    });

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

    // Function to format date for datetime-local input
    function formatDateForInput(dateString) {
        try {
            if (!dateString) return '';
            const date = new Date(dateString);
            if (isNaN(date.getTime())) return '';
            
            // Get the local date and time components
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            const hours = String(date.getHours()).padStart(2, '0');
            const minutes = String(date.getMinutes()).padStart(2, '0');
            
            // Return in the format required by datetime-local input (YYYY-MM-DDThh:mm)
            return `${year}-${month}-${day}T${hours}:${minutes}`;
        } catch (error) {
            console.error('Error formatting date for input:', error);
            return '';
        }
    }

    // Function to fetch events
    async function fetchEvents(page = 1) {
        try {
            const response = await fetch(`https://ahmedhamdy-areeb-api.runasp.net/api/event?pageNumber=${page}&pageSize=${pageSize}`, {
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
            return data;
        } catch (error) {
            console.error('Error fetching events:', error);
            throw error;
        }
    }

    // Function to fetch single event
    async function fetchEvent(eventId) {
        try {
            const response = await fetch(`https://ahmedhamdy-areeb-api.runasp.net/api/event/${eventId}`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Accept': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Failed to fetch event');
            }

            const data = await response.json();
            return data.eventData; // Return the nested eventData property
        } catch (error) {
            console.error('Error fetching event:', error);
            throw error;
        }
    }

    // Function to render events table
    function renderEvents(events) {
        const eventsGrid = document.getElementById('eventsTableBody');
        eventsGrid.innerHTML = '';

        events.forEach(event => {
            const card = document.createElement('div');
            card.className = 'event-card';
            card.onclick = () => window.location.href = `event-details.html?id=${event.id}`;
            card.innerHTML = `
                <div class="event-header">
                    <h3 class="event-name">${event.name}</h3>
                    <span class="event-status ${event.isActive ? 'active' : 'inactive'}">
                        ${event.isActive ? 'Active' : 'Inactive'}
                    </span>
                </div>
                <div class="event-details">
                    <div class="event-detail">
                        <i class="fas fa-map-marker-alt"></i>
                        <span>${event.venue || 'No venue specified'}</span>
                    </div>
                    <div class="event-detail">
                        <i class="fas fa-calendar-alt"></i>
                        <span>${formatDate(event.date)}</span>
                    </div>
                </div>
                <div class="event-actions">
                    <button class="edit-btn" onclick="event.stopPropagation(); editEvent(${event.id})">Edit</button>
                    <button class="delete-btn" onclick="event.stopPropagation(); deleteEvent(${event.id})">Delete</button>
                </div>
            `;
            eventsGrid.appendChild(card);
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
                loadEvents();
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
                loadEvents();
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
                loadEvents();
            }
        };
        pagination.appendChild(nextButton);
    }

    // Function to load events
    async function loadEvents() {
        try {
            const data = await fetchEvents(currentPage);
            renderEvents(data.events);
            renderPagination(Math.ceil(data.totalCount / pageSize));
        } catch (error) {
            console.error('Error loading events:', error);
            // You might want to show an error message to the user here
        }
    }

    // Function to handle event deletion
    window.deleteEvent = async function(eventId) {
        if (!confirm('Are you sure you want to delete this event?')) {
            return;
        }

        try {
            const response = await fetch(`https://ahmedhamdy-areeb-api.runasp.net/api/event/${eventId}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Accept': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Failed to delete event');
            }

            // Reload events after successful deletion
            loadEvents();
        } catch (error) {
            console.error('Error deleting event:', error);
            alert('Failed to delete event. Please try again.');
        }
    };

    // Function to handle event editing
    window.editEvent = async function(eventId) {
        try {
            const event = await fetchEvent(eventId);
            
            // Populate the edit form
            document.getElementById('editEventId').value = event.id;
            document.getElementById('editEventName').value = event.name || '';
            document.getElementById('editEventDescription').value = event.description || '';
            document.getElementById('editEventCategory').value = event.category || '';
            document.getElementById('editEventVenue').value = event.venue || '';
            
            // Handle date formatting with error checking
            const formattedDate = formatDateForInput(event.date);
            document.getElementById('editEventDate').value = formattedDate;
            
            document.getElementById('editEventPrice').value = event.price || 0;
            document.getElementById('editEventImageUrl').value = event.imageUrl || '';
            document.getElementById('editEventIsActive').checked = event.isActive || false;

            // Show the edit modal
            editModal.style.display = "block";
        } catch (error) {
            console.error('Error loading event for edit:', error);
            alert('Failed to load event details. Please try again.');
        }
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

    // Initial load of events
    loadEvents();
}); 