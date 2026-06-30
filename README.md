# Bet Management System

ASP.NET Core MVC application for managing users, betting accounts, and account transactions.

## Architecture

- `Controllers` handle routing, authorization, workflow redirects, and model-state errors.
- `Models` represent EF Core entities and database relationships.
- `ViewModels` are used for form/list screens so server-owned fields such as balances, capture dates, and timestamps are not overposted.
- `Services` contain business rules for user deletion, account closing/reopening, transaction validation, and balance recalculation.
- `ApplicationDbContext` defines Identity, entity relationships, delete behavior, indexes, decimal precision, and concurrency tokens.

## Key Business Rules Covered

- Users are unique by ID number.
- Users can be searched by ID number, surname, or linked account number.
- User list pagination is capped at 10 users per page.
- Accounts are unique by account number and start with a zero balance.
- Account balance is read-only to the user and changes through transactions only.
- Accounts can only be closed when the balance is zero.
- Closed accounts cannot receive, edit, or delete transactions.
- Transactions require a debit/credit type, positive amount, and non-future transaction date.
- Capture date is set by the server when a transaction is created or edited.

## Default Seed Data

On startup the application seeds:

- Roles: `Admin`, `User`
- Admin login: `admin@betmanager.local`
- Admin password: `Admin12345!`
- Transaction types: `Credit`, `Debit`

## Running Locally

1. Ensure SQL Server or LocalDB is available.
2. Check `appsettings.json` and update `DefaultConnection` if needed.
3. Apply migrations with `dotnet ef database update`.
4. Run the app with `dotnet run`.
5. Sign in with the seeded admin account to manage users, accounts, and transactions.

## Verification Checklist

- `dotnet build` succeeds.
- Register/login/logout works.
- Admin can create, search, edit, view, and delete eligible users.
- Admin can create an account only for an existing user.
- Duplicate ID numbers and duplicate account numbers show validation errors.
- Account details show linked transactions and read-only balance.
- Creating a credit transaction increases balance.
- Creating a debit transaction decreases balance.
- Editing a transaction reverses the old amount and applies the new amount.
- Future transaction dates and zero/negative amounts are rejected.
- Accounts with non-zero balances cannot be closed.
- Contact form saves a message and displays a success message.
