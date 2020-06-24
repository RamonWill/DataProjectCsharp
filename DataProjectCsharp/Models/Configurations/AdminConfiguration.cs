using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Models.Configurations
{
    public class AdminConfiguration: IEntityTypeConfiguration<User> 
    {
        // sets up the dummy admin user
        private const string adminId = "A11756P1-13H5-1887-1542-2Y7X3K5LJ9R1";
        
        public void Configure(EntityTypeBuilder<User> builder)
        {
            User admin = new User
            {
                Id = adminId,
                UserName = "superuserramon",
                NormalizedUserName = "SUPERUSERRAMON",
                Email = "myDefaultMail@admin.com",
                NormalizedEmail = "MYDEFAULTMAIL@ADMIN.COM"
            };
            admin.PasswordHash = PasswordGenerator(admin);
            builder.HasData(admin);
        }

        public string PasswordGenerator(User user)
        {
            var passwordHash = new PasswordHasher<User>();
            return passwordHash.HashPassword(user, "Thispassword987");
        }
    }
}
