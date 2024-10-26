using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Xakaton2024.Models;

namespace Xakaton2024.Controllers;

[ApiController]
[Route("api/info")]
public class TravelInfoController(IConnectionMultiplexer redis): ControllerBase
{
    private async Task<Wagon> GetData()
    {
        using var client = new HttpClient();
        
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJtYWtzaW1iZWwyMDE3MEBnbWFpbC5jb20iLCJpYXQiOjE3Mjk4ODMyNDAsImV4cCI6MTcyOTk2OTY0MH0.YhIyQmyCXhZ6CXHksKKCU6Ek5CQ89W17gDxC-3m39xQ");
        
        string url = "http://84.252.135.231/api/info/wagons?trainId=1";
        
        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
            
        string responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Wagon>(responseBody);
    }
    
    [HttpGet("wagons")]
    public async Task<IActionResult> GetInfoWagons()
    {
        try
        {
            return Ok(GetData());
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Request error: " + e.Message);
            return StatusCode(500);
        }
    }


    [HttpGet("search")]
    public async Task<IActionResult> SearchTicket(Requests data)
    {
        var result = await GetData();
        
        // data.
        // var data = result.Seats.Where(o => o.)
        
        return Ok();
    }
    
    public class Wagon
    {
        public int WagonId { get; set; }
        public string Type { get; set; }
        public List<Seat> Seats { get; set; }
    }

    public class Seat
    {
        public int SeatId { get; set; }
        public string SeatNum { get; set; }
        public string Block { get; set; }
        public decimal Price { get; set; }
        public string BookingStatus { get; set; }
    }
}