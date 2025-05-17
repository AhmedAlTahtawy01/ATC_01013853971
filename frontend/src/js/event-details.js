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

    // Get event ID from URL
    const urlParams = new URLSearchParams(window.location.search);
    const eventId = urlParams.get('id');

    if (!eventId) {
        alert('No event ID provided');
        window.location.href = 'events.html';
        return;
    }

    // Modal elements
    const editModal = document.getElementById('editEventModal');
    const editCloseBtn = editModal.querySelector('.close');
    const editCancelBtn = editModal.querySelector('.cancel-btn');
    const editEventForm = document.getElementById('editEventForm');

    // Function to format date
    function formatDate(dateString) {
        try {
            if (!dateString) return '-';
            const date = new Date(dateString);
            if (isNaN(date.getTime())) return '-';
            return date.toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'long',
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

    // Function to format price
    function formatPrice(price) {
        return `$${parseFloat(price).toFixed(2)}`;
    }

    // Function to fetch event details
    async function fetchEventDetails() {
        try {
            const response = await fetch(`https://localhost:7107/api/event/${eventId}`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Accept': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Failed to fetch event details');
            }

            const data = await response.json();
            return data.eventData;
        } catch (error) {
            console.error('Error fetching event details:', error);
            throw error;
        }
    }

    // Function to update UI with event details
    function updateEventDetails(event) {
        try {
            // Update all fields with proper error handling
            const elements = {
                'eventName': event.name || 'No name available',
                'eventDescription': event.description || 'No description available',
                'eventCategory': event.category || 'Not specified',
                'eventVenue': event.venue || 'Not specified',
                'eventDate': formatDate(event.date) || 'No date available',
                'eventPrice': formatPrice(event.price) || 'No price available'
            };

            // Update text content for each element
            Object.entries(elements).forEach(([id, value]) => {
                const element = document.getElementById(id);
                if (element) {
                    element.textContent = value;
                }
            });

            // Update status
            const statusElement = document.getElementById('eventStatus');
            if (statusElement) {
                statusElement.textContent = event.isActive ? 'Active' : 'Inactive';
                statusElement.className = `event-status ${event.isActive ? 'active' : 'inactive'}`;
            }
            
            // Set event image
            const eventImage = document.getElementById('eventImage');
            if (eventImage) {
                if (event.imageUrl) {
                    eventImage.src = event.imageUrl;
                    eventImage.style.display = 'block';
                } else {
                    eventImage.style.display = 'none';
                }
            }
        } catch (error) {
            console.error('Error updating event details:', error);
            alert('Error displaying event details. Please try again.');
        }
    }

    // Function to load event details
    async function loadEventDetails() {
        try {
            // Show loading state
            const loadingElements = document.querySelectorAll('#eventName, #eventStatus, #eventDescription, #eventCategory, #eventVenue, #eventDate, #eventPrice');
            loadingElements.forEach(element => {
                if (element) element.textContent = 'Loading...';
            });

            const event = await fetchEventDetails();
            if (!event) {
                throw new Error('No event data received');
            }
            updateEventDetails(event);
        } catch (error) {
            console.error('Error loading event details:', error);
            // Show error state
            const errorElements = document.querySelectorAll('#eventName, #eventStatus, #eventDescription, #eventCategory, #eventVenue, #eventDate, #eventPrice');
            errorElements.forEach(element => {
                if (element) element.textContent = 'Error loading data';
            });
            alert('Failed to load event details. Please try again.');
        }
    }

    // Function to close edit modal
    function closeEditModal() {
        editModal.style.display = "none";
        editEventForm.reset();
    }

    // Close modal when clicking close button or cancel button
    editCloseBtn.onclick = closeEditModal;
    editCancelBtn.onclick = closeEditModal;

    // Close modal when clicking outside
    window.onclick = function(event) {
        if (event.target == editModal) {
            closeEditModal();
        }
    }

    // Handle edit button click
    document.getElementById('editEventBtn').addEventListener('click', async function() {
        try {
            const event = await fetchEventDetails();
            
            // Populate the edit form
            document.getElementById('editEventId').value = event.id;
            document.getElementById('editEventName').value = event.name || '';
            document.getElementById('editEventDescription').value = event.description || '';
            document.getElementById('editEventCategory').value = event.category || '';
            document.getElementById('editEventVenue').value = event.venue || '';
            document.getElementById('editEventDate').value = formatDateForInput(event.date);
            document.getElementById('editEventPrice').value = event.price || 0;
            document.getElementById('editEventImageUrl').value = event.imageUrl || '';
            document.getElementById('editEventIsActive').checked = event.isActive || false;

            // Show the edit modal
            editModal.style.display = "block";
        } catch (error) {
            console.error('Error loading event for edit:', error);
            alert('Failed to load event details. Please try again.');
        }
    });

    // Handle edit form submission
    editEventForm.addEventListener('submit', async function(e) {
        e.preventDefault();

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
            const response = await fetch(`https://localhost:7107/api/event/${eventId}`, {
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
            // Reload event details
            const updatedEvent = await fetchEventDetails();
            updateEventDetails(updatedEvent);
        } catch (error) {
            console.error('Error updating event:', error);
            alert(error.message || 'Failed to update event. Please try again.');
        }
    });

    // Handle delete button click
    document.getElementById('deleteEventBtn').addEventListener('click', async function() {
        if (!confirm('Are you sure you want to delete this event?')) {
            return;
        }

        try {
            const response = await fetch(`https://localhost:7107/api/event/${eventId}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Accept': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Failed to delete event');
            }

            alert('Event deleted successfully');
            window.location.href = 'events.html';
        } catch (error) {
            console.error('Error deleting event:', error);
            alert('Failed to delete event. Please try again.');
        }
    });

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

    // Initial load of event details
    loadEventDetails();
}); 