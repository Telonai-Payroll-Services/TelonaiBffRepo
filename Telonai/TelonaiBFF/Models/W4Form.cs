namespace TelonaiWebApi.Models
{
    public class W4Form
    {
        public FilingStatus FilingStatus { get; set; }
        public bool householdIncomeBelow200k { get; set; }
        public bool householdIncomeBelow400k { get; set; }
        public bool MultipleJobs { get; set; }
        public bool SpouseWorks { get; set; }
        public int NumberOfChildrenUnder17 { get; set; }
        public int OtherDependents { get; set; }
        public decimal OtherIncome { get; set; } 
        public decimal Deductions { get; set; } 
        public decimal ExtraWithholding { get; set; } 
    
    }


    public class FilingStatus
    {
        public bool SingleOrMarriedFilingSeparately { get; set; }
        public bool MarriedFilingJointly { get; set; }
        public bool HeadOfHousehold { get; set; }
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
