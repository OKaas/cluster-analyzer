using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zcu.Graphics.Clustering;

namespace ClusterViewer
{
    public class ClusterEngine
    {
        public static string InputFile;
        public static DataSet Data;
        public static Vertex[] Vertices;
        public static Vertex[] VerticesToCluster;
        public static BoundingBox BoundingCluster;
        public static BoundingBox Bounding;

        public static List<Facility> Cluster(string outputFile, double param1, double param2)
        {
            IClustering fac = new MShift();

            // add some properties to particular clustering algorithms
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("boundingBox", BoundingCluster);
            properties.Add("PARAM_1", param1);
            properties.Add("PARAM_2", param2);

            fac.SetProperties(properties);
            fac.ComputeClustering(VerticesToCluster);

            // save clustering result
            List<Facility> facilities = fac.GetFacilities();

            // ClusterSolution.SaveClusteringSolution(fac, inputFile, Dialog_SaveCluster.FileName, facilities);
            ClusterSolution.SaveClusteringToMeshLab(fac, outputFile, facilities, VerticesToCluster);

            return facilities;
        }

        public static void LoadData(string fileName)
        {
            // save for session purposess
            InputFile = fileName;

            Data = Loader.LoadVtx(fileName, 100000);

            Vertices = Data.points;

            if (Vertices == null) { return; }

            int dim = -1;
            if (Vertices.Length > 1)
            {
                dim = Vertices[0].Dimension;
            }

            VerticesToCluster = new Vertex[Vertices.Length];

            for (int i = 0; i < Vertices.Length; ++i)
            {
                double[] scaled = new double[Vertices[0].Dimension];
                for (int j = 0; j < Vertices[0].Dimension; ++j)
                {
                    scaled[j] = (Vertices[i][j] - Data.minCorner[j]) / Data.interval[j];
                }

                VerticesToCluster[i] = new Vertex(scaled);
            }

            double[] min = new double[Vertices[0].Dimension];
            double[] max = new double[Vertices[0].Dimension];
            for (int j = 0; j < Vertices[0].Dimension; ++j)
            {
                min[j] = 0;
                max[j] = 1;
            }

            BoundingCluster = new BoundingBox();
            BoundingCluster.Initialize(min, max);


            Bounding = new BoundingBox();
            Bounding.Initialize(Data.minCorner, Data.maxCorner);
        }
    }
}
