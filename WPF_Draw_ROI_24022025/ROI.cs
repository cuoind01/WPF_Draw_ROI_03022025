using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WPF_Draw_ROI_24022025
{
    public enum ROIType
    {
        Rectangle,
        Ellipse
    }

    public class ROI
    {
        public ROIType Type { get; set; }
        public Rect Bounds { get; set; }
        public bool IsSelected { get; set; }
    }
}
