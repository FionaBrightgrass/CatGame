using System.Drawing;
using System.IO;
using System.Resources;
using System.Windows.Forms;

namespace PocketCat.Controls
{
    public class CatTrayIcon : IDisposable
    {
        private NotifyIcon _notifyIcon;

        public CatTrayIcon()
        {
            Icon iconObject = GetIconFromResources("CatIcon");
            // Initialize NotifyIcon
            _notifyIcon = new NotifyIcon
            {
                Icon = iconObject, // Load the embedded .ico file
                Visible = true,
                Text = "My System Tray Application"
            };

            // Create Context Menu
            var contextMenu = new ContextMenuStrip();
            //contextMenu.Items.Add("Open", null, (s, e) => onOpenAction?.Invoke());
            //contextMenu.Items.Add("Exit", null, (s, e) => onExitAction?.Invoke());

            // Assign context menu to NotifyIcon
            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.Visible = true;
            // Handle double-click event
            //_notifyIcon.DoubleClick += (s, e) => onOpenAction?.Invoke();
        }


        private Icon GetIconFromResources(string resourceName)
        {
            // Load the resource using ResourceManager
            var resourceManager = new ResourceManager("PocketCat.Properties.Resources", typeof(CatTrayIcon).Assembly);
            var resourceCulture = System.Globalization.CultureInfo.CurrentCulture;

            // Retrieve the resource as a byte array
            var iconBytes = resourceManager.GetObject(resourceName, resourceCulture) as byte[];
            if (iconBytes == null)
            {
                throw new Exception($"Resource '{resourceName}' not found or is not valid.");
            }

            // Convert the byte array to an Icon using MemoryStream
            using (var stream = new MemoryStream(iconBytes))
            {
                return new Icon(stream);
            }
        }

        public void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
        {
            _notifyIcon.ShowBalloonTip(5000, title, message, icon);
        }

        public void Dispose()
        {
            // Clean up NotifyIcon
            _notifyIcon?.Dispose();
        }
    }
}
