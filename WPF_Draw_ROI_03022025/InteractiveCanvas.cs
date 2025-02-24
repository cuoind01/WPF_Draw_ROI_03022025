using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPF_Draw_ROI_03022025
{
    public class InteractiveCanvas : Canvas
    {
        public InteractiveCanvas()
        {
            this.MouseDown += InteractiveCanvas_MouseDown; ;
            this.MouseUp += InteractiveCanvas_MouseUp;
            this.MouseMove += InteractiveCanvas_MouseMove;
            this.ClipToBounds = true;
            this.Cursor = Cursors.Cross;
        }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            dc.DrawRectangle(Brushes.Gray, null, new Rect(0, 0, ActualWidth, ActualHeight));
            if (imageSource != null)
            {
                dc.DrawImage(imageSource, new Rect(0, 0, ActualWidth, ActualHeight));
            }
            if (!roiRectangle.IsEmpty)
            {
                dc.DrawRectangle(

                    new SolidColorBrush(Color.FromArgb(54, 255, 0, 0)),
                    new Pen(Brushes.Red, 2),
                    roiRectangle);

                DrawResizeHandle(dc, roiRectangle.TopLeft);
                DrawResizeHandle(dc, roiRectangle.TopRight);
                DrawResizeHandle(dc, roiRectangle.BottomLeft);
                DrawResizeHandle(dc, roiRectangle.BottomRight);
            }
        }
        public void LoadImage(string imagePath)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                imageSource = bitmap;
                //imageSource = (new OpenCvSharp.Mat(imagePath, OpenCvSharp.ImreadModes.Color)).ToWriteableBitmap();
                InvalidateVisual();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}");
            }
        }
        public void ClearROI()
        {
            roiRectangle = Rect.Empty;
            RoiChanged?.Invoke(roiRectangle);
            InvalidateVisual();
        }
        private void DrawResizeHandle(DrawingContext dc, Point p)
        {
            double handlesize = 8;
            var handRect = new Rect(
                p.X - handlesize / 2,
                p.Y - handlesize / 2,
                handlesize,
                handlesize);
            dc.DrawRectangle(Brushes.White, new Pen(Brushes.Red, 2), handRect);
        }
        private void InteractiveCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var pos = e.GetPosition(this);

            if (!isDragging && !isResizing && isDrawing)
            {
                UpdateCursor(pos);
            }
            if (isDrawing)
            {
                double x = Math.Min(pos.X, startPoint.X);
                double y = Math.Min(pos.Y, startPoint.Y);
                double w = Math.Abs(pos.X - startPoint.X);
                double h = Math.Abs(pos.Y - startPoint.Y);
                roiRectangle = new Rect(x, y, w, h);
            }
            else if (isDragging)
            {
                Vector delta = pos - lastMousePostion;
                roiRectangle.Offset(delta.X, delta.Y);
                Console.WriteLine(  delta.ToString());
                lastMousePostion = pos;
            }
            else if (isResizing)
            {
                ResizeROI(pos);
            }
             RoiChanged?.Invoke(roiRectangle);
            InvalidateVisual();
        }

        private void InteractiveCanvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                currentResizeMode = GetResizeMode(pos);
                if (currentResizeMode != ResizeMode.None)
                {
                    isResizing = true;
                }
                else
                if (roiRectangle.Contains(pos))
                {
                    isDragging = true;
                    lastMousePostion = pos;

                }
                else
                {
                    isDrawing = true;
                    startPoint = pos;
                    roiRectangle = new Rect(startPoint, startPoint);
                }
            }
            CaptureMouse();
            InvalidateVisual();
        }

        private void InteractiveCanvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isDrawing = isDragging = isResizing = false;
            currentResizeMode = ResizeMode.None;
            ReleaseMouseCapture();
            UpdateCursor(e.GetPosition(this));
        }

        private ResizeMode GetResizeMode(Point p)
        {
            double threshold = 10;
            if (IsNearPoint(p, roiRectangle.TopLeft, threshold)) return ResizeMode.Topleft;
            if (IsNearPoint(p, roiRectangle.TopRight, threshold)) return ResizeMode.TopRight;
            if (IsNearPoint(p, roiRectangle.BottomLeft, threshold)) return ResizeMode.BottomLeft;
            if (IsNearPoint(p, roiRectangle.BottomRight, threshold)) return ResizeMode.BottomRight;
            return ResizeMode.None;
        }
        private bool IsNearPoint(Point p1, Point p2, double threshold)
        {
            return Math.Abs(p1.X - p2.X) < threshold && Math.Abs(p1.Y - p2.Y) < threshold;
        }
        private void UpdateCursor(Point p)
        {
            if (roiRectangle.Contains(p))
            {
                switch (GetResizeMode(p))
                {
                    case ResizeMode.Topleft:
                    case ResizeMode.BottomRight:
                        this.Cursor = Cursors.SizeNWSE;
                        break;
                    case ResizeMode.TopRight:
                    case ResizeMode.BottomLeft:
                        this.Cursor = Cursors.SizeNESW;
                        break;
                    default:
                        this.Cursor = Cursors.SizeAll;
                        break;
                }
            }
            else
            {
                Cursor = Cursors.Cross;
            }
        }
        private void ResizeROI(Point p)
        {
            double x = roiRectangle.X;
            double y = roiRectangle.Y;
            double w = roiRectangle.Width;
            double h = roiRectangle.Height;
            switch (currentResizeMode)
            {
                case ResizeMode.None:
                    break;
                case ResizeMode.Topleft:
                    w = roiRectangle.Right - p.X;
                    h = roiRectangle.Bottom - p.Y;
                    x = p.X;
                    y = p.Y;
                    break;
                case ResizeMode.TopRight:
                    w = p.X - roiRectangle.Left;
                    h = roiRectangle.Bottom - p.Y;
                    y = p.Y;
                    break;
                case ResizeMode.BottomLeft:
                    w = roiRectangle.Right - p.X;
                    h = p.Y - roiRectangle.Top;
                    x = p.X;
                    break;
                case ResizeMode.BottomRight:
                    w = p.X - roiRectangle.Left;
                    h = roiRectangle.Bottom - p.Y;
                    break;
                default:
                    break;

            }
            if (w > 0 && h > 0)
            {
                roiRectangle = new Rect(x, y, w, h);
            }
        }
        #region Methods

        #endregion

        #region Properties
        public event Action<Rect> RoiChanged;

        #endregion
        #region Fields
        private Point startPoint, lastMousePostion;
        private Rect roiRectangle;
        private bool isDrawing, isDragging, isResizing;
        private ImageSource imageSource;

        private ResizeMode currentResizeMode = ResizeMode.None;
        private enum ResizeMode
        {
            None, Topleft, TopRight, BottomLeft, BottomRight
        }



        #endregion
    }
}
