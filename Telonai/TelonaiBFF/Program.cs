
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json.Serialization;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Helpers.Cache;
using TelonaiWebApi.Services;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Models;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using System.Security.Claims;
using Amazon.S3;
using Amazon.SQS;
using TelonaiWebApi.Models.FileScan;
using TelonaiWebApi.Helpers.FileScan;
using TelonaiWebApi.Helpers.Interface;
using TelonaiWebApi.Helpers.Configuration;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Amazon.AspNetCore.Identity.Cognito;

var builder = WebApplication.CreateBuilder(args);

// add services to DI container
{
    var services = builder.Services;
    var env = builder.Environment;

    var configuration = builder.Configuration;
    services.AddOptions();
    services.Configure<ApiOptions>(configuration);


    IAmazonS3 _s3Client = new AmazonS3Client(Amazon.RegionEndpoint.USEast2);
    var s3ObjectStream = _s3Client.GetObjectAsync("telonai-bff-appsettings", $"appsettings-{env.EnvironmentName}.json").Result.ResponseStream;
    
    builder.Host.ConfigureAppConfiguration((_, configurationBuilder) =>
    {
        configurationBuilder.AddAmazonSecretsManager("us-east-2", "FileScanAuthSettings");
        configurationBuilder.AddAmazonSecretsManager("us-east-2", "AwsUserPoolSettings");
        configurationBuilder.AddJsonStream(s3ObjectStream);
    });

    services.AddSingleton<IConfiguration>(configuration);
    services.AddDbContext<DataContext>();
    services.AddCors();
    services.AddMemoryCache();
    services.AddHttpContextAccessor();
    services.AddControllers().AddJsonOptions(x =>
    {
        // serialize enums as strings in api responses (e.g. Role)
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // ignore omitted parameters on models to enable optional params (e.g. person update)
        x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        x.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    });

    services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    // configure DI for application services
    services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

    services.AddScoped<ITelonaiCache, TelonaiCache>();
    services.AddSingleton<IMailSender, MailSender>();

    services.AddScoped<IBusinessTypeService, BusinessTypeService>();
    services.AddScoped<ICityService, CityService>();
    services.AddScoped<ICompanyService<CompanyModel, Company>, CompanyService>();
    services.AddScoped<IContactTypeService, ContactTypeService>();
    services.AddScoped<ICountryService, CountryService>();
    services.AddScoped<IEmploymentService<EmploymentModel, Employment>, EmploymentService>();
    services.AddScoped<IHolidaysService, HolidaysService>();
    services.AddScoped<IIncomeTaxRateService<IncomeTaxRateModel, IncomeTaxRate>, IncomeTaxRateService>();
    services.AddScoped<IInvitationService<InvitationModel, Invitation>, InvitationService>();
    services.AddScoped<IJobService<JobModel, Job>, JobService>();
    services.AddScoped<IOtherMoneyReceivedService, OtherMoneyReceivedService>();
    services.AddScoped<IPayrollScheduleService, PayrollScheduleService>();
    services.AddScoped<IPayrollService, PayrollService>();
    services.AddScoped<IPayStubService, PayStubService>();
    services.AddScoped<IPersonService<PersonModel, Person>, PersonService>();
    services.AddScoped<IRoleTypeService, RoleTypeService>();
    services.AddScoped<IStateService, StateService>();
    services.AddScoped<IStateStandardDeductionService, StateStandardDeductionService>();
    services.AddScoped<IStaticDataService, StaticDataService>();
    services.AddScoped<ITimecardUsaNoteService, TimecardUsaNoteService>();
    services.AddScoped<ITimecardUsaService, TimecardUsaService>();
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<IWorkScheduleService, WorkScheduleService>();
    services.AddScoped<IZipcodeService, ZipcodeService>();
    services.AddScoped<IDocumentService, DocumentService>();
    services.AddScoped<IDocumentManager, DocumentManager>();
    services.AddScoped<IFileScanRequest, FileScanRequest>();
    services.AddScoped<IEmployeeWithholdingService<EmployeeWithholdingModel, EmployeeWithholding>, EmployeeWithholdingService>();
    services.AddScoped<IScopedAuthorization, ScopedAuthorization>();
    services.AddScoped<IIRSService, IRSService>();
    services.AddScoped<IFormNineFortyOneService, FormNineFortyOneService>();
    services.AddScoped<IFormNineFortyFourService, FormNineFortyFourService>();
    services.AddScoped<IFormNineFortyService, FormNineFortyService>();
    services.AddScoped<ICompanyContactService, CompanyContactService>();
    services.AddScoped<IEmployerSubscriptionService, EmployerSubscriptionService>();
    services.AddScoped<IAgentService, AgentService>();
    services.AddScoped<IMobileAppVersionService, MobileAppVersionService>();
    services.AddScoped<IDayOffRequestService<DayOffRequestModel, DayOffRequest>, DayOffRequestService>();
    services.AddScoped<IDayOffTypeService, DayOffTypeService>();
    services.AddScoped<IFAQService, FAQService>();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    services.AddDefaultAWSOptions(configuration.GetAWSOptions());
    services.AddAWSService<IAmazonS3>();
    services.AddAWSService<IAmazonSQS>();
    services.AddLogging(config =>
    {
        config.AddAWSProvider(configuration.GetAWSLoggingConfigSection());
        config.SetMinimumLevel(LogLevel.Debug);
    });
    //services.AddTransient<CognitoSignInManager<CognitoUser>>();
    //services.AddTransient<CognitoUserManager<CognitoUser>>();

    var fileScanSettings = builder.Configuration.GetSection("FileScan");
    builder.Services.Configure<FileScanSettings>(fileScanSettings);

    builder.Services.Configure<FileScanAuthSettings>(builder.Configuration);
    builder.Services.Configure<AwsUserPoolSettings>(builder.Configuration);

    // Adds Amazon Cognito as Identity Provider
    services.AddCognitoIdentity();

    services.ConfigureApplicationCookie(options =>
    {
        //options.Cookie.Name = "Cookie";
        // options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
        options.SlidingExpiration = true;
        options.LoginPath = "/Users/Login";
        options.AccessDeniedPath = "/Users/AccessDenied";
    });

    services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.SlidingExpiration = true;
            options.LoginPath = "/Users/Login";
            options.AccessDeniedPath = "/Users/AccessDenied";
        });

    services.AddAuthorization(options =>
    {
        //options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

        options.AddPolicy("SystemAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "SystemAdmin"));
    });


}

var app = builder.Build();

// configure HTTP request pipeline
{
    // global cors policy
    app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

    app.UseAuthentication();
    app.UseAuthorization();

    // global error handler
    app.UseMiddleware<ErrorHandlerMiddleware>();

    app.MapControllers();
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
}


app.Run("http://localhost:5000");
//app.Run();
