/*
 * Hierarchical clusterer
 * 
 * Ondrej Kaas
 * 
 */
using System;
using System.Collections.Generic;
using System.IO;

namespace Zcu.Graphics.Clustering
{
	/// <summary>
	/// The hierarchical clusterer.
	/// </summary>
	public class HierarchicalClusterer
	{
		/// <summary>
		/// The list of ordinary clusterers.
		/// </summary>
		private List<ClusteringLevel> clusteringLevels;

		/// <summary>
		/// Size of a block in one level of the hierarchy.
		/// </summary>
		private int blockSize;

		/// <summary>
		/// Facility cost multiplier.
		/// </summary>
		private double facCostMult;

		/// <summary>
		/// Output file name.
		/// </summary>
		private string outputFileName;

		/// <summary>
		/// Determines whether the input stream has been finished.
		/// If so, no more vertices can be added for clustering.
		/// </summary>
		private bool finished;





		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="blockSize">Size of one block that will be clustered, default is 1000.</param>
		/// <param name="facCostMult">Facility cost multiplier, default is 1. Specifies the cluster size,
		/// higher value produces larger clusters.</param>
		/// <param name="outputFileName">Output file name. Level number will be appended.</param>
		public HierarchicalClusterer(int blockSize, double facCostMult, string outputFileName)
		{
			this.blockSize = blockSize;
			this.facCostMult = facCostMult;
			this.outputFileName = outputFileName;

			// prepare the base of the hierarchy
			finished = false;
			clusteringLevels = new List<ClusteringLevel>();
			string fileName = GetNumberedFileName(0);
			clusteringLevels.Add(new ClusteringLevel(blockSize, facCostMult, fileName));
		}

		/// <summary>
		/// Creates a file name with the given number.
		/// </summary>
		/// <param name="level">The number of level.</param>
		/// <returns>Returns the file name with the given number.</returns>
		private string GetNumberedFileName(int level)
		{
			// no file name specified
			if (outputFileName == null)
				return null;

			string path = Path.GetDirectoryName(outputFileName);
			string newFileName = Path.GetFileNameWithoutExtension(outputFileName) + "_" + level
				+ Path.GetExtension(outputFileName);

			// append the level number to the end of the file name
			return Path.Combine(path, newFileName);
		}

		/// <summary>
		/// Adds vertices for the clustering.
		/// </summary>
		/// <param name="vertices">The vertices to add.</param>
		public void PushVertices(Vertex[] vertices)
		{
			Centre[] centres = new Centre[vertices.Length];

			// convert vertices to the Centre structure
			for (int i = 0; i < vertices.Length; i++)
				centres[i] = new Centre(vertices[i]);

			// push
			PushVertices(centres, 0);
		}

		/// <summary>
		/// Adds more centres for clustering.
		/// </summary>
		/// <param name="centres">The centres to add.</param>
		/// <param name="level">The level where to add.</param>
		private void PushVertices(Centre[] centres, int level)
		{
			if (finished)
				throw new ApplicationException("The input stream has already been finished.");

			// get the appropriate clustering level
			ClusteringLevel cl = clusteringLevels[level];

			// is there enough free space?
			if (centres.Length <= cl.FreeSpace)
				// yes, add all the vertices right away
				cl.PushVertices(centres);
			else
			{
				// no, add the vertices by pieces
				int remainToPush = centres.Length;
				int alreadyPushed = 0;

				int free = cl.FreeSpace;
				// prepare the first piece
				Centre[] pieceToPush = new Centre[free];
				Array.Copy(centres, alreadyPushed, pieceToPush, 0, free);

				// push the first piece
				cl.PushVertices(pieceToPush);
				remainToPush -= free;
				alreadyPushed += free;

				// process the full block
				ProcessLevel(level);
				
				pieceToPush = new Centre[blockSize];
				// push further pieces
				while (remainToPush > blockSize)
				{
					// prepare a single piece
					Array.Copy(centres, alreadyPushed, pieceToPush, 0, blockSize);

					// push the piece
					cl.PushVertices(pieceToPush);
					remainToPush -= blockSize;
					alreadyPushed += blockSize;

					// process the full block
					ProcessLevel(level);
				}

				// copy the last remaining piece
				pieceToPush = new Centre[remainToPush];
				Array.Copy(centres, alreadyPushed, pieceToPush, 0, remainToPush);

				// push the last remaining piece
				cl.PushVertices(pieceToPush);
			}

			// check whether there is still some space for further vertices
			if (cl.FreeSpace == 0)
				// no, the level is exactly full, let's process it
				ProcessLevel(level);
		}

		/// <summary>
		/// Performs clustering at specified level.
		/// </summary>
		/// <param name="level">The level where to cluster.</param>
		private void ProcessLevel(int level)
		{
			// do the clustering
			clusteringLevels[level].DoClustering();

			// save the result
			if (level == 0)
				// save just the vertices at the zero level
				clusteringLevels[level].SaveVertices();
			else
				// save cluster centres including the children information
				clusteringLevels[level].SaveCentres();

			// move the result to the higher level
			MoveCentresHigher(level);

			// prepare the current level for further processing
			clusteringLevels[level].ResetVertices();
		}

		/// <summary>
		/// Moves the resulting cluster centres from the given level to a higher level.
		/// </summary>
		/// <param name="currentLevel">The current level from where to move the cluster centres.</param>
		private void MoveCentresHigher(int currentLevel)
		{
			int nextLevel = currentLevel + 1;

			// create a new level if necessary
			if (clusteringLevels.Count <= nextLevel)
			{
				string fileName = GetNumberedFileName(nextLevel);
				clusteringLevels.Add(new ClusteringLevel(blockSize, facCostMult, fileName));
			}

			// get cluster centres from the lower level
			Centre[] centresToMove = clusteringLevels[currentLevel].GetCentres();

			// move cluster centres to the higher level
			PushVertices(centresToMove, nextLevel);
		}

		/// <summary>
		/// Signals the end of the data stream. Finalizes the processing.
		/// </summary>
		public void FinishVertices()
		{
			// finalize all the levels
			// stop before the last one
			// do not cluster the last level, nor move the results to a higher level
			// clusteringLevels.Count may increase during the loop
			for (int i = 0; i < clusteringLevels.Count - 1; i++)
			{
				// check that the level is not empty
				if (clusteringLevels[i].FreeSpace < blockSize)
					// process the remaining vertices
					ProcessLevel(i);

				// finish the level (close output file)
				clusteringLevels[i].FinishVertices();
			}

			// can increase during the previous for-loop
			int lastLevel = clusteringLevels.Count - 1;

			// do the clustering
			//clusteringLevels[lastLevel].DoClustering();


			//// save the result
			//if (lastLevel == 0)
			//    // save just the vertices
			//    clusteringLevels[lastLevel].SaveVertices();
			//else
			//    // save cluster centres including the children information
			//    clusteringLevels[lastLevel].SaveCentres();

			// save the last level without any ordering
			clusteringLevels[lastLevel].SaveUnordered(lastLevel > 0);

			clusteringLevels[lastLevel].FinishVertices();

			// definitely finished
			finished = true;
		}
	}
}
