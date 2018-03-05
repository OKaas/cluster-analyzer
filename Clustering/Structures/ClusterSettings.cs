using System;
using System.Collections.Generic;
using System.Text;

namespace Clustering.Structures
{
    public struct ClusterSettings
    {
        double facilityCost;
        double[] weights;

        public ClusterSettings(double cost, double[] weights)
        {
            this.facilityCost = cost;
            this.weights = weights;
        }
    }
}
