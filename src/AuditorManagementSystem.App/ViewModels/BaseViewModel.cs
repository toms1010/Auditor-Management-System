using AuditorManagementSystem.Core.DTOs;
using AuditorManagementSystem.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuditorManagementSystem.App.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    protected readonly ISessionService _sessionService;

    protected BaseViewModel(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public UserDto? CurrentUser => _sessionService.CurrentUser;

    [RelayCommand]
    public virtual void Cancel()
    {
        // Override in derived viewmodels
    }

    [RelayCommand]
    public virtual void Delete()
    {
        // Override in derived viewmodels
    }
}
