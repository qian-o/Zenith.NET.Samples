using Sample.Maui.ViewModels;

namespace Sample.Maui.Views;

public partial class MainView : ContentPage
{
	public MainView()
	{
		InitializeComponent();

        SizeChanged += (_, _) =>
        {
            MainViewModel viewModel = (MainViewModel)BindingContext;
            viewModel.ActualWidth = Width;
            viewModel.ActualHeight = Height;
        };
    }
}