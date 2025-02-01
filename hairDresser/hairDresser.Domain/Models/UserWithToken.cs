﻿namespace hairDresser.Domain.Models
{
    public class UserWithToken
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
