using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Zcu.Graphics.Clustering
{
    public static class Loader
    {

        private static readonly int GEO_DIM = 7;
        private static readonly char[] DEL = { ' ', '\t' };

        // GEO vector
        private static readonly uint SKIP = 3;
        private static readonly string[] NAMES = { "x", "y", "eX", "eY", "dX", "dY", "v", "rating" };

        public static Vertex[] LoadVertices(string inputFileName)
        {
            System.Globalization.CultureInfo invarCult = System.Globalization.CultureInfo.InvariantCulture;
            Vertex[] ret = null;

            using (StreamReader reader = new StreamReader(inputFileName))
            {
                // suppose at first line is -> count number (space) number of dimensions
                string firstLine = reader.ReadLine();

                string[] control = firstLine.Split();
                if ( control.Length != 2 ) {
                    Console.WriteLine("Wrong first line in file! \n Pattern is: {number of points} {number of dimensions}");
                    return null;
                }

                long vertexCount = long.Parse(control[0]);
                if (vertexCount <= 0) {
                    Console.WriteLine("Number of points is 0!");
                    return null;
                }

                long dim = long.Parse(control[1]);
                if (dim <= 0)
                {
                    Console.WriteLine("Dimension of vertices must be greater that 0!");
                    return null;
                }

                ret = new Vertex[vertexCount];

                double[] coords = new double[dim];
                for (int i = 0; i < vertexCount; ++i)
                {
                    string[] tokens = reader.ReadLine().Split();
                    // suppose 1 vertex on 1 line. +1 is end of line character
                    if (tokens.Length != dim) {
                        Console.WriteLine("Vertex at {0} does not have {1} dimension!", i, dim);
                        return null;
                    }

                    try
                    {
                        for (int j = 0; j < dim; ++j)
                        {
                            coords[j] = double.Parse(tokens[j], invarCult);
                        }

                        ret[i] = new Vertex(coords);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(MethodBase.GetCurrentMethod().DeclaringType + "\n" + e.Message);
                        return null;
                    }
                }
            }

            return ret;
        }

        public static Vertex[] LoadVtk(string inputFileName)
        {
            Vertex[] ret;
            List<Vertex> retTmp = new List<Vertex>();
            List<int[]> lineaments = new List<int[]>();

            using (StreamReader reader = new StreamReader(inputFileName))
            {
                // skip first 4 lines (omg, yay, best format ever)
                reader.ReadLine();
                reader.ReadLine();
                reader.ReadLine();
                reader.ReadLine();

                string points = reader.ReadLine();
                string[] pointCount = points.Split();
                if ( pointCount.Length != 3 ) { return null; }

                int countVertex = int.Parse(pointCount[1]);

                int indexRet = -1;

                int index = 0;
                double[] coord = new double[3];
                string[] separator = new string[] { " ", "\n" };
                string[] numberLines = null;

                for ( int i = 0; i < countVertex; ++i )
                {
                    string line = reader.ReadLine();
                    if ( line.Contains("LINES") ) {
                        numberLines = line.Split(' ');
                        break;
                    }

                    string[] coords = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                    for ( int j = 0; j < coords.Length; ++j)
                    {
                        double c = double.Parse(coords[j]);

                        coord[index] = c;
                        ++index;

                        if ( index >= 3 )
                        {
                            retTmp.Add(new Vertex(coord));
                            index = 0;
                        }
                    }
                }

                // LINES
                int lines = int.Parse(numberLines[1]);

                for (int i = 0; i < lines; ++i) {
                    string[] lineament = reader.ReadLine().Split(separator, StringSplitOptions.RemoveEmptyEntries);

                    int linemantVertex = int.Parse(lineament[0]);

                    int[] l = new int[linemantVertex];

                    for (int j = 0; j < linemantVertex; ++j)
                    {
                        l[j] = int.Parse(lineament[j+1]);
                    }

                    lineaments.Add(l);
                }
            }

            return retTmp.ToArray();
        }

        //public static Vertex[] LoadVertices(string inputFileName)
        //{
        //    System.Globalization.CultureInfo invarCult
        //       = System.Globalization.CultureInfo.InvariantCulture;
        //    Vertex[] ret = null;

        //    using (StreamReader reader = new StreamReader(inputFileName))
        //    {
        //        // suppose vertex count at first line
        //        long vertexCount = long.Parse(reader.ReadLine(), invarCult);
        //        if (vertexCount <= 0) { return null; }

        //        ret = new Vertex[vertexCount];

        //        for (int i = 0; i < vertexCount; ++i)
        //        {
        //            string[] tokens = reader.ReadLine().Split();
        //            if (tokens.Length < 2) { return null; }

        //            try
        //            {
        //                double x = double.Parse(tokens[0], invarCult);
        //                double y = double.Parse(tokens[1], invarCult);
        //                ret[i] = new Vertex(new double[] { x, y });
        //            }
        //            catch (Exception e)
        //            {
        //                Console.WriteLine(MethodBase.GetCurrentMethod().DeclaringType + "\n" + e.Message);
        //                return null;
        //            }
        //        }
        //    }

        //    return ret;
        //}

        public static Vertex[] LoadGEO(string inputFileName)
        {
            // we don't know how many lines is in the file
            List<Vertex> ret = new List<Vertex>();

            // any decimal point
            System.Globalization.CultureInfo invarCult
               = System.Globalization.CultureInfo.InvariantCulture;
            
            using (StreamReader reader = new StreamReader(inputFileName))
            {
                #region SKIP_HEADER

                // skip the header
                for (int f = 0; f < SKIP; ++f)
                {
                    reader.ReadLine();
                }

                #endregion

                Regex firstRegex = new Regex(@"([\s\*\(\d]+\)*)\s+([-\d\.\,]*)\s+([-\d\.\,]*)");
                Regex secondRegex = new Regex(@"([\s\*\(\d)]+)\s+([-\d\.\,]*)\s+([-\d\.\,]*)\s+([-\d\.\,]*)\s+([-\d\.\,]*)\s*([-\d\.\,]*)\s*([\+\*])*");

                Vertex temp;
                // own coords
                double[] coord = null;
                // tempory variable
                string firstLine;
                string secondLine;

                while ((firstLine = reader.ReadLine()) != null && ((secondLine = reader.ReadLine()) != null))
                {
                    MatchCollection matches = firstRegex.Matches(firstLine);

                    coord = new double[NAMES.Length];

                    foreach (Match match in matches)
                    {
                        //Console.WriteLine("{0} {1}", match.Groups[2].Value, match.Groups[3].Value);
                        //Console.WriteLine();

                        coord[0] = -double.Parse(match.Groups[2].Value, invarCult);
                        coord[1] = -double.Parse(match.Groups[3].Value, invarCult);
                    }

                    matches = secondRegex.Matches(secondLine);

                    foreach (Match match in matches)
                    {
                        //Console.WriteLine("{0} {1} {2} {3} {4}", match.Groups[2].Value, match.Groups[3].Value, match.Groups[4].Value, match.Groups[5].Value, match.Groups[6].Value);
                        //Console.WriteLine();

                        coord[2] = -double.Parse(match.Groups[2].Value, invarCult);
                        coord[3] = -double.Parse(match.Groups[3].Value, invarCult);

                        coord[4] = double.Parse(match.Groups[4].Value, invarCult);
                        coord[5] = double.Parse(match.Groups[5].Value, invarCult);

                        // take this vertex if Groups[6] is symbol "+"
                        // match.Groups[6].Value;
                    }

                    // temp will be use twice
                    temp = new Vertex(coord);

                    ret.Add(temp);
                }
            }

            // setup
            // compute coeficient to normalize coords
            return ret.ToArray();
        }

        public static DataSet LoadVtx(string inputFileName, int maximalVertices)
        {
            DataSet ret = new DataSet();
            System.Globalization.CultureInfo invarCult = System.Globalization.CultureInfo.InvariantCulture;

            using ( StreamReader reader = new StreamReader(inputFileName) )
            {
                string line = reader.ReadLine();

                // first line > number of vertices and dimension
                string[] tokens = line.Split();
                int vertices = int.Parse(tokens[0]) > maximalVertices ? maximalVertices : int.Parse(tokens[0]);
                int dimension = int.Parse(tokens[1]);

                string[] names = reader.ReadLine().Split();
                
                // read boundaries
                tokens = reader.ReadLine().Split();

                double[] max = new double[dimension];
                double[] min = new double[dimension];
                double[] interval = new double[dimension];

                for (int i = 0; i < dimension; ++i)
                {
                    min[i] = double.Parse(tokens[i*2]);
                    max[i] = double.Parse(tokens[i*2+1]);

                    interval[i] = max[i] - min[i];
                }

                // skip delimiter
                reader.ReadLine();

                // read points
                Vertex[] points = new Vertex[vertices];
                for ( int i = 0; i < vertices; ++i )
                {
                    double[] coord = new double[dimension];

                    tokens = reader.ReadLine().Split();

                    //for (int j = 0; j < dimension; ++j)
                    //{
                    //    coord[j] = (double.Parse(tokens[j], invarCult) - min[j]) / interval[j];
                    //}

                    for (int j = 0; j < dimension; ++j)
                    {
                        coord[j] = double.Parse(tokens[j], invarCult);
                    }

                    points[i] = new Vertex(coord);
                }

                // TODO DEBUG calculate boundaries
                max = new double[dimension];
                min = new double[dimension];
                interval = new double[dimension];

                for (int i = 0; i < dimension; ++i)
                {
                    max[i] = double.MinValue;
                    min[i] = double.MaxValue;
                }

                for (int i = 0; i < dimension; ++i)
                {
                    for (int j = 0; j < points.Length; ++j)
                    {
                        if( max[i] < points[j][i]) { max[i] = points[j][i]; }
                        if( min[i] > points[j][i]) { min[i] = points[j][i]; }
                    }
                }

                for (int i = 0; i < dimension; ++i)
                {
                    interval[i] = max[i] - min[i];
                }

                // set properties
                ret.points = points;
                ret.minCorner = min;
                ret.maxCorner = max;
                ret.name = names;
                ret.interval = interval;
            }

            return ret;
        }
    }
}

