using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces;
using DonationsTracker.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DonationsTracker.Core.RequestModel;

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

        /// <summary>
        /// Create or Update Transaction
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        [HttpPost("create-update")]
        public async Task<IActionResult> CreateUpdateTransaction([FromBody] TransactionRequest transaction)
        {
            var result = await _transactionService.CreateUpdateTransactionAsync(transaction);

            return Ok(new ApiResponse<TransactionDTO>
            {
                Data = result,
                Meta = new ApiMeta
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow
                }
            });
        }

        /// <summary>
        /// Get User Transactions
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetUserTransactions([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var (transactions, pagination) = await _transactionService.GetUserTransactionsAsync(page, limit);

            return Ok(new ApiResponse<List<TransactionDTO>>
            {
                Data = transactions.ToList(),
                Meta = new ApiMeta
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Pagination = pagination
                }
            });
        }

        
        /// <summary>
        /// Get Transaction By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransactionById(string id)
        {
            var transaction = await _transactionService.GetTransactionAsync(id);

            if (transaction == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Data = null,
                    Errors = new List<ApiError>
                    {
                        new ApiError
                        {
                            Code = ErrorCode.NOT_FOUND,
                            Message = $"Transaction with ID {id} was not found."
                        }
                    },
                    Meta = new ApiMeta
                    {
                        RequestId = Guid.NewGuid().ToString(),
                        Timestamp = DateTime.UtcNow
                    }
                });
            }

            return Ok(new ApiResponse<TransactionDTO>
            {
                Data = transaction,
                Meta = new ApiMeta
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow
                }
            });
        }


        /// <summary>
        /// Get Wallet Transactions
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("wallet/{walletId}")]
        public async Task<IActionResult> GetTransactionsByWalletId(string walletId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var (transactions, pagination) = await _transactionService.GetTransactionsByWalletIdAsync(walletId, page, limit);

            return Ok(new ApiResponse<List<TransactionDTO>>
            {
                Data = transactions.ToList(),
                Meta = new ApiMeta
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    Pagination = pagination
                }
            });
        }

        /// <summary>
        /// Delete Transaction
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(string id)
        {
            var deleted = await _transactionService.DeleteTransactionAsync(id);

            if (!deleted)
            {
                return NotFound(new ApiResponse<object>
                {
                    Data = null,
                    Errors = new List<ApiError>
                    {
                        new ApiError
                        {
                            Code = ErrorCode.NOT_FOUND,
                            Message = $"Transaction with ID {id} was not found."
                        }
                    },
                    Meta = new ApiMeta
                    {
                        RequestId = Guid.NewGuid().ToString(),
                        Timestamp = DateTime.UtcNow
                    }
                });
            }

            return Ok(new ApiResponse<object>
            {
                Data = $"Transaction with ID {id} deleted successfully.",
                Meta = new ApiMeta
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow
                }
            });
        }
    }
}
