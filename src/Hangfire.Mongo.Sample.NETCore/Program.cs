﻿using System;
using Hangfire.Logging.LogProviders;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Mongo2Go;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Hangfire.Mongo.Sample.NETCore
{
    public class Program
    {
        private const int JobCount = 100;

        public static void Main()
        {
            var migrationOptions = new MongoStorageOptions
            {
                MigrationOptions = new MongoMigrationOptions
                {
                    MigrationStrategy = new MigrateMongoMigrationStrategy(),
                    BackupStrategy = new NoneMongoBackupStrategy()
                }
            };

            var conventionPack = new ConventionPack {new CamelCaseElementNameConvention()}; 
            ConventionRegistry.Register("CamelCase", conventionPack, t => true);
            
            GlobalConfiguration.Configuration.UseLogProvider(new ColouredConsoleLogProvider());
            var runner = MongoDbRunner.Start(singleNodeReplSet: false);
            JobStorage.Current = new MongoStorage(
                MongoClientSettings.FromConnectionString(runner.ConnectionString), 
                "Mongo-Hangfire-Sample-NETCore",
                migrationOptions);

            using (new BackgroundJobServer())
            {
                for (var i = 0; i < JobCount; i++)
                {
                    var jobId = i;
                    BackgroundJob.Enqueue(() => Console.WriteLine($"Fire-and-forget ({jobId})"));
                }

                Console.WriteLine($"{JobCount} job(s) has been enqued. They will be executed shortly!");
                Console.WriteLine();
                Console.WriteLine("If you close this application before they are executed, ");
                Console.WriteLine("they will be executed the next time you run this sample.");
                Console.WriteLine();
                Console.WriteLine("Press [enter] to exit...");

                Console.Read();
            }
        }
    }
}
