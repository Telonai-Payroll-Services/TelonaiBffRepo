using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TelonaiWebApi.Models
{
    public class MobileAppVersionModel
    {
        public int Id { get; set; }
        public int VersionSequence { get; set; }
        public string AppVersion { get; set; }
        public int BuildNumber { get; set; }
        public bool ForceUpgrade { get; set; }
        public bool RecommendForUpdate { get; set; }
        public bool Status { get; set; }
        public PlatForm Platform { get; set; }
        public string AppPath { get; set; }
    }

    public enum PlatForm
    {
        Android = 1,
        iOS = 2,
    }
}
