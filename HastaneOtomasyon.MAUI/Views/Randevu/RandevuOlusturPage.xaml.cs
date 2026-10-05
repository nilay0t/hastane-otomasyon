using System.Reflection;
using HastaneOtomasyon.MAUI.Services.Interfaces;
using HastaneOtomasyon.Shared.DTOs.Common;
using HastaneOtomasyon.Shared.DTOs.Doktor;
using HastaneOtomasyon.Shared.DTOs.Hasta;
using HastaneOtomasyon.Shared.DTOs.Randevu;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace HastaneOtomasyon.MAUI.Views.Randevu;

public partial class RandevuOlusturPage : ContentPage
{
    private readonly IRandevuService _randevuService;
    private readonly IHastaService _hastaService;
    private readonly IDoktorService _doktorService;

    public RandevuOlusturPage()
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);

        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("MAUI servis sağlayıcısı bulunamadı.");

        _randevuService = services.GetService<IRandevuService>()
            ?? throw new InvalidOperationException("IRandevuService DI container içinde bulunamadı.");

        _hastaService = services.GetService<IHastaService>()
            ?? throw new InvalidOperationException("IHastaService DI container içinde bulunamadı.");

        _doktorService = services.GetService<IDoktorService>()
            ?? throw new InvalidOperationException("IDoktorService DI container içinde bulunamadı.");

        RandevuDatePicker.MinimumDate = DateTime.Today;
        RandevuDatePicker.Date = DateTime.Today;
        RandevuTimePicker.Time = new TimeSpan(9, 0, 0);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadBackendDataAsync();
    }

    private async Task LoadBackendDataAsync()
    {
        SetLoading("Hasta ve doktor bilgileri backend servisinden yükleniyor...");

        var hastaResponse = await _hastaService.GetAllAsync();
        var doktorResponse = await _doktorService.GetAllAsync();

        if (!GetResponseSuccess(hastaResponse))
        {
            SetError(GetResponseMessage(hastaResponse, "Hasta listesi alınırken hata oluştu."));
            return;
        }

        if (!GetResponseSuccess(doktorResponse))
        {
            SetError(GetResponseMessage(doktorResponse, "Doktor listesi alınırken hata oluştu."));
            return;
        }

        var hastalar = GetResponseData<HastaDto>(hastaResponse);
        var doktorlar = GetResponseData<DoktorDto>(doktorResponse);

        HastaPicker.ItemsSource = hastalar
            .Select(x => new PickerOption
            {
                Id = GetIntValue(x, "Id", "HastaId"),
                DisplayName = BuildHastaDisplayName(x)
            })
            .Where(x => x.Id > 0)
            .ToList();

        DoktorPicker.ItemsSource = doktorlar
            .Select(x => new PickerOption
            {
                Id = GetIntValue(x, "Id", "DoktorId"),
                DisplayName = BuildDoktorDisplayName(x)
            })
            .Where(x => x.Id > 0)
            .ToList();

        SetReady($"Backend verileri yüklendi. Hasta: {hastalar.Count}, Doktor: {doktorlar.Count}");
    }

    private async void OnCreateAppointmentClicked(object? sender, EventArgs e)
    {
        if (HastaPicker.SelectedItem is not PickerOption selectedHasta)
        {
            await Snackbar.Make("Lütfen hasta seçiniz.", duration: TimeSpan.FromSeconds(3)).Show();
            return;
        }

        if (DoktorPicker.SelectedItem is not PickerOption selectedDoktor)
        {
            await Snackbar.Make("Lütfen doktor seçiniz.", duration: TimeSpan.FromSeconds(3)).Show();
            return;
        }

        DateTime selectedDate = RandevuDatePicker.Date;
        TimeSpan selectedTime = RandevuTimePicker.Time;

        var selectedDateTime = selectedDate.Date + selectedTime;

        if (selectedDateTime < DateTime.Now)
        {
            await Snackbar.Make("Geçmiş tarih ve saat için randevu oluşturulamaz.", duration: TimeSpan.FromSeconds(4)).Show();
            return;
        }

        var dto = new RandevuCreateDto
        {
            HastaId = selectedHasta.Id,
            DoktorId = selectedDoktor.Id,
            RandevuTarihi = selectedDateTime
        };

        SetOptionalPropertyIfExists(dto, GetSelectedRandevuTuru(), "RandevuTuru", "Tur", "Tip");
        SetOptionalPropertyIfExists(dto, NotlarEditor.Text ?? string.Empty, "Not", "Notlar", "Aciklama", "RandevuNotu");

        SetLoading("Randevu backend servisine gönderiliyor...");

        var response = await _randevuService.CreateAsync(dto);

        if (!GetResponseSuccess(response))
        {
            SetError(GetResponseMessage(response, "Randevu oluşturulurken hata oluştu."));
            return;
        }

        SetReady("Randevu başarıyla oluşturuldu.");
        await Toast.Make("Randevu başarıyla oluşturuldu.", ToastDuration.Short).Show();
        await Shell.Current.GoToAsync("..");
    }

    private string GetSelectedRandevuTuru()
    {
        if (KontrolRadioButton.IsChecked)
            return "Kontrol";

        if (TakipRadioButton.IsChecked)
            return "Takip";

        return "Muayene";
    }

    private async void OnRefreshDataClicked(object? sender, EventArgs e)
    {
        await LoadBackendDataAsync();
    }

    private void OnHastaPickerSelectionChanged(object? sender, EventArgs e)
    {
        if (HastaPicker.SelectedItem is PickerOption opt)
            HastaPicker.Title = opt.DisplayName;
    }

    private void OnDoktorPickerSelectionChanged(object? sender, EventArgs e)
    {
        if (DoktorPicker.SelectedItem is PickerOption opt)
            DoktorPicker.Title = opt.DisplayName;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private static string BuildHastaDisplayName(HastaDto dto)
    {
        var adSoyad = GetStringValue(dto, "AdSoyad", "HastaAdSoyad", "Ad", "AdSoyadBilgisi");
        var tc = GetStringValue(dto, "TcKimlikNo", "TCKimlikNo", "TC", "Tc");

        if (!string.IsNullOrWhiteSpace(adSoyad) && !string.IsNullOrWhiteSpace(tc))
            return $"{adSoyad} - {tc}";

        if (!string.IsNullOrWhiteSpace(adSoyad))
            return adSoyad;

        return $"Hasta #{GetIntValue(dto, "Id", "HastaId")}";
    }

    private static string BuildDoktorDisplayName(DoktorDto dto)
    {
        var adSoyad = GetStringValue(dto, "AdSoyad", "DoktorAdSoyad", "Ad", "AdSoyadBilgisi");
        var poliklinik = GetStringValue(dto, "PoliklinikAdi", "KlinikAdi", "BolumAdi", "UzmanlikAlani");

        if (!string.IsNullOrWhiteSpace(adSoyad) && !string.IsNullOrWhiteSpace(poliklinik))
            return $"{adSoyad} - {poliklinik}";

        if (!string.IsNullOrWhiteSpace(adSoyad))
            return adSoyad;

        return $"Doktor #{GetIntValue(dto, "Id", "DoktorId")}";
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

    private static List<T> GetResponseData<T>(ApiResponse<List<T>> response)
    {
        var dataValue = GetPropertyValue(response,
            "Data", "Value", "Result", "Veri");

        return dataValue as List<T> ?? new List<T>();
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

    private static void SetOptionalPropertyIfExists<TValue>(object target, TValue value, params string[] propertyNames)
    {
        var type = target.GetType();

        foreach (var propertyName in propertyNames)
        {
            var property = type.GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property == null || !property.CanWrite)
                continue;

            try
            {
                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                if (targetType.IsEnum)
                {
                    var enumValue = Enum.Parse(targetType, value?.ToString() ?? string.Empty, true);
                    property.SetValue(target, enumValue);
                    return;
                }

                var convertedValue = Convert.ChangeType(value, targetType);
                property.SetValue(target, convertedValue);
                return;
            }
            catch
            {
            }
        }
    }

    private sealed class PickerOption
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;

        public override string ToString() => DisplayName;
    }
}