using Microsoft.EntityFrameworkCore;

namespace Books.APP.Domain
{
    public class BooksDb : DbContext
    {
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookGenre> BookGenres { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public BooksDb(DbContextOptions options) : base(options)
        {
        }
    }
}