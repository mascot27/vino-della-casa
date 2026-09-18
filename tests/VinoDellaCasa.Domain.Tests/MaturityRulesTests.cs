using VinoDellaCasa.Domain.Maturity;

namespace VinoDellaCasa.Domain.Tests;

public class MaturityRulesTests
{
    [Theory]
    [InlineData(2018, MaturityStyle.PessacRougeAccessible, 2026, true)]
    [InlineData(2018, MaturityStyle.PessacRougeAccessible, 2023, false)]
    [InlineData(2018, MaturityStyle.PessacRougeAccessible, 2029, false)]
    [InlineData(2020, MaturityStyle.StEmilionPremierGcc, 2026, false)]
    [InlineData(2020, MaturityStyle.StEmilionPremierGcc, 2027, true)]
    [InlineData(2014, MaturityStyle.PomerolMerlot, 2026, true)]
    [InlineData(2019, MaturityStyle.PessacRougeClasse, 2026, false)]
    [InlineData(2019, MaturityStyle.MedocSecondVin, 2026, true)]
    [InlineData(null, MaturityStyle.RougeGenerique, 2026, false)]
    [InlineData(2024, MaturityStyle.RoseOuEffervescent, 2026, true)]
    [InlineData(2020, MaturityStyle.BlancSecSuisse, 2026, false)]
    [InlineData(2023, MaturityStyle.BlancSecSuisse, 2026, true)]
    public void IsReadyToDrink_MatchesMandatoryCases(int? vintage, MaturityStyle style, int asOfYear, bool expected)
    {
        var actual = MaturityRules.IsReadyToDrink(vintage, style, asOfYear);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(MaturityStyle.PessacRougeAccessible, 6, 10)]
    [InlineData(MaturityStyle.PessacRougeSecondVin, 6, 12)]
    [InlineData(MaturityStyle.PessacRougeClasse, 8, 16)]
    [InlineData(MaturityStyle.PomerolMerlot, 8, 18)]
    [InlineData(MaturityStyle.StEmilionPremierGcc, 7, 20)]
    [InlineData(MaturityStyle.StEmilionGcc, 6, 14)]
    [InlineData(MaturityStyle.MedocStEstephe, 8, 18)]
    [InlineData(MaturityStyle.MedocSecondVin, 5, 12)]
    [InlineData(MaturityStyle.BlancSecBordeaux, 3, 10)]
    [InlineData(MaturityStyle.BlancSecSuisse, 1, 5)]
    [InlineData(MaturityStyle.RougeSuisse, 2, 8)]
    [InlineData(MaturityStyle.RougeGenerique, 5, 12)]
    [InlineData(MaturityStyle.BlancGenerique, 1, 6)]
    [InlineData(MaturityStyle.RoseOuEffervescent, 0, 3)]
    public void WindowFor_MatchesTable(MaturityStyle style, int open, int close)
    {
        var w = MaturityRules.WindowFor(style);
        Assert.Equal(open, w.PeakOpenAge);
        Assert.Equal(close, w.PeakCloseAge);
    }
}
