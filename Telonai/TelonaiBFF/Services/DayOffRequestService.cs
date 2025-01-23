namespace TelonaiWebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IDayOffRequestService<Tmodel, Tdto> : IDataService<Tmodel, Tdto>
{
    List<DayOffRequestModel> GetByEmploymentId(int employmentIf);
    List<DayOffRequestModel> GetByPersonId(int personId);
    List<DayOffRequestModel> GetByCompanyId(int companyId);
    List<DayOffRequestModel> GetPendingRequestsByCompanyId(int companyId);
    Task<bool> ApproveDayOffRequest(ApproveDayOffRequest approveDayOffRequest, DayOffRequest dayOffRequest);
    Task<bool> CancelDayOffRequest(int dayOffRequestId);
    DayOffRequest GetDayOffRequestDetail(int id);
    Task<List<DayOffRequest>> GetUnpaidDaysOffForPayrollSchedule(int companyId, DateOnly payrollStartDate, DateOnly payrollRunDate);
}

public class DayOffRequestService : IDayOffRequestService<DayOffRequestModel,DayOffRequest>
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IScopedAuthorization _scopedAuthorization;


    public DayOffRequestService(DataContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor,
        IScopedAuthorization scopedAuthorization)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _scopedAuthorization = scopedAuthorization;
    }

    public IList<DayOffRequestModel> Get()
    {
        var obj =  _context.DayOffRequest;        
        var result=_mapper.Map<IList<DayOffRequestModel>>(obj);
        return result;
    }

    public DayOffRequestModel GetById(int id)
    {
        var obj = GetDayOff(id);

        var email = _httpContextAccessor.HttpContext.User.Claims.First(e => e.Type == "email").Value.ToLower();
        var emp = _context.Employment.FirstOrDefault(e => e.Id ==obj.EmploymentId
        && e.Person.Email.ToLower() == email);

        if (emp == null)
            return null;

        _scopedAuthorization.ValidateByJobId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, emp.JobId);

        var result = _mapper.Map<DayOffRequestModel>(obj);
        return result;
    }
    public List<DayOffRequestModel> GetByEmploymentId(int employmentId)
    {
        var email = _httpContextAccessor.HttpContext.User.Claims.First(e => e.Type == "email").Value.ToLower();
        var emp = _context.Employment.FirstOrDefault(e => e.Id == employmentId 
        && e.Person.Email.ToLower() == email);

        if (emp == null)
            return new List<DayOffRequestModel>();

        _scopedAuthorization.ValidateByJobId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, emp.JobId);

        var obj = _context.DayOffRequest.Where(e=>e.EmploymentId== employmentId && !e.IsCancelled)?.ToList();

        var result = _mapper.Map<List<DayOffRequestModel>>(obj);
        return result;
    }

    public List<DayOffRequestModel> GetByPersonId(int personId)
    {
        var email = _httpContextAccessor.HttpContext.User.Claims.First(e => e.Type == "email").Value.ToLower();
        var emp = _context.Employment.FirstOrDefault(e => e.Person.Email.ToLower() == email
        && e.Person.Id==personId);

        if (emp == null)
            throw new InvalidOperationException("The logged in user is not the same as the request is made for.");

        _scopedAuthorization.ValidateByJobId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, emp.JobId);

        var dayOffRequest = _context.DayOffRequest.Where(e => e.Employment.Id == emp.Id && !e.IsCancelled)?.ToList();
        var result = _mapper.Map<List<DayOffRequestModel>>(dayOffRequest);
        return result;
    }


    public List<DayOffRequestModel> GetByCompanyId(int companyId)
    {
        var thirtyDaysAgo =DateOnly.FromDateTime(DateTime.Today).AddDays(-31);
        var obj = _context.DayOffRequest.Where(e => e.Employment.Job.CompanyId == companyId && !e.IsCancelled &&
        e.ToDate> thirtyDaysAgo).Include(d => d.Employment.Person)?.ToList();

        var result = _mapper.Map<List<DayOffRequestModel>>(obj);
        return result;
    }

    public List<DayOffRequestModel> GetPendingRequestsByCompanyId(int companyId)
    {
        var thirtyDaysAgo = DateOnly.FromDateTime(DateTime.Today).AddDays(-31);
        var obj = _context.DayOffRequest.Where(e => e.Employment.Job.CompanyId == companyId && !e.IsCancelled &&
        e.IsApproved==null && e.ToDate > thirtyDaysAgo)?.ToList();

        var result = _mapper.Map<List<DayOffRequestModel>>(obj);
        return result;
    }

    public async Task<DayOffRequest> CreateAsync(DayOffRequestModel model)
    {
        var email = _httpContextAccessor.HttpContext.User.Claims.First(e => e.Type == "email").Value.ToLower();
        var emp = _context.Employment.FirstOrDefault(e => e.Person.Email.ToLower() == email
        && e.Id == model.EmploymentId);

        if (emp == null) {
            throw new InvalidOperationException("The logged in user is not the same as the request is made for.");
        }

        _scopedAuthorization.ValidateByJobId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, emp.JobId);

        var dayOff = _context.DayOffRequest.FirstOrDefault(e => e.EmploymentId == model.EmploymentId
        && !e.IsCancelled && model.FromDate <= e.ToDate && e.FromDate <= model.ToDate);

        if (dayOff == null)
        {
            dayOff = _mapper.Map<DayOffRequest>(model);
            _context.DayOffRequest.Add(dayOff);
            _context.SaveChanges();
            return dayOff;
        }

        throw new AppException("You already have a day-off request in the range you selected.");
    }

    public async Task UpdateAsync(int id, DayOffRequestModel model)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(int id)
    {
        var dayOff = GetDayOff(id) ?? throw new AppException("Day-off request not found");

        var email = _httpContextAccessor.HttpContext.User.Claims.First(e => e.Type == "email").Value.ToLower();
        var emp = _context.Employment.FirstOrDefault(e => e.Person.Email.ToLower() == email
        && e.Id == dayOff.EmploymentId);

        if (emp == null)        
            throw new AppException("Day-off request not found");
        
        _scopedAuthorization.ValidateByJobId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, emp.JobId);

        dayOff.IsCancelled = true;
        _context.DayOffRequest.Update(dayOff);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ApproveDayOffRequest(ApproveDayOffRequest approveDayOffRequest, DayOffRequest dayOffRequest)
    {
        if (dayOffRequest.IsApproved == null && dayOffRequest.IsCancelled != true)
        {
            dayOffRequest.IsApproved = approveDayOffRequest.IsApproved;
            _context.Update(dayOffRequest);
            return await _context.SaveChangesAsync() > 0;
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> CancelDayOffRequest(int id)
    {
        var dayOffRequest = _context.DayOffRequest.Find(id);
        if (dayOffRequest != null)
        {
            if (dayOffRequest.IsCancelled == false && DateOnly.FromDateTime(DateTime.Now).CompareTo(dayOffRequest.FromDate) < 0)
            {
                dayOffRequest.IsCancelled = true;
                _context.Update(dayOffRequest);
                return await _context.SaveChangesAsync() > 0;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public DayOffRequest GetDayOffRequestDetail(int id)
    {
        var dayOffRequest = _context.DayOffRequest.Include(e=>e.Employment).Where(d => d.Id == id);
        if (dayOffRequest != null)
        {
            return dayOffRequest.Cast<DayOffRequest>().FirstOrDefault();
        }
        else
        {
            return null;
        }
    }

    public async Task<List<DayOffRequest>> GetUnpaidDaysOffForPayrollSchedule(int companyId, DateOnly payrollStartDate, DateOnly payrollRunDate)
    {
        var unpaidDayOff = await _context.DayOffRequest.Where(d => d.DayOffPayTypeId == (int)DayOffPayTypeModel.Unpaid && 
                                                        d.Employment.Job.CompanyId== companyId &&
                                                        d.FromDate >= payrollStartDate && 
                                                        d.ToDate <= payrollRunDate).ToListAsync();
        return unpaidDayOff;
    }

    private DayOffRequest GetDayOff(int id)
    {
        return _context.DayOffRequest.Find(id);
    }


}