using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class TopUpService
    {
        private readonly List<Beneficiary> _beneficiaries = new List<Beneficiary>();
        private readonly List<TopUpOption> _topUpOptions = new List<TopUpOption>
    {
        new TopUpOption { Amount = 5 },
        new TopUpOption { Amount = 10 },
        new TopUpOption { Amount = 20 },
        new TopUpOption { Amount = 30 },
        new TopUpOption { Amount = 50 },
        new TopUpOption { Amount = 75 },
        new TopUpOption { Amount = 100 }
    };

        public TopUpService()
        {
        }

        public List<Beneficiary> GetBeneficiaries()
        {
            return _beneficiaries;
        }

        public List<TopUpOption> GetTopUpOptions()
        {
            return _topUpOptions;
        }

        public bool AddBeneficiary(string nickname)
        {
            if (_beneficiaries.Count >= 5 || nickname.Length > 20)
            {
                return false;
            }

            _beneficiaries.Add(new Beneficiary { Id = _beneficiaries.Count + 1, Nickname = nickname });
            return true;
        }

        public bool TopUp(int beneficiaryId, decimal amount, bool isVerified, decimal userBalance, ref decimal remainingBalance)
        {
            var beneficiary = _beneficiaries.FirstOrDefault(b => b.Id == beneficiaryId);
            if (beneficiary == null || userBalance < amount + 1)
            {
                return false;
            }

            decimal maxMonthlyTopUp = isVerified ? 500 : 1000;
            if (beneficiary.MonthlyTopUpAmount + amount > maxMonthlyTopUp)
            {
                return false;
            }

            decimal totalMonthlyTopUp = _beneficiaries.Sum(b => b.MonthlyTopUpAmount) + amount;
            if (totalMonthlyTopUp > 3000)
            {
                return false;
            }

            // Deduct the balance and update the beneficiary's top-up amount
            remainingBalance = userBalance - (amount + 1);
            beneficiary.MonthlyTopUpAmount += amount;

            return true;
        }
    }
}
