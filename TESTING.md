# Testing Notes

## Automated Tests

The solution includes a small xUnit test project:

```text
Project.Tests
```

Run tests with:

```powershell
dotnet test Project.Tests\Project.Tests.csproj
```

Current automated coverage:

- Resulting a match pays winning bets and marks losing bets.
- A match cannot be resulted twice.
- A betting account cannot be closed when it has a non-zero balance.
- A betting account cannot be closed while it has unsettled bets.

Latest local result:

```text
Passed: 3
Failed: 0
Skipped: 0
```

## Manual Test Checklist

### Admin

- Log in as `admin@betmanager.local`.
- Create a match.
- Confirm the match appears in `Matches`.
- Edit the match odds or active status.
- Result the match.
- Confirm placed bets become `Won` or `Lost`.

### User

- Register a user account.
- Create a betting account.
- Deposit funds.
- Place a bet on an active match.
- Confirm the stake is deducted from the account balance.
- Try to close the account while the balance is not zero. It should fail.
- Withdraw remaining funds until the balance is zero.
- Try to close the account while a bet is still unsettled. It should fail.
- After the admin results the match, close the account if the balance is zero.

### Database

- Confirm `BetMatches` exists after migrations.
- Confirm `BetSettlements` records are created after resulting a match.
- Confirm payout transactions are created for winning bets.

## Known Test Gaps

- No browser-based end-to-end tests yet.
- No tests for Identity login/registration.
- No tests for Razor view rendering.
- More edge cases could be added for invalid odds, inactive matches, and insufficient funds.
