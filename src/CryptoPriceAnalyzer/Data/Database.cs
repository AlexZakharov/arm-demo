using CryptoPriceAnalyzer.Options;
using Microsoft.Data.Sqlite;
using Dapper;
using Microsoft.Extensions.Options;

namespace CryptoPriceAnalyzer.Data;

public class Database(IOptions<DatabaseOptions> options)
{
    private readonly string _connectionString = options.Value.CryptoDb;

    public SqliteConnection Open() => new(_connectionString);

    public void EnsureCreated()
    {
        using var con = Open();
        con.Execute(@"
            CREATE TABLE IF NOT EXISTS Prices (
                Id TEXT PRIMARY KEY,
                Symbol TEXT NOT NULL,
                Source TEXT NOT NULL,
                Price REAL NOT NULL,
                TimestampUtc TEXT NOT NULL
            );
        ");
    }
}