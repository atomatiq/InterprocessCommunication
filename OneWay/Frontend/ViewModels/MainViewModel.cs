using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frontend.Client;

namespace Frontend.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private string _message = string.Empty;

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        var request = new PrintMessageRequest(Message);
        await App.ClientDispatcher.WriteRequestAsync(request);
    }

    [RelayCommand]
    private async Task UpdateModelAsync()
    {
        var request = new UpdateModelRequest(AppDomain.CurrentDomain.FriendlyName, 666, true);
        await App.ClientDispatcher.WriteRequestAsync(request);
    }
}