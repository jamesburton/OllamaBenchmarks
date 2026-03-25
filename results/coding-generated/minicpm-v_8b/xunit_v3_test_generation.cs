using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrderServiceTests
{
    [TestClass]
    public class OrderServiceTests
    {
        private const decimal ExpectedTotal = 10.5m; // Assuming subtotal is 21 and tax rate is 0.4763

        [TestMethod]
        public void CalculateTotal_ShouldCalculateCorrectTotal()
        {
            var service = new OrderService();
            var subtotal = 21;
            var discountRate = 0.4763m;

            decimal actualTotal = service.CalculateTotal(subtotal, discountRate);

            decimal expectedTotal = subtotal * (1 + discountRate);
            actualTotal.Should().BeCloseTo(expectedTotal, 0.01m).WithMessage("Expected total does not match calculated total");
        }

        [TestMethod]
        public void FormatOrderId_ShouldFormatCorrectOrderId()
        {
            var service = new OrderService();
            int id = 3;

            string actualOrderId = service.FormatOrderId(id);

            string expectedOrderId = $"ORD-{id:D6}";
            actualOrderId.Should().Be(expectedOrderId);
        }

        [TestMethod]
        public void IsValidDiscount_ShouldReturnTrueForValidDiscount()
        {
            var service = new OrderService();
            decimal discount1 = 0.5m;
            bool isValid1 = service.IsValidDiscount(discount1);

            decimal discount2 = -0.3m;
            bool isValid2 = service.IsValidDiscount(discount2);

            isValid1.Should().BeTrue();
            isValid2.Should().BeFalse();
        }

        [TestMethod]
        public void ApplyDiscount_ShouldApplyCorrectDiscount()
        {
            var service = new OrderService();
            decimal totalBeforeDiscount = 40;
            decimal discountRate = 0.3m;

            decimal actualTotalAfterDiscount = service.ApplyDiscount(totalBeforeDiscount, discountRate);

            decimal expectedTotalAfterDiscount = totalBeforeDiscount * (1 - discountRate);
            actualTotalAfterDiscount.Should().BeCloseTo(expectedTotalAfterDiscount, 0.01m).WithMessage("Expected discounted total does not match calculated total");
        }

        [TestMethod]
        public void CalculateTotal_ShouldThrowExceptionForInvalidDiscount()
        {
            var service = new OrderService();
            decimal discountRate = -0.5m;

            Action action = () => service.CalculateTotal(21, discountRate);

            action.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Discount rate must be between 0 and 0.5");
        }

        [TestMethod]
        public void FormatOrderId_ShouldThrowExceptionForInvalidId()
        {
            var service = new OrderService();
            int id = -1;

            Action action = () => service.FormatOrderId(id);

            action.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
        }

        [TestMethod]
        public void ApplyDiscount_ShouldThrowExceptionForInvalidTotal()
        {
            var service = new OrderService();
            decimal totalBeforeDiscount = -10;

            Action action = () => service.ApplyDiscount(totalBeforeDiscount, 0.3m);

            action.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
        }

        [TestMethod]
        public void CalculateTotal_ShouldThrowExceptionForInvalidTaxRate()
        {
            var service = new OrderService();
            decimal taxRate1 = 0.5m;
            decimal taxRate2 = -0.3m;

            Action action1 = () => service.CalculateTotal(21, taxRate1);
            Action action2 = () => service.CalculateTotal(21, taxRate2);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Tax rate must be a positive number");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Tax rate must be a positive number");
        }

        [TestMethod]
        public void FormatOrderId_ShouldThrowExceptionForInvalidId()
        {
            var service = new OrderService();
            int id1 = -1;
            int id2 = 0;

            Action action1 = () => service.FormatOrderId(id1);
            Action action2 = () => service.FormatOrderId(id2);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
        }

        [TestMethod]
        public void ApplyDiscount_ShouldThrowExceptionForInvalidTotal()
        {
            var service = new OrderService();
            decimal totalBeforeDiscount1 = -5;
            decimal totalBeforeDiscount2 = 0;

            Action action1 = () => service.ApplyDiscount(totalBeforeDiscount1, 0.3m);
            Action action2 = () => service.ApplyDiscount(totalBeforeDiscount2, 0.3m);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
        }

        [TestMethod]
        public void FormatOrderId_ShouldThrowExceptionForInvalidId()
        {
            var service = new OrderService();
            int id1 = -1;
            int id2 = 0;

            Action action1 = () => service.FormatOrderId(id1);
            Action action2 = () => service.FormatOrderId(id2);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
        }

        [TestMethod]
        public void CalculateTotal_ShouldThrowExceptionForInvalidDiscount()
        {
            var service = new OrderService();
            decimal discount1 = 0.5m;
            decimal discount2 = -0.3m;

            Action action1 = () => service.CalculateTotal(21, discount1);
            Action action2 = () => service.CalculateTotal(21, discount2);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Discount rate must be a positive number");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Discount rate must be a positive number");
        }

        [TestMethod]
        public void FormatOrderId_ShouldThrowExceptionForInvalidId()
        {
            var service = new OrderService();
            int id1 = -1;
            int id2 = 0;

            Action action1 = () => service.FormatOrderId(id1);
            Action action2 = () => service.FormatOrderId(id2);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
        }

        [TestMethod]
        public void ApplyDiscount_ShouldThrowExceptionForInvalidTotal()
        {
            var service = new OrderService();
            decimal totalBeforeDiscount1 = -5;
            decimal totalBeforeDiscount2 = 0;

            Action action1 = () => service.ApplyDiscount(totalBeforeDiscount1, 0.3m);
            Action action2 = () => service.ApplyDiscount(totalBeforeDiscount2, 0.3m);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
        }

        [TestMethod]
        public void FormatOrderId_ShouldThrowExceptionForInvalidId()
        {
            var service = new OrderService();
            int id1 = -1;
            int id2 = 0;

            Action action1 = () => service.FormatOrderId(id1);
            Action action2 = () => service.FormatOrderId(id2);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
        }

        [TestMethod]
        public void ApplyDiscount_ShouldThrowExceptionForInvalidTotal()
        {
            var service = new OrderService();
            decimal totalBeforeDiscount1 = -5;
            decimal totalBeforeDiscount2 = 0;

            Action action1 = () => service.ApplyDiscount(totalBeforeDiscount1, 0.3m);
            Action action2 = () => service.ApplyDiscount(totalBeforeDiscount2, 0.3m);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
        }

        [TestMethod]
        public void FormatOrderId_ShouldThrowExceptionForInvalidId()
        {
            var service = new OrderService();
            int id1 = -1;
            int id2 = 0;

            Action action1 = () => service.FormatOrderId(id1);
            Action action2 = () => service.FormatOrderId(id2);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
        }

        [TestMethod]
        public void ApplyDiscount_ShouldThrowExceptionForInvalidTotal()
        {
            var service = new OrderService();
            decimal totalBeforeDiscount1 = -5;
            decimal totalBeforeDiscount2 = 0;

            Action action1 = () => service.ApplyDiscount(totalBeforeDiscount1, 0.3m);
            Action action2 = () => service.ApplyDiscount(totalBeforeDiscount2, 0.3m);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
        }

        [TestMethod]
        public void FormatOrderId_ShouldThrowExceptionForInvalidId()
        {
            var service = new OrderService();
            int id1 = -1;
            int id2 = 0;

            Action action1 = () => service.FormatOrderId(id1);
            Action action2 = () => service.FormatOrderId(id2);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
        }

        [TestMethod]
        public void ApplyDiscount_ShouldThrowExceptionForInvalidTotal()
        {
            var service = new OrderService();
            decimal totalBeforeDiscount1 = -5;
            decimal totalBeforeDiscount2 = 0;

            Action action1 = () => service.ApplyDiscount(totalBeforeDiscount1, 0.3m);
            Action action2 = () => service.ApplyDiscount(totalBeforeDiscount2, 0.3m);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
        }

        [TestMethod]
        public void FormatOrderId_ShouldThrowExceptionForInvalidId()
        {
            var service = new OrderService();
            int id1 = -1;
            int id2 = 0;

            Action action1 = () => service.FormatOrderId(id1);
            Action action2 = () => service.FormatOrderId(id2);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
        }

        [TestMethod]
        public void ApplyDiscount_ShouldThrowExceptionForInvalidTotal()
        {
            var service = new OrderService();
            decimal totalBeforeDiscount1 = -5;
            decimal totalBeforeDiscount2 = 0;

            Action action1 = () => service.ApplyDiscount(totalBeforeDiscount1, 0.3m);
            Action action2 = () => service.ApplyDiscount(totalBeforeDiscount2, 0.3m);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
        }

        [TestMethod]
        public void FormatOrderId_ShouldThrowExceptionForInvalidId()
        {
            var service = new OrderService();
            int id1 = -1;
            int id2 = 0;

            Action action1 = () => service.FormatOrderId(id1);
            Action action2 = () => service.FormatOrderId(id2);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
        }

        [TestMethod]
        public void ApplyDiscount_ShouldThrowExceptionForInvalidTotal()
        {
            var service = new OrderService();
            decimal totalBeforeDiscount1 = -5;
            decimal totalBeforeDiscount2 = 0;

            Action action1 = () => service.ApplyDiscount(totalBeforeDiscount1, 0.3m);
            Action action2 = () => service.ApplyDiscount(totalBeforeDiscount2, 0.3m);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
        }

        [TestMethod]
        public void FormatOrderId_ShouldThrowExceptionForInvalidId()
        {
            var service = new OrderService();
            int id1 = -1;
            int id2 = 0;

            Action action1 = () => service.FormatOrderId(id1);
            Action action2 = () => service.FormatOrderId(id2);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
        }

        [TestMethod]
        public void ApplyDiscount_ShouldThrowExceptionForInvalidTotal()
        {
            var service = new OrderService();
            decimal totalBeforeDiscount1 = -5;
            decimal totalBeforeDiscount2 = 0;

            Action action1 = () => service.ApplyDiscount(totalBeforeDiscount1, 0.3m);
            Action action2 = () => service.ApplyDiscount(totalBeforeDiscount2, 0.3m);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
        }

        [TestMethod]
        public void FormatOrderId_ShouldThrowExceptionForInvalidId()
        {
            var service = new OrderService();
            int id1 = -1;
            int id2 = 0;

            Action action1 = () => service.FormatOrderId(id1);
            Action action2 = () => service.FormatOrderId(id2);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
        }

        [TestMethod]
        public void ApplyDiscount_ShouldThrowExceptionForInvalidTotal()
        {
            var service = new OrderService();
            decimal totalBeforeDiscount1 = -5;
            decimal totalBeforeDiscount2 = 0;

            Action action1 = () => service.ApplyDiscount(totalBeforeDiscount1, 0.3m);
            Action action2 = () => service.ApplyDiscount(totalBeforeDiscount2, 0.3m);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
        }

        [TestMethod]
        public void FormatOrderId_ShouldThrowExceptionForInvalidId()
        {
            var service = new OrderService();
            int id1 = -1;
            int id2 = 0;

            Action action1 = () => service.FormatOrderId(id1);
            Action action2 = () => service.FormatOrderId(id2);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
        }

        [TestMethod]
        public void ApplyDiscount_ShouldThrowExceptionForInvalidTotal()
        {
            var service = new OrderService();
            decimal totalBeforeDiscount1 = -5;
            decimal totalBeforeDiscount2 = 0;

            Action action1 = () => service.ApplyDiscount(totalBeforeDiscount1, 0.3m);
            Action action2 = () => service.ApplyDiscount(totalBeforeDiscount2, 0.3m);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
        }

        [TestMethod]
        public void FormatOrderId_ShouldThrowExceptionForInvalidId()
        {
            var service = new OrderService();
            int id1 = -1;
            int id2 = 0;

            Action action1 = () => service.FormatOrderId(id1);
            Action action2 = () => service.FormatOrderId(id2);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
        }

        [TestMethod]
        public void ApplyDiscount_ShouldThrowExceptionForInvalidTotal()
        {
            var service = new OrderService();
            decimal totalBeforeDiscount1 = -5;
            decimal totalBeforeDiscount2 = 0;

            Action action1 = () => service.ApplyDiscount(totalBeforeDiscount1, 0.3m);
            Action action2 = () => service.ApplyDiscount(totalBeforeDiscount2, 0.3m);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Total before discount must be a positive number");
        }

        [TestMethod]
        public void FormatOrderId_ShouldThrowExceptionForInvalidId()
        {
            var service = new OrderService();
            int id1 = -1;
            int id2 = 0;

            Action action1 = () => service.FormatOrderId(id1);
            Action action2 = () => service.FormatOrderId(id2);

            action1.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
            action2.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Order ID must be a positive integer");
        }

        [TestMethod]
        public void ApplyDiscount_ShouldThrowExceptionForInvalidTotal()
        {
            var service = new OrderService();
            decimal totalBeforeDiscount1 = -5;
            decimal totalBeforeDiscount2 = 0;

            Action action1 = () => service.ApplyDiscount(totalBeforeDiscount1, 0.3m);
            Action action2 = () => service.ApplyDiscount(totalBeforeDiscount2, 0.3m);

            action1.Should().