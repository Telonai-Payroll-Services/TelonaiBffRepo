using Amazon.Runtime.Internal.Util;
using System.Runtime.Intrinsics.X86;
using TelonaiWebApi.Entities;

namespace TelonaiWebApi.Models
{
    public static class PdfFields
    {
        public const string Step1a_FirstName_MiddleInitial = "a First name and middle initial";
        public const string Step1a_LastName = "Last name";
        public const string Step1a_Address = "Address";
        public const string Step1a_City_Or_Town_State_ZIPCode = "City or town state and ZIP code";
        public const string Step1b_SocialSecurityNumber = "b Social security number";
        public const string Step1c_FilingStatus_SingleOrMarriedFilingSeparately = "Single or Married filing separately";
        public const string Step1c_FilingStatus_MarriedFilingJointly = "Married filing jointly or Qualifying surviving spouse";
        public const string Step1c_FilingStatus_HeadOfHousehold = "Head of household Check only if youre unmarried and pay more than half the costs of keeping up a home for yourself and a qualifying individual";
        public const string Step2_MultipleJobsOrSpouseWorks = "undefined";
        public const string Step3_Dependents_NumberOfChildrenUnder17 = "Multiply the number of qualifying children under age 17 by 2000";
        public const string Step3_Dependents_OtherDependents = "undefined_2";
        public const string Step3_TotalClaimedAmount = "fill_11";
        public const string Step4a_OtherIncome = "fill_12";
        public const string Step4b_Deductions = "fill_13";
        public const string Step4c_ExtraWithholding = "fill_14";
        public const string Signature = "Signature1_es_:signer:signature";
        public const string Date = "Date";
        public const string EmployerNameAndAddress = "Employers name and address";
        public const string EmployerFirstDateOfEmployement = "First date of employment";
        public const string EmployerIdentificationNumber = "Employer identification number EIN";
    }
    public static class NC4PdfFields
    {

        public const string NumberOfAllowance = "NumberOfAllowance";
        public const string AdditionalAmt = "AdditionalAmt";
        public const string SocialSecurity1stPart = "Social Security 1st-Part";
        public const string LastName = "LastName";
        public const string Step1c_FilingStatus_SingleOrMarriedFilingSeparately = "Filing Status";
        public const string FirstName = "First Name USE CAPITAL  LETTERS FOR YOUR NAME AND ADDRESS";
        public const string Step1c_FilingStatus_MarriedFilingJointly = "undefined_4";
        public const string Step1c_FilingStatus_HeadOfHousehold = "undefined_5";
        public const string MI = "MI";
        public const string Address = "Address";
        public const string ZipCode = "ZipCode";
        public const string Country = "Country  If not US";
        public const string City = "City";
        public const string State = "State";
        public const string Date = "Date";
        public const string CountyFirstFiveLetters = "County-FirstFiveLetters";
        public const string Signature = "Signature";
        public const string FilingStatus_FilingStatus2 = "FilingStatus";
        public const string SocialSecurity2ndPart = "Social Security 2ndPart";
        public const string SocialSecurity3rdPart = "Social Security 3rdPart";
    }
    public static class CursiveFont
    {
        public static Guid Id = Guid.Parse("6a909a84-e6ae-44e4-8439-85b1e12f66eb");
    }

}
