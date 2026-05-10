using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using RentalCarApplication.Base;
using RentalCarApplication.Commands;
using RentalCarApplication.Core.Model;
using RentalCarApplication.EntityFramework;
using RentalCarApplication.Infrastructure;
using RentalCarApplication.View.CustomMessageBox;

namespace RentalCarApplication.ViewModel
{
    public class GuestCatalogViewModel : ViewModelBase
    {
        private readonly UnitOfWork unitOfWork;

        public GuestCatalogViewModel(Navigator navigator)
        {
            unitOfWork = new UnitOfWork();
            NavigateToLoginCommand = new NavigationCommand<LoginWindowViewModel>(navigator, () => new LoginWindowViewModel(navigator));
            NavigateToRegisterCommand = new NavigationCommand<RegisterWindowViewModel>(navigator, () => new RegisterWindowViewModel(navigator));
            SearchCarCommand = new RelayCommand(OnSearchCarExecuted, CanSearchCarExecute);
            ClearSearchFieldsCommand = new RelayCommand(OnClearSearchFieldsExecuted, CanClearSearchFieldsExecute);
            DisplayCars();
        }

        public ICommand NavigateToLoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand SearchCarCommand { get; }
        public ICommand ClearSearchFieldsCommand { get; }

        private List<Car> _carList;
        public List<Car> CarList
        {
            get => _carList;
            set => Set(ref _carList, value);
        }

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

        private string _searchGearBox;
        public string SearchGearBox
        {
            get => _searchGearBox;
            set => Set(ref _searchGearBox, value);
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

        private void DisplayCars()
        {
            List<string> brands = new List<string>();
            CarList = (List<Car>)unitOfWork.CarRepository.FindAll();
            if (CarList.Count != 0)
            {
                foreach (var car in CarList)
                {
                    if (!brands.Contains(car.Brand))
                    {
                        brands.Add(car.Brand);
                    }
                }
            }
            SearchBrandList = brands;
        }

        private bool CanSearchCarExecute(object o) => true;
        private void OnSearchCarExecuted(object o)
        {
            try
            {
                var filteredCars = ((List<Car>)unitOfWork.CarRepository.FindAll()).AsEnumerable();

                if (!string.IsNullOrEmpty(SearchBrand))
                {
                    filteredCars = filteredCars.Where(n => Regex.IsMatch(n.Brand, SearchBrand, RegexOptions.IgnoreCase));
                }

                if (!string.IsNullOrEmpty(SearchBodyType))
                {
                    filteredCars = filteredCars.Where(n => Regex.IsMatch(n.BodyType, SearchBodyType, RegexOptions.IgnoreCase));
                }

                if (!string.IsNullOrEmpty(SearchSeats))
                {
                    filteredCars = filteredCars.Where(n => Regex.IsMatch(n.Seats.ToString(), SearchSeats, RegexOptions.IgnoreCase));
                }

                if (!string.IsNullOrEmpty(SearchGearBox))
                {
                    filteredCars = filteredCars.Where(n => Regex.IsMatch(n.GearBox, SearchGearBox, RegexOptions.IgnoreCase));
                }

                if (!string.IsNullOrEmpty(SearchPriceFrom))
                {
                    filteredCars = filteredCars.Where(n => n.Price >= Convert.ToDouble(SearchPriceFrom));
                }

                if (!string.IsNullOrEmpty(SearchPriceTo))
                {
                    filteredCars = filteredCars.Where(n => n.Price <= Convert.ToDouble(SearchPriceTo));
                }

                var resultCars = filteredCars.ToList();
                if (resultCars.Count == 0)
                {
                    throw new Exception("Автомобилей с такими параметрами не найдено");
                }

                CarList = resultCars;
            }
            catch (Exception ex)
            {
                new CustomMessageBox(ex.Message, MessageType.Error, MessageButtons.Ok).ShowDialog();
            }
        }

        private bool CanClearSearchFieldsExecute(object o) => true;
        private void OnClearSearchFieldsExecuted(object o)
        {
            SearchBrand = string.Empty;
            SearchBodyType = string.Empty;
            SearchSeats = string.Empty;
            SearchGearBox = string.Empty;
            SearchPriceFrom = string.Empty;
            SearchPriceTo = string.Empty;
            DisplayCars();
        }
    }
}
