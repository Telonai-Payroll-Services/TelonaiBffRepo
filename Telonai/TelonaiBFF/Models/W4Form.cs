namespace TelonaiWebApi.Models
{
    public class W4Form
    {
        public Employee Employee { get; set; }
        public FilingStatus FilingStatus { get; set; }
        public bool MultipleJobsOrSpouseWorks { get; set; }
        public Dependents Dependents { get; set; }
        public decimal OtherIncome { get; set; } 
        public decimal Deductions { get; set; } 
        public decimal ExtraWithholding { get; set; } 
        public MultipleJobsWorksheet MultipleJobsWorksheet { get; set; }
        public DeductionsWorksheet DeductionsWorksheet { get; set; }
    
    }

    public class Employee
    {
        public string FirstName { get; set; }
        public string MiddleInitial { get; set; }
        public string LastName { get; set; }
        public string SocialSecurityNumber { get; set; }
        public string Address { get; set; }
        public string CityOrTown { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }

    public class FilingStatus
    {
        public bool SingleOrMarriedFilingSeparately { get; set; }
        public bool MarriedFilingJointly { get; set; }
        public bool HeadOfHousehold { get; set; }
    }

    public class Dependents
    {
        public int NumberOfChildrenUnder17 { get; set; }
        public int OtherDependents { get; set; }
        public decimal TotalClaimedAmount { get; set; }
    }

    public class MultipleJobsWorksheet
    {
        public decimal Step2b_1 { get; set; } 
        public decimal Step2b_2a { get; set; } 
        public decimal Step2b_2b { get; set; } 
        public decimal Step2b_2c { get; set; } 
        public decimal Step2b_3 { get; set; } 
        public decimal Step2b_4 { get; set; } 
       
    }

    public class DeductionsWorksheet
    {
        public decimal Step4b_1 { get; set; } 
        public decimal Step4b_2 { get; set; } 
        public decimal Step4b_3 { get; set; } 
        public decimal Step4b_4 { get; set; }
        public decimal Step4b_5 { get; set; }
    }

}
