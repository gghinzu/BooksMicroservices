using System.ComponentModel.DataAnnotations;
using Books.APP.Domain;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Books.APP.Features.Authors
{

    public class AuthorUpdateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(50)]
        public string FirstName { get; set; }

        [Required, StringLength(50)]
        public string LastName { get; set; }
    }


    public class AuthorUpdateHandler : Service<Author>, IRequestHandler<AuthorUpdateRequest, CommandResponse>
    {
        public AuthorUpdateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(AuthorUpdateRequest request, CancellationToken cancellationToken)
        {
            var entity = await DbSet().SingleOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (entity is null)
                return Error("Author not found!");

            if (await DbSet().AnyAsync(a =>
                    a.Id != request.Id &&
                    a.FirstName == request.FirstName.Trim() &&
                    a.LastName == request.LastName.Trim(), cancellationToken))
                return Error($"Author {request.FirstName.Trim()} {request.LastName.Trim()} already exists!");

            entity.FirstName = request.FirstName?.Trim();
            entity.LastName = request.LastName?.Trim();

            await UpdateAsync(entity, cancellationToken);

            return Success($"Author with id {request.Id} updated successfully.", entity.Id);
        }
    }
}