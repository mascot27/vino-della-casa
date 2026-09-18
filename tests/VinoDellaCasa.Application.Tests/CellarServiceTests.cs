using VinoDellaCasa.Application.Services;
using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Enums;
using VinoDellaCasa.Infrastructure.Persistence;

namespace VinoDellaCasa.Application.Tests;

public class CellarServiceTests
{
    [Fact]
    public async Task Crud_RoundTrips_InMemoryStore()
    {
        var store = new InMemoryCellarStore();
        var svc = new CellarService(store);

        var added = await svc.AddAsync(new Bottle
        {
            Name = "Test Rouge",
            Color = Color.Red,
            Region = "Pomerol",
            Country = "France",
            Vintage = 2014,
            Quantity = 1,
            Notes = "MaturityStyle=PomerolMerlot"
        }, asOfYear: 2026);

        Assert.True(added.ReadyToDrink);
        Assert.NotEqual(Guid.Empty, added.Id);

        var all = await svc.GetAllAsync();
        Assert.Single(all);

        added.Quantity = 2;
        var updated = await svc.UpdateAsync(added, asOfYear: 2026);
        Assert.Equal(2, updated.Quantity);

        var deleted = await svc.DeleteAsync(added.Id);
        Assert.True(deleted);
        Assert.Empty(await svc.GetAllAsync());
    }

    [Fact]
    public async Task EnsureM1Seeds_LoadsDemoSample_AndDrinkTonightUsesMaturity()
    {
        var store = new InMemoryCellarStore();
        var svc = new CellarService(store);

        var n = await svc.EnsureM1SeedsAsync(asOfYear: 2026);
        Assert.Equal(53, n);
        Assert.Equal(0, await svc.EnsureM1SeedsAsync(asOfYear: 2026));

        var all = await svc.GetAllAsync();
        Assert.Equal(53, all.Count);
        Assert.True(all.Count(b => b.ReadyToDrink) >= 20);
        Assert.True(all.Count(b => !b.ReadyToDrink) >= 5);

        var tonight = await svc.GetDrinkTonightAsync(topN: 3, asOfYear: 2026);
        Assert.Equal(3, tonight.Count);
        Assert.All(tonight, b => Assert.True(b.ReadyToDrink));
    }
}
