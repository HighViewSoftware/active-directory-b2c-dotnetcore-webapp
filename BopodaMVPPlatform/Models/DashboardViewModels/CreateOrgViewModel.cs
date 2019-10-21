using Aiursoft.Pylon.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace BopodaMVPPlatform.Models.DashboardViewModels
{
    public class CreateOrgViewModel : LayoutViewModel
    {
        [Obsolete(message: "This method is only for framework", error: true)]
        public CreateOrgViewModel() { }
        public CreateOrgViewModel(MVPUser user) : base(user, "Create organization") { }

        public void Recover(MVPUser user)
        {
            RootRecover(user, "Create organization");
        }

        [Display(Name = "Organization name")]
        [Required]
        [StringLength(maximumLength: 25, MinimumLength = 5)]
        [ValidFolderName]
        public string OrganizationName { get; set; }

        [Display(Name = "Organization site name")]
        [Required]
        [StringLength(maximumLength: 25, MinimumLength = 5)]
        public string OrganizationSiteName { get; set; }

        [Display(Name = "Organization description")]
        public string OrganizationDescription { get; set; }
    }
}
