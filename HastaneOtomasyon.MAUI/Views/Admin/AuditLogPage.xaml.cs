using System.Collections.ObjectModel;
using System.Reflection;
using HastaneOtomasyon.MAUI.Services.Interfaces;
using HastaneOtomasyon.Shared.DTOs.Common;
using Microsoft.Extensions.DependencyInjection;

namespace HastaneOtomasyon.MAUI.Views.Admin;

public partial class AuditLogPage : ContentPage
{
    private readonly IAuditLogService _auditLogService;

    private readonly ObservableCollection<AuditLogCardItem> _allItems = new();
    private readonly ObservableCollection<AuditLogCardItem> _visibleItems = new();

    public AuditLogPage()
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);

        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("MAUI servis sağlayıcısı bulunamadı.");

        _auditLogService = services.GetService<IAuditLogService>()
            ?? throw new InvalidOperationException("IAuditLogService DI container içinde bulunamadı.");

        AuditLogCollectionView.ItemsSource = _visibleItems;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAuditLogsAsync();
    }

    private async Task LoadAuditLogsAsync()
    {
        SetLoading("Denetim kayıtları backend servisinden yükleniyor...");

        var response = await _auditLogService.GetAllAsync();

        if (!GetResponseSuccess(response))
        {
            SetError(GetResponseMessage(response, "Denetim kayıtları alınırken hata oluştu."));
            ClearItems();
            return;
        }

        var data = GetResponseData(response);

        LoadItems(data);
        SetReady(data.Count == 0 ? "Denetim kaydı bulunamadı." : $"{data.Count} denetim kaydı listelendi.");
    }

    private void LoadItems(List<AuditLogDto> data)
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

    private AuditLogCardItem MapToCard(AuditLogDto dto)
    {
        var id = GetIntValue(dto, "Id", "AuditLogId", "LogId");

        var kullanici = GetStringValue(dto,
            "KullaniciAdi", "UserName", "Username", "Kullanici", "User");

        var islem = GetStringValue(dto,
            "Islem", "İşlem", "Action", "Operation", "IslemTipi", "ActionType");

        var tablo = GetStringValue(dto,
            "TabloAdi", "TableName", "EntityName", "Tablo", "Entity");

        var aciklama = GetStringValue(dto,
            "Aciklama", "Description", "Detay", "Detail", "Mesaj", "Message");

        var tarih = GetDateValue(dto,
            "Tarih", "CreatedAt", "OlusturmaTarihi", "IslemTarihi", "Timestamp", "Date");

        var title = !string.IsNullOrWhiteSpace(islem)
            ? islem
            : $"Denetim Kaydı #{id}";

        var subtitleParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(kullanici))
            subtitleParts.Add($"Kullanıcı: {kullanici}");

        if (!string.IsNullOrWhiteSpace(aciklama))
            subtitleParts.Add(aciklama);

        var subtitle = subtitleParts.Count > 0
            ? string.Join(" · ", subtitleParts)
            : "Denetim kaydı detayı";

        var tableText = !string.IsNullOrWhiteSpace(tablo)
            ? tablo
            : "Tablo bilgisi yok";

        var dateText = tarih.HasValue
            ? tarih.Value.ToString("dd.MM.yyyy HH:mm")
            : "Tarih bilgisi yok";

        return new AuditLogCardItem
        {
            Id = id,
            IdText = id > 0 ? $"ID: {id}" : "ID yok",
            Title = title,
            Subtitle = subtitle,
            TableText = tableText,
            DateText = dateText,
            SearchText = $"{id} {kullanici} {islem} {tablo} {aciklama} {dateText}"
        };
    }

    private void ApplySearchFilter()
    {
        var query = AuditSearchEntry.Text?.Trim();

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
        await LoadAuditLogsAsync();
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
        AuditSearchEntry.Text = string.Empty;
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

    private static List<AuditLogDto> GetResponseData(ApiResponse<List<AuditLogDto>> response)
    {
        var dataValue = GetPropertyValue(response,
            "Data", "Value", "Result", "Veri");

        return dataValue as List<AuditLogDto> ?? new List<AuditLogDto>();
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

    private sealed class AuditLogCardItem
    {
        public int Id { get; set; }
        public string IdText { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string TableText { get; set; } = string.Empty;
        public string DateText { get; set; } = string.Empty;
        public string SearchText { get; set; } = string.Empty;
    }
}