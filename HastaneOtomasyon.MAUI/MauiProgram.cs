using Microsoft.Extensions.Logging;
using HastaneOtomasyon.MAUI.Services.Interfaces;
using HastaneOtomasyon.MAUI.Services.Implementations;
using CommunityToolkit.Maui;
using Syncfusion.Maui.Toolkit.Hosting;
using UraniumUI;
using DevExpress.Maui;
using UraniumUI.Material;

namespace HastaneOtomasyon.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseDevExpress(useLocalization: false)
            .UseDevExpressCollectionView()
            .UseDevExpressControls()
            .UseDevExpressEditors()
            .UseDevExpressDataGrid()
            .UseDevExpressCharts()
            .UseMauiCommunityToolkit(options =>
            {
                options.SetShouldEnableSnackbarOnWindows(true);
            })
            .ConfigureSyncfusionToolkit()
            .UseUraniumUI()
            .UseUraniumUIMaterial()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton(_ => new HttpClient
        {
            BaseAddress = new Uri(GetApiBaseUrl())
        });

        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IRandevuService, RandevuService>();
        builder.Services.AddSingleton<IHastaService, HastaService>();
        builder.Services.AddSingleton<IDoktorService, DoktorService>();
        builder.Services.AddSingleton<IAuditLogService, AuditLogService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static string GetApiBaseUrl()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            return "http://10.0.2.2:5207/api/";
        }

        return "http://localhost:5207/api/";
    }
}