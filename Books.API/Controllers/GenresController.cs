using Books.APP.Features.Genres;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Books.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GenresController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _mediator.Send(new GenreQueryRequest());
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post(GenreCreateRequest request)
        {
            var result = await _mediator.Send(request);

            if (!result.IsSuccessful)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Put(GenreUpdateRequest request)
        {
            var result = await _mediator.Send(request);

            if (!result.IsSuccessful)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(GenreDeleteRequest request)
        {
            var result = await _mediator.Send(request);

            if (!result.IsSuccessful)
                return BadRequest(result);

            return Ok(result);
        }
    }
}