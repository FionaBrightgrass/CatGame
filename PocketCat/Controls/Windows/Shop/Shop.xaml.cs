using System;
using System.IO;
using System.Linq;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PocketCat.Controls.Windows.Inventory;
using PocketCat.Controls.Windows.Models;
using PocketCat.Controls.Windows.Shop;
using PocketCat.StaticGame;
using PocketCat.StaticGame.Items;

namespace PocketCat.Controls
{
    public partial class Shop : Window
    {
        private MainWindow MainWindow;
        private Money _money;
        public Money Money => _money;
        public ShopViewModel ViewModel { get; set; }

        public Shop(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
            ViewModel = new ShopViewModel();
            _money = ViewModel.Money; // Initialize _money from ViewModel
            this.DataContext = this; // Set DataContext to Shop itself
            InitializeComponent();
            InitializeShopInventory();
            PopulateDataGrid();
            BindWindowToMainWindow(mainWindow);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    MainWindow.DragMove();
                    this.DragMove();
                }
                catch
                {
                    // Handle errors silently to avoid disruptions
                }
            }
        }

        private void BindWindowToMainWindow(MainWindow mainWindow)
        {
            this.Height = mainWindow.ActualHeight;
            this.Top = mainWindow.Top;
            this.Left = mainWindow.Left + mainWindow.ActualWidth;

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

        private void InitializeShopInventory()
        {
            // Add shop items to SharedInventory if not already present
            foreach (var item in ViewModel.ShopItems.Items)
            {
                if (!SharedInventory.ItemsDictionary.ContainsKey(item.Name))
                {
                    SharedInventory.ItemsDictionary[item.Name] = 0; // Initialize quantity to 0
                }
            }
        }

        private void PopulateDataGrid()
        {
            var shopItems = ViewModel.ShopItems.Items.Select(item => new ShopItemViewModel
            {
                Name = item.Name,
                Price = item.Price,
                Image = GetImageFromResources(item.Name) // Bind images here
            }).ToList();

            ShopDataGrid.ItemsSource = shopItems; // Bind the updated shop items
        }
        private BitmapImage GetImageFromResources(string resourceName)
        {
            var resourceManager = new ResourceManager("PocketCat.Properties.Resources", typeof(ShopItems).Assembly);
            var resourceCulture = System.Globalization.CultureInfo.CurrentCulture;

            var iconBytes = resourceManager.GetObject(resourceName, resourceCulture) as byte[];

            if (iconBytes == null)
            {
                // Fallback to "Heart" resource if the requested image is not found
                iconBytes = resourceManager.GetObject("Heart", resourceCulture) as byte[];
                if (iconBytes == null)
                {
                    throw new Exception($"Resource '{resourceName}' and fallback 'Heart' not found.");
                }
            }

            using (var stream = new MemoryStream(iconBytes))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
        private void RefreshGrids()
        {
            if (Application.Current.Windows.OfType<ToyWindow>().FirstOrDefault() is ToyWindow toyWindow)
            {
                toyWindow.RefreshGrid(); // Refresh ToyWindow grid
            }

            if (Application.Current.Windows.OfType<FoodWindow>().FirstOrDefault() is FoodWindow foodWindow)
            {
                foodWindow.RefreshGrid(); // Refresh FoodWindow grid
            }
        }

        private void RefreshGrid()
        {
            ShopDataGrid.ItemsSource = null; // Clear current binding
            PopulateDataGrid(); // Repopulate the grid with updated data
            RefreshGrids();
        }

        private void ShopDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Handle item purchase
            if (ShopDataGrid.SelectedItem is ShopItemViewModel selectedItem)
            {
                string itemName = selectedItem.Name;
                double price = selectedItem.Price;

                // Check if the player has enough money
                if (ViewModel.Money.Amount >= price)
                {
                    ViewModel.Money.SubtractMoney(price);
                    Console.WriteLine($"Purchased {itemName} for ${price:0.00}. Remaining balance: ${ViewModel.Money.Amount:0.00}");

                    // Update SharedInventory
                    SharedInventory.UpdateItem(itemName, 1);
                    Console.WriteLine($"Updated SharedInventory: {itemName} now has {SharedInventory.ItemsDictionary[itemName]} quantity.");

                    // Save to file
                    string itemsFile = Globals.AppWide.ApplicationData + "/items.txt";
                    SharedInventory.SaveToFile(itemsFile);

                    // Display floating text
                    ShowFloatingText($"-{price:F2}", MainGrid.ActualWidth, MainGrid.ActualHeight);

                    // Refresh the grid to reflect updated quantities
                    RefreshGrid();
                }
                else
                {
                    Console.WriteLine($"Not enough money to purchase {itemName}. Required: ${price:0.00}, Available: ${ViewModel.Money.Amount:0.00}");
                }
            }
        }

        private void ShowFloatingText(string text, double moneyDockWidth, double moneyDockHeight)
        {
            // Create the TextBlock dynamically
            var floatingText = new TextBlock
            {
                Text = text,
                FontFamily = new System.Windows.Media.FontFamily("Unispace"),
                FontSize = 18,
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red),
                Opacity = 1.0
            };

            // Calculate starting position (align above numerical balance)
            double startX = moneyDockWidth - 64; // Position horizontally
            double startY = moneyDockHeight - 50; // Position vertically

            // Position the text block
            Canvas.SetLeft(floatingText, startX);
            Canvas.SetTop(floatingText, startY);

            // Add the TextBlock to the FloatingTextCanvas
            FloatingTextCanvas.Children.Add(floatingText);

            // Create upward movement animation
            var moveUpAnimation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = startY,
                To = startY - 50, // Move upward by 50 pixels
                Duration = TimeSpan.FromSeconds(1),
                FillBehavior = System.Windows.Media.Animation.FillBehavior.HoldEnd
            };

            // Create fade-out animation
            var fadeOutAnimation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 1.0, // Fully visible
                To = 0.0, // Fully transparent
                Duration = TimeSpan.FromSeconds(2),
                BeginTime = TimeSpan.FromSeconds(1) // Start fading after 1 second
            };

            // Apply animations
            floatingText.BeginAnimation(Canvas.TopProperty, moveUpAnimation);
            floatingText.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);

            // Schedule removal of the text block
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3) // Wait for animations to complete
            };
            timer.Tick += (s, e) =>
            {
                FloatingTextCanvas.Children.Remove(floatingText);
                timer.Stop();
            };
            timer.Start();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
