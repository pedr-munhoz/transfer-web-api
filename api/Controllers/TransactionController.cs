using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Infrastructure.Database;
using api.Models.Entities;
using api.Models.ViewModels;
using api.Services;
using Medallion.Threading.Postgres;
using Medallion.Threading.Redis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly TransfersDbContext _dbContext;
        private readonly ILockService _lockService;

        public TransactionController(TransfersDbContext dbContext, ILockService lockService)
        {
            _dbContext = dbContext;
            _lockService = lockService;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var transactions = await _dbContext.Transactions.ToListAsync();

            return Ok(transactions);
        }

        [HttpPost]
        [Route("deposit/lock")]
        public async Task<IActionResult> DepositWithLock([FromBody] DepositViewModel model)
        {
            var account = await _dbContext.Accounts
                .Where(x => x.Id == model.AccountId)
                .FirstOrDefaultAsync();

            if (account == null)
                return NotFound("Account not found");

            var transaction = new Transaction
            {
                DestinationAccountId = account.Id,
                Amount = model.Amount,
            };

            await _lockService.ExecuteLockedAsync(
                key: account.Id.ToString(),
                method: async () =>
                {
                    await _dbContext.Entry(account).ReloadAsync();

                    account.Balance += transaction.Amount;

                    Thread.Sleep(5000);

                    await _dbContext.Transactions.AddAsync(transaction);
                    await _dbContext.SaveChangesAsync();
                }
            );

            return Ok(transaction);
        }

        [HttpPost]
        [Route("deposit/no-lock")]
        public async Task<IActionResult> DepositWithoutLock([FromBody] DepositViewModel model)
        {
            var account = await _dbContext.Accounts
                .Where(x => x.Id == model.AccountId)
                .FirstOrDefaultAsync();

            if (account == null)
                return NotFound("Account not found");

            var transaction = new Transaction
            {
                DestinationAccountId = account.Id,
                Amount = model.Amount,
            };

            account.Balance += transaction.Amount;

            Thread.Sleep(5000);

            await _dbContext.Transactions.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();

            return Ok(transaction);
        }

        [HttpPost]
        [Route("transfer/lock")]
        public async Task<IActionResult> TransferWithLock([FromBody] TransferViewModel model)
        {
            var originAccount = await _dbContext.Accounts
                .Where(x => x.Id == model.OriginAccountId)
                .FirstOrDefaultAsync();

            if (originAccount == null)
                return NotFound("Origin account not found");

            var destinationAccount = await _dbContext.Accounts
                .Where(x => x.Id == model.DestinationAccountId)
                .FirstOrDefaultAsync();

            if (destinationAccount == null)
                return NotFound("Destination account not found");

            var transaction = new Transaction
            {
                DestinationAccountId = destinationAccount.Id,
                OriginAccountId = originAccount.Id,
                Amount = model.Amount,
            };

            var approved = await _lockService.ExecuteLockedAsync<bool>(
                keys: new List<string> { originAccount.Id.ToString(), destinationAccount.Id.ToString() },
                method: async () =>
                {
                    await _dbContext.Entry(originAccount).ReloadAsync();

                    if (originAccount.Balance < model.Amount)
                        return false;

                    destinationAccount.Balance += transaction.Amount;
                    originAccount.Balance -= transaction.Amount;

                    Thread.Sleep(5000);

                    await _dbContext.Transactions.AddAsync(transaction);
                    await _dbContext.SaveChangesAsync();

                    return true;
                }
            );

            if (!approved)
                return UnprocessableEntity("Not enough balance");

            return Ok(transaction);
        }

        [HttpPost]
        [Route("transfer/no-lock")]
        public async Task<IActionResult> TransferWithoutLock([FromBody] TransferViewModel model)
        {
            var originAccount = await _dbContext.Accounts
                .Where(x => x.Id == model.OriginAccountId)
                .FirstOrDefaultAsync();

            if (originAccount == null)
                return NotFound("Origin account not found");

            var destinationAccount = await _dbContext.Accounts
                .Where(x => x.Id == model.DestinationAccountId)
                .FirstOrDefaultAsync();

            if (destinationAccount == null)
                return NotFound("Destination account not found");

            if (originAccount.Balance < model.Amount)
                return UnprocessableEntity("Not enough balance");

            var transaction = new Transaction
            {
                DestinationAccountId = destinationAccount.Id,
                OriginAccountId = originAccount.Id,
                Amount = model.Amount,
            };

            destinationAccount.Balance += transaction.Amount;
            originAccount.Balance -= transaction.Amount;

            Thread.Sleep(5000);

            await _dbContext.Transactions.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();

            return Ok(transaction);
        }
    }
}