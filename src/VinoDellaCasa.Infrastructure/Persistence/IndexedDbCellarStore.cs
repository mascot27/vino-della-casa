using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;
using VinoDellaCasa.Application.Abstractions;
using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Enums;

namespace VinoDellaCasa.Infrastructure.Persistence;

/// <summary>
/// Browser persistence via IndexedDB (JS interop). See Docs/architecture.md.
/// CI / unit tests use <see cref="InMemoryCellarStore"/> instead.
/// </summary>
public sealed class IndexedDbCellarStore : ICellarStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly IJSRuntime _js;

    public IndexedDbCellarStore(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<IReadOnlyList<Bottle>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var raw = await _js.InvokeAsync<JsonElement[]>("vinoCellarStore.getAll", cancellationToken);
        var list = raw.Select(Parse).OrderBy(b => b.Name).ToList();
        return list;
    }

    public async Task<Bottle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var raw = await _js.InvokeAsync<JsonElement>("vinoCellarStore.getById", cancellationToken, id.ToString());
        if (raw.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return Parse(raw);
    }

    public async Task<Bottle> AddAsync(Bottle bottle, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(bottle);
        if (bottle.Id == Guid.Empty)
        {
            bottle.Id = Guid.NewGuid();
        }

        var existing = await GetByIdAsync(bottle.Id, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException($"Bottle with id {bottle.Id} already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        bottle.CreatedAt = now;
        bottle.UpdatedAt = now;
        await PutAsync(bottle, cancellationToken);
        return bottle;
    }

    public async Task<Bottle> UpdateAsync(Bottle bottle, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(bottle);
        var existing = await GetByIdAsync(bottle.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Bottle with id {bottle.Id} was not found.");

        bottle.CreatedAt = existing.CreatedAt;
        bottle.UpdatedAt = DateTimeOffset.UtcNow;
        await PutAsync(bottle, cancellationToken);
        return bottle;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _js.InvokeAsync<bool>("vinoCellarStore.delete", cancellationToken, id.ToString());
    }

    private async Task PutAsync(Bottle bottle, CancellationToken cancellationToken)
    {
        var dto = ToDto(bottle);
        await _js.InvokeVoidAsync("vinoCellarStore.put", cancellationToken, dto);
    }

    private static object ToDto(Bottle b) => new
    {
        id = b.Id.ToString(),
        name = b.Name,
        producer = b.Producer,
        region = b.Region,
        country = b.Country,
        color = b.Color.ToString(),
        varietal = b.Varietal,
        vintage = b.Vintage,
        quantity = b.Quantity,
        priceChf = b.PriceChf,
        purchaseDate = b.PurchaseDate?.ToString("yyyy-MM-dd"),
        bin = b.Bin,
        status = b.Status.ToString(),
        notes = b.Notes,
        readyToDrink = b.ReadyToDrink,
        createdAt = b.CreatedAt.ToString("O"),
        updatedAt = b.UpdatedAt.ToString("O")
    };

    private static Bottle Parse(JsonElement el)
    {
        static string? Str(JsonElement e, string name) =>
            e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;

        static int? Int(JsonElement e, string name) =>
            e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number ? p.GetInt32() : null;

        static decimal? Dec(JsonElement e, string name) =>
            e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number ? p.GetDecimal() : null;

        static bool Bool(JsonElement e, string name) =>
            e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.True;

        var idStr = Str(el, "id") ?? throw new InvalidOperationException("IndexedDB bottle missing id.");
        DateOnly? purchase = null;
        var pd = Str(el, "purchaseDate");
        if (!string.IsNullOrWhiteSpace(pd) && DateOnly.TryParse(pd, out var d))
        {
            purchase = d;
        }

        return new Bottle
        {
            Id = Guid.Parse(idStr),
            Name = Str(el, "name") ?? string.Empty,
            Producer = Str(el, "producer"),
            Region = Str(el, "region"),
            Country = Str(el, "country"),
            Color = Enum.TryParse<Color>(Str(el, "color"), true, out var color) ? color : Color.Other,
            Varietal = Str(el, "varietal"),
            Vintage = Int(el, "vintage"),
            Quantity = Int(el, "quantity") ?? 0,
            PriceChf = Dec(el, "priceChf"),
            PurchaseDate = purchase,
            Bin = Str(el, "bin"),
            Status = Enum.TryParse<BottleStatus>(Str(el, "status"), true, out var status) ? status : BottleStatus.InStock,
            Notes = Str(el, "notes"),
            ReadyToDrink = Bool(el, "readyToDrink"),
            CreatedAt = DateTimeOffset.TryParse(Str(el, "createdAt"), out var c) ? c : DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.TryParse(Str(el, "updatedAt"), out var u) ? u : DateTimeOffset.UtcNow
        };
    }
}
