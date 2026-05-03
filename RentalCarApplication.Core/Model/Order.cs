using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Windows.Input;

namespace RentalCarApplication.Core.Model
{
    public class Order : Entity<int>
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Введите адресс заказа")]
        public string City { get; set; }
        [Required(ErrorMessage = "Выберите дату заказа")]
        public DateTime RentDate { get; set; }

        [Required(ErrorMessage = "Выберите дату возврата")]
        public DateTime ReturnDate { get; set; }
        public bool? Status { get; set; }
        public double Price { get; set; }
        public string FrontPhotoPath { get; set; }
        public string RearPhotoPath { get; set; }
        public string SidePhotoPath { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int CarId { get; set; }
        public Car Car { get; set; }

        public string Email { get; set; }
        public User User { get; set; }

        public virtual ICollection<Review> Reviews { get; set; }

        [NotMapped]
        public bool IsUpcomingConfirmed => Status == true && CompletedAt == null && RentDate.Date > DateTime.Today;

        [NotMapped]
        public bool IsActive => Status == true && CompletedAt == null && RentDate.Date <= DateTime.Today;

        [NotMapped]
        public bool IsCompleted => Status == true && CompletedAt != null;

        [NotMapped]
        public bool HasCompletionPhotos =>
            !string.IsNullOrWhiteSpace(FrontPhotoPath) &&
            !string.IsNullOrWhiteSpace(RearPhotoPath) &&
            !string.IsNullOrWhiteSpace(SidePhotoPath);

        [NotMapped]
        public DateTime CompletionDate => CompletedAt ?? ReturnDate;

    }

}
