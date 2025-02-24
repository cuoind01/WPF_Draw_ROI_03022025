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

namespace WPF_Draw_ROI_03022025
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer timer2;
        private const string CurrentUser = "cuoind01";
        public MainWindow()
        {
            InitializeComponent();
            InitializeContextMenu();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            stsDisplay.Text = $"User: Khoa";
            ImageCanvas.RoiChanged += ImageCanvas_RoiChanged;
            UpdateTime();

            InitializeContextMenu2();
            timer2 = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer2.Tick += Timer_Tick;
            timer2.Start();

            userDisplay.Text = $"Current User's Login: {CurrentUser}";
            roiCanvas.ROIUpdated += UpdateROIInfo;
            UpdateTime2();
        }

        private void UpdateTime()
        {
            stsTime.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
        }

        private void ImageCanvas_RoiChanged(Rect roi)
        {
            if (!roi.IsEmpty)
            {
                stsInforDisplay.Text = $"ROI: X={roi.X:F0}, Y={roi.Y:F0}, W={roi.Width:F0}, H={roi.Height:F0}";
            }
            else
            {
                stsInforDisplay.Text = "ROI: None";
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateTime();
        }

        private void InitializeContextMenu()
        {
            var menucontext = new ContextMenu();    
            var menuItem = new MenuItem() { Header  = "Load Image"};
            menuItem.Click += LoadImage_Click; 
            var clearROI = new MenuItem() { Header = "Clear ROI" };
            clearROI.Click += ClearROI_Click; 
            menucontext.Items.Add(menuItem);
            menucontext.Items.Add(clearROI);
            ImageCanvas.ContextMenu = menucontext;
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ImageCanvas.LoadImage(openFileDialog.FileName);
            }
        }
        private void ClearROI_Click(object sender, RoutedEventArgs e)
        {
            ImageCanvas.ClearROI();
        }

        private void InitializeContextMenu2()
        {
            var menu = new ContextMenu();

            var loadImageItem = new MenuItem { Header = "Load Image" };
            loadImageItem.Click += LoadImage2_Click;

            var addRoiMenu = new MenuItem { Header = "Add ROI" };
            var addRectangleItem = new MenuItem { Header = "Rectangle" };
            addRectangleItem.Click += (s, e) => roiCanvas.StartNewROI(ROIType.Rectangle);
            var addEllipseItem = new MenuItem { Header = "Ellipse" };
            addEllipseItem.Click += (s, e) => roiCanvas.StartNewROI(ROIType.Ellipse);

            addRoiMenu.Items.Add(addRectangleItem);
            addRoiMenu.Items.Add(addEllipseItem);

            var clearRoiSingleItem = new MenuItem { Header = "Clear ROI" };
            clearRoiSingleItem.Click += (s, e) => roiCanvas.ClearSingleROI();

            var clearRoisItem = new MenuItem { Header = "Clear All ROIs" };
            clearRoisItem.Click += (s, e) => roiCanvas.ClearROIs();

            menu.Items.Add(loadImageItem);
            menu.Items.Add(new Separator());
            menu.Items.Add(addRoiMenu);
            menu.Items.Add(clearRoiSingleItem);
            menu.Items.Add(clearRoisItem);

            roiCanvas.ContextMenu = menu;
        }

        private void Timer_Tick2(object sender, EventArgs e)
        {
            UpdateTime2();
        }

        private void UpdateTime2()
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

        private void LoadImage2_Click(object sender, RoutedEventArgs e)
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
