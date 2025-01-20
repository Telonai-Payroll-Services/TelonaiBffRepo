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
    TimecardUsaModel GetOpenTimeCard(List<int> personIds);
    Task<List<TimecardUsaModel>> GetTimeCardsByPayrollId(int companyId, int payrollId);
    Task<List<TimecardUsaModel>> GetTimeCardsByPayrollSequence(int companyId, int payrollSequece);
    Task<List<TimecardUsaModel>> GetCurrentTimeCards(string email, int companyId);
    Task<List<TimecardUsaModel>> GetTimeCardsByPayrollIdAndEmployee(int companyId, int payrollId, int employeeId);
    Task<List<TimecardUsaModel>> GetTimeCardsByPayrollSequenceAndEmployee(int companyId, int payrollSequece, int employeeId);

    //Task<EmployeeTimeCardUsaDetail> GetEmployeeTimeCardList(int companyId, int employeeIdId);
    Task CheckOverdueClockOutsAsync();
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
    private readonly IMailSender _mailSender;
    public TimecardUsaService(DataContext context, IMapper mapper, IMailSender mailSender)
    {
        _context = context;
        _mapper = mapper;
        _mailSender = mailSender;
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

    public async Task<List<TimecardUsaModel>> GetTimeCardsByPayrollSequence(int companyId, int payrollSequence)
    {
        Payroll currentPayroll = null;

        if (payrollSequence == 0)
            currentPayroll = _context.Payroll.OrderByDescending(e => e.ScheduledRunDate)
                .FirstOrDefault(e => e.CompanyId == companyId && e.StartDate <= DateOnly.FromDateTime(DateTime.Today));
        else
            currentPayroll = _context.Payroll.OrderByDescending(e => e.ScheduledRunDate)
                .Where(e => e.CompanyId == companyId && e.StartDate <= DateOnly.FromDateTime(DateTime.Today))
                .Skip(payrollSequence).FirstOrDefault();

        var startDateTime = currentPayroll.StartDate.ToDateTime(TimeOnly.MinValue);
        var runDateTime = currentPayroll.ScheduledRunDate.ToDateTime(TimeOnly.MaxValue);

        var timecards = _context.TimecardUsa.Where(e => e.ClockIn < runDateTime && e.ClockIn >= startDateTime
        && e.Job.CompanyId == companyId);

        var result = _mapper.Map<List<TimecardUsaModel>>(timecards);
        return result;
    }
    public async Task<List<TimecardUsaModel>> GetTimeCardsByPayrollSequenceAndEmployee(int companyId, int payrollSequence, int employeeId)
    {
        Payroll currentPayroll = null;

        if (payrollSequence == 0)
            currentPayroll = _context.Payroll.OrderByDescending(e => e.ScheduledRunDate)
                .FirstOrDefault(e => e.CompanyId == companyId && e.StartDate <= DateOnly.FromDateTime(DateTime.Today));
        else
            currentPayroll = _context.Payroll.OrderByDescending(e => e.ScheduledRunDate)
                .Where(e => e.CompanyId == companyId && e.StartDate <= DateOnly.FromDateTime(DateTime.Today))
                .Skip(payrollSequence).FirstOrDefault();

        var startDateTime = currentPayroll.StartDate.ToDateTime(TimeOnly.MinValue);
        var runDateTime = currentPayroll.ScheduledRunDate.ToDateTime(TimeOnly.MaxValue);
        
        var timecards = _context.TimecardUsa.Where(e => e.ClockIn < runDateTime && e.ClockIn >= startDateTime
        && e.Job.CompanyId == companyId && e.PersonId == employeeId);

        var result = _mapper.Map<List<TimecardUsaModel>>(timecards);
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
        var payroll = _context.Payroll.Find(payrollId);

        var runDateTime = payroll.ScheduledRunDate.ToDateTime(TimeOnly.MaxValue);
        var startDateTime = payroll.StartDate.ToDateTime(TimeOnly.MinValue);

        var obj = _context.TimecardUsa.OrderByDescending(e=>e.ClockIn).Where(e => e.ClockIn >= startDateTime && e.ClockIn < runDateTime
        && e.Job.CompanyId == companyId && !e.Person.Deactivated);

        var result = _mapper.Map<List<TimecardUsaModel>>(obj);
        return result;
    }

    public async Task<List<TimecardUsaModel>> GetCurrentTimeCards(string email, int companyId)
    {
        var personId = _context.Person.FirstOrDefault(e => e.Email == email && e.CompanyId == companyId)?.Id;
        var today = DateOnly.FromDateTime(DateTime.Today);
        var currentPayroll = _context.Payroll.FirstOrDefault(e => e.CompanyId == companyId
            && e.StartDate <= today  && e.ScheduledRunDate >= today);
        
        var startDate = currentPayroll?.StartDate.ToDateTime(TimeOnly.MinValue);
        var obj = _context.TimecardUsa.Where(e => e.ClockIn >= startDate && e.Job.CompanyId == companyId 
        && e.PersonId == personId);
        
        var result = _mapper.Map<List<TimecardUsaModel>>(obj);
        return result;
    }

    public TimecardUsaModel GetOpenTimeCard(string email, int jobId)
    {
        var personIds = _context.Person.Where(e => e.Email == email && !e.Deactivated).Select(e=>e.Id);
        var obj = _context.TimecardUsa.FirstOrDefault(e => personIds.Contains(e.PersonId) && e.JobId == jobId && e.ClockOut == null);
        if (obj == null)
            return null;

        return _mapper.Map<TimecardUsaModel>(obj);
    }

    public TimecardUsaModel GetOpenTimeCard(string email)
    {
        var personIds = _context.Person.Where(e => e.Email == email && !e.Deactivated).Select(e => e.Id);
        var obj = _context.TimecardUsa.FirstOrDefault(e => personIds.Contains(e.PersonId) && e.ClockOut == null);
        
        if (obj == null)
            return null;

        return _mapper.Map<TimecardUsaModel>(obj);
    }

    public TimecardUsaModel GetOpenTimeCard(List<int> personIds)
    {
        var obj = _context.TimecardUsa.FirstOrDefault(e => personIds.Contains(e.PersonId) && e.ClockOut == null);
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
       

        var personId = _context.Employment.First(e => e.Person.Email == email && e.JobId == jobId && !e.Deactivated).PersonId;
        var obj = _context.TimecardUsa.FirstOrDefault(e => e.PersonId == personId && e.ClockOut == null);
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
                PersonId = personId,
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


    public async Task CheckOverdueClockOutsAsync()
    {
        var now = DateTime.UtcNow;
        var overdueTimeCards = _context.TimecardUsa.Include(c => c.Person)
            .Where(tc => tc.ClockOut == null && (now - tc.ClockIn).TotalHours >= 8)
            .ToList();

        foreach (var timeCard in overdueTimeCards)
        {
            var hoursWorked = (now - timeCard.ClockIn).TotalHours;
            string notificationNote = string.Empty;

            if (hoursWorked >= 24)
            {
                // Clock out the user
                timeCard.ClockOut = timeCard.ClockIn.AddHours(24);
                await _context.SaveChangesAsync();

                await _mailSender.SendUsingAwsClientAsync(timeCard.Person.Email, $"24 Hour Clock Out Notification",
               CreateHtmlEmailBody(hoursWorked, $"{timeCard.Person.FirstName} {timeCard.Person.LastName}"),
               CreateTextEmailBody(hoursWorked, $"{timeCard.Person.FirstName} {timeCard.Person.LastName}"));
                notificationNote = $"User clocked out automatically after 24 hours for timecard ID: {timeCard.Id}";
            }
            else if (hoursWorked >= 16)
            {
                await _mailSender.SendUsingAwsClientAsync(timeCard.Person.Email, $"16 Hour Clock Out Notification",
              CreateHtmlEmailBody(hoursWorked, $"{timeCard.Person.FirstName} {timeCard.Person.LastName}"),
              CreateTextEmailBody(hoursWorked, $"{timeCard.Person.FirstName} {timeCard.Person.LastName}"));

                notificationNote = $"16-hour clock out notification sent for timecard ID: {timeCard.Id}";
            }
            else if (hoursWorked >= 8)
            {
                await _mailSender.SendUsingAwsClientAsync(timeCard.Person.Email, $"8 Hour Clock Out Notification",
              CreateHtmlEmailBody(hoursWorked, $"{timeCard.Person.FirstName} {timeCard.Person.LastName}"),
              CreateTextEmailBody(hoursWorked, $"{timeCard.Person.FirstName} {timeCard.Person.LastName}"));

                notificationNote = $"8-hour clock out notification sent for timecard ID: {timeCard.Id}";

            }
            if (!string.IsNullOrEmpty(notificationNote))
            {
                await AddNoteAsync(timeCard.Id, notificationNote);
            }
        }
    }

    private static string CreateHtmlEmailBody(double hours, string recieverName)
    {
        string formattedHours = hours.ToString("F2");
        string message = $"<h1>Please clock out</h1>"
         + $"Dear {recieverName}"
         + $"<br/><p>We noticed that you have not clocked out after {formattedHours} hours.</p>";
        if (hours < 16) 
        { 
            message += "<p>If you are still working please ignore this notification.</p>"; 
        }
        return message;

    }
    private static string CreateTextEmailBody(double hours, string recieverName)
    {
        string formattedHours = hours.ToString("F2");
        string message = "Please clock out\r\n"
                + $"Dear {recieverName},\r\n"
               + $"<br/><p>We noticed that you have not clocked out after {formattedHours} hours.</p>";
        if (hours < 16) 
        { 
            message += "\r\nIf you are still working please ignore this notification."; 
        }
        return message;
    }
    public async Task AddNoteAsync(int timeCardUsaId, string note)
    {
        var timecardUsaNote = new TimecardUsaNote
        {
            TimecardUsaId = timeCardUsaId,
            Note = note,
            CreatedBy = "System",
            CreatedDate = DateTime.UtcNow
        };

        _context.TimecardUsaNote.Add(timecardUsaNote);
        await _context.SaveChangesAsync();
    }


}