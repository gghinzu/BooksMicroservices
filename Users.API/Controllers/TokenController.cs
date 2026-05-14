using CORE.APP.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Users.APP.Features.Tokens;

namespace Users.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokensController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;

        public TokensController(IMediator mediator, IConfiguration configuration)
        {
            _mediator = mediator;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Post(TokenRequest request)
        {
            request.SecurityKey = _configuration["JwtSettings:Key"];
            request.Issuer = _configuration["JwtSettings:Issuer"];
            request.Audience = _configuration["JwtSettings:Audience"];

            var response = await _mediator.Send(request);
            if (response is not null)
                return Ok(response);

            return BadRequest(new CommandResponse(false, _configuration["TokenMessage:InvalidCredentials"]));
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequest request)
        {
            request.SecurityKey = _configuration["JwtSettings:Key"];
            request.Issuer = _configuration["JwtSettings:Issuer"];
            request.Audience = _configuration["JwtSettings:Audience"];

            var response = await _mediator.Send(request);
            if (response is not null)
                return Ok(response);

            return BadRequest(new CommandResponse(false, _configuration["TokenMessage:InvalidRefreshToken"]));
        }
    }
}