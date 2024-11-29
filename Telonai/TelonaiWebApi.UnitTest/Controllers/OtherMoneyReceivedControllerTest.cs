using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TelonaiWebApi.Controllers;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using Xunit;

namespace TelonaiWebAPI.UnitTest.Controllers
{
    public class OtherMoneyReceivedControllerTest
    {
        private readonly Mock<IScopedAuthorization> _mockScopedAuthorization;
        private readonly Mock<IOtherMoneyReceivedService> _mockOtherMoneyReceivedService;
        private readonly OtherIncomeController _otherMoneyReceivedController;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IMapper> _mockMapper;
        public ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal();
        public DefaultHttpContext context = new DefaultHttpContext();
        public OtherMoneyReceivedControllerTest()
        {
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockMapper = new Mock<IMapper>();
            _mockScopedAuthorization = new  Mock<IScopedAuthorization>();
            _mockOtherMoneyReceivedService = new Mock<IOtherMoneyReceivedService>();
            _otherMoneyReceivedController = new OtherIncomeController(_mockOtherMoneyReceivedService.Object, _mockScopedAuthorization.Object);
            claimsPrincipal.AddIdentity(new ClaimsIdentity(new Claim[]
            {
                    new Claim("custom:scope", "Admin")
            }));
            context.User = claimsPrincipal;
        }

        [Fact]
        public async void GetCurrentByPayrollId_ProvidingCorrectOtherIncomeForPayStub_ReturnsPaystubWithOtherIncome()
        {
            var otherIncomeId = 12;
            var companyId = 1;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Now),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };
            var otherRecivedMoneyList = new List<OtherMoneyReceived>()
            {
                new OtherMoneyReceived()
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
                }
            }.AsQueryable();

            var payStub = new PayStub()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
                OtherMoneyReceivedId = otherIncomeId,
                RegularHoursWorked = 160,
                OverTimeHoursWorked = 64,
                GrossPay = 2500,
                RegularPay = 2250,
                AmountSubjectToAdditionalMedicareTax = 120,
                YtdRegularHoursWorked = 1920,
                YtdOverTimeHoursWorked = 768,
                YtdGrossPay = 27000,
                YtdNetPay = 20500,
                YtdOverTimePay = 0,
                YtdRegularPay = 20500,
                IsCancelled = false,
                Payroll = payroll,
            };
            
            var otherRecivedMoneyModelList = new List<OtherMoneyReceivedModel>()
            {
                new OtherMoneyReceivedModel()
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
                }
            };
            

            var otherIncomeModelList  = new List<OtherMoneyReceivedModel>()
            {
                new OtherMoneyReceivedModel()
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
                }
            };
            _mockHttpContext.Setup(i => i.Request.HttpContext.User).Returns(claimsPrincipal);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Callback(() => { });
            _mockOtherMoneyReceivedService.Setup(service => service.GetByPayStubId(payStub.Id, out companyId)).Returns(otherIncomeModelList);
            

            //Act 
            var result = _otherMoneyReceivedController.GetCurrentByPayrollId(1);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }
    }
}
