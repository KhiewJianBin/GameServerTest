using Microsoft.AspNetCore.Mvc;
using Server.Services;
using SharedLibrary.Requests;
using SharedLibrary.Responses;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    private IAuthenticationService authenticationService;

    public AuthenticationController(IAuthenticationService authenticationService)
    {
        this.authenticationService = authenticationService;
    }
    [HttpPost("register")]
    public IActionResult Register(AuthenticationRequest request)
    {
        var (success, content) = authenticationService.Register(request.Username, request.Password);
        if(!success) return BadRequest(content);

        return Login(request);
    }
    [HttpPost("login")]
    public IActionResult Login(AuthenticationRequest request)
    {
        var (success, content) = authenticationService.Login(request.Username, request.Password);
        if (!success) return BadRequest(content);

        return Ok(new AuthenticationResponse() { Token = content });
    }
}
