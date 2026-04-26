using RentalCarApplication.Core.Model;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace RentalCarApplication.View
{
    public partial class UserDocumentsPreviewWindow : Window
    {
        public UserDocumentsPreviewWindow(User user)
        {
            InitializeComponent();

            txtEmail.Text = user?.Email ?? "-";
            txtName.Text = string.IsNullOrWhiteSpace(user?.Name) ? "-" : user.Name;
            txtSurname.Text = string.IsNullOrWhiteSpace(user?.Surname) ? "-" : user.Surname;
            txtPassport.Text = string.IsNullOrWhiteSpace(user?.Passport) ? "-" : user.Passport;
            txtDriverLicense.Text = string.IsNullOrWhiteSpace(user?.DriverLicense) ? "-" : user.DriverLicense;
            txtDocumentsStatus.Text = user?.IsDocumentsVerified == true
                ? "\u041F\u043E\u0434\u0442\u0432\u0435\u0440\u0436\u0434\u0435\u043D\u044B"
                : "\u041D\u0435 \u043F\u043E\u0434\u0442\u0432\u0435\u0440\u0436\u0434\u0435\u043D\u044B";

            if (!string.IsNullOrWhiteSpace(user?.IdentitySelfiePhotoPath))
            {
                try
                {
                    imgSelfie.Source = new BitmapImage(new Uri(user.IdentitySelfiePhotoPath));
                }
                catch
                {
                    txtNoImage.Text = "\u041D\u0435 \u0443\u0434\u0430\u043B\u043E\u0441\u044C \u043E\u0442\u043A\u0440\u044B\u0442\u044C \u0444\u043E\u0442\u043E";
                    txtNoImage.Visibility = Visibility.Visible;
                }
            }
            else
            {
                txtNoImage.Visibility = Visibility.Visible;
            }
        }

        private void HeaderBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
