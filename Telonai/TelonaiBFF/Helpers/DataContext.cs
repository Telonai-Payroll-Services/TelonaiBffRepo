namespace TelonaiWebApi.Helpers;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TelonaiWebApi.Entities;
using Newtonsoft.Json;
using Amazon.SecretsManager.Extensions.Caching;
using TelonaiWebApi.Helpers.Interface;
public class DataContext : DbContext, IDataContext
{
    private readonly IHttpContextAccessor _context;
    private readonly SecretsManagerCache _cache;
    public DataContext() { }
    public DataContext(IHttpContextAccessor context)
    {
        _cache = new SecretsManagerCache();
        _context = context;
    }

    private async Task<Dictionary<string, string>> GetSecret(string MySecretName)
    {
        string secret = await _cache.GetSecretString(MySecretName);
        return JsonConvert.DeserializeObject<Dictionary<string, string>>(secret);

    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var secret = GetSecret("TelonaiDBConnectionString").Result;        
        var connectionString = $"Host={secret["host"]};Database={secret["dbname"]};Username={secret["username"]};Password={secret["password"]}";

        options.UseNpgsql(connectionString).ReplaceService<ISqlGenerationHelper, NpgsqlSqlGenerationLowercasingHelper>();

    }

    public override int SaveChanges()
    {
        this.ChangeTracker.DetectChanges();
        var added = this.ChangeTracker.Entries()
                    .Where(t => t.State == EntityState.Added)
                    .Select(t => t.Entity)
                    .ToArray();

        foreach (var entity in added)
        {
            if (entity is IBaseTracker)
            {
                var track = entity as IBaseTracker;
                track.CreatedDate = DateTime.UtcNow;
                track.CreatedBy = _context.HttpContext.User?.Identity?.Name?? "System";
            }
        }

        var modified = this.ChangeTracker.Entries()
                    .Where(t => t.State == EntityState.Modified)
                    .Select(t => t.Entity)
                    .ToArray();

        foreach (var entity in modified)
        {
            if (entity is IBaseTracker)
            {
                var track = entity as IBaseTracker;
                track.UpdatedDate = DateTime.UtcNow;

                DateTime.SpecifyKind((DateTime)track.UpdatedDate!, DateTimeKind.Utc);
                track.UpdatedBy = _context.HttpContext.User?.Identity?.Name; ;
            }
        }
        return base.SaveChanges();
    }
    public virtual async Task<int> SaveChangesAsync()
    {
        this.ChangeTracker.DetectChanges();
        var added = this.ChangeTracker.Entries()
                    .Where(t => t.State == EntityState.Added)
                    .Select(t => t.Entity)
                    .ToArray();

        foreach (var entity in added)
        {
            if (entity is IBaseTracker)
            {
                var track = entity as IBaseTracker;
                track.CreatedDate = DateTime.UtcNow;
                track.CreatedBy = _context.HttpContext.User?.Identity?.Name??"System";
            }
        }

        var modified = this.ChangeTracker.Entries()
                    .Where(t => t.State == EntityState.Modified)
                    .Select(t => t.Entity)
                    .ToArray();

        foreach (var entity in modified)
        {
            if (entity is IBaseTracker)
            {
                var track = entity as IBaseTracker;
                track.UpdatedDate = DateTime.UtcNow;
                track.UpdatedBy = _context.HttpContext.User?.Identity?.Name; ;
            }
        }
        return await base.SaveChangesAsync();
    }

    public virtual DbSet<Employment> Employment { get; set; }
    public virtual DbSet<Person> Person { get; set; }
    public virtual DbSet<Company> Company { get; set; }
    public DbSet<City> City { get; set; }
    public DbSet<State> State { get; set; }
    public DbSet<Country> Country { get; set; }
    public DbSet<BusinessType> BusinessType { get; set; }
    public  DbSet<CompanyContact> CompanyContact { get; set; }
    public virtual DbSet<ContactType> ContactType { get; set; }
    public virtual DbSet<Job> Job { get; set; }
    public DbSet<TimecardUsa> TimecardUsa { get; set; }
    public DbSet<TimecardUsaNote> TimecardUsaNote { get; set; }
    public DbSet<Invitation> Invitation { get; set; }
    public DbSet<Zipcode> Zipcode { get; set; }
    public DbSet<RoleType> RoleType { get; set; }
    public DbSet<PayrollSchedule> PayrollSchedule { get; set; }
    public DbSet<PayrollScheduleType> PayrollScheduleType { get; set; }
    public DbSet<Payroll> Payroll { get; set; }
    public DbSet<Holiday> Holiday { get; set; }
    public DbSet<WorkSchedule> WorkSchedule { get; set; }
    public DbSet<WorkScheduleNote> WorkScheduleNote { get; set; }
    public DbSet<IncomeTaxRate> IncomeTaxRate { get; set; }
    public DbSet<IncomeTaxType> IncomeTaxType { get; set; }
    public DbSet<EmployeeWithholding> EmployeeWithholding { get; set; }
    public DbSet<EmployeeWithholdingField> EmployeeWithholdingField { get; set; }
    public DbSet<PayStub> PayStub { get; set; }
    public DbSet<OtherMoneyReceived> OtherMoneyReceived { get; set; }
    public DbSet<PayRateBasis> PayRateBasis { get; set; }
    public DbSet<IncomeTax> IncomeTax { get; set; }
    public virtual DbSet<Document> Document { get; set; }
    public DbSet<StateStandardDeduction> StateStandardDeduction { get; set; }

}