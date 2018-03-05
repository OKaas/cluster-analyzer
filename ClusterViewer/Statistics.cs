using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Zcu.Graphics.Clustering;

namespace ClusterViewer
{
    public partial class Statistics : Form
    {
        public Statistics(BoundingBox box, Facility[] facilities, Vertex[] vertices)
        {
            InitializeComponent();

            text_Box.Text = "Time: " + DateTime.Now.ToString("h:mm:ss tt");
            text_Box.Text += "\nVertices: "+vertices.Length;

            // dummy counting of facilities
            int facCount = 0;
            foreach (Vertex v in vertices)
            {
                if ( v.IsFacility ){ ++facCount; }
            }
            text_Box.Text += "\nFacilities: " + facCount;
            text_Box.Text += "\nClustering performance: " + Analysis.ComputeClusterSolution(box, facilities, vertices);

            // add all statistics whatever you will need

            this.Show();
        }
    }
}
