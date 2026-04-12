using Books.APP.Domain;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Books.APP.Features.Books
{
    public class BookDeleteRequest : Request, IRequest<CommandResponse>
    {
    }

    public class BookDeleteHandler : Service<Book>, IRequestHandler<BookDeleteRequest, CommandResponse>
    {
        public BookDeleteHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(BookDeleteRequest request, CancellationToken cancellationToken)
        {
            var entity = await DbSet().SingleOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

            if (entity is null)
                return Error("Book not found!");

            await DeleteAsync(entity, cancellationToken);

            return Success($"Book with id {request.Id} deleted successfully.", entity.Id);
        }
    }
}