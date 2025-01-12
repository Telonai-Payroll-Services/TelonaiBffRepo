namespace TelonaiWebApi.Services;

using Amazon.S3;
using AutoMapper;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IInvitationService<InvitationModel, Invitation>: IDataService<InvitationModel, Invitation>
{
    InvitationModel GetAllByActivaionCodeAndInviteeEmail(string activationCode, string email);
    Invitation GetAllByActivaionCodeAndInviteeEmail2(string activationCode, string email);
    Invitation GetByActivaionCodeAndInviteeEmail(string activationCode, string email, string taxId);
    InvitationModel GetByActivaionCodeAndInviteeEmail(string activationCode, string email);
    IList<InvitationModel> GetByInviteeEmail(string email);
    InvitationModel GetById(Guid id);
    IList<InvitationModel> GetByJobId(int jobId);
    Task<Invitation> CreateAsync(InvitationModel model, bool IsForEmployee = false);
    Task DeleteAsync(Guid id);
    Task UpdateAsync(Invitation dto);
    Task UpdateAsync(Guid id, InvitationModel model);
    IList<InvitationStatusModel> GetStatusByCompanyId(int companyId);
    Task SendQuoteAsync(QuoteModel model);
}

public class InvitationService : IInvitationService<InvitationModel, Invitation>
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IMailSender _mailSender;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public InvitationService(DataContext context, IMapper mapper, IMailSender mailSender, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _mailSender = mailSender;
        _httpContextAccessor = httpContextAccessor;
    }
    public IList<InvitationModel> Get()
    {
        var dto = _context.Invitation;
        return _mapper.Map<IList<InvitationModel>>(dto);
    }

    public InvitationModel GetById(Guid id)
    {
        var dto = _context.Invitation.Find(id);
        return _mapper.Map<InvitationModel>(dto);
    }
    public InvitationModel GetById(int id)
    {
        throw new NotImplementedException();
    }
    public IList<InvitationModel> GetByJobId(int jobId)
    {
        var dto =  _context.Invitation.Where(e=> e.JobId==jobId).ToList();

        return _mapper.Map<IList<InvitationModel>>(dto);
    }

    public IList<InvitationStatusModel> GetStatusByCompanyId(int companyId)
    {
        var dto = _context.Invitation.Where(e => e.Job.CompanyId == companyId).ToList();
      
        return _mapper.Map<IList<InvitationStatusModel>>(dto);
    }
    public IList<InvitationModel> GetByInviteeEmail(string email)
    {
        var dto = _context.Invitation.Where(e => e.Email == email).ToList();

        return _mapper.Map<IList<InvitationModel>>(dto);
    }

    public InvitationModel GetAllByActivaionCodeAndInviteeEmail(string activationCode, string email)
    {
        var dto = _context.Invitation.Include(e => e.Job).Include(e => e.Country)
            .FirstOrDefault(e => e.Id.ToString().EndsWith(activationCode.ToLower()) &&
        e.Email == email && e.ExpirationDate > DateTime.UtcNow);

        return _mapper.Map<InvitationModel>(dto ?? throw new AppException("Invalid Activation Code or Email"));
    }
    public Invitation GetAllByActivaionCodeAndInviteeEmail2(string activationCode, string email)
    {
        var dto = _context.Invitation.Include(e => e.Job).Include(e => e.Country)
            .FirstOrDefault(e => e.Id.ToString().EndsWith(activationCode.ToLower()) &&
        e.Email.ToLower() == email.ToLower() && e.ExpirationDate > DateTime.UtcNow);

        return dto ?? throw new AppException("Invalid Activation Code or Email");
    }
    public Invitation GetByActivaionCodeAndInviteeEmail(string activationCode, string email, string taxId)
    {
        var dto = _context.Invitation.FirstOrDefault(e => e.Id.ToString().EndsWith(activationCode.ToLower()) &&
        e.Email == email && e.ExpirationDate > DateTime.UtcNow && e.TaxId == taxId);

        return dto ?? throw new AppException("Invalid Activation Code");
    }
    public InvitationModel GetByActivaionCodeAndInviteeEmail(string activationCode, string email)
    {
        var dto = _context.Invitation.FirstOrDefault(e => e.Id.ToString().EndsWith(activationCode.ToLower()) &&
        e.Email == email && e.ExpirationDate > DateTime.UtcNow);

        return _mapper.Map<InvitationModel>(dto ?? throw new AppException("Invalid Activation Code or Email"));
    }

    public Task<Invitation> CreateAsync(InvitationModel model)
    {
        throw new NotImplementedException();
    }
    public async Task<Invitation> CreateAsync(InvitationModel model, bool IsForEmployee)
    {
        var somethingChanged = false;
        var isNewInvitation = false;
        var companyName = model.Company;

        var invitation  = _context.Invitation.FirstOrDefault(x => x.Email == model.Email && x.JobId == model.JobId && 
        x.TaxId == model.TaxId && x.ExpirationDate > DateTime.UtcNow);

        if (invitation == null)
        {
            isNewInvitation = true;
            invitation = _mapper.Map<Invitation>(model);
            invitation.ExpirationDate = DateTime.UtcNow.AddDays(10);
            invitation.Id = Guid.NewGuid();
            _context.Invitation.Add(invitation);
            somethingChanged = true;
        }

        if (IsForEmployee)
        {
            var company = _context.Job.Include(e => e.Company).First(e => e.Id == model.JobId).Company;
            companyName= company.Name;

            var person = _context.Person.FirstOrDefault(e => e.CompanyId == company.Id && e.Email == model.Email && !e.Deactivated);
            if (person == null)
            {
                person = new Person
                {
                    CompanyId = company.Id,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    INineVerificationStatusId= (int)INineVerificationStatusModel.INineNotSubmitted,
                };
                _context.Person.Add(person);
                try
                {
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {

                }
                somethingChanged = false ;
            }
            var emp = _context.Employment.FirstOrDefault(e => e.JobId == model.JobId.Value && e.PersonId == person.Id &&
            !e.EndDate.HasValue && !e.Deactivated);
            if (emp == null)
            {
                emp = new Employment
                {
                    PersonId = person.Id,
                    JobId = model.JobId.Value,
                    IsTenNinetyNine = model.Employment.IsTenNinetyNine,
                    PayRate = model.Employment.PayRate,
                    PayRateBasisId = model.Employment.PayRateBasisId,
                    IsSalariedOvertimeExempt = model.Employment.IsSalariedOvertimeExempt,
                };
                _context.Employment.Add(emp);
                somethingChanged = true;
            }

            //update current user's status
            var currentUserEmail = _httpContextAccessor.HttpContext.User.Claims.First(e => e.Type == "email").Value;
            var currentEmp = _context.Employment.First(e => e.Person.Email == currentUserEmail && e.Job.CompanyId == company.Id && !e.Deactivated);

            if (currentEmp.SignUpStatusTypeId < (int)SignUpStatusTypeModel.EmployeeAdditionStarted)
            {
                currentEmp.SignUpStatusTypeId = (int)SignUpStatusTypeModel.EmployeeAdditionStarted;
                _context.Employment.Update(currentEmp);
                somethingChanged = true;
            }
        }

        if (somethingChanged)
        {
            if(_context.SaveChanges() < 1)
                throw new AppException("Invitation cannot be completed.");
        }

        try
        {
            var activationCode = GetActivationCode(invitation.Id);
            await _mailSender.SendUsingAwsClientAsync(model.Email, $"Activation Request by {companyName}",
                CreateInvitationHtmlEmailBoby(activationCode.ToUpper(), companyName, $"{model.FirstName} {model.LastName}"),
                CreateInvitationTextEmailBoby(activationCode.ToUpper(), companyName, $"{model.FirstName} {model.LastName}"));
            return invitation;
        }
        catch
        {
            if (isNewInvitation)
            {
                _context.Invitation.Remove(invitation);
                _context.SaveChanges();
            }
            throw;
        }
    }

    public async Task UpdateAsync(Guid id, InvitationModel model)
    {
        var result = await _context.Invitation.FindAsync(id) ?? throw new AppException("Invitation not found");
        _mapper.Map(model, result);
        _context.Invitation.Update(result);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateAsync(Invitation dto)
    {
        _context.Invitation.Update(dto);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var result = await _context.Invitation.FindAsync(id) ?? throw new AppException("Invitation not found");
        _context.Invitation.Remove(result);
        _context.SaveChanges();
    }
    public async Task SendQuoteAsync(QuoteModel model)
    {
        await _mailSender.SendUsingAwsClientAsync(model.CustomerEmail, $"Quote from Telonai",
            CreateQuoteHtmlEmailBoby(model),
            CreateQuoteTextEmailBoby(model));
    }

    public async Task UpdateAsync(int id, InvitationModel model)
    {
        throw new NotImplementedException();
    }
    public async Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    private static string CreateQuoteTextEmailBoby(QuoteModel model)
    {
        var annualCost = model.MonthlyCost * 12;
        var annualCostAfterDiscount = annualCost / 2;
        var monthlyCostAfterDiscount = annualCostAfterDiscount / 12;

        return $"Dear {model.CustomerName}.\r\n"
        + $"Thank you for reaching out to us requesting a quote for our payroll solution. \r\n"
        + $"It is my great pleasure to quote you ${model.MonthlyCost} per month for using our payroll system for your entire team of "
        + $"{model.NumberOfEmployees} people. \r\n"
        + $"I can apply an additional {model.DiscountPercentage}% discount, if you choose to be billed annually. With the additional discount,"
        + $" you will be charged only ${annualCostAfterDiscount} for the entire year, which in my opinion is a great "
        + "saving for your business. \r\n"
        + $"Here is a summary of my offer. \r\n"
        + $"${model.MonthlyCost} per month (${annualCost} per year) if billed monthly. \r\n"
        + $"${annualCostAfterDiscount} per year (${monthlyCostAfterDiscount} per month) if billed annually. \r\n \r\n"
        + $"To make a payment and secure this offer instantly click on the link below and complete the payment form. \r\n"
        + $"https://telonai.com/subscription{model.AgentId} \r\n \r\n"
        + $"Thanks again and if you have any questions, do not hesitate to let us know by replying to this email. \r\n";

    }

    private static string CreateQuoteHtmlEmailBoby(QuoteModel model)
    {
        var annualCost = model.MonthlyCost * 12;
        var annualCostAfterDiscount = annualCost / 2;
        var monthlyCostAfterDiscount = annualCostAfterDiscount / 12;

        return $"Dear {model.CustomerName},  </br><p>"
        + $"Thank you for reaching out to us requesting a quote for our payroll solution.>/br?"
        + $"It is my great pleasure to quote you <u><strong>${model.MonthlyCost}</strong></u> per month for using our payroll system for your entire team of "
        + $"{model.NumberOfEmployees} people. \r\n"
        + $"I can apply an additional <u><strong>{model.DiscountPercentage}%</strong></u> discount, if you choose to be billed annually. With the additional discount,"
        + $" you will be charged only <u><strong>${annualCostAfterDiscount}</strong></u> for the entire year, which in my opinion is a great "
        + "saving for your business. \r\n"
        + $"Here is a summary of my offer. \r\n"
        + $"$<u><strong>{model.MonthlyCost}</strong></u> per month (${annualCost} per year) if billed monthly. \r\n"
        + $"$<u><strong>{annualCostAfterDiscount}</strong></u> per year (${monthlyCostAfterDiscount} per month) if billed annually. \r\n \r\n"
        + $"To make a payment and secure this offer instantly click on the link below and complete the payment form. \r\n"
        + $"https://telonai.com/subscription{model.AgentId} \r\n \r\n"
        + $"Thanks again and if you have any questions, do not hesitate to let us know by replying to this email. \r\n";
    }

    private static string CreateInvitationTextEmailBoby(string activationCode, string senderCompanyName, string recieverName)
    {
        return "Activate your account\r\n"
                + $"Dear {recieverName},\r\n"
                + $"You are invited by {senderCompanyName} to activate your Telonai account. "
                + $"To activate your account,  download and install the Telonai app. "
                + "If you are an iOS (iPhone or iPad) user, download the app from https://apps.apple.com/us/app/telonai/id6738379955 .\r\n"
                + "If you are an Android user, download the app from https://play.google.com/store/apps/details?id=com.telonai.app .\r\n"
                + $"When prompted for activation code, please enter {activationCode} .";                
    }
    private static string CreateInvitationHtmlEmailBoby(string activationCode, string senderCompanyName, string recieverName)
    {
        return $"<h1>Activate your account</h1>" 
         + $"Dear {recieverName}, </br><p>You are invited by <strong>{senderCompanyName}</strong> to activate your Telonai account. " 
         + $"<br/>To activate your account, download and install the <strong>Telonai</strong> app."
         + "<br/>If you are an iOS (iPhone or iPad) user, download the app from:  <a href='https://apps.apple.com/us/app/telonai/id6738379955'> App Store </a> ."
         + "<br/>If you are an Android user, download the app from: <a href='https://play.google.com/store/apps/details?id=com.telonai.app'> Google Play </a> ."
         + $"<br/>When prompted for activation code, please enter <strong>{activationCode}</strong> .";
    }
    private static string GetActivationCode(Guid invitationId)
    {
        return invitationId.ToString().Substring(invitationId.ToString().Length - 8);
    }
}