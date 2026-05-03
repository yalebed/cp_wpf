using RentalCarApplication.Core.Model;
using System.Linq;
using System.Windows;

namespace RentalCarApplication.View
{
    public partial class CompletedOrderPhotosWindow : Window
    {
        public string HeaderText { get; }
        public string FrontPhotoPath { get; }
        public string RearPhotoPath { get; }
        public string SidePhotoPath { get; }

        public CompletedOrderPhotosWindow(Order order)
        {
            InitializeComponent();
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            HeaderText = $"Заказ №{order.OrderId}, автомобиль #{order.CarId}";
            FrontPhotoPath = order.FrontPhotoPath;
            RearPhotoPath = order.RearPhotoPath;
            SidePhotoPath = order.SidePhotoPath;
            DataContext = this;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
