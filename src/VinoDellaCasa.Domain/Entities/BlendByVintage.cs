namespace VinoDellaCasa.Domain.Entities;

/// <summary>One cépage share inside a vintage assemblage (facts only).</summary>
public sealed class BlendComponent
{
    /// <summary>Grape label as seeded (full name or Bordeaux abbrev CF/CS/PV).</summary>
    public required string Grape { get; set; }

    /// <summary>Share of the blend when known (e.g. 60 for 60%).</summary>
    public int? Percent { get; set; }
}

/// <summary>Assemblage breakdown for a single millésime.</summary>
public sealed class BlendByVintage
{
    public int Vintage { get; set; }

    public IReadOnlyList<BlendComponent> Components { get; set; } = [];
}
