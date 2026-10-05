using System.Collections.ObjectModel;
using System.Reflection;
using HastaneOtomasyon.MAUI.Services.Interfaces;
using HastaneOtomasyon.Shared.DTOs.Common;
using HastaneOtomasyon.Shared.DTOs.Doktor;
using Microsoft.Extensions.DependencyInjection;

namespace HastaneOtomasyon.MAUI.Views.Doktor;

public partial class DoktorListPage : ContentPage
{
    private readonly IDoktorService _doktorService;

    private readonly ObservableCollection<DoktorCardItem> _allItems = new();
    private readonly ObservableCollection<DoktorCardItem> _visibleItems = new();

    public DoktorListPage()
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);

        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("MAUI servis sağlayıcısı bulunamadı.");

        _doktorService = services.GetService<IDoktorService>()
            ?? throw new InvalidOperationException("IDoktorService DI container içinde bulunamadı.");

        DoktorCollectionView.ItemsSource = _visibleItems;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDoctorsAsync();
    }

    private async Task LoadDoctorsAsync()
    {
        SetLoading("Doktor kayıtları backend servisinden yükleniyor...");

        var response = await _doktorService.GetAllAsync();

        if (!GetResponseSuccess(response))
        {
            SetError(GetResponseMessage(response, "Doktor kayıtları alınırken hata oluştu."));
            ClearItems();
            return;
        }

        var data = GetResponseData(response);

        LoadItems(data);
        SetReady(data.Count == 0 ? "Doktor kaydı bulunamadı." : $"{data.Count} doktor listelendi.");
    }

    private void LoadItems(List<DoktorDto> data)
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

    private DoktorCardItem MapToCard(DoktorDto dto)
    {
        var id = GetIntValue(dto, "Id", "DoktorId");

        var adSoyad = GetStringValue(dto,
            "AdSoyad", "DoktorAdSoyad", "AdSoyadBilgisi", "FullName");

        var ad = GetStringValue(dto, "Ad", "Adi", "DoktorAdi", "FirstName");
        var soyad = GetStringValue(dto, "Soyad", "Soyadi", "DoktorSoyadi", "LastName");

        if (string.IsNullOrWhiteSpace(adSoyad))
            adSoyad = $"{ad} {soyad}".Trim();

        if (string.IsNullOrWhiteSpace(adSoyad))
            adSoyad = $"Doktor #{id}";

        var uzmanlik = GetStringValue(dto,
            "UzmanlikAlani", "Uzmanlik", "Brans", "Branş", "Specialty");

        var poliklinik = GetStringValue(dto,
            "PoliklinikAdi", "KlinikAdi", "BolumAdi", "BirimAdi", "Department");

        var telefon = GetStringValue(dto,
            "Telefon", "TelefonNo", "Phone", "CepTelefonu");

        var email = GetStringValue(dto,
            "Email", "Eposta", "EPosta", "Mail");

        var departmentText = !string.IsNullOrWhiteSpace(poliklinik)
            ? poliklinik
            : !string.IsNullOrWhiteSpace(uzmanlik)
                ? uzmanlik
                : "Birim bilgisi yok";

        var subtitleParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(uzmanlik))
            subtitleParts.Add($"Uzmanlık: {uzmanlik}");

        if (!string.IsNullOrWhiteSpace(poliklinik))
            subtitleParts.Add($"Birim: {poliklinik}");

        var subtitle = subtitleParts.Count > 0
            ? string.Join(" · ", subtitleParts)
            : "Doktor bilgileri";

        var contact = !string.IsNullOrWhiteSpace(telefon)
            ? telefon
            : !string.IsNullOrWhiteSpace(email)
                ? email
                : "İletişim bilgisi yok";

        return new DoktorCardItem
        {
            Id = id,
            IdText = id > 0 ? $"ID: {id}" : "ID yok",
            FullName = adSoyad,
            Subtitle = subtitle,
            DepartmentText = departmentText,
            ContactText = contact,
            SearchText = $"{adSoyad} {uzmanlik} {poliklinik} {telefon} {email}"
        };
    }

    private void ApplySearchFilter()
    {
        var query = DoktorSearchEntry.Text?.Trim();

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
        await LoadDoctorsAsync();
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
        DoktorSearchEntry.Text = string.Empty;
        ApplySearchFilter();
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

    private static List<DoktorDto> GetResponseData(ApiResponse<List<DoktorDto>> response)
    {
        var dataValue = GetPropertyValue(response,
            "Data", "Value", "Result", "Veri");

        return dataValue as List<DoktorDto> ?? new List<DoktorDto>();
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

    private sealed class DoktorCardItem
    {
        public int Id { get; set; }
        public string IdText { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string DepartmentText { get; set; } = string.Empty;
        public string ContactText { get; set; } = string.Empty;
        public string SearchText { get; set; } = string.Empty;
    }
}