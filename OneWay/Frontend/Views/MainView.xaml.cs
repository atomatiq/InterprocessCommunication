using Frontend.ViewModels;

namespace Frontend.Views;

public sealed partial class MainView
{
    public MainView(MainViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}