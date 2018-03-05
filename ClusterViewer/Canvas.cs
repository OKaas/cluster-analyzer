using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Zcu.Graphics.Clustering;

namespace ClusterViewer
{
    public partial class Canvas : UserControl
    {

        public Canvas()
        {
            InitializeComponent();

            //graph.GraphPane.Legend.IsVisible = false;
            //graph.GraphPane.Title.IsVisible = false;
            //graph.GraphPane.XAxis.IsVisible = false;
            //graph.GraphPane.YAxis.IsVisible = false;
            //graph.GraphPane.X2Axis.IsVisible = false;
            //graph.GraphPane.Y2Axis.IsVisible = false;
        }

        public void Draw()
        {
            //ILArray<float> terr = ILMath.tosingle(ILSpecialData.terrain["20:40;20:45"]);

            //CreateScatterplot(graph);

            //graph.Invalidate();
        }

        private void Canvas_Load(object sender, EventArgs e)
        {
            
        }
    }
}
