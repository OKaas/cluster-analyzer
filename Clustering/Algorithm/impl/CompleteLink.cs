/*
 * Complete-link clustering.
 * 
 * Ondrej Kaas
 */
using Clustering.Helpers;
using System;
using System.Collections.Generic;

namespace Zcu.Graphics.Clustering
{
	/// <summary>
	/// Computes the complete-link clustering.
	/// </summary>
	public class CompleteLink : IClustering
	{
		/// <summary>
		/// The array of vertices to cluster.
		/// </summary>
		private Vertex[] vertices; 

		/// <summary>
		/// The list of clusters.
		/// </summary>
		private List<LinkedCluster> clusters;

        private double maxClusterDiameter = 0;


        #region CONSTRUCTOR
        /// <summary>
        /// Constructor.
        /// </summary>
        public CompleteLink()
		{
			// nothing
		}
        #endregion

        #region PROPERTIES
        #endregion

        #region GETTERS/SETTERS
        /// <summary>
        /// Gets the list of clusters.
        /// </summary>
        /// <returns>Returns the list of clusters.</returns>
        public List<GenericCluster> GetClusters()
            {
                List<GenericCluster> genClusters = new List<GenericCluster>(clusters.Count);

                // copy all clusters
                foreach (LinkedCluster cluster in clusters)
                {
                    GenericCluster genClus = new GenericCluster(cluster.CentreIndex);

                    foreach (int vertexIndex in cluster.MemberIndices)
                        genClus.AddVertex(vertexIndex);

                    genClusters.Add(genClus);
                }

                return genClusters;
            }

            /// <summary>
            /// Gets the list of clusters. Cluster centre is the vertex closest to the geometric centre of the cluster.
            /// </summary>
            /// <returns>Returns the list of clusters.</returns>
            public List<GenericCluster> GetCenteredClusters()
            {
                List<GenericCluster> centeredClusters = new List<GenericCluster>(clusters.Count);

                // rebuild all clusters
                foreach (LinkedCluster cluster in clusters)
                {
                    //double sumX = 0, sumY = 0, sumZ = 0;
                    //// find the centre of mass
                    //foreach (int vertexIndex in cluster.linkedVertices)
                    //{
                    //    sumX += vertices[vertexIndex].X;
                    //    sumY += vertices[vertexIndex].Y;
                    //    sumZ += vertices[vertexIndex].Z;
                    //}
                    //double count = cluster.linkedVertices.Count;
                    //Vertex perfectCentre = new Vertex(sumX / count, sumY / count, sumZ / count);

                    double minX, minY, minZ;
                    minX = minY = minZ = double.PositiveInfinity;
                    double maxX, maxY, maxZ;
                    maxX = maxY = maxZ = double.NegativeInfinity;
                    // find the bounding box
                    foreach (int vertexIndex in cluster.MemberIndices)
                    {
                        double x = vertices[vertexIndex].X;
                        double y = vertices[vertexIndex].Y;
                        double z = vertices[vertexIndex].Z;

                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;

                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;

                        if (z < minZ) minZ = z;
                        if (z > maxZ) maxZ = z;
                    }
                    // find the geometric centre
                    Vertex perfectCentre = new Vertex((maxX + minX) / 2.0, (maxY + minY) / 2.0, (maxZ + minZ) / 2.0);

                    int closestIndex = -1;
                    double minDist = double.PositiveInfinity;
                    // find the closest vertex
                    foreach (int vertexIndex in cluster.MemberIndices)
                    {
                        // compute the distance to the centre of mass
                        double dist = perfectCentre.WeightedDistance(vertices[vertexIndex]);

                        if (dist < minDist)
                        {
                            // the vertex is closer
                            minDist = dist;
                            closestIndex = vertexIndex;
                        }
                    }

                    GenericCluster genClus = new GenericCluster(closestIndex);
                    foreach (int vertexIndex in cluster.MemberIndices)
                        genClus.AddVertex(vertexIndex);
                    centeredClusters.Add(genClus);
                }

                return centeredClusters;
            }
        #endregion

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
            // TODO: dont carre about this algorithm ... yet
            return null;
        }

        public void SetProperties(Dictionary<string, object> properties)
        {
            Reflection.InitProperties(this, properties);
        }

        /// <summary>
        /// Computes the complete link clustering.
        /// </summary>
        /// <param name="vertices">The array of vertices to cluster.</param>
        /// <param name="maxClusterDiameter">The maximal allowed distance between any points in a cluster.</param>
        public int ComputeClustering(Vertex[] vertices)
        {
            this.vertices = vertices;

            // initialize every vertex as a cluster
            clusters = new List<LinkedCluster>(vertices.Length);
            for (int i = 0; i < vertices.Length; i++)
            {
                LinkedCluster f = new LinkedCluster(i);
                f.AddVertex(i);
                clusters.Add(f);
            }

            // initialize mutual distances
            for (int i = 0; i < vertices.Length - 1; i++)
                for (int j = i + 1; j < vertices.Length; j++)
                {
                    double distance = vertices[i].WeightedDistance(vertices[j]);
                    if (distance <= maxClusterDiameter)
                    {
                        clusters[i].distanceList.Add(distance, clusters[j]);
                        clusters[j].distanceList.Add(distance, clusters[i]);
                    }
                }

            double[,] distanceMatrix = new double[vertices.Length, vertices.Length];
            int half = vertices.Length / 2;
            // create distance matrix
            for (int i = 0; i < vertices.Length; i++)
                for (int jPartial = 0; jPartial < half; jPartial++)
                {
                    // make the index start after i and rotate modulo Length
                    int j = (jPartial + i + 1) % vertices.Length;

                    // compute the distance
                    double distance = vertices[i].WeightedDistance(vertices[j]);
                    // store it to the matrix
                    if (distance <= maxClusterDiameter)
                        distanceMatrix[i, j] = distance;
                }

            LinkedCluster a, b;
            // do the clustering
            while (FindClosestPair(out a, out b))
            {
                // merge b into a
                foreach (int vertexIndex in b.MemberIndices)
                    a.AddVertex(vertexIndex);

                // keep only a, discard b
                clusters.Remove(b);

                // remove the distance between the merged pair
                a.distanceList.RemoveAt(0);
                // update distances
                foreach (LinkedCluster f in clusters)
                {
                    // skip a and b
                    if (f == a) // || f == b	b is already removed from facilities
                        continue;

                    // find distance to both clusters of the pair
                    int indexOfA = f.distanceList.IndexOfValue(a);
                    int indexOfB = f.distanceList.IndexOfValue(b);
                    int indexOfF = a.distanceList.IndexOfValue(f);

                    if (indexOfA < 0 || indexOfB < 0)
                    {
                        // farther than the maximal allowed diameter
                        if (indexOfA >= 0)
                            f.distanceList.RemoveAt(indexOfA);
                        if (indexOfB >= 0)
                            f.distanceList.RemoveAt(indexOfB);
                        if (indexOfF >= 0)
                            a.distanceList.RemoveAt(indexOfF);
                    }
                    else
                    {
                        double newDistance = f.distanceList.Keys[indexOfA];
                        // keep the greater distance

                        // compare only indices, the array is sorted

                        if (f.distanceList.Keys[indexOfA] < f.distanceList.Keys[indexOfB])
                        {
                            // replace the key, keep the value
                            LinkedCluster value = f.distanceList.Values[indexOfA];
                            double key = f.distanceList.Keys[indexOfB];
                            f.distanceList.RemoveAt(indexOfB);
                            f.distanceList.Add(key, value);
                            newDistance = key;
                        }
                        // discard the other distance
                        f.distanceList.RemoveAt(indexOfB);

                        if (indexOfF >= 0)
                        {
                            // do this only if the above if passes

                            // update distance in a
                            LinkedCluster value2 = a.distanceList.Values[indexOfF];
                            a.distanceList.RemoveAt(indexOfF);
                            a.distanceList.Add(newDistance, value2);
                        }
                    }
                }
            }

            return -1;
        }
        #endregion

        #endregion

        #region PRIVATE
        /// <summary>
        /// Finds the closest pair of clusters.
        /// </summary>
        /// <param name="a">The first cluster.</param>
        /// <param name="b">The second cluster.</param>
        /// <returns>Returns false if no more clusters can be merged.</returns>
        private bool FindClosestPair(out LinkedCluster a, out LinkedCluster b)
            {
                // initialize the minimal distance
                double minDist = double.PositiveInfinity;
                a = b = null;

                // look at all clusters
                foreach (LinkedCluster f in clusters)
                {
                    if (f.distanceList.Count == 0)
                        continue;

                    // take the minimal distance
                    if (f.distanceList.Keys[0] < minDist)
                    {
                        // store the minimal distance
                        minDist = f.distanceList.Keys[0];
                        // store the indices of the closest pair
                        a = f;
                        b = f.distanceList.Values[0];
                    }
                }

                if (a != null && b != null)
                    // closest pair found
                    return true;
                else
                    // no more pairs to merge
                    return false;
            }
        #endregion
    }
}
