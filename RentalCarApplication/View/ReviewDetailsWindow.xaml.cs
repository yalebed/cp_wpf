using RentalCarApplication.Core.Model;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace RentalCarApplication.View
{
    public partial class ReviewDetailsWindow : Window
    {
        public ReviewDetailsWindow(Review review)
        {
            InitializeComponent();
            LoadReview(review);
        }

        private void LoadReview(Review review)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            txtAuthor.Text = string.IsNullOrWhiteSpace(review.User?.Name)
                ? review.Email
                : $"{review.User.Name} {review.User?.Surname}".Trim();
            txtCreatedAt.Text = review.CreatedAt.ToString("dd.MM.yyyy HH:mm");
            txtCar.Text = $"Автомобиль: {review.Car?.Brand ?? $"#{review.CarId}"}";
            txtRating.Text = review.RatingStars;
            txtEmail.Text = review.Email;
            txtReviewText.Text = review.Text;

            if (review.PhotoData != null)
            {
                try
                {
                    using (var ms = new System.IO.MemoryStream(review.PhotoData))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = ms;
                        image.EndInit();
                        imgPhoto.Source = image;
                    }
                    imgPhoto.Visibility = Visibility.Visible;
                }
                catch
                {
                    imgPhoto.Source = null;
                    imgPhoto.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                imgPhoto.Source = null;
                imgPhoto.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
