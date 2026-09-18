using VinoDellaCasa.Application.Services;
using VinoDellaCasa.Domain.Enums;
using VinoDellaCasa.Domain.Seed;
using VinoDellaCasa.Infrastructure.Persistence;

namespace VinoDellaCasa.Application.Tests;

public class CatalogQuantityTests
{
    [Fact]
    public async Task SetCatalogQuantity_UpsertsAndRemoves()
    {
        var store = new InMemoryCellarStore();
        var svc = new CellarService(store);
        var entry = CatalogSeedData.CreateEntries(2026)
            .Single(e => e.Name == "Domaine de la Solitude");

        var added = await svc.SetCatalogQuantityAsync(entry, 2, asOfYear: 2026);
        Assert.NotNull(added);
        Assert.Equal(2, added!.Quantity);
        Assert.Equal(entry.Id, added.Id);
        Assert.Equal(BottleStatus.InStock, added.Status);

        var bumped = await svc.SetCatalogQuantityAsync(entry, 3, asOfYear: 2026);
        Assert.Equal(3, bumped!.Quantity);
        Assert.Single(await svc.GetAllAsync());

        var cleared = await svc.SetCatalogQuantityAsync(entry, 0, asOfYear: 2026);
        Assert.Null(cleared);
        Assert.Empty(await svc.GetAllAsync());
    }

    [Fact]
    public async Task SetCatalogQuantity_MatchesByNameVintage_WhenIdDiffers()
    {
        var store = new InMemoryCellarStore();
        var svc = new CellarService(store);
        var entry = CatalogSeedData.CreateEntries(2026)
            .Single(e => e.Name == "Minuty");

        // Pre-existing bottle with different Id but same name+vintage.
        await svc.AddAsync(new Domain.Entities.Bottle
        {
            Id = Guid.NewGuid(),
            Name = entry.Name,
            Color = entry.Color,
            Region = entry.Appellation,
            Country = entry.Country,
            Vintage = entry.Vintage,
            Quantity = 1,
            Status = BottleStatus.InStock
        }, asOfYear: 2026);

        var updated = await svc.SetCatalogQuantityAsync(entry, 4, asOfYear: 2026);
        Assert.NotNull(updated);
        Assert.Equal(4, updated!.Quantity);
        Assert.Single(await svc.GetAllAsync());
    }
}
