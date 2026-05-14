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

        public List<int> GenreIds { get; set; } = new List<int>();
    }

    public class BookCreateHandler : Service<Book>, IRequestHandler<BookCreateRequest, CommandResponse>
    {
        public BookCreateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(BookCreateRequest request, CancellationToken cancellationToken)
        {
            if (await DbSet().AnyAsync(b => b.Name == request.Name.Trim(), cancellationToken))
                return Error($"Book with the same name: \"{request.Name.Trim()}\" exists!");

            if (!await DbSet<Author>().AnyAsync(a => a.Id == request.AuthorId, cancellationToken))
                return Error("Related author not found!");

            if (request.GenreIds.Any())
            {
                var count = await DbSet<Genre>().CountAsync(g => request.GenreIds.Contains(g.Id), cancellationToken);
                if (count != request.GenreIds.Count)
                    return Error("Related genre or genres not found!");
            }

            var entity = new Book
            {
                Name = request.Name?.Trim(),
                NumberOfPages = request.NumberOfPages,
                PublishDate = request.PublishDate,
                Price = request.Price,
                IsTopSeller = request.IsTopSeller,
                AuthorId = request.AuthorId,
                GenreIds = request.GenreIds
            };

            await CreateAsync(entity, cancellationToken);

            return Success($"Book with name {request.Name.Trim()} created successfully.", entity.Id);
        }
    }
}