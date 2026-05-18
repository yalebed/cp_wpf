using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalCarApplication.Core.Model
{
    public class Review : Entity<int>
    {
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "Enter review text")]
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "Review length must be between 5 and 1000 characters")]
        public string Text { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        public byte[] PhotoData { get; set; }

        public DateTime CreatedAt { get; set; }

        public int CarId { get; set; }
        public Car Car { get; set; }

        public string Email { get; set; }
        public User User { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        [NotMapped]
        public string RatingStars => new string('\u2605', Rating) + new string('\u2606', 5 - Rating);
    }
}
