using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DonationsTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        // ✅ Combined create + update
        [HttpPost("create-update")]
        public async Task<IActionResult> CreateUpdateTransaction([FromBody] Transaction transaction)
        {
            var result = await _transactionService.CreateUpdateTransactionAsync(transaction);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserTransactions()
        {
            var transactions = await _transactionService.GetUserTransactionsAsync();
            return Ok(transactions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransactionById(string id)
        {
            var transaction = await _transactionService.GetTransactionAsync(id);
            if (transaction == null) return NotFound();
            return Ok(transaction);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(string id)
        {
            var deleted = await _transactionService.DeleteTransactionAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
