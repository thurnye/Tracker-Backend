using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> CreateUpdateWallet([FromBody] Wallet wallet)
        {
            var result = await _walletService.CreateUpdateWalletAsync(wallet);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserWallets()
        {
            var wallets = await _walletService.GetUserWalletsAsync();
            return Ok(wallets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWalletById(string id)
        {
            var wallet = await _walletService.GetWalletAsync(id);
            if (wallet == null) return NotFound();
            return Ok(wallet);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWallet(string id)
        {
            var deleted = await _walletService.DeleteWalletAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
