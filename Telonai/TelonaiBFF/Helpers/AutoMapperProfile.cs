namespace TelonaiWebApi.Helpers;

using AutoMapper;
using TelonaiWebApi.Services;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Models;
using System.Drawing;

public class AutoMapperProfile : Profile
{
    private readonly IStaticDataService _dataService;

    public AutoMapperProfile(IStaticDataService dataService)
    {
        _dataService = dataService;
    }
    public AutoMapperProfile()
    {
        CreateMap<PayStub, PayStubModel>()
            .ForMember(dest => dest.PersonId, opt => opt.MapFrom(src => src.Employment.PersonId))
            .ForMember(dest => dest.Employment, opt => opt.Ignore());
        CreateMap<PayStubModel, PayStub>()
            .ForMember(dest => dest.YtdGrossPay, opt => opt.Ignore())
            .ForMember(dest => dest.YtdNetPay, opt => opt.Ignore())
            .ForMember(dest => dest.YtdOverTimeHoursWorked, opt => opt.Ignore())
            .ForMember(dest => dest.YtdOverTimePay, opt => opt.Ignore())
            .ForMember(dest => dest.YtdRegularHoursWorked, opt => opt.Ignore())
            .ForMember(dest => dest.YtdRegularHoursWorked, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<OtherMoneyReceived, OtherMoneyReceivedModel>();

        CreateMap<OtherMoneyReceivedModel, OtherMoneyReceived>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());


        CreateMap<AdditionalOtherMoneyReceived, AdditionalOtherMoneyReceivedModel>()
             .ForMember(dest => dest.ExemptFromFutaTaxType, opt => opt.MapFrom(src =>
             (ExemptFromFutaTaxTypeModel)src.ExemptFromFutaTaxTypeId));

        CreateMap<AdditionalOtherMoneyReceivedModel, AdditionalOtherMoneyReceivedModel>()
             .ForMember(dest => dest.ExemptFromFutaTaxType, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());


        CreateMap<Document, DocumentModel>()
             .ForMember(dest => dest.DocumentType, opt => opt.MapFrom(src => (DocumentTypeModel)src.DocumentTypeId))
             .ForMember(dest => dest.PersonId, opt => opt.MapFrom(src => src.PersonId))
             .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName));
        CreateMap<DocumentModel, Document>()
            .ForMember(dest => dest.PersonId, opt => opt.MapFrom(src => src.PersonId))
            .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
            .ForMember(dest => dest.DocumentTypeId, opt => opt.MapFrom(src => (int)src.DocumentType))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DocumentType, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

        CreateMap<Payroll, PayrollModel>()
           .ForMember(dest => dest.ScheduledRunDate, opt => opt.MapFrom(src => src.ScheduledRunDate.ToDateTime(TimeOnly.MinValue)))
           .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ToDateTime(TimeOnly.MinValue)));

        CreateMap<PayrollModel, Payroll>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.StartDate)))
            .ForMember(dest => dest.ScheduledRunDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.ScheduledRunDate)))
            .ForMember(dest => dest.Company, opt => opt.Ignore()).ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());


        CreateMap<PayrollSchedule, PayrollScheduleModel>()
            .ForMember(dest => dest.Compnay, opt => opt.MapFrom(src => src.Company.Name))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ToDateTime(TimeOnly.MinValue)))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.Value.ToDateTime(TimeOnly.MinValue))); ;
        //.ForMember(dest => dest.PayrollScheduleType, opt => opt.MapFrom(src => (PayrollScheduleTypeModel)src.PayrollScheduleTypeId));

        CreateMap<PayrollScheduleModel, PayrollSchedule>()
             .ForMember(dest => dest.Id, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.Company, opt => opt.Ignore())
             .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.StartDate)))
             .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.EndDate.Value)))
             .ForMember(dest => dest.PayrollScheduleType, opt => opt.Ignore())
             .ForMember(dest => dest.PayrollScheduleTypeId, opt => opt.MapFrom(src => (PayrollScheduleTypeModel)Enum.Parse(typeof(PayrollScheduleTypeModel), src.PayrollScheduleType)));

        CreateMap<Employment, EmploymentModel>()
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.Job.CompanyId))
            .ForMember(dest => dest.SignUpStatusType, opt => opt.MapFrom(src => (SignUpStatusTypeModel)(src.SignUpStatusTypeId ?? 0)))
            .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Job.Company.Name));
        CreateMap<EmploymentModel, Employment>()
             .ForMember(dest => dest.Id, opt => opt.Ignore())
             .ForMember(dest => dest.SignUpStatusType, opt => opt.Ignore())
             .ForMember(dest => dest.SignUpStatusTypeId, opt => opt.MapFrom(src => (int)src.SignUpStatusType))
             .ForMember(dest => dest.Job, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.Person, opt => opt.Ignore());

        CreateMap<Person, PersonModel>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Zipcode.City.Name))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.Zipcode.City.State.Name))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Zipcode.City.State.Country.Name))
            .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.Zipcode.City.State.CountryId))
            .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Company.Name))
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
            .ForMember(dest => dest.Zipcode, opt => opt.MapFrom(src => src.Zipcode.Code))
            .ForMember(dest => dest.CityId, opt => opt.MapFrom(src => src.Zipcode.CityId))
             .ForMember(dest => dest.INineVerificationStatus, opt => opt.MapFrom(src => (INineVerificationStatusModel)src.INineVerificationStatusId))
            .ForMember(dest => dest.StateWithholdingDocumentStatus, opt => opt.MapFrom(src => (StateWithholdingDocumentStatusModel)src.StateWithholdingDocumentStatusId))
            .ForMember(dest => dest.INineVerificationStatus, opt => opt.MapFrom(src => (WFourWithholdingDocumentStatusModel)src.WfourWithholdingDocumentStatusId))
            .ForMember(dest => dest.CityId, opt => opt.MapFrom(src => src.Zipcode.CityId))
            .ForMember(dest => dest.StateId, opt => opt.MapFrom(src => src.Zipcode.City.StateId));

        CreateMap<PersonModel, Person>()
             .ForMember(dest => dest.Id, opt => opt.Ignore())
             .ForMember(dest => dest.Zipcode, opt => opt.Ignore())
             .ForMember(dest => dest.Company, opt => opt.Ignore())
             .ForMember(dest => dest.INineVerificationStatus, opt => opt.Ignore())
             .ForMember(dest => dest.WfourWithholdingDocumentStatus, opt => opt.Ignore())
             .ForMember(dest => dest.StateWithholdingDocumentStatus, opt => opt.Ignore())
             .ForMember(dest => dest.INineVerificationStatusId, opt => opt.MapFrom(src => (int)src.INineVerificationStatus))
             .ForMember(dest => dest.StateWithholdingDocumentStatusId, opt => opt.MapFrom(src => (int)src.StateWithholdingDocumentStatus))
             .ForMember(dest => dest.WfourWithholdingDocumentStatusId, opt => opt.MapFrom(src => (int)src.WFourWithholdingDocumentStatus))
             .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

        CreateMap<Job, JobModel>()
            .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Company.Name))
            .ForMember(dest => dest.ZipcodeId, opt => opt.MapFrom(src => src.ZipcodeId));

        CreateMap<JobModel, Job>()
             .ForMember(dest => dest.Id, opt => opt.Ignore())
             .ForMember(dest => dest.Zipcode, opt => opt.Ignore())
             .ForMember(dest => dest.Company, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

        CreateMap<JobRequestModel, JobModel>()
             .ForMember(dest => dest.Id, opt => opt.Ignore())
             .ForMember(dest => dest.Company, opt => opt.Ignore());

        CreateMap<Company, CompanyModel>()
           .ForMember(dest => dest.BusinessType, opt => opt.MapFrom(src => (BusinessTypeModel)src.BusinessTypeId))
           .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Zipcode.City.Name))
           .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.Zipcode.City.State.Name))
           .ForMember(dest => dest.CityId, opt => opt.MapFrom(src => src.Zipcode.CityId))
           .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.Zipcode.Code))
           .ForMember(dest => dest.StateId, opt => opt.MapFrom(src => src.Zipcode.City.StateId));


        CreateMap<CompanyModel, Company>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
             .ForMember(dest => dest.BusinessType, opt => opt.Ignore())
             .ForMember(dest => dest.BusinessTypeId, opt => opt.MapFrom(src => (int)src.BusinessType))
             .ForMember(dest => dest.ZipcodeId, opt => opt.MapFrom(src => (int)src.ZipcodeId))
             .ForMember(dest => dest.Zipcode, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

        CreateMap<CompanySpecificField, CompanySpecificFieldModel>();

        CreateMap<CompanySpecificFieldModel, CompanySpecificField>()
             .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<CompanySpecificFieldValue, CompanySpecificFieldValueModel>();

        CreateMap<CompanySpecificFieldValueModel, CompanySpecificFieldValue>()
             .ForMember(dest => dest.Company, opt => opt.Ignore())
             .ForMember(dest => dest.CompanySpecificField, opt => opt.Ignore())
             .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<CompanyModel, Company>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
             .ForMember(dest => dest.BusinessType, opt => opt.Ignore())
             .ForMember(dest => dest.BusinessTypeId, opt => opt.MapFrom(src => (int)src.BusinessType))
             .ForMember(dest => dest.ZipcodeId, opt => opt.MapFrom(src => (int)src.ZipcodeId))
             .ForMember(dest => dest.Zipcode, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

        CreateMap<TimecardUsa, TimecardUsaModel>()
           .ForMember(dest => dest.Job, opt => opt.MapFrom(src => src.Job.LocationName));
        CreateMap<TimecardUsaModel, TimecardUsa>()
             .ForMember(dest => dest.Id, opt => opt.Ignore())
             .ForMember(dest => dest.Person, opt => opt.Ignore())
             .ForMember(dest => dest.Job, opt => opt.Ignore())
            .ForMember(dest => dest.JobId, opt => opt.MapFrom(src => src.JobId))
            .ForMember(dest => dest.PersonId, opt => opt.MapFrom(src => src.PersonId))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

        CreateMap<TimecardUsaNote, TimecardUsaNoteModel>();
        CreateMap<TimecardUsaNoteModel, TimecardUsaNote>()
             .ForMember(dest => dest.Id, opt => opt.Ignore())
             .ForMember(dest => dest.TimecardUsa, opt => opt.Ignore())
            .ForMember(dest => dest.TimecardUsaId, opt => opt.MapFrom(src => src.TimecardUsaId))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

        CreateMap<WorkSchedule, WorkScheduleModel>()
           .ForMember(dest => dest.Job, opt => opt.MapFrom(src => src.Job.LocationName));

        CreateMap<WorkScheduleModel, WorkSchedule>()
             .ForMember(dest => dest.Id, opt => opt.Ignore())
             .ForMember(dest => dest.Person, opt => opt.Ignore())
             .ForMember(dest => dest.Job, opt => opt.Ignore())
            .ForMember(dest => dest.JobId, opt => opt.MapFrom(src => src.JobId))
            .ForMember(dest => dest.PersonId, opt => opt.MapFrom(src => src.PersonId))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

        CreateMap<WorkScheduleNote, WorkScheduleNoteModel>()
            .ForMember(dest => dest.WorkSchedule, opt => opt.Ignore()); ;

        CreateMap<WorkScheduleNoteModel, WorkScheduleNote>()
             .ForMember(dest => dest.Id, opt => opt.Ignore())
             .ForMember(dest => dest.WorkSchedule, opt => opt.Ignore())
            .ForMember(dest => dest.WorkScheduleId, opt => opt.MapFrom(src => src.WorkScheduleId))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

        CreateMap<Invitation, InvitationStatusModel>()
         .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
         .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.UpdatedBy == null ? "Invitation Sent" : "Completed"));

        CreateMap<Invitation, InvitationModel>()
            .ForMember(dest => dest.Employment, opt => opt.Ignore())
           .ForMember(dest => dest.ActivationCode, opt => opt.MapFrom(src => src.Id.ToString().Substring(src.Id.ToString().Length - 8)))
           .ForMember(dest => dest.Job, opt => opt.MapFrom(src => src.Job == null ? "" : src.Job.LocationName))
           .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Job == null || src.Job.Company == null ? src.CompanyName : src.Job.Company.Name))
           .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country.Name))
           .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.CountryId))
           .ForMember(dest => dest.PhoneCountryCode, opt => opt.MapFrom(src => src.Country.PhoneCountryCode));

        CreateMap<InvitationModel, Invitation>()
             .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company))
             .ForMember(dest => dest.Id, opt => opt.Ignore())
             .ForMember(dest => dest.Job, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

        CreateMap<Country, CountryModel>();

        CreateMap<City, CityModel>()
           .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State));

        CreateMap<State, StateModel>()
           .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country == null ? "" : src.Country.Name))
           .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.CountryId));

        CreateMap<Zipcode, ZipcodeModel>()
           .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City));

        CreateMap<FormNineFortyOne, FormNineFortyOneModel>()
                        .ForMember(dest => dest.DepositScheduleType, opt => opt.MapFrom(src => (BusinessTypeModel)src.DepositScheduleTypeId))
                        .ForMember(dest => dest.QuarterType, opt => opt.MapFrom(src => (QuarterTypeModel)src.QuarterTypeId));

        CreateMap<FormNineFortyOneModel, FormNineFortyOne>()
           .ForMember(dest => dest.Id, opt => opt.Ignore())
           .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
           .ForMember(dest => dest.Company, opt => opt.Ignore());

        CreateMap<EmployeeWithholding, EmployeeWithholdingModel>()
            .ForMember(dest => dest.Document, opt => opt.MapFrom(src => src.Document));

        CreateMap<EmployeeWithholdingModel, EmployeeWithholding>()
             .ForMember(des => des.Id, opt => opt.Ignore())
             .ForMember(dest => dest.Field, opt => opt.Ignore())
             .ForMember(dest => dest.Employment, opt => opt.Ignore())
             .ForMember(dest => dest.Document, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

        CreateMap<FormNineForty, FormNineFortyModel>();

        CreateMap<FormNineFortyModel, FormNineForty>()
           .ForMember(dest => dest.Id, opt => opt.Ignore())
           .ForMember(dest => dest.Company, opt => opt.Ignore())
           .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

        CreateMap<FormNineFortyFour, FormNineFortyFourModel>();

        CreateMap<FormNineFortyFourModel, FormNineFortyFour>()
           .ForMember(dest => dest.Id, opt => opt.Ignore())
           .ForMember(dest => dest.Company, opt => opt.Ignore())
           .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

        CreateMap<FormNineFortyOne, FormNineFortyOneModel>()
            .ForMember(dest => dest.CheckedBoxSixteenType, opt => opt.MapFrom(src => (CheckedBoxSixteenTypeModel)src.CheckedBoxSixteenTypeId));

        CreateMap<CompanyContact, CompanyContactModel>()
            .ForMember(dest => dest.Person, opt => opt.MapFrom(src => src.Person))
            .ForMember(dest => dest.ContactType, opt => opt.MapFrom(src => (ContactTypeModel)src.ContacttypeId));

        CreateMap<CompanyContactModel, CompanyContact>()
            .ForMember(dest => dest.ContacttypeId, opt => opt.MapFrom(src => Convert.ToInt32(src.ContactType)))
            .ForMember(dest => dest.Company, opt => opt.Ignore())
            .ForMember(dest => dest.Person, opt => opt.Ignore())
            .ForMember(dest => dest.ContactType, opt => opt.Ignore())
            .ForMember(dest => dest.ContactType, opt => opt.Ignore());

        CreateMap<EmployerSubscriptionModel, EmployerSubscription>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.SubscriptionTypeId, opt => opt.MapFrom(src => Convert.ToInt32(src.SubscriptionType)))
            .ForMember(dest => dest.BankAccountTypeId, opt => opt.MapFrom(src => Convert.ToInt32(src.BankAccountType)));

        CreateMap<EmployerSubscription, EmployerSubscriptionModel>()
               .ForMember(dest => dest.SubscriptionType, opt => opt.MapFrom(src => (SubscriptionTypeModel)src.SubscriptionTypeId))
               .ForMember(dest => dest.BankAccountType, opt => opt.MapFrom(src => (BankAccountTypeModel)src.BankAccountTypeId));
        CreateMap<AgentField, AgentFieldModel>();

        CreateMap<AgentFieldModel, AgentField>()
             .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<AgentFieldValue, AgentFieldValueModel>();

        CreateMap<AgentFieldValueModel, AgentFieldValue>()
             .ForMember(dest => dest.Person, opt => opt.Ignore())
             .ForMember(dest => dest.AgentField, opt => opt.Ignore())
             .ForMember(dest => dest.Id, opt => opt.Ignore());

    }
}