using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TestJwt.Scripts;
using WAYMORR_MS_Product;

namespace Xakaton2024.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ApplicationContext context, IConnectionMultiplexer redis): ControllerBase
{
    private readonly PasswordHasher<Person> _passwordHasher = new();
    private static List<Person> Users = new();
    
    private static readonly HttpClient client = new ();

    private static Dictionary<string, DateTime> TimeEmailMailing = new();
    
    private async Task SendMessage(string email, string message)
    {
        try
        {
            string url = $"http://62.217.178.173/api/send_message/{email}/{Uri.EscapeDataString(message)}";
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response from server: " + responseBody);
            }
            else
            {
                Console.WriteLine("Request failed with status: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
    
    private async Task StoreConfirmationCodeAsync(string userId, string code)
    {
        IDatabase db = redis.GetDatabase();
        await db.StringSetAsync(userId, code, TimeSpan.FromMinutes(5));
    }
    private async Task<string> GetConfirmationCodeAsync(string userId)
    {
        IDatabase db = redis.GetDatabase();
        return (await db.StringGetAsync(userId));
    }
    private async Task<bool> DeleteConfirmationCodeAsync(string userId)
    {
        IDatabase db = redis.GetDatabase();
        return await db.KeyDeleteAsync(userId);
    }
    
    
    [HttpPost("registration/user")]
    public async Task<IActionResult> RegistrationUser([FromQuery] string name, [FromQuery] string email, [FromQuery] string password)
    {
        if (await context.Users.FirstOrDefaultAsync(u => u.Email == email) != null)
        {
            var problem = new ProblemDetails {
                Status = 403,
                Title = "Forbidden",
                Detail = "данный пользователь уже существует!"
            };

            return Problem(problem.Detail, null, problem.Status, problem.Title);
        }

        Person person = new Person
        {
            PersonId = Guid.NewGuid().ToString(),
            Name = name,
            Email = email,
            PasswordVersion = 1
        };
        person.Password = _passwordHasher.HashPassword(person, password);
        
        Users.Add(person);

        return await SendNewConfirmationCode(person.PersonId);
    }

    [HttpPost("confirmation-code/user")]
    public async Task<IActionResult> CheckConfirmationCode([FromQuery] string user, [FromQuery] string code)
    {
        var userIpAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress?.ToString();

        if (!HttpContext.Request.Headers.ContainsKey("deviceInfo"))
        {
            var problem = new ProblemDetails {
                Status = 400,
                Title = "Bad Request",
                Detail = "отсутствует параметр deviceInfo"
            };

            return Problem(problem.Detail, null, problem.Status, problem.Title);
        }
        
        string result = await GetConfirmationCodeAsync(user);

        if (result == code)
        {
            Person person = context.Users.FirstOrDefault(u => u.PersonId == user)!;
            
            string adress = await DeterminingIPAddress.GetPositionUser(userIpAddress);
            string message = $"Вход с нового устройства: мы обнаружили вход в Ваш аккаунт с нового устройства в {DateTime.Now} \n\nУстройство: {HttpContext.Request.Headers["User-Agent"]}, {adress} - {userIpAddress}";
            await SendMessage(person.Email, message);

            try
            {
                await context.Users.AddAsync(person);
                await context.SaveChangesAsync();
            }
            catch
            {
                
            }
            finally
            {
                Users.Remove(person);
            }
            
            await DeleteConfirmationCodeAsync(user);
            return Ok(TokenService.GenerateToken(person.PersonId, person.PasswordVersion));
        }
        
        var problem1 = new ProblemDetails {
            Status = 403,
            Title = "Forbidden",
            Detail = "данный код не верен"
        };

        return Problem(problem1.Detail, null, problem1.Status, problem1.Title);
    }

    [HttpPost("new-confirmation-code/send")]
    public async Task<IActionResult> SendNewConfirmationCode([FromQuery] string user)
    {
        if (TimeEmailMailing.ContainsKey(user))
        {
            var time = (DateTime.Now - TimeEmailMailing[user]).TotalSeconds;
            if (time < 120)
            {
                var problem = new ProblemDetails
                {
                    Status = 403,
                    Title = "Forbidden",
                    Detail = "вы слишком часто отправляете код подтверждения"
                };

                problem.Extensions["Time"] = time;

                return Problem(problem.Detail, null, problem.Status, problem.Title, JsonSerializer.Serialize(problem.Extensions));
            }
        }

        var person = Users.FirstOrDefault(u => u.PersonId == user);
        if (person == null)
            person = await context.Users.FirstOrDefaultAsync(u => u.PersonId == user);
        await DeleteConfirmationCodeAsync(user);

        if (person != null)
        {
            Random rnd = new Random();
            string code = rnd.Next(111111, 999999).ToString();
            await StoreConfirmationCodeAsync(user, code);
            
            Console.WriteLine($"Почта: {person.Email} код: {code}");

            await SendMessage(person.Email, $"Ваш код подтверждения: {code} \n\nКод действителен в течении 5 минут");
        }
        
        TimeEmailMailing[user] = DateTime.Now;
        
        return Ok(new {id = person.PersonId, message = "На указанную почту был отправлен код подтверждения"});
    }

    [HttpPost("authorization/user")]
    public async Task<IActionResult> AuthorizationUser([FromQuery] string email, [FromQuery] string password)
    {
        var person = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

        
        if (person == null || _passwordHasher.VerifyHashedPassword(person, person.Password, password) != PasswordVerificationResult.Success)
        {
            var problem = new ProblemDetails {
                Status = 403,
                Title = "Forbidden",
                Detail = "Логин или пароль не верен!"
            };

            return Problem(problem.Detail, null, problem.Status, problem.Title);
        }
        
        return await SendNewConfirmationCode(person.PersonId);
    }

    [Authorize]
    [HttpPost("refresh-token")]
    public async Task<ActionResult> RefreshToken()
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        
        try
        {
            if (TokenService.GetTwtTokenRevoked(token))
            {
                var problem = new ProblemDetails {
                    Status = 403,
                    Title = "Forbidden",
                    Detail = "Отказано в доступе!"
                };

                return Problem(problem.Detail, null, problem.Status, problem.Title);
            }
        }
        catch
        {
            var problem = new ProblemDetails {
                Status = 403,
                Title = "Forbidden",
                Detail = "Отказано в доступе!"
            };

            return Problem(problem.Detail, null, problem.Status, problem.Title);
        }

        var data = TokenService.GetJwtTokenData(token);
        var user = await context.Users.FirstOrDefaultAsync(u => u.PersonId == data.UserId);

        if (data.Version != user.PasswordVersion)
        {
            var problem = new ProblemDetails {
                Status = 403,
                Title = "Forbidden",
                Detail = "Отказано в доступе!"
            };

            return Problem(problem.Detail, null, problem.Status, problem.Title);
        }
        
        TokenService.RevokeJwtToken(token);
        
        return Ok(TokenService.GenerateToken(user.PersonId, user.PasswordVersion));
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfileData()
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var data = TokenService.GetJwtTokenData(token);

        if (data.TokenType == "refresh")
            return Unauthorized();
        
        var person = await context.Users.FirstOrDefaultAsync(u => u.PersonId == data.UserId);

        return Ok(new
        {
            User = person.Name,
            Email = person.Email,
            Order = "None"
        });
    }
}

public class Person
{
    public int Id { get; set; }
    public string PersonId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int PasswordVersion { get; set; }
}

public class PersonToken
{
    public string refreshToken { get; set; }
}