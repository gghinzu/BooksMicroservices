using CORE.APP.Models;
using CORE.APP.Services;
using Books.APP.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Books.APP.Features.Books
{
    public class BookQueryRequest : IRequest<List<BookQueryResponse>>
    {
    }

    public class BookQueryResponse : Response
    {
        public string Name { get; set; }
        public short? NumberOfPages { get; set; }
        public DateTime PublishDate { get; set; }
        public decimal Price { get; set; }
        public bool IsTopSeller { get; set; }
        public int AuthorId { get; set; }
        public string AuthorFullName { get; set; }
    }

    public class BookQueryHandler : Service<Book>, IRequestHandler<BookQueryRequest, List<BookQueryResponse>>
    {
        public BookQueryHandler(DbContext db) : base(db)
        {
        }

        public async Task<List<BookQueryResponse>> Handle(BookQueryRequest request, CancellationToken cancellationToken)
        {
            return await DbSet()
                .Include(b => b.Author)
                .OrderBy(b => b.Name)
                .Select(b => new BookQueryResponse
                {
                    Id = b.Id,
                    Name = b.Name,
                    NumberOfPages = b.NumberOfPages,
                    PublishDate = b.PublishDate,
                    Price = b.Price,
                    IsTopSeller = b.IsTopSeller,
                    AuthorId = b.AuthorId,
                    AuthorFullName = b.Author.FirstName + " " + b.Author.LastName
                })
                .ToListAsync(cancellationToken);
        }
    }
}