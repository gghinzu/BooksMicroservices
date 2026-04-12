using Books.APP.Features.Books;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Books.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BooksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _mediator.Send(new BookQueryRequest());
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post(BookCreateRequest request)
        {
            var result = await _mediator.Send(request);

            if (!result.IsSuccessful)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Put(BookUpdateRequest request)
        {
            var result = await _mediator.Send(request);

            if (!result.IsSuccessful)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(BookDeleteRequest request)
        {
            var result = await _mediator.Send(request);

            if (!result.IsSuccessful)
                return BadRequest(result);

            return Ok(result);
        }
    }
}