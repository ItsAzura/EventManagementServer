# Event Management Server

A robust and scalable REST API backend for event management, built with **ASP.NET Core 8.0**. This solution empowers event organizers with seamless event creation, user registration, payment processing, and real-time notifications.

## ✨ Key Features

- **🎟 Event Management**: Create, update, delete, and list events with detailed metadata.
- **🔐 User Authentication & Authorization**: Secure access with JWT-based authentication and role-based permissions.
- **💳 Payment Integration**: Seamless transactions via **Stripe** and **PayPal**.
- **🔑 Google Authentication**: Enable users to sign in with their Google accounts.
- **📩 Email Notifications**: Automated email updates for event registrations and status changes.
- **📢 Real-time Updates**: Powered by **SignalR** for instant event notifications.
- **🔄 API Versioning**: Maintain backward compatibility with multiple API versions.
- **📊 Rate Limiting**: Prevent abuse by enforcing request limits.
- **📜 Structured Logging**: Enhanced debugging with **Serilog**.
- **📖 Interactive API Docs**: Swagger-based documentation for easy API exploration.

---

## 🛠 Technology Stack

| Category       | Technology               |
| -------------- | ------------------------ |
| **Framework**  | ASP.NET Core 8.0         |
| **Database**   | PostgreSQL (EF Core 9.0) |
| **Auth**       | JWT + Google OAuth       |
| **Validation** | FluentValidation         |
| **API Docs**   | Swagger/OpenAPI          |
| **Payments**   | Stripe, PayPal           |
| **Real-time**  | SignalR                  |
| **Logging**    | Serilog                  |
| **Env Config** | DotNetEnv                |

---

## ⚡ Quick Start

### 1️⃣ Prerequisites

- Install **.NET 8.0 SDK**
- Set up a **PostgreSQL database**
- Register for **Stripe/PayPal** accounts (for payments)
- Create a **Google Developer Console project** (for OAuth sign-in)
- Configure **SMTP server** (for email notifications)

### 2️⃣ Configuration

#### Database Connection

Update `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=EventManagementDB;Username=your_username;Password=your_password"
}
```

#### Environment Variables

Create a `.env` file in the root directory:

```
GOOGLE_CLIENT_ID=your_google_client_id
GOOGLE_CLIENT_SECRET=your_google_client_secret
```

#### Email Settings

```json
"EmailSettings": {
  "SmtpServer": "your_smtp_server",
  "SmtpPort": 587,
  "SmtpUsername": "your_email",
  "SmtpPassword": "your_password",
  "SenderEmail": "your_email",
  "SenderName": "Your Sender Name"
}
```

#### Payment Configuration

```json
"Stripe": {
  "SecretKey": "your_stripe_secret_key",
  "PublishableKey": "your_stripe_publishable_key",
  "WebhookSecret": "your_stripe_webhook_secret"
}
```

### 3️⃣ Run the Application

```bash
# Clone the repository
git clone https://github.com/your-repo/event-management-server.git
cd event-management-server

# Run database migrations
dotnet ef database update

# Start the application
dotnet run
```

Access API documentation at: [`https://localhost:5001/swagger`](https://localhost:5001/swagger)

---

## 🔗 API Endpoints Overview

| Endpoint             | Description                        |
| -------------------- | ---------------------------------- |
| `/api/auth`          | Authentication & User Registration |
| `/api/users`         | User Management                    |
| `/api/events`        | Event CRUD Operations              |
| `/api/categories`    | Event Categories                   |
| `/api/tickets`       | Ticket Management                  |
| `/api/registrations` | User Registrations                 |
| `/api/payment`       | Payment Processing                 |
| `/api/notifications` | Notification Handling              |
| `/api/contacts`      | Contact Form Management            |
| `/api/feedback`      | User Feedback Collection           |
| `/api/comments`      | Comment System                     |

For a full list of endpoints and request details, refer to the **Swagger API documentation**.

---

## 📁 Project Structure

```
📂 EventManagementServer
├── 📁 Controllers       # API controllers
├── 📁 Models            # Data models
├── 📁 Data              # Database context & migrations
├── 📁 Services          # Business logic
├── 📁 Repositories      # Data access logic
├── 📁 DTOs              # Data Transfer Objects
├── 📁 Validators        # FluentValidation logic
├── 📁 Helpers           # Utility functions
├── 📁 Interfaces        # Abstraction layers
└── 📁 Configurations    # Application settings
```

---

## 🔒 Security Best Practices

✅ **JWT Authentication** with expiration handling  
✅ **Role-based Authorization** for restricted access  
✅ **Rate Limiting** to prevent excessive API calls  
✅ **Input Validation** using FluentValidation  
✅ **HTTPS Enforcement** for secure communication

---

## 🤝 Contributing

We welcome contributions! 🚀 Follow these steps:

1. **Fork** the repository.
2. **Create** a new feature branch.
3. **Commit** your changes with clear messages.
4. **Submit** a pull request (PR).

---

## 📜 License

This project is licensed under the **MIT License**. See the `LICENSE` file for more details.

---

💡 _Building the future of event management—one API at a time!_
