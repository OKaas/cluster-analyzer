/*
 * Datastream clusterer
 * 
 * Ondrej Kaas
 */
using System;

namespace Zcu.Graphics.Clustering
{
	/// <summary>
	/// Bounding box of points in 3D.
	/// </summary>
	public struct BoundingBox
	{
		/// <summary>
		/// Bounding box corner points.
		/// </summary>
		public double[] minCorner, maxCorner;




		/// <summary>
		/// Constructor. Initializes bounding box to the first point inserted.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <param name="z">Z coordinate.</param>
		public BoundingBox(double x, double y, double z)
		{
			// initialize bounds to the first point
			minCorner = new double[] { x, y, z };
			maxCorner = new double[] { x, y, z };
            Initialized = false;
        }

		/// <summary>
		/// Constructor. Initializes bounding box to the first point inserted.
		/// </summary>
		/// <param name="v">The first vertex.</param>
		public BoundingBox(Vertex v)
		{
			minCorner = new double[v.Dimension];
			maxCorner = new double[v.Dimension];
            Initialized = false;

            // initialize bounds to the first point
            for (int i = 0; i < Dimension; i++)
				minCorner[i] = maxCorner[i] = v[i];
        }

		/// <summary>
		/// Initializes the bounding box to an empty box.
		/// </summary>
		public void Initialize(int dimension)
		{
			minCorner = new double[dimension];
			maxCorner = new double[dimension];
            Initialized = true;

            // initialize bounds to extremes
            for (int i = 0; i < Dimension; i++)
			{
				minCorner[i] = double.PositiveInfinity;
				maxCorner[i] = double.NegativeInfinity;
			}
		}

        public void Initialize(double[] min, double[] max)
        {
            minCorner = min;
            maxCorner = max;

            //Array.Copy(min, minCorner, min.Length);
            //Array.Copy(max, maxCorner, max.Length);

            Initialized = true;
        }


		#region Field accessors

		/// <summary>
		/// Gets bounding box dimension, i.e., the number of coordinates.
		/// </summary>
		public int Dimension
		{
			get { return minCorner.Length; }
		}

		/// <summary>
		/// Gets or sets the lower x bound.
		/// </summary>
		public double MinX
		{
			get { return minCorner[0]; }
			set { minCorner[0] = value; }
		}

		/// <summary>
		/// Gets or sets the upper x bound
		/// </summary>
		public double MaxX
		{
			get { return maxCorner[0]; }
			set { maxCorner[0] = value; }
		}

		/// <summary>
		/// Gets or sets the lower y bound.
		/// </summary>
		public double MinY
		{
			get { return minCorner[1]; }
			set { minCorner[1] = value; }
		}

		/// <summary>
		/// Gets or sets the upper y bound
		/// </summary>
		public double MaxY
		{
			get { return maxCorner[1]; }
			set { maxCorner[1] = value; }
		}

		/// <summary>
		/// Gets or sets the lower z bound
		/// </summary>
		public double MinZ
		{
			get { return minCorner[2]; }
			set { minCorner[2] = value; }
		}

		/// <summary>
		/// Gets or sets the upper z bound
		/// </summary>
		public double MaxZ
		{
			get { return maxCorner[2]; }
			set { maxCorner[2] = value; }
		}

		/// <summary>
		/// Gets the lower bound in the given dimension.
		/// </summary>
		/// <param name="dimension">Dimension index.</param>
		/// <returns>Returns the lower bound in the given dimension.</returns>
		public double GetMin(int dimension)
		{
			return minCorner[dimension];
		}

		/// <summary>
		/// Sets the lower bound in the given dimension.
		/// </summary>
		/// <param name="dimension">Dimension index.</param>
		/// <param name="value">The lower bound value.</param>
		public void SetMin(int dimension, double value)
		{
			minCorner[dimension] = value;
		}

		/// <summary>
		/// Gets the upper bound in the given dimension.
		/// </summary>
		/// <param name="dimension">Dimension index.</param>
		/// <returns>Returns the upper bound in the given dimension.</returns>
		public double GetMax(int dimension)
		{
			return maxCorner[dimension];
		}

		/// <summary>
		/// Sets the upper bound in the given dimension.
		/// </summary>
		/// <param name="dimension">Dimension index.</param>
		/// <param name="value">The upper bound value.</param>
		public void SetMax(int dimension, double value)
		{
			maxCorner[dimension] = value;
		}

		#endregion


		#region Size properties

		/// <summary>
		/// Gets bounding box width, i.e., size in the first (x) dimension.
		/// </summary>
		public double Width
		{
			get { return maxCorner[0] - minCorner[0]; }
		}

		/// <summary>
		/// Gets bounding box height, i.e., size in the second (y) dimension.
		/// </summary>
		public double Height
		{
			get { return maxCorner[1] - minCorner[1]; }
		}

		/// <summary>
		/// Gets bounding box depth, i.e., size in the third (z) dimension.
		/// </summary>
		public double Depth
		{
			get { return maxCorner[2] - minCorner[2]; }
		}

        public bool Initialized { get; set; }

        /// <summary>
        /// Gets the box size in the specified dimension.
        /// </summary>
        /// <param name="dimension">Dimension index.</param>
        /// <returns>Returns the box size in the specified dimension.</returns>
        public double Size(int dimension)
		{
			return maxCorner[dimension] - minCorner[dimension];
		}

		#endregion


		/// <summary>
		/// Computes the diagonal of the bounding box.
		/// </summary>
		/// <returns>Returns the diagonal length.</returns>
		public double GetDiagonal()
		{
			double sum = 0;

			// sum up the squares of the box sizes in all dimensions
			for (int i = 0; i < Dimension; i++)
			{
				double s = Size(i);
				sum += s * s;
			}

			return Math.Sqrt(sum);
		}

		/// <summary>
		/// Computes diagonal in xy plane.
		/// </summary>
		/// <returns>Returns diagonal in xy plane.</returns>
		public double GetDiagonalXY()
		{
			double dx = Width;
			double dy = Height;

			return Math.Sqrt(dx*dx + dy*dy);
		}

		/// <summary>
		/// Computes diagonal in xyz plane.
		/// </summary>
		/// <returns>Returns diagonal in xyz plane.</returns>
		public double GetDiagonalXYZ()
		{
			double dx = Width;
			double dy = Height;
			double dz = Depth;

			return Math.Sqrt(dx*dx + dy*dy + dz*dz);
		}

		/// <summary>
		/// Computes the diagonal with respect to vertex coordinate weights.
		/// </summary>
		/// <returns>Returns the box diagonal.</returns>
		public double GetWeightedDiagonal()
		{
			// if no weights are defined
			if (Vertex.CoordWeights == null)
				return GetDiagonal();

			double sum = 0;

			// sum up the squares of the box sizes in all dimensions
			for (int i = 0; i < Dimension; i++)
			{
				// multiply the size by the weight
				double s = Size(i) * Vertex.CoordWeights[i];
				sum += s * s;
			}

			return Math.Sqrt(sum);
		}

		/// <summary>
		/// Updates bounding box according to new point.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <param name="z">Z coordinate.</param>
		public void AddVertex(double x, double y, double z)
		{
			System.Diagnostics.Debug.Assert(Dimension == 3, "Dimension missmatch.");

			if (x < minCorner[0])
				minCorner[0] = x;
			if (x > maxCorner[0])
				maxCorner[0] = x;

			if (y < minCorner[1])
				minCorner[1] = y;
			if (y > maxCorner[1])
				maxCorner[1] = y;

			if (z < minCorner[2])
				minCorner[2] = z;
			if (z > maxCorner[2])
				maxCorner[2] = z;
		}

        public void AddVertices(Vertex[] vertices)
        {
            foreach (Vertex v in vertices)
            {
                AddVertex(v);
            }
        }

        /// <summary>
        /// Updates bounding box according to the new vertex.
        /// </summary>
        /// <param name="v">New vertex to enclose into bounding box.</param>
        public void AddVertex(Vertex v)
		{
			System.Diagnostics.Debug.Assert(this.Dimension == v.Dimension, "Dimension missmatch.");

			// go through all dimensions
			for (int i = 0; i < Dimension; i++)
			{
				// check minimum
				if (v[i] < minCorner[i])
					minCorner[i] = v[i];

				// check maximum
				if (v[i] > maxCorner[i])
					maxCorner[i] = v[i];
			}
		}

		/// <summary>
		/// Adds given bounding box to this one.
		/// </summary>
		/// <param name="box">Bounding box to add.</param>
		public void AddBox(BoundingBox box)
		{
			System.Diagnostics.Debug.Assert(this.Dimension == box.Dimension, "Dimension missmatch.");

			// enlarge this bounding box to enclose also the given box
			this.AddVertex(new Vertex(box.minCorner));
			this.AddVertex(new Vertex(box.maxCorner));
		}

		/// <summary>
		/// Adds given bounding box to this one.
		/// Results in new bounding box.
		/// </summary>
		/// <param name="box">Bounding box to add.</param>
		/// <returns>Returns union of this and given bounding box.</returns>
		public BoundingBox GetUnion(BoundingBox box)
		{
			// prepare a new box
			BoundingBox newBox = new BoundingBox();
			newBox.Initialize(this.Dimension);

			// copy the current box
			newBox.AddBox(this);

			// enlarge to enclose also the given box
			newBox.AddBox(box);		// the dimensions will be checked here

			return newBox;
		}
	}
}
