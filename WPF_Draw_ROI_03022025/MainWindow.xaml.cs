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
    }
}
