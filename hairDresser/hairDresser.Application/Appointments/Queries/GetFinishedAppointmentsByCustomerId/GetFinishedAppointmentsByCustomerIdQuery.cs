﻿using hairDresser.Domain.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hairDresser.Application.Appointments.Queries.GetFinishedAppointmentsByCustomerId
{
    public class GetFinishedAppointmentsByCustomerIdQuery : IRequest<IQueryable<Appointment>>
    {
        public string CustomerId { get; set; }
    }
}
