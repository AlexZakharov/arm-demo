using System.Net;
using System.Web;
using CryptoPriceAnalyzer.Data;
using CryptoPriceAnalyzer.Services;
using Dapper;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CryptoPriceAnalyzer.Functions;

public class FetchPricesFunction(PriceProviders providers)
{
    [Function("FetchPrices")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "GET")] HttpRequestData req)
    {
        var query = HttpUtility.ParseQueryString(req.Url.Query);

        var symbol = (query["symbol"] ?? "btc").Trim().ToLower();

        int sourcesCount = int.TryParse(query["sources"], out var sc) ? sc : 3;
        int take = int.TryParse(query["take"], out var t) ? t : 10;

        if (sourcesCount < 1)
            sourcesCount = 1;

        var providers1 = providers.AllProviders.Take(sourcesCount).ToList();

        // --- external calls ---
        List<PriceResult> results;

        try
        {
            var tasks = providers1.Select(p => p(symbol)).ToList();
            var fetched = await Task.WhenAll(tasks);

            results = fetched
                .Where(x => x.Price > 0)
                .OrderByDescending(x => x.Price)
                .ToList();
        }
        catch (Exception ex)
        {
            var error = req.CreateResponse(HttpStatusCode.BadRequest);
            await error.WriteAsJsonAsync(new { error = ex.Message });
            return error;
        }

        // --- save to DB ASC ---
        foreach (var r in results)
        {
            await using var con = Db.Open();
            await con.ExecuteAsync(@"
                INSERT INTO Prices (Id, Symbol, Source, Price, TimestampUtc)
                VALUES (@Id, @Symbol, @Source, @Price, @TimestampUtc)",
                new
                {
                    Id = Guid.NewGuid().ToString(),
                    Symbol = symbol,
                    Source = r.Source,
                    Price = r.Price,
                    TimestampUtc = DateTime.UtcNow
                });
        }

        // --- read last N ---
        IEnumerable<dynamic> lastRows;

        await using (var con = Db.Open())
        {
            lastRows = con.Query(@"
                SELECT Id, Symbol, Source, Price, TimestampUtc
                FROM Prices
                WHERE Symbol = @Symbol
                ORDER BY TimestampUtc DESC
                LIMIT @Take",
                new { Symbol = symbol, Take = take });
        }

        // --- output ---
        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(new
        {
            symbol,
            calls = results,
            saved = results.Count,
            last = lastRows
        });

        return response;
    }
}