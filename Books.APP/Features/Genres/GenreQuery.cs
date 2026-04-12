using Books.APP.Domain;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Books.APP.Features.Genres
{
    public class GenreQueryRequest : IRequest<List<GenreQueryResponse>>
    {
    }

    public class GenreQueryResponse : Response
    {
        public string Name { get; set; }
    }

    public class GenreQueryHandler : Service<Genre>, IRequestHandler<GenreQueryRequest, List<GenreQueryResponse>>
    {
        public GenreQueryHandler(DbContext db) : base(db)
        {
        }

        public async Task<List<GenreQueryResponse>> Handle(GenreQueryRequest request, CancellationToken cancellationToken)
        {
            return await DbSet()
                .OrderBy(g => g.Name)
                .Select(g => new GenreQueryResponse
                {
                    Id = g.Id,
                    Name = g.Name
                })
                .ToListAsync(cancellationToken);
        }
    }
}