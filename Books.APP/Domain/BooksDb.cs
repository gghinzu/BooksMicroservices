using Microsoft.EntityFrameworkCore;

namespace Books.APP.Domain
{
    public class BooksDb : DbContext
    {
        public DbSet<Book> Books { get; set; }
        
        public DbSet<Author> Authors { get; set; }
        
        public DbSet<Genre> Genres { get; set; }
        
        public DbSet<BookGenre> BookGenres { get; set; }

        public BooksDb(DbContextOptions options) : base(options)
        {
        }
    }
}