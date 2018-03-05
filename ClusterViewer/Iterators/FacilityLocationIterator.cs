using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zcu.Graphics.Clustering;

namespace ClusterViewer.Iterators.impl
{
    public class FacilityLocationIterator : AClusterIterator
    {
        public FacilityLocationIterator(Vertex[] vertices, BoundingBox box) 
            : base(vertices, box)
        {
            this.algorithm = new FacilityLocation();

            // add some properties to particular clustering algorithms
            this.properties = new Dictionary<string, object>();

            // TODO: best solution is not that easy to get :)
            // this.bestClusterSolution = Analysis.ComputeBestClusterSolution(vertices, box);

            properties.Add("boundingBox", box);

            // TODO: here should start the alchemy -> investigate where start with facility cost (facility size)
            properties.Add("facCostMult", 5);

            double[] initWeights = new double[vertices[0].Dimension];
            for (int i = 0; i < vertices[0].Dimension; ++i) { initWeights[i] = 1.0f; }

            properties.Add("weights", initWeights);
            algorithm.SetProperties(properties);
        }

        public override string Solve()
        {
            // here comes finding best solution of clustering
            Console.WriteLine("Analysing");
            //throw new NotImplementedException();

            // TODO: iterate

            algorithm.ComputeClustering(extendedVertices);

            List<Facility> fac = algorithm.GetFacilities();

            //double res = ComputeClusterSolution(fac);
            double res = Analysis.ComputeClusterSolution(boundingBox, fac, extendedVertices);
            Console.WriteLine("Facilities count: " + fac.Count);
            Console.WriteLine("Result: " + res);

            // TODO: change properties

            // TODO: iterate again
            return "";
        }
    }
}
