# Event Booking API

This is the backend API for the Event Booking System, built with .NET Core using a 3-tier architecture.

## Live API Documentation

The API is deployed and available at: [API Documentation](https://ahmedhamdy-areeb-api.runasp.net/index.html)

## Project Structure

```
Event_Booking_API/
├── BusinessLayer/       # Business Logic Layer
├── DataAccess/          # Data Access Layer
└── Event_Booking_API/   # API Layer
```

## Prerequisites

- .NET Core SDK 6.0 or later
- MySQL 8.0 or later
- Visual Studio 2022 (recommended) or Visual Studio Code

## Setup Instructions

1. Clone the repository
2. Open the solution file `Event_Booking_API.sln` in Visual Studio
3. Add your MySQL connection string to environment variables with this command: setx DefaultConnection__ConnectionString "Server=localhost;Database=EventBookingDB;User=your_username;Password=your_password;"
4. Generate JWT key from any JWT key generator website and add it to the environment variables with this command: setx JWT_SecretKey "YourSecretKey"
5. Build the solution
6. Run the application

## API Documentation

The API documentation is available through Swagger UI when running the application locally at:
```
https://localhost:7107/index.html
```

## Development

### Running Locally

1. Set the startup project to `Event_Booking_API`
2. Press F5 or click the "Run" button in Visual Studio
3. The API will be available at `https://localhost:7107`

### Testing

The API includes Swagger UI for testing endpoints. You can access it at:
```
https://localhost:7107/index.html
```

## Deployment

The API is currently deployed on Monster ASP. For deployment instructions, please contact the system administrator.

## API Endpoints

The API provides the following main endpoints:

- Authentication
  - POST /api/auth/login
  - POST /api/auth/register
- Events
  - GET /api/events
  - POST /api/events
  - GET /api/events/{id}
  - PUT /api/events/{id}
  - DELETE /api/events/{id}
- Bookings
  - GET /api/bookings
  - POST /api/bookings
  - GET /api/bookings/{id}
  - PUT /api/bookings/{id}
  - DELETE /api/bookings/{id}

For detailed API documentation, please refer to the Swagger UI. 