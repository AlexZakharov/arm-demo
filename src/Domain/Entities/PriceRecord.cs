namespace Domain.Entities;

public class PriceRecord
{
    public Guid Id { get; set; }
    public string Symbol { get; set; }
    public string Source { get; set; }
    public double Price { get; set; }
    public DateTime TimestampUtc { get; set; }
}