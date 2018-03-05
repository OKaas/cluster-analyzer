using System;
using System.Collections.Generic;
using System.Text;

namespace Zcu.Graphics.Clustering
{
    public struct MaxMin
    {
        public double max;
        public double min;
    }

    public class DataSet
    {
        public string[] name;
        public double[] minCorner;
        public double[] maxCorner;
        public double[] interval;
        public Vertex[] points;

        public void Scale()
        {

        }

        public DataSet Copy()
        {
            DataSet ret = new DataSet();

            ret.name = new string[this.name.Length];
            this.name.CopyTo(ret.name, 0);

            ret.minCorner = new double[this.minCorner.Length];
            this.minCorner.CopyTo(ret.minCorner, 0);

            ret.maxCorner = new double[this.maxCorner.Length];
            this.maxCorner.CopyTo(ret.maxCorner, 0);
            
            ret.interval = new double[this.interval.Length];
            this.interval.CopyTo(ret.interval, 0);

            ret.points = new Vertex[this.name.Length];
            this.points.CopyTo(ret.points, 0);

            return ret;
        }
    }
}
