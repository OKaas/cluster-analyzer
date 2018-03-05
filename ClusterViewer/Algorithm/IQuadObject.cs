using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;

namespace ClusterViewer.Algorithm
{
    public interface IQuadObject
    {
        Rectangle Bounds { get; }
        event EventHandler BoundsChanged;
    }
}
