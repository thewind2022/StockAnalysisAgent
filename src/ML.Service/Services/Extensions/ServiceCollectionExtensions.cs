using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ML.Service.Services;
using Shared.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Service.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMLServices(this IServiceCollection services)
        {
            services.AddSingleton<ITechnicalIndicatorService, TechnicalIndicatorService>();
            services.AddSingleton<IOnnxPredictionService, OnnxPredictionService>();
            return services;
        }
    }
}
