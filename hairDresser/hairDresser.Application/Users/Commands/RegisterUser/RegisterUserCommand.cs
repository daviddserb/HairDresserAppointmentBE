﻿using hairDresser.Domain.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hairDresser.Application.Users.Commands.RegisterUser
{
    public class RegisterUserCommand : IRequest<User>
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }
}
