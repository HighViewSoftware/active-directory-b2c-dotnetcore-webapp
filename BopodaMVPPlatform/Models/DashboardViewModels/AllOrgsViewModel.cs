using System;
using System.Collections.Generic;

namespace BopodaMVPPlatform.Models.DashboardViewModels
{
    public class AllOrgsViewModel : LayoutViewModel
    {
        [Obsolete(error: true, message: "This method is only for framework!")]
        public AllOrgsViewModel()
        {

        }

        public AllOrgsViewModel(MVPUser user) : base(user, "Quick upload")
        {
        }

        public List<Organization> MyOrgs { get; set; }
    }
}
