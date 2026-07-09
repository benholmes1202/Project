# Bet Management System

## Project Documentation and Planning Records

## 1. Project Overview

The Bet Management System is an ASP.NET Core MVC application for managing users, betting accounts, matches, bets, transactions, and bet settlements.

The application supports two roles:

* **Admin**
* **User**

ASP.NET Core Identity is used for authentication and role management. Entity Framework Core and SQL Server are used for database access and storage.

The project is intended for educational and demonstration purposes.

---

## 2. Project Objectives

The main objectives of the system are to:

* Allow users to register and log in securely.
* Separate administrator and user permissions.
* Allow users to manage their profiles.
* Allow users to create and manage betting accounts.
* Support deposits and withdrawals.
* Allow administrators to create and manage matches.
* Allow users to place bets on active matches.
* Deduct stakes from betting account balances.
* Settle winning and losing bets.
* Record account transactions and bet settlements.
* Prevent unauthorised access to another user’s information.
* Enforce account-closing and balance rules.



## 3. Project Scope

Included

* Registration and login
* Role-based authorization
* Profile management
* User management
* Betting account creation
* Deposits and withdrawals
* Match management
* Bet placement
* User bet history
* Match settlement
* Transaction history
* Account closing and reopening
* Database migrations
* Seed data
* Unit and integration testing

Excluded

* Real payment-provider integration
* Live sports data
* Live odds
* Real-money gambling
* Multi-currency support
* Mobile applications
* Complex accumulator bets
* Production gambling compliance systems

---

## 4. User Roles

### Admin

Administrators can:

* Manage users.
* Manage betting accounts.
* Create and edit matches.
* Activate or deactivate matches.
* Result completed matches.
* View all bets and transactions.
* Close or reopen eligible betting accounts.

### User

Users can:

* Register and log in.
* View and edit their profile.
* Create betting accounts.
* Deposit and withdraw funds.
* View active matches.
* Place bets.
* View their own bets.
* View their own transactions.
* Close eligible betting accounts.

---

## 5. Functional Requirements

| ID    | Requirement                                                      |
| ----- | ---------------------------------------------------------------- |
| FR-01 | Users must be able to register and log in.                       |
| FR-02 | Administrative features must be restricted to the Admin role.    |
| FR-03 | Users must only access their own private betting information.    |
| FR-04 | Users must be able to create betting accounts.                   |
| FR-05 | Users must be able to deposit and withdraw funds.                |
| FR-06 | Withdrawals must not exceed the available balance.               |
| FR-07 | Administrators must be able to create and manage matches.        |
| FR-08 | Users must only place bets on active matches.                    |
| FR-09 | A user must have enough funds to place a bet.                    |
| FR-10 | A successful bet must deduct the stake from the account balance. |
| FR-11 | Users must be able to view their own bets.                       |
| FR-12 | Administrators must be able to result matches.                   |
| FR-13 | Winning bets must receive the correct payout.                    |
| FR-14 | Losing bets must be marked as lost.                              |
| FR-15 | A settlement record must be created for each settled bet.        |
| FR-16 | An account may only close when its balance is zero.              |
| FR-17 | An account may only close when it has no unsettled bets.         |

---

## 6. Non-Functional Requirements

* The system must use server-side validation.
* Passwords must be handled through ASP.NET Core Identity.
* Important business logic must be placed in services.
* Database access must use Entity Framework Core.
* Sensitive production credentials must not be committed to Git.
* Users must not be able to modify another user’s records.
* Important business rules should be covered by tests.
* The project should follow consistent C# naming and coding conventions.

---

## 7. Initial Development Plan

Phase 1: Planning

* Review the project specification and rubric.
* Identify Admin and User requirements.
* Define entities and relationships.
* Plan controllers, services, ViewModels, and views.
* Record important business rules.

Phase 2: Authentication and Users

* Configure ASP.NET Core Identity.
* Create Admin and User roles.
* Implement registration and login.
* Add profile management.
* Add administrative user management.

Phase 3: Betting Accounts

* Create betting account entities.
* Implement deposits and withdrawals.
* Record transactions.
* Implement account-closing rules.

Phase 4: Matches and Bets

* Create match management.
* Display active matches.
* Implement bet placement.
* Deduct stakes.
* Add the My Bets page.

Phase 5: Settlement

* Add match-result functionality.
* Settle winning and losing bets.
* Credit winning payouts.
* Create settlement records.
* Prevent duplicate settlement.

Phase 6: Testing and Documentation

* Add unit and integration tests.
* Record test evidence.
* Complete setup instructions.
* Add diagrams and technical documentation.

---

## 8. Use-Case Diagram

The following diagram shows the main actions available to users and administrators.

<img width="367" height="2556" alt="Use Case diagram" src="https://github.com/user-attachments/assets/14aba14f-dafa-46f5-a534-11ef573ffa52" />


## 9. Application Architecture

The project follows the ASP.NET Core MVC pattern with a service layer.

<img width="705" height="523" alt="High-Level Architecture Diagram" src="https://github.com/user-attachments/assets/3003ce26-0c25-49ce-9b41-5ee13a8fc0bd" />


### Layer Responsibilities

| Layer                 | Responsibility                             |
| --------------------- | ------------------------------------------ |
| Razor Views           | Display information and collect form input |
| Controllers           | Handle HTTP requests and responses         |
| ViewModels            | Transfer page-specific data                |
| Services              | Enforce business rules                     |
| ApplicationDbContext  | Access and update database records         |
| ASP.NET Core Identity | Handle users, passwords, roles, and login  |
| SQL Server            | Store application data                     |



## 10. Main Entities

### ApplicationUser

Represents the user’s application profile and links to an ASP.NET Identity user.

Important fields may include:

* `Id`
* `IdentityUserId`
* `FirstName`
* `LastName`
* `IdNumber`
* `Email`

### BettingAccount

Represents an account used to hold funds and place bets.

Important fields may include:

* `Id`
* `ApplicationUserId`
* `AccountName`
* `Balance`
* `IsOpen`
* `CreatedAt`
* `ClosedAt`

### AccountTransaction

Represents a change to a betting account balance.

Important fields may include:

* `Id`
* `BettingAccountId`
* `TransactionTypeId`
* `Amount`
* `BalanceBefore`
* `BalanceAfter`
* `Description`
* `CreatedAt`

### TransactionType

Represents the type of transaction.

Seeded values include:

* `Credit`
* `Debit`

### BetMatch

Represents a sporting match available for betting.

Important fields may include:

* `Id`
* `Sport`
* `HomeTeam`
* `AwayTeam`
* `MatchDate`
* `HomeOdds`
* `AwayOdds`
* `DrawOdds`
* `IsActive`
* `IsResulted`
* `WinningSelection`

### Bet

Represents a bet placed by a user.

Important fields may include:

* `Id`
* `BettingAccountId`
* `BetMatchId`
* `Selection`
* `Stake`
* `Odds`
* `PotentialPayout`
* `Status`
* `DatePlaced`

### BetSettlement

Represents the result of a settled bet.

Important fields may include:

* `Id`
* `BetId`
* `Outcome`
* `PayoutAmount`
* `SettledAt`
* `AccountTransactionId`



## 11. Entity-Relationship Diagram

<img width="1780" height="2353" alt="Entity-Relationship Diagram" src="https://github.com/user-attachments/assets/c259da98-2962-4359-b82a-c794c8606dd6" />


## 12. UML Class Diagram

<img width="1780" height="1768" alt="UML Class Diagram" src="https://github.com/user-attachments/assets/45432594-2dba-4854-bf0f-343dc84da638" />


## 13. Core Business Rules

### Betting Accounts

* Users may only manage accounts they own.
* Closed accounts cannot be used to place bets.
* Account balances may not fall below zero.
* An account may only close when its balance is exactly zero.
* An account may only close when no `Placed` bets exist.

### Deposits

* The amount must be greater than zero.
* The account must exist and be open.
* The balance must increase by the deposit amount.
* A credit transaction must be recorded.

### Withdrawals

* The amount must be greater than zero.
* The account must exist and be open.
* The amount must not exceed the available balance.
* The balance must decrease by the withdrawal amount.
* A debit transaction must be recorded.

### Bet Placement

* The user must be authenticated.
* The match must exist and be active.
* The match must not already be resulted.
* The betting account must belong to the user.
* The betting account must be open.
* The stake must be greater than zero.
* The account must have enough funds.
* The selected outcome must be valid.
* The stake must be deducted immediately.
* The bet must initially have the status `Placed`.
* A transaction must be recorded.

### Match Settlement

* Only an administrator may result a match.
* A match may only be resulted once.
* Winning bets must be marked `Won`.
* Losing bets must be marked `Lost`.
* Winning payouts must be credited to the correct account.
* A settlement record must be created for each settled bet.



## 14. Bet Placement Sequence Diagram

<img width="1538" height="2145" alt="Place Bet Sequence Diagram" src="https://github.com/user-attachments/assets/550243b8-0c1d-4666-9b85-cf00b6d8b3e6" />


## 15. Match Settlement Sequence Diagram

<img width="1393" height="1869" alt="Result Match Sequence Diagram" src="https://github.com/user-attachments/assets/335e685e-964c-4bbf-a982-da3f9a117281" />


## 16. Important Services

### `ApplicationUserService`

Responsible for:

* User search
* Pagination
* User creation
* User updates
* User deletion rules
* Profile retrieval

### `BettingAccountService`

Responsible for:

* Account creation
* Account retrieval
* Ownership checks
* Account closing
* Account reopening
* Closing eligibility rules

### `AccountTransactionService`

Responsible for:

* Deposits
* Withdrawals
* Balance updates
* Credit and debit transactions
* Transaction history

### `BetService`

Responsible for:

* Bet validation
* Stake validation
* Ownership checks
* Balance checks
* Bet placement
* User bet history

### `BetMatchService`

Responsible for:

* Match creation
* Match updates
* Active match retrieval
* Match resulting
* Bet settlement
* Winning payouts

---

## 17. Security

The application uses ASP.NET Core Identity for:

* Registration
* Login
* Logout
* Password hashing
* User identification
* Role management

Security measures include:

* Role-based authorization
* Ownership checks
* Server-side validation
* Anti-forgery protection
* ViewModels for form input
* Entity Framework Core parameterised queries
* Restricted administrator actions
* Secure handling of configuration values

Authorization must be enforced in controllers and services, not only by hiding buttons or links.

---

## 18. Database Setup

### Prerequisites

* Visual Studio 2022
* Required .NET SDK
* SQL Server or LocalDB
* Entity Framework Core tools
* Git

Check installed SDKs:

```powershell
dotnet --list-sdks
```

Install Entity Framework Core tools:

```powershell
dotnet tool install --global dotnet-ef
```

Restore packages:

```powershell
dotnet restore
```

Build the solution:

```powershell
dotnet build
```

Apply migrations:

```powershell
dotnet ef database update
```

Or use Package Manager Console:

```powershell
Update-Database
```

Run the application:

```powershell
dotnet run
```

---

## 19. Seed Data

The application may seed:

* `Admin` role
* `User` role
* `Credit` transaction type
* `Debit` transaction type
* A development administrator account

Seeded administrator credentials should only be used for local development. Production credentials must not be stored directly in source control.

---

## 20. Testing

The solution should contain a separate test project with actual `.cs` test files.

Recommended tools include:

* xUnit
* Moq
* FluentAssertions
* SQLite in-memory
* Coverlet
* ReportGenerator

### Important Test Areas

* Valid deposits
* Invalid deposit amounts
* Valid withdrawals
* Withdrawals above the balance
* Bets with insufficient funds
* Bets using closed accounts
* Bets using another user’s account
* Account closure with a non-zero balance
* Account closure with unsettled bets
* Winning payout calculation
* Losing bet settlement
* Duplicate match settlement
* My Bets returning only the logged-in user’s bets

Run all tests:

```powershell
dotnet test
```

Generate a test result file:

```powershell
dotnet test --logger "trx;LogFileName=test-results.trx"
```

Collect coverage:

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

---

## 21. Known Limitations

* Deposits and withdrawals are simulated.
* No real payment provider is connected.
* Match information and odds are entered manually.
* No live sports provider is connected.
* The application is not intended for real-money gambling.
* Production auditing and monitoring are limited.

---

## 22. Future Improvements

Possible improvements include:

* Live sports-data integration
* Live odds
* Payment-provider integration
* Email notifications
* Two-factor authentication
* Audit logs
* Responsible-gambling limits
* Real-time updates using SignalR
* Expanded reporting
* Improved automated testing
* Continuous integration and deployment

