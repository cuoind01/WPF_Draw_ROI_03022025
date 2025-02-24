using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace WPF_Draw_ROI_03022025
{
    public class ROICanvas : Canvas
    {
        public event Action<ROI> ROIUpdated;
        public event Action<ROI> ROIClear;

        private ImageSource imageSource;
        private List<ROI> rois = new List<ROI>();
        private ROI selectedRoi;
        private Point startPoint;
        private bool isDrawing;
        private bool isDragging;
        private bool isResizing;
        private ResizeHandle currentHandle;
        private Point lastMousePosition;
        private ROIType currentRoiType;

        private enum ResizeHandle
        {
            None,
            TopLeft, Top, TopRight,
            Left, Right,
            BottomLeft, Bottom, BottomRight
        }

        public void LoadImage(string path)
        {
            try
            {
                var bitmap = new BitmapImage(new Uri(path));
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                imageSource = bitmap;
                InvalidateVisual();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}");
            }
        }

        public void StartNewROI(ROIType type)
        {
            currentRoiType = type;
            isDrawing = true;
            Cursor = Cursors.Cross;
        }

        public void ClearROIs()
        {
            rois.Clear();
            selectedRoi = null;
            ROIUpdated?.Invoke(null);
            InvalidateVisual();
        }
        public void ClearSingleROI()
        {
            if (selectedRoi == null) return;
            rois.Remove(selectedRoi);
            selectedRoi = null;
            ROIClear?.Invoke(null);
            InvalidateVisual();
        }


        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            // Draw background and image
            dc.DrawRectangle(Background, null, new Rect(0, 0, ActualWidth, ActualHeight));
            if (imageSource != null)
            {
                dc.DrawImage(imageSource, new Rect(0, 0, ActualWidth, ActualHeight));
            }

            // Draw all ROIs
            foreach (var roi in rois)
            {
                DrawROI(dc, roi);
            }
        }

        private void DrawROI(DrawingContext dc, ROI roi)
        {
            var brush = roi.IsSelected ?
                new SolidColorBrush(Color.FromArgb(96, 255, 0, 0)) :
                new SolidColorBrush(Color.FromArgb(64, 255, 0, 0));
            var pen = new Pen(Brushes.Red, 2);

            if (roi.Type == ROIType.Rectangle)
            {
                dc.DrawRectangle(brush, pen, roi.Bounds);
            }
            else // Ellipse
            {
                dc.DrawEllipse(brush, pen,
                    new Point(roi.Bounds.X + roi.Bounds.Width / 2, roi.Bounds.Y + roi.Bounds.Height / 2),
                    roi.Bounds.Width / 2, roi.Bounds.Height / 2);
            }

            if (roi.IsSelected)
            {
                DrawResizeHandles(dc, roi.Bounds);
            }
        }

        private void DrawResizeHandles_(DrawingContext dc, Rect bounds)
        {
            double handleSize = 8;
            Point[] handles = new[]
            {
                bounds.TopLeft,
                new Point(bounds.Left + bounds.Width/2, bounds.Top),
                bounds.TopRight,
                new Point(bounds.Left, bounds.Top + bounds.Height/2),
                new Point(bounds.Right, bounds.Top + bounds.Height/2),
                bounds.BottomLeft,
                new Point(bounds.Left + bounds.Width/2, bounds.Bottom),
                bounds.BottomRight
            };

            foreach (var handle in handles)
            {
                var handleRect = new Rect(
                    handle.X - handleSize / 2,
                    handle.Y - handleSize / 2,
                    handleSize,
                    handleSize);
                dc.DrawRectangle(Brushes.White, new Pen(Brushes.Red, 1), handleRect);
            }
        }
        private void DrawResizeHandles(DrawingContext dc, Rect bounds)
        {
            double handleSize = 8;
            Point[] handles = new[]
            {
                bounds.TopLeft,
                new Point(bounds.Left + bounds.Width/2, bounds.Top),
                bounds.TopRight,
                new Point(bounds.Left, bounds.Top + bounds.Height/2),
                new Point(bounds.Right, bounds.Top + bounds.Height/2),
                bounds.BottomLeft,
                new Point(bounds.Left + bounds.Width/2, bounds.Bottom),
                bounds.BottomRight
            };

            foreach (var handle in handles)
            {
                dc.DrawEllipse(Brushes.White, new Pen(Brushes.Red, 1), handle, handleSize / 2, handleSize / 2);
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            var pos = e.GetPosition(this);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (isDrawing)
                {
                    startPoint = pos;
                    var newRoi = new ROI
                    {
                        Type = currentRoiType,
                        Bounds = new Rect(startPoint, new Size(0, 0)),
                        IsSelected = true
                    };
                    rois.Add(newRoi);
                    selectedRoi = newRoi;
                }
                else
                {
                    HandleROISelection(pos);
                }
                InvalidateVisual();
            }
        }

        private void HandleROISelection(Point pos)
        {
            if (selectedRoi != null)
            {
                currentHandle = GetResizeHandle(pos, selectedRoi.Bounds);
                if (currentHandle != ResizeHandle.None)
                {
                    isResizing = true;
                    return;
                }
            }

            selectedRoi = null;
            foreach (var roi in rois)
            {
                roi.IsSelected = false;
                if (IsPointInROI(pos, roi))
                {
                    selectedRoi = roi;
                    isDragging = true;
                    lastMousePosition = pos;
                }
            }

            if (selectedRoi != null)
            {
                selectedRoi.IsSelected = true;
                ROIUpdated?.Invoke(selectedRoi);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var pos = e.GetPosition(this);

            if (isDrawing && selectedRoi != null)
            {
                UpdateROISize(pos);
            }
            else if (isDragging && selectedRoi != null)
            {
                MoveROI(pos);
                //Vector delta = pos - lastMousePosition;
                //selectedRoi.Bounds = new Rect(selectedRoi.Bounds.X + delta.X, selectedRoi.Bounds.Y + delta.Y, selectedRoi.Bounds.Width, selectedRoi.Bounds.Height);
                ////selectedRoi.Bounds.Offset(delta.X, delta.Y);
                //Console.WriteLine(delta.ToString()); 
                //lastMousePosition = pos;
                //ROIUpdated?.Invoke(selectedRoi);
            }
            else if (isResizing && selectedRoi != null)
            {
                ResizeROI(pos);
            }
            else
            {
                UpdateCursor(pos);
            }

            InvalidateVisual();
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            isDrawing = false;
            isDragging = false;
            isResizing = false;
            currentHandle = ResizeHandle.None;

            if (selectedRoi != null)
            {
                ROIUpdated?.Invoke(selectedRoi);
            }

            UpdateCursor(e.GetPosition(this));
        }

        private void UpdateROISize(Point currentPoint)
        {
            double x = Math.Min(currentPoint.X, startPoint.X);
            double y = Math.Min(currentPoint.Y, startPoint.Y);
            double width = Math.Abs(currentPoint.X - startPoint.X);
            double height = Math.Abs(currentPoint.Y - startPoint.Y);

            selectedRoi.Bounds = new Rect(x, y, width, height);
            ROIUpdated?.Invoke(selectedRoi);
        }

        private void MoveROI(Point currentPoint)
        {
            Vector delta = currentPoint - lastMousePosition;
            selectedRoi.Bounds = new Rect(selectedRoi.Bounds.X + delta.X, selectedRoi.Bounds.Y + delta.Y, selectedRoi.Bounds.Width, selectedRoi.Bounds.Height);
            //selectedRoi.Bounds.Offset(delta.X, delta.Y);
            lastMousePosition = currentPoint;
            ROIUpdated?.Invoke(selectedRoi);
        }

        private void ResizeROI(Point currentPoint)
        {
            var bounds = selectedRoi.Bounds;
            double x = bounds.X;
            double y = bounds.Y;
            double width = bounds.Width;
            double height = bounds.Height;

            switch (currentHandle)
            {
                case ResizeHandle.TopLeft:
                    width = bounds.Right - currentPoint.X;
                    height = bounds.Bottom - currentPoint.Y;
                    x = currentPoint.X;
                    y = currentPoint.Y;
                    break;
                case ResizeHandle.Top:
                    height = bounds.Bottom - currentPoint.Y;
                    y = currentPoint.Y;
                    break;
                case ResizeHandle.TopRight:
                    width = currentPoint.X - bounds.Left;
                    height = bounds.Bottom - currentPoint.Y;
                    y = currentPoint.Y;
                    break;
                case ResizeHandle.Left:
                    width = bounds.Right - currentPoint.X;
                    x = currentPoint.X;
                    break;
                case ResizeHandle.Right:
                    width = currentPoint.X - bounds.Left;
                    break;
                case ResizeHandle.BottomLeft:
                    width = bounds.Right - currentPoint.X;
                    height = currentPoint.Y - bounds.Top;
                    x = currentPoint.X;
                    break;
                case ResizeHandle.Bottom:
                    height = currentPoint.Y - bounds.Top;
                    break;
                case ResizeHandle.BottomRight:
                    width = currentPoint.X - bounds.Left;
                    height = currentPoint.Y - bounds.Top;
                    break;
            }

            if (width > 0 && height > 0)
            {
                selectedRoi.Bounds = new Rect(x, y, width, height);
                ROIUpdated?.Invoke(selectedRoi);
            }
        }

        private ResizeHandle GetResizeHandle(Point point, Rect bounds)
        {
            double threshold = 10;
            var handles = new Dictionary<ResizeHandle, Point>
            {
                { ResizeHandle.TopLeft, bounds.TopLeft },
                { ResizeHandle.Top, new Point(bounds.Left + bounds.Width/2, bounds.Top) },
                { ResizeHandle.TopRight, bounds.TopRight },
                { ResizeHandle.Left, new Point(bounds.Left, bounds.Top + bounds.Height/2) },
                { ResizeHandle.Right, new Point(bounds.Right, bounds.Top + bounds.Height/2) },
                { ResizeHandle.BottomLeft, bounds.BottomLeft },
                { ResizeHandle.Bottom, new Point(bounds.Left + bounds.Width/2, bounds.Bottom) },
                { ResizeHandle.BottomRight, bounds.BottomRight }
            };

            foreach (var handle in handles)
            {
                if (IsNearPoint(point, handle.Value, threshold))
                    return handle.Key;
            }

            return ResizeHandle.None;
        }

        private bool IsPointInROI(Point point, ROI roi)
        {
            if (roi.Type == ROIType.Rectangle)
            {
                return roi.Bounds.Contains(point);
            }
            else // Ellipse
            {
                var center = new Point(
                    roi.Bounds.X + roi.Bounds.Width / 2,
                    roi.Bounds.Y + roi.Bounds.Height / 2);
                var rx = roi.Bounds.Width / 2;
                var ry = roi.Bounds.Height / 2;

                if (rx <= 0 || ry <= 0) return false;

                var dx = (point.X - center.X) / rx;
                var dy = (point.Y - center.Y) / ry;
                return (dx * dx + dy * dy) <= 1;
            }
        }

        private bool IsNearPoint(Point point1, Point point2, double threshold)
        {
            return Math.Abs(point1.X - point2.X) <= threshold &&
                   Math.Abs(point1.Y - point2.Y) <= threshold;
        }

        private void UpdateCursor(Point position)
        {
            if (selectedRoi != null)
            {
                var handle = GetResizeHandle(position, selectedRoi.Bounds);
                switch (handle)
                {
                    case ResizeHandle.TopLeft:
                    case ResizeHandle.BottomRight:
                        Cursor = Cursors.SizeNWSE;
                        break;
                    case ResizeHandle.TopRight:
                    case ResizeHandle.BottomLeft:
                        Cursor = Cursors.SizeNESW;
                        break;
                    case ResizeHandle.Top:
                    case ResizeHandle.Bottom:
                        Cursor = Cursors.SizeNS;
                        break;
                    case ResizeHandle.Left:
                    case ResizeHandle.Right:
                        Cursor = Cursors.SizeWE;
                        break;
                    default:
                        Cursor = IsPointInROI(position, selectedRoi) ?
                            Cursors.SizeAll : Cursors.Cross;
                        break;
                }
            }
            else
            {
                Cursor = Cursors.Cross;
            }
        }
    }
}
