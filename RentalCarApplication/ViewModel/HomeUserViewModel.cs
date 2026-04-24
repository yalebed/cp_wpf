using System;
using System.Collections.Generic;
using System.Linq;
using RentalCarApplication.Base;
using RentalCarApplication.ViewModel;
using System.Windows;
using System.Windows.Input;
using RentalCarApplication.Infrastructure;
using RentalCarApplication.EntityFramework;
using RentalCarApplication.Core.Model;
using RentalCarApplication.Commands;
using RentalCarApplication.View.CustomMessageBox;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.Win32;

namespace RentalCarApplication.ViewModel
{
    public class HomeUserViewModel : ViewModelBase
    {
        UnitOfWork unitOfWork;
        public HomeUserViewModel(Navigator navigator)
        {
            unitOfWork = new UnitOfWork();
            CurrentUser = LoginWindowViewModel.CurrentUser;
            if (CurrentUser != null)
            {
                UserName = CurrentUser.Name;
                UserSurname = CurrentUser.Surname;
                UserEmail = CurrentUser.Email;
                UserDriverLicense = CurrentUser.DriverLicense;
                UserPassport = CurrentUser.Passport;
                UserPassportPhotoPath = CurrentUser.PassportPhotoPath;
                UserIdentitySelfiePhotoPath = CurrentUser.IdentitySelfiePhotoPath;
                UserDriverLicensePhotoPath = CurrentUser.DriverLicensePhotoPath;
                UserTelNumber = CurrentUser.TelNumber;
                UserPassword = CurrentUser.Password;
            }
            ReviewRating = 5;
            LogoutCommand = new NavigationCommand<LoginWindowViewModel>(navigator, () => new LoginWindowViewModel(navigator));
            AboutCommand = new RelayCommand(OnAboutCommandExecuted, CanAboutCommandExecute);
            //OrderTab
            CurrentStartDate = DateTime.Today;
            CurrentEndDate = CurrentStartDate.AddMonths(1);
            ReturnStartDate = CurrentStartDate.AddDays(1);
            ReturnEndDate = CurrentEndDate.AddMonths(1);
            PlaceOrderCommand = new RelayCommand(OnPlaceOrderExecuted, CanPlaceOrderExecute);
            RefreshOrderTabCommand = new RelayCommand(OnRefreshOrderTabExecuted, CanRefreshOrderTabExecute);
            SearchCarCommand = new RelayCommand(OnSearchCarExecuted, CanSearchCarExecute);
            ClearSearchFieldsCommand = new RelayCommand(OnClearSearchFieldsExecuted, CanClearSearchFieldsExecute);
            DisplayCars();
            RefreshReviewsData();

            //CabinetTab
            ChangePasswordCommand = new RelayCommand(OnChangePasswordExecuted, CanChangePasswordExecute);
            UploadPassportPhotoCommand = new RelayCommand(OnUploadPassportPhotoExecuted, CanUploadPassportPhotoExecute);
            UploadIdentitySelfiePhotoCommand = new RelayCommand(OnUploadIdentitySelfiePhotoExecuted, CanUploadIdentitySelfiePhotoExecute);
            UploadDriverLicensePhotoCommand = new RelayCommand(OnUploadDriverLicensePhotoExecuted, CanUploadDriverLicensePhotoExecute);
            SaveDocumentsCommand = new RelayCommand(OnSaveDocumentsExecuted, CanSaveDocumentsExecute);
            ClearPassportPhotoCommand = new RelayCommand(OnClearPassportPhotoExecuted, CanClearPassportPhotoExecute);
            ClearIdentitySelfiePhotoCommand = new RelayCommand(OnClearIdentitySelfiePhotoExecuted, CanClearIdentitySelfiePhotoExecute);
            ClearDriverLicensePhotoCommand = new RelayCommand(OnClearDriverLicensePhotoExecuted, CanClearDriverLicensePhotoExecute);
            RefreshDocumentsStatus();

            //OrdersBasket
            RefreshOrdersCommand = new RelayCommand(OnRefreshOrdersExecuted, CanRefreshOrdersExecute);
            CancelOrderCommand = new RelayCommand(OnCancelOrderExecuted, CanCancelOrderExecute);
            DisplayOrders();

            //Reviews
            AddReviewPhotoCommand = new RelayCommand(OnAddReviewPhotoExecuted, CanAddReviewPhotoExecute);
            RemoveReviewPhotoCommand = new RelayCommand(OnRemoveReviewPhotoExecuted, CanRemoveReviewPhotoExecute);
            SetReviewRatingCommand = new RelayCommand(OnSetReviewRatingExecuted, CanSetReviewRatingExecute);
            SubmitReviewCommand = new RelayCommand(OnSubmitReviewExecuted, CanSubmitReviewExecute);
            RefreshReviewsCommand = new RelayCommand(OnRefreshReviewsExecuted, CanRefreshReviewsExecute);
        }

        #region PopupMenuCommands
       
        public ICommand LogoutCommand { get; }
        public ICommand AboutCommand { get; }
        private bool CanAboutCommandExecute(object o) => true;
        private void OnAboutCommandExecuted(object o)
        {
            try
            {

                var p = new Process();
                p.StartInfo = new ProcessStartInfo(@"D:\Учеба\2 семестр\OOP\Курсач\Пояснительная_записка_(Писарик).docx")
                {
                    UseShellExecute = true
                };
                p.Start();


            }
            catch (Exception ex)
            {
                var result = new CustomMessageBox(ex.Message,
                                    MessageType.Error,
                                    MessageButtons.Ok).ShowDialog();
            }
        }
        #endregion


        #region CurrentUser
        User CurrentUser = new User();
        private string _currentUserName;
        public string CurrentUserName
        {
            get => _currentUserName;
            set => Set(ref _currentUserName, value);
        }

        private string _currentUserSurname;
        public string CurrentUserSurname
        {
            get => _currentUserSurname;
            set => Set(ref _currentUserSurname, value);
        }
        #endregion

        #region OrderTab

        #region RefreshOrderTab
        public ICommand RefreshOrderTabCommand { get; }
        private bool CanRefreshOrderTabExecute(object o) => true;
        private void OnRefreshOrderTabExecuted(object o)
        {
            try
            {
                DisplayCars();
                ClearOrderFields();
            }
            catch(Exception ex)
            {
                var result = new CustomMessageBox(ex.Message,
                                    MessageType.Error,
                                    MessageButtons.Ok).ShowDialog();

            }
           
        }
        #endregion

        #region CarProperties
        private string _carNumber;
        public string CarNumber
        {
            get => _carNumber;
            set => Set(ref _carNumber, value);

        }

        private string _carModel;
        public string CarModel
        {
            get => _carModel;
            set => Set(ref _carModel, value);
        }

        private string _carEngine;
        public string CarEngine
        {
            get => _carEngine;
            set => Set(ref _carEngine, value);

        }

        private string _carGearBox;
        public string CarGearBox
        {
            get => _carGearBox;
            set => Set(ref _carGearBox, value);
        }

        private string _carBody;
        public string CarBody
        {
            get => _carBody;
            set => Set(ref _carBody, value);
        }

        private string _carSeats;
        public string CarSeats
        {
            get => _carSeats;
            set => Set(ref _carSeats, value);
        }

        private string _carConsumption;
        public string CarConsumption
        {
            get => _carConsumption;
            set => Set(ref _carConsumption, value);
        }

        private string _carPrice;
        public string CarPrice
        {
            get => _carPrice;
            set
            {
                Set(ref _carPrice, value);
                CalculateOrderPrice();
            }
        }





        #endregion

        #region SelectedCar
        private Car _selectedCar;
        public Car SelectedCar
        {
            get => _selectedCar;
            set
            {

                Set(ref _selectedCar, value);
                SelectionCarChanged();
            }
        }
        private void SelectionCarChanged()
        {
            if (SelectedCar != null)
            {
                CarNumber = SelectedCar.CarId.ToString();
                CarBody = SelectedCar.BodyType;
                CarEngine = SelectedCar.EngineCapacity.ToString();
                CarGearBox = SelectedCar.GearBox;
                CarConsumption = SelectedCar.Consumption.ToString();
                CarModel = SelectedCar.Brand;
                CarSeats = SelectedCar.Seats.ToString();
                CarPrice = SelectedCar.Price.ToString();
                SelectedReviewCar = SelectedCar;
            }

        }
        #endregion

        #region DisplayAllCars
        private List<Car> _carList;
        public List<Car> CarList
        {
            get => _carList;
            set => Set(ref _carList, value);
        }
        private void DisplayCars()
        {
            List<string> temp = new List<string>();
            CarList = (List<Car>)unitOfWork.CarRepository.FindAll();
            ReviewCarList = CarList.ToList();
            SearchBrandList = new List<string>();
           
            if(CarList.Count!=0)
            {
                foreach (var n in CarList)
                {
                    if (!temp.Contains(n.Brand))
                        temp.Add(n.Brand);
                }
            }
            SearchBrandList = temp;
        }
        #endregion

        #region OrderProperties

        public DateTime CurrentStartDate { get; set; }
        public DateTime CurrentEndDate { get; set; }

        private DateTime _returnEndDate;
        public DateTime ReturnEndDate
        {
            get => _returnEndDate;
            set => Set(ref _returnEndDate, value);
        }


        private DateTime _returnStartDate;
        public DateTime ReturnStartDate
        {
            get => _returnStartDate;
            set => Set(ref _returnStartDate, value);
        }


        private DateTime _rentDate = DateTime.Today;
        public DateTime RentDate
        {
            get => _rentDate;
            set
            {
                Set(ref _rentDate, value);
                ReturnStartDate = value.AddDays(1);
                ReturnEndDate = value.AddMonths(1);
                ReturnDate = value.AddDays(1);
                CalculateOrderPrice();
            }
        }

        private DateTime _returnDate = DateTime.Today.AddDays(1);
        public DateTime ReturnDate
        {
            get => _returnDate;
            set
            {
                Set(ref _returnDate, value);
                CalculateOrderPrice();
            }

        }

        private string _orderCity;
        public string OrderCity
        {
            get => _orderCity;
            set => Set(ref _orderCity, value);
        }

        private string _orderPrice;
        public string OrderPrice
        {
            get => _orderPrice;
            set => Set(ref _orderPrice, value);
        }


        #endregion

        #region CalculateOrderPrice
        public void CalculateOrderPrice()
        {
            if (CarPrice != null && RentDate != null && ReturnDate != null)
            {
                OrderPrice = (Convert.ToDouble(CarPrice) * Convert.ToDouble((ReturnDate - RentDate).Duration().Days)).ToString();
            }
        }
        #endregion

        #region ClearOrderFieldsMethod
        public void ClearOrderFields()
        {
            CarNumber = "";
            CarBody = "";
            CarModel = "";
            CarEngine = "";
            CarConsumption = "";
            CarGearBox = "";
            CarSeats = "";
            CarPrice = null;
            SelectedCar = null;
            RentDate = DateTime.Today;
            OrderCity = "";
            OrderPrice = "";

        }
        #endregion

        #region PlaceOrder
        public ICommand PlaceOrderCommand { get; }
        private bool CanPlaceOrderExecute(object o) => true;
        private void OnPlaceOrderExecuted(object o)
        {
            try
            {

                Order order = new Order();
                if (SelectedCar == null)
                {
                    throw new Exception("Выберите автомобиль");
                }
                if (OrderPrice == null)
                {
                    throw new Exception("Выберите автомобиль");
                }
                if (ReturnDate <= RentDate)
                {
                    throw new Exception("Дата возврата должна быть позже даты аренды");
                }
                if (!HasCompletedDocuments())
                {
                    throw new Exception("Перед оформлением заказа заполните документы в личном кабинете");
                }
                order.CarId = Convert.ToInt32(CarNumber);
                order.Email = CurrentUser.Email;
                order.City = OrderCity;
                order.RentDate = RentDate;
                order.ReturnDate = ReturnDate;
                order.Price = Convert.ToDouble(OrderPrice);
                order.Status = null;

                if (Validation.CheckValid(order))
                {
                    if(WaitingOrders.Where(x=>x.Email == CurrentUser.Email && x.Status == null).Any())
                    {
                        throw new Exception("Дождитесь пока администратор обработает ваш предыдущий заказ");
                    }
                    if (unitOfWork.OrderRepository.HasOverlappingOrder(order.CarId, order.RentDate, order.ReturnDate))
                    {
                        throw new Exception("Автомобиль уже занят на выбранный период");
                    }
                    unitOfWork.OrderRepository.Create(order);
                    unitOfWork.Save();
                    RefreshOrderTabCommand.Execute(order);
                    RefreshOrdersCommand.Execute(order);
                    var result = new CustomMessageBox("Ваш заказ принят.\nПерейдите в корзину, чтобы следить за статусом заказа",
                                     MessageType.Success,
                                     MessageButtons.Ok).ShowDialog();

                }

            }
            catch (Exception ex)
            {
                var result = new CustomMessageBox(ex.Message,
                                    MessageType.Error,
                                    MessageButtons.Ok).ShowDialog();
            }


        }
        #endregion

        #region SearchCar

        #region SearchCarCommand
        public ICommand SearchCarCommand { get; }
        private bool CanSearchCarExecute(object o) => true;
        private void OnSearchCarExecuted(object o)
        {
            try
            {
                //List<Car> _carList = new List<Car>();
                List<Car> _tempCarList = new List<Car>();
                List<Car> _tempList = new List<Car>();

                //_carList.AddRange(CarList);
                _tempCarList = (List<Car>)unitOfWork.CarRepository.FindAll();
                if (!String.IsNullOrEmpty(SearchBrand))
                {
                    _tempList.Clear();
                    foreach (var n in _tempCarList)
                    {
                        if (Regex.IsMatch(n.Brand, SearchBrand, RegexOptions.IgnoreCase))
                        {
                            _tempList.Add(n);
                        }
                    }
                    _tempCarList.Clear();
                    _tempCarList.AddRange(_tempList);
                }

                if (!String.IsNullOrEmpty(SearchBodyType))
                {
                    _tempList.Clear();
                    foreach (var n in _tempCarList)
                    {
                        if (Regex.IsMatch(n.BodyType, SearchBodyType, RegexOptions.IgnoreCase))
                        {
                            _tempList.Add(n);
                        }
                    }
                    _tempCarList.Clear();
                    _tempCarList.AddRange(_tempList);
                }

                if (!String.IsNullOrEmpty(SearchSeats))
                {
                    _tempList.Clear();
                    foreach (var n in _tempCarList)
                    {
                        if (Regex.IsMatch(n.Seats.ToString(), SearchSeats, RegexOptions.IgnoreCase))
                        {
                            _tempList.Add(n);
                        }
                    }
                    _tempCarList.Clear();
                    _tempCarList.AddRange(_tempList);
                }

                if (!String.IsNullOrEmpty(SearchGearBox))
                {
                    _tempList.Clear();
                    foreach (var n in _tempCarList)
                    {
                        if (Regex.IsMatch(n.GearBox, SearchGearBox, RegexOptions.IgnoreCase))
                        {
                            _tempList.Add(n);
                        }
                    }
                    _tempCarList.Clear();
                    _tempCarList.AddRange(_tempList);
                }

                if (!String.IsNullOrEmpty(SearchPriceFrom))
                {
                    _tempList.Clear();
                    foreach (var n in _tempCarList)
                    {
                        if ((n.Price >= Convert.ToDouble(SearchPriceFrom)))
                        {
                            _tempList.Add(n);
                        }
                    }
                    _tempCarList.Clear();
                    _tempCarList.AddRange(_tempList);
                }

                if (!String.IsNullOrEmpty(SearchPriceTo))
                {
                    _tempList.Clear();
                    foreach (var n in _tempCarList)
                    {
                        if ((n.Price <= Convert.ToDouble(SearchPriceTo)))
                        {
                            _tempList.Add(n);
                        }
                    }
                    _tempCarList.Clear();
                    _tempCarList.AddRange(_tempList);
                }
                _tempCarList.Clear();
                _tempCarList.AddRange(_tempList);

                if (_tempCarList.Count == 0)
                {
                    throw new Exception("Автомобилей с такими параметрами не найдено");
                }
                else
                {
                    CarList = _tempCarList;
                }

            }
            catch (Exception ex)
            {
                var result = new CustomMessageBox(ex.Message,
                                    MessageType.Error,
                                    MessageButtons.Ok).ShowDialog();
                DisplayCars();
            }
        }
        #endregion

        #region SearchProperties

        #region SearchBrandFields

        //public ICommand SearchBrandFocusChangedCommand { get; }
        //private bool CanSearchBrandFocusChangedExecute(object o) => true;
        //private void OnSearchBrandFocusChangedExecuted(object o)
        //{
        //    if(SearchBrandFocus == false)
        //        SearchBrandFocus = true;
        //    else
        //        SearchBrandFocus = false;
        //}

        //private bool _searchBrandFocus = false;
        //public bool SearchBrandFocus
        //{
        //    get => _searchBrandFocus;
        //    set => Set(ref _searchBrandFocus, value);
        //}



        #endregion


        private List<string> _searchBrandList;
        public List<string> SearchBrandList
        {
            get => _searchBrandList;
            set => Set(ref _searchBrandList, value);
        }

        private string _searchBrand;
        public string SearchBrand
        {
            get => _searchBrand;
            set => Set(ref _searchBrand, value);
        }

        private string _searchGearBox;
        public string SearchGearBox
        {
            get => _searchGearBox;
            set => Set(ref _searchGearBox, value);
        }

        private string _searchBodyType;
        public string SearchBodyType
        {
            get => _searchBodyType;
            set => Set(ref _searchBodyType, value);
        }

        private string _searchSeats;
        public string SearchSeats
        {
            get => _searchSeats;
            set => Set(ref _searchSeats, value);
        }

        private string _searchPriceFrom;
        public string SearchPriceFrom
        {
            get => _searchPriceFrom;
            set => Set(ref _searchPriceFrom, value);
        }

        private string _searchPriceTo;
        public string SearchPriceTo
        {
            get => _searchPriceTo;
            set => Set(ref _searchPriceTo, value);
        }

        #endregion

        #region ClearSearchFields
        public ICommand ClearSearchFieldsCommand { get; }
        private bool CanClearSearchFieldsExecute(object o) => true;
        private void OnClearSearchFieldsExecuted(object o)
        {
            SearchBrand = "";
            SearchGearBox = "";
            SearchSeats = "";
            SearchBodyType = "";
            SearchPriceFrom = "";
            SearchPriceTo = "";
            //SearchBrandList.Clear();
            DisplayCars();
        }
        #endregion

        #endregion

        #endregion

        #region CabinetTab

        #region CabinetProperties
        private string _userName;
        public string UserName
        {
            get => _userName;
            set => Set(ref _userName, value);
        }

        private string _userSurname;
        public string UserSurname
        {
            get => _userSurname;
            set => Set(ref _userSurname, value);
        }

        private string _userPassport;
        public string UserPassport
        {
            get => _userPassport;
            set => Set(ref _userPassport, value);
        }

        private string _userDriverLicense;
        public string UserDriverLicense
        {
            get => _userDriverLicense;
            set => Set(ref _userDriverLicense, value);
        }

        private string _userPassportPhotoPath;
        public string UserPassportPhotoPath
        {
            get => _userPassportPhotoPath;
            set => Set(ref _userPassportPhotoPath, value);
        }

        private string _userIdentitySelfiePhotoPath;
        public string UserIdentitySelfiePhotoPath
        {
            get => _userIdentitySelfiePhotoPath;
            set => Set(ref _userIdentitySelfiePhotoPath, value);
        }

        private string _userDriverLicensePhotoPath;
        public string UserDriverLicensePhotoPath
        {
            get => _userDriverLicensePhotoPath;
            set => Set(ref _userDriverLicensePhotoPath, value);
        }

        private string _documentsStatusText;
        public string DocumentsStatusText
        {
            get => _documentsStatusText;
            set => Set(ref _documentsStatusText, value);
        }

        private string _userTelNumber;
        public string UserTelNumber
        {
            get => _userTelNumber;
            set => Set(ref _userTelNumber, value);
        }

        private string _userEmail;
        public string UserEmail
        {
            get => _userEmail;
            set => Set(ref _userEmail, value);
        }

        private string _userPassword;
        public string UserPassword
        {
            get => _userPassword;
            set => Set(ref _userPassword, value);
        }

        private string _userConfirmPassword;
        public string UserConfirmPassword
        {
            get => _userConfirmPassword;
            set => Set(ref _userConfirmPassword, value);
        }

        private string _userNewPassword;
        public string UserNewPassword
        {
            get => _userNewPassword;
            set => Set(ref _userNewPassword, value);
        }

        private string _userShownNewPassword;
        public string UserShownNewPassword
        {
            get => _userShownNewPassword;
            set => Set(ref _userShownNewPassword, value);
        }

        //private string _userRole;
        //public string UserRole
        //{
        //    get => _userRole;
        //    set => Set(ref _userRole, value);
        //}


        #endregion

        public ICommand ChangePasswordCommand { get; }
        public ICommand UploadPassportPhotoCommand { get; }
        public ICommand UploadIdentitySelfiePhotoCommand { get; }
        public ICommand UploadDriverLicensePhotoCommand { get; }
        public ICommand SaveDocumentsCommand { get; }
        public ICommand ClearPassportPhotoCommand { get; }
        public ICommand ClearIdentitySelfiePhotoCommand { get; }
        public ICommand ClearDriverLicensePhotoCommand { get; }

        private void RefreshDocumentsStatus()
        {
            if (CurrentUser == null)
            {
                DocumentsStatusText = "Документы не загружены";
                return;
            }

            if (!CurrentUser.HasRequiredDocuments)
            {
                DocumentsStatusText = "Документы не заполнены";
                return;
            }

            DocumentsStatusText = CurrentUser.IsDocumentsVerified
                ? "Документы подтверждены администратором"
                : "Документы загружены и ожидают проверки администратора";
        }

        private bool HasCompletedDocuments()
        {
            return !string.IsNullOrWhiteSpace(UserPassport) &&
                   !string.IsNullOrWhiteSpace(UserDriverLicense) &&
                   !string.IsNullOrWhiteSpace(UserIdentitySelfiePhotoPath);
        }

        private void ValidateDocuments()
        {
            if (string.IsNullOrWhiteSpace(UserPassport))
            {
                throw new Exception("Введите номер паспорта");
            }

            if (!Regex.IsMatch(UserPassport, @"^([A-Z][A-Z][0-9]{7})$"))
            {
                throw new Exception("Паспорт | Формат неверный.");
            }

            if (string.IsNullOrWhiteSpace(UserDriverLicense))
            {
                throw new Exception("Введите номер водительского удостоверения");
            }

            if (!Regex.IsMatch(UserDriverLicense, @"^([A-Z][A-Z]([0-9]){7})$"))
            {
                throw new Exception("Водительское удостоверение | Формат неверный.");
            }

            if (string.IsNullOrWhiteSpace(UserIdentitySelfiePhotoPath))
            {
                throw new Exception("Добавьте селфи с удостоверением личности");
            }
        }

        private string PickImageFile()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Image files|*.bmp;*.jpg;*.jpeg;*.gif;*.png;*.tif",
                FilterIndex = 1
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        private bool CanUploadPassportPhotoExecute(object o) => true;
        private void OnUploadPassportPhotoExecuted(object o)
        {
            try
            {
                var fileName = PickImageFile();
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    UserPassportPhotoPath = fileName;
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox(ex.Message, MessageType.Error, MessageButtons.Ok).ShowDialog();
            }
        }

        private bool CanUploadIdentitySelfiePhotoExecute(object o) => true;
        private void OnUploadIdentitySelfiePhotoExecuted(object o)
        {
            try
            {
                var fileName = PickImageFile();
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    UserIdentitySelfiePhotoPath = fileName;
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox(ex.Message, MessageType.Error, MessageButtons.Ok).ShowDialog();
            }
        }

        private bool CanUploadDriverLicensePhotoExecute(object o) => true;
        private void OnUploadDriverLicensePhotoExecuted(object o)
        {
            try
            {
                var fileName = PickImageFile();
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    UserDriverLicensePhotoPath = fileName;
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox(ex.Message, MessageType.Error, MessageButtons.Ok).ShowDialog();
            }
        }

        private bool CanClearPassportPhotoExecute(object o) => true;
        private void OnClearPassportPhotoExecuted(object o)
        {
            UserPassportPhotoPath = string.Empty;
        }

        private bool CanClearIdentitySelfiePhotoExecute(object o) => true;
        private void OnClearIdentitySelfiePhotoExecuted(object o)
        {
            UserIdentitySelfiePhotoPath = string.Empty;
        }

        private bool CanClearDriverLicensePhotoExecute(object o) => true;
        private void OnClearDriverLicensePhotoExecuted(object o)
        {
            UserDriverLicensePhotoPath = string.Empty;
        }

        private bool CanSaveDocumentsExecute(object o) => true;
        private void OnSaveDocumentsExecuted(object o)
        {
            try
            {
                User user = unitOfWork.UserRepository.Find(CurrentUser.Email);
                if (user == null)
                {
                    throw new Exception("Пользователь не найден");
                }

                if (!unitOfWork.UserRepository.CheckPassportAndLicense(UserPassport, UserDriverLicense, CurrentUser.Email))
                {
                    throw new Exception("Пользователь с такими паспортными данными уже существует");
                }

                user.Passport = UserPassport;
                user.DriverLicense = UserDriverLicense;
                user.PassportPhotoPath = UserPassportPhotoPath;
                user.IdentitySelfiePhotoPath = UserIdentitySelfiePhotoPath;
                user.DriverLicensePhotoPath = UserDriverLicensePhotoPath;

                ValidateDocuments();

                bool isVerifiedBeforeUpdate = CurrentUser.IsDocumentsVerified;
                bool documentsChanged = CurrentUser.Passport != user.Passport ||
                                        CurrentUser.DriverLicense != user.DriverLicense ||
                                        CurrentUser.PassportPhotoPath != user.PassportPhotoPath ||
                                        CurrentUser.IdentitySelfiePhotoPath != user.IdentitySelfiePhotoPath ||
                                        CurrentUser.DriverLicensePhotoPath != user.DriverLicensePhotoPath;

                user.IsDocumentsVerified = documentsChanged ? false : isVerifiedBeforeUpdate;

                unitOfWork.UserRepository.Update(user.Email, user);
                unitOfWork.Save();

                CurrentUser = user;
                LoginWindowViewModel.CurrentUser = user;
                UserPassport = user.Passport;
                UserDriverLicense = user.DriverLicense;
                UserPassportPhotoPath = user.PassportPhotoPath;
                UserIdentitySelfiePhotoPath = user.IdentitySelfiePhotoPath;
                UserDriverLicensePhotoPath = user.DriverLicensePhotoPath;
                RefreshDocumentsStatus();

                new CustomMessageBox("Документы сохранены", MessageType.Success, MessageButtons.Ok).ShowDialog();
            }
            catch (Exception ex)
            {
                new CustomMessageBox(ex.Message, MessageType.Error, MessageButtons.Ok).ShowDialog();
            }
        }

        private bool CanChangePasswordExecute(object o) => true;
        private void OnChangePasswordExecuted(object o)
        {
            try
            {
                User user = CurrentUser;
 
                if(Encryption.Dencrypt(UserPassword) != UserConfirmPassword)
                {
                    throw new Exception("Текущий пароль не верный");
                }

                user.Password = UserNewPassword;
                if(Validation.CheckValid(user))
                {
                    CurrentUser.Password = Encryption.Encrypt(UserNewPassword);
                    unitOfWork.UserRepository.Update(CurrentUser.Email, CurrentUser);
                    unitOfWork.Save();
                    //CurrentUser.Password = Encryption.Dencrypt(CurrentUser.Password); ;
                    UserPassword = CurrentUser.Password;
                    var result = new CustomMessageBox("Пароль успешно изменен",
                                     MessageType.Success,
                                     MessageButtons.Ok).ShowDialog();
                }
                
               
            }
            catch(Exception ex)
            {
                var result = new CustomMessageBox(ex.Message,
                                    MessageType.Error,
                                    MessageButtons.Ok).ShowDialog();
            }
        }




        #endregion

        #region OrderBasketTab

        #region Properties

        private List<Order> _allOrders;
        public List<Order> AllOrders
        {
            get => _allOrders;
            set => Set(ref _allOrders, value);
        }

        private List<Order> _waitingOrders;
        public List<Order> WaitingOrders
        {
            get => _waitingOrders;
            set => Set(ref _waitingOrders, value);
        }

        private List<Order> _confirmedOrders;
        public List<Order> ConfirmedOrders
        {
            get => _confirmedOrders;
            set => Set(ref _confirmedOrders, value);
        }

        private List<Order> _canceledOrders;
        public List<Order> CanceledOrders
        {
            get => _canceledOrders;
            set => Set(ref _canceledOrders, value);
        }

        private Order _selectedOrder;
        public Order SelectedOrder
        {
            get => _selectedOrder;
            set => Set(ref _selectedOrder, value);
        }

        #endregion

        #region DisplayOrdersMethod
        private void DisplayOrders()
        {
            AllOrders = (List<Order>)unitOfWork.OrderRepository.FindAll();
            WaitingOrders = AllOrders.Where(x => x.Email == CurrentUser.Email && x.Status == null).ToList();
            ConfirmedOrders = AllOrders.Where(x => x.Email == CurrentUser.Email && x.Status == true).ToList();
            CanceledOrders = AllOrders.Where(x => x.Email == CurrentUser.Email && x.Status == false).ToList();

        }

        #endregion

        #region RefreshOrders
        public ICommand RefreshOrdersCommand { get; }

        private bool CanRefreshOrdersExecute(object o) => true;
        private void OnRefreshOrdersExecuted(object o)
        {
            DisplayOrders();
            DisplayAvailableReviewOrders();
        }
        #endregion

        #region CancelOrder
        
        public ICommand CancelOrderCommand { get; }

        private bool CanCancelOrderExecute(object o) => true;
        private void OnCancelOrderExecuted(object o)
        {
            
            var result = new CustomMessageBox("Вы уверены, что хотите отменить свой заказ?",
                                     MessageType.Confirmation,
                                     MessageButtons.YesNo).ShowDialog();
            if(result == true)
            {
                Order order = SelectedOrder;
                order.Status = false;
                unitOfWork.OrderRepository.Update(order.OrderId, order);
                unitOfWork.Save();
                DisplayOrders();
                DisplayAvailableReviewOrders();
                var res = new CustomMessageBox("Ваш заказ отменен и перемещен во вкладку \"Отмененные\" ",
                                     MessageType.Info,
                                     MessageButtons.Ok).ShowDialog();
            }
            
        }

        #endregion

        #endregion

        #region ReviewsTab

        private List<Car> _reviewCarList;
        public List<Car> ReviewCarList
        {
            get => _reviewCarList;
            set => Set(ref _reviewCarList, value);
        }

        private Car _selectedReviewCar;
        public Car SelectedReviewCar
        {
            get => _selectedReviewCar;
            set
            {
                Set(ref _selectedReviewCar, value);
                DisplayReviewsForSelectedCar();
            }
        }

        private List<Review> _reviewList;
        public List<Review> ReviewList
        {
            get => _reviewList;
            set => Set(ref _reviewList, value);
        }

        private List<ReviewOrderOption> _availableReviewOrders;
        public List<ReviewOrderOption> AvailableReviewOrders
        {
            get => _availableReviewOrders;
            set => Set(ref _availableReviewOrders, value);
        }

        private bool _hasAvailableReviewOrders;
        public bool HasAvailableReviewOrders
        {
            get => _hasAvailableReviewOrders;
            set => Set(ref _hasAvailableReviewOrders, value);
        }

        private ReviewOrderOption _selectedReviewOrderOption;
        public ReviewOrderOption SelectedReviewOrderOption
        {
            get => _selectedReviewOrderOption;
            set
            {
                Set(ref _selectedReviewOrderOption, value);
                SelectedReviewOrder = value?.Order;
            }
        }

        private Order _selectedReviewOrder;
        public Order SelectedReviewOrder
        {
            get => _selectedReviewOrder;
            set
            {
                Set(ref _selectedReviewOrder, value);
                OnPropertyChanged(nameof(SelectedReviewOrderInfo));
                if (value != null)
                {
                    SelectedReviewCar = ReviewCarList?.FirstOrDefault(x => x.CarId == value.CarId);
                }
            }
        }

        public string SelectedReviewOrderInfo
        {
            get
            {
                if (SelectedReviewOrderOption != null)
                {
                    return SelectedReviewOrderOption.Details;
                }

                if (SelectedReviewOrder == null)
                {
                    return "Выберите завершенный заказ, чтобы увидеть подробности";
                }

                var car = ReviewCarList?.FirstOrDefault(x => x.CarId == SelectedReviewOrder.CarId);
                var brand = car?.Brand ?? $"Авто #{SelectedReviewOrder.CarId}";
                return $"{brand}; заказ №{SelectedReviewOrder.OrderId}; период: {SelectedReviewOrder.RentDate:dd.MM.yyyy} - {SelectedReviewOrder.ReturnDate:dd.MM.yyyy}; адрес: {SelectedReviewOrder.City}; сумма: {SelectedReviewOrder.Price}$";
            }
        }

        private string _reviewText;
        public string ReviewText
        {
            get => _reviewText;
            set => Set(ref _reviewText, value);
        }

        private string _reviewPhotoPath;
        public string ReviewPhotoPath
        {
            get => _reviewPhotoPath;
            set => Set(ref _reviewPhotoPath, value);
        }

        private int _reviewRating;
        public int ReviewRating
        {
            get => _reviewRating;
            set
            {
                Set(ref _reviewRating, value);
                OnPropertyChanged(nameof(ReviewStar1));
                OnPropertyChanged(nameof(ReviewStar2));
                OnPropertyChanged(nameof(ReviewStar3));
                OnPropertyChanged(nameof(ReviewStar4));
                OnPropertyChanged(nameof(ReviewStar5));
            }
        }

        public string ReviewStar1 => ReviewRating >= 1 ? "\u2605" : "\u2606";
        public string ReviewStar2 => ReviewRating >= 2 ? "\u2605" : "\u2606";
        public string ReviewStar3 => ReviewRating >= 3 ? "\u2605" : "\u2606";
        public string ReviewStar4 => ReviewRating >= 4 ? "\u2605" : "\u2606";
        public string ReviewStar5 => ReviewRating >= 5 ? "\u2605" : "\u2606";

        public ICommand AddReviewPhotoCommand { get; }
        public ICommand RemoveReviewPhotoCommand { get; }
        public ICommand SetReviewRatingCommand { get; }
        public ICommand SubmitReviewCommand { get; }
        public ICommand RefreshReviewsCommand { get; }

        private void RefreshReviewsData()
        {
            ReviewCarList = (List<Car>)unitOfWork.CarRepository.FindAll();
            DisplayAvailableReviewOrders();

            if (SelectedReviewCar == null && ReviewCarList != null && ReviewCarList.Count > 0)
            {
                SelectedReviewCar = ReviewCarList.First();
            }
            else
            {
                DisplayReviewsForSelectedCar();
            }
        }

        private void DisplayAvailableReviewOrders()
        {
            var allOrders = (List<Order>)unitOfWork.OrderRepository.FindAll();
            var availableOrders = allOrders
                .Where(x => x.Email == CurrentUser.Email && x.Status == true && x.ReturnDate.Date <= DateTime.Today)
                .OrderByDescending(x => x.ReturnDate)
                .ToList();

            foreach (var order in availableOrders)
            {
                order.Car = ReviewCarList?.FirstOrDefault(x => x.CarId == order.CarId);
            }

            AvailableReviewOrders = availableOrders
                .Select(order =>
                {
                    var car = order.Car ?? ReviewCarList?.FirstOrDefault(x => x.CarId == order.CarId);
                    var brand = car?.Brand ?? $"Авто #{order.CarId}";
                    var hasReview = unitOfWork.ReviewRepository.ExistsForOrder(order.OrderId);
                    var reviewStatus = hasReview ? "; отзыв уже оставлен" : "";
                    return new ReviewOrderOption
                    {
                        Order = order,
                        Title = $"{brand}, заказ №{order.OrderId}, {order.RentDate:dd.MM} - {order.ReturnDate:dd.MM}",
                        Details = $"{brand}; заказ №{order.OrderId}; период: {order.RentDate:dd.MM.yyyy} - {order.ReturnDate:dd.MM.yyyy}; адрес: {order.City}; сумма: {order.Price}${reviewStatus}"
                    };
                })
                .ToList();

            if (AvailableReviewOrders.Count == 0)
            {
                HasAvailableReviewOrders = false;
                SelectedReviewOrderOption = null;
                SelectedReviewOrder = null;
                return;
            }

            HasAvailableReviewOrders = true;

            if (SelectedReviewOrderOption == null || !AvailableReviewOrders.Any(x => x.Order.OrderId == SelectedReviewOrderOption.Order.OrderId))
            {
                SelectedReviewOrderOption = AvailableReviewOrders.First();
            }
        }

        private void DisplayReviewsForSelectedCar()
        {
            if (SelectedReviewCar == null)
            {
                ReviewList = new List<Review>();
                return;
            }

            ReviewList = unitOfWork.ReviewRepository.FindByCarId(SelectedReviewCar.CarId).ToList();
        }

        private bool CanAddReviewPhotoExecute(object o) => true;
        private void OnAddReviewPhotoExecuted(object o)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Image files|*.bmp;*.jpg;*.jpeg;*.gif;*.png;*.tif";
                dialog.FilterIndex = 1;

                if (dialog.ShowDialog() == true)
                {
                    ReviewPhotoPath = dialog.FileName;
                }
            }
            catch (Exception ex)
            {
                var result = new CustomMessageBox(ex.Message,
                                    MessageType.Error,
                                    MessageButtons.Ok).ShowDialog();
            }
        }

        private bool CanRemoveReviewPhotoExecute(object o) => true;
        private void OnRemoveReviewPhotoExecuted(object o)
        {
            ReviewPhotoPath = string.Empty;
        }

        private bool CanSetReviewRatingExecute(object o) => true;
        private void OnSetReviewRatingExecuted(object o)
        {
            if (o == null)
            {
                return;
            }

            if (int.TryParse(o.ToString(), out int rating))
            {
                ReviewRating = rating;
            }
        }

        private bool CanSubmitReviewExecute(object o) => true;
        private void OnSubmitReviewExecuted(object o)
        {
            try
            {
                if (SelectedReviewOrder == null)
                {
                    throw new Exception("Выберите завершенный заказ для отзыва");
                }

                if (SelectedReviewOrder.Status != true || SelectedReviewOrder.ReturnDate.Date > DateTime.Today)
                {
                    throw new Exception("Оставить отзыв можно только после завершения подтвержденной аренды");
                }

                if (unitOfWork.ReviewRepository.ExistsForOrder(SelectedReviewOrder.OrderId))
                {
                    throw new Exception("Для выбранного заказа отзыв уже существует");
                }

                Review review = new Review
                {
                    OrderId = SelectedReviewOrder.OrderId,
                    CarId = SelectedReviewOrder.CarId,
                    Email = CurrentUser.Email,
                    Text = ReviewText,
                    Rating = ReviewRating,
                    PhotoPath = ReviewPhotoPath,
                    CreatedAt = DateTime.Now
                };

                if (Validation.CheckValid(review))
                {
                    unitOfWork.ReviewRepository.Create(review);
                    unitOfWork.Save();
                    ReviewText = string.Empty;
                    ReviewRating = 5;
                    ReviewPhotoPath = string.Empty;
                    SelectedReviewOrder = null;
                    RefreshReviewsData();
                    var result = new CustomMessageBox("Отзыв успешно опубликован",
                                        MessageType.Success,
                                        MessageButtons.Ok).ShowDialog();
                }
            }
            catch (Exception ex)
            {
                var result = new CustomMessageBox(ex.Message,
                                    MessageType.Error,
                                    MessageButtons.Ok).ShowDialog();
            }
        }

        private bool CanRefreshReviewsExecute(object o) => true;
        private void OnRefreshReviewsExecuted(object o)
        {
            RefreshReviewsData();
        }

        #endregion

    }

    public class ReviewOrderOption
    {
        public Order Order { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
    }

}
