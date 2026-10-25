using AuditorManagementSystem.Core.DTOs;
using AuditorManagementSystem.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Windows;

namespace AuditorManagementSystem.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;
    private readonly ISessionService _sessionService;
    private readonly IAuthorizationService _authzService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoggingIn;

    public LoginViewModel(
        IAuthenticationService authService,
        ISessionService sessionService,
        IAuthorizationService authzService)
    {
        _authService = authService;
        _sessionService = sessionService;
        _authzService = authzService;
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        IsLoggingIn = true;
        ErrorMessage = string.Empty;

        try
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter both username and password.";
                return;
            }

            var result = await _authService.LoginAsync(new Core.DTOs.LoginDto
            {
                Username = Username,
                Password = Password
            });

            if (result.Success && result.User is { } user)
            {
                _sessionService.SignIn(user);
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var vm = App.Services?.GetRequiredService<ViewModels.MainWindowViewModel>();
                    if (vm == null)
                    {
                        ErrorMessage = "Application services not initialized.";
                        return;
                    }
                    var mainWindow = new Views.Shell.MainWindow
                    {
                        DataContext = vm
                    };
                    mainWindow.Show();
                    foreach (Window w in System.Windows.Application.Current.Windows)
                    {
                        if (w is Views.Authentication.LoginWindow)
                            w.Close();
                    }
                });
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Invalid username or password.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login error: {ex.Message}";
        }
        finally
        {
            IsLoggingIn = false;
        }
    }

    private bool CanLogin() => !IsLoggingIn && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
}
