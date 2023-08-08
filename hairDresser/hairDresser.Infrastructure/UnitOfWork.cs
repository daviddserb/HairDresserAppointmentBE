﻿using hairDresser.Application.Interfaces;
using hairDresser.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hairDresser.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _dataContext;

        public UnitOfWork
        (
            DataContext dataContext,
            IAppointmentRepository appointmentRepository,
            IHairServiceRepository hairServiceRepository,
            IWorkingDayRepository workingDayRepository,
            IWorkingIntervalRepository workingIntervalRepository,
            IUserRepository userRepository
        )
        {
            _dataContext = dataContext;
            AppointmentRepository = appointmentRepository;
            HairServiceRepository = hairServiceRepository;
            WorkingDayRepository = workingDayRepository;
            WorkingIntervalRepository = workingIntervalRepository;
            UserRepository = userRepository;
        }

        public IAppointmentRepository AppointmentRepository { get; private set; }
        public IHairServiceRepository HairServiceRepository { get; private set; }
        public IWorkingDayRepository WorkingDayRepository { get; private set; }
        public IWorkingIntervalRepository WorkingIntervalRepository { get; private set; }
        public IUserRepository UserRepository { get; private set; }

        public async Task SaveAsync()
        {
            await _dataContext.SaveChangesAsync();
        }

        public async void Dispose()
        {
            await _dataContext.DisposeAsync();
        }
    }
}
