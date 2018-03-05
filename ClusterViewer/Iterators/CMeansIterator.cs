using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zcu.Graphics.Clustering;

using System.IO;

namespace ClusterViewer.Iterators.impl
{
    public class CMeansIterator : AClusterIterator
    {
        private CMeans algorithm = new CMeans();

        /// <summary>
        /// It is needed to extend the array of vertices for the vertices
        /// corresponding to the created facilities.
        /// Storing original array of vertices for easy restoration of the
        /// original array.
        /// </summary>
        private Vertex[] originalVertices;
        

        public CMeansIterator(Vertex[] vertices, BoundingBox box) 
            : base(vertices, box)
        {
            this.algorithm = new CMeans();
            
            originalVertices = (Vertex[]) vertices.Clone();
            // add some properties to particular clustering algorithms
            properties = new Dictionary<string, object>();

            properties.Add("boundingBox", box);

            // TODO: here should start the alchemy -> investigate where start with number of clusters and threshold
            properties.Add("numberOfClusters", 10);
            properties.Add("treshold", 0.0);

            algorithm.SetProperties(properties);
        }

        public override string Solve()
        {
            bool stopIterating = false;
            Vertex[] extendedVertices = null;
            List<Facility> facilities = null;
            double solutionCost = 0.0;
            int iterationIndex = 1;

            /* Algorithm parameter - initial weight for coordinates */
            double xStart = 0.05;
            double xStep = 0.05;
            /* Algorithm parameter - number of clusters */
            int initialNumberOfClusters = 4;
            int maxNumberOfClusters = 50;
            /* Algorithm parameter - treshold */
            double startTreshold = 0.0;
            double endTreshold = 1.0;
            double tresholdStep = 0.1;
            /* Algorithm parameter - fuzzier */
            int startFuzzier = 1;
            int endFuzzier = 10;
            int fuzzierStep = 1;

            double bestSolution = Double.MaxValue;
            int bestNumberOfClusters = 0;
            double[] bestWeights = null;
            double bestTreshold = 0.0;
            int bestFuzzier = 0;

            StreamWriter writer = new StreamWriter("Vysledky.txt", true);
            writer.WriteLine("*************************************************");
            writer.WriteLine("************* NOVE VYSLEDKY C-MEANS *************");
            writer.WriteLine("*************************************************");

            /* *** ITERATING number of clusters *** */
            for (int actualNumberOfClusters = initialNumberOfClusters; actualNumberOfClusters <= maxNumberOfClusters; actualNumberOfClusters++)
            {
                Console.WriteLine("Iteration number " + iterationIndex);

                extendedVertices = fillExtendedVerticesArray(actualNumberOfClusters);

                /* Weights initialisation */
                double[] weights = new double[]{ xStart, 2 - xStart };

                /* *** ITERATING weights *** */
                /*for (double xValue = xStart; xValue < 2.0; xValue += xStep)
                {
                    weights[0] = xValue;
                    weights[1] = 2.0 - xValue;

                    Vertex.CoordWeights = weights;*/
                for (int actualFuzzier = startFuzzier; actualFuzzier <= endFuzzier; actualFuzzier += fuzzierStep)
                {

                    for (double actualTreshold = startTreshold; actualTreshold <= endTreshold; actualTreshold += tresholdStep)
                    {
                        /* Setting properties */
                        properties = new Dictionary<string, object>();
                        properties.Add("boundingBox", boundingBox);
                        properties.Add("numberOfClusters", actualNumberOfClusters);
                        properties.Add("treshold", actualTreshold);
                        properties.Add("fuzzier", actualFuzzier);
                        algorithm.SetProperties(properties);
                     
                        /* Computation */
                        algorithm.ComputeClustering(extendedVertices);


                        facilities = algorithm.GetFacilities();
                        solutionCost = Analysis.ComputeClusterSolution(boundingBox, facilities, extendedVertices);

                        writer.WriteLine("Solution cost: " + solutionCost);
                        writer.WriteLine("Number of clusters: " + actualNumberOfClusters);
                        //writer.WriteLine("Weights: X: " + weights[0] + "  Y: " + weights[1]);
                        writer.WriteLine("Treshold: " + actualTreshold);
                        writer.WriteLine("Fuzzier: " + actualFuzzier);

                        if (solutionCost < bestSolution)
                        {
                            bestSolution = solutionCost;
                            bestNumberOfClusters = actualNumberOfClusters;
                            bestWeights = (double[])weights.Clone();
                            bestTreshold = actualTreshold;
                            bestFuzzier = actualFuzzier;
                        }

                        iterationIndex++;
                    }
                }

                /*MainForm mf = new MainForm();
                mf.vertices = (Vertex[]) extendedVertices.Clone();
                mf.showPoints = true;
                mf.Show();
                mf.Invalidate();*/
            }

            Console.WriteLine("********************************************");
            Console.WriteLine("Best solution cost: " + bestSolution);
            Console.WriteLine("With number of clusters: " + bestNumberOfClusters);
            Console.WriteLine("With weights: X: " + bestWeights[0] + "  Y: " + bestWeights[1]);
            Console.WriteLine("And treshold: " + bestTreshold);
            Console.WriteLine("Fuzzier: " + bestFuzzier);

            writer.WriteLine("********************************************");
            writer.WriteLine("Best solution cost: " + bestSolution);
            writer.WriteLine("With number of clusters: " + bestNumberOfClusters);
            writer.WriteLine("With weights: " + bestWeights[0] + ", " + bestWeights[1]);
            writer.WriteLine("And treshold: " + bestTreshold);
            writer.WriteLine("Fuzzier: " + bestFuzzier);

            writer.WriteLine("************** KONEC VYSLEDKU **************");
            writer.Flush();

            return "";
        }

        private Vertex[] fillExtendedVerticesArray(int numberOfClusters)
        {
            int numberOfVertices = originalVertices.Count();
            Vertex[] extendedVertices = new Vertex[numberOfVertices + numberOfClusters];
            for (int i = 0; i < numberOfVertices; i++)
            {
                extendedVertices[i] = originalVertices[i];
            }

            return extendedVertices;
        }
    }
}
