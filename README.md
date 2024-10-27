Overview
This project provides a secure system for user registration, login, and password recovery with OTP (One-Time Password) verification, using ASP.NET Core. It’s designed as a reference for students and developers aiming to build high-security environments for authentication, leveraging ASP.NET Identity, JWT, and other modern technologies.

Features
Secure User Registration: Ensures user identity through phone number validation before account creation.
OTP-based Login: Adds an extra security layer with OTP-based authentication.
Password Recovery: Allows users to securely reset passwords using OTP.
Full User Management: Create, update, and delete accounts with phone numbers as primary identifiers.
Technologies Used
ASP.NET Core: For building the web API and managing requests.
C#: The primary programming language for backend logic.
Entity Framework Core: For database operations and ORM.
ASP.NET Identity: To handle authentication and user management.
JWT (JSON Web Token): To generate secure tokens for user sessions.
RESTful API: For standardized HTTP requests and responses.
SQL Server: For data storage and user information management.
Installation and Setup
Prerequisites
.NET SDK (6.0 or later)
SQL Server for database setup
Optional: Postman for API testing
Steps
Clone the Repository:

bash
Copy code
git clone https://github.com/yourusername/your-repo-name.git
cd your-repo-name
Setup the Database:

Update the appsettings.json file with your SQL Server connection string:
json
Copy code
"ConnectionStrings": {
  "DefaultConnection": "Server=your_server;Database=your_database;User Id=your_user;Password=your_password;"
}
Run database migrations to create necessary tables:
bash
Copy code
dotnet ef database update
Run the Project:

bash
Copy code
dotnet run
Test the API:

Use tools like Postman or curl to test endpoints.
API Endpoints
User Registration and OTP Verification
POST /api/Auth/register

Request Body: { "phoneNumber": "1234567890", "password": "yourPassword", "firstName": "John", "lastName": "Doe", "email": "example@example.com" }
Description: Registers a new user and sends an OTP for phone verification.
POST /api/Auth/verify-registration-otp

Request Body: { "phoneNumber": "1234567890", "otpCode": "123456" }
Description: Verifies the OTP for registration.
User Login and Password Reset
POST /api/Auth/login

Request Body: { "phoneNumber": "1234567890", "password": "yourPassword" }
Description: Authenticates user and sends an OTP for login.
POST /api/Auth/verify-login-otp

Request Body: { "phoneNumber": "1234567890", "otpCode": "123456" }
Description: Verifies the OTP for login and returns a JWT token.
POST /api/Auth/forgot-password

Request Body: { "phoneNumber": "1234567890" }
Description: Sends an OTP for password reset.
POST /api/Auth/reset-password

Request Body: { "phoneNumber": "1234567890", "otpCode": "123456", "newPassword": "yourNewPassword" }
Description: Resets the password after OTP verification.
Additional Management Endpoints
GET /api/Auth/user/id/{id}

Description: Retrieves user information by user ID.
DELETE /api/Auth/user/id/{id}

Description: Deletes a user by ID.
Project Structure
Controllers: API controllers handling user authentication requests.
Services: Business logic for OTP generation, user management, and security.
DTOs: Data Transfer Objects to structure incoming and outgoing requests.
Models: Database models for users and roles.
Contributing
Contributions are welcome! Please submit a pull request or open an issue for feature requests or bugs.

Acknowledgments
Special thanks to Professor Khalid Al-Sabaei for guidance and support in understanding and implementing these modern authentication techniques.

License
This project is open-source and available under the MIT License.
