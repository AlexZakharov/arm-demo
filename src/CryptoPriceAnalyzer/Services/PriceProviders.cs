using System.Net.Http.Json;
using System.Text.Json;
using CryptoPriceAnalyzer.Helpers;
using Microsoft.Extensions.Options;

namespace CryptoPriceAnalyzer.Services;

public class PriceProviders(IOptions<ProviderOptions> options)
{
    private readonly ProviderOptions _options = options.Value;
    private static readonly HttpClient HttpClient = new();

    public async Task<PriceResult> Binance(string symbol)
    {
        var s = symbol.ToUpper();
        var url = $"{_options.Binance}?symbol={s}USDT";
        var json = await HttpClient.GetStringAsync(url);
        using var doc = JsonDocument.Parse(json);
        var price = double.Parse(doc.RootElement.GetProperty("price").GetString()!);
        return new PriceResult { Source = "binance", Price = price };
    }

    public async Task<PriceResult> Kraken(string symbol)
    {
        var s = symbol.ToUpper();
        var url = $"{_options.Kraken}?pair={s}USD";
        var json = await HttpClient.GetStringAsync(url);
        using var doc = JsonDocument.Parse(json);
        var value = doc.RootElement.GetProperty("result").EnumerateObject().First().Value;
        var price = double.Parse(value.GetProperty("c")[0].GetString()!);
        return new PriceResult { Source = "kraken", Price = price };
    }

    public async Task<PriceResult> CoinGecko(string symbol)
    {
        var result = new PriceResult { Source = "coingecko", Price = -1 };

        try
        {
            var id = CoinIds.Get(symbol);
            var url = $"{_options.CoinGecko}?ids={id}&vs_currencies=usd";

            var data = await HttpClient
                .GetFromJsonAsync<Dictionary<string, Dictionary<string, double>>>(url);

            if (data is not null &&
                data.TryGetValue(id, out var prices) &&
                prices.TryGetValue("usd", out var price))
            {
                result.Price = price;
            }
        }
        catch
        {
            // swallow -> result.Price stays -1
        }

        return result;
    }

    public List<Func<string, Task<PriceResult>>> AllProviders =>
        [Binance, Kraken, CoinGecko];
}