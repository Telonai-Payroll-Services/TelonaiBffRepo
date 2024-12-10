using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Services;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Models;
using System.Security.Claims;

public class InvitationServiceTests
{
    private readonly Mock<DataContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMailSender> _mailSenderMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly InvitationService _invitationService;

    public InvitationServiceTests()
    {
        _contextMock = new Mock<DataContext>(MockBehavior.Default, new object[] { new Mock<IHttpContextAccessor>().Object });

        _mapperMock = new Mock<IMapper>();
        _mailSenderMock = new Mock<IMailSender>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _invitationService = new InvitationService(_contextMock.Object, _mapperMock.Object, _mailSenderMock.Object, _httpContextAccessorMock.Object);
    }
    [Fact]
    public void GetAllByActivaionCodeAndInviteeEmail_ValidData_ReturnsInvitationModel()
    {
        var activationCode = "1234"; 
        var email = "test@example.com";

        var guidEndingWithCode = Guid.Parse($"00000000-0000-0000-0000-00000000{activationCode}");

        var invitation = new Invitation
        {
            Id = guidEndingWithCode,
            Email = email,
            ExpirationDate = DateTime.UtcNow.AddDays(1)
        };

        var invitationModel = new InvitationModel();

        var invitations = new List<Invitation> { invitation }.AsQueryable();
        var mockSet = new Mock<DbSet<Invitation>>();
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.Provider).Returns(invitations.Provider);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.Expression).Returns(invitations.Expression);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.ElementType).Returns(invitations.ElementType);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.GetEnumerator()).Returns(invitations.GetEnumerator());

        _contextMock.Setup(ctx => ctx.Invitation).Returns(mockSet.Object);
        _mapperMock.Setup(m => m.Map<InvitationModel>(It.IsAny<Invitation>())).Returns(invitationModel);

        var result = _invitationService.GetAllByActivaionCodeAndInviteeEmail(activationCode, email);

        Assert.NotNull(result);
        Assert.Equal(invitationModel, result);
        _mapperMock.Verify(m => m.Map<InvitationModel>(invitation), Times.Once);
    }



    [Fact]
    public void GetAllByActivaionCodeAndInviteeEmail_InvalidActivationCodeOrEmail_ThrowsAppException()
    {
        var activationCode = "invalidcode";
        var email = "test@example.com";

        var invitations = new List<Invitation>().AsQueryable();
        var mockSet = new Mock<DbSet<Invitation>>();
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.Provider).Returns(invitations.Provider);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.Expression).Returns(invitations.Expression);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.ElementType).Returns(invitations.ElementType);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.GetEnumerator()).Returns(invitations.GetEnumerator());

        _contextMock.Setup(ctx => ctx.Invitation).Returns(mockSet.Object);

        var exception = Assert.Throws<AppException>(() => _invitationService.GetAllByActivaionCodeAndInviteeEmail(activationCode, email));
        Assert.Equal("Invalid Activation Code or Email", exception.Message);
    }
    [Fact]
    public void GetAllByActivaionCodeAndInviteeEmail2_ValidData_ReturnsInvitation()
    {
        var activationCode = "1234"; 
        var email = "test@example.com";

        var guidEndingWithCode = Guid.Parse($"00000000-0000-0000-0000-00000000{activationCode}");

        var invitation = new Invitation
        {
            Id = guidEndingWithCode,
            Email = email,
            ExpirationDate = DateTime.UtcNow.AddDays(1)
        };

        var invitationModel = new InvitationModel();

        var invitations = new List<Invitation> { invitation }.AsQueryable();
        var mockSet = new Mock<DbSet<Invitation>>();
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.Provider).Returns(invitations.Provider);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.Expression).Returns(invitations.Expression);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.ElementType).Returns(invitations.ElementType);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.GetEnumerator()).Returns(invitations.GetEnumerator());

        _contextMock.Setup(ctx => ctx.Invitation).Returns(mockSet.Object);
        _mapperMock.Setup(m => m.Map<InvitationModel>(It.IsAny<Invitation>())).Returns(invitationModel);

        var result = _invitationService.GetAllByActivaionCodeAndInviteeEmail2(activationCode, email);

        Assert.NotNull(result);
        Assert.Equal(invitation, result);
    }

    [Fact]
    public void GetAllByActivaionCodeAndInviteeEmail2_InvalidActivationCodeOrEmail_ThrowsAppException()
    {

        var activationCode = "invalidcode";
        var email = "test@example.com";

        var invitations = new List<Invitation>().AsQueryable();
        var mockSet = new Mock<DbSet<Invitation>>();
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.Provider).Returns(invitations.Provider);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.Expression).Returns(invitations.Expression);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.ElementType).Returns(invitations.ElementType);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.GetEnumerator()).Returns(invitations.GetEnumerator());

        _contextMock.Setup(ctx => ctx.Invitation).Returns(mockSet.Object);

        var exception = Assert.Throws<AppException>(() => _invitationService.GetAllByActivaionCodeAndInviteeEmail2(activationCode, email));
        Assert.Equal("Invalid Activation Code or Email", exception.Message);
    }
    [Fact]
    public void GetByActivaionCodeAndInviteeEmail_WithTaxId_ValidData_ReturnsInvitation()
    {
        var activationCode = "1234"; 
        var email = "test@example.com";
        var taxId = "12345";

        var guidEndingWithCode = Guid.Parse($"00000000-0000-0000-0000-00000000{activationCode}");

        var invitation = new Invitation
        {
            Id = guidEndingWithCode,
            Email = email,
            TaxId= taxId,
            ExpirationDate = DateTime.UtcNow.AddDays(1)
        };

        var invitationModel = new InvitationModel();

        var invitations = new List<Invitation> { invitation }.AsQueryable();
        var mockSet = new Mock<DbSet<Invitation>>();
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.Provider).Returns(invitations.Provider);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.Expression).Returns(invitations.Expression);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.ElementType).Returns(invitations.ElementType);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.GetEnumerator()).Returns(invitations.GetEnumerator());

        _contextMock.Setup(ctx => ctx.Invitation).Returns(mockSet.Object);
        _mapperMock.Setup(m => m.Map<InvitationModel>(It.IsAny<Invitation>())).Returns(invitationModel);

        var result = _invitationService.GetByActivaionCodeAndInviteeEmail(activationCode, email, taxId);

        Assert.NotNull(result);
        Assert.Equal(invitation, result);
    }

    [Fact]
    public void GetByActivaionCodeAndInviteeEmail_WithTaxId_InvalidActivationCode_ThrowsAppException()
    {
        var activationCode = "invalidcode";
        var email = "test@example.com";
        var taxId = "12345";

        var invitations = new List<Invitation>().AsQueryable();
        var mockSet = new Mock<DbSet<Invitation>>();
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.Provider).Returns(invitations.Provider);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.Expression).Returns(invitations.Expression);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.ElementType).Returns(invitations.ElementType);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.GetEnumerator()).Returns(invitations.GetEnumerator());

        _contextMock.Setup(ctx => ctx.Invitation).Returns(mockSet.Object);

        var exception = Assert.Throws<AppException>(() => _invitationService.GetByActivaionCodeAndInviteeEmail(activationCode, email, taxId));
        Assert.Equal("Invalid Activation Code", exception.Message);
    }

    [Fact]
    public void GetByActivaionCodeAndInviteeEmail_ValidData_ReturnsInvitationModel()
    {

        var activationCode = "1234"; 
        var email = "test@example.com";       

        var guidEndingWithCode = Guid.Parse($"00000000-0000-0000-0000-00000000{activationCode}");

        var invitation = new Invitation
        {
            Id = guidEndingWithCode,
            Email = email,
            ExpirationDate = DateTime.UtcNow.AddDays(1)
        };
        var invitationModel = new InvitationModel();

        var invitations = new List<Invitation> { invitation }.AsQueryable();
        var mockSet = new Mock<DbSet<Invitation>>();
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.Provider).Returns(invitations.Provider);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.Expression).Returns(invitations.Expression);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.ElementType).Returns(invitations.ElementType);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.GetEnumerator()).Returns(invitations.GetEnumerator());

        _contextMock.Setup(ctx => ctx.Invitation).Returns(mockSet.Object);
        _mapperMock.Setup(m => m.Map<InvitationModel>(It.IsAny<Invitation>())).Returns(invitationModel);

        var result = _invitationService.GetByActivaionCodeAndInviteeEmail(activationCode, email);

        Assert.NotNull(result);
        Assert.Equal(invitationModel, result);
        _mapperMock.Verify(m => m.Map<InvitationModel>(invitation), Times.Once);
    }

    [Fact]
    public void GetByActivaionCodeAndInviteeEmail_InvalidActivationCodeOrEmail_ThrowsAppException()
    {

        var activationCode = "invalidcode";
        var email = "test@example.com";

        var invitations = new List<Invitation>().AsQueryable();
        var mockSet = new Mock<DbSet<Invitation>>();
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.Provider).Returns(invitations.Provider);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.Expression).Returns(invitations.Expression);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.ElementType).Returns(invitations.ElementType);
        mockSet.As<IQueryable<Invitation>>().Setup(m => m.GetEnumerator()).Returns(invitations.GetEnumerator());

        _contextMock.Setup(ctx => ctx.Invitation).Returns(mockSet.Object);

        var exception = Assert.Throws<AppException>(() => _invitationService.GetByActivaionCodeAndInviteeEmail(activationCode, email));
        Assert.Equal("Invalid Activation Code or Email", exception.Message);
    }
    [Fact]
    public async Task CreateAsync_NewInvitation_ReturnsInvitation()
    {

        var model = new InvitationModel
        {
            Email = "test@example.com",
            JobId = 1,
            TaxId = "12345",
            Company = "Test Company",
            FirstName = "John",
            LastName = "Doe",
            Employment = new EmploymentModel { IsTenNinetyNine = false, PayRate = 50, PayRateBasisId = 1, IsSalariedOvertimeExempt = false }
        };
        var invitation = new Invitation { Email = model.Email, JobId = model.JobId, TaxId = model.TaxId, ExpirationDate = DateTime.UtcNow.AddDays(1) };
        var company = new Company { Id = 1, Name = model.Company }; 
        var person = new Person { Id = 1, CompanyId = company.Id, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName };
        var employment = new Employment { Id = 1, JobId = (int)model.JobId, PersonId = person.Id ,Person = new Person { Email = model.Email }, Job = new Job { CompanyId = company.Id }, SignUpStatusTypeId = (int)SignUpStatusTypeModel.None };
        var currentUserEmail = model.Email;
        var currentEmp = new Employment { Id = 2, Person = new Person { Email = currentUserEmail }, Job = new Job { CompanyId = company.Id }, SignUpStatusTypeId = (int)SignUpStatusTypeModel.None };

  
        var invitations = new List<Invitation>() { invitation}.AsQueryable();
        var invitationMockSet = new Mock<DbSet<Invitation>>();
        invitationMockSet.As<IQueryable<Invitation>>().Setup(m => m.Provider).Returns(invitations.Provider);
        invitationMockSet.As<IQueryable<Invitation>>().Setup(m => m.Expression).Returns(invitations.Expression);
        invitationMockSet.As<IQueryable<Invitation>>().Setup(m => m.ElementType).Returns(invitations.ElementType);
        invitationMockSet.As<IQueryable<Invitation>>().Setup(m => m.GetEnumerator()).Returns(invitations.GetEnumerator());

        _contextMock.Setup(ctx => ctx.Invitation).Returns(invitationMockSet.Object);

        var jobs = new List<Job> { new Job { Id = (int)model.JobId, Company = company } }.AsQueryable();
        var jobMockSet = new Mock<DbSet<Job>>();
        jobMockSet.As<IQueryable<Job>>().Setup(m => m.Provider).Returns(jobs.Provider);
        jobMockSet.As<IQueryable<Job>>().Setup(m => m.Expression).Returns(jobs.Expression);
        jobMockSet.As<IQueryable<Job>>().Setup(m => m.ElementType).Returns(jobs.ElementType);
        jobMockSet.As<IQueryable<Job>>().Setup(m => m.GetEnumerator()).Returns(jobs.GetEnumerator());

        _contextMock.Setup(ctx => ctx.Job).Returns(jobMockSet.Object);

        var people = new List<Person>() { person}.AsQueryable();
        var personMockSet = new Mock<DbSet<Person>>();
        personMockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(people.Provider);
        personMockSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(people.Expression);
        personMockSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(people.ElementType);
        personMockSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(people.GetEnumerator());

        _contextMock.Setup(ctx => ctx.Person).Returns(personMockSet.Object);

        var employments = new List<Employment>() { employment}.AsQueryable();
        var employmentMockSet = new Mock<DbSet<Employment>>();
        employmentMockSet.As<IQueryable<Employment>>().Setup(m => m.Provider).Returns(employments.Provider);
        employmentMockSet.As<IQueryable<Employment>>().Setup(m => m.Expression).Returns(employments.Expression);
        employmentMockSet.As<IQueryable<Employment>>().Setup(m => m.ElementType).Returns(employments.ElementType);
        employmentMockSet.As<IQueryable<Employment>>().Setup(m => m.GetEnumerator()).Returns(employments.GetEnumerator());

        _contextMock.Setup(ctx => ctx.Employment).Returns(employmentMockSet.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] 
        { 
            new Claim("email", currentUserEmail),
        })); 
        var httpContext = new DefaultHttpContext { User = user };
        _httpContextAccessorMock.Setup(_ => _.HttpContext).Returns(httpContext);
        _mapperMock.Setup(m => m.Map<Invitation>(model)).Returns(invitation);
        _contextMock.Setup(ctx => ctx.SaveChanges()).Returns(1);

        var result = await _invitationService.CreateAsync(model, true);

        Assert.NotNull(result);
        Assert.Equal(invitation, result);
        _contextMock.Verify(ctx => ctx.SaveChanges(), Times.AtLeastOnce);
        _mailSenderMock.Verify(ms => ms.SendUsingAwsClientAsync(
            model.Email,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Once);
    }



    [Fact]
    public async Task CreateAsync_ExistingInvitation_MailSendingFails_ThrowsException()
    {

        var model = new InvitationModel
        {
            Email = "test@example.com",
            JobId = 1,
            TaxId = "12345",
            Company = "Test Company",
            FirstName = "John",
            LastName = "Doe",
            Employment = new EmploymentModel { IsTenNinetyNine = false, PayRate = 50, PayRateBasisId = 1, IsSalariedOvertimeExempt = false }
        };
        var invitation = new Invitation { Id = Guid.NewGuid(), Email = model.Email, JobId = model.JobId, TaxId = model.TaxId, ExpirationDate = DateTime.UtcNow.AddDays(1) };
        var company = new Company { Id = 1, Name = model.Company }; 
        var person = new Person { Id = 1, CompanyId = company.Id, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName };
        var employment = new Employment { Id = 1, JobId = (int)model.JobId, PersonId = person.Id };
        var currentUserEmail = "currentuser@example.com";
        var currentEmp = new Employment { Id = 2, Person = new Person { Email = currentUserEmail }, Job = new Job { CompanyId = company.Id }, SignUpStatusTypeId = (int)SignUpStatusTypeModel.None };

        var invitations = new List<Invitation> { invitation }.AsQueryable();
        var invitationMockSet = new Mock<DbSet<Invitation>>();
        invitationMockSet.As<IQueryable<Invitation>>().Setup(m => m.Provider).Returns(invitations.Provider);
        invitationMockSet.As<IQueryable<Invitation>>().Setup(m => m.Expression).Returns(invitations.Expression);
        invitationMockSet.As<IQueryable<Invitation>>().Setup(m => m.ElementType).Returns(invitations.ElementType);
        invitationMockSet.As<IQueryable<Invitation>>().Setup(m => m.GetEnumerator()).Returns(invitations.GetEnumerator());

        _contextMock.Setup(ctx => ctx.Invitation).Returns(invitationMockSet.Object);

        var jobs = new List<Job> { new Job { Id = (int)model.JobId, Company = company } }.AsQueryable();
        var jobMockSet = new Mock<DbSet<Job>>();
        jobMockSet.As<IQueryable<Job>>().Setup(m => m.Provider).Returns(jobs.Provider);
        jobMockSet.As<IQueryable<Job>>().Setup(m => m.Expression).Returns(jobs.Expression);
        jobMockSet.As<IQueryable<Job>>().Setup(m => m.ElementType).Returns(jobs.ElementType);
        jobMockSet.As<IQueryable<Job>>().Setup(m => m.GetEnumerator()).Returns(jobs.GetEnumerator());

        _contextMock.Setup(ctx => ctx.Job).Returns(jobMockSet.Object);

        var people = new List<Person> { person }.AsQueryable();
        var personMockSet = new Mock<DbSet<Person>>();
        personMockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(people.Provider);
        personMockSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(people.Expression);
        personMockSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(people.ElementType);
        personMockSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(people.GetEnumerator());

        _contextMock.Setup(ctx => ctx.Person).Returns(personMockSet.Object);

        var employments = new List<Employment> { currentEmp }.AsQueryable();
        var employmentMockSet = new Mock<DbSet<Employment>>();
        employmentMockSet.As<IQueryable<Employment>>().Setup(m => m.Provider).Returns(employments.Provider);
        employmentMockSet.As<IQueryable<Employment>>().Setup(m => m.Expression).Returns(employments.Expression);
        employmentMockSet.As<IQueryable<Employment>>().Setup(m => m.ElementType).Returns(employments.ElementType);
        employmentMockSet.As<IQueryable<Employment>>().Setup(m => m.GetEnumerator()).Returns(employments.GetEnumerator());

        _contextMock.Setup(ctx => ctx.Employment).Returns(employmentMockSet.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
        new Claim("email", currentUserEmail),
        }));
        var httpContext = new DefaultHttpContext { User = user };
        _httpContextAccessorMock.Setup(_ => _.HttpContext).Returns(httpContext);

        _contextMock.Setup(ctx => ctx.SaveChanges()).Returns(1);

        _mailSenderMock.Setup(ms => ms.SendUsingAwsClientAsync(
            model.Email,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        )).ThrowsAsync(new Exception("Mail sending failed"));

        var exception = await Assert.ThrowsAsync<Exception>(() => _invitationService.CreateAsync(model, true));
        Assert.Equal("Mail sending failed", exception.Message);
       
    }


}