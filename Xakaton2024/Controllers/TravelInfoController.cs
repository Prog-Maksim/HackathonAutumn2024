using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Xakaton2024.Controllers;

[ApiController]
[Route("api/info")]
public class TravelInfoController(IConnectionMultiplexer redis): ControllerBase
{
    [HttpGet("wagons")]
    public async Task<IActionResult> GetInfoWagons()
    {
        using var client = new HttpClient();

        // Установка заголовков
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJtYWtzaW1iZWwyMDE3MEBnbWFpbC5jb20iLCJpYXQiOjE3Mjk4ODMyNDAsImV4cCI6MTcyOTk2OTY0MH0.YhIyQmyCXhZ6CXHksKKCU6Ek5CQ89W17gDxC-3m39xQ");

        // URL для запроса
        string url = "http://84.252.135.231/api/info/wagons?trainId=1";

        try
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            string responseBody = await response.Content.ReadAsStringAsync();
            return Ok(responseBody);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Request error: " + e.Message);
            return StatusCode(500);
        }
    }
}