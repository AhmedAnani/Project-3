# Country Explorer 🌍

A full-stack travel discovery and planning application that helps you explore countries, discover hidden attractions, and plan your next adventure with personalized recommendations.

---

##  Table of Contents

- [Features](#features)
- [Project Structure](#project-structure)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Setup & Installation](#setup--installation)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
- [Usage Guide](#usage-guide)
- [Database](#database)
- [Authentication](#authentication)
- [Contributing](#contributing)

---

##  Features

### Core Features
- **🔐 Google OAuth Authentication** - Secure login via Google accounts
- **🌐 Country Explorer** - Browse detailed country information, climate, economy, and more
- **🎯 Smart Destination Discovery** - AI-powered recommendations based on your trip preferences
- **🗺️ Tourist Attractions** - Discover popular attractions with ratings and details
- **💰 Budget Planning** - Calculate estimated costs for your trips
- **✈️ Trip Management** - Create, organize, and manage multiple trips
- **📅 Google Calendar Integration** - Sync your travel plans with Google Calendar
- **💬 User Profiles** - Personalized user settings and preferences

### Technical Features
- JWT-based Authentication & Authorization
- Rate Limiting to prevent abuse
- Global Exception Handling
- CORS Support
- Comprehensive API Documentation (Swagger)
- Database Migrations with EF Core
- AutoMapper for DTO Mapping

---

##  Project Structure

```
/backend (ASP.NET Core)
  ├── Controllers/          # API endpoints
  ├── Services/             # Business logic
  ├── Repositories/         # Data access
  ├── DTOs/                 # Data transfer objects
  ├── Models/               # Database entities
  ├── Middlewares/          # Custom middleware
  ├── Extensions/           # Extension methods
  └── Program.cs            # Configuration

/frontend (React + TypeScript)
  ├── pages/                # Page components (home, country, trips, etc.)
  ├── components/           # Reusable UI components
  ├── hooks/                # Custom React hooks
  ├── context/              # Context API for state management
  ├── lib/                  # Utilities and helpers
  └── styles/               # Tailwind CSS styles
```

---

##  Tech Stack

### Backend
- **Framework**: ASP.NET Core 8
- **Database**: Entity Framework Core (SQL Server)
- **Authentication**: Google OAuth, JWT
- **API Documentation**: Swagger/OpenAPI
- **Mapping**: AutoMapper
- **Validation**: FluentValidation

### Frontend
- **UI Framework**: React 18
- **Language**: TypeScript
- **Build Tool**: Vite
- **Styling**: Tailwind CSS
- **UI Components**: shadcn/ui (Radix UI)
- **Form Handling**: React Hook Form
- **HTTP Client**: Axios
- **State Management**: React Context + React Query
- **Routing**: Wouter
- **Data Validation**: Zod

### External APIs
- **Google OAuth** - Authentication
- **Google Calendar API** - Calendar integration
- **REST Countries API** - Country data
- **Open Trip Map API** - Tourist attractions
- **Exchange Rate API** - Currency conversion

---

##  Prerequisites

### Backend
- .NET SDK 8.0 or higher
- SQL Server (local or remote)
- Visual Studio or VS Code

### Frontend
- Node.js 18+ and npm/pnpm
- VS Code or your preferred code editor

---

##  Setup & Installation

### Step 1: Clone the Repository
```bash
git clone <repository-url>
cd country-explorer
```

### Step 2: Backend Setup

```bash
# Navigate to backend directory
cd backend

# Install dependencies
dotnet restore

# Update database connection in appsettings.json
# Create and run migrations
dotnet ef database update

# Run the backend
dotnet run
```

The backend will be available at `https://localhost:7293`

### Step 3: Frontend Setup

```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
pnpm install
# or
npm install

# Start the development server
pnpm dev
# or
npm run dev
```

The frontend will be available at `http://localhost:5173`

---

##  Configuration

### Backend Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=CountryExplorerDb;Trusted_Connection=true;"
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-characters",
    "Issuer": "YourAppName",
    "Audience": "YourAppUsers",
    "ExpirationMinutes": 60
  },
  "Google": {
    "ClientId": "your-google-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-google-client-secret"
  },
  "AppBaseUrl": "https://localhost:7293",
  "FrontendBaseUrl": "http://localhost:5173"
}
```

### Frontend Configuration (.env)

```env
VITE_API_BASE_URL=https://localhost:7293
VITE_GOOGLE_CLIENT_ID=your-google-client-id
```

### Google OAuth Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com)
2. Create a new project
3. Enable Google+ API
4. Create OAuth 2.0 credentials (Web application)
5. Add authorized redirect URIs:
   - `https://localhost:7293/signin-google`
   - `https://yourdomain.com/signin-google`
6. Copy Client ID and Client Secret to your configuration

---

##  Running the Application

### Development Mode

**Terminal 1 - Backend**:
```bash
cd backend
dotnet run
# Swagger UI: https://localhost:7293/swagger
```

**Terminal 2 - Frontend**:
```bash
cd frontend
pnpm dev
# Open: http://localhost:5173
```

### Production Build

**Backend**:
```bash
cd backend
dotnet publish -c Release
```

**Frontend**:
```bash
cd frontend
pnpm build
pnpm serve
```

---

##  API Endpoints

### Authentication
- `GET /api/auth/login` - Initiate Google OAuth login
- `GET /api/auth/oauth-complete` - OAuth callback
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Logout user

### Countries
- `GET /api/countries` - Get all countries
- `GET /api/countries/{code}` - Get country details
- `GET /api/countries/{code}/attractions` - Get attractions in a country

### Discovery
- `POST /api/discovery` - Get destination recommendations
- `POST /api/discovery/save` - Save a discovery result

### Trips
- `GET /api/trips` - Get user's trips
- `POST /api/trips` - Create a new trip
- `PUT /api/trips/{id}` - Update trip
- `DELETE /api/trips/{id}` - Delete trip
- `POST /api/trips/{id}/items` - Add items to trip
- `PUT /api/trips/{id}/items/{itemId}` - Update trip item
- `DELETE /api/trips/{id}/items/{itemId}` - Remove trip item

### Users
- `GET /api/users/profile` - Get user profile
- `PUT /api/users/profile` - Update user profile
- `POST /api/users/calendar/sync` - Sync trips to Google Calendar

---

##  Usage Guide

### For Users

1. **Sign Up/Login**
   - Click "Login" button
   - Authenticate with Google account
   - Grant necessary permissions

2. **Explore Countries**
   - Use search bar to find a country
   - View detailed country information
   - Browse attractions and landmarks

3. **Discover Destinations**
   - Go to Discover section
   - Answer questions about your trip (budget, purpose, preferences)
   - Get AI-powered recommendations
   - Save interesting destinations

4. **Plan Trips**
   - Create a new trip
   - Add attractions and activities
   - Set budget and dates
   - Sync to Google Calendar

5. **Manage Profile**
   - Update personal information
   - View trip history
   - Manage saved destinations

### For Developers

1. **Adding New Features**
   - Backend: Add services, repositories, controllers
   - Frontend: Create React components, pages
   - Update API endpoints and DTOs
   - Write tests

2. **Database Changes**
   ```bash
   cd backend
   dotnet ef migrations add MigrationName
   dotnet ef database update
   ```

3. **API Documentation**
   - Swagger available at `/swagger`
   - Auto-generated from XML comments
   - Update endpoint summaries and descriptions

---

## 🗄️ Database

### Entity Relationships

- **User** ↔ **Trip** (One-to-Many)
- **Trip** ↔ **TripBucketItem** (One-to-Many)
- **User** ↔ **Destination** (One-to-Many, saved)
- **Destination** ↔ **VibeTag** (Many-to-Many)

### Migrations

Located in `Infrastructure/Data/Migrations/`

To create a new migration:
```bash
dotnet ef migrations add YourMigrationName --project Infrastructure
```

---

##  Authentication

### How It Works

1. **Google OAuth Flow**
   - User clicks login
   - Redirected to Google
   - User authenticates and grants permissions
   - Google redirects back with code
   - Backend exchanges code for tokens
   - User stored in database (first time only)
   - JWT and Refresh tokens generated

2. **Token Management**
   - Access Token: Short-lived (60 minutes)
   - Refresh Token: Long-lived, stored in DB
   - Token refresh automatically handled by frontend

3. **Authorization**
   - Role-based access control (User, Admin)
   - Policies enforced on protected endpoints
   - Claims-based authorization

---

##  Troubleshooting

### Common Issues

**CORS Error**
- Check `AppBaseUrl` and `FrontendBaseUrl` configuration
- Ensure CORS policy includes your frontend URL

**Google OAuth Not Working**
- Verify Client ID and Secret are correct
- Check redirect URIs in Google Console
- Ensure domain is in authorized list

**Database Connection Error**
- Verify connection string
- Ensure SQL Server is running
- Check credentials and permissions

**Frontend Build Issues**
- Clear `node_modules` and reinstall: `rm -rf node_modules && pnpm install`
- Clear Vite cache: `rm -rf dist .vite`

---

##  Environment Variables

### Backend (appsettings.json)
```
ConnectionStrings:DefaultConnection
Jwt:Key
Jwt:Issuer
Jwt:Audience
Google:ClientId
Google:ClientSecret
AppBaseUrl
FrontendBaseUrl
```

### Frontend (.env)
```
VITE_API_BASE_URL
VITE_GOOGLE_CLIENT_ID
```

---

##  Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

##  Future Enhancements

- [ ] Mobile app (React Native)
- [ ] Advanced filters in discovery
- [ ] Social sharing features
- [ ] Travel itinerary templates
- [ ] Multi-language support
- [ ] Dark mode
- [ ] Offline support
- [ ] Push notifications

---

##  Support

For issues, questions, or suggestions:
- Open an GitHub issue
- Contact the development team
- Check documentation and FAQs

---

##  Acknowledgments

- Built with ❤️ by the Country Explorer Team
- Uses [shadcn/ui](https://ui.shadcn.com) components
- Inspired by travel enthusiasts worldwide
- Thanks to all contributors

---

**Happy Traveling! 🌏✈️**

---

## Submission Information

### Domain

```text
https://localhost:7293
```

### Endpoint List

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/auth/login` | Start Google OAuth |
| GET | `/api/auth/oauth-complete` | OAuth callback |
| POST | `/api/auth/refresh` | Refresh JWT token |
| POST | `/api/auth/logout` | Logout |
| GET | `/api/countries` | Get all countries |
| GET | `/api/countries/{code}` | Get country details |
| GET | `/api/countries/{code}/attractions` | Get country attractions |
| POST | `/api/discovery` | Get destination recommendations |
| POST | `/api/discovery/save` | Save recommendation |
| GET | `/api/trips` | Get user trips |
| POST | `/api/trips` | Create trip |
| PUT | `/api/trips/{id}` | Update trip |
| DELETE | `/api/trips/{id}` | Delete trip |
| GET | `/api/users/profile` | Get user profile |
| PUT | `/api/users/profile` | Update profile |
| POST | `/api/users/calendar/sync` | Sync Google Calendar |

### Setup Instructions

#### Backend

```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
```

Backend URL: `https://localhost:7293`

#### Frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend URL: `http://localhost:5173`

### Postman Collection

https://science-cosmologist-98925816-s-team.postman.co/workspace/My-Workspace~47cfdf6d-8c77-4568-8a76-fbd773454d26/collection/23821349-97073763-48b3-4e32-8ce9-745cdc140cf4?action=share&source=copy-link&creator=23821349
