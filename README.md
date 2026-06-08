# School Information System (Edupage)

ASP.NET Core web application for school administration and student services.
It includes login/session handling, role-based pages (admin, teacher, student), timetable management,
lunch menu management, courses, and school events.
<p align="center">
<img width="995" height="624" alt="image" src="https://github.com/user-attachments/assets/42c7133d-049a-4851-a079-21e64ff77702" />
<p/>


## Prerequisites

- .NET SDK 8.0+ installed
- Windows, Linux, or macOS

## Quick Start

1. Restore dependencies:

```bash
dotnet restore Edupage.sln
```

2. Build the project:

```bash
dotnet build Edupage.sln
```

3. Run the app:

```bash
dotnet run --project Edupage.csproj
```

4. Open in browser:

- App: http://localhost:5093/


## Features

- User login, logout, and session handling with cookie-based auth
- Role-based experience for admin, teacher, and student users
- Timetable management, including classes, subjects, and classrooms
- Course management, including course entries/content updates
- Lunch menu planning with editable food options
- School event management
- Separate role-specific pages for main view, timetable, courses, lunch menu, user management, and gallery
  
<p align="center">
<img width="1342" height="547" alt="b" src="https://github.com/user-attachments/assets/586bc59d-c4f5-4bf9-a602-eed2744cbf24" />
<img width="1344" height="610" alt="imageeee" src="https://github.com/user-attachments/assets/00d4131b-70ab-4795-b1a5-d53204e6b444" />
<p/>
  
## Database Notes

- SQLite file path: `Database/edupage.sqlite3`
- Schema creation runs on startup in `Program.cs`
- Lunch sample data/menu generation runs at startup (`LunchGenerator`)

## Project Structure

```text
Controllers/    API controllers and database helpers
Models/         Data models and DTOs
Database/       SQLite database file
wwwroot/        Login page and shared static assets
assets/         Role-specific pages (admin/student/teacher)
Program.cs      App startup, routing, DB initialization
```
