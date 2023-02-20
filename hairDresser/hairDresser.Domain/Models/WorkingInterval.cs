﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hairDresser.Domain.Models
{
    public class WorkingInterval
    {
        public int Id { get; set; }

        public int WorkingDayId { get; set; }
        public WorkingDay WorkingDay { get; set; }

        public string EmployeeId { get; set; }
        public User Employee { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
