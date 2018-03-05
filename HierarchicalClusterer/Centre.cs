/*
 * Hierarchical clusterer
 * 
 * Ondrej Kaas
 * 
 */
using System;

namespace Zcu.Graphics.Clustering
{
	/// <summary>
	/// Cluster centre. Contains a vertex and some additional information.
	/// </summary>
	public struct Centre
	{
		/// <summary>
		/// The centre coordinates.
		/// </summary>
		public Vertex vertex;

		/// <summary>
		/// Address to a file where the cluster members are stored.
		/// </summary>
		/// <remarks>
		/// If you change the type, remember to update FileAddressSize and ReadBinary().
		/// </remarks>
		public long childrenFileStart;

		/// <summary>
		/// The size in bytes of the address into the file.
		/// </summary>
		private const int FileAddressSize = sizeof(long);

		/// <summary>
		/// Index into the array of points loaded in the memory.
		/// </summary>
		public int childrenMemoryStart;

		/// <summary>
		/// The number of cluster members.
		/// </summary>
		/// <remarks>
		/// If you change the type, remember to update ChildrenCountSize and ReadBinary().
		/// </remarks>
		public int childrenCount;

		/// <summary>
		/// The size in bytes of the children count.
		/// </summary>
		private const int ChildrenCountSize = sizeof(int);

		/// <summary>
		/// Is this cluster centre expanded, i.e., cluster members are loaded?
		/// </summary>
		public bool isExpanded;

		/// <summary>
		/// Experimental.
		/// Radius of the cluster bounding sphere.
		/// </summary>
		public double radius;

		/// <summary>
		/// Experimental.
		/// Is this centre duplicate?
		/// </summary>
		public bool isDuplicate;





		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="v">Coordinates of the cluster centre.</param>
		public Centre(Vertex vertex)
		{
			// store the vertex
			this.vertex = vertex;

			// initialize fields
			childrenFileStart = -1;
			childrenMemoryStart = -1;
			childrenCount = 0;
			isExpanded = false;

			radius = 0;
			isDuplicate = false;
		}
		
		/// <summary>
		/// Gets a text representation of the cluster centre.
		/// </summary>
		public override string ToString()
		{
			return vertex.ToString() + "\t" + childrenFileStart + "\t" + childrenCount;
		}

		/// <summary>
		/// Saves the cluster centre to a binary file.
		/// </summary>
		/// <param name="fileOut">The output file.</param>
		/// <returns>Returns the number of bytes written.</returns>
		public int SaveBinary(System.IO.BinaryWriter fileOut)
		{
			// save the vertex coordinates
			int bytesWritten = vertex.SaveBinary(fileOut);

			// save children information
			fileOut.Write(childrenFileStart);
			bytesWritten += FileAddressSize;

			fileOut.Write(childrenCount);
			bytesWritten += ChildrenCountSize;

			return bytesWritten;
		}

		/// <summary>
		/// Reads a cluster centre from a binary file.
		/// </summary>
		/// <param name="fileIn">The input file.</param>
		/// <param name="coordCount">The number of vertex coordinates to read.</param>
		/// <returns>Returns the read centre.</returns>
		public static Centre ReadBinary(System.IO.BinaryReader fileIn, ushort coordCount)
		{
			// check that we are loading the right types
			System.Diagnostics.Debug.Assert(FileAddressSize == sizeof(long));
			System.Diagnostics.Debug.Assert(ChildrenCountSize == sizeof(int));

			// create the cluster centre
			Centre c = new Centre();
			// read the vertex coordinates
			c.vertex = Vertex.ReadBinary(fileIn, coordCount);

			// read children information
			c.childrenFileStart = fileIn.ReadInt64();
			c.childrenCount = fileIn.ReadInt32();

			return c;
		}

		/// <summary>
		/// Reads just the coordinates of a vertex from a binary file.
		/// Used for the vertices at the level zero, where there are no children infomration.
		/// </summary>
		/// <param name="fileIn">The input file.</param>
		/// <param name="coordCount">The number of vertex coordinates to read.</param>
		/// <returns>Returns the read vertex.</returns>
		public static Centre ReadBinaryJustCoords(System.IO.BinaryReader fileIn, ushort coordCount)
		{
			// create the cluster centre
			Centre c = new Centre();
			// read the vertex coordinates
			c.vertex = Vertex.ReadBinary(fileIn, coordCount);

			// no children
			c.childrenFileStart = 0;
			c.childrenCount = 0;

			return c;
		}
	}
}
