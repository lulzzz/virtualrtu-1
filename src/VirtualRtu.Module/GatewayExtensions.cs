﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using VirtualRtu.Configuration;

namespace VirtualRtu.Module
{
    public static class GatewayExtensions
    {
        public static IServiceCollection AddModuleConfiguration(this IServiceCollection services, out ModuleConfig config)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            if (File.Exists("./data/config.json"))
            {
                builder.AddJsonFile("./data/config.json")
                    .AddEnvironmentVariables("MC_");
            }
            IConfigurationRoot root = builder.Build();
            config = new ModuleConfig();
            ConfigurationBinder.Bind(root, config);
            services.AddSingleton<ModuleConfig>(config);

            return services;
        }

        public static ILoggingBuilder AddLogging(this ILoggingBuilder builder, ModuleConfig config)
        {
            LogLevel logLevel = config.LoggingLevel;
            builder.AddConsole();
            builder.SetMinimumLevel(logLevel);

            return builder;
        }
    }
}
