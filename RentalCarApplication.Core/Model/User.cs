using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RentalCarApplication.Core.Model
{
    public class User : Entity<string>
    {
        [Required(ErrorMessage = "Email | Введите ваш Email\n")]
        [RegularExpression(@"^[-a-z0-9!#$%&'*+/=?^_`{|}~]+(\.[-a-z0-9!#$%&'*+/=?^_`{|}~]+)*@([a-z0-9]([-a-z0-9]{0,61}[a-z0-9])?\.)*(aero|arpa|asia|biz|by|cat|com|coop|edu|gov|info|int|jobs|mil|mobi|museum|name|net|org|pro|ru|tel|travel|[a-z][a-z])$", ErrorMessage = "Email | Формат неверный. \n")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Пароль | Введите пароль \n")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)[a-zA-Z0-9]*$", ErrorMessage = "Пароль | Должен содержать хотя бы одну заглавную букву и одну цифру. \n")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль | Минимальная длина 8 символов. \n")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Имя | Введите ваше Имя\n")]
        [RegularExpression(@"^([a-zA-Zа-яА-ЯёЁ]{2,15})$", ErrorMessage = "Имя | Формат неверный. \n")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Фамилия | Введите вашу Фамилию\n")]
        [RegularExpression(@"^([a-zA-Zа-яА-ЯёЁ]{2,20}(\-[a-zA-Zа-яА-ЯёЁ]{2,20})?)$", ErrorMessage = "Фамилия | Формат неверный. \n")]
        public string Surname { get; set; }

        [RegularExpression(@"^([A-Z][A-Z][0-9]{7})$", ErrorMessage = "Паспорт | Формат неверный. \n")]
        public string Passport { get; set; }

        [RegularExpression(@"^([A-Z][A-Z]([0-9]){7})$", ErrorMessage = "Водительское удостоверение | Формат неверный.\n")]
        public string DriverLicense { get; set; }

        public byte[] IdentitySelfiePhotoData { get; set; }

        [Required(ErrorMessage = "Телефон | Введите номер телефона \n")]
        [RegularExpression(@"^\+375(29|33|44|25|17)[0-9]{7}$", ErrorMessage = "Телефон | Формат неверный.\n")]
        public string TelNumber { get; set; }

        public bool IsAdmin { get; set; }
        public bool IsDocumentsVerified { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }

        [NotMapped]
        public bool HasRequiredDocuments =>
            !string.IsNullOrWhiteSpace(Passport) &&
            !string.IsNullOrWhiteSpace(DriverLicense) &&
            IdentitySelfiePhotoData != null && IdentitySelfiePhotoData.Length > 0;

    }
}
