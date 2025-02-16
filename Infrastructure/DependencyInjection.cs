using System;
using Microsoft.Extensions.DependencyInjection;
using NDBotUI.UI.Base.ViewModels;
using ReactiveUI;

namespace NDBotUI.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
    }

    public static void AddViewModels(this IServiceCollection collection)
    {
        // ViewModels
        collection.AddTransient<MainWindowViewModel>();

        // Ví dụ về khai báo Factory
        collection.AddTransient<Func<IScreen, AutoContainerViewModel>>(
            provider =>
            {
                return screen => new AutoContainerViewModel(
                    screen
                    // provider.GetRequiredService<IMyService>() // Inject service khác vào
                );
            }
        );
    }
}