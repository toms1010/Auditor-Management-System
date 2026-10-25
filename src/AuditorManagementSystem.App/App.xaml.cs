using AuditorManagementSystem.Application.Interfaces;
using AuditorManagementSystem.Application.Services;
using AuditorManagementSystem.Core.Interfaces;
using AuditorManagementSystem.Infrastructure.Data;
using AuditorManagementSystem.Infrastructure.Export;
using AuditorManagementSystem.Infrastructure.Repositories;
using AuditorManagementSystem.Infrastructure.Security;
using AuditorManagementSystem.Infrastructure.Storage;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Windows;

namespace AuditorManagementSystem.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : global::System.Windows.Application
{
    public static IServiceProvider? Services { get; private set; }
    public static IConfiguration? Configuration { get; private set; }

    public App()
    {
        InitializeComponent();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            var services = new ServiceCollection();

            // Configuration - load from the App project directory where appsettings.json lives
            var appDir = AppContext.BaseDirectory;
            var config = new ConfigurationBuilder()
                .SetBasePath(appDir)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            Configuration = config;
            services.AddSingleton(config);

            // Logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // App data directory (safe location for DB and receipts)
            var appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AuditorManagementSystem");
            Directory.CreateDirectory(appData);
            services.AddSingleton(appData);

            // Database
            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? $"Data Source={Path.Combine(appData, "audit.db")}";
            services.AddSingleton(new DatabaseConnectionFactory(config));
            services.AddSingleton<DatabaseInitializer>();

            // Repositories (connection string passed to each)
            services.AddSingleton<IUserRepository>(sp => new UserRepository(connectionString));
            services.AddSingleton<ITransactionRepository>(sp => new TransactionRepository(connectionString));
            services.AddSingleton<IReceiptRepository>(sp => new ReceiptRepository(connectionString));
            services.AddSingleton<ICategoryRepository>(sp => new CategoryRepository(connectionString));
            services.AddSingleton<IAuditLogRepository>(sp => new AuditLogRepository(connectionString));

            // File storage abstraction
            services.AddSingleton<IReceiptFileStorage>(sp =>
                new ReceiptFileStorage(Path.Combine(appData, "Receipts")));

            // Password hasher abstraction
            services.AddSingleton<IPasswordHasher, PasswordHasher>();

            // Exporters
            services.AddSingleton<ICsvExporter, CsvExporter>();

            // Application services
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IReceiptService, ReceiptService>();
            services.AddScoped<IFinancialCalculationService, FinancialCalculationService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IApprovalService, ApprovalService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IBackupService, BackupService>();

            // ViewModels
            services.AddScoped<ViewModels.LoginViewModel>();
            services.AddScoped<ViewModels.MainWindowViewModel>();

            Services = services.BuildServiceProvider();

            // Initialize CommunityToolkit.Mvvm IoC (optional, for ViewModel locator)
            Ioc.Default.ConfigureServices(Services);

            // Initialize database
            var initializer = Services.GetRequiredService<DatabaseInitializer>();
            try
            {
                initializer.InitializeAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                var logger = Services.GetRequiredService<ILogger<App>>();
                logger.LogError(ex, "Failed to initialize database");
            }

            base.OnStartup(e);

            // Show login window first
            var login = Services.GetRequiredService<ViewModels.LoginViewModel>();
            var loginWindow = new Views.Authentication.LoginWindow
            {
                DataContext = login
            };
            loginWindow.Show();
        }
        catch (Exception ex)
        {
            // Unhandled startup exception - show a user-friendly error
            System.Windows.MessageBox.Show(
                $"The application failed to start: {ex.Message}",
                "Auditor Management System - Startup Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
            throw;
        }
    }
}
