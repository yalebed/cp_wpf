using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RentalCarApplication
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
{
    public App()
    {
        DispatcherUnhandledException += (s, e) =>
        {
            System.IO.File.WriteAllText("crash.log",
                $"{DateTime.Now}: {e.Exception}");
            e.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            System.IO.File.WriteAllText("crash.log",
                $"{DateTime.Now}: {e.ExceptionObject}");
        };
    }
}
}
