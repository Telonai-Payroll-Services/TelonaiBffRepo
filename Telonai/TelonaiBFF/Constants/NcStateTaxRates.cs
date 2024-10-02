namespace TelonaiWebApi.Constants
{
    public static class NcStateTaxRates
    {
        //SUTA = State Unemployment Tax paid by employer only

        //For Employer********************
        public static double EmployerSutaTaxRateForNewEmployer = 0.01;
        public static double EmployerSutaTaxRateRangeMinimum = 0.01;
        public static double EmployerSutaTaxRateRangeMaximum = 0.01;
        public static double EmployerSutaTaxWageBaseLimit = 29000;
        public static double EmployerSutaTaxRateForNewEmployere = 0.01;

        //End For Eployer******************


        //************* For Employee ********************

        public static Dictionary<double, Dictionary<string, int>> EmployeeStateTaxRates = new() 
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
        public static Dictionary<int, double> EmployeeStateTaxRate = new() { { 0, 0.045 } };

        //********8 End of For Employee******************

    }
}
