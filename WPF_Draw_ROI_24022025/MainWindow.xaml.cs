using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WPF_Draw_ROI_24022025
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer timer;
        private const string CurrentUser = "cuoind01";

        public MainWindow()
        {
            InitializeComponent();
            InitializeContextMenu();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
            timer.Start();

            userDisplay.Text = $"Current User's Login: {CurrentUser}";
            roiCanvas.ROIUpdated += UpdateROIInfo;
            UpdateTime();
        }

        private void InitializeContextMenu()
        {
            var menu = new ContextMenu();

            var loadImageItem = new MenuItem { Header = "Load Image" };
            loadImageItem.Click += LoadImage_Click;

            var addRoiMenu = new MenuItem { Header = "Add ROI" };
            var addRectangleItem = new MenuItem { Header = "Rectangle" };
            addRectangleItem.Click += (s, e) => roiCanvas.StartNewROI(ROIType.Rectangle);
            var addEllipseItem = new MenuItem { Header = "Ellipse" };
            addEllipseItem.Click += (s, e) => roiCanvas.StartNewROI(ROIType.Ellipse);

            addRoiMenu.Items.Add(addRectangleItem);
            addRoiMenu.Items.Add(addEllipseItem);

            var clearRoisItem = new MenuItem { Header = "Clear All ROIs" };
            clearRoisItem.Click += (s, e) => roiCanvas.ClearROIs();

            menu.Items.Add(loadImageItem);
            menu.Items.Add(new Separator());
            menu.Items.Add(addRoiMenu);
            menu.Items.Add(clearRoisItem);

            roiCanvas.ContextMenu = menu;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateTime();
        }

        private void UpdateTime()
        {
            timeDisplay.Text = $"Current Date and Time (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
        }

        private void UpdateROIInfo(ROI roi)
        {
            if (roi != null)
            {
                roiInfoDisplay.Text = $"ROI: Type={roi.Type}, X={roi.Bounds.X:F0}, Y={roi.Bounds.Y:F0}, W={roi.Bounds.Width:F0}, H={roi.Bounds.Height:F0}";
            }
            else
            {
                roiInfoDisplay.Text = "No ROI selected";
            }
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                roiCanvas.LoadImage(dialog.FileName);
            }
        }
    }
}
