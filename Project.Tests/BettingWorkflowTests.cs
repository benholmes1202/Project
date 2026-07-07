using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Project.Models;
using Project.Services;
using Project.ViewModels.BetMatches;

namespace Project.Tests;

public class BettingWorkflowTests
{
    [Fact]
    public async Task ResultMatchAsync_PaysWinningBetsAndMarksLosingBets()
    {
        await using var context = CreateContext();
        var account = await SeedSettlementScenarioAsync(context);
        var match = await context.BetMatches.SingleAsync();
        var service = new BetMatchService(context);

        var result = await service.ResultMatchAsync(new ResultMatchViewModel
        {
            BetMatchId = match.BetMatchId,
            WinningSelection = "Home"
        });

        Assert.True(result.Succeeded);
        Assert.Equal(20m, account.Balance);
        Assert.Equal("Home", match.WinningSelection);
        Assert.False(match.IsActive);

        var bets = await context.Bets.OrderBy(b => b.BetId).ToListAsync();
        Assert.Equal("Won", bets[0].Status);
        Assert.Equal("Lost", bets[1].Status);

        var settlements = await context.BetSettlements.OrderBy(s => s.SettlementId).ToListAsync();
        Assert.Equal(2, settlements.Count);
        Assert.Contains(settlements, s => s.Result == "Won" && s.PayoutAmount == 20m && s.ProfitLoss == 10m);
        Assert.Contains(settlements, s => s.Result == "Lost" && s.PayoutAmount == 0m && s.ProfitLoss == -5m);

        var payoutTransaction = await context.AccountTransactions.SingleAsync();
        Assert.Equal(20m, payoutTransaction.Amount);
        Assert.Equal("Payout Bet #1", payoutTransaction.Reference);
    }

    [Fact]
    public async Task ResultMatchAsync_DoesNotAllowDuplicateResult()
    {
        await using var context = CreateContext();
        await SeedSettlementScenarioAsync(context);
        var match = await context.BetMatches.SingleAsync();
        var service = new BetMatchService(context);

        await service.ResultMatchAsync(new ResultMatchViewModel
        {
            BetMatchId = match.BetMatchId,
            WinningSelection = "Home"
        });

        var secondResult = await service.ResultMatchAsync(new ResultMatchViewModel
        {
            BetMatchId = match.BetMatchId,
            WinningSelection = "Away"
        });

        Assert.False(secondResult.Succeeded);
        Assert.Equal(2, await context.BetSettlements.CountAsync());
    }

    [Fact]
    public async Task CloseAsync_BlocksNonZeroBalanceAndUnsettledBets()
    {
        await using var context = CreateContext();
        var service = new BettingAccountService(context);
        var user = new ApplicationUser
        {
            IdNumber = "1234567890123",
            FirstName = "Test",
            Surname = "User",
            Email = "test@example.com"
        };
        context.AppUsers.Add(user);
        await context.SaveChangesAsync();

        var fundedAccount = new BettingAccount
        {
            UserId = user.UserId,
            AccountNumber = "ACC-1",
            CurrencyCode = "ZAR",
            Balance = 25m,
            Status = "Open"
        };
        var pendingBetAccount = new BettingAccount
        {
            UserId = user.UserId,
            AccountNumber = "ACC-2",
            CurrencyCode = "ZAR",
            Balance = 0m,
            Status = "Open"
        };
        context.BettingAccounts.AddRange(fundedAccount, pendingBetAccount);
        await context.SaveChangesAsync();

        context.Bets.Add(new Bet
        {
            AccountId = pendingBetAccount.AccountId,
            Category = "Soccer",
            Selection = "Home",
            Amount = 10m,
            Odds = 2m,
            PotentialPayout = 20m,
            Status = "Placed"
        });
        await context.SaveChangesAsync();

        var fundedClose = await service.CloseAsync(fundedAccount.AccountId);
        var pendingBetClose = await service.CloseAsync(pendingBetAccount.AccountId);

        Assert.False(fundedClose.Succeeded);
        Assert.False(pendingBetClose.Succeeded);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<BettingAccount> SeedSettlementScenarioAsync(ApplicationDbContext context)
    {
        context.TransactionTypes.Add(new TransactionType
        {
            Name = "Credit",
            Direction = "Credit",
            BalanceEffect = 1,
            IsActive = true
        });

        var user = new ApplicationUser
        {
            IdNumber = "9001015009087",
            FirstName = "Player",
            Surname = "One",
            Email = "player@example.com"
        };

        var account = new BettingAccount
        {
            User = user,
            AccountNumber = "BET-100",
            CurrencyCode = "ZAR",
            Balance = 0m,
            Status = "Open"
        };

        var match = new BetMatch
        {
            HomeTeam = "Falcons",
            AwayTeam = "Rangers",
            Sport = "Soccer",
            MatchDate = DateTime.Now.AddHours(1),
            HomeOdds = 2m,
            AwayOdds = 3m,
            IsActive = true
        };

        context.AppUsers.Add(user);
        context.BettingAccounts.Add(account);
        context.BetMatches.Add(match);
        await context.SaveChangesAsync();

        context.Bets.AddRange(
            new Bet
            {
                AccountId = account.AccountId,
                BetMatchId = match.BetMatchId,
                Category = "Soccer",
                Selection = "Home",
                Amount = 10m,
                Odds = 2m,
                PotentialPayout = 20m,
                Status = "Placed"
            },
            new Bet
            {
                AccountId = account.AccountId,
                BetMatchId = match.BetMatchId,
                Category = "Soccer",
                Selection = "Away",
                Amount = 5m,
                Odds = 3m,
                PotentialPayout = 15m,
                Status = "Placed"
            });

        await context.SaveChangesAsync();
        return account;
    }
}
