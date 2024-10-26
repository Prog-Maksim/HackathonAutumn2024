using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace Xakaton2024.Controllers;

[ApiController]
[Route("api")]
public class OrderController: ControllerBase
{
    [Authorize]
    [HttpPost("order")]
    public async Task<IActionResult> Order(int trainId, int wagonId, int[] seatIds)
    {
        using var client = new HttpClient();
        
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJtYWtzaW1iZWwyMDE3MEBnbWFpbC5jb20iLCJpYXQiOjE3Mjk5NjMxMDgsImV4cCI6MTczMDA0OTUwOH0.derAeYFxmJRlj4876LIujZ8FEMKC5P4EsawUPoD_YMY");
        
        string url = "http://84.252.135.231/api/order";
        
        var values = new Dictionary<string, string>
        {
            { "train_id", trainId.ToString() },
            { "wagon_id", wagonId.ToString() },
            { "seat_ids", "[" + String.Join(", ", seatIds) + "]" }
        };

        var content = new FormUrlEncodedContent(values);

        try
        {
            HttpResponseMessage response = await client.PostAsync(url, content);
            Console.WriteLine(response);
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