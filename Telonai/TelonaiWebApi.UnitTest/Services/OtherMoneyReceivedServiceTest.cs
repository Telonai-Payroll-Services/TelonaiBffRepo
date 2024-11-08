using AutoMapper;
using Microsoft.AspNetCore.Http;
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
            _mockOtherMoneyReceivedService = new OtherMoneyReceivedService(_mockDataContext.Object,_mockMapper.Object, _mockHttpContextAccessor.Object, _mockScopedAuthorization.Object);
        }

        #region Get Other Money Received By Id

        public async void GetById_whenOtherIncomeExistWithIdProvided_ReturnsOtherIncomeDetails()
        {
            var otherIncome = new OtherMoneyReceived()
            {
                Id = 1,
                IsCancelled = false,
                CreditCardTips = 0,
                YtdCreditCardTips = 32,
                AdditionalOtherMoneyReceivedId = new[] { 12,67,300 },
                CashTips = 200,
                YtdCashTips = 200,
                Reimbursement = 100,
                YtdReimbursement = 100,
            };


        }

        #endregion
    }
}
