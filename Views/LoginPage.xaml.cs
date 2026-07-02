using Giga.ViewModels;

namespace Giga.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    public LoginPage(LoginViewModel viewModel) : this()
    {
        BindingContext = viewModel;
    }
}