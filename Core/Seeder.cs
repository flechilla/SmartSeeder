using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SeedEngine.Utilities;
using Serilog;

namespace SeedEngine.Core
{
    public static class Seeder
    {
        /// <summary>
        ///     Discover all the classes that are used
        ///     for the seeds of the application using reflection
        ///     and Invoke the method AddOrUpdate that is implemented
        ///     in each one.
        /// </summary>
        /// <typeparam name="T">The type of the context used to add the new objects.</typeparam>
        /// <param name="app">The <see cref="IWebHost" /> that this method will extend.</param>
        public static IWebHost EnsureSeedData<T>(this IWebHost host) where T : DbContext
        {

            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetService(typeof(T)) as T;

                var stopWatch = new Stopwatch();
                stopWatch.Start();
                Log.Debug("Starting the seeding of the objects...");

                var contextAssambly = typeof(T).Assembly;

                if (!context.AllMigrationsApplied())
                    context.Database.Migrate();

                var seedInstances = contextAssambly.GetTypes()
                    .Where(type => type.GetInterfaces().Any(t => t == typeof(ISeed)))
                    .Select(t => (t, Activator.CreateInstance(t)));

                Log.Debug($"Founded {seedInstances.Count()} seed classes.");

                var orderedSeeds = seedInstances.OrderBy(tuple =>
                {
                    var order = (int) tuple.Item1.GetProperty("OrderToByApplied").GetValue(tuple.Item2, null);
                    return order;
                });

                foreach (var seedTuple in orderedSeeds)
                    try
                    {
                        seedTuple.Item1.GetMethod("AddOrUpdate")
                            .Invoke(seedTuple.Item2, new List<object> {context, 100}
                                .ToArray());
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Exception seeding in the seed class of name: {seedTuple.Item1.FullName}");
                    }
                stopWatch.Stop();
                Log.Debug($"Finished the seeding process after {stopWatch.Elapsed.Seconds}");
                context.Dispose();
            }

            return host;
        }
        /// <summary>
        ///     Discover all the classes that are used
        ///     for the seeds of the application using reflection
        ///     and Invoke the method AddOrUpdate that is implemented
        ///     in each one.
        /// </summary>
        /// <typeparam name="T">The type of the context used to add the new objects.</typeparam>
        /// <param name="app">The <see cref="IWebHost" /> that this method will extend.</param>
        public static void EnsureSeedData<T>(IServiceScope scope) where T : DbContext
        {

           
                var context = scope.ServiceProvider.GetService(typeof(T)) as T;

                var stopWatch = new Stopwatch();
                stopWatch.Start();
                Log.Debug("Starting the seeding of the objects...");

                var contextAssambly = typeof(T).Assembly;

                if (!context.AllMigrationsApplied())
                    context.Database.Migrate();

                var seedInstances = contextAssambly.GetTypes()
                    .Where(type => type.GetInterfaces().Any(t => t == typeof(ISeed)))
                    .Select(t => (t, Activator.CreateInstance(t)));

                Log.Debug($"Founded {seedInstances.Count()} seed classes.");

                var orderedSeeds = seedInstances.OrderBy(tuple =>
                {
                    var order = (int) tuple.Item1.GetProperty("OrderToByApplied").GetValue(tuple.Item2, null);
                    return order;
                });

                foreach (var seedTuple in orderedSeeds)
                    try
                    {
                        seedTuple.Item1.GetMethod("AddOrUpdate")
                            .Invoke(seedTuple.Item2, new List<object> {context, 100}
                                .ToArray());
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Exception seeding in the seed class of name: {seedTuple.Item1.FullName}");
                    }
                stopWatch.Stop();
                Log.Debug($"Finished the seeding process after {stopWatch.Elapsed.Seconds}");
                context.Dispose();
        }
    }
}