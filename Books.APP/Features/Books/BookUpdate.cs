using System.ComponentModel.DataAnnotations;
using Books.APP.Domain;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Books.APP.Features.Books
{
    public class BookUpdateRequest : Request, IRequest<CommandResponse>
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

    public class BookUpdateHandler : Service<Book>, IRequestHandler<BookUpdateRequest, CommandResponse>
    {
        public BookUpdateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(BookUpdateRequest request, CancellationToken cancellationToken)
        {
            var entity = await DbSet()
                .Include(b => b.BookGenres)
                .SingleOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

            if (entity is null)
                return Error("Book not found!");

            if (await DbSet().AnyAsync(b => b.Id != request.Id && b.Name == request.Name.Trim(), cancellationToken))
                return Error($"Book with name {request.Name.Trim()} already exists!");

            if (!await DbSet<Author>().AnyAsync(a => a.Id == request.AuthorId, cancellationToken))
                return Error("Author not found!");

            var invalidGenreIds = request.GenreIds.Any() &&
                                  await DbSet<Genre>().CountAsync(g => request.GenreIds.Contains(g.Id), cancellationToken) != request.GenreIds.Count;

            if (invalidGenreIds)
                return Error("One or more genres were not found!");

            entity.Name = request.Name?.Trim();
            entity.NumberOfPages = request.NumberOfPages;
            entity.PublishDate = request.PublishDate;
            entity.Price = request.Price;
            entity.IsTopSeller = request.IsTopSeller;
            entity.AuthorId = request.AuthorId;

            if (entity.BookGenres.Any())
                Delete(entity.BookGenres);

            entity.BookGenres = request.GenreIds.Select(genreId => new BookGenre
            {
                BookId = entity.Id,
                GenreId = genreId
            }).ToList();

            await UpdateAsync(entity, cancellationToken);

            return Success($"Book with id {request.Id} updated successfully.", entity.Id);
        }
    }
}