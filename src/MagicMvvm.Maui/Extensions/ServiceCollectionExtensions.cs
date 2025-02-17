﻿using System.Reflection;
using MagicMvvm.AppModel;
using MagicMvvm.Attributes;
using MagicMvvm.Dialogs;
using MagicMvvm.Navigation;
using MagicMvvm.Options;

namespace MagicMvvm;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MVVM support.
    /// </summary>
    /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IMvvmBuilder"/></returns>
    public static IMvvmBuilder AddMvvmMaui(this IServiceCollection services)
    {
        return services.AddMvvmMaui(new MvvmOptions());
    }

    /// <summary>
    /// Adds MVVM support.
    /// </summary>
    /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IMvvmBuilder"/></returns>
    public static IMvvmBuilder AddMvvmMaui(this IServiceCollection services, Action<MvvmOptions> configure)
    {
        var options = new MvvmOptions();
        configure.Invoke(options);
        return services.AddMvvmMaui(options);
    }

    /// <summary>
    /// Adds MVVM support.
    /// </summary>
    /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IMvvmBuilder"/></returns>
    public static IMvvmBuilder AddMvvmMaui(this IServiceCollection services, MvvmOptions options)
    {
        services.AddScoped<IKeyboardMapper, KeyboardMapper>();
        services.AddScoped<IActionSheetButton, ActionSheetButton>();
        services.AddSingleton<INavigationManager, NavigationManager>();
        services.AddSingleton<IShellNavigationManager, ShellNavigationManager>();
        services.AddSingleton<IDialogManager, DialogManager>();
        services.AddSingleton<IPopupDialog, PopupDialog>();
        services.AddSingleton<INavigationRegistry, NavigationRegistry>(i => NavigationRegistry.Instance);
        services.AddSingleton<IAppProvider, AppProvider>(i => new AppProvider(i));
        services.AddSingleton<IMvvmBuilder, MvvmBuilder>();
        services.AddSingleton(options);

        RegisterViewModels(services);
        RegisterViews(services, options);
        return services.BuildServiceProvider().GetRequiredService<IMvvmBuilder>();
    }

    private static void RegisterViewModels(IServiceCollection services)
    {
        RegisterTypes<ViewModelBase>(
            services, 
            onAfterRegister: (type) => RegistryContainer.Instance.Types.Add(type)
            );
    }

    private static void RegisterViews(IServiceCollection services, MvvmOptions options)
    {
        if (options.AutoWireViewModels)
        {
            RegisterTypes<IWiredView>(services, WireViewModel);
        }
        else
        {
            RegisterTypes<IWiredView>(services);
        }
    }

    private static void RegisterTypes<T>(
        IServiceCollection services, 
        Func<IServiceProvider, Type, object> factory = null,
        Action<Type> onAfterRegister = null)
    {
        var dataType = typeof(T);
        var definedTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(i => i.DefinedTypes)
            .Where(i => i.IsClass && !i.IsAbstract && dataType.IsAssignableFrom(i));

        foreach (Type type in definedTypes)
        {
            var attr = type.GetCustomAttribute<ServiceLifetimeAttribute>();
            if (attr == null)
            {
                if (factory != null)
                {
                    services.AddScoped(type, i => factory.Invoke(i, type));
                }
                else
                {
                    services.AddScoped(type);
                }

                onAfterRegister?.Invoke(type);
                continue;
            }

            switch (attr.ServiceLifetime)
            {
                case ServiceLifetime.Scoped:
                {
                    if (factory != null)
                    {
                        services.AddScoped(type, i => factory.Invoke(i, type));
                    }
                    else
                    {
                        services.AddScoped(type);
                    }
                    break;
                }
                case ServiceLifetime.Transient:
                {
                    if (factory != null)
                    {
                        services.AddTransient(type, i => factory.Invoke(i, type));
                    }
                    else
                    {
                        services.AddTransient(type);
                    }
                    break;
                }
                case ServiceLifetime.Singleton:
                {
                    if (factory != null)
                    {
                        services.AddSingleton(type, i => factory.Invoke(i, type));
                    }
                    else
                    {
                        services.AddSingleton(type);
                    }
                    break;
                }
                default:
                {
                    if (factory != null)
                    {
                        services.AddScoped(type, i => factory.Invoke(i, type));
                    }
                    else
                    {
                        services.AddScoped(type);
                    }
                    break;
                } 
            }

            onAfterRegister?.Invoke(type);
        }
    }

    private static object WireViewModel(IServiceProvider serviceProvider, Type viewType)
    {
        var dataType = typeof(ViewModelBase);
        var viewModelType = RegistryContainer.Instance.Types
                    .FirstOrDefault(
                        i => i.IsClass && !i.IsAbstract &&
                        dataType.IsAssignableFrom(i) &&
                        i.Name.Contains(viewType.Name) &&
                        i.Name.EndsWith("ViewModel")
                    );

        var viewModel = serviceProvider.GetService(viewModelType);
        var ctorParams = GetCtorParameters(serviceProvider, viewType);
        var view = Activator.CreateInstance(viewType, ctorParams) as VisualElement;
        view.BindingContext = viewModel;
        return view;
    }

    private static object[] GetCtorParameters(IServiceProvider serviceProvider, Type type)
    {
        var ctorParams = new List<object>();
        var parameters = type.GetConstructors().FirstOrDefault(i => i.IsPublic)?.GetParameters();

        if (parameters is null)
        {
            return null;
        }

        foreach (var parameter in parameters)
        {
            ctorParams.Add(serviceProvider.GetRequiredService(parameter.ParameterType));
        }
        return ctorParams.ToArray();
    }
}