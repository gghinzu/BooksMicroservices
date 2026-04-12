using Books.APP.Domain;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Books.APP.Features.Authors
{
    public class AuthorQueryRequest : IRequest<List<AuthorQueryResponse>>
    {
    }
    
    public class AuthorQueryResponse : Response
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    
    public class AuthorQueryHandler : Service<Author>, IRequestHandler<AuthorQueryRequest, List<AuthorQueryResponse>>
    {
        public AuthorQueryHandler(DbContext db) : base(db)
        {
        }

        public async Task<List<AuthorQueryResponse>> Handle(AuthorQueryRequest request, CancellationToken cancellationToken)
        {
            return await DbSet()
                .OrderBy(a => a.FirstName)
                .ThenBy(a => a.LastName)
                .Select(a => new AuthorQueryResponse
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName
                })
                .ToListAsync(cancellationToken);
        }
    }
}