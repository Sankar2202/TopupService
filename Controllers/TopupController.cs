using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TopUpController : ControllerBase
    {
        private readonly TopUpService _topUpService;

        public TopUpController(TopUpService topUpService)
        {
            _topUpService = topUpService;
        }

        [HttpGet("beneficiaries")]
        public IActionResult GetBeneficiaries()
        {
            return Ok(_topUpService.GetBeneficiaries());
        }

        [HttpGet("topup-options")]
        public IActionResult GetTopUpOptions()
        {
            return Ok(_topUpService.GetTopUpOptions());
        }

        [HttpPost("beneficiaries")]
        public IActionResult AddBeneficiary([FromBody] string nickname)
        {
            if (_topUpService.AddBeneficiary(nickname))
            {
                return Ok();
            }
            return BadRequest("Unable to add beneficiary. Ensure the nickname is under 20 characters and the limit of 5 beneficiaries is not exceeded.");
        }

        [HttpPost("topup")]
        public async Task<IActionResult> TopUp(int beneficiaryId, decimal amount, bool isVerified)
        {
            // Retrieve the user balance from an external service
            decimal userBalance = await GetUserBalanceAsync();

            decimal remainingBalance = userBalance;
            if (_topUpService.TopUp(beneficiaryId, amount, isVerified, userBalance, ref remainingBalance))
            {
                // Call the external service to debit the user's balance
                if (await DebitUserBalanceAsync(userBalance - remainingBalance))
                {
                    return Ok();
                }
            }
            return BadRequest("Top-up failed. Please check the constraints and your balance.");
        }

        private async Task<decimal> GetUserBalanceAsync()
        {
            // Simulate calling an external service to get the user balance
            await Task.Delay(100); // Simulating network delay
            return 5000m; // Example balance
        }

        private async Task<bool> DebitUserBalanceAsync(decimal amount)
        {
            // Simulate calling an external service to debit the user's balance
            await Task.Delay(100); // Simulating network delay
            return true; // Assume debit is successful
        }
    }
}
