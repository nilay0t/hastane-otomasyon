using HastaneOtomasyon.MAUI.Views.Admin;
using HastaneOtomasyon.MAUI.Views.Auth;
using HastaneOtomasyon.MAUI.Views.Doktor;
using HastaneOtomasyon.MAUI.Views.Hasta;
using HastaneOtomasyon.MAUI.Views.Randevu;
using HastaneOtomasyon.MAUI.Views.Shared;

namespace HastaneOtomasyon.MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(SifreDegistirPage), typeof(SifreDegistirPage));
        Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));

        Routing.RegisterRoute(nameof(RandevuListPage), typeof(RandevuListPage));
        Routing.RegisterRoute(nameof(RandevuOlusturPage), typeof(RandevuOlusturPage));
        Routing.RegisterRoute(nameof(RandevuDetayPage), typeof(RandevuDetayPage));

        Routing.RegisterRoute(nameof(HastaListPage), typeof(HastaListPage));
        Routing.RegisterRoute(nameof(HastaDetayPage), typeof(HastaDetayPage));

        Routing.RegisterRoute(nameof(DoktorListPage), typeof(DoktorListPage));

        Routing.RegisterRoute(nameof(ProfilPage), typeof(ProfilPage));
        Routing.RegisterRoute(nameof(AuditLogPage), typeof(AuditLogPage));
        Routing.RegisterRoute(nameof(AdminDashboardPage), typeof(AdminDashboardPage));
    }
}