using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Zcu.Graphics.Clustering
{
    public static class ClusterSolution
    {
        private const string SEPARATOR_HEADER = "================================================";

        public static double ToDouble(this Color @this)
        {

            CommonDenominatorBetweenColoursAndDoubles denom = new CommonDenominatorBetweenColoursAndDoubles();

            denom.R = (byte)@this.R;
            denom.G = (byte)@this.G;
            denom.B = (byte)@this.B;

            double result = denom.AsDouble;
            return result;

        }

        public static Color ToColor(this double @this)
        {

            CommonDenominatorBetweenColoursAndDoubles denom = new CommonDenominatorBetweenColoursAndDoubles();

            denom.AsDouble = @this;

            Color color = Color.FromArgb(
                red: denom.R,
                green: denom.G,
                blue: denom.B
            );
            return color;

        }

        public static bool SaveClusteringToMeshLab(IClustering algorithm, string outputFile, List<Facility> facilities, Vertex[] points)
        {
            if (facilities == null) { return false; }

            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                //ClusteringInfo info = new ClusteringInfo();
                //info.date = DateTime.Now;
                //info.inputFile = inputFile;
                //info.outputFile = outputFile;

                //// write some information header
                //writer.WriteLine(JsonConvert.SerializeObject(info));
                //writer.WriteLine(algorithm.GetInfo());

                //writer.WriteLine(SEPARATOR_HEADER);

                // writer.WriteLine("Cl, x, y, z, nx, ny, nz, k1, k2");

                for ( int i = 0; i < facilities.Count; ++i )
                {
                    // writer.Write("{0}", i);

                    Color colorCluster = ToColor( ((double)i) / facilities.Count);

                    for (int dim = 0; dim < 3; ++dim)
                    {
                        writer.Write(" {0}", facilities[i].Coords[dim]);
                    }

                    writer.Write(" {0} {1} {2}", colorCluster.R, colorCluster.G, colorCluster.B);

                    writer.Write("\n");

                    foreach(int indexClient in facilities[i].VertexIndices)
                    {
                        // writer.Write("{0}", i);

                        Vertex client = points[indexClient];

                        for (int dim = 0; dim < 3; ++dim)
                        {
                            writer.Write(" {0}", client.Coords[dim]);
                        }

                        writer.Write(" {0} {1} {2}", colorCluster.R, colorCluster.G, colorCluster.B);

                        writer.Write("\n");
                    }
                }
                writer.WriteLine();
            }

            return true;
        }

        public static bool SaveClusteringSolution(IClustering algorithm, string inputFile, string outputFile, List<Facility> facilities)
        {
            if (facilities == null) { return false; }

            using ( StreamWriter writer = new StreamWriter(outputFile) )
            {
                ClusteringInfo info = new ClusteringInfo();
                info.date = DateTime.Now;
                info.inputFile = inputFile;
                info.outputFile = outputFile;

                // write some information header
                writer.WriteLine(JsonConvert.SerializeObject(info));
                writer.WriteLine(algorithm.GetInfo());

                writer.WriteLine(SEPARATOR_HEADER);

                // write structure it self
                foreach (Facility f in facilities)
                {
                    writer.WriteLine(JsonConvert.SerializeObject(f));
                }
            }
            
            return true;
        }

        public static List<Facility> LoadClusteringSolution(string inputFileClusteringResult, out string inputFileVertices)
        {
            List<Facility> facilities = null;
            inputFileVertices = null;
            using ( StreamReader reader = new StreamReader(inputFileClusteringResult) )
            {
                ClusteringInfo info = JsonConvert.DeserializeObject<ClusteringInfo>(reader.ReadLine());
                inputFileVertices = info.inputFile;

                // skip to separator header
                while (!reader.ReadLine().Equals(SEPARATOR_HEADER));

                string line;
                facilities = new List<Facility>();
                while ((line = reader.ReadLine()) != null)
                {
                    facilities.Add(JsonConvert.DeserializeObject<Facility>(line));
                }
            }

            return facilities;
        }
    }
}
