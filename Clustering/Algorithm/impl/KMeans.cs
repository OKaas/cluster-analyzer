using Clustering.Helpers;
using System;
using System.Collections.Generic;

namespace Zcu.Graphics.Clustering
{
    /// <summary>
    /// Computes clustering using K-means clustering
    /// </summary>
    public class KMeans : IClustering
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
        /// Indicates if a vertex has been moved to another cluster
        /// </summary>
        private bool vertexMoved;
            
        /// <summary>
        /// Indicates if the algorithm is in the faze of computing (or preparing initial solution)
        /// </summary>
        private bool computing;

        /// <summary>
        /// Dimension of vertices which the algorithm is working with
        /// </summary>
        private int vertexDimension;

        private int randomSeed;

        private double[][] maxMin;

        private long timeEstimated;

        #region PUBLIC

        #region OVERRIDE
        public bool PrepareStructures(ref List<Facility> facilities, ref Vertex[] vertices)
        {
            Array.Resize<Vertex>(ref vertices, vertices.Length + facilities.Count);

            int indexFac = -1;
            for (int i = vertices.Length - facilities.Count; i < vertices.Length; ++i)
            {
                vertices[i] = new Vertex(facilities[++indexFac].Coords);
            }

            // fill all neccessary structures (correct connection between vertex and facility)
            foreach (Facility fac in facilities)
            {
                vertices[fac.VertexIndex].IsFacility = true;
                vertices[fac.VertexIndex].AssignToFacility(fac, 0);
            }

            return true;
        }

        public string GetInfo()
        {
            string ret = 
                "Algorithm:\t "+this+"\n"+
                "Vertices:\t "+vertices.Length +"\n"+
                "Dimension:\t "+vertices[0].Dimension+"\n"+
                "Facilities:\t " + facilities.Count + "\n" +
                "Time:\t " + timeEstimated + "\n"
                ;

            return ret;
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
            //rnd = new Random(DateTime.Now.Millisecond);
            rnd = new Random(randomSeed);
            //Console.WriteLine("Recreated random number generator with the seed of " + randomSeed);

            this.vertices = points;
            this.facilities.Clear();
            int start = Environment.TickCount;

            // the dimension for clustering is given by the first vertex received
            vertexDimension = vertices[0].Dimension;

            maxMin = new double[vertexDimension][];
            for (int i = 0; i < vertexDimension; i++)
            {
                maxMin[i] = new double[2];
                maxMin[i][0] = Double.MinValue;
                maxMin[i][1] = Double.MaxValue;
            }

            for (int i = 0; i < vertices.Length - numberOfClusters; i++)
            {
                for (int j = 0; j < vertexDimension; j++)
                {
                    if (vertices[i][j] > maxMin[j][0])
                    {
                        maxMin[j][0] = vertices[i][j];
                    }

                    if (vertices[i][j] < maxMin[j][1])
                    {
                        maxMin[j][1] = vertices[i][j];
                    }
                }
            }
            

            computing = false;
            // first it is needed to create initial solution
            GenerateInitialForgySolution();
            //GenerateInitialRandomPartitionSolution();
            computing = true;

            // repeat clustering while there are vertices reassigned
            int numberOfIterations = 0;
            do
            {
                vertexMoved = false;
                ComputeClustering();
                numberOfIterations++;
            } while (vertexMoved);
            //Console.WriteLine("Number of iterations: " + numberOfIterations);
            int end = Environment.TickCount;
            timeEstimated = end - start;
            return end - start;
        }
        #endregion

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="numberOfClusters">Number of clusters</param>
        public KMeans()
        {
            facilities = new List<Facility>(numberOfClusters);
        }

        #endregion

        #region PRIVATE

        /// <summary>
        /// Performs one round of clustering. Creates new facilities as centers
        /// of each cluster and assigns all vertices to the closest facility
        /// </summary>
        private void ComputeClustering()
        {
            // new centers of clusters - centroids of each clusters
            Vertex[] newCenters = ComputeClusterCenters();

            // the old facilities will not be neded anymore
            facilities.Clear();

            // iterate over each cluster
            for (int i = 0; i < numberOfClusters; i++)
            {
                // vertices for the facilities are stored at the end of vertices array
                vertices[vertices.Length - numberOfClusters + i] = newCenters[i];
                
                // create new facility for the according center
                facilities.Add(new Facility(vertices.Length - numberOfClusters + i, vertices[vertices.Length - numberOfClusters + i]));
                vertices[vertices.Length - numberOfClusters + i].IsFacility = true;
                vertices[vertices.Length - numberOfClusters + i].AssignToFacility(facilities[i], 0);
            }

            // assign every vertex to the new cluster centers
            for (int i = 0; i < vertices.Length - numberOfClusters; i++)
            {
                AssignVertexToClosestFacility(i);
            }
        }

        /// <summary>
        /// Computes centers (centroids) of each cluster
        /// </summary>
        /// <returns>Array of vertices representing each new center</returns>
        private Vertex[] ComputeClusterCenters()
        {
            // array for the new centers
            Vertex[] newCenters = new Vertex[numberOfClusters];

            // iterate over every cluster and compute its centroid
            for (int i = 0; i < numberOfClusters; i++)
            {
                // matching facility of the cluster
                Facility actualFacility = facilities[i];
                
                // array for computing the average of all vertices in the cluster
                double[] newCenterCoordinates = new double[vertexDimension];

                if (actualFacility.VertexIndices.Count != 0)
                {
                    // fill it with zeroes
                    for (int j = 0; j < vertexDimension; j++) { newCenterCoordinates[j] = 0.0; }

                    // how many vertices there is in the cluster
                    int numberOfVertices = actualFacility.GetAssignedVerticesCount();

                    // add every vertices coordinates values to the array
                    for (int j = 0; j < numberOfVertices; j++)
                    {
                        // iterating over all coordinates
                        for (int k = 0; k < vertexDimension; k++)
                        {
                            newCenterCoordinates[k] += vertices[actualFacility.VertexIndices[j]][k];
                        }
                    }

                    for (int j = 0; j < vertexDimension; j++)
                    {
                        newCenterCoordinates[j] /= (double)numberOfVertices;
                    }
                } else
                {
                    for (int j = 0; j < vertexDimension; j++) {
                        newCenterCoordinates[j] = vertices[actualFacility.VertexIndex][j];
                    }
                }

                newCenters[i] = new Vertex(newCenterCoordinates);
            }

            return newCenters;
        }

        /// <summary>
        /// Randomly generates initial solution - facilities with random
        /// location and assigns vertices to the closest ones
        /// </summary>
        /*private void GenerateInitialSolution()
        {
            for (int i = 0; i < numberOfClusters; i++)
            {
                // create array of random coordinates
                double[] coordinates = new double[vertexDimension];
                for (int j = 0; j < vertexDimension; j++)
                {
                    coordinates[j] = (rnd.NextDouble() * (maxMin[j][0] - maxMin[j][1])) + maxMin[j][0];
                }
                // create new vertex with the random coordinates
                vertices[vertices.Length - numberOfClusters + i] = new Vertex(coordinates);
                
                // make it a facility
                facilities.Add(new Facility(vertices.Length - numberOfClusters + i));
                vertices[vertices.Length - numberOfClusters + i].IsFacility = true;
                vertices[vertices.Length - numberOfClusters + i].AssignToFacility(facilities[i], 0);
            }

            // assign vertices to the closest facility
            for (int i = 0; i < vertices.Length - numberOfClusters; i++)
            {
                AssignVertexToClosestFacility(i);
            }
        }*/

         private void GenerateInitialForgySolution()
        {
            //Console.WriteLine("Generating initial solution, testing random number: " + rnd.Next());
            List<int> chosenIndices = new List<int>();
            for (int i = 0; i < numberOfClusters; i++)
            {
                double[] coordinates = new double[vertexDimension];
                int random;
                while (true)
                {
                    random = rnd.Next(vertices.Length - numberOfClusters);
                    //Console.WriteLine("Generated: " + random + "\t\t for input value: " + (vertices.Length - numberOfClusters));

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
                facilities.Add(new Facility(vertices.Length - numberOfClusters + i));
                vertices[vertices.Length - numberOfClusters + i].IsFacility = true;
                vertices[vertices.Length - numberOfClusters + i].AssignToFacility(facilities[i], 0);
            }

            // assign vertices to the closest facility
            for (int i = 0; i < vertices.Length - numberOfClusters; i++)
            {
                AssignVertexToClosestFacility(i);
            }
        }

        private void GenerateInitialRandomPartitionSolution()
        {
            for (int i = 0; i < numberOfClusters; i++)
            {
                // create array of random coordinates
                double[] coordinates = new double[vertexDimension];
                for (int j = 0; j < vertexDimension; j++)
                {
                    coordinates[j] = (rnd.NextDouble() * (maxMin[j][0] - maxMin[j][1])) + maxMin[j][0];
                }
                // create new vertex with the random coordinates
                vertices[vertices.Length - numberOfClusters + i] = new Vertex(coordinates);

                // make it a facility
                facilities.Add(new Facility(vertices.Length - numberOfClusters + i));
                vertices[vertices.Length - numberOfClusters + i].IsFacility = true;
                vertices[vertices.Length - numberOfClusters + i].AssignToFacility(facilities[i], 0);
            }

            // assign vertices to the closest facility
            for (int i = 0; i < vertices.Length - numberOfClusters; i++)
            {
                Facility fac = facilities[i % numberOfClusters];
                vertices[i].AssignToFacility(fac, vertices[i].WeightedDistance(vertices[fac.VertexIndex]));
                fac.AddVertex(i, vertices[i].WeightedDistance(vertices[fac.VertexIndex]));
            }
        }

        /// <summary>
        /// Finds closest facility to the given vertex and assigns it to it
        /// </summary>
        /// <param name="vertexIntex">Index of the vertex to assign</param>
        private void AssignVertexToClosestFacility(int vertexIntex)
        {
            // minimal distance of the vertex to the facility
            double minDistance = Double.MaxValue;
            // facility which is closest to the fiven vertex
            Facility closestFacility = null;

            // check distance to every facility
            for (int i = 0; i < numberOfClusters; i++)
            {
                // distance of the vertex to the actual facility
                double actualDistance = vertices[vertexIntex].WeightedDistance(vertices[facilities[i].VertexIndex]);

                // store it if it is closer than the actual facility
                if (actualDistance < minDistance)
                {
                    closestFacility = facilities[i];
                    minDistance = actualDistance;
                }
            }
            
            // tell if the vertex was moved to another facility that it was before 
            // only if the algorithm is in the process of computing and not generating initial solution
            if (computing && closestFacility.VertexIndex != vertices[vertexIntex].Facility.VertexIndex)
            {
                vertexMoved = true;
            }

            // assign vertex to the detected closest facility
            vertices[vertexIntex].AssignToFacility(closestFacility, vertices[vertexIntex].WeightedDistance(vertices[closestFacility.VertexIndex]));
            closestFacility.AddVertex(vertexIntex, minDistance);
        }
    }

    #endregion
}
