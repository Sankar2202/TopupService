using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Pages
{
   
    public class IndexModel : PageModel
    {
        private readonly TopUpService _topUpService;

        public IndexModel(TopUpService topUpService)
        {
            _topUpService = topUpService;
        }

        [BindProperty]
        public string Nickname { get; set; }
        [BindProperty]
        public int BeneficiaryId { get; set; }
        [BindProperty]
        public decimal Amount { get; set; }
        [BindProperty]
        public bool IsVerified { get; set; }

        public List<Beneficiary> Beneficiaries { get; set; }
        public List<TopUpOption> TopUpOptions { get; set; }

        public void OnGet()
        {
            Beneficiaries = _topUpService.GetBeneficiaries();
            TopUpOptions = _topUpService.GetTopUpOptions();
        }

        public IActionResult OnPostAddBeneficiary()
        {
            if (!string.IsNullOrEmpty(Nickname))
            {
                if (_topUpService.AddBeneficiary(Nickname))
                {
                    return RedirectToPage();
                }
                ModelState.AddModelError(string.Empty, "Unable to add beneficiary. Ensure the nickname is under 20 characters and the limit of 5 beneficiaries is not exceeded.");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostTopUp()
        {
            decimal userBalance = await GetUserBalanceAsync();
            decimal remainingBalance = userBalance;

            if (_topUpService.TopUp(BeneficiaryId, Amount, IsVerified, userBalance, ref remainingBalance))
            {
                if (await DebitUserBalanceAsync(userBalance - remainingBalance))
                {
                    return RedirectToPage();
                }
                ModelState.AddModelError(string.Empty, "Failed to debit user balance.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Top-up failed. Please check the constraints and your balance.");
            }

            Beneficiaries = _topUpService.GetBeneficiaries();
            TopUpOptions = _topUpService.GetTopUpOptions();
            return Page();
        }

        private async Task<decimal> GetUserBalanceAsync()
        {
            await Task.Delay(100); // Simulate network delay
            return 5000m; // Example balance
        }

        private async Task<bool> DebitUserBalanceAsync(decimal amount)
        {
            await Task.Delay(100); // Simulate network delay
            return true; // Assume debit is successful
        }
    }

}
