using System.ComponentModel.DataAnnotations;
using Books.APP.Domain;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Books.APP.Features.Authors
{
    public class AuthorCreateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(50)]
        public string FirstName { get; set; }

        [Required, StringLength(50)]
        public string LastName { get; set; }
    }

    public class AuthorCreateHandler : Service<Author>, IRequestHandler<AuthorCreateRequest, CommandResponse>
    {
        public AuthorCreateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(AuthorCreateRequest request, CancellationToken cancellationToken)
        {
            if (await DbSet().AnyAsync(a =>
                    a.FirstName == request.FirstName.Trim() &&
                    a.LastName == request.LastName.Trim(), cancellationToken))
                return Error($"Author {request.FirstName.Trim()} {request.LastName.Trim()} already exists!");

            var entity = new Author
            {
                FirstName = request.FirstName?.Trim(),
                LastName = request.LastName?.Trim()
            };

            await CreateAsync(entity, cancellationToken);

            return Success(
                $"Author {request.FirstName.Trim()} {request.LastName.Trim()} created successfully.",
                entity.Id);
        }
    }
}