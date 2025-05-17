# Event Booking Database

This directory contains the SQL scripts for setting up the Event Booking System database.

## Database Scripts

- `Database.sql` - Main database creation script
- `Roles_Insertion.sql` - Script for inserting initial roles

## Setup Instructions

1. Ensure MySQL Server is installed and running
2. Open MySQL Workbench or MySQL Command Line Client
3. Connect to your MySQL instance
4. Execute the scripts in the following order:
   ```sql
   -- First, run the main database script
   source Database.sql;
   
   -- Then, run the roles insertion script
   USE EventBookingDB;
   source Roles_Insertion.sql;
   ```

## Database Schema

The database includes the following main tables:

- Users
- Roles
- Events
- Bookings
- Tags
- EventTags

## Troubleshooting

Common issues and solutions:

1. Connection Issues
   - Verify MySQL service is running
   - Check connection string
   - Ensure MySQL port (3306) is accessible
   - Check MySQL error logs