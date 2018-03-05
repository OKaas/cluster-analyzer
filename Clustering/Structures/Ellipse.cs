/*
 * Data stream clusterer
 * 
 * Ondrej Kaas
 */
using System;

namespace Zcu.Graphics.Clustering
{
	/// <summary>
	/// Region of elliptical space deformation.
	/// </summary>
	public struct Ellipse
	{
		/// <summary>
		/// Boundaries of the region affected by this ellipse.
		/// </summary>
		private double minX, minY, maxX, maxY;

		/// <summary>
		/// Transformation matrix.
		/// </summary>
		private double m00, m01, m11;





		/// <summary>
		/// Initializes a new ellipse.
		/// </summary>
		/// <param name="minX">Left border.</param>
		/// <param name="minY">Bottom border.</param>
		/// <param name="maxX">Right border.</param>
		/// <param name="maxY">Top border.</param>
		/// <param name="a">Ellipse semimajor axis.</param>
		/// <param name="b">Ellipse semiminor axis.</param>
		/// <param name="angle">Ellipse angle of rotation.</param>
		public Ellipse(double minX, double minY, double maxX, double maxY,
			double a, double b, double angle)
		{
			// store boundaries
			this.minX = minX;
			this.minY = minY;
			this.maxX = maxX;
			this.maxY = maxY;

			// compute transformation matrix
			double sin = Math.Sin(angle);
			double cos = Math.Cos(angle);

			double aRecipSqr = 1 / (a*a);
			double bRecipSqr = 1 / (b*b);

			m00 = aRecipSqr * cos * cos + bRecipSqr * sin * sin;
			m01 = (aRecipSqr - bRecipSqr) * sin * cos;
			m11 = aRecipSqr * sin * sin + bRecipSqr * cos * cos;
		}

		/// <summary>
		/// Gets the scaling part in the x axis.
		/// </summary>
		public double ScaleX
		{
			get { return m00; }
		}

		/// <summary>
		/// Gets the scaling part in the y axis.
		/// </summary>
		public double ScaleY
		{
			get { return m11; }
		}

		/// <summary>
		/// Initializes a new special ellipse with no deformation.
		/// </summary>
		/// <param name="circle">If true, the ellipse is initialized to a circle
		/// (e.i., ellipse with no deformation). Otherwise the ellipse is initialized
		/// as default (everything to zero).</param>
		public Ellipse(bool circle)
		{
			if (circle)
			{
				// circle
				m00 = m11 = 1;
				m01 = 0;
				// everywhere
				minX = minY = double.NegativeInfinity;
				maxX = maxY = double.PositiveInfinity;
			}
			else
			{
				// everything zero
				m00 = m01 = m11 = 0;
				minX = minY = maxX = maxY = 0;
			}
		}

		/// <summary>
		/// Computes 2D elliptical distance between given points.
		/// </summary>
		/// <param name="va">The first point. The weight is taken from this point.</param>
		/// <param name="vb">The second point.</param>
		/// <returns>Returns 2D elliptical distance between points.</returns>
		public double EllipticalDistance2D(Vertex va, Vertex vb)
		{
			double dx = vb.X - va.X;
			double dy = vb.Y - va.Y;

			return va.VertexWeight * Math.Sqrt(m00*dx*dx + 2*m01*dx*dy + m11*dy*dy);
		}

		/// <summary>
		/// Computes elliptical distance between given points.
		/// </summary>
		/// <param name="va">The first point. The weight is taken from this point.</param>
		/// <param name="vb">The second point.</param>
		/// <returns>Returns elliptical distance between points.</returns>
		public double EllipticalDistance(Vertex va, Vertex vb)
		{
			double dx = vb.X - va.X;
			double dy = vb.Y - va.Y;
			double dz = vb.Z - va.Z;

			return va.VertexWeight * Math.Sqrt(m00*dx*dx + 2*m01*dx*dy + m11*dy*dy + dz*dz);
		}

		/// <summary>
		/// Determines whether given vertex is inside the area affected by this ellipse.
		/// </summary>
		/// <param name="v">The vertex to look for.</param>
		/// <returns>Returns true if the vertex is inside, otherwise returns false.</returns>
		public bool IsVertexInside(Vertex v)
		{
			return minX <= v.X && v.X <= maxX
				&& minY <= v.Y && v.Y <= maxY;
		}
	}
}
