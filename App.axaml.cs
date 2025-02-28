using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NDBotUI.Infrastructure;
using NDBotUI.Modules.Core.Db;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NDBotUI.UI.Base.ViewModels;
using NLog;
using MainWindow = NDBotUI.UI.Base.Views.MainWindow;

namespace NDBotUI;

public class App : Application
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            // Register all the services needed for the application to run
            var collection = new ServiceCollection();
            collection.AddCommonServices();
            collection.AddViewModels();

            // Creates a ServiceProvider containing services from the provided IServiceCollection
            var services = collection.BuildServiceProvider();
            var vm = services.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = vm,
            };

            using var context = new ApplicationDbContext();
            context.EnsureDatabaseCreated();
            CoreDataHelper.InitConfigData();

            RxEventManager.Dispatch(EmulatorAction.EmulatorInitAction.Create());
        }

        base.OnFrameworkInitializationCompleted();

        Logger.Info("Application started");
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins
                .DataValidators
                .OfType<DataAnnotationsValidationPlugin>()
                .ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}