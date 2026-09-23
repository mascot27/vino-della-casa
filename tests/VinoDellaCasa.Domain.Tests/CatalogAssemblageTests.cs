using VinoDellaCasa.Domain.Entities;
using VinoDellaCasa.Domain.Seed;

namespace VinoDellaCasa.Domain.Tests;

public class CatalogAssemblageTests
{
    [Fact]
    public void FormatChip_IncludesPercentWhenPresent()
    {
        var chip = CatalogAssemblage.FormatChip(new BlendComponent { Grape = "Merlot", Percent = 60 });
        Assert.Equal("Merlot 60%", chip);
    }

    [Fact]
    public void FormatChip_AllowsBordeauxAbbreviations()
    {
        Assert.Equal("CF 25%", CatalogAssemblage.FormatChip(new BlendComponent { Grape = "CF", Percent = 25 }));
        Assert.Equal("CS 15%", CatalogAssemblage.FormatChip(new BlendComponent { Grape = "CS", Percent = 15 }));
        Assert.Equal("PV 5%", CatalogAssemblage.FormatChip(new BlendComponent { Grape = "PV", Percent = 5 }));
    }

    [Fact]
    public void FormatChip_KeepsBourgogneFullNames()
    {
        Assert.Equal(
            "Pinot Noir 100%",
            CatalogAssemblage.FormatChip(new BlendComponent { Grape = "Pinot Noir", Percent = 100 }));
        Assert.Equal(
            "Chardonnay",
            CatalogAssemblage.FormatChip(new BlendComponent { Grape = "Chardonnay", Percent = null }));
    }

    [Fact]
    public void SanitizeText_StripsHtml()
    {
        Assert.Equal("Merlot", CatalogAssemblage.SanitizeText(" <b>Merlot</b> "));
        Assert.Equal("CF", CatalogAssemblage.SanitizeText("<script>CF</script>"));
        Assert.Equal(string.Empty, CatalogAssemblage.SanitizeText("   "));
    }

    [Fact]
    public void PreferForFiche_MatchesFicheVintage()
    {
        var blends = new List<BlendByVintage>
        {
            new()
            {
                Vintage = 2018,
                Components = [new BlendComponent { Grape = "Merlot", Percent = 70 }]
            },
            new()
            {
                Vintage = 2019,
                Components =
                [
                    new BlendComponent { Grape = "Merlot", Percent = 60 },
                    new BlendComponent { Grape = "CF", Percent = 25 },
                    new BlendComponent { Grape = "CS", Percent = 15 }
                ]
            }
        };

        var preferred = CatalogAssemblage.PreferForFiche(blends, ficheVintage: 2019);
        Assert.NotNull(preferred);
        Assert.Equal(2019, preferred!.Vintage);
        Assert.Equal(3, preferred.Components.Count);
        Assert.Equal("Merlot 60%", CatalogAssemblage.FormatChip(preferred.Components[0]));
    }

    [Fact]
    public void PreferForFiche_SingleVintage_UsedWhenFicheDiffers()
    {
        var blends = new List<BlendByVintage>
        {
            new()
            {
                Vintage = 2019,
                Components = [new BlendComponent { Grape = "Chardonnay", Percent = 100 }]
            }
        };

        var preferred = CatalogAssemblage.PreferForFiche(blends, ficheVintage: 2020);
        Assert.NotNull(preferred);
        Assert.Equal(2019, preferred!.Vintage);
    }

    [Fact]
    public void PreferForFiche_MultiWithoutMatch_ReturnsNull_ForFallback()
    {
        var blends = new List<BlendByVintage>
        {
            new()
            {
                Vintage = 2018,
                Components = [new BlendComponent { Grape = "Merlot", Percent = 70 }]
            },
            new()
            {
                Vintage = 2019,
                Components = [new BlendComponent { Grape = "Merlot", Percent = 60 }]
            }
        };

        Assert.Null(CatalogAssemblage.PreferForFiche(blends, ficheVintage: 2021));
        Assert.Null(CatalogAssemblage.PreferForFiche(null, 2019));
        Assert.Null(CatalogAssemblage.PreferForFiche([], 2019));
    }

    [Fact]
    public void MapFromSeed_DropsEmptyGrapes()
    {
        var mapped = CatalogAssemblage.MapFromSeed(
        [
            new BlendByVintage
            {
                Vintage = 2019,
                Components =
                [
                    new BlendComponent { Grape = " <i>Merlot</i> ", Percent = 60 },
                    new BlendComponent { Grape = "  ", Percent = 10 },
                    new BlendComponent { Grape = "CF", Percent = 40 }
                ]
            }
        ]);

        Assert.Single(mapped);
        Assert.Equal(2, mapped[0].Components.Count);
        Assert.Equal("Merlot", mapped[0].Components[0].Grape);
        Assert.Equal("CF", mapped[0].Components[1].Grape);
    }

    [Fact]
    public void ListForFiche_ReturnsAllVintages_MatchFirstThenDesc()
    {
        var blends = new List<BlendByVintage>
        {
            new()
            {
                Vintage = 2018,
                Components = [new BlendComponent { Grape = "Merlot", Percent = 70 }]
            },
            new()
            {
                Vintage = 2019,
                Components =
                [
                    new BlendComponent { Grape = "Merlot", Percent = 60 },
                    new BlendComponent { Grape = "CF", Percent = 40 }
                ]
            },
            new()
            {
                Vintage = 2020,
                Components = [new BlendComponent { Grape = "Merlot", Percent = 55 }]
            }
        };

        var rows = CatalogAssemblage.ListForFiche(blends, ficheVintage: 2019);
        Assert.Equal(3, rows.Count);
        Assert.Equal(2019, rows[0].Vintage);
        Assert.Equal(2020, rows[1].Vintage);
        Assert.Equal(2018, rows[2].Vintage);
    }

    [Fact]
    public void ListForFiche_EmptyWhenAbsent()
    {
        Assert.Empty(CatalogAssemblage.ListForFiche(null, 2019));
        Assert.Empty(CatalogAssemblage.ListForFiche([], 2019));
        Assert.Empty(CatalogAssemblage.ListForFiche(
        [
            new BlendByVintage { Vintage = 2019, Components = [] }
        ], 2019));
    }
}
