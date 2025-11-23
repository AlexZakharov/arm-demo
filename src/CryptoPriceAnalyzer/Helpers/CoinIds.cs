namespace CryptoPriceAnalyzer.Helpers;

public static class CoinIds
{
    private static readonly Dictionary<string, string> _map =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "btc", "bitcoin" },
            { "eth", "ethereum" },
            { "doge", "dogecoin" },
            { "ltc", "litecoin" },
            { "sol", "solana" }
        };

    public static string Get(string symbol)
        => _map.TryGetValue(symbol, out var id)
            ? id
            : symbol.ToLower(); // fallback
}