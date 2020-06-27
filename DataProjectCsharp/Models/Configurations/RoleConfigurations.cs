using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Models.Configurations
{
    public class RoleConfigurations:IEntityTypeConfiguration<IdentityRole>
    {
        private const string adminRoleId = "9f581206-6046-4cc0-92ac-86313c875f50";
        /*Manually create roles for the DataBase
         * a visitor role and an admin role.
         */
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.HasData(
             new IdentityRole
             {
                 Name = "Visitor",
                 NormalizedName = "Visitor"
             },
             new IdentityRole
             {
                 Id = adminRoleId,
                 Name = "Administrator",
                 NormalizedName = "ADMIN"
             });
        }
    }
}
