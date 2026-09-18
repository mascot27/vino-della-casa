using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Enums;
using VinoDellaCasa.Domain.Maturity;
using VinoDellaCasa.Domain.Seed;
using VinoDellaCasa.Domain.Suggestions;
using VinoDellaCasa.Mcp.Labels;
using VinoDellaCasa.Mcp.Models;

namespace VinoDellaCasa.Mcp.Handlers;

/// <summary>
/// Read-only MCP tool handlers. Local compute only — no network, no FS writes, no cellar persistence.
/// </summary>
public static class ToolHandlers
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        WriteIndented = false
    };

    public static string EvaluateMaturity(EvaluateMaturityInput? input)
    {
        input ??= new EvaluateMaturityInput();

        if (!TryParseColor(input.Color, required: true, out var color, out var colorError))
        {
            return Err("invalid_color", colorError!);
        }

        if (!TryClampAsOfYear(input.AsOfYear, out var asOfYear, out var yearError))
        {
            return Err("invalid_as_of_year", yearError!);
        }

        if (input.Vintage is { } v && (v < ToolLimits.MinVintage || v > ToolLimits.MaxVintage))
        {
            return Err("invalid_vintage", $"vintage must be between {ToolLimits.MinVintage} and {ToolLimits.MaxVintage}.");
        }

        var name = ClampString(input.Name, ToolLimits.MaxStringLength) ?? "Bottle";
        var region = ClampString(input.Region, ToolLimits.MaxStringLength);
        var country = ClampString(input.Country, ToolLimits.MaxStringLength);
        var notes = ClampString(input.Notes, ToolLimits.MaxNotesLength);

        MaturityStyle style;
        if (!string.IsNullOrWhiteSpace(input.MaturityStyle))
        {
            if (!Enum.TryParse<MaturityStyle>(input.MaturityStyle.Trim(), ignoreCase: true, out style))
            {
                return Err("invalid_maturity_style", $"Unknown maturityStyle '{input.MaturityStyle}'.");
            }
        }
        else
        {
            var bottleForStyle = new Bottle
            {
                Name = name,
                Region = region,
                Country = country,
                Color = color,
                Notes = notes,
                Vintage = input.Vintage
            };
            style = MaturityRules.ResolveStyleOrFromNotes(bottleForStyle);
        }

        var window = MaturityRules.WindowFor(style);
        var ready = MaturityRules.IsReadyToDrink(input.Vintage, style, asOfYear);
        int? ageYears = input.Vintage is null ? null : asOfYear - input.Vintage.Value;

        string labelFr;
        string whyFr;
        if (input.Vintage is null)
        {
            labelFr = FrLabels.NoVintage;
            whyFr = FrLabels.NoVintage;
        }
        else if (ready)
        {
            labelFr = FrLabels.Ready;
            whyFr = $"{FrLabels.Ready} · millésime {input.Vintage} dans la fenêtre {window.PeakOpenAge}–{window.PeakCloseAge} ans";
        }
        else
        {
            labelFr = FrLabels.Wait;
            whyFr = $"{FrLabels.Wait} · millésime {input.Vintage} hors fenêtre {window.PeakOpenAge}–{window.PeakCloseAge} ans";
        }

        return Ok(new
        {
            readyToDrink = ready,
            labelFr,
            maturityStyle = style.ToString(),
            window = new { peakOpenAge = window.PeakOpenAge, peakCloseAge = window.PeakCloseAge },
            ageYears,
            whyFr
        });
    }

    public static string RankDrinkTonight(RankDrinkTonightInput? input)
    {
        input ??= new RankDrinkTonightInput();
        var bottlesIn = input.Bottles ?? [];

        if (bottlesIn.Count > ToolLimits.MaxBottles)
        {
            return Err("too_many_bottles", $"bottles length must be <= {ToolLimits.MaxBottles}.");
        }

        if (!TryClampAsOfYear(input.AsOfYear, out var asOfYear, out var yearError))
        {
            return Err("invalid_as_of_year", yearError!);
        }

        var topN = input.TopN ?? ToolLimits.DefaultTopN;
        if (topN < 1 || topN > ToolLimits.MaxTopN)
        {
            return Err("invalid_top_n", $"topN must be between 1 and {ToolLimits.MaxTopN}.");
        }

        Color? colorFilter = null;
        if (!string.IsNullOrWhiteSpace(input.ColorFilter))
        {
            if (!TryParseColor(input.ColorFilter, required: true, out var cf, out var cfError))
            {
                return Err("invalid_color_filter", cfError!);
            }

            colorFilter = cf;
        }

        var recalculate = input.RecalculateReady ?? true;
        var domainBottles = new List<(Bottle Bottle, string Id)>(bottlesIn.Count);

        foreach (var b in bottlesIn)
        {
            if (!TryParseColor(b.Color, required: true, out var color, out var colorError))
            {
                return Err("invalid_color", colorError!);
            }

            if (!TryParseStatus(b.Status, out var status, out var statusError))
            {
                return Err("invalid_status", statusError!);
            }

            if (b.Vintage is { } v && (v < ToolLimits.MinVintage || v > ToolLimits.MaxVintage))
            {
                return Err("invalid_vintage", $"vintage must be between {ToolLimits.MinVintage} and {ToolLimits.MaxVintage}.");
            }

            var qty = b.Quantity ?? 0;
            if (qty < 0 || qty > 10_000)
            {
                return Err("invalid_quantity", "quantity must be between 0 and 10000.");
            }

            var bottle = new Bottle
            {
                Name = ClampString(b.Name, ToolLimits.MaxStringLength) ?? "Bottle",
                Region = ClampString(b.Region, ToolLimits.MaxStringLength),
                Country = ClampString(b.Country, ToolLimits.MaxStringLength),
                Notes = ClampString(b.Notes, ToolLimits.MaxNotesLength),
                Color = color,
                Vintage = b.Vintage,
                Quantity = qty,
                Status = status,
                ReadyToDrink = b.ReadyToDrink ?? false
            };

            if (recalculate)
            {
                MaturityRules.ApplyReadyToDrink(bottle, asOfYear);
            }

            var id = string.IsNullOrWhiteSpace(b.Id) ? bottle.Id.ToString() : ClampString(b.Id, 80)!;
            domainBottles.Add((bottle, id));
        }

        if (domainBottles.Count == 0)
        {
            return Ok(new
            {
                items = Array.Empty<object>(),
                emptyReasonFr = FrLabels.EmptyCellar
            });
        }

        var eligible = domainBottles
            .Where(x => x.Bottle.Status == BottleStatus.InStock
                        && x.Bottle.Quantity > 0
                        && x.Bottle.ReadyToDrink
                        && (colorFilter is null || x.Bottle.Color == colorFilter.Value))
            .OrderBy(x => x.Bottle.Vintage ?? int.MaxValue)
            .ThenBy(x => x.Bottle.Name, StringComparer.OrdinalIgnoreCase)
            .Take(topN)
            .ToList();

        if (eligible.Count == 0)
        {
            var anyInStock = domainBottles.Any(x =>
                x.Bottle.Status == BottleStatus.InStock && x.Bottle.Quantity > 0);
            var emptyReason = colorFilter is not null
                ? FrLabels.EmptyFilter
                : anyInStock
                    ? FrLabels.EmptyNoneReady
                    : FrLabels.EmptyCellar;

            return Ok(new
            {
                items = Array.Empty<object>(),
                emptyReasonFr = emptyReason
            });
        }

        var oldestVintage = eligible.Min(x => x.Bottle.Vintage ?? int.MaxValue);
        var items = new List<object>(eligible.Count);
        for (var i = 0; i < eligible.Count; i++)
        {
            var (bottle, id) = eligible[i];
            items.Add(new
            {
                id,
                name = bottle.Name,
                vintage = bottle.Vintage,
                region = bottle.Region,
                color = bottle.Color.ToString(),
                colorLabelFr = FrLabels.ColorLabel(bottle.Color),
                readyLabelFr = FrLabels.Ready,
                whyFr = BuildWhyFr(bottle, oldestVintage, colorFilter is not null),
                rank = i + 1
            });
        }

        return Ok(new { items, emptyReasonFr = (string?)null });
    }

    public static string ListSeedBottles(ListSeedBottlesInput? input)
    {
        input ??= new ListSeedBottlesInput();
        if (!TryClampAsOfYear(input.AsOfYear ?? 2026, out var asOfYear, out var yearError))
        {
            return Err("invalid_as_of_year", yearError!);
        }

        var readyOnly = input.IncludeReadyOnly ?? false;
        var seeds = DemoSample50SeedData.CreateSeeds(asOfYear);
        var filtered = readyOnly ? seeds.Where(b => b.ReadyToDrink).ToList() : seeds.ToList();
        var readyCount = seeds.Count(b => b.ReadyToDrink);

        var bottles = filtered.Select(b => new
        {
            id = b.Id.ToString(),
            name = b.Name,
            producer = b.Producer,
            region = b.Region,
            country = b.Country,
            color = b.Color.ToString(),
            colorLabelFr = FrLabels.ColorLabel(b.Color),
            varietal = b.Varietal,
            vintage = b.Vintage,
            quantity = b.Quantity,
            priceChf = b.PriceChf,
            bin = b.Bin,
            status = b.Status.ToString(),
            notes = b.Notes,
            readyToDrink = b.ReadyToDrink
        }).ToList();

        return Ok(new
        {
            count = bottles.Count,
            readyCount,
            bottles
        });
    }

    public static string SuggestVarietalRegion(SuggestVarietalRegionInput? input)
    {
        input ??= new SuggestVarietalRegionInput();
        var field = (input.Field ?? string.Empty).Trim().ToLowerInvariant();
        IReadOnlyList<string> source = field switch
        {
            "varietal" => FieldSuggestions.Varietal,
            "region" => FieldSuggestions.Region,
            "country" => FieldSuggestions.Country,
            _ => Array.Empty<string>()
        };

        if (source.Count == 0)
        {
            return Err("invalid_field", "field must be one of: varietal, region, country.");
        }

        var limit = input.Limit ?? ToolLimits.DefaultSuggestionLimit;
        if (limit < 1 || limit > ToolLimits.MaxSuggestionLimit)
        {
            return Err("invalid_limit", $"limit must be between 1 and {ToolLimits.MaxSuggestionLimit}.");
        }

        var query = ClampString(input.Query, ToolLimits.MaxStringLength) ?? string.Empty;
        var suggestions = FieldSuggestions.Filter(source, query, limit).ToList();

        return Ok(new
        {
            field,
            suggestions
        });
    }

    public static string GetColorLabels()
    {
        var colors = Enum.GetValues<Color>()
            .Select(c => new
            {
                value = c.ToString(),
                labelFr = FrLabels.ColorLabel(c),
                hex = FrLabels.ColorHex(c)
            })
            .ToList();

        var maturityBadges = new[]
        {
            new { key = "ready", labelFr = FrLabels.Ready, hex = FrLabels.ReadyHex },
            new { key = "wait", labelFr = FrLabels.Wait, hex = FrLabels.WaitHex }
        };

        return Ok(new { colors, maturityBadges });
    }

    private static string BuildWhyFr(Bottle bottle, int oldestVintage, bool colorFiltered)
    {
        if (bottle.Vintage is { } y && y == oldestVintage)
        {
            return $"{FrLabels.Ready} · millésime {y} (plus ancien en cave)";
        }

        if (colorFiltered)
        {
            return $"{FrLabels.Ready} · {FrLabels.ColorLabel(bottle.Color)}";
        }

        if (!string.IsNullOrWhiteSpace(bottle.Region))
        {
            return $"{FrLabels.Ready} · {bottle.Region}";
        }

        return $"{FrLabels.Ready} à boire · en stock";
    }

    private static bool TryClampAsOfYear(int? asOfYear, out int year, out string? error)
    {
        year = asOfYear ?? DateTimeOffset.UtcNow.Year;
        // Prefer Europe/Zurich calendar year when host provides local time via DateTimeOffset.Now
        if (asOfYear is null)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Zurich");
                year = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz).Year;
            }
            catch (TimeZoneNotFoundException)
            {
                year = DateTimeOffset.UtcNow.Year;
            }
        }

        if (year < ToolLimits.MinAsOfYear || year > ToolLimits.MaxAsOfYear)
        {
            error = $"asOfYear must be between {ToolLimits.MinAsOfYear} and {ToolLimits.MaxAsOfYear}.";
            return false;
        }

        error = null;
        return true;
    }

    private static bool TryParseColor(string? value, bool required, out Color color, out string? error)
    {
        color = Color.Other;
        if (string.IsNullOrWhiteSpace(value))
        {
            if (required)
            {
                error = "color is required (Red|White|Rose|Sparkling|Other).";
                return false;
            }

            error = null;
            return true;
        }

        if (!Enum.TryParse(value.Trim(), ignoreCase: true, out color)
            || !Enum.IsDefined(color))
        {
            error = $"color must be one of: Red, White, Rose, Sparkling, Other (got '{value}').";
            return false;
        }

        error = null;
        return true;
    }

    private static bool TryParseStatus(string? value, out BottleStatus status, out string? error)
    {
        status = BottleStatus.InStock;
        if (string.IsNullOrWhiteSpace(value))
        {
            error = null;
            return true;
        }

        if (!Enum.TryParse(value.Trim(), ignoreCase: true, out status)
            || !Enum.IsDefined(status))
        {
            error = $"status must be one of: InStock, Drinking, Finished (got '{value}').";
            return false;
        }

        error = null;
        return true;
    }

    private static string? ClampString(string? value, int max)
    {
        if (value is null)
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }

    private static string Ok(object payload)
    {
        var node = JsonSerializer.SerializeToNode(payload, JsonOptions)!.AsObject();
        // Ensure ok is present; clients treat missing ok as failure.
        node["ok"] = true;
        return node.ToJsonString(JsonOptions);
    }

    private static string Err(string code, string message) =>
        JsonSerializer.Serialize(new ErrorResult { Error = code, Message = message }, JsonOptions);
}
