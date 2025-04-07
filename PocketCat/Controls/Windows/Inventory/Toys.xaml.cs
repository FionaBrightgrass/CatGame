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

namespace PocketCat.Controls.Windows.Inventory
{
    public partial class ToyWindow : Window
    {
        private MainWindow MainWindow;
        private Flags Flags;

        public ToyWindow(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
            Flags = mainWindow.Flags;
            Flags.PropertyChanged += Flags_PropertyChanged;

            this.DataContext = this;
            InitializeComponent();

            InitializeToyInventory();
            PopulateDataGrid();
            BindWindowToMainWindow(mainWindow);
            mainWindow.AppSettings.PropertyChanged += AppSettings_PropertyChanged;
        }

        private void AppSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            InvalidateVisual();
            UpdateLayout();
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

        private void InitializeToyInventory()
        {
            var itemActionData = new ItemActionData();
            foreach (var item in itemActionData.DataDict)
            {
                if (!SharedInventory.ItemsDictionary.ContainsKey(item.Key))
                {
                    SharedInventory.ItemsDictionary[item.Key] = 0; // Initialize quantity to 0
                }
            }
        }

        private void PopulateDataGrid()
        {
            var itemActionData = new ItemActionData();
            var toyItems = new List<InventoryItemViewModel>();

            foreach (var item in itemActionData.DataDict)
            {
                // Filter only toy items
                if (item.Value.Contains("Category: Toys"))
                {
                    toyItems.Add(new InventoryItemViewModel
                    {
                        Name = item.Key,
                        Quantity = SharedInventory.ItemsDictionary.ContainsKey(item.Key)
                            ? SharedInventory.ItemsDictionary[item.Key].ToString()
                            : "0",
                        Description = item.Value,
                        Image = GetImageFromResources(item.Key)
                    });
                }
            }

            ToysGrid.ItemsSource = toyItems; // Bind toy-specific items to the grid
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

        private async void ToysGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.SelectedItem is InventoryItemViewModel selectedItem)
            {
                var itemActionData = new ItemActionData();

                if (int.TryParse(selectedItem.Quantity, out int currentQty) && currentQty > 0)
                {
                    if (itemActionData.DataDict.TryGetValue(selectedItem.Name, out string actionData))
                    {
                        var breakChanceText = actionData.Split(";")
                                                         .FirstOrDefault(s => s.Contains("BreakChance"))
                                                         ?.Split(":")[1]
                                                         ?.Trim();

                        if (double.TryParse(breakChanceText, out double breakChance))
                        {
                            Random random = new Random();

                            if (random.NextDouble() <= breakChance)
                            {
                                SharedInventory.ItemsDictionary[selectedItem.Name]--;
                                selectedItem.Quantity = (currentQty - 1).ToString();

                                SaveItemsToFile();
                                RefreshGrid();

                                Console.WriteLine($"{selectedItem.Name} broke!");

                                // Show floating text for quantity decrement
                                var rowPosition = dataGrid.TransformToVisual(FloatingTextCanvas).Transform(new Point(0, 0));
                                ShowFloatingText("-1", rowPosition.X + 226, rowPosition.Y + 62);
                                var button = new Button
                                {
                                    DataContext = new KeyValuePair<string, string>(selectedItem.Name, selectedItem.Description)
                                };

                                MainWindow.PlayPet(button, e);
                            }
                        }
                    }
                }
                else if (selectedItem.Quantity == "Unlimited")
                {
                    Console.WriteLine("Unlimited item used without decrementing quantity.");
                }
                else if (int.TryParse(selectedItem.Quantity, out int currentQty1) && currentQty1 <= 0)
                {
                    Console.WriteLine("The quantity for this item is already 0.");
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
            ToysGrid.ItemsSource = null; // Clear the grid binding
            PopulateDataGrid(); // Repopulate the grid with updated data
        }

        private void Flags_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Flags.ButtonLock) || e.PropertyName == nameof(Flags.BlinkLock))
            {
                UpdateToysGridTransparencyBasedOnFlags();
            }
        }

        private void UpdateToysGridTransparencyBasedOnFlags()
        {
            if (Flags.ButtonLock || Flags.BlinkLock)
            {
                SetItemsControlTransparency(ToysGrid, 0.4);
                ToysGrid.IsHitTestVisible = false;
            }
            else
            {
                SetItemsControlTransparency(ToysGrid, 1.0);
                ToysGrid.IsHitTestVisible = true;
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
            this.Visibility = Visibility.Collapsed;
        }
    }
}
