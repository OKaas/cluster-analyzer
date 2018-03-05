using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zcu.Graphics.Clustering
{
    public static class Analysis
    {
        /// <summary>
        /// Facility cost multiplicator
        /// </summary>
        private static readonly double FAC_COST_MULT = 1;

        public static double ComputeClusterSolution(BoundingBox box, Facility[] facilities, Vertex[] points)
        {
            double ret = 0.0f;
            double facilityCost = box.GetWeightedDiagonal() * FAC_COST_MULT;

            foreach (Facility fac in facilities) {

                Vertex vertexFacility = points[fac.VertexIndex];

                foreach (int indexClient in fac.VertexIndices)
                {
                    ret += vertexFacility.WeightedDistance(points[indexClient]);
                }

                // add facility cost per facility
                ret += facilityCost;
            }

            return ret;
        }

        public static double ComputeClusterSolution(BoundingBox box, List<Facility> facilities, Vertex[] points)
        {
            double ret = 0.0f;
            double facilityCost = box.GetWeightedDiagonal() * FAC_COST_MULT;

            foreach (Facility fac in facilities)
            {

                Vertex vertexFacility = points[fac.VertexIndex];

                foreach (int indexClient in fac.VertexIndices)
                {
                    ret += vertexFacility.WeightedDistance(points[indexClient]);
                }

                // add facility cost per facility
                ret += facilityCost;
            }

            return ret;
        }
    }
}
