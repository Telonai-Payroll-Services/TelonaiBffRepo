using Amazon.Auth.AccessControlPolicy;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using TelonaiWebApi.Controllers;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using Xunit;

namespace TelonaiWebAPI.UnitTest.Controllers
{
    public  class PayStubControllerTest
    {
        private readonly Mock<IScopedAuthorization> _mockScopedAuthorization;
        private readonly Mock<IPayStubService> _mockPayStubService;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly PayStubController _payStubController;
        private readonly Mock<IMapper> _mockMapper;
        public ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal();
        public DefaultHttpContext context = new DefaultHttpContext();
        private readonly Mock<PersonService> _personService;

        public PayStubControllerTest()
        {
            _mockScopedAuthorization = new Mock<IScopedAuthorization>();
            _mockPayStubService = new Mock<IPayStubService>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockMapper = new Mock<IMapper>();
            _personService = new Mock<PersonService>();
            _payStubController = new PayStubController(_mockPayStubService.Object, _mockScopedAuthorization.Object, _personService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
            claimsPrincipal.AddIdentity(new ClaimsIdentity(new Claim[]
            {
                    new Claim("custom:scope", "Admin")
            }));
            context.User = claimsPrincipal;
        }

        #region GetCurrntCompanyPayStub

        [Fact]
        public async Task GetCurrntCompanyPayStub_WhenPayStubExistWithProvided_ReturnsTheCurrentPaystubOfComapny()
        {
            //Arrange
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

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
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
                },
                new PayStub()
                {
                    Id = 2,
                    PayrollId = 12,
                    EmploymentId = 14,
                    OtherMoneyReceivedId = 25,
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
                },
                new PayStub()
                {
                    Id = 3,
                    PayrollId = 12,
                    EmploymentId = 15,
                    OtherMoneyReceivedId = 25,
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
                },
            }.AsQueryable();

            var payStubModel = new List<PayStubModel>()
            {
                new PayStubModel()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
                    RegularHoursWorked = 160,
                    OverTimeHoursWorked = 64,
                    GrossPay = 2500,
                    RegularPay = 2250
                }
            };

            
            _mockHttpContext.Setup(i => i.Request.HttpContext.User).Returns(claimsPrincipal);
            _mockScopedAuthorization.Setup(x => x.ValidateByJobId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Callback(() => { });
            _mockPayStubService.Setup(service => service.GetCurrentByCompanyId(companyId)).Returns(payStubModel);

            //Act 
            var result = _payStubController.GetCurrentPayStubs(companyId);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDocument = Assert.IsType<List<PayStubModel>>(okResult.Value);
            Assert.Equal(payStubModel.First(), returnedDocument.First());
        }

        [Fact]
        public async Task GetCurrntCompanyPayStub_WhenNonExistWithProvided_ReturnsNull()
        {
            //Arrange
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

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
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
                },
                new PayStub()
                {
                    Id = 2,
                    PayrollId = 12,
                    EmploymentId = 14,
                    OtherMoneyReceivedId = 25,
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
                },
                new PayStub()
                {
                    Id = 3,
                    PayrollId = 12,
                    EmploymentId = 15,
                    OtherMoneyReceivedId = 25,
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
                },
            }.AsQueryable();

            var payStubModel = new List<PayStubModel>()
            {
                new PayStubModel()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
                    RegularHoursWorked = 160,
                    OverTimeHoursWorked = 64,
                    GrossPay = 2500,
                    RegularPay = 2250
                }
            };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                    new Claim("custom:scope", "Admin")
            }));
            var context = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            _mockHttpContext.Setup(i => i.Request.HttpContext.User).Returns(claimsPrincipal);
            _mockScopedAuthorization.Setup(x => x.ValidateByJobId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Callback(() => { });
            _mockPayStubService.Setup(service => service.GetCurrentByCompanyId(companyId)).Returns(payStubModel);

            //Act 
            var result = _payStubController.GetCurrentPayStubs(290);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        #endregion

        #region GetCurrent By JobId And PersonId

        [Fact]
        public async Task GetCurrentByJobIdAndPersonIdb_WhenPayStubExistWithProvidedCompanyIdAndPersonId_ReturnsTheCurrentPaystubOfComapny()
        {
            var companyId = 1;
            var presonId = 21;
            var jobId = 20;

            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Now),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var employment = new Employment()
            {
                Id = 20,
                PersonId = presonId,
                JobId = jobId,
                PayRateBasisId = 11,
                PayRate = 90,
                IsPayrollAdmin = false,
                IsSalariedOvertimeExempt = false,
                IsTenNinetyNine = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                Job = new Job()
                {
                    Id = jobId,
                    CompanyId = companyId,
                    AddressLine1 = "Bole Getu Commercial Center",
                    AddressLine2 = "Kebede Building",
                    AddressLine3 = "Second Floor",
                    ZipcodeId = 232
                }
            };

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
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
                    Employment = employment,
                },
                new PayStub()
                {
                    Id = 2,
                    PayrollId = 12,
                    EmploymentId = 14,
                    OtherMoneyReceivedId = 25,
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
                    Employment = employment,
                },
                new PayStub()
                {
                    Id = 3,
                    PayrollId = 12,
                    EmploymentId = 15,
                    OtherMoneyReceivedId = 25,
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
                    Employment = employment
                },
            };
            var payStubModel = new List<PayStubModel>()
            {
                new PayStubModel()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
                    RegularHoursWorked = 160,
                    OverTimeHoursWorked = 64,
                    GrossPay = 2500,
                    RegularPay = 2250
                }
            };
            _mockHttpContext.Setup(i => i.Request.HttpContext.User).Returns(claimsPrincipal);
            _mockScopedAuthorization.Setup(x => x.ValidateByJobId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Callback(() => { });
            _mockPayStubService.Setup(service => service.GetCurrentByCompanyIdAndPersonId(companyId, presonId)).Returns(payStubModel);

            //Act 
            var result = _payStubController.GetCurrentByCompanyAndPersonId(companyId, presonId);
            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDocument = Assert.IsType<List<PayStubModel>>(okResult.Value);
            Assert.Equal(payStubModel.First(), returnedDocument.First());
        }

        [Fact]
        public async Task GetCurrentByJobIdAndPersonIdb_PayStubExistButProvidedNonExistingProvidedCompanyIdAndPersonId_ReturnsNull()
        {
            var companyId = 1;
            var presonId = 21;
            var jobId = 20;

            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Now),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var employment = new Employment()
            {
                Id = 20,
                PersonId = presonId,
                JobId = jobId,
                PayRateBasisId = 11,
                PayRate = 90,
                IsPayrollAdmin = false,
                IsSalariedOvertimeExempt = false,
                IsTenNinetyNine = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                Job = new Job()
                {
                    Id = jobId,
                    CompanyId = companyId,
                    AddressLine1 = "Bole Getu Commercial Center",
                    AddressLine2 = "Kebede Building",
                    AddressLine3 = "Second Floor",
                    ZipcodeId = 232
                }
            };

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
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
                    Employment = employment,
                },
                new PayStub()
                {
                    Id = 2,
                    PayrollId = 12,
                    EmploymentId = 14,
                    OtherMoneyReceivedId = 25,
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
                    Employment = employment,
                },
                new PayStub()
                {
                    Id = 3,
                    PayrollId = 12,
                    EmploymentId = 15,
                    OtherMoneyReceivedId = 25,
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
                    Employment = employment
                },
            };
            var payStubModel = new List<PayStubModel>()
            {
                new PayStubModel()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
                    RegularHoursWorked = 160,
                    OverTimeHoursWorked = 64,
                    GrossPay = 2500,
                    RegularPay = 2250
                }
            };
            _mockHttpContext.Setup(i => i.Request.HttpContext.User).Returns(claimsPrincipal);
            _mockScopedAuthorization.Setup(x => x.ValidateByJobId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Callback(() => { });
            _mockPayStubService.Setup(service => service.GetCurrentByCompanyIdAndPersonId(companyId, presonId)).Returns(payStubModel);

            //Act 
            var result = _payStubController.GetCurrentByCompanyAndPersonId(It.IsAny<int>(), It.IsAny<int>());

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        #endregion

        #region Get Time For User

        [Fact]
        public async void GetByTimeForUser_WhenProvidingExistingCompanyIdAndPayStubStartAndEndDate_ReturnsPayStubWithDuration()
        {
            var companyId = 1;
            var personId = 21;
            var jobId = 20;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Today),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var employment = new Employment()
            {
                Id = 20,
                PersonId = personId,
                JobId = jobId,
                PayRateBasisId = 11,
                PayRate = 90,
                IsPayrollAdmin = false,
                IsSalariedOvertimeExempt = false,
                IsTenNinetyNine = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                Job = new Job()
                {
                    Id = jobId,
                    CompanyId = companyId,
                    AddressLine1 = "Bole Getu Commercial Center",
                    AddressLine2 = "Kebede Building",
                    AddressLine3 = "Second Floor",
                    ZipcodeId = 232
                }
            };

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
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
                    Employment = employment,
                },
                new PayStub()
                {
                    Id = 2,
                    PayrollId = 12,
                    EmploymentId = 14,
                    OtherMoneyReceivedId = 25,
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
                    Employment = employment,
                },
                new PayStub()
                {
                    Id = 3,
                    PayrollId = 12,
                    EmploymentId = 15,
                    OtherMoneyReceivedId = 25,
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
                    Employment = employment
                },
            };

            var payStubModel = new List<PayStubModel>()
            {
                new PayStubModel()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
                    RegularHoursWorked = 160,
                    OverTimeHoursWorked = 64,
                    GrossPay = 2500,
                    RegularPay = 2250
                }
            };

            _mockHttpContext.Setup(i => i.Request.HttpContext.User).Returns(claimsPrincipal);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Callback(() => { });
            DateOnly from = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
            DateOnly to = DateOnly.FromDateTime(DateTime.Today);
            _mockPayStubService.Setup(service => service.GetReport(companyId, from, to)).Returns(payStubModel);

            //Act 
            var result = _payStubController.GetByTimeForUser(companyId, from, to);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDocument = Assert.IsType<List<PayStubModel>>(okResult.Value);
            Assert.Equal(payStubModel.First(), returnedDocument.First());
        }

        [Fact]
        public async void GetByTimeForUser_WhenProvidingNonExistingCompanyIdAndPayStubStartAndEndDate_ReturnsNull()
        {
            var companyId = 1;
            var personId = 21;
            var jobId = 20;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Today),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var employment = new Employment()
            {
                Id = 20,
                PersonId = personId,
                JobId = jobId,
                PayRateBasisId = 11,
                PayRate = 90,
                IsPayrollAdmin = false,
                IsSalariedOvertimeExempt = false,
                IsTenNinetyNine = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                Job = new Job()
                {
                    Id = jobId,
                    CompanyId = companyId,
                    AddressLine1 = "Bole Getu Commercial Center",
                    AddressLine2 = "Kebede Building",
                    AddressLine3 = "Second Floor",
                    ZipcodeId = 232
                }
            };

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
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
                    Employment = employment,
                },
                new PayStub()
                {
                    Id = 2,
                    PayrollId = 12,
                    EmploymentId = 14,
                    OtherMoneyReceivedId = 25,
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
                    Employment = employment,
                },
                new PayStub()
                {
                    Id = 3,
                    PayrollId = 12,
                    EmploymentId = 15,
                    OtherMoneyReceivedId = 25,
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
                    Employment = employment
                },
            };

            var payStubModel = new List<PayStubModel>()
            {
                new PayStubModel()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
                    RegularHoursWorked = 160,
                    OverTimeHoursWorked = 64,
                    GrossPay = 2500,
                    RegularPay = 2250
                }
            };

            _mockHttpContext.Setup(i => i.Request.HttpContext.User).Returns(claimsPrincipal);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Callback(() => { });
            DateOnly from = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
            DateOnly to = DateOnly.FromDateTime(DateTime.Today);

            //Act 
            var result = _payStubController.GetByTimeForUser(It.IsAny<int>(), from, to);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        [Fact]
        public async void GetByTimeForUser_WhenProvidingNonExistingCompanyIdAndPayStubNotInDuration_ReturnsNull()
        {
            var companyId = 1;
            var personId = 21;
            var jobId = 20;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Today),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var employment = new Employment()
            {
                Id = 20,
                PersonId = personId,
                JobId = jobId,
                PayRateBasisId = 11,
                PayRate = 90,
                IsPayrollAdmin = false,
                IsSalariedOvertimeExempt = false,
                IsTenNinetyNine = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                Job = new Job()
                {
                    Id = jobId,
                    CompanyId = companyId,
                    AddressLine1 = "Bole Getu Commercial Center",
                    AddressLine2 = "Kebede Building",
                    AddressLine3 = "Second Floor",
                    ZipcodeId = 232
                }
            };

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
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
                    Employment = employment,
                },
                new PayStub()
                {
                    Id = 2,
                    PayrollId = 12,
                    EmploymentId = 14,
                    OtherMoneyReceivedId = 25,
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
                    Employment = employment,
                },
                new PayStub()
                {
                    Id = 3,
                    PayrollId = 12,
                    EmploymentId = 15,
                    OtherMoneyReceivedId = 25,
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
                    Employment = employment
                },
            };

            var payStubModel = new List<PayStubModel>()
            {
                new PayStubModel()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
                    RegularHoursWorked = 160,
                    OverTimeHoursWorked = 64,
                    GrossPay = 2500,
                    RegularPay = 2250
                }
            };

            _mockHttpContext.Setup(i => i.Request.HttpContext.User).Returns(claimsPrincipal);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Callback(() => { });
            DateOnly from = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
            DateOnly to = DateOnly.FromDateTime(DateTime.Today);
            _mockPayStubService.Setup(service => service.GetReport(companyId, from, to)).Returns(payStubModel);

            //Act 
            DateOnly fromParam = DateOnly.FromDateTime(DateTime.Today);
            DateOnly toParm = DateOnly.FromDateTime(DateTime.Today.AddDays(10));
            var result = _payStubController.GetByTimeForUser(companyId, fromParam, toParm);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        #endregion

        #region Get Current By PayrollId

        [Fact]
        public async void GetCurrentByPayrollId_WhenTherIsPayStubForSpecificPayroll_ReturnsPayStubUnderPayroll()
        {
            //Arrange
            var companyId = 1;
            var payrollId = 10;  
            var payroll = new Payroll()
            {
                Id = payrollId,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Now),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
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
                },
                new PayStub()
                {
                    Id = 2,
                    PayrollId = 12,
                    EmploymentId = 14,
                    OtherMoneyReceivedId = 25,
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
                },
                new PayStub()
                {
                    Id = 3,
                    PayrollId = 12,
                    EmploymentId = 15,
                    OtherMoneyReceivedId = 25,
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
                },
            };

            var payrollModel = new PayrollModel()
            {
                Id = payrollId,
                CompanyId = companyId, 
                
            };

            var payStubModel = new List<PayStubModel>()
            {
                new PayStubModel()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
                    RegularHoursWorked = 160,
                    OverTimeHoursWorked = 64,
                    GrossPay = 2500,
                    RegularPay = 2250,
                    Payroll = payrollModel
                }
            };


            _mockHttpContext.Setup(i => i.Request.HttpContext.User).Returns(claimsPrincipal);
            _mockPayStubService.Setup(service => service.GetCurrentByPayrollId(payrollId)).Returns(payStubModel);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, companyId)).Callback(() => { });
           
            //Act
            var result = _payStubController.GetCurrentByPayrollId(payrollId);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDocument = Assert.IsType<List<PayStubModel>>(okResult.Value);
            Assert.Equal(payStubModel.First(), returnedDocument.First());
        }
        //[Fact]
        //public async void GetCurrentByPayrollId_WhenTherIsNOPayStubForSpecificPayroll_ReturnsNull()
        //{
        //    //Arrange
        //    var companyId = 1;
        //    var payrollId = 10;
        //    var payroll = new Payroll()
        //    {
        //        Id = payrollId,
        //        CompanyId = companyId,
        //        PayrollScheduleId = 2,
        //        ScheduledRunDate = DateOnly.FromDateTime(DateTime.Now),
        //        StartDate = DateOnly.FromDateTime(DateTime.Now),
        //        TrueRunDate = DateTime.Now
        //    };

        //    var payStubs = new List<PayStub>()
        //    {
        //        new PayStub()
        //        {
        //            Id = 1,
        //            PayrollId = 12,
        //            EmploymentId = 13,
        //            OtherMoneyReceivedId = 25,
        //            RegularHoursWorked = 160,
        //            OverTimeHoursWorked = 64,
        //            GrossPay = 2500,
        //            RegularPay = 2250,
        //            AmountSubjectToAdditionalMedicareTax = 120,
        //            YtdRegularHoursWorked = 1920,
        //            YtdOverTimeHoursWorked = 768,
        //            YtdGrossPay = 27000,
        //            YtdNetPay = 20500,
        //            YtdOverTimePay = 0,
        //            YtdRegularPay = 20500,
        //            IsCancelled = false,
        //            Payroll = payroll,
        //        },
        //        new PayStub()
        //        {
        //            Id = 2,
        //            PayrollId = 12,
        //            EmploymentId = 14,
        //            OtherMoneyReceivedId = 25,
        //            RegularHoursWorked = 160,
        //            OverTimeHoursWorked = 64,
        //            GrossPay = 2500,
        //            RegularPay = 2250,
        //            AmountSubjectToAdditionalMedicareTax = 120,
        //            YtdRegularHoursWorked = 1920,
        //            YtdOverTimeHoursWorked = 768,
        //            YtdGrossPay = 27000,
        //            YtdNetPay = 20500,
        //            YtdOverTimePay = 0,
        //            YtdRegularPay = 20500,
        //            IsCancelled = false,
        //            Payroll = payroll,
        //        },
        //        new PayStub()
        //        {
        //            Id = 3,
        //            PayrollId = 12,
        //            EmploymentId = 15,
        //            OtherMoneyReceivedId = 25,
        //            RegularHoursWorked = 160,
        //            OverTimeHoursWorked = 64,
        //            GrossPay = 2500,
        //            RegularPay = 2250,
        //            AmountSubjectToAdditionalMedicareTax = 120,
        //            YtdRegularHoursWorked = 1920,
        //            YtdOverTimeHoursWorked = 768,
        //            YtdGrossPay = 27000,
        //            YtdNetPay = 20500,
        //            YtdOverTimePay = 0,
        //            YtdRegularPay = 20500,
        //            IsCancelled = false,
        //            Payroll = payroll,
        //        },
        //    };

        //    var payrollModel = new PayrollModel()
        //    {
        //        Id = payrollId,
        //        CompanyId = companyId,
        //        PayrollSchedule = new PayrollScheduleModel()
        //        {
        //            Id = 12,
        //            CompanyId = 123,
        //            Compnay = "birascomputing",
        //            PayrollScheduleType = "Monthly",
        //            StartDate = DateTime.Today,
        //            FirstRunDate = DateTime.Today,
        //        }
        //    };

        //    var payStubModel = new List<PayStubModel>()
        //    {
        //        new PayStubModel()
        //        {
        //            Id = 1,
        //            PayrollId = 12,
        //            EmploymentId = 13,
        //            OtherMoneyReceivedId = 25,
        //            RegularHoursWorked = 160,
        //            OverTimeHoursWorked = 64,
        //            GrossPay = 2500,
        //            RegularPay = 2250,
        //            Payroll = payrollModel
        //        }
        //    };

        //    _mockHttpContext.Setup(i => i.Request.HttpContext.User).Returns(claimsPrincipal);
        //    _mockPayStubService.Setup(service => service.GetCurrentByPayrollId(payrollId)).Returns(payStubModel);
        //    _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, companyId)).Callback(() => { });

        //    //Act
        //    var result = _payStubController.GetCurrentByPayrollId(78);

        //    //Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    Assert.Null(okResult.Value);
        //}

        #endregion

        #region Get PayStub by id

        [Fact]
        public async Task GetById_WhenProvidingExistingPayStubId_ReturnsPayStubDetail()
        {
            //Arrange
            var companyId = 1;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = 1,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Today),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var payStub = new PayStub()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
                OtherMoneyReceivedId = 25,
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

            _mockHttpContext.Setup(i => i.Request.HttpContext.User).Returns(claimsPrincipal);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Callback(() => { });
            _mockPayStubService.Setup(service => service.GetById(1)).Returns(payStub);

            //Act
            var result = _payStubController.GetById(1);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDocument = Assert.IsType<PayStub>(okResult.Value);
            Assert.Equal(payStub, returnedDocument);
        }

        #endregion

        #region Update Paystub

        [Fact]
        public async Task Update_ExistingPayStub_ReturnsUpdated()
        {
            //Arrange
            var companyId = 1;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = 1,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Today),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var payStub = new PayStub()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
                OtherMoneyReceivedId = 25,
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

            var payStubModel = new PayStubModel()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
                OtherMoneyReceivedId = 25,
                RegularHoursWorked = 160,
                OverTimeHoursWorked = 64,
                GrossPay = 2500,
                RegularPay = 2250
            };

            _mockHttpContext.Setup(i => i.Request.HttpContext.User).Returns(claimsPrincipal);
            _mockMapper.Setup(m => m.Map<PayStub>(It.IsAny<PayStubModel>())).Returns(payStub);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Callback(() => { });
            _mockPayStubService.Setup(service => service.GetById(1)).Returns(payStub);

            //Act
            var result = _payStubController.Update(1, payStubModel);

            //Assert
            Assert.IsType<OkObjectResult>(result);
            _mockPayStubService.Verify(service => service.Update(1, payStubModel), Times.Once);
        }

        [Fact]
        public async Task Update_NonExistingPayStub_ReturnsNotFound()
        {
            //Arrange
            var companyId = 1;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = 1,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Today),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var payStub = new PayStub()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
                OtherMoneyReceivedId = 25,
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

            var payStubModel = new PayStubModel()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
                OtherMoneyReceivedId = 25,
                RegularHoursWorked = 160,
                OverTimeHoursWorked = 64,
                GrossPay = 2500,
                RegularPay = 2250
            };

            _mockHttpContext.Setup(i => i.Request.HttpContext.User).Returns(claimsPrincipal);
            _mockMapper.Setup(m => m.Map<PayStub>(It.IsAny<PayStubModel>())).Returns(payStub);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Callback(() => { });
            _mockPayStubService.Setup(service => service.GetById(19)).Returns(payStub);

            //Act
            var result = _payStubController.Update(1, payStubModel);

            //Assert
            Assert.IsType<NotFoundResult>(result);

        }

        #endregion

        #region

        [Fact]
        public async Task Delete_ExistingPayStub_ReturnsOKStatus()
        {
            //Arrange
            var payStub = new PayStub()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
                OtherMoneyReceivedId = 25,
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
            };
            _mockPayStubService.Setup(service => service.GetById(1)).Returns(payStub);

            //Act
            var result = _payStubController.Delete(1);

            //Assert
            Assert.IsType<OkObjectResult>(result);
            _mockPayStubService.Verify(service => service.Delete(1), Times.Once);
        }

        [Fact]
        public async Task Delete_NotExistingPayStub_ReturnsNotFound()
        {
            //Arrange
            var payStub = new PayStub()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
                OtherMoneyReceivedId = 25,
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
            };
            _mockPayStubService.Setup(service => service.GetById(19)).Returns(payStub);

            //Act
            var result = _payStubController.Delete(19);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion
    }
}
