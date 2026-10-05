using System.Collections.ObjectModel;
using System.Reflection;
using HastaneOtomasyon.MAUI.Services.Interfaces;
using HastaneOtomasyon.Shared.DTOs.Common;
using HastaneOtomasyon.Shared.DTOs.Hasta;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace HastaneOtomasyon.MAUI.Views.Hasta;

public partial class HastaListPage : ContentPage
{
    private readonly IHastaService _hastaService;

    private readonly ObservableCollection<HastaCardItem> _allItems = new();
    private readonly ObservableCollection<HastaCardItem> _visibleItems = new();

    public HastaListPage()
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);

        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("MAUI servis sağlayıcısı bulunamadı.");

        _hastaService = services.GetService<IHastaService>()
            ?? throw new InvalidOperationException("IHastaService DI container içinde bulunamadı.");

        HastaCollectionView.ItemsSource = _visibleItems;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPatientsAsync();
    }

    private async Task LoadPatientsAsync()
    {
        SetLoading("Hasta kayıtları backend servisinden yükleniyor...");

        var response = await _hastaService.GetAllAsync();

        if (!GetResponseSuccess(response))
        {
            SetError(GetResponseMessage(response, "Hasta kayıtları alınırken hata oluştu."));
            ClearItems();
            return;
        }

        var data = GetResponseData(response);

        LoadItems(data);
        SetReady(data.Count == 0 ? "Hasta kaydı bulunamadı." : $"{data.Count} hasta listelendi.");
    }

    private void LoadItems(List<HastaDto> data)
    {
        _allItems.Clear();
        _visibleItems.Clear();

        foreach (var item in data)
        {
            var card = MapToCard(item);
            _allItems.Add(card);
            _visibleItems.Add(card);
        }

        ApplySearchFilter();
    }

    private void ClearItems()
    {
        _allItems.Clear();
        _visibleItems.Clear();
    }

    private HastaCardItem MapToCard(HastaDto dto)
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

        var dogumTarihi = GetDateValue(dto,
            "DogumTarihi", "BirthDate", "DateOfBirth");

        var subtitleParts = new List<string>();

        if (dogumTarihi.HasValue)
            subtitleParts.Add($"Doğum: {dogumTarihi.Value:dd.MM.yyyy}");

        if (!string.IsNullOrWhiteSpace(email))
            subtitleParts.Add(email);

        var subtitle = subtitleParts.Count > 0
            ? string.Join(" · ", subtitleParts)
            : "Hasta bilgileri";

        var contact = !string.IsNullOrWhiteSpace(telefon)
            ? telefon
            : !string.IsNullOrWhiteSpace(email)
                ? email
                : "İletişim bilgisi yok";

        var tcText = !string.IsNullOrWhiteSpace(tc)
            ? $"TC: {tc}"
            : "TC bilgisi yok";

        return new HastaCardItem
        {
            Id = id,
            FullName = adSoyad,
            Subtitle = subtitle,
            TcText = tcText,
            ContactText = contact,
            SearchText = $"{adSoyad} {tc} {telefon} {email} {subtitle}"
        };
    }

    private void ApplySearchFilter()
    {
        var query = HastaSearchEntry.Text?.Trim();

        _visibleItems.Clear();

        var source = string.IsNullOrWhiteSpace(query)
            ? _allItems
            : _allItems.Where(x =>
                x.SearchText.Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach (var item in source)
            _visibleItems.Add(item);
    }

    private async void OnRefreshClicked(object? sender, EventArgs e)
    {
        await LoadPatientsAsync();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        ApplySearchFilter();
    }

    private void OnClearSearchClicked(object? sender, EventArgs e)
    {
        HastaSearchEntry.Text = string.Empty;
        ApplySearchFilter();
    }

    private async void OnDetailClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is int id && id > 0)
        {
            await Shell.Current.GoToAsync($"{nameof(HastaDetayPage)}?id={id}");
            return;
        }

        await Snackbar.Make("Hasta detayına gitmek için geçerli bir kayıt ID bilgisi bulunamadı", duration: TimeSpan.FromSeconds(4)).Show();
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

    private static List<HastaDto> GetResponseData(ApiResponse<List<HastaDto>> response)
    {
        var dataValue = GetPropertyValue(response,
            "Data", "Value", "Result", "Veri");

        return dataValue as List<HastaDto> ?? new List<HastaDto>();
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

    private sealed class HastaCardItem
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string TcText { get; set; } = string.Empty;
        public string ContactText { get; set; } = string.Empty;
        public string SearchText { get; set; } = string.Empty;
    }
}