# Bet Management System Documentation

## Purpose

Bet Management System is an ASP.NET Core MVC application for managing betting users, their betting accounts, match betting, and account transactions.

## Roles

### Admin

Admins can:

- Manage users.
- Create and manage betting accounts.
- Create matches that users can bet on.
- Result matches after they are completed.
- View all placed bets and transactions.
- Close betting accounts only when the balance is zero and there are no unsettled bets.

### User

Users can:

- Register and log in.
- View and edit their profile.
- Create their own betting accounts.
- Deposit and withdraw funds.
- Place bets on active matches created by admins.
- Close their own betting account only when the balance is zero and there are no unsettled bets.

## Main Workflows

### Match Creation

1. Admin opens `Admin Dashboard`.
2. Admin chooses `Manage Matches`.
3. Admin creates a match with teams, sport, date, odds, and active status.
4. Active upcoming matches become available to users.

### Placing a Bet

1. User opens `Bet Matches`.
2. User selects an active match.
3. User chooses an open betting account.
4. User selects Home, Away, or Draw where available.
5. User enters a stake.
6. The system checks that the account is open and has enough funds.
7. The stake is deducted from the betting account balance.
8. A bet and account transaction are recorded.

### Resulting a Match

1. Admin opens match details.
2. Admin clicks `Result Match`.
3. Admin chooses the winning selection.
4. The system settles every placed bet for that match.
5. Winning bets are marked `Won` and paid into the user's betting account.
6. Losing bets are marked `Lost`; the stake remains lost because it was deducted when the bet was placed.
7. A `BetSettlement` record is created for each settled bet.

### Closing an Account

An account may only be closed when:

- The balance is exactly `0`.
- There are no bets with status `Placed`.

This rule applies to both users and admins.

## Important Services

- `BetMatchService`: match CRUD and match settlement.
- `BetService`: placing bets and reading bets.
- `BettingAccountService`: account creation, update, close, reopen, and ownership-aware close rules.
- `AccountTransactionService`: deposits, withdrawals, and transaction balance effects.
- `ApplicationUserService`: user search, pagination, create/edit/delete rules.

## Database Notes

The application uses EF Core with SQL Server.

Important tables include:

- `AppUsers`
- `BettingAccounts`
- `AccountTransactions`
- `TransactionTypes`
- `BetMatches`
- `Bets`
- `BetSettlements`

The app calls `Database.MigrateAsync()` on startup, so pending migrations are applied automatically when the database connection is valid.

## Default Seed Data

On startup the app seeds:

- Roles: `Admin`, `User`
- Admin account: `admin@betmanager.local`
- Admin password: `Admin12345!`
- Transaction types: `Credit`, `Debit`

## Known Setup Requirement

If the app reports a missing table, run:

```powershell
Update-Database
```

or:

```powershell
dotnet ef database update
```

Then restart the application.
