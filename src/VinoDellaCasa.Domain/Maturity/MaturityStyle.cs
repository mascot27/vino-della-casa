namespace VinoDellaCasa.Domain.Maturity;

/// <summary>
/// Style buckets used to pick a deterministic ReadyToDrink age window.
/// See Docs/domain/M1-maturity-rules.md.
/// </summary>
public enum MaturityStyle
{
    PessacRougeAccessible,
    PessacRougeSecondVin,
    PessacRougeClasse,
    PomerolMerlot,
    StEmilionPremierGcc,
    StEmilionGcc,
    MedocStEstephe,
    MedocSecondVin,
    BlancSecBordeaux,
    BlancSecSuisse,
    RougeSuisse,
    RougeGenerique,
    BlancGenerique,
    RoseOuEffervescent
}
