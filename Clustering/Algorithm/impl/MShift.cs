using Clustering.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Accord.Math;
using Accord.Statistics.Distributions.DensityKernels;
using Accord.Math.Distances;
using Accord.MachineLearning;
using System.Collections;

namespace Zcu.Graphics.Clustering
{
    public class MShift : IClustering
    {
        double PARAM_1 = 0.5;
        double PARAM_2 = 1;
        double EPSILON_DEFAULT = 0.00001;

        /// <summary>
        /// Array of vertices.
        /// </summary>
        private Vertex[] vertices = null;

        private Vertex[] shiftedPoints = null;

        /// <summary>
        /// Array of open facilities.
        /// </summary>
        private List<Facility> facilities = new List<Facility>();

        private bool[] stopMoving;

        private IDensityKernel Kernel;
        private double Bandwidth;

        public int ComputeClustering(Vertex[] points)
        {
            int pixelSize = 3;

            // Retrieve the kernel bandwidth
            // double sigma = (double)numBandwidth.Value;

            // Create a MeanShift algorithm using the given bandwidth
            // and a Gaussian density kernel as the kernel function:

            Accord.Compat.ParallelOptions opt = new Accord.Compat.ParallelOptions();
            opt.MaxDegreeOfParallelism = 1;

            var meanShift = new MeanShift()
            {
                Kernel = new GaussianKernel( (int) PARAM_2),
                Bandwidth = PARAM_1,
                ComputeLabels = true,

                // Please set ParallelOptions.MaxDegreeOfParallelism to 1 instead.

                ParallelOptions = opt
                //Tolerance = 0.05,
                // MaxIterations = (int)ITERATION
            };

            double[][] input = new double[points.Length][];

            for ( int i = 0; i < points.Length; ++i )
            {
                input[i] = points[i].Coords;
            }

            // Compute the mean-shift algorithm until the difference 
            // in shift vectors between two iterations is below 0.05

            int[] classification = meanShift.Learn(input).Decide(input);

            Hashtable ret = new Hashtable();
            Hashtable wut = new Hashtable();

            for ( int i = 0; i < input.Length; ++i )
            {
                int idCluster = classification[i];

                if ( ret[idCluster] == null )
                {
                    Facility newFac = new Facility();
                    newFac.VertexIndex = i;
                    newFac.Coords = meanShift.Clusters.Modes[idCluster];

                    ret.Add(idCluster, newFac);

                    points[i].IsFacility = true;
                    points[i].Facility = newFac;

                } else
                {
                    ((Facility)ret[idCluster]).AddVertex(i, 0);
                }
            }

            facilities.Clear();

            Console.WriteLine("===============================");

            // WTF
            foreach (Facility f in ret.Values)
            {
                //f.Coords = new double[points[0].Dimension];
                //foreach (int v in f.VertexIndices)
                //{

                //    for (int i = 0; i < f.Coords.Length; ++i)
                //    {
                //        f.Coords[i] += points[v][i];
                //    }

                //    for (int i = 0; i < f.Coords.Length; ++i)
                //    {
                //        f.Coords[i] /= f.VertexIndices.Count;
                //    }
                //}

                facilities.Add(f);

                Console.WriteLine("Clients: {0}", f.VertexIndices.Count);
            }

            return 0;
        }

        public double euclidean_distance(Vertex point_a, Vertex point_b) {
	        double total = 0;
	        for (int i = 0; i<point_a.Dimension; i++) {
		        double temp = (point_a.Coords[i] - point_a.Coords[i]);
                total += temp* temp;
            }
	        return Math.Sqrt(total);
        }

        public bool PrepareStructures(ref List<Facility> facilities, ref Vertex[] vertices)
        {
            return true;
        }

        public void SetProperties(Dictionary<string, object> properties)
        {
            Reflection.InitProperties(this, properties);
        }

        public List<Facility> GetFacilities()
        {
            return facilities;
        }

        public string GetInfo()
        {
            return "";
        }
    }
}
