using Microsoft.AspNetCore.Mvc;
using auth8.Records;
using auth8.Data;
using auth8.Models;
using auth8.DTO;
using Microsoft.EntityFrameworkCore;

namespace auth8.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _factory;

    public AuthController(AppDbContext context,IHttpClientFactory factory)
    {
        _context = context;
        _factory = factory;
    }

    [HttpPost("reg")]
    public async Task<IActionResult> RegUser(RegDto dto)
    {

        if (string.IsNullOrEmpty(dto.Name))
        {
            return BadRequest("Please type a username");
        }
        if (await _context.User.AnyAsync(u=> (u.Name ?? "").ToLower() == dto.Name.ToLower()))
        {
            return BadRequest("Username already in use");
        }
        var user = new User
        {
            Name=dto.Name,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };
        var response = new RegResp
        {
            Id = user.Id,
            Name = user.Name
        };
        _context.User.Add(user);
        await _context.SaveChangesAsync();
        return Ok(response);
    }

    [HttpPost("log")]
    public async Task<IActionResult> LogUser(LogDto dto)
    {
        if(string.IsNullOrEmpty(dto.Name) || string.IsNullOrEmpty(dto.Password))
        {
            return BadRequest("Please add username and password");
        }
        var user = await _context.User.FirstOrDefaultAsync(u=> (u.Name ?? "").ToLower() == dto.Name.ToLower());

        if (user== null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.HashedPassword))
        {
            return BadRequest("Invalid username or password");
        }
        var response = new LogResp
        {
            Id = user.Id,
            Name = user.Name
        };
        return Ok(response);

    }
    [HttpGet("meteo")]
    public async Task<IActionResult> GetMeteo()
    {
        try
        {
            var client = _factory.CreateClient();
            var url = "https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41&hourly=rain";
            var response = await client.GetFromJsonAsync<InfobyHour>(url);
            if (response?.Hourly?.Rain == null || response?.Hourly?.Time == null)
            {
                return StatusCode(503,"servicen unavailabe");
            }
            var forecast= response.Hourly.Time.Select((time,index)=> new
            {
                Time=time,
                Probability = response.Hourly.Rain[index]
            }).ToList();
            return Ok(forecast);
        }
        catch(HttpRequestException ex)
        {
            return StatusCode(503,"service not available");
        }
        catch(Exception ex)
        {
            return StatusCode(500,"Internal error ,Plewase try agin later");
        }
    }
}