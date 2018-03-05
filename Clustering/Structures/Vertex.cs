/*
 * Client for the facility location.
 * 
 * Ondrej Kaas
 */
using System;

namespace Zcu.Graphics.Clustering
{
	public struct Vertex
	{
		/// <summary>
		/// Vertex coordinates.
		/// </summary>
		/// <remarks>
		/// If you change the type, remember to update CoordinateSize and ReadBinary().
		/// </remarks>
		private readonly double[] coords;

		/// <summary>
		/// The size in bytes of a single coordinate.
		/// </summary>
		private const int CoordinateSize = sizeof(double);

		/// <summary>
		/// Weights of particular coordinates. The weights are the same for all the vertices.
		/// If null, no weights are used.
		/// </summary>
		private static double[] coordWeights = null;

		/// <summary>
		/// Vertex weight. Initialized to 1.
		/// </summary>
		/// <remarks>
		/// If vertex was previously a facility, weight equals to the sum of weights of all vertices
		/// assigned to the facility. Some normalization will be done afterwards.
		/// </remarks>
		private double weight;

		/// <summary>
		/// Reference to the facility for this vertex.
		/// </summary>
		private Facility facility;

		/// <summary>
		/// Is this vetex a facility?
		/// </summary>
		private bool isFacility;

		/// <summary>
		/// The weighted distance to the facility.
		/// If isFacility == true then distToFacility == 0.
		/// </summary>
		private double weightedDistToFac;





		/// <summary>
		/// Constructor. Creates vertex with 3 coordinates.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <param name="z">Z coordinate.</param>
		public Vertex(double x, double y, double z)
		{
			// check dimension
			//if (coordWeights != null && coordWeights.Length != 3)
			//	throw new ApplicationException("The number of coordinates does not match "
			//		+ "the number of coordinate weights.");

			// store coordinates
			coords = new double[3] { x, y, z };

			// initialise unit weight
			weight = 1;

			// initialise - not assigned to any facility
			facility = null;
			isFacility = false;
			weightedDistToFac = 0;
		}

		/// <summary>
		/// Constructor. Creates a vertex with arbitrary number of coordinates.
		/// All the coordinates have the same weight.
		/// </summary>
		/// <remarks>
		/// Makes and stores a shallow copy of the array of coordinates.
		/// </remarks>
		/// <param name="coordinates">Array of coordinates.</param>
		public Vertex(double[] coordinates)
		{
			// check dimension
			if (coordWeights != null && coordWeights.Length != coordinates.Length)
				throw new ApplicationException("The number of coordinates does not match "
					+ "the number of coordinate weights.");

			// store coordinates
			coords = (double[])coordinates.Clone();

			// initialise unit weight
			weight = 1;

			// initialise - not assigned to any facility
			facility = null;
			isFacility = false;
			weightedDistToFac = 0;
		}

		/// <summary>
		/// Gets or sets the weight of particular coordinates.
		/// If the array of weights is set to null, no weights will be used.
		/// </summary>
		/// <remarks>
		/// Gets or sets a shallow copy of the coordinates.
		/// </remarks>
		public static double[] CoordWeights
		{
			get { return coordWeights == null ? null : (double[])coordWeights.Clone(); }
			set { coordWeights = value == null ? null : (double[])value.Clone(); }
		}

        public double[] Coords
        {
            get { return (double[])coords.Clone(); }
        }

        /// <summary>
        /// Gets the x coordinate.
        /// </summary>
        public double X
		{
			get { return coords[0]; }
		}

		/// <summary>
		/// Gets the y coordinate.
		/// </summary>
		public double Y
		{
			get { return coords[1]; }
		}

		/// <summary>
		/// Gets the z coordinate.
		/// </summary>
		public double Z
		{
			get { return coords[2]; }
		}

		/// <summary>
		/// Indexer. Gets one particular coordinate.
		/// </summary>
		public double this[int index]
		{
			get { return coords[index]; }
            set { coords[index] = value; }
		}
        
		/// <summary>
		/// Gets the dimension of the vertex (the number of coordinates).
		/// </summary>
		public int Dimension
		{
			get { return coords.Length; }
		}

		/// <summary>
		/// Gets or sets the weight of this vertex.
		/// </summary>
		public double VertexWeight
		{
			get { return weight; }
			set { weight = value; }
		}

		/// <summary>
		/// Gets the coordinates separated by spaces.
		/// </summary>
		/// <returns>Returns string representation.</returns>
		public override string ToString()
		{
			// store the first coordinate
			System.Text.StringBuilder s = new System.Text.StringBuilder(coords[0].ToString());

			// store the following coordinates delimited by a space
			for (int i = 1; i < coords.Length; i++)
				s.Append(" " + coords[i]);

			return s.ToString();
		}

		/// <summary>
		/// Saves the vertex coordinates to a binary file.
		/// </summary>
		/// <param name="fileOut">The output file.</param>
		/// <returns>Returns the number of bytes written.</returns>
		public int SaveBinary(System.IO.BinaryWriter fileOut)
		{
			// save the coordinates
			for (int i = 0; i < coords.Length; i++)
				fileOut.Write(coords[i]);

			// compute the number of bytes
			return coords.Length * CoordinateSize;
		}

		/// <summary>
		/// Reads the vertex coordinates from a binary file.
		/// </summary>
		/// <param name="fileIn">The input file.</param>
		/// <param name="coordCount">The number of coordinates to read.</param>
		/// <returns>Returns the read vertex.</returns>
		public static Vertex ReadBinary(System.IO.BinaryReader fileIn, ushort coordCount)
		{
			// check that we are loading the right type
			System.Diagnostics.Debug.Assert(CoordinateSize == sizeof(double));

			double[] c = new double[coordCount];

			// read the coordinates
			for (int i = 0; i < coordCount; i++)
				c[i] = fileIn.ReadDouble();

			return new Vertex(c);
		}

		/// <summary>
		/// Gets the facility for this vertex.
		/// </summary>
		public Facility Facility
		{
			get { return facility; }
			set { facility = value; }		// use AssignToFacility instead
		}

		/// <summary>
		/// Gets or sets whether this vertex is a facility.
		/// </summary>
		public bool IsFacility
		{
			get { return isFacility; }
			set { isFacility = value; }
		}

		/// <summary>
		/// Gets the weighted distance to the current facility.
		/// </summary>
		public double WeightedDistToFac
		{
			get { return weightedDistToFac; }
			//set { weightedDistToFac = value; }	// computed in AssignToFacility
		}

		/// <summary>
		/// Gets the non-weighted distance to the current facility.
		/// </summary>
		internal double NonWeightedDistToFac
		{
			get { return weightedDistToFac / weight; }
		}

		/// <summary>
		/// Assigns this vertex to a facility.
		/// </summary>
		/// <param name="facility">The facility.</param>
		/// <param name="weightedDistance">The weighted distance to the facility.</param>
		public void AssignToFacility(Facility facility, double weightedDistance)
		{
			this.facility = facility;
			this.weightedDistToFac = weightedDistance;
		}

		/// <summary>
		/// Computes the distance to the other vertex.
		/// </summary>
		/// <param name="vert">Vertex to which compute distance.</param>
		/// <returns>Returns the distance to given vertex.</returns>
		public double WeightedDistance(Vertex v)
		{
			// check dimensions
			int dim = this.Dimension;
			if (dim != v.Dimension)
				throw new ApplicationException("Vertex dimensions do not match.");

			double diffSqrSum = 0;

			// check whether coordinate weights are defined
			if (coordWeights == null)
				// compute and sum up the squared differences
				for (int i = 0; i < dim; i++)
				{
					double d = v.coords[i] - this.coords[i];
					diffSqrSum += d * d;
				}
			else
			{
				// The length of coords and coordWeights is checked upon Vertex creation.
				// Let's hope the coordWeights did not change.

				// compute and sum up the squared differences multiplied by the appropriate weights
				for (int i = 0; i < dim; i++)
				{
					double d = (v.coords[i] - this.coords[i]) * coordWeights[i];
					diffSqrSum += d * d;
				}
			}

			// Euclidean distance
			return Math.Sqrt(diffSqrSum);
		}

		/// <summary>
		/// Computes the weighted distance to the other vertex.
		/// Works in orthogonal projection into xy plane.
		/// </summary>
		/// <param name="vert">Vertex to which compute the weighted distance.</param>
		/// <returns>Returns the weighted distance to given vertex.</returns>
		public double WeightedDistance2D(Vertex v)
		{
			double dx = v.X - this.X;
			double dy = v.Y - this.Y;

			// do coordinates have weights?
			if (coordWeights != null)
			{
				// The length of coords and coordWeights is checked upon Vertex creation.
				// Let's hope the coordWeights did not change.

				dx *= coordWeights[0];
				dy *= coordWeights[1];
			}

			// weighted Euclidean distance in xy plane.
			return this.weight * Math.Sqrt(dx*dx + dy*dy);
		}

		/// <summary>
		/// Computes weighted Manhattan distance to the other vertex.
		/// </summary>
		/// <param name="vert">Vertex to which compute the distance.</param>
		/// <returns>Returns weighted Manhattan distance to given vertex.</returns>
		public double WeightedDistanceManhattan(Vertex v)
		{
			// check dimensions
			int dim = this.Dimension;
			if (dim != v.Dimension)
				throw new ApplicationException("Vertex dimensions do not match.");

			double diffSum = 0;
			
			// check whether coordinate weights are defined
			if (coordWeights == null)
				// compute and sum up the differences
				for (int i = 0; i < dim; i++)
					diffSum += Math.Abs(v.coords[i] - this.coords[i]);
			else
			{
				// The length of coords and coordWeights is checked upon Vertex creation.
				// Let's hope the coordWeights did not change.

				// compute and sum up the differences multiplied by the appropriate weights
				for (int i = 0; i < dim; i++)
					diffSum += Math.Abs(v.coords[i] - this.coords[i]) * coordWeights[i];
			}

			// weighted Manhattan distance
			return this.weight * diffSum;
		}



		#region Legacy distance methods

		[Obsolete("The Distance method was renamed to WeightedDistance to better match what it computes.")]
		public double Distance(Vertex v)
		{
			return WeightedDistance(v);
		}

		[Obsolete("The Distance2D method was renamed to WeightedDistance2D to better match what it computes.")]
		public double Distance2D(Vertex v)
		{
			return WeightedDistance2D(v);
		}

		[Obsolete("The Distance method was renamed to WeightedDistance to better match what it computes.")]
		public double DistanceManhattan(Vertex v)
		{
			return WeightedDistanceManhattan(v);
		}

		#endregion

		

		#region Vertex arithmetics

		/// <summary>
		/// Vector addition.
		/// </summary>
		public static Vertex operator +(Vertex a, Vertex b)
		{
			System.Diagnostics.Debug.Assert(a.Dimension == b.Dimension);

			double[] coords = new double[a.Dimension];

			// add the coordinates
			for (int i = 0; i < a.Dimension; i++)
				coords[i] = a[i] + b[i];

			return new Vertex(coords);
		}

		/// <summary>
		/// Vector subtraction.
		/// </summary>
		public static Vertex operator -(Vertex a, Vertex b)
		{
			System.Diagnostics.Debug.Assert(a.Dimension == b.Dimension);

			double[] coords = new double[a.Dimension];

			// subtract the coordinates
			for (int i = 0; i < a.Dimension; i++)
				coords[i] = a[i] - b[i];

			return new Vertex(coords);
		}

		/// <summary>
		/// Vector inverse.
		/// </summary>
		public static Vertex operator -(Vertex v)
		{
			double[] coords = new double[v.Dimension];

			// invert the coordinates
			for (int i = 0; i < v.Dimension; i++)
				coords[i] = -v[i];

			return new Vertex(coords);
		}

		/// <summary>
		/// Multiplication by a constant.
		/// </summary>
		public static Vertex operator *(double c, Vertex v)
		{
			double[] coords = new double[v.Dimension];

			// multiply the coordinates
			for (int i = 0; i < v.Dimension; i++)
				coords[i] = c * v[i];

			return new Vertex(coords);
		}

        public static Vertex operator /(double c, Vertex v)
        {
            double[] coords = new double[v.Dimension];

            // multiply the coordinates
            for (int i = 0; i < v.Dimension; i++)
                coords[i] = v[i] / 2;

            return new Vertex(coords);
        }

        #endregion
    }
}
