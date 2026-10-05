using System.Reflection;
using HastaneOtomasyon.MAUI.Services.Interfaces;
using HastaneOtomasyon.Shared.DTOs.Common;
using HastaneOtomasyon.Shared.DTOs.Randevu;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
namespace HastaneOtomasyon.MAUI.Views.Randevu;

public partial class RandevuDetayPage : ContentPage, IQueryAttributable
{
    private readonly IRandevuService _randevuService;
    private int _randevuId;

    public RandevuDetayPage()
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);

        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("MAUI servis sağlayıcısı bulunamadı.");

        _randevuService = services.GetService<IRandevuService>()
            ?? throw new InvalidOperationException("IRandevuService DI container içinde bulunamadı.");
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idValue) &&
            int.TryParse(idValue?.ToString(), out var parsedId))
        {
            _randevuId = parsedId;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_randevuId <= 0)
        {
            SetError("Randevu detayını görüntülemek için geçerli bir id bilgisi bulunamadı.");
            return;
        }

        await LoadRandevuDetailAsync();
    }

    private async Task LoadRandevuDetailAsync()
    {
        SetLoading("Randevu detayı backend servisinden yükleniyor...");
        DetailContainer.IsVisible = false;

        var response = await _randevuService.GetByIdAsync(_randevuId);

        if (!GetResponseSuccess(response))
        {
            SetError(GetResponseMessage(response, "Randevu detayı alınırken hata oluştu."));
            return;
        }

        var dto = GetResponseData(response);

        if (dto == null)
        {
            SetError("Backend servisinden randevu detayı boş döndü.");
            return;
        }

        BindRandevu(dto);
        SetReady("Randevu detayı yüklendi.");
    }

    private void BindRandevu(RandevuListDto dto)
    {
        var id = GetIntValue(dto, "Id", "RandevuId");

        var hasta = GetStringValue(dto,
            "HastaAdSoyad", "HastaAdiSoyadi", "HastaAd", "HastaAdi", "Hasta");

        var doktor = GetStringValue(dto,
            "DoktorAdSoyad", "DoktorAdiSoyadi", "DoktorAd", "DoktorAdi", "Doktor");

        var durum = GetStringValue(dto,
            "Durum", "RandevuDurumu", "Status");

        var poliklinik = GetStringValue(dto,
            "PoliklinikAdi", "KlinikAdi", "BolumAdi", "Poliklinik");

        var not = GetStringValue(dto,
            "Not", "Notlar", "Aciklama", "RandevuNotu");

        var tarih = GetDateValue(dto,
            "Tarih", "RandevuTarihi", "BaslangicTarihi", "RandevuZamani", "Date");

        TitleLabel.Text = !string.IsNullOrWhiteSpace(hasta)
            ? hasta
            : $"Randevu #{id}";

        SubtitleLabel.Text = !string.IsNullOrWhiteSpace(doktor)
            ? $"Doktor: {doktor}"
            : "Randevu detay bilgileri";

        HastaLabel.Text = string.IsNullOrWhiteSpace(hasta)
            ? "Hasta bilgisi yok"
            : hasta;

        DoktorLabel.Text = string.IsNullOrWhiteSpace(doktor)
            ? "Doktor bilgisi yok"
            : doktor;

        TarihLabel.Text = tarih.HasValue
            ? tarih.Value.ToString("dd.MM.yyyy HH:mm")
            : "Tarih bilgisi yok";

        PoliklinikLabel.Text = string.IsNullOrWhiteSpace(poliklinik)
            ? "Birim bilgisi yok"
            : poliklinik;

        StatusBadgeLabel.Text = string.IsNullOrWhiteSpace(durum)
            ? "Durum bilgisi yok"
            : durum;

        NotLabel.Text = string.IsNullOrWhiteSpace(not)
            ? "Randevu notu bulunmuyor."
            : not;

        DetailContainer.IsVisible = true;
    }

    private async void OnRefreshClicked(object? sender, EventArgs e)
    {
        await LoadRandevuDetailAsync();
    }

    private async void OnCancelAppointmentClicked(object? sender, EventArgs e)
    {
        if (_randevuId <= 0)
        {
            await Snackbar.Make("İptal işlemi için geçerli randevu id bilgisi bulunamadı", duration: TimeSpan.FromSeconds(4)).Show(); return;
        }

        var confirm = await DisplayAlert(
            "Randevu İptali",
            "Bu randevuyu iptal etmek istediğinize emin misiniz?",
            "Evet",
            "Vazgeç");

        if (!confirm)
            return;

        SetLoading("Randevu iptal isteği backend servisine gönderiliyor...");

        var response = await _randevuService.DeleteAsync(_randevuId);

        if (!GetResponseSuccess(response))
        {
            SetError(GetResponseMessage(response, "Randevu iptal edilirken hata oluştu."));
            return;
        }

        await Toast.Make("Randevu başarıyla iptal edildi", ToastDuration.Short).Show(); await Shell.Current.GoToAsync("..");
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void SetLoading(string message)
    {
        StatusContainer.IsVisible = true;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        StatusLabel.Text = message;
    }

    private void SetReady(string message)
    {
        StatusContainer.IsVisible = true;
        LoadingIndicator.IsVisible = false;
        LoadingIndicator.IsRunning = false;
        StatusLabel.Text = message;
    }

    private void SetError(string message)
    {
        StatusContainer.IsVisible = true;
        LoadingIndicator.IsVisible = false;
        LoadingIndicator.IsRunning = false;
        StatusLabel.Text = message;
        DetailContainer.IsVisible = false;
    }

    private static bool GetResponseSuccess<T>(ApiResponse<T> response)
    {
        var successValue = GetPropertyValue(response,
            "Success", "IsSuccess", "Succeeded", "Basarili", "IsSuccessful");

        return successValue is bool value && value;
    }

    private static string GetResponseMessage<T>(ApiResponse<T> response, string fallback)
    {
        var messageValue = GetPropertyValue(response,
            "Message", "ErrorMessage", "Mesaj", "HataMesaji");

        return messageValue?.ToString() ?? fallback;
    }

    private static RandevuListDto? GetResponseData(ApiResponse<RandevuListDto> response)
    {
        var dataValue = GetPropertyValue(response,
            "Data", "Value", "Result", "Veri");

        return dataValue as RandevuListDto;
    }

    private static object? GetPropertyValue(object source, params string[] propertyNames)
    {
        var type = source.GetType();

        foreach (var propertyName in propertyNames)
        {
            var property = type.GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property != null)
                return property.GetValue(source);
        }

        return null;
    }

    private static string GetStringValue(object source, params string[] propertyNames)
    {
        var value = GetPropertyValue(source, propertyNames);
        return value?.ToString() ?? string.Empty;
    }

    private static int GetIntValue(object source, params string[] propertyNames)
    {
        var value = GetPropertyValue(source, propertyNames);

        if (value is int intValue)
            return intValue;

        return int.TryParse(value?.ToString(), out var parsed)
            ? parsed
            : 0;
    }

    private static DateTime? GetDateValue(object source, params string[] propertyNames)
    {
        var value = GetPropertyValue(source, propertyNames);

        if (value is DateTime dateTime)
            return dateTime;

        return DateTime.TryParse(value?.ToString(), out var parsed)
            ? parsed
            : null;
    }
}