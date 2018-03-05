/*
 * Data stream clusterer
 * 
 * Ondrej Kaas
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Zcu.Graphics.Clustering
{
	/// <summary>
	/// Facility is a cluster centre.
	/// Each facility is also a vertex.
	/// </summary>
	public class Facility
	{
		/// <summary>
		/// List of indices of vertices assigned to this facility.
		/// </summary>
		private List<int> vertices;

		/// <summary>
		/// Index of corresponding vertex.
		/// This index is not to local <code>vertices</code> array.
		/// It is index to some external array outside this class.
		/// </summary>
		/// <remarks>Each facility is also a vertex.</remarks>
		private int vertexIndex;

		/// <summary>
		/// Accumulator for cost of closing this facility.
		/// That is the cost for reassigning all vertices somewhere else.
		/// </summary>
		private double accumulator;

		/// <summary>
		/// Determines whether this facility has been marked.
		/// Could be used for anything. This is for identification
		/// of clusters containing some sample points.
		/// </summary>
		private bool marked = false;

		/// <summary>
		/// The maximal non-weighted distance of any vertex assigned to this facility.
		/// </summary>
		private double maxNonWeightedDistance;

		/// <summary>
		/// Index of the vertex having the maximal distance from this facility.
		/// </summary>
		private int maxDistVertexIndex;
        
        private double[] coords;

        public Facility()
        {
            vertices = new List<int>();
        }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="vertex">Index of corresponding vertex.</param>
		public Facility(int vertexIndex) : this()
		{
			this.vertexIndex = vertexIndex;
		}

        public Facility(int vertexIndex, Vertex vertex) : this(vertexIndex)
        {
            coords = vertex.Coords;
        }

        [JsonProperty("Coords")]
        public double[] Coords
        {
            get { return coords; }
            set { coords = value; }
        }

        /// <summary>
        /// Gets the vertex index.
        /// </summary>
        [JsonProperty("VertexIndex")]
        public int VertexIndex
		{
			get { return vertexIndex; }
            set { vertexIndex = value; }
		}

        /// <summary>
        /// Gets or sets whether this facility is marked.
        /// </summary>
        [JsonProperty("Marked")]
        public bool Marked
		{
			get { return marked; }
			set { marked = value; }
		}

        /// <summary>
        /// Gets the maximal non-weighted distance of all assigned vertices.
        /// </summary>
        [JsonProperty("MaxNonWeightedDistance")]
        public double MaxNonWeightedDistance
        {
            get { return maxNonWeightedDistance; }
            set { maxNonWeightedDistance = value; }
		}

		/// <summary>
		/// Adds given vertex to facility.
		/// </summary>
		/// <param name="vertexToAdd">Index of vertex to add.</param>
		/// <param name="nonWeightedDistance">The non-weighted distance of the vertex being added.</param>
		public void AddVertex(int vertexToAdd, double nonWeightedDistance)
		{
			// add the vertex
			vertices.Add(vertexToAdd);

			// check the distance
			if (nonWeightedDistance > maxNonWeightedDistance)
			{
				// the vertex is farther than the current maximum
				maxNonWeightedDistance = nonWeightedDistance;
				maxDistVertexIndex = vertexToAdd;
			}
		}

        public void AddVertex(List<int> vertexToAdd)
        {
            // add the vertex
            vertices.AddRange(vertexToAdd);
        }

        /// <summary>
        /// Removes given vertex from facility.
        /// </summary>
        /// <param name="vertexToRemove">Index of vertex to remove.</param>
        /// <param name="allVertices">Array of all the vertices
        /// (necessary for updating the maximal distance).</param>
        public void RemoveVertex(int vertexToRemove, Vertex[] allVertices)
		{
			// remove the vertex
			vertices.Remove(vertexToRemove);

			// is the vertex with the maximal distance being removed?
			if (vertexToRemove == maxDistVertexIndex)
				// yes, must find the new maximum
				FindNewMaximum(allVertices);
		}

        public void RemoveAllClient()
        {
            vertices.Clear();
        }

		/// <summary>
		/// Finds the new maximal weighted distance.
		/// </summary>
		/// <param name="allVertices">The array of all the vertices involved in the clustering.</param>
		private void FindNewMaximum(Vertex[] allVertices)
		{
			maxNonWeightedDistance = 0;

			// find the new maximum
			foreach (int i in vertices)	// looking at THIS.vertices
			{
				// get the distance
				double dist = allVertices[i].NonWeightedDistToFac;

				// check the distance
				if (dist > maxNonWeightedDistance)
				{
					// new maximum found
					maxNonWeightedDistance = dist;
					maxDistVertexIndex = i;
				}
			}
		}

		/// <summary>
		/// Property to get accumulator value.
		/// </summary>
		public double Accumulator
		{
			get { return accumulator; }
		}

		/// <summary>
		/// Adds a value to accumulator.
		/// </summary>
		/// <param name="value">Value to add.</param>
		public void AddToAccum(double value)
		{
			accumulator += value;
		}

		/// <summary>
		/// Resets the accumulator.
		/// </summary>
		public void ResetAccum()
		{
			accumulator = 0;
		}

		/// <summary>
		/// Gets a read-only list of vertices assigned to this facility.
		/// </summary>
		public List<int> VertexIndices
		{
			get { return vertices; }
            set { vertices = value; }
		}

        /// <summary>
        /// Returns the size of list of vertices
        /// </summary>
        /// <returns>Number of vertices assigned to this facility</returns>
        public int GetAssignedVerticesCount()
        {
            return vertices.Count;
        }
	}
}
