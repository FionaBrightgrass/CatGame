using System;
using System.ComponentModel;

namespace PocketCat.StaticGame.Items
{
    public class Affection : INotifyPropertyChanged
    {
        private int _affectionValue = 0;

        public int AffectionValue
        {
            get => _affectionValue;
            set
            {
                if (_affectionValue != value)
                {
                    _affectionValue = value;
                    OnPropertyChanged(nameof(AffectionValue));
                    OnPropertyChanged(nameof(DisplayAffection));
                }
            }
        }

        public string DisplayAffection => $"{_affectionValue}";

        public void AddAffectionValue(int amount)
        {
            AffectionValue = Math.Max(AffectionValue + amount, 0);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            // Use "this" as the sender to ensure bindings work correctly
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
