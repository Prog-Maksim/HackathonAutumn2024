namespace Xakaton2024.Models;

public class Requests
{
    public DateTime TicketDay { get; set; }
    public List<string> direction { get; set; }
    public string WagonType { get; set; }
    public int SeatsOfNumber { get; set; }
    public string shelf { get; set; }
}