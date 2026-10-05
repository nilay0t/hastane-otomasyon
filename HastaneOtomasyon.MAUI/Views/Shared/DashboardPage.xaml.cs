using HastaneOtomasyon.MAUI.Helpers;
using HastaneOtomasyon.MAUI.Services.Interfaces;
using HastaneOtomasyon.MAUI.Views.Admin;
using HastaneOtomasyon.MAUI.Views.Doktor;
using HastaneOtomasyon.MAUI.Views.Hasta;
using HastaneOtomasyon.MAUI.Views.Randevu;
using Microsoft.Extensions.DependencyInjection;

namespace HastaneOtomasyon.MAUI.Views.Shared;

public partial class DashboardPage : ContentPage
{
    private readonly IAuthService _authService;

    public DashboardPage()
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);

        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("MAUI servis sağlayıcısı bulunamadı.");

        _authService = services.GetService<IAuthService>()
            ?? throw new InvalidOperationException("IAuthService DI container içinde bulunamadı.");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RolBazliKartlariGosterAsync();
    }

    private async Task RolBazliKartlariGosterAsync()
    {
        var rol = await SecureTokenStorage.GetAsync("kullanici_rol");

        bool isAdmin = rol == "Admin";
        bool isDoktor = rol == "Doktor";
        bool isHasta = rol == "Hasta";

        RandevuCard.IsVisible = isAdmin || isDoktor || isHasta;
        HastaCard.IsVisible = isAdmin || isDoktor;
        DoktorCard.IsVisible = isAdmin || isHasta;
        ProfilCard.IsVisible = true;
        AdminCard.IsVisible = isAdmin;
    }

    private async void OnRandevularTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RandevuListPage));
    }

    private async void OnHastalarTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(HastaListPage));
    }

    private async void OnDoktorlarTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(DoktorListPage));
    }

    private async void OnProfilTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProfilPage));
    }

    private async void OnAuditLogTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AuditLogPage));
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Çıkış Yap",
            "Oturumu kapatmak istediğinize emin misiniz?",
            "Evet",
            "Vazgeç");

        if (!confirm)
            return;

        await _authService.LogoutAsync();
        SecureTokenStorage.Remove("kullanici_rol");
        await Shell.Current.GoToAsync("//LoginPage");
    }
}