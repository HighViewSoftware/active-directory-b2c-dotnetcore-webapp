using Microsoft.AspNetCore.Mvc;

namespace BopodaMVPPlatform.Models.DashboardViewModels
{
    public class OrgHomeAddressModel
    {
        [FromRoute]
        public string OrgName { get; set; }
    }
}
