namespace TelonaiWebApi.Models
{
    public class PayrollSummary
    {
        public double WagesAndTIps{ get; set; }
        public double FederalTaxWithHeld { get; set; }
        public double EmployeeSocialSecurity { get; set; }
        public double EmployeeMediCare { get; set; }
        public double EmployerSocialSecurity { get; set; }
        public double EmployerMediCare { get; set; }
        public double StateTaxWithHeld { get; set; }
    }
}
