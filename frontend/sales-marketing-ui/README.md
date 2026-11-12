# SalesMarketing System

Salesman Master and Marketing Team Master for the .NET 8/PostgreSQL SalesMarketing API.

## Structure

- `../` - ASP.NET Core Web API, EF Core/Npgsql services, controllers, DTOs and database mappings.
- `src/app/pages/salesmen` - searchable, filterable Salesman Master with create/edit/deactivate/delete.
- `src/app/pages/marketing-teams` - Marketing Team list and transactional header/member editor.
- `src/app/pages/marketing-team-view` - read-only team detail view.
- `src/app/services/api.service.ts` - Angular API client.

## Setup

1. Configure `ConnectionStrings:DefaultConnection` in the API configuration for PostgreSQL 15+.
2. Apply the database schema/migrations and ensure the `salesman`, `marketingteam`, and `marketingteamdetail` tables exist.
3. Start the API from `backend/SalesMarketing/SalesMarketing.API` with `dotnet run`.
4. Install and start the frontend:

```bash
npm install
npm start
```

The development frontend runs at `http://localhost:4200`; the default API URL is `https://localhost:7178/api`.

## API endpoints

- `GET/POST /api/salesmen`, `GET/PUT/DELETE /api/salesmen/{id}`
- `PATCH /api/salesmen/{id}/status?status=1|2`
- `GET /api/salesmen/available`
- `GET/POST /api/marketingteams`, `GET/PUT/DELETE /api/marketingteams/{id}`
- `PATCH /api/marketingteams/{id}/status?status=1|2`

List endpoints support `search`, `status`, `page`, and `pageSize`. Team create/update sends the header and all member rows in one transaction. Team deletion cascades to detail rows; a salesman with assignment history cannot be deleted.

## Validation and tests

The API validates uniqueness, active-salesman assignment, one matching team leader, duplicate members, non-negative targets, and future joining dates. Run `npm run build`, `npm test -- --watch=false`, and `dotnet build` before deployment.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
