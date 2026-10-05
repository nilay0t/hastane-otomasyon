using HastaneOtomasyon.MAUI.Services.Interfaces;
using HastaneOtomasyon.Shared.DTOs.Auth;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
namespace HastaneOtomasyon.MAUI.Views.Auth;

public partial class SifreDegistirPage : ContentPage
{
    private readonly IAuthService _authService;

    public SifreDegistirPage()
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);

        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("MAUI servis sağlayıcısı bulunamadı.");

        _authService = services.GetService<IAuthService>()
            ?? throw new InvalidOperationException("IAuthService DI container içinde bulunamadı.");
    }

    private async void OnBackToLoginClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnChangePasswordClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EskiSifreEntry.Text) ||
            string.IsNullOrWhiteSpace(YeniSifreEntry.Text) ||
            string.IsNullOrWhiteSpace(YeniSifreTekrarEntry.Text))
        {
            await Snackbar.Make("Lütfen tüm şifre alanlarını doldurun", duration: TimeSpan.FromSeconds(4)).Show();
            return;
        }

        if (YeniSifreEntry.Text.Length < 6)
        {
            await Snackbar.Make("Yeni şifre en az 6 karakter olmalıdır", duration: TimeSpan.FromSeconds(4)).Show();
            return;
        }

        if (YeniSifreEntry.Text != YeniSifreTekrarEntry.Text)
        {
            await Snackbar.Make("Yeni şifreler birbiriyle eşleşmiyor", duration: TimeSpan.FromSeconds(4)).Show();
            return;
        }

        if (EskiSifreEntry.Text == YeniSifreEntry.Text)
        {
            await Snackbar.Make("Yeni şifre eski şifre ile aynı olamaz", duration: TimeSpan.FromSeconds(4)).Show();
            return;
        }

        SifreDegistirButton.IsEnabled = false;
        SifreDegistirButton.Text = "Güncelleniyor...";

        var request = new SifreDegistirRequest
        {
            EskiSifre = EskiSifreEntry.Text,
            YeniSifre = YeniSifreEntry.Text
        };

        var response = await _authService.SifreDegistirAsync(request);

        SifreDegistirButton.IsEnabled = true;
        SifreDegistirButton.Text = "Şifreyi Güncelle";

        if (!response.Success)
        {
            await Snackbar.Make("Şifre değiştirme başarısız: " + (response.Message ?? "Bilinmeyen hata"), duration: TimeSpan.FromSeconds(5)).Show();
            return;
        }

        await Toast.Make("Şifre başarıyla değiştirildi", ToastDuration.Short).Show();
        await Shell.Current.GoToAsync("..");
    }
}