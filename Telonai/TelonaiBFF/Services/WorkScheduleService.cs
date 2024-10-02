namespace TelonaiWebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IWorkScheduleService
{
    IEnumerable<WorkScheduleModel> GetReport(string email,int jobId,  DateOnly fromDate, DateOnly toDate);
    IEnumerable<WorkScheduleModel> GetReport(int personId, int jobId, DateOnly fromDate, DateOnly toDate);
    IEnumerable<WorkScheduleModel> GetReportByJobId(int jobId, DateOnly fromDate, DateOnly toDate);
    
    WorkScheduleModel GetById(int id);
    IEnumerable<WorkScheduleModel> GetCurrentForUser(string email, int jobId);

    IEnumerable<WorkScheduleModel> GetCurrentByPersonIdAndJobId(int personId, int  jobId);
    IEnumerable<WorkScheduleModel> GetCurrentByPersonId(int personId);
    IEnumerable<WorkScheduleModel> GetCurrentForUser(string email);

    void Create(int personId, int jobId, DateOnly scheduledDate, string startTime, string endTime);
    void Update(int id, WorkScheduleModel model);
    void Delete(int id);
}

public class WorkScheduleService : IWorkScheduleService
{
    private DataContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<WorkScheduleService> _logger;

    public WorkScheduleService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public IEnumerable<WorkScheduleModel> GetReportByJobId(int jobId, DateOnly from, DateOnly to)
    {
        var obj = _context.WorkSchedule.Include(e => e.Job).Where(e => e.JobId == jobId
        && e.ScheduledDate >= from && e.ScheduledDate <= to);

        var result = _mapper.Map<IList<WorkScheduleModel>>(obj);
        return result;

    }

    public IEnumerable<WorkScheduleModel> GetReport(string email, int jobId, DateOnly from, DateOnly to)
    {
        var person = _context.Person.First(e => e.Email == email && !e.Deactivated);
        var obj = _context.WorkSchedule.Include(e => e.Job).Where(e => e.PersonId == person.Id && e.JobId == jobId
        && e.ScheduledDate >= from && e.ScheduledDate <= to);

        var result = _mapper.Map<IList<WorkScheduleModel>>(obj);
        return result;
    }

    public IEnumerable<WorkScheduleModel> GetReport(int personId, int jobId, DateOnly from, DateOnly to)
    {
        var obj = _context.WorkSchedule.Include(e => e.Job).Where(e => e.PersonId == personId && e.JobId == jobId
        && e.ScheduledDate >= from && e.ScheduledDate <= to);

        var result = _mapper.Map<IList<WorkScheduleModel>>(obj);
        return result;
    }

    public IEnumerable<WorkScheduleModel> GetReport(int jobId, DateTime from, DateTime to)
    {
        var obj = _context.WorkSchedule.Include(e => e.Job).Where(e => e.JobId == jobId && e.CreatedDate >= from && e.CreatedDate <= to);
        var result = _mapper.Map<IList<WorkScheduleModel>>(obj);
        return result;
    }

    public IEnumerable<WorkScheduleModel> GetReport(string email, int jobId, DateTime from, DateTime to)
    {
        var personId = _context.Person.First(e => e.Email == email && !e.Deactivated)?.Id;
        var obj = _context.WorkSchedule.Include(e=>e.Job).Where(e => e.PersonId==personId && e.JobId == jobId && 
        e.CreatedDate >= from && e.CreatedDate <= to);
        var result = _mapper.Map<IList<WorkScheduleModel>>(obj);
        return result;
    }

    public IEnumerable<WorkScheduleModel> GetCurrentByPersonId(int personId)
    {
        var obj = _context.WorkSchedule.FirstOrDefault(e => e.PersonId == personId  && 
        e.ScheduledDate >=DateOnly.FromDateTime(DateTime.Now));
        if (obj == null)
            return null;

        return _mapper.Map<IList<WorkScheduleModel>>(obj);
    }

    public IEnumerable<WorkScheduleModel> GetCurrentForUser(string email)
    {
        var p=_context.Person.FirstOrDefault(e => e.Email == email && !e.Deactivated);
        if (p == null) return null;

        var obj = _context.WorkSchedule.FirstOrDefault(e => e.PersonId == p.Id &&
        e.ScheduledDate >= DateOnly.FromDateTime(DateTime.Now));
        if (obj == null)
            return null;

        return _mapper.Map<IList<WorkScheduleModel>>(obj);
    }

    public IEnumerable<WorkScheduleModel> GetCurrentForUser(string email, int jobId)
    {
        var p = _context.Person.FirstOrDefault(e => e.Email == email && !e.Deactivated);
        if (p == null) return null;

        var obj = _context.WorkSchedule.FirstOrDefault(e => e.PersonId == p.Id && e.JobId== jobId &&
        e.ScheduledDate >= DateOnly.FromDateTime(DateTime.Now));
        if (obj == null) return null;

        return _mapper.Map<IList<WorkScheduleModel>>(obj);
    }
    public IEnumerable<WorkScheduleModel> GetCurrentByPersonIdAndJobId(int personId, int jobId)
    {
        var obj = _context.WorkSchedule.FirstOrDefault(e => e.PersonId == personId && e.JobId == jobId 
        && e.ScheduledDate >= DateOnly.FromDateTime(DateTime.Now));
        if (obj == null)
            return null;

        return _mapper.Map<IList<WorkScheduleModel>>(obj);
    }

    public WorkScheduleModel GetCurrentSchedule(string email)
    {
        var person = _context.Person.First(e => e.Email == email && !e.Deactivated);
        var obj = _context.WorkSchedule.FirstOrDefault(e => e.PersonId == person.Id
         && e.ScheduledDate >= DateOnly.FromDateTime(DateTime.Now)); 
        if (obj == null)
            return null;

        return _mapper.Map<WorkScheduleModel>(obj);
    }

    public WorkScheduleModel GetCurrentSchedule(int personId)
    { 
        var obj = _context.WorkSchedule.FirstOrDefault(e => e.PersonId == personId
         && e.ScheduledDate >= DateOnly.FromDateTime(DateTime.Now));
        if (obj == null)
            return null;

        return _mapper.Map<WorkScheduleModel>(obj);
    }

    public WorkScheduleModel GetById(int id)
    {
        var obj = getWorkSchedule(id);
        var result = _mapper.Map<WorkScheduleModel>(obj);
        return result;
    }

    public void Create(int personId, int jobId, DateOnly scheduledDate, string startTime,  string endTime)
    {

        var emp = _context.Employment.Where(e => e.PersonId == personId && !e.Deactivated);
        if (!emp.Any(e => e.JobId == jobId))
            throw new InvalidOperationException("Emplouyee not found.");

        var obj = new WorkSchedule
            {
                PersonId = personId,
                JobId = jobId,
                ScheduledDate = scheduledDate,
                StartTime = startTime,
                EndTime = endTime
            };
            _context.WorkSchedule.Add(obj);
        
        _context.SaveChanges();
    }

    public void Update(int id, WorkScheduleModel model)
    {
        var dto = getWorkSchedule(id) ?? throw new KeyNotFoundException("Schedule not found");
        dto.Accepted = model.Accepted;
        dto.ScheduledDate= model.ScheduledDate;
        dto.StartTime= model.StartTime;
        dto.EndTime= model.EndTime;
        
        _context.WorkSchedule.Update(dto);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var dto = getWorkSchedule(id);
        _context.WorkSchedule.Remove(dto);
        _context.SaveChanges();
    }

    private WorkSchedule getWorkSchedule(int id)
    {
        var dto = _context.WorkSchedule.Find(id);
        return dto;
    }
}