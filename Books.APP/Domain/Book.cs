using CORE.APP.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Books.APP.Domain
{
    public class Book : Entity
    {
        [Required, StringLength(50)]
        public string Name { get; set; }

        public short? NumberOfPages { get; set; }

        public DateTime PublishDate { get; set; }

        public decimal Price { get; set; }

        public bool IsTopSeller { get; set; }

        public int AuthorId { get; set; }

        public Author Author { get; set; }

        public List<BookGenre> BookGenres { get; set; } = new List<BookGenre>();

        [NotMapped]
        public List<int> GenreIds
        {
            get => BookGenres.Select(bg => bg.GenreId).ToList();
            set => BookGenres = value.Select(v => new BookGenre { GenreId = v }).ToList();
        }
    }
}