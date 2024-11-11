using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using Xunit;

namespace TelonaiWebAPI.UnitTest.Services
{
    public class OtherMoneyReceivedServiceTest
    {
        private readonly Mock<DataContext> _mockDataContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IScopedAuthorization> _mockScopedAuthorization;
        private readonly OtherMoneyReceivedService _mockOtherMoneyReceivedService;

        public OtherMoneyReceivedServiceTest()
        {
            _mockDataContext = new Mock<DataContext>();
            _mockMapper = new Mock<IMapper>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockScopedAuthorization = new Mock<IScopedAuthorization>();
            _mockOtherMoneyReceivedService = new OtherMoneyReceivedService(_mockDataContext.Object, _mockMapper.Object, _mockHttpContextAccessor.Object, _mockScopedAuthorization.Object);
        }

        #region Get Other Money Received By Id

        [Fact]
        public async void GetById_whenOtherIncomeExistWithIdProvided_ReturnsOtherIncomeDetails()
        {
            int otherIncomeId = 1;
            var otherIncome = new OtherMoneyReceived()
            {
                Id = otherIncomeId,
                IsCancelled = false,
                CreditCardTips = 0,
                YtdCreditCardTips = 32,
                AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                CashTips = 200,
                YtdCashTips = 200,
                Reimbursement = 100,
                YtdReimbursement = 100,
            };
            var otherIncomeList = new List<OtherMoneyReceived> {
                otherIncome,
            }.AsQueryable();

            var otherIncomeModel = new OtherMoneyReceivedModel()
            {
                Id = 1,
                IsCancelled = false,
                CreditCardTips = 0,
                YtdCreditCardTips = 32,
                AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                CashTips = 200,
                YtdCashTips = 200,
                Reimbursement = 100,
                YtdReimbursement = 100,
                AdditionalOtherMoneyReceived = new List<AdditionalOtherMoneyReceived>()
                {
                    It.IsAny<AdditionalOtherMoneyReceived>()
                }
            };

            var mockSet = new Mock<DbSet<OtherMoneyReceived>>();
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.Provider).Returns(otherIncomeList.Provider);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.Expression).Returns(otherIncomeList.Expression);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.ElementType).Returns(otherIncomeList.ElementType);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.GetEnumerator()).Returns(otherIncomeList.GetEnumerator());

            _mockDataContext.Setup(c => c.OtherMoneyReceived).Returns(mockSet.Object);
            _mockMapper.Setup(m => m.Map<OtherMoneyReceivedModel>(It.IsAny<OtherMoneyReceived>())).Returns(otherIncomeModel);
            
            // Act
            var result = _mockOtherMoneyReceivedService.GetById(otherIncomeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(otherIncomeId, result.Id);
        }

        [Fact]
        public async void GetById_WhenSendingNonExistingIdAsParameter_ReturnsNull()
        {
            int otherIncomeId = 1;
            var otherIncome = new OtherMoneyReceived()
            {
                Id = otherIncomeId,
                IsCancelled = false,
                CreditCardTips = 0,
                YtdCreditCardTips = 32,
                AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                CashTips = 200,
                YtdCashTips = 200,
                Reimbursement = 100,
                YtdReimbursement = 100,
            };
            var otherIncomeList = new List<OtherMoneyReceived> {
                otherIncome,
            }.AsQueryable();

            var otherIncomeModel = new OtherMoneyReceivedModel()
            {
                Id = 1,
                IsCancelled = false,
                CreditCardTips = 0,
                YtdCreditCardTips = 32,
                AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                CashTips = 200,
                YtdCashTips = 200,
                Reimbursement = 100,
                YtdReimbursement = 100,
                AdditionalOtherMoneyReceived = new List<AdditionalOtherMoneyReceived>()
                {
                    It.IsAny<AdditionalOtherMoneyReceived>()
                }
            };

            var mockSet = new Mock<DbSet<OtherMoneyReceived>>();
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.Provider).Returns(otherIncomeList.Provider);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.Expression).Returns(otherIncomeList.Expression);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.ElementType).Returns(otherIncomeList.ElementType);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.GetEnumerator()).Returns(otherIncomeList.GetEnumerator());

            _mockDataContext.Setup(c => c.OtherMoneyReceived).Returns(mockSet.Object);
            _mockMapper.Setup(m => m.Map<OtherMoneyReceivedModel>(It.IsAny<OtherMoneyReceived>())).Returns(otherIncomeModel);

            // Act
            var result = _mockOtherMoneyReceivedService.GetById(12);

            // Assert
            Assert.Null(result);
        }
        
        #endregion
    }
}
