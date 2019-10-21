using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BopodaMVPPlatform.Models
{
    public class MVPUser
    {
        public string Id { get; set; }
        public string SiteName { get; set; }

        [InverseProperty(nameof(UserRelationship.User))]
        public IEnumerable<UserRelationship> Organizations { get; set; }
    }

    public class Organization
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string SiteName { get; set; }

        [InverseProperty(nameof(UserRelationship.Organization))]
        public IEnumerable<UserRelationship> Users { get; set; }
    }

    public class UserRelationship
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public MVPUser User { get; set; }

        public int OrganizationId { get; set; }
        [ForeignKey(nameof(OrganizationId))]
        public Organization Organization { get; set; }
    }
}
