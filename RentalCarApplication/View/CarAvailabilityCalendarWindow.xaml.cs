using RentalCarApplication.Core.Model;
using RentalCarApplication.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RentalCarApplication.View
{
    public partial class CarAvailabilityCalendarWindow : Window
    {
        public CarAvailabilityCalendarWindow(Car car, List<CarAvailabilityPeriod> periods)
        {
            InitializeComponent();

            txtCarTitle.Text = $"{car?.Brand} #{car?.CarId}";

            var availabilityPeriods = periods ?? new List<CarAvailabilityPeriod>();
            periodsList.ItemsSource = availabilityPeriods;

            if (availabilityPeriods.Count == 0)
            {
                periodsScroll.Visibility = Visibility.Collapsed;
                txtNoPeriods.Visibility = Visibility.Visible;
                return;
            }

            var firstDate = availabilityPeriods.Min(x => x.RentDate).Date;
            var lastDate = availabilityPeriods.Max(x => x.ReturnDate).Date;
            calendarOccupancy.DisplayDateStart = firstDate.AddMonths(-1);
            calendarOccupancy.DisplayDateEnd = lastDate.AddMonths(1);
            calendarOccupancy.DisplayDate = firstDate;
            calendarOccupancy.SelectedDate = null;

            var mergedPeriods = availabilityPeriods
                .Select(x => new
                {
                    Start = x.RentDate.Date,
                    End = x.ReturnDate.Date.AddDays(-1)
                })
                .Where(x => x.End >= x.Start)
                .OrderBy(x => x.Start)
                .ToList();

            if (mergedPeriods.Count == 0)
            {
                return;
            }

            var currentStart = mergedPeriods[0].Start;
            var currentEnd = mergedPeriods[0].End;

            for (int i = 1; i < mergedPeriods.Count; i++)
            {
                var nextPeriod = mergedPeriods[i];
                if (nextPeriod.Start <= currentEnd.AddDays(1))
                {
                    if (nextPeriod.End > currentEnd)
                    {
                        currentEnd = nextPeriod.End;
                    }
                }
                else
                {
                    calendarOccupancy.BlackoutDates.Add(new CalendarDateRange(currentStart, currentEnd));
                    currentStart = nextPeriod.Start;
                    currentEnd = nextPeriod.End;
                }
            }

            calendarOccupancy.BlackoutDates.Add(new CalendarDateRange(currentStart, currentEnd));
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
