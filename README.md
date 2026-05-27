# Student Planner

Student Planner is a comprehensive web application designed to help students manage their academic and personal schedules efficiently. It integrates with university systems (USOS) and allows for collaborative event management.

## Project Overview

The application provides a centralized platform for:
- Managing personal and academic events.
- Synchronizing with university schedules (via USOS integration).
- Submitting and moderating requests for academic events.
- Role-based access control (Student, Manager, Admin).

## 🛠 Tech Stack

### Backend
- **Framework:** .NET 8 / ASP.NET Core Web API
- **Database:** Microsoft SQL Server
- **ORM:** Entity Framework Core
- **Authentication:** ASP.NET Core Identity with JWT
- **Email:** SMTP Integration for notifications
- **Testing:** xUnit for integration and unit tests

### Frontend
- **Framework:** React 19 (Vite)
- **Styling:** Tailwind CSS 4
- **Routing:** React Router 7
- **State Management:** React Hooks
- **Testing:** Vitest & React Testing Library

### Infrastructure
- **Containerization:** Docker & Docker Compose

## Screenshots

*TODO*

|                              Landing Page                              |                            Dashboard                             |                            Schedule View                            |
|:----------------------------------------------------------------------:|:----------------------------------------------------------------:|:-------------------------------------------------------------------:|
| ![Landing Page](https://via.placeholder.com/300x200?text=Landing+Page) | ![Dashboard](https://via.placeholder.com/300x200?text=Dashboard) | ![Schedule](https://via.placeholder.com/300x200?text=Schedule+View) |

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js & npm
- SQL Server (or Docker)

### Running with Docker
The easiest way to get the whole system running is using Docker Compose:
```bash
docker-compose up --build
```

### Manual Setup

#### Backend
1. Navigate to `StudentPlanner.Api`.
2. Update `appsettings.json` with your database connection string.
3. Run migrations and start:
   ```bash
   dotnet run
   ```

#### Frontend
1. Navigate to `frontend`.
2. Install dependencies:
   ```bash
   npm install
   ```
3. Start the development server:
   ```bash
   npm run dev
   ```
