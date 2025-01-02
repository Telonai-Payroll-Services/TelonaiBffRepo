namespace TelonaiWebApi.Helpers;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TelonaiWebApi.Entities;
using Newtonsoft.Json;
using Amazon.SecretsManager.Extensions.Caching;
using TelonaiWebApi.Models;

public class DataContext : DbContext
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
        options.EnableSensitiveDataLogging();
        options.LogTo(Console.WriteLine, LogLevel.Information);
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
        try
        {
            return await base.SaveChangesAsync();
        }
        catch (Exception ex)
        {
          
            throw new InvalidOperationException("An error occurred while saving changes", ex);
        }
    }

    public virtual DbSet<Employment> Employment { get; set; }
    public virtual DbSet<Person> Person { get; set; }
    public virtual DbSet<Company> Company { get; set; }
    public DbSet<City> City { get; set; }
    public DbSet<State> State { get; set; }
    public DbSet<Country> Country { get; set; }
    public virtual DbSet<County> County { get; set; }
    public DbSet<BusinessType> BusinessType { get; set; }
    public  DbSet<CompanyContact> CompanyContact { get; set; }
    public virtual DbSet<ContactType> ContactType { get; set; }
    public virtual DbSet<Job> Job { get; set; }
    public virtual DbSet<TimecardUsa> TimecardUsa { get; set; }
    public DbSet<TimecardUsaNote> TimecardUsaNote { get; set; }
    public virtual DbSet<Invitation> Invitation { get; set; }
    public virtual DbSet<Zipcode> Zipcode { get; set; }
    public DbSet<RoleType> RoleType { get; set; }
    public virtual DbSet<PayrollSchedule> PayrollSchedule { get; set; }
    public DbSet<PayrollScheduleType> PayrollScheduleType { get; set; }
    public virtual DbSet<Payroll> Payroll { get; set; }
    public DbSet<Holiday> Holiday { get; set; }
    public DbSet<WorkSchedule> WorkSchedule { get; set; }
    public DbSet<WorkScheduleNote> WorkScheduleNote { get; set; }
    public DbSet<IncomeTaxRate> IncomeTaxRate { get; set; }
    public DbSet<IncomeTaxType> IncomeTaxType { get; set; }
    public DbSet<QuarterType> QuarterType { get; set; }
    public DbSet<INineVerificationStatus> INineVerificationStatus { get; set; }
    public DbSet<WfourWithholdingDocumentStatus> WfourWithholdingDocumentStatus { get; set; }
    public DbSet<StateWithholdingDocumentStatus> StateWithholdingDocumentStatus { get; set; }
    public DbSet<DepositScheduleType> DepositScheduleType { get; set; }
    public DbSet<EmployeeWithholding> EmployeeWithholding { get; set; }
    public DbSet<EmployeeWithholdingField> EmployeeWithholdingField { get; set; }
    public virtual DbSet<PayStub> PayStub { get; set; }
    public virtual DbSet<OtherMoneyReceived> OtherMoneyReceived { get; set; }
    public DbSet<PayRateBasis> PayRateBasis { get; set; }
    public DbSet<IncomeTax> IncomeTax { get; set; }
    public virtual DbSet<Document> Document { get; set; }
    public DbSet<StateStandardDeduction> StateStandardDeduction { get; set; }
    public DbSet<FormNineFortyOne> FormNineFortyOne { get; set; }
    public DbSet<DepositSchedule> DepositSchedule { get; set; }
    public DbSet<CompanySpecificField> CompanySpecificField { get; set; }
    public DbSet<CompanySpecificFieldValue> CompanySpecificFieldValue { get; set; }
    public DbSet<PayStubSpecificField> PayStubSpecificFiel { get; set; }
    public DbSet<PayStubSpecificFieldValue> PayStubSpecificFieldValue { get; set; }
    public DbSet<CountrySpecificField> CountrySpecificField { get; set; }
    public DbSet<CountrySpecificFieldValue> CountrySpecificFieldValue { get; set; }
    public DbSet<FormNineForty> FormNineForty { get; set; }
    public DbSet<StateSpecificField> StateSpecificField { get; set; }
    public DbSet<StateSpecificFieldValue> StateSpecificFieldValue { get; set; }
    public DbSet<FormNineFortyFour> FormNineFortyFour { get; set; }
    public DbSet<AdditionalOtherMoneyReceived> AdditionalOtherMoneyReceived { get; set; }
    public DbSet<DocumentType> DocumentType { get; set; }
    public DbSet<EmployerSubscription> EmployerSubscription { get; set; }
    public DbSet<AgentField> AgentField { get; set; }
    public DbSet<AgentFieldValue> AgentFieldValue { get; set; }
    public DbSet<MobileAppVersion> MobileAppVersion { get; set; }
    public DbSet<DayOffRequest> DayOffRequest { get; set; }
    public DbSet<DayOffType> DayoffType { get; set; }
    public DbSet<DayOffPayType> DayOffPayType { get; set; }
}