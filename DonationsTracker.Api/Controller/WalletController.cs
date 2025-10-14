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
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        // create + update
        [HttpPost("create-update")]
        public async Task<IActionResult> CreateUpdateWallet([FromBody] WalletRequest wallet)
        {
            var result = await _walletService.CreateUpdateWalletAsync(wallet);

            return Ok(new ApiResponse<WalletDTO>
            {
                Data = result,
                Meta = new ApiMeta
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserWallets()
        {
            var wallets = await _walletService.GetUserWalletsAsync();

            return Ok(new ApiResponse<List<WalletDTO>>
            {
                Data = wallets.ToList(),
                Meta = new ApiMeta
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWalletById(string id)
        {
            var wallet = await _walletService.GetWalletAsync(id);

            if (wallet == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Data = null,
                    Errors = new List<ApiError>
                    {
                        new ApiError
                        {
                            Code = ErrorCode.NOT_FOUND,
                            Message = $"Wallet with ID {id} was not found."
                        }
                    },
                    Meta = new ApiMeta
                    {
                        RequestId = Guid.NewGuid().ToString(),
                        Timestamp = DateTime.UtcNow
                    }
                });
            }

            return Ok(new ApiResponse<WalletDTO>
            {
                Data = wallet,
                Meta = new ApiMeta
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow
                }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWallet(string id)
        {
            var deleted = await _walletService.DeleteWalletAsync(id);

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
                            Message = $"Wallet with ID {id} was not found."
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
                Data = $"Wallet with ID {id} deleted successfully.",
                Meta = new ApiMeta
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow
                }
            });
        }
    }
}
