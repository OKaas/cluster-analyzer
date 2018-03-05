/*
 * Cluster Viewer
 * 
 * Cluster Loader - loads clusters from a file.
 * 
 * Jiri Skala, March 2010
 * 
 */
//#define LOAD_RADII_DUPL

using System;
using System.IO;

namespace Zcu.Graphics.Clustering
{
	/// <summary>
	/// Loads clusters from a file.
	/// </summary>
	public class ClusterLoader
	{
		/// <summary>
		/// General file name without the level number.
		/// </summary>
		private string generalFileName;

		/// <summary>
		/// The highest level number.
		/// </summary>
		private int highestLevel;





		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="inputFileName">The file containing the highest level of clusters.</param>
		public ClusterLoader(string inputFileName)
		{
			// get the top level number and a general file name
			highestLevel = ExtractGeneralFileName(inputFileName);
		}

		/// <summary>
		/// Gets the highest level number.
		/// </summary>
		public int HighestLevel
		{
			get { return highestLevel; }
		}

		/// <summary>
		/// Extracts a general file name without the level number.
		/// </summary>
		/// <param name="fileName">File name with any level number.</param>
		/// <returns>Returns the number that was appended to the file name.</returns>
		private int ExtractGeneralFileName(string fileName)
		{
			// prepare regular expression to match a numbered file name
			// the mask is <underscore><1 or 2 digits><dot><anything except dot>
			System.Text.RegularExpressions.Regex regexp
				= new System.Text.RegularExpressions.Regex(@"_([0-9]{1,2})(\.[^.]*)");

			// strip out the number from the file name
			generalFileName = regexp.Replace(fileName, "$2");

			// get the file number
			System.Text.RegularExpressions.Match match = regexp.Match(fileName);

			return int.Parse(match.Result("$1"));
		}

		/// <summary>
		/// Constructs a file name with given level number.
		/// </summary>
		/// <param name="level">The level number to include in the file name.</param>
		/// <returns>Returns the file name with the given level number.</returns>
		private string GetNumberedFileName(int level)
		{
			// prepare regular expression
			System.Text.RegularExpressions.Regex regexp
				= new System.Text.RegularExpressions.Regex(@"(\.[^.]*)");

			// insert the level number before the file extension
			return regexp.Replace(generalFileName, "_" + level + "$1");

			// could by done by Path.GetFileNameWithoutExtension
		}

		///// <summary>
		///// Finds the highest level available for the given file name.
		///// </summary>
		///// <param name="fileName">File name of any level.</param>
		///// <returns>Returns the maximal level number.</returns>
		//public int MaximumAvailableLevel(string fileName)
		//{
		//    // get the general file name and the current level
		//    int maxLevel = ExtractGeneralFileName(fileName);

		//    // construct appropriate file name
		//    //string name = GetNumberedFileName(maxLevel++);

		//    do
		//    {
		//        fileName = GetNumberedFileName(maxLevel);
		//        maxLevel++;
		//    }
		//    while (File.Exists(fileName));

		//    // the current value of maxLevel was not found, so return maxlevel-1
		//    return maxLevel - 1;
		//}

		///// <summary>
		///// Checks consistency of given level.
		///// All vertices should be referenced by cluster centres exactly once.
		///// </summary>
		///// <param name="level">The level to check.</param>
		///// <returns>Returns true if the level is consistent.</returns>
		//private bool IsConsistent(short level)
		//{
		//    // prepare reference counters
		//    byte[] referenced = new byte[vertData[level].Length];
		//    Array.Clear(referenced, 0, referenced.Length);

		//    // get the array of cluster centres at the level above
		//    Vertex[] centres = vertData[level+1];

		//    // check all cluster centres
		//    for (int i = 0; i < centres.Length; i++)
		//    {
		//        // get the current centre
		//        Vertex c = centres[i];

		//        // is this cluster expanded?
		//        if (c.expanded)
		//        {
		//            // first check children start and count
		//            if (c.childrenLoadedStart < 0 || c.childrenCount <= 0
		//                || c.childrenLoadedStart+c.childrenCount > referenced.Length)
		//                return false;

		//            // increment referenced children (i.e., vertices at the level below)
		//            for (int j = c.childrenLoadedStart; j < c.childrenLoadedStart+c.childrenCount; j++)
		//                referenced[j]++;
		//        }
		//        else
		//            // cluster not expanded, should have childrenLoadStart == -1
		//            if (c.childrenLoadedStart >= 0 /*|| c.childrenCount > 0*/)
		//                return false;
		//    }

		//    // now check reference counters
		//    for (int i = 0; i < referenced.Length; i++)
		//    {
		//        // all counters must be exactly one
		//        if (referenced[i] != 1)
		//            return false;
		//    }

		//    // if we got down here, everything was OK
		//    return true;
		//}

		/// <summary>
		/// Loads clusters from the top level.
		/// </summary>
		/// <returns>Returns an array of vertices.</returns>
		public Centre[] LoadTopLevelClusters()
		{
			return LoadClusters(highestLevel);
		}

		/// <summary>
		/// Loads clusters from the specified level.
		/// </summary>
		/// <param name="level">Number of the level from where to load the data.</param>
		/// <returns>Returns an array of vertices.</returns>
		public Centre[] LoadClusters(int level)
		{
			// open input file
			string fileName = GetNumberedFileName(level);
			FileStream streamIn = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			BinaryReader fileIn = new BinaryReader(streamIn);

			// read the number of coordinates each vertex has
			ushort coordCount = fileIn.ReadUInt16();

			int vertexSize = sizeof(double) * coordCount;
			int clusterCentreSize = vertexSize + sizeof(long) + sizeof(int);

			// determine the number of points in the file
			long fileLengthExclHeader = streamIn.Length - sizeof(UInt16);

			int vertexCount;
			if (level == 0)
				// zero level vertices without pointers to cluster members
				vertexCount = (int)fileLengthExclHeader / vertexSize;
			else
				vertexCount = (int)fileLengthExclHeader / clusterCentreSize;

			// prepare memory
			Centre[] vertices = new Centre[vertexCount];

			// load the data
			if (level > 0)
				for (int i = 0; i < vertexCount; i++)
					vertices[i] = Centre.ReadBinary(fileIn, coordCount);
			else
				for (int i = 0; i < vertexCount; i++)
					vertices[i] = Centre.ReadBinaryJustCoords(fileIn, coordCount);

			// close input file
			fileIn.Close();
			streamIn.Close();

			return vertices;
		}

		/// <summary>
		/// Loads compressed clusters from the specified level.
		/// </summary>
		/// <param name="level">Number of the level from where to load the data.</param>
		/// <param name="precision">The precision used for encoding.</param>
		/// <returns>Returns an array of vertices.</returns>
		public Centre[] LoadClustersCompressed(int level, double precision)
		{
			// open input file
			string fileName = GetNumberedFileName(level);
			FileStream streamIn = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			BinaryReader fileIn = new BinaryReader(streamIn);

			// load and decompress the data
			Experiments.ArithCoder decoder = new Experiments.ArithCoder(streamIn);
			double[] coords = decoder.DecodeDoubles(precision);

			// determine the number of vertices; 3 coordinates per vertex
			int vertexCount = coords.Length / 3;

			// prepare memory
			Centre[] vertices = new Centre[vertexCount];

			// copy data from the decoded array to the vertices array
			for (int i = 0; i < vertexCount; i++)
			{
				// create vertex
				Vertex v = new Vertex(coords[i], coords[i + vertexCount], coords[i + 2*vertexCount]);
				vertices[i] = new Centre(v);

				if (level > 0)
					// load addresses of cluster members
					//vertices[i].childrenFileStart = fileIn.ReadInt32();
					vertices[i].childrenFileStart = fileIn.ReadInt64();
				else
					vertices[i].childrenFileStart = -1;
			}

#if LOAD_RADII_DUPL
			for (int i = 0; i < vertexCount; i++)
				if (level > 0)
				{
					// load cluster radius
					vertices[i].radius = fileIn.ReadDouble();
					vertices[i].isDuplicate = fileIn.ReadBoolean();
				}
#endif

			// close input file
			fileIn.Close();
			streamIn.Close();

			return vertices;
		}

		/// <summary>
		/// Loads all members of the given cluster.
		/// </summary>
		/// <param name="level">The level from where to load the members (one level lower than the cluster centre).</param>
		/// <param name="startIndex">Index of the first cluster member.</param>
		/// <param name="count">Number of the cluster members.</param>
		/// <returns>Returns an array of cluster vertices.</returns>
		public Centre[] ExpandCluster(int level, long startIndex, int count)
		{
			// open input file
			string fileName = GetNumberedFileName(level);
			FileStream streamIn = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			BinaryReader fileIn = new BinaryReader(streamIn);

			// read the number of coordinates each vertex has
			ushort coordCount = fileIn.ReadUInt16();

			// seek to the appropriate cluster
			streamIn.Seek(startIndex, SeekOrigin.Begin);

			// prepare memory
			Centre[] clusterMembers = new Centre[count];

			// load the cluster members
			if (level > 0)
				for (int i = 0; i < count; i++)
					clusterMembers[i] = Centre.ReadBinary(fileIn, coordCount);
			else
				for (int i = 0; i < count; i++)
					clusterMembers[i] = Centre.ReadBinaryJustCoords(fileIn, coordCount);

			// close input file
			fileIn.Close();
			streamIn.Close();

			return clusterMembers;
		}

		/// <summary>
		/// Loads all members of the given cluster.
		/// </summary>
		/// <param name="level">The level from where to load the members (one level lower than the cluster centre).</param>
		/// <param name="startIndex">Index of the first cluster member.</param>
		/// <param name="precision">The precision used for encoding.</param>
		/// <returns>Returns an array of cluster vertices.</returns>
		public Centre[] ExpandClusterCompressed(int level, long startIndex, double precision)
		{
			// open input file
			string fileName = GetNumberedFileName(level);
			FileStream streamIn = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			BinaryReader fileIn = new BinaryReader(streamIn);

			// seek to the appropriate cluster
			streamIn.Seek(startIndex, SeekOrigin.Begin);

			// load and decompress the data
			Experiments.ArithCoder decoder = new Experiments.ArithCoder(streamIn);
			double[] coords = decoder.DecodeDoubles(precision);

			// determine the number of vertices; 3 coordinates per vertex
			int count = coords.Length / 3;

			// prepare memory
			Centre[] clusterMembers = new Centre[count];

			// copy data from the decoded array to the vertices array
			for (int i = 0; i < count; i++)
			{
				// create vertex
				Vertex v = new Vertex(coords[i], coords[i + count], coords[i + 2*count]);
				clusterMembers[i] = new Centre(v);

				if (level > 0)
					// load addresses of cluster members
					clusterMembers[i].childrenFileStart = fileIn.ReadInt64();
				else
					clusterMembers[i].childrenFileStart = -1;
			}

#if LOAD_RADII_DUPL
			for (int i = 0; i < count; i++)
				if (level > 0)
				{
					// load cluster radius
					clusterMembers[i].radius = fileIn.ReadDouble();
					clusterMembers[i].isDuplicate = fileIn.ReadBoolean();
				}
#endif

			// close input file
			fileIn.Close();
			streamIn.Close();

			return clusterMembers;
		}
	}
}
