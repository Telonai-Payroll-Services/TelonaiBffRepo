namespace TelonaiWebApi.Constants
{
    public static class FederalTaxRates
    {
        //FICA =  socialsecurity + medicare
        //FUCA = Federal Unemployment Tax paid by employer only

        //For Employer********************
        public static Dictionary<int, double> EmployerFUTA = new() { { 0, 0.06 }, { 7000, 0 } };

        public static Dictionary<int, double> EmployerSocialSecurityRate = new()
        {
            { 0, 0.062 }, { 168600, 0 }
        };
        public static Dictionary<int, double> EmployerSocialSecurityRateForHouseholdWorker = new()
        {
            { 2700, 0.062 }, { 168600, 0 }
        };
        public static Dictionary<int, double> EmployerSocialSecurityRateForElectionWorker = new()
        {
            { 2300, 0.062 }, { 168600, 0 }
        };

        public static Dictionary<int, double> EmployerMedicareRate = new() { { 0, 0.0145 } };
        public static Dictionary<int, double> EmployerAdditionalMedicareRate = new() { { 200000, 0.009 } };
        public static Dictionary<int, double> EmployerMedicareRateForHouseholdWorker = new() { { 2700, 0.0145 } };
        public static Dictionary<int, double> EmployerMedicareRateForElectionWorker = new() { { 2300, 0.0145 } };
        //End For Eployer******************


        //************* For Employee ********************

        public static Dictionary<double, Dictionary<string, int>> EmployeeFederalTaxRates = new() 
        {
            //Dictionary<rate%, Dictionary<FilingStatus, MaximumTaxableIncome>> 
            {
                0.1, new Dictionary<string, int>{ 
                    { "Single", 0 },{ "MarriedFilingSeparately", 0 },{ "HeadOfHousehold", 0 },{ "MarriedFilingJointly", 0 },
                } 
            },
            {
                0.12, new Dictionary<string, int>{
                    { "Single", 11600 },{ "MarriedFilingSeparately", 11600 },{ "HeadOfHousehold", 16550 },{ "MarriedFilingJointly", 23200 },
                }
            },
            {
                0.22, new Dictionary<string, int>{
                    { "Single", 47150 },{ "MarriedFilingSeparately", 47150 },{ "HeadOfHousehold", 63100 },{ "MarriedFilingJointly", 94300 },
                }
            },
            {
                0.24, new Dictionary<string, int>{
                    { "Single", 10525 },{ "MarriedFilingSeparately", 10525 },{ "HeadOfHousehold", 100500 },{ "MarriedFilingJointly", 201050 },
                }
            },
            {
                0.32, new Dictionary<string, int>{
                    { "Single", 191950 },{ "MarriedFilingSeparately", 191950 },{ "HeadOfHousehold", 191950 },{ "MarriedFilingJointly", 308900 },
                }
            },
            {
                0.35, new Dictionary<string, int>{
                    { "Single", 243725 },{ "MarriedFilingSeparately", 243725 },{ "HeadOfHousehold", 243700 },{ "MarriedFilingJointly", 487450 },
                }
            },
            {
                0.37, new Dictionary<string, int>{
                    { "Single", 609350 },{ "MarriedFilingSeparately", 609350 },{ "HeadOfHousehold", 609350 },{ "MarriedFilingJointly", 731200 },
                }
            },
        };
        public static Dictionary<int, double> EmployeeSocialSecurityRate = new()
        {
            { 0, 0.062 }, { 168600, 0 }
        };
        public static Dictionary<int, double> EmployeeSocialSecurityRateForHouseholdWorker = new()
        {
            { 2700, 0.062 }, { 168600, 0 }
        };
        public static Dictionary<int, double> EmployeeSocialSecurityRateForElectionWorker = new()
        {
            { 2300, 0.062 }, { 168600, 0 }
        };

        public static Dictionary<int, double> EmployeeMedicareRate = new() { { 0, 0.0145 } };
        public static Dictionary<int, double> EmployeeAdditionalMedicareRate = new() { { 200000, 0.009 } };
        public static Dictionary<int, double> EmployeeMedicareRateForHouseholdWorker = new() { { 2700, 0.0145 } };
        public static Dictionary<int, double> EmployeeMedicareRateForElectionWorker = new() { { 2300, 0.0145 } };
        //For Eployee******************

    }
}
