using System.Windows;
using System.Windows.Controls;

namespace PocketCat.Controls
{
    /// <summary>
    /// Interaction logic for HoverBox.xaml
    /// </summary>
    public partial class HoverBox : Window
    {
        public HoverBox()
        {
            InitializeComponent();
        }

        public void UpdateText(string text)
        {
            HoverTextBlock.Text = text;
        }

        public void UpdatePosition(double x, double y)
        {
            this.Left = x - (this.Width / 2); // Center horizontally
            this.Top = y - (this.Height / 2); // Center vertically
        }
    }
}
