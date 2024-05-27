namespace WebApplication1.Models
{
    public class Beneficiary
    {
        public int Id { get; set; }
        public string Nickname { get; set; }
        public decimal MonthlyTopUpAmount { get; set; } // Track monthly top-up amount
    }

    public class TopUpOption
    {
        public decimal Amount { get; set; }
    }
}