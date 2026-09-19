using System.Security.Cryptography;
using System.Text;

namespace VinoDellaCasa.Domain.Seed;

/// <summary>Stable Guid ids for string catalogue keys (e.g. bg-001) via UUID v5 / URL namespace.</summary>
internal static class CatalogSeedIds
{
    /// <summary>RFC 4122 URL namespace UUID.</summary>
    private static readonly Guid UrlNamespace = Guid.Parse("6ba7b811-9dad-11d1-80b4-00c04fd430c8");

    public static Guid FromKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return CreateUuidV5(UrlNamespace, $"vino-della-casa/catalog/{key.Trim()}");
    }

    private static Guid CreateUuidV5(Guid ns, string name)
    {
        var nsBytes = ns.ToByteArray();
        ToNetworkOrder(nsBytes);
        var nameBytes = Encoding.UTF8.GetBytes(name);
        var data = new byte[nsBytes.Length + nameBytes.Length];
        Buffer.BlockCopy(nsBytes, 0, data, 0, nsBytes.Length);
        Buffer.BlockCopy(nameBytes, 0, data, nsBytes.Length, nameBytes.Length);

        var hash = SHA1.HashData(data);
        var bytes = hash.AsSpan(0, 16).ToArray();
        bytes[6] = (byte)((bytes[6] & 0x0f) | 0x50); // version 5
        bytes[8] = (byte)((bytes[8] & 0x3f) | 0x80); // RFC 4122 variant
        ToNetworkOrder(bytes); // Guid ctor expects mixed-endian layout
        return new Guid(bytes);
    }

    /// <summary>Swap .NET Guid mixed-endian groups ↔ network (big-endian) order in place.</summary>
    private static void ToNetworkOrder(byte[] guid)
    {
        (guid[0], guid[3]) = (guid[3], guid[0]);
        (guid[1], guid[2]) = (guid[2], guid[1]);
        (guid[4], guid[5]) = (guid[5], guid[4]);
        (guid[6], guid[7]) = (guid[7], guid[6]);
    }
}
