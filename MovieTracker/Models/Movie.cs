using System.ComponentModel.DataAnnotations;

namespace MovieTracker.Models
{
    public class Movie
    {
        // Уникален идентификатор на филма.
        // SQLite ще генерира стойността при добавяне.
        public int Id { get; set; }

        [Required(ErrorMessage = "Заглавието е задължително.")]
        [StringLength(150)]
        [Display(Name = "Заглавие")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Жанрът е задължителен.")]
        [StringLength(50)]
        [Display(Name = "Жанр")]
        public string Genre { get; set; } = string.Empty;

        [Range(1888, 2100, ErrorMessage = "Въведи валидна година.")]
        [Display(Name = "Година")]
        public int ReleaseYear { get; set; }

        [Range(0, 10, ErrorMessage = "Рейтингът трябва да е между 0 и 10.")]
        [Display(Name = "Рейтинг")]
        public double Rating { get; set; }

        [Display(Name = "Гледан")]
        public bool IsWatched { get; set; }
    }
}
