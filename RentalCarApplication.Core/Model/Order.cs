using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalCarApplication.Core.Model
{
    public class Order : Entity<int>
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Выберите дату заказа")]
        public DateTime RentDate { get; set; }

        [Required(ErrorMessage = "Выберите дату возврата")]
        public DateTime ReturnDate { get; set; }

        public bool? Status { get; set; }
        public double Price { get; set; }
        public byte[] FrontPhotoData { get; set; }
        public byte[] RearPhotoData { get; set; }
        public byte[] SidePhotoData { get; set; }
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
            FrontPhotoData != null && FrontPhotoData.Length > 0 &&
            RearPhotoData != null && RearPhotoData.Length > 0 &&
            SidePhotoData != null && SidePhotoData.Length > 0;

        [NotMapped]
        public DateTime CompletionDate => CompletedAt ?? ReturnDate;
    }
}
