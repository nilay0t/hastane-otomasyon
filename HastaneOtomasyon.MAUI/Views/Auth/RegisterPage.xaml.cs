using HastaneOtomasyon.MAUI.Services.Interfaces;
using HastaneOtomasyon.Shared.DTOs.Auth;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
namespace HastaneOtomasyon.MAUI.Views.Auth;

public partial class RegisterPage : ContentPage
{
    private readonly IAuthService _authService;

    public RegisterPage()
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

    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AdSoyadEntry.Text) ||
            string.IsNullOrWhiteSpace(KullaniciAdiEntry.Text) ||
            string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(SifreEntry.Text) ||
            string.IsNullOrWhiteSpace(SifreTekrarEntry.Text) ||
            RolPicker.SelectedIndex == -1)
        {
            await Snackbar.Make("Lütfen tüm alanları doldurun", duration: TimeSpan.FromSeconds(4)).Show();
            return;
        }

        if (SifreEntry.Text != SifreTekrarEntry.Text)
        {
            await Snackbar.Make("Şifreler birbiriyle eşleşmiyor", duration: TimeSpan.FromSeconds(4)).Show();
            return;
        }

        if (!OnayCheckBox.IsChecked)
        {
            await Snackbar.Make("Devam etmek için bilgilerin doğru olduğunu onaylamalısınız", duration: TimeSpan.FromSeconds(4)).Show();
            return;
        }

        KayitButton.IsEnabled = false;
        KayitButton.Text = "Kayıt oluşturuluyor...";

        var request = new RegisterRequest
        {
            AdSoyad = AdSoyadEntry.Text.Trim(),
            KullaniciAdi = KullaniciAdiEntry.Text.Trim(),
            Email = EmailEntry.Text.Trim(),
            Sifre = SifreEntry.Text,
            RolId = GetSelectedRoleId()
        };

        var response = await _authService.RegisterAsync(request);

        KayitButton.IsEnabled = true;
        KayitButton.Text = "Kayıt Oluştur";

        if (!response.Success)
        {
            await Snackbar.Make("Kayıt başarısız: " + (response.Message ?? "Bilinmeyen hata"), duration: TimeSpan.FromSeconds(5)).Show();
            return;
        }

        await Toast.Make("Kayıt başarıyla oluşturuldu", ToastDuration.Short).Show();
        await Shell.Current.GoToAsync("..");
    }

    private int GetSelectedRoleId()
    {
        var selectedRole = RolPicker.SelectedItem?.ToString();

        return selectedRole switch
        {
            "Admin" => 1,
            "Doktor" => 2,
            "Hasta" => 3,
            _ => 3
        };
    }

    private void OnRolPickerSelectionChanged(object? sender, EventArgs e)
    {
        
        if (RolPicker.SelectedItem is string secilen)
        {
            RolPicker.Title = secilen;
        }
    }
}