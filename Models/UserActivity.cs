﻿namespace AuthProject.Models
{
    public class UserActivity
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Data { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public string MethodType { get; set; }
        public DateTime ActivityDate { get; set; } = DateTime.Now;
    }
}