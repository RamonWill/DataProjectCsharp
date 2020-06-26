using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Services.Email
{
    public interface IEmailMessenger
    {
        Task SendEmailAsync(Message message);
    }
}
