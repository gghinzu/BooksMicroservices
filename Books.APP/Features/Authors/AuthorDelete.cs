using Books.APP.Domain;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Books.APP.Features.Authors
{
    public class AuthorDeleteRequest : Request, IRequest<CommandResponse>
    {
    }

    public class AuthorDeleteHandler : Service<Author>, IRequestHandler<AuthorDeleteRequest, CommandResponse>
    {
        public AuthorDeleteHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(AuthorDeleteRequest request, CancellationToken cancellationToken)
        {
            var entity = await DbSet().SingleOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (entity is null)
                return Error("Author not found!");

            await DeleteAsync(entity, cancellationToken);

            return Success($"Author with id {request.Id} deleted successfully.", entity.Id);
        }
    }
}