﻿using Aiursoft.Pylon.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace BopodaMVPPlatform.Models.DashboardViewModels
{
    public class CreateSiteViewModel : LayoutViewModel
    {
        [Obsolete(error: true, message: "This method is only for framework!")]
        public CreateSiteViewModel()
        {
        }

        public CreateSiteViewModel(MVPUser user) : base(user, "Quick upload")
        {
        }

        public void Recover(MVPUser user)
        {
            RootRecover(user, "Quick upload");
        }

        [Display(Name = "New site name")]
        [Required]
        [MaxLength(50)]
        [MinLength(5)]
        [ValidDomainName]
        public string SiteName { get; set; }

        [Required]
        [Display(Name = "Open to upload")]
        public bool OpenToUpload { get; set; } = true;

        [Required]
        [Display(Name = "Open to download")]
        public bool OpenToDownload { get; set; } = true;
    }
}
