using Microsoft.Win32;
using RentalCarApplication.Commands;
using RentalCarApplication.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CustomMessageBoxWindow = RentalCarApplication.View.CustomMessageBox.CustomMessageBox;
using MessageButtons = RentalCarApplication.View.CustomMessageBox.MessageButtons;
using MessageType = RentalCarApplication.View.CustomMessageBox.MessageType;

namespace RentalCarApplication.View
{
    public partial class CompleteOrderWindow : Window, INotifyPropertyChanged
    {
        public CompleteOrderWindow(Order order)
        {
            InitializeComponent();
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            UploadFrontPhotoCommand = new RelayCommand(_ => UploadPhoto(PhotoTarget.Front), _ => true);
            UploadRearPhotoCommand = new RelayCommand(_ => UploadPhoto(PhotoTarget.Rear), _ => true);
            UploadSidePhotoCommand = new RelayCommand(_ => UploadPhoto(PhotoTarget.Side), _ => true);
            OrderSummary = $"Заказ №{order.OrderId}, автомобиль #{order.CarId}, период: {order.RentDate:dd.MM.yyyy} - {order.ReturnDate:dd.MM.yyyy}.";
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand UploadFrontPhotoCommand { get; }
        public ICommand UploadRearPhotoCommand { get; }
        public ICommand UploadSidePhotoCommand { get; }

        public string OrderSummary { get; }

        private byte[] _frontPhotoData;
        public byte[] FrontPhotoData
        {
            get => _frontPhotoData;
            set => SetProperty(ref _frontPhotoData, value);
        }

        private byte[] _rearPhotoData;
        public byte[] RearPhotoData
        {
            get => _rearPhotoData;
            set => SetProperty(ref _rearPhotoData, value);
        }

        private byte[] _sidePhotoData;
        public byte[] SidePhotoData
        {
            get => _sidePhotoData;
            set => SetProperty(ref _sidePhotoData, value);
        }

        private void UploadPhoto(PhotoTarget target)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image files|*.bmp;*.jpg;*.jpeg;*.gif;*.png;*.tif",
                FilterIndex = 1
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            switch (target)
            {
                case PhotoTarget.Front:
                    FrontPhotoData = File.ReadAllBytes(dialog.FileName);
                    break;
                case PhotoTarget.Rear:
                    RearPhotoData = File.ReadAllBytes(dialog.FileName);
                    break;
                case PhotoTarget.Side:
                    SidePhotoData = File.ReadAllBytes(dialog.FileName);
                    break;
            }
        }

        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (FrontPhotoData == null ||
                RearPhotoData == null ||
                SidePhotoData == null)
            {
                new CustomMessageBoxWindow("Прикрепите фото автомобиля спереди, сзади и сбоку.",
                    MessageType.Error,
                    MessageButtons.Ok).ShowDialog();
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private enum PhotoTarget
        {
            Front,
            Rear,
            Side
        }
    }
}
