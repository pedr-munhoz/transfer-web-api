using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Infrastructure.Database;
using api.Models.Entities;
using api.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly TransfersDbContext _dbContext;

        public TransactionController(TransfersDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var transactions = await _dbContext.Transactions.ToListAsync();

            return Ok(transactions);
        }

        [HttpPost]
        [Route("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositViewModel model)
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

            await _dbContext.Transactions.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();

            return Ok(transaction);
        }

        [HttpPost]
        [Route("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferViewModel model)
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

            await _dbContext.Transactions.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();

            return Ok(transaction);
        }
    }
}