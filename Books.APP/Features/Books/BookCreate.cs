using System.ComponentModel.DataAnnotations;
using Books.APP.Domain;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Books.APP.Features.Books
{
    public class BookCreateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(50)]
        public string Name { get; set; }

        public short? NumberOfPages { get; set; }

        public DateTime PublishDate { get; set; }

        public decimal Price { get; set; }

        public bool IsTopSeller { get; set; }

        public int AuthorId { get; set; }

        public List<int> GenreIds { get; set; } = new();
    }

    public class BookCreateHandler : Service<Book>, IRequestHandler<BookCreateRequest, CommandResponse>
    {
        public BookCreateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(BookCreateRequest request, CancellationToken cancellationToken)
        {
            if (await DbSet().AnyAsync(b => b.Name == request.Name.Trim(), cancellationToken))
                return Error($"Book with name {request.Name.Trim()} already exists!");

            if (!await DbSet<Author>().AnyAsync(a => a.Id == request.AuthorId, cancellationToken))
                return Error("Author not found!");

            var invalidGenreIds = request.GenreIds.Any() &&
                                  await DbSet<Genre>().CountAsync(g => request.GenreIds.Contains(g.Id), cancellationToken) != request.GenreIds.Count;

            if (invalidGenreIds)
                return Error("One or more genres were not found!");

            var entity = new Book
            {
                Name = request.Name?.Trim(),
                NumberOfPages = request.NumberOfPages,
                PublishDate = request.PublishDate,
                Price = request.Price,
                IsTopSeller = request.IsTopSeller,
                AuthorId = request.AuthorId
            };

            if (request.GenreIds.Any())
            {
                entity.BookGenres = request.GenreIds.Select(genreId => new BookGenre
                {
                    GenreId = genreId
                }).ToList();
            }

            await CreateAsync(entity, cancellationToken);

            return Success($"Book with name {request.Name.Trim()} created successfully.", entity.Id);
        }
    }
}