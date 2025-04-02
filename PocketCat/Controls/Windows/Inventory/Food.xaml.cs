using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using PocketCat.Controls.Windows.Models;
using PocketCat.Controls.Windows.Shop;
using PocketCat.StaticGame;
using PocketCat.StaticGame.Items;
using PocketCat.StaticGame.Stats;

namespace PocketCat.Controls.Windows.Inventory
{
    public partial class FoodWindow : Window
    {
        private MainWindow MainWindow;
        private Flags Flags;

        public FoodWindow(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
            Flags = mainWindow.Flags;
            Flags.PropertyChanged += Flags_PropertyChanged;

            this.DataContext = this;
            InitializeComponent();

            InitializeFoodInventory();
            PopulateDataGrid();
            BindWindowToMainWindow(mainWindow);
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

        private void InitializeFoodInventory()
        {
            var foodActionData = new FoodActionData();
            foreach (var item in foodActionData.DataDict)
            {
                if (!SharedInventory.ItemsDictionary.ContainsKey(item.Key))
                {
                    SharedInventory.ItemsDictionary[item.Key] = 0; // Initialize quantity to 0
                }
            }
        }

        private void PopulateDataGrid()
        {
            var foodActionData = new FoodActionData();
            var foodItems = new List<InventoryItemViewModel>();

            foreach (var item in foodActionData.DataDict)
            {
                string quantityDisplay;

                // Check for "Dry Food" to display as "Unlimited"
                if (item.Key == "Dry Food")
                {
                    quantityDisplay = "Infinite";
                }
                else
                {
                    quantityDisplay = SharedInventory.ItemsDictionary.ContainsKey(item.Key)
                        ? SharedInventory.ItemsDictionary[item.Key].ToString()
                        : "0";
                }

                foodItems.Add(new InventoryItemViewModel
                {
                    Name = item.Key,
                    Quantity = quantityDisplay,
                    Description = item.Value,
                    Image = GetImageFromResources(item.Key) // Preserve image logic
                });
            }

            FoodGrid.ItemsSource = foodItems;
        }


        private BitmapImage GetImageFromResources(string resourceName)
        {
            var resourceManager = new ResourceManager("PocketCat.Properties.Resources", typeof(ShopItems).Assembly);
            var resourceCulture = System.Globalization.CultureInfo.CurrentCulture;

            var iconBytes = resourceManager.GetObject(resourceName, resourceCulture) as byte[];

            if (iconBytes == null)
            {
                // Fallback to "Heart" resource if the requested resource is not found
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

        private async void FoodDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.SelectedItem is InventoryItemViewModel selectedItem)
            {
                if (StatSheet.GetStat("Hunger").Value == StatSheet.MaxValue)
                {
                    Console.WriteLine("Hunger is already at maximum!", "Action Not Allowed");
                }
                else if (int.TryParse(selectedItem.Quantity, out int currentQty) && currentQty > 0)
                {
                    selectedItem.Quantity = (currentQty - 1).ToString();
                    SharedInventory.ItemsDictionary[selectedItem.Name] = currentQty - 1;

                    Console.WriteLine($"Consumed {selectedItem.Name}. Remaining quantity: {SharedInventory.ItemsDictionary[selectedItem.Name]}");

                    SaveItemsToFile();
                    RefreshGrid();

                    // Show floating text for quantity decrement
                    var rowPosition = dataGrid.TransformToVisual(FloatingTextCanvas).Transform(new Point(0, 0));
                    ShowFloatingText("-1", rowPosition.X + 226, rowPosition.Y + 62);
                    var button = new Button
                    {
                        DataContext = new KeyValuePair<string, string>(selectedItem.Name, selectedItem.Description)
                    };

                    // Call MainWindow's FeedPet method
                    MainWindow.FeedPet(button, e);
                }
                else if (selectedItem.Quantity == "Infinite")
                {
                    var button = new Button
                    {
                        DataContext = new KeyValuePair<string, string>(selectedItem.Name, selectedItem.Description)
                    };

                    // Call MainWindow's FeedPet method
                    MainWindow.FeedPet(button, e);
                    Console.WriteLine("Unlimited item used without decrementing quantity.");
                }
                else if (int.TryParse(selectedItem.Quantity, out int currentQty1) && currentQty1 <= 0)
                {
                    Console.WriteLine("The quantity for this item is already 0.", "Action Not Allowed");
                }

            }
        }

        private void ShowFloatingText(string text, double itemX, double itemY)
        {
            var floatingText = new TextBlock
            {
                Text = text,
                Foreground = Brushes.Red,
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Opacity = 1
            };

            FloatingTextCanvas.Children.Add(floatingText);

            Canvas.SetLeft(floatingText, itemX);
            Canvas.SetTop(floatingText, itemY);

            var moveUpAnimation = new DoubleAnimation
            {
                From = itemY,
                To = itemY - 50, // Move upward by 50 pixels
                Duration = TimeSpan.FromSeconds(1),
                FillBehavior = FillBehavior.HoldEnd
            };

            var fadeOutAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(1),
                AutoReverse = false
            };

            floatingText.BeginAnimation(Canvas.TopProperty, moveUpAnimation);
            floatingText.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);

            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            timer.Tick += (s, e) =>
            {
                FloatingTextCanvas.Children.Remove(floatingText);
                timer.Stop();
            };
            timer.Start();
        }

        private void SaveItemsToFile()
        {
            string filePath = Globals.AppWide.ApplicationData + "/items.txt";

            var lines = SharedInventory.ItemsDictionary.Select(kvp => $"{kvp.Key}: {kvp.Value}");
            File.WriteAllLines(filePath, lines);
        }

        public void RefreshGrid()
        {
            FoodGrid.ItemsSource = null; // Clear the grid binding
            PopulateDataGrid(); // Repopulate the grid with updated data
        }

        private void Flags_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Flags.ButtonLock) || e.PropertyName == nameof(Flags.BlinkLock))
            {
                UpdateFoodGridTransparencyBasedOnFlags();
            }
        }

        private void UpdateFoodGridTransparencyBasedOnFlags()
        {
            if (Flags.ButtonLock || Flags.BlinkLock)
            {
                SetItemsControlTransparency(FoodGrid, 0.4);
                FoodGrid.IsHitTestVisible = false;
            }
            else
            {
                SetItemsControlTransparency(FoodGrid, 1.0);
                FoodGrid.IsHitTestVisible = true;
            }
        }

        private void SetItemsControlTransparency(ItemsControl itemsControl, double opacity)
        {
            if (itemsControl != null)
            {
                itemsControl.Opacity = opacity;
            }
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
