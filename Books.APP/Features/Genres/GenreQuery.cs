using CORE.APP.Models;
using CORE.APP.Services;
using Books.APP.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Books.APP.Features.Genres
{
    public class GenreQueryRequest : Request, IRequest<IQueryable<GenreQueryResponse>>
    {
    }

    public class GenreQueryResponse : Response
    {
        public string Name { get; set; }
    }

    public class GenreQueryHandler : Service<Genre>, IRequestHandler<GenreQueryRequest, IQueryable<GenreQueryResponse>>
    {
        public GenreQueryHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<Genre> DbSet()
        {
            return base.DbSet()
                .OrderBy(g => g.Name);
        }

        public Task<IQueryable<GenreQueryResponse>> Handle(GenreQueryRequest request, CancellationToken cancellationToken)
        {
            var query = DbSet().Select(g => new GenreQueryResponse
            {
                Id = g.Id,
                Name = g.Name
            });

            return Task.FromResult(query);
        }
    }
}