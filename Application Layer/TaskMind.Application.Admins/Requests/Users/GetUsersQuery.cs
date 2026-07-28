using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Requests.Users
{
    public class GetUsersQuery : IRequest<ServiceResult<List<UserDto>>>
    {
        public string? SearchText { get; set; }

        /// <summary>"All" | "Active" | "Locked" | "Banned"</summary>
        public string StatusFilter { get; set; } = "All";
    }
}
