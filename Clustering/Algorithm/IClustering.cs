/*
 * General interface for clustering algorithms
 * 
 * Ondrej Kaas
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Zcu.Graphics.Clustering
{
    public interface IClustering
    {
        /// <summary>
        /// General mechanism for setting algorithms 
        /// </summary>
        /// <param name="properties">setup for algorithm</param>
        void SetProperties(Dictionary<string, Object> properties);

        /// <summary>
        /// Main entry point to clustering
        /// </summary>
        /// <param name="points"></param>
        /// <returns>time for debug purpose</returns>
        int ComputeClustering(Vertex[] points);

        /// <summary>
		/// Gets indices of all facilities (cluster centres).
		/// </summary>
		/// <returns>Returns an array of facility indices.</returns>
        List<Facility> GetFacilities();

        /// <summary>
        /// Get all information about clustering properties, times, input files, etc.
        /// Give more information after calling method ComputeClustering
        /// </summary>
        /// <returns></returns>
        string GetInfo();

        bool PrepareStructures(ref List<Facility> facilities, ref Vertex[] vertices);
    }
}
