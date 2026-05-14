using CORE.APP.Models;
using CORE.APP.Services;
using Books.APP.Domain;
using Books.APP.Features.Authors;
using Books.APP.Features.Genres;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Books.APP.Features.Books
{
    public class BookQueryRequest : Request, IRequest<IQueryable<BookQueryResponse>>
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

        public List<int> GenreIds { get; set; }

        public string IsTopSellerF { get; set; }

        public string PriceF { get; set; }

        public string PublishDateF { get; set; }

        public string AuthorF { get; set; }

        public AuthorQueryResponse Author { get; set; }

        public string GenresF { get; set; }

        public List<GenreQueryResponse> Genres { get; set; }
    }

    public class BookQueryHandler : Service<Book>, IRequestHandler<BookQueryRequest, IQueryable<BookQueryResponse>>
    {
        public BookQueryHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<Book> DbSet()
        {
            return base.DbSet().Include(b => b.Author)
                .Include(b => b.BookGenres).ThenInclude(bg => bg.Genre)
                .OrderByDescending(b => b.PublishDate).ThenBy(b => b.Price)
                .ThenBy(b => b.Name);
        }

        public Task<IQueryable<BookQueryResponse>> Handle(BookQueryRequest request, CancellationToken cancellationToken)
        {
            var query = DbSet().Select(b => new BookQueryResponse
            {
                Id = b.Id,
                Name = b.Name,
                NumberOfPages = b.NumberOfPages,
                PublishDate = b.PublishDate,
                Price = b.Price,
                IsTopSeller = b.IsTopSeller,
                AuthorId = b.AuthorId,
                GenreIds = b.GenreIds,

                IsTopSellerF = b.IsTopSeller ? "Top Seller" : "No Top Seller",
                PriceF = b.Price.ToString("C2"),
                PublishDateF = b.PublishDate.ToString("MM/dd/yyyy HH:mm:ss"),

                AuthorF = b.Author.FirstName + " " + b.Author.LastName,
                GenresF = string.Join(", ", b.BookGenres.Select(bg => bg.Genre.Name)),

                Author = new AuthorQueryResponse
                {
                    Id = b.Author.Id,
                    FirstName = b.Author.FirstName,
                    LastName = b.Author.LastName,
                    FullName = b.Author.FirstName + " " + b.Author.LastName
                },

                Genres = b.BookGenres.Select(bg => new GenreQueryResponse
                {
                    Id = bg.Genre.Id,
                    Name = bg.Genre.Name
                }).ToList()
            });

            return Task.FromResult(query);
        }
    }
}