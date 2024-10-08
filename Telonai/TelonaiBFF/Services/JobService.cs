namespace TelonaiWebApi.Services;

using AutoMapper;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IJobService<JobModel, Job> : IDataService<JobModel, Job>
{
    Task<Job> CreateAsync(JobModel model, string username, string email);
}
public class JobService : IJobService<JobModel,Job>
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public JobService(DataContext context, IMapper mapper, IUserService userService)
    {
        _context = context;
        _mapper = mapper;
        _userService = userService;
    }

    public IList<JobModel> Get()
    {
        var obj =  _context.Job;        
        var result=_mapper.Map<IList<JobModel>>(obj);
        return result;
    }

    public JobModel GetById(int id)
    {
        var obj = GetJob(id);
        var result = _mapper.Map<JobModel>(obj);
        return result;
    }

    public async Task<Job> CreateAsync(JobModel model)
    {
     throw new NotImplementedException();
    }

    public async Task<Job> CreateAsync(JobModel model, string username, string email)
    {
        var job = _context.Job.FirstOrDefault(e => e.CompanyId == model.CompanyId && e.LocationName == model.LocationName);
        if (job == null)
        {
            job = _mapper.Map<Job>(model);
            var newJob = _context.Job.Add(job);
            await _context.SaveChangesAsync();            
        }

        //Add the Job Id to current user's authorization scope
        var user = new User
        {
            Email = email,
            Username = username,
        };
        await _userService.SignUpAsync(user,UserRole.Admin, model.CompanyId, job.Id);

        return job;
    }

    public async Task UpdateAsync(int id, JobModel model)
    {
        var job = GetJob(id) ?? throw new AppException("Job not found");

        _mapper.Map(model, job);
        _context.Job.Update(job);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var job = GetJob(id) ?? throw new AppException("Job not found");
        _context.Job.Remove(job);
        await _context.SaveChangesAsync();
    }

    private Job GetJob(int id)
    {
        return _context.Job.Find(id);

    }
}