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
        /*Manually create roles for the DataBase
         * a dummy user and an admin user.
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
                 Name = "Administrator",
                 NormalizedName = "ADMIN"
             });
        }
    }
}
