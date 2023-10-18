using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frontend.Client;

namespace Frontend.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private string _message = string.Empty;

    [RelayCommand]
    private async Task DeleteElementsAsync()
    {
        var request = new DeleteElementsRequest();
        await App.ClientDispatcher.WriteRequestAsync(request);

        var response = await App.ClientDispatcher.ReadResponseAsync();
        if (response.Type == Response.ResponseType.Success)
        {
            var completedResponse = (DeletionCompletedResponse) response;
            MessageBox.Show($"{completedResponse.Changes} elements successfully deleted");
        }
        else if (response.Type == Response.ResponseType.Rejected)
        {
            var rejectedResponse = (RejectedResponse) response;
            MessageBox.Show($"Deletion failed\n{rejectedResponse.Reason}");
        }
    }
}