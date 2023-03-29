﻿using hairDresser.Application.Interfaces;
using hairDresser.Domain;
using hairDresser.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hairDresser.Infrastructure.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly DataContext context;

        public AppointmentRepository(DataContext context)
        {
            this.context = context;
        }

        public async Task CreateAppointmentAsync(Appointment appointment)
        {
            await context.Appointments.AddAsync(appointment);
        }

        public async Task<IQueryable<Appointment>> GetAllAppointmentsAsync(int pageNumber, int PageSize)
        {
            return context.Appointments
                .Include(customers => customers.Customer)
                .Include(employees => employees.Employee)
                .Include(appointmentHairServices => appointmentHairServices.AppointmentHairServices)
                    .ThenInclude(hairServices => hairServices.HairService)
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize);
        }
        public async Task<Appointment> GetAppointmentByIdAsync(int appointmentId)
        {
            return await context.Appointments
                .Include(customers => customers.Customer)
                .Include(employees => employees.Employee)
                .Include(appointmentHairServices => appointmentHairServices.AppointmentHairServices)
                    .ThenInclude(hairServices => hairServices.HairService)
                .FirstOrDefaultAsync(appointment => appointment.Id == appointmentId);
        }

        public async Task<IQueryable<Appointment>> GetAllAppointmentsByCustomerIdAsync(string customerId)
        {
            return context.Appointments
                .Where(appointment => appointment.CustomerId == customerId)
                .Include(customers => customers.Customer)
                .Include(employees => employees.Employee)
                .Include(appointmentHairServices => appointmentHairServices.AppointmentHairServices)
                    .ThenInclude(hairServices => hairServices.HairService);
        }

        public async Task<IQueryable<Appointment>> GetAllAppointmentsByCustomerIdByDateAsync(string customerId, DateTime appointmentDate)
        {
            return context.Appointments
                .Where(date => date.StartDate.Date == appointmentDate.Date)
                .Where(id => id.CustomerId == customerId)
                .OrderBy(date => date.StartDate);
        }

        public async Task<IQueryable<Appointment>> GetFinishedAppointmentsByCustomerIdAsync(string customerId)
        {
            return context.Appointments
                .Where(appointment => appointment.CustomerId == customerId)
                .Where(date => date.StartDate < DateTime.Now.Date)
                .OrderBy(date => date.StartDate)
                .Include(customers => customers.Customer)
                .Include(employees => employees.Employee)
                .Include(appointmentHairServices => appointmentHairServices.AppointmentHairServices)
                    .ThenInclude(hairServices => hairServices.HairService);
        }

        public async Task<IQueryable<Appointment>> GetInWorkAppointmentsByCustomerIdAsync(string customerId)
        {
            return context.Appointments
                .Where(appointment => appointment.CustomerId == customerId)
                .Where(date => date.StartDate >= DateTime.Now.Date)
                .OrderBy(date => date.StartDate)
                .Include(customers => customers.Customer)
                .Include(employees => employees.Employee)
                .Include(appointmentHairServices => appointmentHairServices.AppointmentHairServices)
                    .ThenInclude(hairServices => hairServices.HairService);
        }

        public async Task<int> CountCustomerAppointmentsLastMonthAsync(string customerId)
        {
            return context.Appointments
                .Where(appointment => appointment.CustomerId == customerId)
                .Where(date => date.StartDate >= DateTime.Today.AddDays(-30))
                .Count();
        }

        public async Task<IQueryable<Appointment>> GetAllAppointmentsByEmployeeIdAsync(string employeeId)
        {
            return context.Appointments
                .Where(appointment => appointment.EmployeeId == employeeId)
                .Include(customers => customers.Customer)
                .Include(employees => employees.Employee)
                .Include(appointmentHairServices => appointmentHairServices.AppointmentHairServices)
                    .ThenInclude(hairServices => hairServices.HairService);
        }

        public async Task<IQueryable<Appointment>> GetAllAppointmentsByEmployeeIdByDateAsync(string employeeId, DateTime appointmentDate)
        {
            return context.Appointments
                .Where(date => date.StartDate.Date == appointmentDate.Date)
                .Where(id => id.EmployeeId == employeeId)
                .Include(customers => customers.Customer)
                .Include(employees => employees.Employee);
        }

        public async Task<IQueryable<Appointment>> GetFinishedAppointmentsByEmployeeIdAsync(string employeeId)
        {
            return context.Appointments
                .Where(appointment => appointment.EmployeeId == employeeId)
                .Where(date => date.StartDate < DateTime.Now.Date)
                .OrderBy(date => date.StartDate)
                .Include(customers => customers.Customer)
                .Include(employees => employees.Employee)
                .Include(appointmentHairServices => appointmentHairServices.AppointmentHairServices)
                    .ThenInclude(hairServices => hairServices.HairService);
        }

        public async Task<IQueryable<Appointment>> GetInWorkAppointmentsByEmployeeIdAsync(string employeeId)
        {
            return context.Appointments
                .Where(appointment => appointment.EmployeeId == employeeId)
                .Where(date => date.StartDate >= DateTime.Now.Date)
                .OrderBy(date => date.StartDate)
                .Include(customers => customers.Customer)
                .Include(employees => employees.Employee)
                .Include(appointmentHairServices => appointmentHairServices.AppointmentHairServices)
                    .ThenInclude(hairServices => hairServices.HairService);
        }

        // !!! Not permanently deleted but soft deleted.
        public async Task DeleteAppointmentAsync(int appointmentId)
        {
            var appointment = await context.Appointments.FirstOrDefaultAsync(appointment => appointment.Id == appointmentId);

            appointment.isDeleted = DateTime.Now;

            context.Appointments.Update(appointment);
        }
    }
}
