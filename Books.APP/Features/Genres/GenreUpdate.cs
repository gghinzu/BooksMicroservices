using System.ComponentModel.DataAnnotations;
using Books.APP.Domain;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Books.APP.Features.Genres
{
    public class GenreUpdateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(50)]
        public string Name { get; set; }
    }

    public class GenreUpdateHandler : Service<Genre>, IRequestHandler<GenreUpdateRequest, CommandResponse>
    {
        public GenreUpdateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(GenreUpdateRequest request, CancellationToken cancellationToken)
        {
            if (await DbSet().AnyAsync(g => g.Id != request.Id && g.Name == request.Name.Trim(), cancellationToken))
                return Error($"Genre with the same name: \"{request.Name.Trim()}\" exists!");

            var entity = await DbSet().SingleOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

            if (entity is null)
                return Error("Genre not found!");

            entity.Name = request.Name?.Trim();

            await UpdateAsync(entity, cancellationToken);

            return Success($"Genre with name {request.Name.Trim()} updated successfully.", entity.Id);
        }
    }
}