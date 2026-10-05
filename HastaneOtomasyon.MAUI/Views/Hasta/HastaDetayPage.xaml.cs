using System.Reflection;
using HastaneOtomasyon.MAUI.Services.Interfaces;
using HastaneOtomasyon.Shared.DTOs.Common;
using HastaneOtomasyon.Shared.DTOs.Hasta;
using Microsoft.Extensions.DependencyInjection;

namespace HastaneOtomasyon.MAUI.Views.Hasta;

public partial class HastaDetayPage : ContentPage, IQueryAttributable
{
    private readonly IHastaService _hastaService;
    private int _hastaId;

    public HastaDetayPage()
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);

        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("MAUI servis sağlayıcısı bulunamadı.");

        _hastaService = services.GetService<IHastaService>()
            ?? throw new InvalidOperationException("IHastaService DI container içinde bulunamadı.");
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idValue) &&
            int.TryParse(idValue?.ToString(), out var parsedId))
        {
            _hastaId = parsedId;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_hastaId <= 0)
        {
            SetError("Hasta detayını görüntülemek için geçerli bir id bilgisi bulunamadı.");
            return;
        }

        await LoadHastaDetailAsync();
    }

    private async Task LoadHastaDetailAsync()
    {
        SetLoading("Hasta detayı backend servisinden yükleniyor...");
        DetailContainer.IsVisible = false;

        var response = await _hastaService.GetByIdAsync(_hastaId);

        if (!GetResponseSuccess(response))
        {
            SetError(GetResponseMessage(response, "Hasta detayı alınırken hata oluştu."));
            return;
        }

        var dto = GetResponseData(response);

        if (dto == null)
        {
            SetError("Backend servisinden hasta detayı boş döndü.");
            return;
        }

        BindHasta(dto);
        SetReady("Hasta detayı yüklendi.");
    }

    private void BindHasta(HastaDto dto)
    {
        var id = GetIntValue(dto, "Id", "HastaId");

        var adSoyad = GetStringValue(dto,
            "AdSoyad", "HastaAdSoyad", "AdSoyadBilgisi", "FullName");

        var ad = GetStringValue(dto, "Ad", "Adi", "HastaAdi", "FirstName");
        var soyad = GetStringValue(dto, "Soyad", "Soyadi", "HastaSoyadi", "LastName");

        if (string.IsNullOrWhiteSpace(adSoyad))
            adSoyad = $"{ad} {soyad}".Trim();

        if (string.IsNullOrWhiteSpace(adSoyad))
            adSoyad = $"Hasta #{id}";

        var tc = GetStringValue(dto,
            "TcKimlikNo", "TCKimlikNo", "TC", "Tc", "KimlikNo");

        var telefon = GetStringValue(dto,
            "Telefon", "TelefonNo", "Phone", "CepTelefonu");

        var email = GetStringValue(dto,
            "Email", "Eposta", "EPosta", "Mail");

        var adres = GetStringValue(dto,
            "Adres", "Address", "AcikAdres", "AçıkAdres", "Aciklama");

        var dogumTarihi = GetDateValue(dto,
            "DogumTarihi", "BirthDate", "DateOfBirth");

        var durum = GetStringValue(dto,
            "Durum", "Status", "HastaDurumu");

        AdSoyadLabel.Text = adSoyad;

        SubtitleLabel.Text = !string.IsNullOrWhiteSpace(tc)
            ? $"TC: {tc}"
            : "Hasta detay bilgileri";

        TcLabel.Text = string.IsNullOrWhiteSpace(tc)
            ? "TC bilgisi yok"
            : tc;

        TelefonLabel.Text = string.IsNullOrWhiteSpace(telefon)
            ? "Telefon bilgisi yok"
            : telefon;

        EmailLabel.Text = string.IsNullOrWhiteSpace(email)
            ? "E-posta bilgisi yok"
            : email;

        DogumTarihiLabel.Text = dogumTarihi.HasValue
            ? dogumTarihi.Value.ToString("dd.MM.yyyy")
            : "Doğum tarihi bilgisi yok";

        AdresLabel.Text = string.IsNullOrWhiteSpace(adres)
            ? "Adres veya açıklama bilgisi bulunmuyor."
            : adres;

        StatusBadgeLabel.Text = string.IsNullOrWhiteSpace(durum)
            ? "Aktif Kayıt"
            : durum;

        DetailContainer.IsVisible = true;
    }

    private async void OnRefreshClicked(object? sender, EventArgs e)
    {
        await LoadHastaDetailAsync();
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

    private static HastaDto? GetResponseData(ApiResponse<HastaDto> response)
    {
        var dataValue = GetPropertyValue(response,
            "Data", "Value", "Result", "Veri");

        return dataValue as HastaDto;
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