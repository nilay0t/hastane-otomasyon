using HastaneOtomasyon.MAUI.Services.Interfaces;
using HastaneOtomasyon.MAUI.Views.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace HastaneOtomasyon.MAUI.Views.Shared;

public partial class ProfilPage : ContentPage
{
    private readonly IAuthService _authService;

    public ProfilPage()
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);

        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("MAUI servis sağlayıcısı bulunamadı.");

        _authService = services.GetService<IAuthService>()
            ?? throw new InvalidOperationException("IAuthService DI container içinde bulunamadı.");
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnChangePasswordClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SifreDegistirPage));
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Çıkış Yap",
            "Oturumu kapatmak istediğinize emin misiniz?",
            "Evet",
            "Vazgeç");

        if (!confirm)
            return;

        await _authService.LogoutAsync();

        await Shell.Current.GoToAsync("//LoginPage");
    }
}