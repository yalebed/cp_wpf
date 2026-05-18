using RentalCarApplication.Core.Model;
using System.Linq;
using System.Windows;

namespace RentalCarApplication.View
{
    public partial class CompletedOrderPhotosWindow : Window
    {
        public string HeaderText { get; }
        public byte[] FrontPhotoData { get; }
        public byte[] RearPhotoData { get; }
        public byte[] SidePhotoData { get; }

        public CompletedOrderPhotosWindow(Order order)
        {
            InitializeComponent();
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            HeaderText = $"Заказ №{order.OrderId}, автомобиль #{order.CarId}";
            FrontPhotoData = order.FrontPhotoData;
            RearPhotoData = order.RearPhotoData;
            SidePhotoData = order.SidePhotoData;
            DataContext = this;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
