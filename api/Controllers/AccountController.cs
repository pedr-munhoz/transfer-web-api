using api.Infrastructure.Database;
using api.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly TransfersDbContext _dbContext;

        public AccountController(TransfersDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var accounts = await _dbContext.Accounts.ToListAsync();

            return Ok(accounts);
        }

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            var account = new Account();

            await _dbContext.Accounts.AddAsync(account);
            await _dbContext.SaveChangesAsync();

            return Ok(account);
        }
    }
}