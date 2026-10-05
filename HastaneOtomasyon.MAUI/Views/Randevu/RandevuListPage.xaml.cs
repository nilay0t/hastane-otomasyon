using System.Collections.ObjectModel;
using System.Reflection;
using HastaneOtomasyon.MAUI.Services.Interfaces;
using HastaneOtomasyon.Shared.DTOs.Randevu;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
namespace HastaneOtomasyon.MAUI.Views.Randevu;

public partial class RandevuListPage : ContentPage
{
    private readonly IRandevuService _randevuService;

    private readonly ObservableCollection<RandevuCardItem> _allItems = new();
    private readonly ObservableCollection<RandevuCardItem> _visibleItems = new();

    public RandevuListPage()
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);

        _randevuService = Application.Current?.Handler?.MauiContext?.Services.GetService<IRandevuService>()
            ?? throw new InvalidOperationException("IRandevuService DI container içinde bulunamadı.");

        RandevuCollectionView.ItemsSource = _visibleItems;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAllRandevularAsync();
    }

    private async Task LoadAllRandevularAsync()
    {
        SetLoading("Randevular backend servisinden yükleniyor...");

        var response = await _randevuService.GetAllAsync();

        if (!GetResponseSuccess(response))
        {
            SetError(GetResponseMessage(response, "Randevular alınırken bir hata oluştu."));
            ClearItems();
            return;
        }

        var data = GetResponseData(response);

        LoadItems(data);
        SetReady(data.Count == 0 ? "Randevu kaydı bulunamadı." : $"{data.Count} randevu listelendi.");
    }

    private async Task LoadTodayRandevularAsync()
    {
        SetLoading("Bugünkü randevular backend servisinden yükleniyor...");

        var start = DateTime.Today;
        var end = DateTime.Today.AddDays(1).AddTicks(-1);

        var response = await _randevuService.GetByTarihAsync(start, end);

        if (!GetResponseSuccess(response))
        {
            SetError(GetResponseMessage(response, "Bugünkü randevular alınırken bir hata oluştu."));
            ClearItems();
            return;
        }

        var data = GetResponseData(response);

        LoadItems(data);
        SetReady(data.Count == 0 ? "Bugün için randevu kaydı bulunamadı." : $"Bugün için {data.Count} randevu listelendi.");
    }

    private void LoadItems(List<RandevuListDto> data)
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

    private void ApplySearchFilter()
    {
        var query = SearchEntry.Text?.Trim().ToLowerInvariant();

        _visibleItems.Clear();

        var source = string.IsNullOrWhiteSpace(query)
            ? _allItems
            : new ObservableCollection<RandevuCardItem>(
                _allItems.Where(x =>
                    x.SearchText.Contains(query, StringComparison.OrdinalIgnoreCase)));

        foreach (var item in source)
            _visibleItems.Add(item);
    }

    private RandevuCardItem MapToCard(RandevuListDto dto)
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

        var tarih = GetDateValue(dto,
            "Tarih", "RandevuTarihi", "BaslangicTarihi", "RandevuZamani", "Date");

        var saat = GetStringValue(dto,
            "Saat", "RandevuSaati", "Time");

        var title = !string.IsNullOrWhiteSpace(hasta)
            ? hasta
            : $"Randevu #{id}";

        var subtitleParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(doktor))
            subtitleParts.Add($"Doktor: {doktor}");

        if (!string.IsNullOrWhiteSpace(poliklinik))
            subtitleParts.Add($"Birim: {poliklinik}");

        var subtitle = subtitleParts.Count > 0
            ? string.Join(" · ", subtitleParts)
            : "Randevu detayı";

        var dateText = tarih.HasValue
            ? tarih.Value.ToString("dd.MM.yyyy HH:mm")
            : !string.IsNullOrWhiteSpace(saat)
                ? saat
                : "Tarih bilgisi yok";

        var statusText = string.IsNullOrWhiteSpace(durum)
            ? "Durum bilgisi yok"
            : durum;

        return new RandevuCardItem
        {
            Id = id,
            Title = title,
            Subtitle = subtitle,
            StatusText = statusText,
            DateText = dateText,
            SearchText = $"{title} {subtitle} {statusText} {dateText}"
        };
    }

    private static bool GetResponseSuccess(object response)
    {
        var successValue = GetPropertyValue(response,
            "Success", "IsSuccess", "Succeeded", "Basarili", "IsSuccessful");

        return successValue is bool value && value;
    }

    private static string GetResponseMessage(object response, string fallback)
    {
        var messageValue = GetPropertyValue(response,
            "Message", "ErrorMessage", "Mesaj", "HataMesaji");

        return messageValue?.ToString() ?? fallback;
    }

    private static List<RandevuListDto> GetResponseData(object response)
    {
        var dataValue = GetPropertyValue(response,
            "Data", "Value", "Result", "Veri");

        return dataValue as List<RandevuListDto> ?? new List<RandevuListDto>();
    }

    private static object? GetPropertyValue(object source, params string[] propertyNames)
    {
        var type = source.GetType();

        foreach (var propertyName in propertyNames)
        {
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

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

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadAllRandevularAsync();
    }

    private async void OnTodayClicked(object sender, EventArgs e)
    {
        await LoadTodayRandevularAsync();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplySearchFilter();
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RandevuOlusturPage));
    }

    private async void OnDetailClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is int id && id > 0)
        {
            await Shell.Current.GoToAsync($"{nameof(RandevuDetayPage)}?id={id}");
            return;
        }

        await Snackbar.Make("Randevu detayına gitmek için geçerli bir kayıt ID bilgisi bulunamadı", duration: TimeSpan.FromSeconds(4)).Show();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private sealed class RandevuCardItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public string DateText { get; set; } = string.Empty;
        public string SearchText { get; set; } = string.Empty;
    }
}