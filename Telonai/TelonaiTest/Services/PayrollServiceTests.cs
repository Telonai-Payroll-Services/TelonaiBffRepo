using AutoMapper;
using Moq;
using Microsoft.EntityFrameworkCore;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using System.Reflection;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;

public class PayrollServiceTests
{
    private readonly Mock<DataContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly PayrollService _payrollService;

    public PayrollServiceTests()
    {
        _mockContext = new Mock<DataContext>(MockBehavior.Default, new object[] { new Mock<IHttpContextAccessor>().Object });
        _mockMapper = new Mock<IMapper>();
        _payrollService = new PayrollService(_mockContext.Object, _mockMapper.Object);
    }

    [Fact]
    public void GetLatestByCount_ShouldReturnCorrectPayrollModels()
    {
       
        int companyId = 1;
        int count = 4;
        var payrolls = new List<Payroll>
        {
            new Payroll { CompanyId = companyId, ScheduledRunDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)), EmployeesOwed = 600 },
            new Payroll { CompanyId = companyId, ScheduledRunDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-11)), EmployeesOwed = 2400 },
            new Payroll { CompanyId = companyId, ScheduledRunDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)), StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-12)), EmployeesOwed = 3000 },
            new Payroll { CompanyId = companyId, ScheduledRunDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)), StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-12)), EmployeesOwed = 4000 }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrolls.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrolls.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrolls.ElementType);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(payrolls.GetEnumerator());

        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);
        _mockMapper.Setup(m => m.Map<List<PayrollModel>>(It.IsAny<List<Payroll>>())).Returns(new List<PayrollModel>
        {
            new PayrollModel { ScheduledRunDate = DateTime.UtcNow.AddDays(-1), EmployeesOwed = 600 },
            new PayrollModel { ScheduledRunDate = DateTime.UtcNow.AddDays(-2), EmployeesOwed = 2400 },
            new PayrollModel { ScheduledRunDate = DateTime.UtcNow.AddDays(-3), EmployeesOwed = 3000 },
            new PayrollModel { ScheduledRunDate = DateTime.UtcNow.AddDays(3), EmployeesOwed = 4000 }
        });

       
        var result = _payrollService.GetLatestByCount(companyId, count);
     
        Assert.Equal(4, result.Count);
        Assert.Equal("#3CA612", result[0].ExpenseTrackingHexColor);
        Assert.Equal("#FCCC44", result[1].ExpenseTrackingHexColor);
        Assert.Equal("#D20103", result[2].ExpenseTrackingHexColor);
        Assert.Equal("#D20103", result[3].ExpenseTrackingHexColor);
    }

    [Fact]
    public void GetDailyPayrollExpense_ShouldReturnCorrectDailyAmount()
    {
        double totalEmployeesOwed = 1000;
        DateTime startDate = DateTime.UtcNow.AddDays(-10);
        DateTime endDate = DateTime.UtcNow;
        var span = endDate - startDate;

        MethodInfo methodInfo = typeof(PayrollService).GetMethod("GetDailyPayrollExpense", BindingFlags.NonPublic | BindingFlags.Static);

        object result = methodInfo.Invoke(null, new object[] { totalEmployeesOwed, startDate, endDate });
        
        double expectedDailyAmount = totalEmployeesOwed / span.TotalDays;
        Assert.Equal(expectedDailyAmount, result);
    }
    [Fact]
    public void GetReport_ReturnsMappedPayrollModels_WhenDataExists()
    {
        int companyId = 1;
        DateOnly from = new DateOnly(2024, 10, 15);
        DateOnly to = new DateOnly(2024, 10, 20);

        var payrollData = new List<Payroll>
        {
            new Payroll { CompanyId = companyId, ScheduledRunDate = new DateOnly(2024, 10, 18) },
        }.AsQueryable(); ;
        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrollData.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrollData.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrollData.ElementType);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(payrollData.GetEnumerator());
       

        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);
       
        var expectedPayrollModels = new List<PayrollModel>
        {
            
            new PayrollModel(), 
        };
        _mockMapper.Setup(m => m.Map<List<PayrollModel>>(It.IsAny<List<Payroll>>())).Returns(expectedPayrollModels);

        var result = _payrollService.GetReport(companyId, from, to);

        Assert.Equal(expectedPayrollModels, result);
    }
    [Fact]
    public void GetReport_ReturnsNull_WhenNoDataExists()
    {
        int companyId = 1;
        DateOnly from = new DateOnly(2024, 10, 15);
        DateOnly to = new DateOnly(2024, 10, 20);
        var payrollData = new List<Payroll> { };
       
        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(new Mock<IQueryProvider>().Object);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(Expression.Constant(payrollData.AsQueryable()));
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(typeof(Payroll));
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(payrollData.GetEnumerator());

        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);  
        
        var result =_payrollService.GetReport(companyId, from, to);

        Assert.Null(result);
    }
 
    [Fact]
    public void GetCurrentPayroll_ReturnsMappedModel_WhenPayrollExists()
    {

        int companyId = 1;
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        var payrollData = new List<Payroll>()
        {
            new Payroll { CompanyId = companyId, ScheduledRunDate = today.AddDays(-2), TrueRunDate = null },
            new Payroll { CompanyId = companyId, ScheduledRunDate = today.AddDays(-1), StartDate = today, TrueRunDate = null },
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrollData.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrollData.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrollData.ElementType);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(payrollData.GetEnumerator());
        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);

        var expectedModel = new PayrollModel(); 
        _mockMapper.Setup(m => m.Map<PayrollModel>(It.IsAny<Payroll>())).Returns(expectedModel);

        var result = _payrollService.GetCurrentPayroll(companyId);

        Assert.Equal(expectedModel, result);
    }

    [Fact]
    public void GetCurrentPayroll_ReturnsNull_WhenNoPayrollExists()
    {
        int companyId = 1;

        var payrollData = new List<Payroll> { }.AsQueryable();
     
        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrollData.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrollData.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrollData.ElementType);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(payrollData.GetEnumerator());
        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);

        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);

        var result = _payrollService.GetCurrentPayroll(companyId);

        Assert.Null(result);
    }
    
        [Fact]
        public void GetCurrentPayroll_ReturnsFirst_WhenMultipleScheduledPayrollsExist()
        {
            int companyId = 1;
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);

            var payrollData = new List<Payroll>()
            {
                new Payroll { CompanyId = companyId, ScheduledRunDate = today.AddDays(-3), StartDate = today, TrueRunDate = null },
                new Payroll { CompanyId = companyId, ScheduledRunDate = today.AddDays(-2), StartDate = today, TrueRunDate = null },
            }.AsQueryable();
        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrollData.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrollData.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrollData.ElementType);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(payrollData.GetEnumerator());
        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);

            var expectedModel = new PayrollModel { CompanyId = companyId, ScheduledRunDate =  today.AddDays(-3).ToDateTime(TimeOnly.MinValue), StartDate = today.ToDateTime(TimeOnly.MinValue), TrueRunDate = null }; 
        

    _mockMapper.Setup(m => m.Map<PayrollModel>(It.IsAny<Payroll>())).Returns(expectedModel);

            var result = _payrollService.GetCurrentPayroll(companyId);

            Assert.Equal(expectedModel, result); 
        }
    [Fact]
    public void GetPreviousPayroll_ReturnsMappedModel_WhenPreviousPayrollExists()
    {
        int companyId = 1;
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        var payrollData = new List<Payroll>()
        {
            new Payroll { CompanyId = companyId, ScheduledRunDate = today.AddDays(-3) },
            new Payroll { CompanyId = companyId, ScheduledRunDate = today.AddDays(-5) },
            new Payroll { CompanyId = companyId, ScheduledRunDate = today.AddDays(-2) },
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrollData.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrollData.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrollData.ElementType);
        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);

        var expectedModel = new PayrollModel { CompanyId = companyId, ScheduledRunDate = today.AddDays(-5).ToDateTime(TimeOnly.MinValue), StartDate = today.ToDateTime(TimeOnly.MinValue), TrueRunDate = null };
        _mockMapper.Setup(m => m.Map<PayrollModel>(It.IsAny<Payroll>())).Returns(expectedModel);

        var result = _payrollService.GetPreviousPayroll(companyId);
       
        Assert.Equal(expectedModel, result);
    }
    [Fact]
    public void GetPreviousPayroll_ReturnsNull_WhenNoPreviousPayrollExists()
    {

        int companyId = 1;
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        var payrollData = new List<Payroll>()
        {
            new Payroll { CompanyId = companyId, ScheduledRunDate = today }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrollData.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrollData.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrollData.ElementType);
        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);

        var result = _payrollService.GetPreviousPayroll(companyId);

        Assert.Null(result);
    }
    [Fact]
    public void GetPreviousPayroll_ReturnsNull_WhenNoPayrollExists()
    {
        int companyId = 1;
        var payrollData = new List<Payroll> { }.AsQueryable();

        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrollData.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrollData.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrollData.ElementType);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(payrollData.GetEnumerator());
        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);

        var result = _payrollService.GetPreviousPayroll(companyId);

        Assert.Null(result);
    }
    [Fact]
    public void GetById_ReturnsMappedModel_WhenPayrollExists()
    {
        int companyId = 1;
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        var payrollData = new List<Payroll>()
        {
            new Payroll { CompanyId = companyId, ScheduledRunDate = today }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrollData.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrollData.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrollData.ElementType);
        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);

        var expectedPayrollModel = new PayrollModel { CompanyId = companyId, ScheduledRunDate = today.ToDateTime(TimeOnly.MinValue) };
             
        _mockMapper.Setup(m => m.Map<PayrollModel>(It.IsAny<Payroll>())).Returns(expectedPayrollModel);

        var result = _payrollService.GetById(companyId);

        Assert.Equal(expectedPayrollModel, result);
    }

    [Fact]
    public void GetById_ReturnsNull_WhenPayrollDoesNotExist()
    {
        int id = 1;
        var payrollData = new List<Payroll>{ }.AsQueryable();

        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrollData.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrollData.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrollData.ElementType);
        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);
        var result = _payrollService.GetById(id);

        Assert.Null(result);
    }
    [Fact]
    public async Task CreateNextPayrollForAll_CreatesPayrolls_ForExistingWithScheduleChange()
    {
   
        int companyId1 = 1;
        int companyId2 = 2;

        var startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(2));
        var existingSchedule1 = new PayrollSchedule { Id = 1, CompanyId = companyId1, FirstRunDate = startDate.AddDays(-10),PayrollScheduleTypeId=1, StartDate = startDate.AddDays(-10) };
        var existingSchedule2 = new PayrollSchedule { Id = 2, CompanyId = companyId2, FirstRunDate = startDate.AddDays(-15), PayrollScheduleTypeId = 2, StartDate = startDate.AddDays(-5) };
        var existingSchedules=new List<PayrollSchedule>() { existingSchedule1,existingSchedule2 }.AsQueryable();

        var newSchedule1 = new PayrollSchedule { Id = 3, CompanyId = companyId1, FirstRunDate = startDate.AddDays(-5) , PayrollScheduleTypeId = 1,StartDate= startDate.AddDays(-10) };
        var newSchedule2 = new PayrollSchedule { Id = 4, CompanyId = companyId2, FirstRunDate = startDate.AddDays(-6), PayrollScheduleTypeId=2, StartDate = startDate.AddDays(-5) };
        var newSchedules = new List<PayrollSchedule>() { newSchedule1, newSchedule2 }.AsQueryable();

        var mockPaySchedules = new Mock<DbSet<PayrollSchedule>>();
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.Provider).Returns(newSchedules.Provider);
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.Expression).Returns(newSchedules.Expression);
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.ElementType).Returns(newSchedules.ElementType);
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.GetEnumerator()).Returns(newSchedules.GetEnumerator());
        _mockContext.Setup(c => c.PayrollSchedule).Returns(mockPaySchedules.Object);

        var currentPayroll1 = new Payroll { Id = 1, CompanyId = companyId1, ScheduledRunDate = startDate, PayrollSchedule = existingSchedule1 };
        var currentPayroll2 = new Payroll { Id = 2, CompanyId = companyId2, ScheduledRunDate = startDate.AddDays(1), PayrollSchedule = existingSchedule2 };
        var currentPayrolls = new List<Payroll>() { currentPayroll1, currentPayroll2 }.AsQueryable();

        var mockPayroll = new Mock<DbSet<Payroll>>();
        mockPayroll.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(currentPayrolls.Provider);
        mockPayroll.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(currentPayrolls.Expression);
        mockPayroll.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(currentPayrolls.ElementType);
        mockPayroll.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(currentPayrolls.GetEnumerator());
        _mockContext.Setup(c => c.Payroll).Returns(mockPayroll.Object);

        var addedPayrolls = new List<Payroll>();
        _mockContext.Setup(m => m.Payroll.AddRange(It.IsAny<IEnumerable<Payroll>>()))
           .Callback<IEnumerable<Payroll>>(payrolls => addedPayrolls.AddRange(payrolls));

      
        var result = await _payrollService.CreateNextPayrollForAll();

        Assert.Equal(2, result); 

        _mockContext.Verify(m => m.Payroll.AddRange(It.IsAny<IEnumerable<Payroll>>()), Times.Once);

        var newPayroll1 = addedPayrolls.Single(p => p.CompanyId == companyId1);
        var newPayroll2 = addedPayrolls.Single(p => p.CompanyId == companyId2);

        Assert.Equal(newSchedule1.Id, newPayroll1.PayrollScheduleId);
        Assert.Equal(newSchedule1.StartDate.AddDays(-1).AddMonths(1), newPayroll1.ScheduledRunDate); 
        Assert.Equal(newSchedule2.Id, newPayroll2.PayrollScheduleId);
       

    }
    [Fact]
    public async Task CreateNextPayrollForAll_CreatesPayrolls_ForExistingWithoutScheduleChange()
    {

        int companyId1 = 1;
        int companyId2 = 2;

        var startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(2));

        var existingSchedule1 = new PayrollSchedule { Id = 1, CompanyId = companyId1, FirstRunDate = startDate.AddDays(-10), PayrollScheduleTypeId = 1, StartDate = startDate.AddDays(-10) };
        var existingSchedule2 = new PayrollSchedule { Id = 2, CompanyId = companyId2, FirstRunDate = startDate.AddDays(-15), PayrollScheduleTypeId = 2, StartDate = startDate.AddDays(-5) };
        var existingSchedules = new List<PayrollSchedule>() { existingSchedule1, existingSchedule2 }.AsQueryable();

        var mockPaySchedules = new Mock<DbSet<PayrollSchedule>>();
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.Provider).Returns(existingSchedules.Provider);
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.Expression).Returns(existingSchedules.Expression);
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.ElementType).Returns(existingSchedules.ElementType);
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.GetEnumerator()).Returns(existingSchedules.GetEnumerator());
        _mockContext.Setup(c => c.PayrollSchedule).Returns(mockPaySchedules.Object);

        var currentPayroll1 = new Payroll { Id = 1, CompanyId = companyId1, ScheduledRunDate = startDate, PayrollSchedule = existingSchedule1 };
        var currentPayroll2 = new Payroll { Id = 2, CompanyId = companyId2, ScheduledRunDate = startDate.AddDays(1), PayrollSchedule = existingSchedule2 };
        var currentPayrolls = new List<Payroll>() { currentPayroll1, currentPayroll2 }.AsQueryable();

        var mockPayroll = new Mock<DbSet<Payroll>>();
        mockPayroll.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(currentPayrolls.Provider);
        mockPayroll.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(currentPayrolls.Expression);
        mockPayroll.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(currentPayrolls.ElementType);
        mockPayroll.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(currentPayrolls.GetEnumerator());
        _mockContext.Setup(c => c.Payroll).Returns(mockPayroll.Object);

        var addedPayrolls = new List<Payroll>();
        _mockContext.Setup(m => m.Payroll.AddRange(It.IsAny<IEnumerable<Payroll>>()))
           .Callback<IEnumerable<Payroll>>(payrolls => addedPayrolls.AddRange(payrolls));


        var result = await _payrollService.CreateNextPayrollForAll();

        Assert.Equal(2, result);

        _mockContext.Verify(m => m.Payroll.AddRange(It.IsAny<IEnumerable<Payroll>>()), Times.Once);

        var newPayroll1 = addedPayrolls.Single(p => p.CompanyId == companyId1);
        var newPayroll2 = addedPayrolls.Single(p => p.CompanyId == companyId2);

        var nextRunDate =currentPayroll1.ScheduledRunDate.AddDays(1).AddMonths(1);
        if (nextRunDate.DayOfWeek == DayOfWeek.Saturday)
            nextRunDate = nextRunDate.AddDays(-1);
        else if (nextRunDate.DayOfWeek == DayOfWeek.Sunday)
            nextRunDate = nextRunDate.AddDays(-2);

        Assert.Equal(existingSchedule1.Id, newPayroll1.PayrollScheduleId);
        Assert.Equal(nextRunDate, newPayroll1.ScheduledRunDate);
        Assert.Equal(existingSchedule2.Id, newPayroll2.PayrollScheduleId);
    }
  
    [Fact]
    public void Create_ThrowsException_WhenScheduleNotFound()
    {    
        int companyId = 1;

        var existingSchedules = new List<PayrollSchedule>() { }.AsQueryable();

        var mockPaySchedules = new Mock<DbSet<PayrollSchedule>>();
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.Provider).Returns(existingSchedules.Provider);
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.Expression).Returns(existingSchedules.Expression);
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.ElementType).Returns(existingSchedules.ElementType);
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.GetEnumerator()).Returns(existingSchedules.GetEnumerator());
        _mockContext.Setup(c => c.PayrollSchedule).Returns(mockPaySchedules.Object);

        Assert.Throws<AppException>(() => _payrollService.Create(companyId));
    }

    [Fact]
    public void Create_CreatesFirstPayroll_WhenNoPreviousPayrollExists()
    {
   
        int companyId = 1;
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var firstRunDate = startDate.AddDays(2);
       
        var existingSchedules = new List<PayrollSchedule>() {
             new PayrollSchedule { Id = 1, CompanyId = companyId, FirstRunDate = firstRunDate, PayrollScheduleTypeId = 1, StartDate = startDate.AddDays(-10) }
        }.AsQueryable();

        var mockPaySchedules = new Mock<DbSet<PayrollSchedule>>();
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.Provider).Returns(existingSchedules.Provider);
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.Expression).Returns(existingSchedules.Expression);
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.ElementType).Returns(existingSchedules.ElementType);
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.GetEnumerator()).Returns(existingSchedules.GetEnumerator());
        _mockContext.Setup(c => c.PayrollSchedule).Returns(mockPaySchedules.Object);      

        var currentPayrolls = new List<Payroll>() {  }.AsQueryable();

        var mockPayroll = new Mock<DbSet<Payroll>>();
        mockPayroll.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(currentPayrolls.Provider);
        mockPayroll.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(currentPayrolls.Expression);
        mockPayroll.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(currentPayrolls.ElementType);
        mockPayroll.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(currentPayrolls.GetEnumerator());
        _mockContext.Setup(c => c.Payroll).Returns(mockPayroll.Object);

       _payrollService.Create(companyId);

        _mockContext.Verify(m => m.Payroll.Add(It.IsAny<Payroll>()), Times.Once);
        _mockContext.Setup(c => c.Payroll.Add(It.IsAny<Payroll>()))
      .Callback<Payroll>(addedPayroll =>
      {
          Assert.Equal(existingSchedules.FirstOrDefault().Id, addedPayroll.PayrollScheduleId);
          Assert.Equal(existingSchedules.FirstOrDefault().StartDate, addedPayroll.StartDate);
          Assert.Equal(firstRunDate, addedPayroll.ScheduledRunDate);
          Assert.Equal(companyId, addedPayroll.CompanyId);
      });
    }

    
    [Fact]
    public void Update_ThrowsUnauthorizedAccessException_WhenCompanyIdMismatch()
    {
        int id = 1;
        int companyId = 2; 

        var dto = new Payroll { Id = id, CompanyId = 1 }; 
        _mockContext.Setup(dc => dc.Payroll.Find(It.IsAny<int>()))
          .Returns(dto);
      
        Assert.Throws<UnauthorizedAccessException>(() => _payrollService.Update(id, companyId));
    }
    [Fact]
    public void Update_UpdatesPayrollAndCreatesPaystubs_WhenTrueRunDateIsNull()
    {
        int id = 1;
        int companyId = 1;     

        var dto = new Payroll { Id = id, CompanyId = companyId, TrueRunDate = null ,
            StartDate= DateOnly.FromDateTime(DateTime.Now.AddDays(-10)),
            ScheduledRunDate= DateOnly.FromDateTime(DateTime.Now.AddDays(5))
        };
        var mockCompany = new Mock<Company>();
        dto.Company = mockCompany.Object;
        _mockContext.Setup(dc => dc.Payroll.Find(It.IsAny<int>()))
        .Returns(dto);

        var employments = new List<Employment>
        { new Employment { Job = new Job { CompanyId = companyId, Id = 1 }, Deactivated = false,PayRateBasisId=4,PayRate=10000 }           
        }.AsQueryable();
        var mockEmploymentSet = new Mock<DbSet<Employment>>();
        mockEmploymentSet.As<IQueryable<Employment>>().Setup(m => m.Provider).Returns(employments.Provider);
        mockEmploymentSet.As<IQueryable<Employment>>().Setup(m => m.Expression).Returns(employments.Expression);
        mockEmploymentSet.As<IQueryable<Employment>>().Setup(m => m.ElementType).Returns(employments.ElementType);
        mockEmploymentSet.As<IQueryable<Employment>>().Setup(m => m.GetEnumerator()).Returns(employments.GetEnumerator());
        _mockContext.Setup(c => c.Employment).Returns(mockEmploymentSet.Object);

        var timeCardUsa = new List<TimecardUsa>
        { new TimecardUsa { Job = new Job { CompanyId = companyId, Id = 1 }, PersonId = 2, Id = 1 ,ClockIn= DateTime.UtcNow,ClockOut = DateTime.UtcNow.AddHours(8)}
        }.AsQueryable();
        var mockTimeCardUsaSet = new Mock<DbSet<TimecardUsa>>();
        mockTimeCardUsaSet.As<IQueryable<TimecardUsa>>().Setup(m => m.Provider).Returns(timeCardUsa.Provider);
        mockTimeCardUsaSet.As<IQueryable<TimecardUsa>>().Setup(m => m.Expression).Returns(timeCardUsa.Expression);
        mockTimeCardUsaSet.As<IQueryable<TimecardUsa>>().Setup(m => m.ElementType).Returns(timeCardUsa.ElementType);
        mockTimeCardUsaSet.As<IQueryable<TimecardUsa>>().Setup(m => m.GetEnumerator()).Returns(timeCardUsa.GetEnumerator());
        _mockContext.Setup(c => c.TimecardUsa).Returns(mockTimeCardUsaSet.Object);

        var existingSchedules = new List<PayrollSchedule>() {
             new PayrollSchedule { Id = 1, CompanyId = companyId, PayrollScheduleTypeId = 1}
        }.AsQueryable();

        var mockPaySchedules = new Mock<DbSet<PayrollSchedule>>();
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.Provider).Returns(existingSchedules.Provider);
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.Expression).Returns(existingSchedules.Expression);
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.ElementType).Returns(existingSchedules.ElementType);
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.GetEnumerator()).Returns(existingSchedules.GetEnumerator());
        _mockContext.Setup(c => c.PayrollSchedule).Returns(mockPaySchedules.Object);

        var mockPayStubs = new Mock<DbSet<PayStub>>();    
        _mockContext.Setup(c => c.PayStub).Returns(mockPayStubs.Object);

        var mockCreatePaystubs = new Mock<Func<Payroll, List<PayStub>>>();
        mockCreatePaystubs.Setup(c => c(dto)).Returns(new List<PayStub>());

        _payrollService.Update(id, companyId);

        _mockContext.Verify(c => c.Payroll.Update(dto), Times.Once);
        _mockContext.Verify(c => c.PayStub.AddRange(It.IsAny<IEnumerable<PayStub>>()), Times.Once);
        _mockContext.Verify(c => c.SaveChanges(), Times.Once);
    }
    [Fact]
    public void Delete_RemovesPayroll_WhenFound()
    {
        int id = 1;
        var payroll = new Payroll
        {
            Id = id,
            CompanyId = 2,
            TrueRunDate = null,
            StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)),
            ScheduledRunDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5))
        };

        _mockContext.Setup(c => c.Payroll.Find(id)).Returns(payroll);

        _payrollService.Delete(id);

        _mockContext.Verify(c => c.Payroll.Remove(payroll), Times.Once);
        _mockContext.Verify(c => c.SaveChanges(), Times.Once);
    }
    [Fact]
    public void Delete_ThrowsKeyNotFoundException_WhenNotFound()
    {
        int id = 1;

        _mockContext.Setup(c => c.Payroll.Find(id)).Returns((Payroll)null);

        Assert.Throws<AppException>(() => _payrollService.Delete(id));
    }
    [Fact]
    public void CalculatePayForHourlyRatedEmployees_ShouldReturnCorrectPay()
    {

        var timecards = new List<TimecardUsa>
        {
            new TimecardUsa { PersonId = 1, ClockIn = DateTime.Now.AddDays(-8), HoursWorked = TimeSpan.FromHours(10) },
            new TimecardUsa { PersonId = 1, ClockIn = DateTime.Now.AddDays(-7), HoursWorked = TimeSpan.FromHours(10) },
            new TimecardUsa { PersonId = 1, ClockIn = DateTime.Now.AddDays(-9), HoursWorked = TimeSpan.FromHours(10) },
            new TimecardUsa { PersonId = 1, ClockIn = DateTime.Now.AddDays(-10), HoursWorked = TimeSpan.FromHours(10) },
            new TimecardUsa { PersonId = 1, ClockIn = DateTime.Now.AddDays(-5), HoursWorked = TimeSpan.FromHours(15) },
            new TimecardUsa { PersonId = 1, ClockIn = DateTime.Now.AddDays(-6), HoursWorked = TimeSpan.FromHours(15) },
            new TimecardUsa { PersonId = 1, ClockIn = DateTime.Now.AddDays(-3), HoursWorked = TimeSpan.FromHours(8) }
        };
        var currentPayroll = new Payroll { StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)), ScheduledRunDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)) };
        var emp = new Employment { PersonId = 1, PayRate = 20 };
        var frequency = PayrollScheduleTypeModel.Biweekly;

        var result = typeof(PayrollService).GetMethod("CalculatePayForHourlyRatedEmployees", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Invoke(null, new object[] { timecards, currentPayroll, emp, frequency }) as Tuple<double, double, double, double>;

        var firstWeekOverTime = (10 + 10 + 10 + 10 + 15 + 15) - 40;
        var secondWeekOverTime = 0;
        var totalHoursWorked = 10 + 10 + 10 + 10 + 15 + 15 + 8;
        var overTimeHours = firstWeekOverTime + secondWeekOverTime;
        var regularHours = totalHoursWorked - overTimeHours;
        double expectedRegularPay = emp.PayRate * regularHours; 
        double expectedRegularHours = regularHours; 
        double expectedOverTimePay = emp.PayRate * 1.5 * overTimeHours; 
        double expectedOverTimeHours = totalHoursWorked- regularHours; 

        Assert.NotNull(result);
        Assert.Equal(expectedRegularPay, result.Item1, 2);
        Assert.Equal(expectedRegularHours, result.Item2, 2);
        Assert.Equal(expectedOverTimePay, result.Item3, 2);
        Assert.Equal(expectedOverTimeHours, result.Item4, 2);
    }
    [Fact]
    public void CalculatePayForDailyRatedEmployees_ShouldReturnCorrectPayAndDaysWorked()
    {
        var timecards = new List<TimecardUsa>
        {
            new TimecardUsa { PersonId = 1, ClockIn = new DateTime(2024, 10, 1, 9, 0, 0) },
            new TimecardUsa { PersonId = 1, ClockIn = new DateTime(2024, 10, 2, 9, 0, 0) },
            new TimecardUsa { PersonId = 1, ClockIn = new DateTime(2024, 10, 3, 9, 0, 0) }
        };
        var currentPayroll = new Payroll
        {
            StartDate = new DateOnly(2024, 10, 1),
            ScheduledRunDate = new DateOnly(2024, 10, 31)
        };
        var emp = new Employment { PersonId = 1, PayRate = 100 };

        var methodInfo = typeof(PayrollService).GetMethod("CalculatePayForDailyRatedEmployees", BindingFlags.NonPublic | BindingFlags.Static);
        var result = (Tuple<double, int>)methodInfo.Invoke(null, new object[] { timecards, currentPayroll, emp });

        Assert.Equal(300, result.Item1); 
        Assert.Equal(3, result.Item2); 
    }
}
