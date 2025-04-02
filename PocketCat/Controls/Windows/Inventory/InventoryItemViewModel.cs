using System.ComponentModel;
using System.Windows.Media.Imaging;

public class InventoryItemViewModel : INotifyPropertyChanged
{
    private string _quantity;

    public string Name { get; set; }

    public string Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }
    }

    public string Description { get; set; }
    public BitmapImage Image { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
