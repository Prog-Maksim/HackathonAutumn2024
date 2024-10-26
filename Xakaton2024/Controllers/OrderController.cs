using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Xakaton2024.Controllers;

[ApiController]
[Route("api/order")]
public class OrderController: ControllerBase
{
    [Authorize]
    [HttpPost("/")]
    public async Task<IActionResult> Order()
    {
        
        
        return Ok();
    }
}