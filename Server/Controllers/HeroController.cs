using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Server.Services;
using SharedLibrary;
using SharedLibrary.Requests;

namespace Server.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class HeroController : ControllerBase
{
    private readonly IHeroService playerService;
    private readonly GameDBContext context;

    public HeroController(IHeroService playerService, GameDBContext context)
    {
        this.playerService = playerService;
        this.context = context;
    }
    [HttpGet]
    public Hero Get([FromQuery] int Id)
    {
        var player = new Hero() { Id = Id };

        playerService.DoSomething();

        return player;
    }

    [HttpPost("{ID}")]
    public IActionResult Edit([FromRoute] int Id, [FromBody] CreateHeroRequest request)
    {
        var heroIdsAvailable = JsonConvert.DeserializeObject<List<int>>(User.FindFirst("heroes").Value);

        if(!heroIdsAvailable.Contains(Id)) return Unauthorized();

        var hero = context.Heroes.First(h => h.Id == Id);

        hero.Name = request.Name;

        context.SaveChanges();

        return Ok();
    }

    [HttpPost]
    public Hero Post(CreateHeroRequest request)
    {
        var userId = int.Parse(User.FindFirst("id").Value);

        var user = context.Users.Include(u => u.Heroes).First(u => u.Id == userId);

        var hero = new Hero()
        {
            Name = request.Name,
            User = user
        };

        context.Add(hero);
        context.SaveChanges();

        hero.User = null;

        return hero;
    }
} 