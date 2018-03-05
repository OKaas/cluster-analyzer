
using Clustering.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zcu.Graphics.Clustering
{
    public class CMeans : IClustering
    {

        /// <summary>
        /// Array of vertices.
        /// </summary>
        private Vertex[] vertices = null;

        /// <summary>
        /// Array of open facilities.
        /// </summary>
        private List<Facility> facilities = null;

        /// <summary>
        /// Number of clusters - K parameter
        /// </summary>
        private int numberOfClusters;

        /// <summary>
        /// Random number generator
        /// </summary>
        private Random rnd;

        /// <summary>
        /// Dimension of vertices which the algorithm is working with
        /// </summary>
        private int vertexDimension;

        /// <summary>
        /// Number of vertices given to the algorithm
        /// </summary>
        private int numberOfVertices;

        /// <summary>
        /// Value of the greatest change in the coefficients of belonging to the clusters.
        /// This is to stop the iterations
        /// </summary>
        private double greatestChange;

        /// <summary>
        /// The value of the greatest change needet not stop iterating
        /// </summary>
        private const double EPSILON = 0.0001;

        /// <summary>
        /// What is the point's minimal value of belonging to the given cluster
        /// to be counted to the cluster centroid
        /// </summary>
        private double treshold;

        /// <summary>
        /// Matrix of coefficients of belonging to the clusters
        /// </summary>
        private double[][] matrix;

        private int fuzzier;

        #region PUBLIC
        #region OVERRIDE

        public bool PrepareStructures(ref List<Facility> facilities, ref Vertex[] vertices)
        {
            return true;
        }

        public string GetInfo()
        {
            return "";
        }

        public List<Facility> GetFacilities()
        {
            return facilities;
        }

        public void SetProperties(Dictionary<string, object> properties)
        {
            Reflection.InitProperties(this, properties);
        }

        public int ComputeClustering(Vertex[] points)
        {
            this.vertices = points;
            int start = Environment.TickCount;

            // the dimension for clustering is given by the first vertex received
            vertexDimension = vertices[0].Dimension;

            GenerateInitialSolution();

            int numberOfIterations = 0;
            do
            {
                greatestChange = Double.MinValue;
                ComputeNewClusters();
                numberOfIterations++;
            } while (greatestChange > EPSILON);/**/
            //Console.WriteLine("Number of iterations: " + numberOfIterations);

            AssignVerticesToFacilities();

            /*for (int i = 0; i < numberOfClusters; i++)
            {
                Console.WriteLine("Centrum " + i + ": " + facilities[i].VertexIndices.Count);
            }/**/

            int end = Environment.TickCount;
            return end - start;
        }
        #endregion
        #endregion

        #region CONSTRUCTORS
        public CMeans()
        {
            rnd = new Random(DateTime.Now.Millisecond);
            //rnd = new Random(1234);
        }
        #endregion

        #region PRIVATE

        /// <summary>
        /// Generates initial solution - the first random facilities and coefficients
        /// </summary>
        private void GenerateInitialSolution()
        {
            facilities = new List<Facility>(numberOfClusters);
            numberOfVertices = vertices.Length - numberOfClusters;
            matrix = new double[numberOfVertices][];

            for (int i = 0; i < numberOfVertices; i++)
            {
                matrix[i] = new double[numberOfClusters];
            }

            CreateInitialFacilities();

            RecalculateCoefficients();
            
            AssignVerticesToFacilities();
        }

        /// <summary>
        /// Computes centroids of the actual clusters and new coefficients for them
        /// </summary>
        private void ComputeNewClusters()
        {
            ComputeClusterCentroids();

            RecalculateCoefficients();         
        }

        /// <summary>
        /// Computes centroids of all clusters.
        /// Centroid is calculated as the sum of the point's (belonging to the cluser) coordinates multiplied by the coefficient of
        /// belonging to the clusters. This sum is then divided by the sum of the coefficients.
        /// </summary>
        private void ComputeClusterCentroids()
        {
            facilities.Clear();
            double[] newCoordinates;
            double weightSum;
            double fuzzWeight;

            for (int i = 0; i < numberOfClusters; i++)
            {
                newCoordinates = new double[vertexDimension];
                weightSum = 0.0;
                for (int j = 0; j < vertexDimension; j++) {
                    newCoordinates[j] = 0.0;
                }

                for (int j = 0; j < numberOfVertices; j++)
                {
                    if (matrix[j][i] < treshold) continue;

                    /* Computing weight^fuzzier */
                    fuzzWeight = matrix[j][i];
                    for (int f = 1; f < fuzzier; f++)
                    {
                        fuzzWeight *= matrix[j][i];
                    }

                    // iterating over all coordinates
                    for (int k = 0; k < vertexDimension; k++)
                    {
                        newCoordinates[k] += vertices[j][k] * fuzzWeight;
                    }
                    weightSum += fuzzWeight;
                }

                for (int j = 0; j < vertexDimension; j++)
                {
                    newCoordinates[j] /= weightSum; 
                }

                vertices[numberOfVertices + i] = new Vertex(newCoordinates);

                facilities.Add(new Facility(numberOfVertices + i, vertices[numberOfVertices + i]));
                vertices[numberOfVertices + i].IsFacility = true;
                vertices[numberOfVertices + i].AssignToFacility(facilities[i], 0);/**/
            }
        }

        /// <summary>
        /// Creates random facilities. Used at the initiation of the algorithm
        /// </summary>
        private void CreateInitialFacilities()
        {
            List<int> chosenIndices = new List<int>();
            for (int i = 0; i < numberOfClusters; i++)
            {
                double[] coordinates = new double[vertexDimension];
                int random;
                while (true)
                {
                    random = rnd.Next(vertices.Length - numberOfClusters);
                    if (!chosenIndices.Contains(random))
                    {
                        chosenIndices.Add(random);
                        break;
                    }
                }
                //Console.WriteLine("i: " + random);

                Vertex v = vertices[random];

                for (int j = 0; j < vertexDimension; j++)
                {
                    coordinates[j] = v[j];
                }

                vertices[vertices.Length - numberOfClusters + i] = new Vertex(coordinates);

                // make it a facility
                facilities.Add(new Facility(numberOfVertices + i, vertices[numberOfVertices + i]));
                vertices[numberOfVertices + i].IsFacility = true;
                vertices[numberOfVertices + i].AssignToFacility(facilities[i], 0);
            }
        }

        /// <summary>
        /// Calculate coefficients of belonging in the clusters for every vertex
        /// </summary>
        private void RecalculateCoefficients()
        {
            //to normalize the results
            double min, max;
            double dist, diff;

            double[][] newMatrix = new double[numberOfVertices][];
            for (int i = 0; i < numberOfVertices; i++)
            {
                newMatrix[i] = new double[numberOfClusters];
            }

            // iterate over all vertices
            for (int i = 0; i < numberOfVertices; i++)
            {
                max = Double.MinValue;
                min = Double.MaxValue;
                // calculate actual distances
                for (int j = 0; j < numberOfClusters; j++)
                {
                    dist = vertices[i].WeightedDistance(vertices[numberOfVertices + j]);
                    newMatrix[i][j] = dist;
                    if (dist > max)
                    {
                        max = dist;
                    }
                    if (dist < min)
                    {
                        min = dist;
                    }
                }

                // map coefficients to <0, 1>
                for (int j = 0; j < numberOfClusters; j++)
                {
                    newMatrix[i][j] = 1 - ((newMatrix[i][j] - min) / (max - min));

                    // Calculate the greatest change to stop the algorithm
                    diff = Math.Abs(newMatrix[i][j] - matrix[i][j]);
                    if (diff > greatestChange) greatestChange = diff;
                }
            }

            matrix = newMatrix;
        }

        /// <summary>
        /// Assigns vertices to facilities. Used at the end of the algorithm to show the result
        /// </summary>
        private void AssignVerticesToFacilities()
        {
            // Zatim prirazeni k vertexu s nejvyssim koeficientem prislusnosti
            int facilIndex;
            for (int i = 0; i < numberOfVertices; i++)
            {
                facilIndex = GetFacilityIndex(i);

                vertices[i].AssignToFacility(facilities[facilIndex], vertices[i].WeightedDistance(vertices[facilities[facilIndex].VertexIndex]));
                facilities[facilIndex].AddVertex(i, vertices[i].NonWeightedDistToFac);
            }
            //Console.WriteLine("****");
        }

        /// <summary>
        /// Used for assigning vertices to the clusters.
        /// Looks for the given vertices greatest value of belonging to the clusters and returns that cluster's index
        /// </summary>
        /// <param name="vertexIndex">Index of vertex to assign facility to</param>
        /// <returns>Index of facility with max value</returns>
        private int GetFacilityIndex(int vertexIndex)
        {
            double max = Double.MinValue;
            int index = 0;

            for (int i = 0; i < numberOfClusters; i++)
            {
                if (matrix[vertexIndex][i] > max)
                {
                    index = i;
                    max = matrix[vertexIndex][i];
                }
            }
            //Console.WriteLine(index);
            return index;
        }

        #endregion
    }
}
