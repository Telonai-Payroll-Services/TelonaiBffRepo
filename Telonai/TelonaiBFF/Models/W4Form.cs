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
    public class W4PdfResult
    {
        public byte[] FileBytes { get; set; }
        public Guid DocumentId { get; set; }
    }

    public class NC4Form
    {
        public FilingStatus FilingStatus { get; set; }
        public int NumberOfAllowance { get; set; }
        public decimal AdditionalAmt { get; set; }

    }
    }
