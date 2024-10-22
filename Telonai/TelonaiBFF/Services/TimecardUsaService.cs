namespace TelonaiWebApi.Services;

using AutoMapper;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface ITimecardUsaService
{
    IEnumerable<TimecardUsaModel> GetReport(string email, DateTime startTime, DateTime endTime);
    IEnumerable<TimecardUsaModel> GetReport(string email,int companyId,  DateTime startTime, DateTime endTime);
    IEnumerable<TimecardUsaModel> GetReport(int companyId, DateTime startTime, DateTime endTime);
    TimecardUsaModel GetOpenTimeCard(string email, int jobId);
    TimecardUsaModel GetOpenTimeCard(string email);
    TimecardUsaModel GetOpenTimeCard(int personId);
    Task<List<TimecardUsaModel>> GetTimeCardsByPayrollId(int companyId, int payrollId);
    Task<List<TimecardUsaModel>> GetTimeCardsByPayrollSequence(int companyId, int payrollSequece);
    Task<List<TimecardUsaModel>> GetTimeCardsByPayrollSequenceAndEmail(string email, int payrollSequece, int companyId);
    Task<List<TimecardUsaModel>> GetTimeCardsByPayrollIdAndEmployee(int companyId, int payrollId, int employeeId);
    Task<List<TimecardUsaModel>> GetTimeCardsByPayrollSequenceAndEmployee(int companyId, int payrollSequece, int employeeId);

    //Task<EmployeeTimeCardUsaDetail> GetEmployeeTimeCardList(int companyId, int employeeIdId);

    TimecardUsaModel GetById(int id);
    void Create(string emaild, int jobId);
    void Update(int id, TimecardUsaModel model);
    void Delete(int id);
    public void Update(List<TimecardUsaModel> model);
}

public class TimecardUsaService : ITimecardUsaService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly PayrollScheduleService _payrollScheduleService;

    public TimecardUsaService(DataContext context, IMapper mapper, PayrollScheduleService payrollScheduleService)
    {
        _context = context;
        _mapper = mapper;
        _payrollScheduleService = payrollScheduleService;
    }

    public IEnumerable<TimecardUsaModel> GetReport(string email, DateTime from, DateTime to)
    {
        var person = _context.Person.First(e => e.Email == email && !e.Deactivated);
        var obj = _context.TimecardUsa.Include(e => e.Job).Where(e => e.PersonId == person.Id && e.CreatedDate >= from && e.CreatedDate <= to);
        var result = _mapper.Map<IList<TimecardUsaModel>>(obj);
        return result;
    }

    public IEnumerable<TimecardUsaModel> GetReport(int companyId, DateTime from, DateTime to)
    {
        var obj = _context.TimecardUsa.Include(e => e.Job).Where(e => e.Job.CompanyId == companyId &&
        e.CreatedDate >= from && e.CreatedDate <= to);

        var result = _mapper.Map<IList<TimecardUsaModel>>(obj);
        return result;
    }

    public IEnumerable<TimecardUsaModel> GetReport(string email, int companyId, DateTime from, DateTime to)
    {
        var personId = _context.Person.First(e => e.Email == email && e.CompanyId== companyId && !e.Deactivated)?.Id;
        var obj = _context.TimecardUsa.Include(e=>e.Job).Where(e => e.PersonId==personId && e.Job.CompanyId == companyId && 
        e.CreatedDate >= from && e.CreatedDate <= to);
        var result = _mapper.Map<IList<TimecardUsaModel>>(obj);
        return result;
    }

    public async Task<List<TimecardUsaModel>> GetTimeCardsByPayrollSequence(int companyId, int payrollSequece)
    {
        Payroll currentPayroll;

        if (payrollSequece == 0)
            currentPayroll = await _context.Payroll.OrderByDescending(e => e.ScheduledRunDate).FirstOrDefaultAsync(e => e.CompanyId == companyId);
        else
            currentPayroll = _context.Payroll.OrderByDescending(e => e.ScheduledRunDate).Skip(payrollSequece).FirstOrDefault(e => e.CompanyId == companyId);

        var runDate = currentPayroll.ScheduledRunDate.ToDateTime(TimeOnly.MinValue);
        var obj = _context.TimecardUsa.Where(e => e.ClockIn < runDate && e.Job.CompanyId == companyId);
        var result = _mapper.Map<List<TimecardUsaModel>>(obj);
        return result;
    }
    public async Task<List<TimecardUsaModel>> GetTimeCardsByPayrollSequenceAndEmployee(int companyId, int payrollSequece, int employeeId)
    {
        Payroll currentPayroll;

        if (payrollSequece == 0)
            currentPayroll = await _context.Payroll.OrderByDescending(e => e.ScheduledRunDate).FirstOrDefaultAsync(e => e.CompanyId == companyId);
        else
            currentPayroll = _context.Payroll.OrderByDescending(e => e.ScheduledRunDate).Skip(payrollSequece).FirstOrDefault(e => e.CompanyId == companyId);

        var runDate = currentPayroll.ScheduledRunDate.ToDateTime(TimeOnly.MinValue);
        var obj = _context.TimecardUsa.Where(e => e.ClockIn < runDate && e.Job.CompanyId == companyId && e.PersonId==employeeId);
        var result = _mapper.Map<List<TimecardUsaModel>>(obj);
        return result;
    }
    public async Task<List<TimecardUsaModel>> GetTimeCardsByPayrollIdAndEmployee(int companyId, int payrollId, int employeeId)
    {
        var currentPayroll = _context.Payroll.Find(payrollId);

        var runDateTime = currentPayroll.ScheduledRunDate.ToDateTime(TimeOnly.MaxValue);
        var startDateTime = currentPayroll.StartDate.ToDateTime(TimeOnly.MinValue);

        var obj = _context.TimecardUsa.Where(e => e.ClockIn >= startDateTime && e.ClockIn < runDateTime
        && e.Job.CompanyId == companyId && e.PersonId == employeeId);

        var result = _mapper.Map<List<TimecardUsaModel>>(obj);
        return result;
    }

    public async Task<List<TimecardUsaModel>> GetTimeCardsByPayrollId(int companyId, int payrollId)
    {
        var currentPayroll = _context.Payroll.Find(payrollId);

        var runDateTime = currentPayroll.ScheduledRunDate.ToDateTime(TimeOnly.MaxValue);
        var startDateTime = currentPayroll.StartDate.ToDateTime(TimeOnly.MinValue);

        var obj = _context.TimecardUsa.OrderByDescending(e=>e.ClockIn).Where(e => e.ClockIn >= startDateTime && e.ClockIn < runDateTime
        && e.Job.CompanyId == companyId && !e.Person.Deactivated);

        var result = _mapper.Map<List<TimecardUsaModel>>(obj);
        return result;
    }
    public async Task<List<TimecardUsaModel>> GetTimeCardsByPayrollSequenceAndEmail(string email, int payrollSequece,  int companyId)
    {
        var personId = _context.Person.First(e => e.Email == email && e.CompanyId==companyId)?.Id;

        Payroll currentPayroll;

        if (payrollSequece == 0)
            currentPayroll = await _context.Payroll.OrderByDescending(e => e.ScheduledRunDate).FirstOrDefaultAsync(e => e.CompanyId == companyId);
        else
            currentPayroll = _context.Payroll.OrderByDescending(e => e.ScheduledRunDate).Skip(payrollSequece).FirstOrDefault(e => e.CompanyId == companyId);

        var runDate = currentPayroll.ScheduledRunDate.ToDateTime(TimeOnly.MinValue);
        var obj = _context.TimecardUsa.Where(e => e.ClockIn < runDate && e.Job.CompanyId == companyId && e.PersonId == personId);
        var result = _mapper.Map<List<TimecardUsaModel>>(obj);
        return result;
    }

    public TimecardUsaModel GetOpenTimeCard(string email, int jobId)
    {
        var person = _context.Person.First(e => e.Email == email && !e.Deactivated);
        var obj = _context.TimecardUsa.FirstOrDefault(e => e.PersonId == person.Id && e.JobId == jobId && e.ClockOut == null);
        if (obj == null)
            return null;

        return _mapper.Map<TimecardUsaModel>(obj);
    }

    public TimecardUsaModel GetOpenTimeCard(string email)
    {
        var person = _context.Person.First(e => e.Email == email && !e.Deactivated);
        var obj = _context.TimecardUsa.FirstOrDefault(e => e.PersonId == person.Id && e.ClockOut == null);
        if (obj == null)
            return null;

        return _mapper.Map<TimecardUsaModel>(obj);
    }

    public TimecardUsaModel GetOpenTimeCard(int personId)
    {
        var obj = _context.TimecardUsa.FirstOrDefault(e => e.PersonId == personId && e.ClockOut == null);
        if (obj == null)
            return null;

        return _mapper.Map<TimecardUsaModel>(obj);
    }

    public TimecardUsaModel GetById(int id)
    {
        var obj = getTimecardUsa(id);
        var result = _mapper.Map<TimecardUsaModel>(obj);
        return result;
    }

    public void Create(string email, int jobId)
    {
        if (string.IsNullOrEmpty(email))
             throw new AppException("You are not authorized to perform this action.");
       

        var person = _context.Person.First(e => e.Email == email && !e.Deactivated);
        var obj = _context.TimecardUsa.FirstOrDefault(e => e.PersonId == person.Id && e.ClockOut == null);
        if (obj != null)
        {
            var clockoutTime = DateTime.UtcNow;
            var span = clockoutTime - obj.ClockIn;
            obj = _context.TimecardUsa.FindAsync(obj.Id).Result;
            obj.ClockOut = clockoutTime;
            obj.HoursWorked = new TimeSpan(span.Days, span.Hours, span.Minutes, span.Seconds);
            _context.TimecardUsa.Update(obj);
        }
        else
        {
            obj = new TimecardUsa
            {
                PersonId = person.Id,
                JobId = jobId,
                ClockIn = DateTime.UtcNow
            };
            _context.TimecardUsa.Add(obj);
        }
        _context.SaveChanges();
    }

    public void Update(List<TimecardUsaModel> model)
    {
        model.ForEach(item =>
        {
            var dirty = false;
            var dto = getTimecardUsa(item.Id) ?? throw new KeyNotFoundException("Timecard not found");

            if (dto.IsLocked) return;

            if (dto.ClockIn-item.ClockIn > TimeSpan.FromMinutes(1))
            {
                dirty = true;
                dto.ClockIn = item.ClockIn;
            }
            if (item.ClockOut.HasValue)
            {
                if (!dto.ClockOut.HasValue || dto.ClockOut - item.ClockOut > TimeSpan.FromMinutes(1))
                {
                    dirty = true;
                    dto.ClockOut = item.ClockOut;
                    var span = item.ClockOut.Value - dto.ClockIn;
                    dto.HoursWorked = new TimeSpan(span.Days, span.Hours, span.Minutes, span.Seconds);
                }
            }
            if (dto.IsApproved != item.IsApproved)
            {
                dirty = true;
                dto.IsApproved = item.IsApproved;
            }
            if (dirty) _context.SaveChanges();
        });
    }

    public void Update(int id, TimecardUsaModel model)
    {
        var dto = getTimecardUsa(id) ?? throw new KeyNotFoundException("Timecard not found");

        _mapper.Map(model, dto);
        _context.TimecardUsa.Update(dto);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var dto = getTimecardUsa(id);
        _context.TimecardUsa.Remove(dto);
        _context.SaveChanges();
    }

    private TimecardUsa getTimecardUsa(int id)
    {
        var dto = _context.TimecardUsa.Find(id);
        return dto;
    }

    //public async Task<EmployeeTimeCardUsaDetail> GetEmployeeTimeCardList(int companyId, int employeeIdId)
    //{
    //    var employeeTimeCardHistory = new EmployeeTimeCardUsaDetail();
    //    var companyPayrollSchedule = _payrollScheduleService.GetLatestByCompanyId(companyId);
    //    if(companyPayrollSchedule != null)
    //    {
    //        employeeTimeCardHistory.EmployeePayrollScheduleName = companyPayrollSchedule.PayrollScheduleType;


    //    }
    //    else
    //    {
    //       return  employeeTimeCardHistory;
    //    }
    //}
}