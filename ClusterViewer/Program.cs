using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ClusterViewer
{
	static class Program
	{
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //    [STAThread]
        //    static void Main()
        //    {
        //        Application.EnableVisualStyles();
        //        Application.SetCompatibleTextRenderingDefault(false);
        //        Application.Run(new MainForm());
        //    }

        [STAThread]
        static void Main()
        {
            ClusterEngine.LoadData(@"D:\__gDrive\__PHD\Input\PointCloud\Geo_Normals_Curveture_TRIANGLE\BILO59_5g\05CSMEstimator.txt");

            ClusterEngine.Cluster(@"D:\__gDrive\__PHD\Output\ArtefactDetection\Clustering_MeanShift\MeshLab\f005.xyz", 0.05, 0.05);
        }


    }
}
