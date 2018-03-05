using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zcu.Graphics.Clustering;

namespace ClusterViewer
{
    abstract public class AClusterIterator
    {
        protected Vertex[] extendedVertices;
        protected BoundingBox boundingBox;

        protected IClustering algorithm;
        protected Dictionary<string, object> properties;

        protected double bestClusterSolution = double.MaxValue;

        public AClusterIterator(Vertex[] vertices, BoundingBox box)
        {
            this.extendedVertices = vertices;
            this.boundingBox = box;
        }

        /// <summary>
        /// Find cluster properties for best cluster solution
        /// </summary>
        /// <returns>best properties</returns>
        abstract public string Solve();
    }
}
