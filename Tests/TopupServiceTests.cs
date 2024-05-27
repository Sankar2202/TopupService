using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using WebApplication1.Services;

namespace WebApplication1.TopUpServiceTests
{
    [TestFixture]
    public class TopUpServiceTests
    {
        private TopUpService _topUpService;

        [SetUp]
        public void Setup()
        {
            _topUpService = new TopUpService();
        }

        [Test]
        public void AddBeneficiary_ShouldAddBeneficiary_WhenNicknameIsValid()
        {
            // Arrange
            string nickname = "TestBeneficiary";

            // Act
            bool result = _topUpService.AddBeneficiary(nickname);
            var beneficiaries = _topUpService.GetBeneficiaries();

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, beneficiaries.Count);
            Assert.AreEqual(nickname, beneficiaries.First().Nickname);
        }

        [Test]
        public void AddBeneficiary_ShouldNotAddBeneficiary_WhenNicknameIsTooLong()
        {
            // Arrange
            string nickname = new string('a', 21);

            // Act
            bool result = _topUpService.AddBeneficiary(nickname);
            var beneficiaries = _topUpService.GetBeneficiaries();

            // Assert
            Assert.IsFalse(result);
            Assert.IsEmpty(beneficiaries);
        }

        [Test]
        public void AddBeneficiary_ShouldNotAddBeneficiary_WhenMaxBeneficiariesReached()
        {
            // Arrange
            for (int i = 0; i < 5; i++)
            {
                _topUpService.AddBeneficiary($"Beneficiary{i}");
            }

            // Act
            bool result = _topUpService.AddBeneficiary("ExtraBeneficiary");
            var beneficiaries = _topUpService.GetBeneficiaries();

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(5, beneficiaries.Count);
        }

        [Test]
        public void TopUp_ShouldSucceed_ForValidTopUp()
        {
            // Arrange
            _topUpService.AddBeneficiary("TestBeneficiary");
            var beneficiary = _topUpService.GetBeneficiaries().First();
            decimal initialBalance = 1000m;
            decimal topUpAmount = 100m;
            decimal remainingBalance = initialBalance;
            bool isVerified = true;

            // Act
            bool result = _topUpService.TopUp(beneficiary.Id, topUpAmount, isVerified, initialBalance, ref remainingBalance);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(initialBalance - topUpAmount - 1, remainingBalance);
            Assert.AreEqual(topUpAmount, beneficiary.MonthlyTopUpAmount);
        }

        [Test]
        public void TopUp_ShouldFail_WhenUserBalanceIsInsufficient()
        {
            // Arrange
            _topUpService.AddBeneficiary("TestBeneficiary");
            var beneficiary = _topUpService.GetBeneficiaries().First();
            decimal initialBalance = 50m;
            decimal topUpAmount = 100m;
            decimal remainingBalance = initialBalance;
            bool isVerified = true;

            // Act
            bool result = _topUpService.TopUp(beneficiary.Id, topUpAmount, isVerified, initialBalance, ref remainingBalance);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(initialBalance, remainingBalance);
            Assert.AreEqual(0, beneficiary.MonthlyTopUpAmount);
        }

        [Test]
        public void TopUp_ShouldFail_WhenMonthlyLimitForUnverifiedUserExceeded()
        {
            // Arrange
            _topUpService.AddBeneficiary("TestBeneficiary");
            var beneficiary = _topUpService.GetBeneficiaries().First();
            decimal initialBalance = 2000m;
            decimal topUpAmount = 1100m;
            decimal remainingBalance = initialBalance;
            bool isVerified = false;

            // Act
            bool result = _topUpService.TopUp(beneficiary.Id, topUpAmount, isVerified, initialBalance, ref remainingBalance);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(initialBalance, remainingBalance);
            Assert.AreEqual(0, beneficiary.MonthlyTopUpAmount);
        }

        [Test]
        public void TopUp_ShouldFail_WhenMonthlyLimitForVerifiedUserExceeded()
        {
            // Arrange
            _topUpService.AddBeneficiary("TestBeneficiary");
            var beneficiary = _topUpService.GetBeneficiaries().First();
            decimal initialBalance = 2000m;
            decimal topUpAmount = 600m;
            decimal remainingBalance = initialBalance;
            bool isVerified = true;

            // Act
            bool result = _topUpService.TopUp(beneficiary.Id, topUpAmount, isVerified, initialBalance, ref remainingBalance);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(initialBalance, remainingBalance);
            Assert.AreEqual(0, beneficiary.MonthlyTopUpAmount);
        }

        [Test]
        public void TopUp_ShouldFail_WhenTotalMonthlyTopUpLimitExceeded()
        {
            // Arrange
            for (int i = 0; i < 5; i++)
            {
                _topUpService.AddBeneficiary($"Beneficiary{i}");
            }
            var beneficiaries = _topUpService.GetBeneficiaries();
            decimal initialBalance = 4000m;
            decimal remainingBalance = initialBalance;
            bool isVerified = true;

            foreach (var beneficiary in beneficiaries)
            {
                _topUpService.TopUp(beneficiary.Id, 600m, isVerified, remainingBalance, ref remainingBalance);
            }

            // Act
            bool result = _topUpService.TopUp(beneficiaries.First().Id, 100m, isVerified, remainingBalance, ref remainingBalance);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(initialBalance - 3005m, remainingBalance); // 3000 for top-ups + 5 for transaction fees
        }
    }
}
