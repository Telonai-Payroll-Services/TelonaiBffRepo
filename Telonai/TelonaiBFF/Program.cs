
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json.Serialization;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Helpers.Cache;
using TelonaiWebApi.Services;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Models;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using System.Security.Claims;
using System.ComponentModel;
using Newtonsoft.Json;
using Amazon.S3;
using Amazon.SQS;

var builder = WebApplication.CreateBuilder(args);

// add services to DI container
{
    var services = builder.Services;
    var env = builder.Environment;

    var configuration = builder.Configuration;
    services.AddOptions();
    services.Configure<ApiOptions>(configuration);
    services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

    services.AddSingleton<IConfiguration>(configuration);

   // var options = configuration.GetAWSOptions();

    services.AddDefaultAWSOptions(configuration.GetAWSOptions());

    //IAmazonS3 client = options.CreateServiceClient<IAmazonS3>();
    //IAmazonS3 client2 = options.CreateServiceClient<IAmazonS3>();

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
    services.AddScoped<IIncomeTaxService<IncomeTaxRateModel, IncomeTaxRate>, IncomeTaxService>();
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
    services.AddScoped<IEmployeeWithholdingService<EmployeeWithholdingModel, EmployeeWithholding>, EmployeeWithholdingService>();
    services.AddScoped<IScopedAuthorization, ScopedAuthorization>();
    services.AddDefaultAWSOptions(configuration.GetAWSOptions());
    services.AddAWSService<IAmazonS3>();
    services.AddAWSService<IAmazonSQS>();

    services.AddLogging(config =>
    {
        config.AddAWSProvider(configuration.GetAWSLoggingConfigSection());
        config.SetMinimumLevel(LogLevel.Debug);
    });

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

    //services
    //    .AddAuthentication("Bearer")
    //    .AddJwtBearer(options =>
    //    {
    //        options.Authority = "https://cognito-idp.us-east-2.amazonaws.com/us-east-2_vCPO95uWK";
    //        options.TokenValidationParameters = new TokenValidationParameters
    //        {
    //            ValidateAudience = false,
    //        };
    //    });

   
    services.AddAuthorization(options =>
    {
        //options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

        options.AddPolicy("SystemAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "SystemAdmin"));
    });


    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
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
    app.UseSwaggerUI();
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
}

//app.Run();
app.Run("http://localhost:5000");