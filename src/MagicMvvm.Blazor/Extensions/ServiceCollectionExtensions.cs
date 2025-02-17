﻿using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MagicMvvm.Parameters;
using MagicMvvm.Attributes;

namespace MagicMvvm;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MVVM support.
    /// </summary>
    /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddMvvmBlazor(this IServiceCollection services)
    {
        services.AddSingleton<IParameterResolver, ParameterResolver>();
        services.AddSingleton<IParameterCache, ParameterCache>();
        services.AddSingleton<IParameterSetter, ParameterSetter>();

        var viewModelType = typeof(ViewModelBase);
        var definedTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(i => i.DefinedTypes);

        foreach (var type in definedTypes)
        {
            var attr = type.GetCustomAttribute<ServiceLifetimeAttribute>();
            if (viewModelType.IsAssignableFrom(type) &&
                type.IsClass && !type.IsAbstract)
            {
                if (attr == null)
                {
                    services.AddScoped(type);
                    continue;
                }

                switch (attr.ServiceLifetime)
                {
                    case ServiceLifetime.Scoped:
                        services.AddScoped(type);
                        break;
                    case ServiceLifetime.Transient:
                        services.AddTransient(type);
                        break;
                    case ServiceLifetime.Singleton:
                        services.AddSingleton(type);
                        break;
                    default:
                        services.AddScoped(type);
                        break;
                }
            }
        }
        return services;
    }
}