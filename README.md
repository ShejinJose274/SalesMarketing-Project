# SalesMarketing System

SalesMarketing System is a full-stack sales-force and marketing-team management application. It contains a .NET 8 ASP.NET Core Web API backed by PostgreSQL and an Angular 22 frontend using Syncfusion controls.

The application provides:

- Cookie-based login and role-aware navigation
- Salesman Master maintenance
- Marketing Team Master maintenance
- Team-member assignment with roles and target amounts
- Searchable and paged master lists
- Read-only marketing-team details
- EF Core relationship constraints and transactional team updates

## Technology stack

### Backend

- .NET 8 / ASP.NET Core Web API
- C# 12 with nullable reference types enabled
- Entity Framework Core 8
- Npgsql PostgreSQL provider 8
- PostgreSQL 15 or later recommended
- Swashbuckle / Swagger
- ASP.NET Core cookie authentication

### Frontend

- Angular 22
- TypeScript 6
- Angular Router and HttpClient
- Angular Reactive Forms and RxJS
- Syncfusion Angular Grid, DropDownList, DatePicker, NumericTextBox, Dialog, Button, and input controls
- Vitest and jsdom for unit tests

## Repository structure

```text
SalesMarketingSystem/
├── README.md
└── backend/
	└── SalesMarketing/
		├── SalesMarketing.slnx
		└── SalesMarketing.API/
			├── Controllers/
			├── Data/
			├── DTOs/
			├── Enums/
			├── Models/
			├── Services/
			├── Properties/
			├── appsettings.json
			├── appsettings.Development.json
			├── Program.cs
			└── frontend/
				└── sales-marketing-ui/
					├── src/app/
					├── public/
					├── package.json
					├── angular.json
					└── vitest.config.ts
```

## Backend architecture

The API uses a small service-oriented structure:

- `Controllers` expose HTTP endpoints and translate application errors into HTTP responses.
- `Services` contain validation, business rules, queries, transactions, and persistence operations.
- `DTOs` define request and response contracts so database entities are not exposed directly.
- `Models` contain EF Core entities mapped to PostgreSQL tables.
- `Data/SalesMarketingDbContext.cs` configures tables, indexes, relationships, defaults, precision, and database checks.
- `Enums` contains shared backend values such as roles and record statuses.
- `Program.cs` registers EF Core, CORS, authentication, authorization, Swagger, and application services.

The current API registers `SalesmanService` and `MarketingTeamService` with dependency injection and uses one `SalesMarketingDbContext` per request.

## Database model

The application expects PostgreSQL tables with the following relationships:

### `salesman`

Stores salesman master records.

- Identity primary key: `id`
- Unique code: `salesmancode`, maximum 20 characters
- Name: maximum 100 characters
- Optional mobile, email, and joining date
- `status`: `1` Active or `2` Inactive
- Created and modified timestamps

### `marketingteam`

Stores team headers.

- Identity primary key: `id`
- Unique team code: `teamcode`, maximum 20 characters
- Team name: maximum 100 characters
- Required `teamleaderid` referencing `salesman`
- Optional description, maximum 500 characters
- `status`: `1` Active or `2` Inactive

### `marketingteamdetail`

Stores salesman assignments to teams.

- Identity primary key: `id`
- `marketingteamid` references `marketingteam`
- `salesmanid` references `salesman`
- `role`: values `1` through `4`
- `targetamount`: numeric `(18,2)`, not negative
- `status`: `1` Active or `2` Inactive
- Unique `(marketingteamid, salesmanid)` constraint
- Unique filtered index on `salesmanid` for active assignments (`status = 1`)

Team deletion uses cascade behavior for detail rows. Salesman references use restricted deletion, so assigned salesmen cannot be physically deleted. EF Core mappings are in `Data/SalesMarketingDbContext.cs`.

> The H8 index deployment script is at `backend/SalesMarketing/SalesMarketing.API/Data/Sql/20261112_add_active_salesman_unique_index.sql`. Run its duplicate check, resolve any returned rows, then apply the `CREATE UNIQUE INDEX` statement to an existing database. The same index is also configured in EF Core for newly created schemas.

## Enumerations

### Record status

| Value | Name | Meaning |
|---:|---|---|
| 1 | Active | Available for normal use |
| 2 | Inactive | Retained but unavailable for new assignments |

### Salesman role

| Value | Name | Display text |
|---:|---|---|
| 1 | TeamLeader | Team Leader |
| 2 | SeniorSalesExecutive | Senior Sales Executive |
| 3 | SalesExecutive | Sales Executive |
| 4 | Trainee | Trainee |

## Prerequisites

Install the following tools:

- .NET 8 SDK
- Node.js compatible with the Angular 22 toolchain
- npm 12 or a compatible npm version
- PostgreSQL 15 or later
- Visual Studio 2022/2026, VS Code, or another .NET-compatible IDE

Verify the installations:

```powershell
dotnet --version
node --version
npm --version
psql --version
```

## Configuration

### Database connection

Set `ConnectionStrings:DefaultConnection` in user secrets, environment-specific configuration, or `appsettings.Development.json`.

Example placeholder configuration:

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Host=localhost;Port=5432;Database=salesmarketing;Username=postgres;Password=CHANGE_ME"
  }
}
```

Do not commit real passwords or production connection strings.

### Development login configuration

`AuthController` compares the submitted login values with the configured development credentials and creates an HTTP-only authentication cookie. Login is rate-limited per remote IP. The current fixed-account approach is for development/demo use; use an identity provider or hashed application-user store for production.

Configure credentials outside source control, preferably with user secrets:

```powershell
cd backend/SalesMarketing/SalesMarketing.API
dotnet user-secrets set "AdminLogin:MasterUsername" "masteradmin"
dotnet user-secrets set "AdminLogin:MasterPassword" "CHANGE_ME"
dotnet user-secrets set "AdminLogin:ManagerUsername" "salesmanager"
dotnet user-secrets set "AdminLogin:ManagerPassword" "CHANGE_ME"
```

The available application roles are:

- `MasterAdmin` - marketing-team management access
- `SalesManagerAdmin` - salesman management access

Use strong, environment-specific credentials outside development. The current credential approach is intended for this application’s development setup and should be replaced with a production identity provider or secure credential store for production deployment.

### CORS and API URL

The API allows frontend origins configured under `Frontend:AllowedOrigins`. Development defaults include:

- `http://localhost:4200`
- `https://localhost:4200`

The Angular development environment currently targets:

```text
https://localhost:7178/api
```

If the API uses another port or protocol, update the Angular environment configuration under `frontend/sales-marketing-ui/src/environments/` and the API CORS settings.

## Run the backend

From the API directory:

```powershell
cd backend/SalesMarketing/SalesMarketing.API
dotnet restore
dotnet build
dotnet run
```

When running in Development, Swagger is available at the HTTPS URL printed by ASP.NET Core, normally:

```text
https://localhost:7178/swagger
```

The API also exposes health and database test controllers in the project for development diagnostics.

## Run the frontend

From the Angular directory:

```powershell
cd backend/SalesMarketing/SalesMarketing.API/frontend/sales-marketing-ui
npm install
npm start
```

Open:

```text
http://localhost:4200
```

The frontend sends cookies with API requests. Start the API first and trust the local HTTPS development certificate if the browser reports a certificate warning.

## Frontend navigation

The authenticated shell contains the following routes:

| Route | Purpose | Access |
|---|---|---|
| `/login` | Login page | Anonymous |
| `/dashboard` | Summary dashboard | Authenticated |
| `/salesman` | Salesman Master list | Authenticated |
| `/salesman/new` | Create salesman | Authenticated |
| `/salesman/:id/edit` | Edit salesman | Authenticated |
| `/marketing-team` | Marketing Team Master list | `MasterAdmin` |
| `/marketing-team/new` | Create marketing team | `MasterAdmin` |
| `/marketing-team/:id` | Read-only team view | `MasterAdmin` |
| `/marketing-team/:id/edit` | Edit team and members | `MasterAdmin` |

Legacy aliases `/salesmen` and `/marketing-teams` are also retained in the Angular router.

## API reference

All endpoints are under `/api`.

### Authentication

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/auth/login` | Sign in and create the authentication cookie |
| `GET` | `/auth/me` | Return the current username and role |
| `POST` | `/auth/logout` | Clear the authentication cookie |

### Salesmen

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/salesmen` | Searchable, status-filtered, paged salesman list |
| `GET` | `/salesmen/{id}` | Get one salesman |
| `POST` | `/salesmen` | Create a salesman |
| `PUT` | `/salesmen/{id}` | Update a salesman |
| `PATCH` | `/salesmen/{id}/status?status=1\|2` | Activate or deactivate a salesman |
| `DELETE` | `/salesmen/{id}` | Delete an unused salesman |
| `GET` | `/salesmen/available` | Get active salesmen available for assignment |
| `GET` | `/salesmen/status/{status}` | Get salesmen by status |

`GET /salesmen` accepts `search`, `status`, `page`, and `pageSize` query parameters. Page size is limited by the service to 100 records.

### Marketing teams

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/marketingteams` | Searchable, status-filtered, paged team list |
| `GET` | `/marketingteams/{id}` | Get a team with its member rows |
| `POST` | `/marketingteams` | Create a team and its members transactionally |
| `PUT` | `/marketingteams/{id}` | Update header and merge member rows transactionally |
| `POST` | `/marketingteams/{id}/members` | Add a member to an existing team |
| `PATCH` | `/marketingteams/{id}/status?status=1\|2` | Activate or deactivate a team |
| `DELETE` | `/marketingteams/{id}` | Delete a team and cascade its detail rows |

`GET /marketingteams` accepts `search`, `status`, `page`, and `pageSize` query parameters.

Paged responses have this shape:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 10,
  "totalCount": 0,
  "totalPages": 0
}
```

## Business rules and validation

The application and database enforce the following rules:

- Salesman and team codes are required, normalized to uppercase by the services, and unique.
- Salesman codes and team codes accept letters and digits only.
- Salesman names and team names are required.
- Mobile numbers, when supplied, must contain 7–15 digits.
- Email addresses are validated when supplied.
- Joining dates cannot be in the future.
- Status must be `Active` or `Inactive`.
- A team must contain at least one member.
- A salesman cannot be repeated within one team.
- Exactly one member must have the `TeamLeader` role.
- The detail-row team leader must match the team header’s `teamleaderid`.
- Only active salesmen can be newly assigned.
- An active salesman cannot be an active member of multiple active teams.
- Target amounts must be zero or greater.
- Team create/update operations save the header and detail rows in one transaction.
- Removed team rows are deleted during update.
- Deleting a team cascades to its detail rows.
- A salesman with team assignment history cannot be deleted; deactivate the record instead.

## Development commands

### Backend

```powershell
cd backend/SalesMarketing/SalesMarketing.API
dotnet restore
dotnet build
dotnet run
```

### Frontend

```powershell
cd backend/SalesMarketing/SalesMarketing.API/frontend/sales-marketing-ui
npm install
npm start
npm run build
npm test -- --watch=false
```

### Solution build

From the repository root:

```powershell
dotnet build backend/SalesMarketing/SalesMarketing.slnx
```

## Testing checklist

Before submitting changes, verify:

- Login succeeds for each configured development role.
- Salesman list search, status filter, create, edit, activate/deactivate, and delete work.
- Assigned salesmen cannot be deleted.
- A team can be created with multiple active salesmen.
- Duplicate team codes and duplicate team members are rejected.
- The header team leader matches the only `TeamLeader` detail row.
- Team edit can add, update, and remove member rows in one save.
- Team deletion removes its detail rows.
- Invalid team saves do not leave partial data.
- Angular build and tests pass.
- API build passes and Swagger endpoints are reachable.

## Troubleshooting

### Database connection failure

Confirm PostgreSQL is running, the database exists, the connection string is configured in the active environment, and the database user has access.

### Browser CORS or cookie errors

Confirm the frontend origin exactly matches `Frontend:AllowedOrigins`, the API is running over HTTPS when the Angular environment uses HTTPS, and the browser accepts the local development certificate.

### Port mismatch

Check `Properties/launchSettings.json`, the terminal output from `dotnet run`, and `src/environments/environment.development.ts`. Keep the API URL and CORS origin configuration synchronized.

### API executable locked during build

Stop any running API/debug process, then run `dotnet build` again. Visual Studio or `dotnet run` can hold the generated executable while a build is attempted.

## Security notes

- Never commit database passwords or login credentials.
- Use HTTPS outside local development.
- Replace development credential comparison with a production authentication provider.
- Review and enable controller-level authorization policies before production deployment.
- Validate database permissions and use least-privilege PostgreSQL users.
- Keep Angular and .NET dependencies updated with security patches.
