using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zcu.Graphics.Clustering;
using System.IO;

namespace ClusterViewer.Iterators.impl
{
    public class KMeansIterator : AClusterIterator
    {

        Vertex[] originalVertices;


        public KMeansIterator(Vertex[] inputPoints, BoundingBox boxArea) 
            : base(inputPoints, boxArea)
        {
            this.algorithm = new KMeans();
            originalVertices = (Vertex[]) inputPoints.Clone();

            // add some properties to particular clustering algorithms
            this.properties = new Dictionary<string, object>();

            properties.Add("boundingBox", boxArea);

            // TODO: here should start the alchemy -> investigate where start with number of clusters and threshold
            properties.Add("numberOfClusters", 10);
            properties.Add("treshold", 0.0);

            algorithm.SetProperties(properties);
        }

        public override string Solve()
        {
            Vertex[] extendedVertices = null;
            List<Facility> facilities = null;
            double solutionCost = 0.0;
            int iterationIndex = 1;
            /* Algorithm parameter - number of clusters */
            int initialNumberOfClusters = 4;
            int maxNumberOfClusters = 15;

            double bestSolution = Double.MaxValue;
            int bestNumberOfClusters = 0;

            StreamWriter writer = new StreamWriter("Vysledky.txt", true);
            writer.WriteLine("*************************************************");
            writer.WriteLine("************* NOVE VYSLEDKY K-MEANS *************");
            writer.WriteLine("*************************************************");

            /* *** ITERATING number of clusters *** */
            for (int actualNumberOfClusters = initialNumberOfClusters; actualNumberOfClusters <= maxNumberOfClusters; actualNumberOfClusters++)
            {
                algorithm = new KMeans();

                Console.WriteLine("Iteration number " + iterationIndex);

                extendedVertices = fillExtendedVerticesArray(actualNumberOfClusters);


                /* Setting properties */
                properties = new Dictionary<string, object>();
                properties.Add("boundingBox", boundingBox);
                properties.Add("numberOfClusters", actualNumberOfClusters);
                properties.Add("randomSeed", 1234);
                algorithm.SetProperties(properties);

                /* Computation */
                algorithm.ComputeClustering(extendedVertices);


                facilities = algorithm.GetFacilities();
                solutionCost = Analysis.ComputeClusterSolution(boundingBox, facilities, extendedVertices);

                writer.WriteLine("Solution cost: " + solutionCost);
                writer.WriteLine("Number of clusters: " + actualNumberOfClusters);

                if (solutionCost < bestSolution)
                {
                    bestSolution = solutionCost;
                    bestNumberOfClusters = actualNumberOfClusters;
                }

                iterationIndex++;
            }

            Console.WriteLine("********************************************");
            Console.WriteLine("Best solution cost: " + bestSolution);
            Console.WriteLine("With number of clusters: " + bestNumberOfClusters);

            writer.WriteLine("********************************************");
            writer.WriteLine("Best solution cost: " + bestSolution);
            writer.WriteLine("With number of clusters: " + bestNumberOfClusters);

            IterateOverNWeights(bestNumberOfClusters, bestSolution);

            writer.WriteLine("************** KONEC VYSLEDKU **************");
            writer.Flush();
            writer.Close();

            return "";
        }

        /// <summary>
        /// Iterates over all possible weights configurations to see
        /// if it generates better solution
        /// </summary>
        /// <param name="bestNumberOfClusters">What number of clusters generated the best solution so far</param>
        /// <param name="bestSolutionCost">What was the cost of the best solution found so far</param>
        private void IterateOverNWeights(int bestNumberOfClusters, double bestSolutionCost)
        {
            /* With what step will be the weights iterated */
            double weightStep = 0.2;
            /* How far from the bestNumberOfClusters will the algorithm look for better solution */
            int numberOfClustersRange = 0;

            double maxWeightSum = originalVertices[0].Dimension;

            List<Facility> facilities;
            double solutionCost;

            double newBestSolutionCost = bestSolutionCost;
            int newBestNumberOfClusters = bestNumberOfClusters;
            double[] bestWeights = null;

            int possibilitiesCount = (int)Math.Round((maxWeightSum - weightStep) / weightStep);
            long numberOfPermutations = GetNumberOfPermutations(possibilitiesCount);
            int dimCount = originalVertices[0].Dimension;

            StreamWriter writer = new StreamWriter("VysledekMeneniVah.txt");
            for (int actualNumberOfClusters = bestNumberOfClusters - numberOfClustersRange; actualNumberOfClusters <= bestNumberOfClusters + numberOfClustersRange; actualNumberOfClusters++)
            {
                extendedVertices = fillExtendedVerticesArray(actualNumberOfClusters);
                for (long i = 0; i < numberOfPermutations; i++)
                {
                    double[] weights = GetWeightsForPermutation(dimCount, i, possibilitiesCount, weightStep, maxWeightSum, writer);

                    if (Math.Abs(GetWeightSum(weights) - maxWeightSum) > 0.001)
                    {
                        continue;
                    }

                    Vertex.CoordWeights = weights;
                    
                    properties = new Dictionary<string, object>();
                    properties.Add("boundingBox", boundingBox);
                    properties.Add("numberOfClusters", actualNumberOfClusters);
                    properties.Add("randomSeed", 1234);
                    algorithm.SetProperties(properties);

                    /* Computation */
                    algorithm.ComputeClustering(extendedVertices);


                    facilities = algorithm.GetFacilities();
                    solutionCost = Analysis.ComputeClusterSolution(boundingBox, facilities, extendedVertices);

                    if (solutionCost < newBestSolutionCost)
                    {
                        newBestSolutionCost = solutionCost;
                        bestWeights = (double[])weights.Clone();
                        newBestNumberOfClusters = actualNumberOfClusters;

                    }
                }
            }
            writer.Flush();
            Console.WriteLine("New best solution cost: " + newBestSolutionCost);
            Console.WriteLine("New best number of clusters: " + newBestNumberOfClusters);
            Console.WriteLine("With weights: " + bestWeights[0] + ", " + bestWeights[1] + ", " + bestWeights[2] + ", " + bestWeights[3]);
        }

        private double GetWeightSum(double[] weights)
        {
            double result = 0.0;
            for (int i = 0; i < weights.Length; i++)
            {
                result += weights[i];
            }
            return result;
        }

        private double[] GetWeightsForPermutation(int dimCount, long i, int possibilitiesCount, double weightStep, double weightMax, StreamWriter writer)
        {
            double[] weights = new double[dimCount];
            long actualIndex = i;
            //writer.WriteLine("Pro icko: " + i);
            for (int g = dimCount - 1; g >= 0; g--)
            {
                int indexDivider = Pow(possibilitiesCount, g);

                int weightIndex = (int) actualIndex / indexDivider;
                //writer.Write(weightIndex + ", ");
                weights[dimCount - 1 - g] = GetWeightByWeightIndex(weightStep, weightIndex);
                //writer.WriteLine("g: " + g + ", indexDivider: " + indexDivider + ", actualIndex: " + actualIndex + "weightIndex: " + weightIndex);
                actualIndex = actualIndex % indexDivider;
            }
            //writer.WriteLine();

            //writer.WriteLine(weights[0] + ", " + weights[1] + ", " + weights[2] + ", " + weights[3]);

            return weights;
        }

        private double GetWeightByWeightIndex(double weightStep, int i)
        {
            return weightStep + (i * weightStep);
        }

        private int Pow(int powBase, int exponent)
        {
            int result = 1;
            for (int i = 0; i < exponent; i++)
            {
                result *= powBase;
            }
            return result;
        }

        private long GetNumberOfPermutations(int possibilitiesCount)
        {
            long result = possibilitiesCount;
            for (int i = 1; i < originalVertices[0].Dimension; i++)
            {
                result *= possibilitiesCount;
            }
            return result;
        }

        private void IterateOverWeights(int bestNumberOfClusters, double bestCost)
        {
            double weightStep = 0.2;

            int numberOfClusterRange = 0;

            double maxWeightSum = (double) originalVertices[0].Dimension;

            List<Facility> facilities;
            double solutionCost;
            double bestNewSolutionCost = bestCost;
            int bestNewNumberOfClusters = bestNumberOfClusters;
            double[] bestWeights = null;

            int pocetProzkoumanychMoznosti = 0;

            Console.WriteLine("\n\nIterating weights for best number of clusters of " + bestNumberOfClusters);
            StreamWriter writer = new StreamWriter("VysledekMeneniVah.txt");
            for (int actualNumberOfClusters = bestNumberOfClusters - numberOfClusterRange; actualNumberOfClusters <= bestNumberOfClusters + numberOfClusterRange; actualNumberOfClusters++)
            {
                extendedVertices = fillExtendedVerticesArray(actualNumberOfClusters);
                for (double i = weightStep; i < maxWeightSum; i += weightStep)
                {
                    for (double j = weightStep; j < maxWeightSum - i + 0.01; j += weightStep)
                    {
                        if (i + j > maxWeightSum) break;
                        for (double k = weightStep; k < maxWeightSum - i - j + 0.01; k += weightStep)
                        {
                            if (i + j + k > maxWeightSum) break;
                            for (double l = weightStep; l < maxWeightSum - i - j - k + 0.01; l += weightStep)
                            {
                                writer.WriteLine("With weights: " + i + ", " + j + ", " + k + ", " + l);

                                if (Math.Abs((i + j + k + l) - maxWeightSum) > 0.001)
                                {
                                    Console.WriteLine("Musim brejkovat :/");
                                    Console.WriteLine("Weights: " + i + ", " + j + ", " + k + ", " + l);
                                    continue;
                                }

                                double[] actualWeights = new double[] { i, j, k, l };

                                Vertex.CoordWeights = actualWeights;
                                pocetProzkoumanychMoznosti++;
                                properties = new Dictionary<string, object>();
                                properties.Add("boundingBox", boundingBox);
                                properties.Add("numberOfClusters", actualNumberOfClusters);
                                properties.Add("randomSeed", 1234);
                                algorithm.SetProperties(properties);

                                /* Computation */
                                algorithm.ComputeClustering(extendedVertices);


                                facilities = algorithm.GetFacilities();
                                solutionCost = Analysis.ComputeClusterSolution(boundingBox, facilities, extendedVertices);

                                if (solutionCost < bestNewSolutionCost)
                                {
                                    bestNewSolutionCost = solutionCost;
                                    bestWeights = (double[]) actualWeights.Clone();
                                    bestNewNumberOfClusters = actualNumberOfClusters;

                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Examined : " + pocetProzkoumanychMoznosti + " options");
            writer.Flush();
            Console.WriteLine("New best solution cost: " +  bestNewSolutionCost);
            Console.WriteLine("New best number of clusters: " + bestNewNumberOfClusters);
            Console.WriteLine("With weights: " + bestWeights[0] + ", " + bestWeights[1] + ", " + bestWeights[2] + ", " + bestWeights[3]);
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

