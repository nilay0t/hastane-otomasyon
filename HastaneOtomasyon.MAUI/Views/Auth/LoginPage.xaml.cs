using HastaneOtomasyon.MAUI.Services.Interfaces;
using HastaneOtomasyon.Shared.DTOs.Auth;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace HastaneOtomasyon.MAUI.Views.Auth;

public partial class LoginPage : ContentPage
{
    private readonly IAuthService _authService;

    private string _selectedRole = "hasta";
    private bool _isPasswordVisible = false;

    public LoginPage()
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);

        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("MAUI servis sağlayıcısı bulunamadı.");

        _authService = services.GetService<IAuthService>()
            ?? throw new InvalidOperationException("IAuthService DI container içinde bulunamadı.");

        SetSelectedRole("hasta");
    }

    private void OnAdminRoleClicked(object? sender, EventArgs e)
    {
        SetSelectedRole("admin");
    }

    private void OnDoktorRoleClicked(object? sender, EventArgs e)
    {
        SetSelectedRole("doktor");
    }

    private void OnHastaRoleClicked(object? sender, EventArgs e)
    {
        SetSelectedRole("hasta");
    }

    private void SetSelectedRole(string role)
    {
        _selectedRole = role;

        ResetRoleButtons();

        switch (role)
        {
            case "admin":
                AdminRoleButton.BackgroundColor = Color.FromArgb("#FFFFFF");
                AdminRoleButton.TextColor = Color.FromArgb("#0F3D5E");
                RoleHintLabel.Text = "Sistem yöneticisi girişi";
                KullaniciAdiEntry.Title = "Admin Kullanıcı Adı";
                break;

            case "doktor":
                DoktorRoleButton.BackgroundColor = Color.FromArgb("#FFFFFF");
                DoktorRoleButton.TextColor = Color.FromArgb("#0F3D5E");
                RoleHintLabel.Text = "Doktor / klinisyen girişi";
                KullaniciAdiEntry.Title = "Doktor Kullanıcı Adı";
                break;

            case "hasta":
                HastaRoleButton.BackgroundColor = Color.FromArgb("#FFFFFF");
                HastaRoleButton.TextColor = Color.FromArgb("#0F3D5E");
                RoleHintLabel.Text = "Hasta girişi";
                KullaniciAdiEntry.Title = "Hasta Kullanıcı Adı";
                break;
        }
    }

    private void ResetRoleButtons()
    {
        AdminRoleButton.BackgroundColor = Colors.Transparent;
        AdminRoleButton.TextColor = Color.FromArgb("#8899AA");

        DoktorRoleButton.BackgroundColor = Colors.Transparent;
        DoktorRoleButton.TextColor = Color.FromArgb("#8899AA");

        HastaRoleButton.BackgroundColor = Colors.Transparent;
        HastaRoleButton.TextColor = Color.FromArgb("#8899AA");
    }

    private void OnPasswordVisibilityClicked(object? sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        SifreEntry.IsPassword = !_isPasswordVisible;
        PasswordVisibilityButton.Text = _isPasswordVisible ? "🙈" : "👁";
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(KullaniciAdiEntry.Text) ||
            string.IsNullOrWhiteSpace(SifreEntry.Text))
        {
            await Snackbar.Make("Lütfen kullanıcı adı ve şifre alanlarını doldurun", duration: TimeSpan.FromSeconds(4)).Show();
            return;
        }

        GirisButton.IsEnabled = false;
        GirisButton.Text = "Giriş yapılıyor...";

        var request = new LoginRequest
        {
            KullaniciAdi = KullaniciAdiEntry.Text.Trim(),
            Sifre = SifreEntry.Text
        };
        var response = await _authService.LoginAsync(request);

        GirisButton.IsEnabled = true;
        GirisButton.Text = "Sisteme Giriş Yap  ›";

        if (!response.Success)
        {
            await Snackbar.Make("Giriş başarısız: " + (response.Message ?? "Bilinmeyen hata"), duration: TimeSpan.FromSeconds(4)).Show();
            return;
        }

        

        if (response.Data is not null)
        {
            var gercekRol = response.Data.Rol;           

            if (!string.Equals(gercekRol, _selectedRole, StringComparison.OrdinalIgnoreCase))
            {
                await Snackbar.Make($"Bu kullanıcı '{gercekRol}' rolünde. Üstten '{gercekRol}' butonunu seçiniz", duration: TimeSpan.FromSeconds(5)).Show();

                return;
            }

            
            await HastaneOtomasyon.MAUI.Helpers.SecureTokenStorage.SetAsync("kullanici_rol", response.Data.Rol);
        }

        await Shell.Current.GoToAsync(nameof(HastaneOtomasyon.MAUI.Views.Shared.DashboardPage));

    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RegisterPage));
    }

    private async void OnForgotPasswordClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SifreDegistirPage));
    }
}