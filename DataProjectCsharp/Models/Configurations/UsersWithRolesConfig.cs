using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Models.Configurations
{
    public class UsersWithRolesConfig: IEntityTypeConfiguration<IdentityUserRole<string>>
    {
        // seeds a dummy admin
        private const string adminUserId = "A11756P1-13H5-1887-1542-2Y7X3K5LJ9R1";
        private const string adminRoleId = "9f581206-6046-4cc0-92ac-86313c875f50";

        public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
        {
            IdentityUserRole<string> roleAssignment = new IdentityUserRole<string>
            {
                RoleId = adminRoleId,
                UserId = adminUserId
            };
            builder.HasData(roleAssignment);
        }
    }

}
