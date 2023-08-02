﻿using hairDresser.Infrastructure;
using hairDresser.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hairDresser.IntegrationTests
{
    /// <summary>
    /// Sets the enviroment for testing.
    /// In our case, we prepare a copy of the Real Database (Tables and Columns) to a In-Memory Database using SQLite.
    /// </summary>
    /// <typeparam name="TProgram"></typeparam>
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        private SqliteConnection _connection;

        public CustomWebApplicationFactory()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                //Find the service provider (Program.cs) which uses the DB and remove it.
                var serviceDescriptor = services.SingleOrDefault(service => service.ServiceType == typeof(DbContextOptions<DataContext>));
                services.Remove(serviceDescriptor);

                //Create a new service provider.
                var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .BuildServiceProvider();

                services.AddDbContext<DataContext>(options =>
                {
                    options.UseSqlite(_connection);
                    options.UseInternalServiceProvider(serviceProvider);
                }, ServiceLifetime.Scoped);

                //Build the service provider.
                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;

                    var db = scopedServices.GetRequiredService<DataContext>();

                    db.Database.EnsureDeleted();

                    //Ensure the DB is created.
                    db.Database.EnsureCreated();

                    //Send the DB with test data.
                    Utilities.InitializeDbForTests(db);
                }
            });

            // To bypass the [Authorize] Attribute from the Controller.
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _connection.Close();
            _connection.Dispose();
        }
    }
}
