using System.ComponentModel.DataAnnotations;
using CORE.APP.Domain;

namespace Books.APP.Domain;

public class Genre : Entity
{
    [Required, StringLength(50)]
    public string Name { get; set; }
    
    public List<BookGenre> BookGenres { get; set; } = new();
}