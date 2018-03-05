/*
 * Hierarchical clustering
 * 
 * Ondrej Kaas
 * 
 */
#define TEXT_OUTPUT

using System;
using System.IO;

namespace Zcu.Graphics.Clustering
{
	/// <summary>
	/// One level of the hierarchical clustering.
	/// </summary>
	class ClusteringLevel
	{
		/// <summary>
		/// The clusterer to cluster vertices at this level.
		/// </summary>
		private FacilityLocation cls;

		/// <summary>
		/// The facility cost multiplier.
		/// </summary>
		private double facCostMult = 1;

		/// <summary>
		/// Buffer for accumulating vertices before clustering.
		/// </summary>
		private Centre[] vertexBuffer;

		/// <summary>
		/// Size of the block to cluster.
		/// </summary>
		private int blockSize;

		/// <summary>
		/// Position in the vertex buffer while filling it.
		/// Marks the first free position.
		/// </summary>
		private int bufferPosition;

		/// <summary>
		/// Output file name.
		/// </summary>
		private string outputFileName;

		/// <summary>
		/// Output file.
		/// </summary>
#if TEXT_OUTPUT
		private StreamWriter fileOut = null;
#else
		private BinaryWriter fileOut = null;

		/// <summary>
		/// Tells whether the file header needs to be written.
		/// </summary>
		private bool needsToWriteHeader;
#endif

		/// <summary>
		/// The position in the output file.
		/// </summary>
		private long filePosition;

		/// <summary>
		/// Determines whether the input stream has been finished.
		/// If so, no more vertices can be added for clustering.
		/// </summary>
		private bool finished;



		private static Random rnd;

		static ClusteringLevel()
		{
			rnd = new Random(1234);
		}





		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="blockSize">The size of the block to cluster.</param>
		/// <param name="facCostMult">The facility cost multiplier. Higher value produces larger clusters;
		/// default is 1.</param>
		/// <param name="outputFileName">Output file name. Can be null.</param>
		public ClusteringLevel(int blockSize, double facCostMult, string outputFileName)
		{
			this.blockSize = blockSize;
			this.facCostMult = facCostMult;
			this.outputFileName = outputFileName;

			// prepare array for vertices to cluster
			vertexBuffer = new Centre[blockSize];
			bufferPosition = 0;

			// prepare the clusterer
			cls = new FacilityLocation();

			// prepare file for output
			filePosition = 0;
			finished = false;

			if (outputFileName != null)
			{
#if TEXT_OUTPUT
				fileOut = new StreamWriter(outputFileName);
#else
				fileOut = new BinaryWriter(
					new FileStream(outputFileName, FileMode.Create, FileAccess.Write, FileShare.None));

				// must wait for some vertices to determine the number of coordinates
				needsToWriteHeader = true;
#endif
			}
			else
				fileOut = null;
		}

		/// <summary>
		/// Adds cluster centres from the lower level.
		/// </summary>
		/// <param name="vertices">The vertices to add.</param>
		public void PushVertices(Centre[] centres)
		{
			if (finished)
				throw new ApplicationException("The input stream has already been finished.");

#if !TEXT_OUTPUT
			if (needsToWriteHeader)
			{
				System.Diagnostics.Debug.Assert(centres[0].vertex.Dimension < UInt16.MaxValue);

				// write the number of coordinates each vertex has
				fileOut.Write((UInt16)centres[0].vertex.Dimension);
				filePosition = sizeof(UInt16);

				// header written
				needsToWriteHeader = false;
			}
#endif

			int length = centres.Length;

			// check whether the new vertices fit the block size
			if (bufferPosition + length > blockSize)
				throw new ApplicationException("That would be more vertices than fit in a single block.");

			// copy the new vertices to the buffer
			Array.Copy(centres, 0, vertexBuffer, bufferPosition, length);
			bufferPosition += length;
		}

		/// <summary>
		/// Gets the number of vertices that can be further added to this block.
		/// </summary>
		public int FreeSpace
		{
			get { return blockSize - bufferPosition; }
		}

		/// <summary>
		/// Gets the cluster centres.
		/// </summary>
		/// <returns>Returns the cluster centres.</returns>
		public Centre[] GetCentres()
		{
			// get cluster centre indices
			int[] centreIndices = cls.GetAllFacilities();

			// prepare array for output
			Centre[] result = new Centre[centreIndices.Length];

			// copy centres
			for (int i = 0; i < centreIndices.Length; i++)
			{
				// get the centre index
				int centreIndex = centreIndices[i];

				// copy the centre
				result[i] = vertexBuffer[centreIndex];

				// get member indices
				int[] memberIndices = cls.GetClusterVertices(centreIndices[i]);

				// compute weight of the centre
				double weight = 0;
				for (int j = 0; j < memberIndices.Length; j++)
				{
					weight += vertexBuffer[memberIndices[j]].vertex.VertexWeight;
				}
				result[i].vertex.VertexWeight = weight;
			}

			return result;
		}

		/// <summary>
		/// Discards all the vertices and prepares for clustering new ones.
		/// </summary>
		public void ResetVertices()
		{
			bufferPosition = 0;
		}

		/// <summary>
		/// Mark the data stream of vertices as finished.
		/// </summary>
		public void FinishVertices()
		{
			if (fileOut != null)
				fileOut.Close();

			finished = true;
		}

		/// <summary>
		/// Destructor. Ensures that the output file will be closed.
		/// </summary>
		~ClusteringLevel()
		{
			if (fileOut != null)
				fileOut.Close();
		}

		/// <summary>
		/// Performs the clustering.
		/// </summary>
		public void DoClustering()
		{
			// first normalize the weights
			NormalizeWeights();

			// copy vertices
			Vertex[] vertices = new Vertex[bufferPosition];
			for (int i = 0; i < bufferPosition; i++)
			{
				vertices[i] = vertexBuffer[i].vertex;
			}

			cls.FacilityCostMultiplier = facCostMult;
			//int n = bufferPosition;
			//cls.Iterations = n * (int)Math.Ceiling(Math.Log10(n));
			cls.ComputeClustering(vertices);
		}

		/// <summary>
		/// Performs weight normalization before the clustering.
		/// </summary>
		private void NormalizeWeights()
		{
			double sum = 0;
			// compute the sum of all weights
			for (int i = 0; i < bufferPosition; i++)
				if (vertexBuffer[i].vertex.VertexWeight < double.PositiveInfinity)
					sum += vertexBuffer[i].vertex.VertexWeight;

			// compute 1 / average weight
			double oneOverAverage = (double)bufferPosition / sum;

			// normalize the weights
			for (int i = 0; i < bufferPosition; i++)
				vertexBuffer[i].vertex.VertexWeight *= oneOverAverage;
		}

		/// <summary>
		/// Saves the zero level vertices to a file.
		/// </summary>
		public void SaveVertices()
		{
			// should we save?
			if (fileOut == null)
				return;

			// get all cluster centres
			int[] centreIndices = cls.GetAllFacilities();

			// save all clusters
			for (int i = 0; i < centreIndices.Length; i++)
			{
				// get all cluster members
				int centreIndex = centreIndices[i];
				int[] memberIndices = cls.GetClusterVertices(centreIndex);

				// store children information
				vertexBuffer[centreIndex].childrenFileStart = filePosition;
				vertexBuffer[centreIndex].childrenCount = memberIndices.Length;

				// save all members
				for (int j = 0; j < memberIndices.Length; j++)
				{
					// save member
					int memberIndex = memberIndices[j];
#if TEXT_OUTPUT
					// save just the vertex
					fileOut.WriteLine(vertexBuffer[memberIndex].vertex.ToString());
					filePosition++;
#else
					filePosition += vertexBuffer[memberIndex].vertex.SaveBinary(fileOut);
#endif
				}
			}
		}

		/// <summary>
		/// Saves cluster centres to a file. I.e., saves the vertices along with the children information.
		/// </summary>
		public void SaveCentres()
		{
			// should we save?
			if (fileOut == null)
				return;

			// get all cluster centres
			int[] centreIndices = cls.GetAllFacilities();

			// save all clusters
			for (int i = 0; i < centreIndices.Length; i++)
			{
				// get all cluster members
				int centreIndex = centreIndices[i];
				int[] memberIndices = cls.GetClusterVertices(centreIndex);

				int bytesWritten = 0;

				// save all members
				for (int j = 0; j < memberIndices.Length; j++)
				{
					// save member
					int memberIndex = memberIndices[j];
#if TEXT_OUTPUT
					// save centre coordinates along with children information
					fileOut.WriteLine(vertexBuffer[memberIndex].ToString());
#else
					bytesWritten += vertexBuffer[memberIndex].SaveBinary(fileOut);
#endif
				}

				// !
				// first save the centre with "old" (from the lower level) children information
				// then overwrite the children information with the "new" one (from the current level)

				// store children information
				vertexBuffer[centreIndex].childrenFileStart = filePosition;
				vertexBuffer[centreIndex].childrenCount = memberIndices.Length;

				// move the file position
#if TEXT_OUTPUT
				filePosition += memberIndices.Length;
#else
				filePosition += bytesWritten;
#endif
			}
		}

		/// <summary>
		/// Saves unordered (unclustered) vertices to a file.
		/// </summary>
		/// <param name="areCentres">Determines whether the vertices are cluster centres from a lower level.
		/// Most probably they are, but in a special case (very very short input) they can be the zero level
		/// vertices.</param>
		public void SaveUnordered(bool areCentres)
		{
			// should we save?
			if (fileOut == null)
				return;

			if (areCentres)
				// save centres
				for (int i = 0; i < bufferPosition; i++)
				{
#if TEXT_OUTPUT
					fileOut.WriteLine(vertexBuffer[i].ToString());
#else
					vertexBuffer[i].SaveBinary(fileOut);
#endif
				}
			else
				// save vertices
				for (int i = 0; i < bufferPosition; i++)
				{
#if TEXT_OUTPUT
					fileOut.WriteLine(vertexBuffer[i].vertex.ToString());
#else
					vertexBuffer[i].vertex.SaveBinary(fileOut);
#endif
				}
		}
	}
}
