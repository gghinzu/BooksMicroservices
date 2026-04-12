using Books.APP.Features.Authors;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Books.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthorsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _mediator.Send(new AuthorQueryRequest());
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post(AuthorCreateRequest request)
        {
            var result = await _mediator.Send(request);

            if (!result.IsSuccessful)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Put(AuthorUpdateRequest request)
        {
            var result = await _mediator.Send(request);

            if (!result.IsSuccessful)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(AuthorDeleteRequest request)
        {
            var result = await _mediator.Send(request);

            if (!result.IsSuccessful)
                return BadRequest(result);

            return Ok(result);
        }
    }
}