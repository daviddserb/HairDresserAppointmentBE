﻿using hairDresser.Application.CustomExceptions;
using hairDresser.Application.Interfaces;
using hairDresser.Domain.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hairDresser.Application.Users.Queries.GetEmployeeFreeIntervalsByDate
{
    public class GetEmployeeFreeIntervalsByDateQueryHandler : IRequestHandler<GetEmployeeFreeIntervalsByDateQuery, List<EmployeeFreeInterval>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetEmployeeFreeIntervalsByDateQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<EmployeeFreeInterval>> Handle(GetEmployeeFreeIntervalsByDateQuery request, CancellationToken cancellationToken)
        {
            // Set a maximum number of appointments per month for the customer.
            var maximumCustomerAppointmentsPerMonth = 5;
            var customerAppointmentsLastMonth = await _unitOfWork.AppointmentRepository.GetHowManyAppointmentsCustomerHasInLastMonthAsync(request.CustomerId);
            if (customerAppointmentsLastMonth > maximumCustomerAppointmentsPerMonth) throw new ClientException("Can't create more appointments for this month because you have reached the appointments limit!");

            var appointmentDate = new DateTime(request.Year, request.Month, request.Date);
            Console.WriteLine($"appointment (with the selected Date, ignore Time)= '{appointmentDate}'");

            var duration = TimeSpan.FromMinutes(request.DurationInMinutes);
            Console.WriteLine($"\nduration (converted from minutes to TimeSpan)= '{duration}'");

            var employeeAppointmentsDates = new List<(DateTime startDate, DateTime endDate)>();
            Console.WriteLine("\nAll the appointments from the selected employee and the selected date:");
            foreach (var appointment in await _unitOfWork.AppointmentRepository.GetAllAppointmentsByEmployeeIdByDateAsync(request.EmployeeId, appointmentDate))
            {
                employeeAppointmentsDates.Add((appointment.StartDate, appointment.EndDate));
                // BEFORE:
                //Console.WriteLine($"{appointment.Id} - customer= '{appointment.Customer.Name}', employee='{appointment.Employee.Name}', start= '{appointment.StartDate}', end= '{appointment.EndDate}'");
                // AFTER v1:
                Console.WriteLine($"{appointment.Id} - customer= '{appointment.Customer.UserName}', employee='{appointment.Employee.UserName}', start= '{appointment.StartDate}', end= '{appointment.EndDate}'");
            }

            Console.WriteLine("\nSorted appointments:");
            var sorted_employeeAppointmentsDates = employeeAppointmentsDates.OrderBy(date => date.startDate);
            foreach (var sortedAppointment in sorted_employeeAppointmentsDates)
            {
                Console.WriteLine($"start= '{sortedAppointment.startDate}', end= '{sortedAppointment.endDate}'");
            }

            var nameOfDay = appointmentDate.ToString("dddd");
            Console.WriteLine($"\nname of the day based on the selected date is= '{nameOfDay}'");

            var workingDay = await _unitOfWork.WorkingDayRepository.GetWorkingDayByNameAsync(nameOfDay);

            var employeeWorkingIntervals = await _unitOfWork.WorkingIntervalRepository.GetWorkingIntervalsByEmployeeIdByWorkingDayIdAsync(request.EmployeeId, workingDay.Id);

            if (!employeeWorkingIntervals.Any()) return await Task.FromResult(new List<EmployeeFreeInterval>());

            var possibleIntervals = new List<DateTime>();
            foreach (var intervals in employeeWorkingIntervals)
            {
                Console.WriteLine($"day intervals (start - end): '{intervals.StartTime}' - '{intervals.EndTime}'");
                possibleIntervals.Add(appointmentDate.Add(intervals.StartTime));
                foreach (var app in sorted_employeeAppointmentsDates)
                {
                    if (intervals.StartTime <= app.startDate.TimeOfDay && app.endDate.TimeOfDay <= intervals.EndTime)
                    {
                        possibleIntervals.Add(app.startDate);
                        possibleIntervals.Add(app.endDate);
                    }
                }
                possibleIntervals.Add(appointmentDate.Add(intervals.EndTime));
            }
            Console.WriteLine("\nPossible Intervals:");
            foreach (var intervals in possibleIntervals)
            {
                Console.WriteLine(intervals);
            }

            // Am lista asta doar de testare a intervalelor. !!! Ca sa scap de ea, as putea sa parsez lista de EmployeeFreeInterval, dar trebuie sa invat sa vad cum fac asta.
            //var validIntervals = new List<(DateTime startDate, DateTime endDate)>();
            var employeeFreeIntervalList = new List<EmployeeFreeInterval>();
            Console.WriteLine("\nCheck for valid intervals:");
            for (int i = 0; i < possibleIntervals.Count - 1; i += 2)
            {
                var startOfInterval = possibleIntervals[i];
                var copy_startOfInterval = startOfInterval;
                var endOfInterval = possibleIntervals[i + 1];
                Console.WriteLine($"intervals (start - end): '{startOfInterval.TimeOfDay}' - '{endOfInterval.TimeOfDay}'");
                Console.WriteLine("Valid dates:");
                while ((startOfInterval += duration) <= endOfInterval)
                {
                    Console.WriteLine(copy_startOfInterval.TimeOfDay + " - " + startOfInterval.TimeOfDay);
                    //validIntervals.Add((copy_startOfInterval, startOfInterval));
                    employeeFreeIntervalList.Add(new EmployeeFreeInterval { StartDate = copy_startOfInterval, EndDate = startOfInterval });
                    copy_startOfInterval = startOfInterval;
                }
            }
            Console.WriteLine("\nEMPLOYEE FREE INTERVALS FOR APPOINTMENT:");
            foreach (var employeeFreeInterval in employeeFreeIntervalList)
            {
                Console.WriteLine($"start= '{employeeFreeInterval.StartDate}', end= '{employeeFreeInterval.EndDate}'");
            }

            var customerAppointmentsInSelectedDate = await _unitOfWork.AppointmentRepository.GetAllAppointmentsByCustomerIdByDateAsync(request.CustomerId, appointmentDate);
            Console.WriteLine("\nCUSTOMER CURRENT APPOINTMENTS:");
            foreach (var customerAppointments in customerAppointmentsInSelectedDate)
            {
                Console.WriteLine($"start= '{customerAppointments.StartDate}', end= '{customerAppointments.EndDate}'");
            }

            // Check, all the possible free intervals from the employee in the selected date, to don't overlap with the appointments from the customer in the selected date.
            for (int i = 0; i < employeeFreeIntervalList.Count; ++i)
            {
                //Console.WriteLine("i= " + i);
                //Console.WriteLine($"EMPLOYEE: start= '{employeeFreeIntervalList[i].StartDate}', end= '{employeeFreeIntervalList[i].EndDate}'");
                foreach (var customerAppointments in customerAppointmentsInSelectedDate)
                {
                    //Console.WriteLine($"CUSTOMER: start= '{customerAppointments.StartDate}', end= '{customerAppointments.EndDate}'");
                    bool overlap = employeeFreeIntervalList[i].StartDate < customerAppointments.EndDate && customerAppointments.StartDate < employeeFreeIntervalList[i].EndDate;
                    //Console.WriteLine("overlap= " + overlap);
                    if (overlap)
                    {
                        employeeFreeIntervalList.RemoveAt(i);
                        --i; // ! Because otherwise it will jump over the next element when it will remove the current one (think about how the algorithm of removing works and it makes sense).
                        break;
                    }
                }
                //Console.WriteLine("---\n");
            }

            Console.WriteLine("\nFINAL VALID INTERVALS:");
            foreach (var employeeFreeInterval in employeeFreeIntervalList)
            {
                Console.WriteLine($"start= '{employeeFreeInterval.StartDate}', end= '{employeeFreeInterval.EndDate}'");
            }

            return await Task.FromResult(employeeFreeIntervalList);
        }
    }
}
