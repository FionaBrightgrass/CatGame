using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PocketCat.Controls.Windows.Models;
using PocketCat.Controls.Windows.Shop;

namespace PocketCat.Controls
{
    public partial class Shop : Window
    {
        private MainWindow MainWindow;
        public ShopViewModel ViewModel { get; set; }

        public Shop(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
            ViewModel = new ShopViewModel();
            this.DataContext = ViewModel;

            InitializeComponent();
            PopulateDataGrid(); // Populate DataGrid cells with ViewModel.ShopItems.Items
            BindWindowToMainWindow(mainWindow);
        }

        private void BindWindowToMainWindow(MainWindow mainWindow)
        {
            // Bind the height of Shop to MainWindow
            this.Height = mainWindow.ActualHeight;

            // Set the initial position to the right of MainWindow
            this.Top = mainWindow.Top;
            this.Left = mainWindow.Left + mainWindow.ActualWidth;

            // Update height and position dynamically if MainWindow size or location changes
            mainWindow.SizeChanged += (sender, e) =>
            {
                this.Height = mainWindow.ActualHeight;
                this.Top = mainWindow.Top;
                this.Left = mainWindow.Left + mainWindow.ActualWidth;
            };

            mainWindow.LocationChanged += (sender, e) =>
            {
                this.Top = mainWindow.Top;
                this.Left = mainWindow.Left + mainWindow.ActualWidth;
            };
        }

        private void PopulateDataGrid()
        {
            // Bind ItemsSource to ObservableCollection
            ShopDataGrid.ItemsSource = ViewModel.ShopItems.Items;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    MainWindow.DragMove();
                    this.DragMove(); // Allows the user to drag the window
                }
                catch
                {
                    // Handle errors silently
                }
            }
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
