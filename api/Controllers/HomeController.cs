using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneSource.Api.Data;
using OneSource.Api.DTOs;
using OneSource.Api.Models;

namespace OneSource.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeController(AppDbContext db) : ControllerBase
{
    private const int HomeContentId = 1;
    private const string DefaultHeading = "Empowering You with the Latest in IT & Technology";
    private const string DefaultDescription = "OneSource Solutions Limited is an ICT company providing customized hardware and software solutions to enable vital business processes across varied sectors. As KRA authorized suppliers of Electronic Fiscal Devices and providers of hospitality, payroll, networking, and CCTV solutions, we're your complete technology partner in Nairobi.";

    [HttpGet]
    public async Task<ActionResult<HomePageDto>> Get()
    {
        var home = await GetOrCreateHomeAsync();
        return Ok(new HomePageDto(home.Heading, home.Description));
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<HomePageDto>> Update(UpdateHomePageDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Heading) || string.IsNullOrWhiteSpace(dto.Description))
            return BadRequest("Heading and description are required.");

        var home = await GetOrCreateHomeAsync();
        home.Heading = dto.Heading.Trim();
        home.Description = dto.Description.Trim();
        await db.SaveChangesAsync();

        return Ok(new HomePageDto(home.Heading, home.Description));
    }

    private async Task<HomeContent> GetOrCreateHomeAsync()
    {
        var home = await db.HomeContents.FirstOrDefaultAsync(item => item.HomeContentId == HomeContentId);
        if (home is not null) return home;

        home = new HomeContent
        {
            HomeContentId = HomeContentId,
            Heading = DefaultHeading,
            Description = DefaultDescription
        };
        db.HomeContents.Add(home);
        await db.SaveChangesAsync();
        return home;
    }
}
