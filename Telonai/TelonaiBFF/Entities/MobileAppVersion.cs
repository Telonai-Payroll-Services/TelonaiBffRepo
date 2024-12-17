using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TelonaiWebApi.Entities
{
    public class MobileAppVersion
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Version Sequence")]
        public int VersionSequence { get; set; }

        [Required]
        [Display(Name = "App Version")]
        public string AppVersion { get; set; }
        //[Required]
        [Display(Name = "Build Number")]
        public int BuildNumber { get; set; }

        [Display(Name = "Force Upgrade")]
        public bool ForceUpgrade { get; set; }

        [Display(Name = "Recommend For Update")]
        public bool RecommendForUpdate { get; set; }

        public bool Status { get; set; }
        
        [Required]
        public int Platform { get; set; }

    }

}
