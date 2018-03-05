/*
 * Cluster of linked vertices. For the complete-link clustering.
 * 
 * Ondrej Kaas
 */
using System;
using System.Collections.Generic;

namespace Zcu.Graphics.Clustering
{
    /// <summary>
    /// Cluster of linked vertices.
    /// </summary>
    class LinkedCluster : GenericCluster
    {
        /// <summary>
        /// List of distances to other vertices / clusters.
        /// </summary>
        public SortedList<double, LinkedCluster> distanceList;






		private class IdiotComparer : IComparer<double>
		{
			public int Compare(double x, double y)
			{
				if (x <= y)
					return -1;
				else
					return 1;
			}
		}
		
		/// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="vertexIndex">Index of some vertex belonging to this cluster.</param>
        public LinkedCluster(int vertexIndex)
			: base(vertexIndex)
        {
			// ??
			//memberIndices.Add(vertexIndex);
			
			distanceList = new SortedList<double, LinkedCluster>(new IdiotComparer());
        }
    }
}
