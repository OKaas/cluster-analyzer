/*
 * Cluster viewer
 * 
 * Ondrej Kaas
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

using Zcu.Graphics.Clustering;
using ClusterViewer.Iterators.impl;
using Clustering.Helpers;
using System.Threading.Tasks;
using ZedGraph;

namespace ClusterViewer
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// Cluster loader for loading clusters from file.
        /// </summary>
        private ClusterLoader clusterLoader;

        public string inputFile;

        /// <summary>
        /// Vertices to draw.
        /// </summary>
        private Vertex[] vertices;
        private Vertex[] verticesToCluster;

        private List<Facility> facilities;

        private List<List<Vertex>> convexHull = new List<List<Vertex>>();

        private ClusterGroup[] clusterGroups;

        private static readonly List<Color> colors = new List<Color>() {
            Color.Black, Color.Red, Color.Orange, Color.Green, Color.Blue,
            Color.Violet, Color.Yellow, Color.Cyan, Color.Pink, Color.Brown, Color.Gray
        };
        private List<Brush> brushes;
        private Array symbolTypes = Enum.GetValues(typeof(SymbolType));

        /// <summary>
        /// Data bounding box.
        /// </summary>
        private BoundingBox bBox;
        private BoundingBox bBoxToCluster;

        /// <summary>
        /// Show cluster site
        /// </summary>
        public bool showPoints = false;

        public bool showClusterGroups = false;

        private float SCALE_DIFFERENCE = 10f;

        /// <summary>
        /// Constructor. Initialises window components.
        /// </summary>
        public MainForm()
        {
            // set up an empty bounding box
            bBox.Initialize(3);
            bBoxToCluster.Initialize(3);

            InitializeComponent();

            InitializeBrushes();
        }

        private void ShowCalculateClusterConvexHull()
        {
            // showClusterGroups = true;
            convexHull.Clear();

            // it's time to calculate convex hull of clusters
            foreach (Facility fac in facilities)
            {
                List<Vertex> points = new List<Vertex>();
                foreach (int i in fac.VertexIndices)
                {
                    points.Add(vertices[i]);
                }

                if (points.Count > 3)
                {
                    convexHull.Add(ConvexHull.MakeConvexHull(points));
                }
            }

            //Graph.Draw();
        }

        private void CalculateFacilitiesPosition()
        {
            int dim = vertices[0].Coords.Length;

            foreach (Facility fac in facilities)
            {
                double[] coord = new double[dim];
                
                foreach(int client in fac.VertexIndices)
                {
                    for (int i = 0; i < dim; ++i)
                    {
                        coord[i] += vertices[client].Coords[i];
                    }
                }

                for (int i = 0; i < dim; ++i)
                {
                    coord[i] /= (double)fac.VertexIndices.Count;
                }
            }
        }

        private void ScaleVertices()
        {
            //for ( int i = 0; i < vertices[0].Dimension; ++i )
            //{
            //    double interval = bBox.maxCorner[i] - bBox.minCorner[i];

            //    for (int j = 0; j < vertices.Length; ++j)
            //    {
            //        vertices[j][i] = (vertices[j][i] - bBox.minCorner[i]) / interval;
            //    }
            //}
        }

        /// <summary>
        /// Redraws the canvas after resizing.
        /// </summary>
        private void panelCanvas_Resize(object sender, EventArgs e)
        {
            
            
        }
        
        /// <summary>
        /// Loads clusters from a file.
        /// </summary>
        /// <param name="fileName">Input file name.</param>
        //private void LoadClusters(string fileName)
        //{
        //	// initialise the loader
        //	clusterLoader = new ClusterLoader(fileName);

        //	// load the top level
        //	Centre[] loadedTopLevel = clusterLoader.LoadClusters(clusterLoader.HighestLevel);

        //	// reset bounding box
        //	int dim = loadedTopLevel[0].vertex.Dimension;
        //	bBox.Initialize(dim);

        //	// prepare a list for cluster members
        //	List<Vertex> vertexList = new List<Vertex>();

        //	int globalIndex = 0;
        //	// create all clusters (load the members)
        //	for (int clusterIndex = 0; clusterIndex < loadedTopLevel.Length; clusterIndex++)
        //	{
        //		// locate the cluster members in the file
        //		long start = loadedTopLevel[clusterIndex].childrenFileStart;
        //		int count = loadedTopLevel[clusterIndex].childrenCount;
        //		// load the cluster members
        //		Centre[] clusterMembers = clusterLoader.ExpandCluster(clusterLoader.HighestLevel - 1, start, count);

        //		// find the vertex which is the facility
        //		int i = 0;
        //		while ( ! VerticesEqual(loadedTopLevel[clusterIndex].vertex, clusterMembers[i].vertex))
        //		{
        //			i++;
        //		}

        //		// get the facility position
        //		Vertex fPos = clusterMembers[i].vertex;
        //		// create the facility
        //		Facility f = new Facility(globalIndex + i);

        //		// assign cluster members to the facility
        //		for (int vertexIndex = 0; vertexIndex < clusterMembers.Length; vertexIndex++)
        //		{
        //			// get only the Vertex from the Centre
        //			Vertex v = clusterMembers[vertexIndex].vertex;

        //			// update bounding box
        //			bBox.AddVertex(v);

        //			// set IsFacility flag if appropriate
        //			if (vertexIndex == i)
        //				v.IsFacility = true;
        //			else
        //				v.IsFacility = false;

        //			// compute distance
        //			double dist = fPos.WeightedDistance(v);
        //			// assign facility to the vertex
        //			v.AssignToFacility(f, dist);
        //			// assign vertex to the facility
        //			f.AddVertex(vertexIndex + globalIndex, dist);

        //			// store the vertex to the list
        //			vertexList.Add(v);
        //		}

        //		// advance the global index
        //		globalIndex += clusterMembers.Length;
        //	}

        //	// copy the list to an array
        //	vertices = vertexList.ToArray();

        //	// update the drawing canvas
        //	Canvas.Invalidate();
        //}

        private void InitializeBrushes()
        {
            brushes = new List<Brush>();

            foreach (Color cls in colors)
            {
                brushes.Add(new SolidBrush(cls));
            }

            //KnownColor[] values = (KnownColor[])Enum.GetValues(typeof(KnownColor));
            //foreach (KnownColor kc in values)
            //{
            //    colors.Add(Color.FromKnownColor(kc));
            //    brushes.Add( new SolidBrush(Color.FromKnownColor(kc))  );
            //}
        }

        #region UI_Handling

        public void CreateScatterplot(ZedGraphControl zgc)
        {
            GraphPane myPane = zgc.GraphPane;
            myPane.CurveList.Clear();

            // Set the titles
            myPane.Title.IsVisible = false;
            myPane.XAxis.Title.Text = "x";
            myPane.YAxis.Title.Text = "y";

            myPane.GraphObjList.Clear();

            int colorIndex = -1;
            int colorSymbol = -1;

            //if (convexHull != null && convexHull.Count > 0)
            //{
            //    foreach (List<Vertex> vrt in convexHull)
            //    {
            //        ++colorIndex;
            //        if (colorIndex >= brushes.Count)
            //        {
            //            colorIndex = 0;
            //        }

            //        ++colorSymbol;
            //        if (colorSymbol >= symbolTypes.Length)
            //        {
            //            colorSymbol = 0;
            //        }

            //        PointD[] pol = new PointD[vrt.Count];
            //        PolyObj obj = new PolyObj();
            //        obj.Fill = new Fill(colors[colorIndex]);
            //        obj.Fill.Color = Color.Transparent;
            //        // obj.ZOrder = ZOrder.E_BehindCurves;

            //        int id = -1;
            //        foreach (Vertex cl in vrt)
            //        {
            //            pol[++id] = new PointD(cl[0], cl[1]);
            //        }

            //        obj.Points = pol;

            //        myPane.GraphObjList.Add(obj);
            //    }
            //}

            if ( facilities != null && facilities.Count > 0 )
            {
                
                for (int f = 0; f < facilities.Count; ++f)
                {
                    Facility fac = facilities[f];
                    List<int> clients = fac.VertexIndices;
                    PointPairList list1 = new PointPairList();


                    foreach (int cl in clients)
                    {
                        list1.Add(vertices[cl][0], vertices[cl][1]);
                    }

                    ++colorIndex;
                    if (colorIndex >= brushes.Count)
                    {
                        colorIndex = 0;
                    }

                    ++colorSymbol;
                    if (colorSymbol >= symbolTypes.Length)
                    {
                        colorSymbol = 0;
                    }

                    Fill fillCurrent = new Fill(colors[colorIndex]);
                    
                    // Add the curve
                    LineItem myCurve = myPane.AddCurve("F" + f, list1, colors[colorIndex], SymbolType.Circle);
                    myCurve.Line.IsVisible = false;
                    myCurve.Line.Fill = fillCurrent;
                    
                    myCurve.Symbol.Size = 1;
                    myCurve.Symbol.Border.IsVisible = true;
                    myCurve.Symbol.Fill = new Fill(colors[colorIndex]);
                    myCurve.Symbol.Fill.IsVisible = true;
                }
            } else
            {
                for (int i = 0; i < vertices.Length; ++i)
                {
                    PointPairList list1 = new PointPairList();
                    list1.Add(vertices[i][0], vertices[i][1]);

                    ++colorIndex;
                    if (colorIndex >= brushes.Count)
                    {
                        colorIndex = 0;
                    }

                    ++colorSymbol;
                    if (colorSymbol >= symbolTypes.Length)
                    {
                        colorSymbol = 0;
                    }

                    // Add the curve
                    LineItem myCurve = myPane.AddCurve("V" + i, list1, Color.Green, SymbolType.Circle);
                    myCurve.Symbol.Size = 0.7f;
                    myCurve.Line.IsVisible = false;
                    myCurve.Symbol.Border.IsVisible = true;
                    // myCurve.Symbol.Fill = new Fill(Color.Blue);
                    //myCurve.Symbol.Fill.IsVisible = false;
                }
            }

            

            //// Add the curve
            //LineItem myCurve = myPane.AddCurve("G1", list1, Color.Blue, SymbolType.Diamond);
            //myCurve.Line.IsVisible = false;
            //myCurve.Symbol.Border.IsVisible = false;
            //myCurve.Symbol.Fill = new Fill(Color.Blue);

            //myCurve = myPane.AddCurve("G2", list2, Color.Green, SymbolType.Diamond);
            //myCurve.Line.IsVisible = false;
            //myCurve.Symbol.Border.IsVisible = false;
            //myCurve.Symbol.Fill = new Fill(Color.Green);


            // Fill the background of the chart rect and pane
            //myPane.Chart.Fill = new Fill(Color.White, Color.LightGoldenrodYellow, 45.0f);
            //myPane.Fill = new Fill(Color.White, Color.SlateGray, 45.0f);
            myPane.Fill = new Fill(Color.WhiteSmoke);

            zgc.AxisChange();
            zgc.Invalidate();
        }

        /// <summary>
        /// Draws the clusters.
        /// </summary>
        //private void panelCanvas_Paint(object sender, PaintEventArgs e)
        //{
        //    // constants defining drawing properties
        //    Pen penVertex = Pens.Blue;          // vertex color
        //    Pen penDifference = Pens.Black;
        //    Pen penLine = Pens.Blue;            // line color
        //    Brush brushFacility = Brushes.Red;  // facility color
        //    const float vertexRadius = 2.0f;    // vertex radius
        //    const float facilityRadius = 3.5f;  // facility radius

        //    int colorIndex = -1;
        //    KnownColor[] values = (KnownColor[])Enum.GetValues(typeof(KnownColor));

        //    bool drawXYplane = true;
        //    bool drawSpheres = false;

        //    // prepare a canvas to draw 
        //    System.Drawing.Graphics gr = e.Graphics;
        //    gr.Clear(Color.White);
        //    // get panel (canvas) dimensions
        //    int width = e.ClipRectangle.Width;
        //    int height = e.ClipRectangle.Height;

        //    // compute scaling
        //    float scaleX = (float)width / (float)bBox.Width;
        //    float scaleY = (float)height / (float)bBox.Height;
        //    //float scaleZ = (float)height / (float)bBox.Depth;
        //    float scale = Math.Min(scaleX, scaleY) * 0.9f;
        //    //float scale = scaleX < scaleY ? scaleX : scaleY;
        //    //float scale = scaleX < scaleZ ? scaleX : scaleZ;
        //    float minX = (float)bBox.MinX - (float)bBox.Width * 0.05f;
        //    float minY = (float)bBox.MinY - (float)bBox.Height * 0.05f;
        //    //float minZ = (float)bBox.MinZ;
        //    float minYZ = minY;

        //    // font for writing vertex numbers etc.
        //    Font font = new Font(FontFamily.GenericSansSerif, 10);

        //    // check we have something to draw
        //    if (vertices == null)
        //        return;

        //    if ( showClusterGroups )
        //    {
        //        Brush fill = Brushes.LawnGreen;
        //        Pen pen = Pens.Black;

        //        //foreach (List<Vertex> convex in convexHull)
        //        //{
        //        //    PointF[] poly = new PointF[convex.Count];

        //        //    for (int i = 0; i < convex.Count; ++i)
        //        //    {
        //        //        Vertex v = convex[i];

        //        //        float vx = ((float)v.X - minX) * scale;
        //        //        float vy = height - ((float)(drawXYplane ? v.Y : v.Z) - minYZ) * scale;

        //        //        poly[i] = new PointF(vx, vy);
        //        //    }

        //        //    // gr.FillPolygon(fill, poly);
        //        //    gr.DrawPolygon(pen, poly);
        //        //}

        //        foreach (Facility f in facilities)
        //        {
        //            float fx = ((float)f.Coords[0] - minX) * scale;
        //            float fy = height - ((float)(drawXYplane ? f.Coords[1] : f.Coords[2]) - minYZ) * scale;

        //            // draw lines to facility vertices
        //            foreach (int vertexIndex in f.VertexIndices)
        //            {
        //                // get the vertex position
        //                Vertex vertPos = vertices[vertexIndex];
        //                float vx = ((float)vertPos.X - minX) * scale;
        //                float vy = height - ((float)(drawXYplane ? vertPos.Y : vertPos.Z) - minYZ) * scale;

        //                //float dx = ((float)vertPos[2]) * SCALE_DIFFERENCE;
        //                // float dy = ((float)vertPos[3]) * SCALE_DIFFERENCE;

        //                // check radius
        //                //if (vertPos.WeightedDistToFac > radius)
        //                //    radius = (float)vertPos.WeightedDistToFac;

        //                // draw line to the vertex
        //                gr.DrawLine(penLine, fx, fy, vx, vy);
        //                // draw the vertex
        //                gr.DrawEllipse(penVertex, vx - vertexRadius, vy - vertexRadius, 2 * vertexRadius, 2 * vertexRadius);

        //                // draw difference
        //                //gr.DrawLine(penDifference, vx - vertexRadius, vy - vertexRadius, vx - vertexRadius + dx, vy - vertexRadius + dy);

        //                // draw facility index
        //                //gr.DrawString(i.ToString(), font, Brushes.Black, vx, vy);
        //                // draw vertex index
        //                //gr.DrawString(vertexIndex.ToString(), font, Brushes.Black, vx, vy);
        //            }

        //            ++colorIndex;
        //            if (colorIndex >= brushes.Count)
        //            {
        //                colorIndex = 0;
        //            }

        //            gr.FillEllipse(brushes[colorIndex], fx - vertexRadius, fy - vertexRadius, 2 * vertexRadius, 2 * vertexRadius);
        //        }

        //    } else {

        //        // draw facilities with connected vertices
        //        for (int i = 0; i < vertices.Length; i++)
        //        {
        //            // draw bounding box information
        //            //string boxInfo = string.Format("{0:0.##}-{1:0.##}", facPos.bbox.MinX, facPos.bbox.MaxX);
        //            //gr.DrawString(boxInfo, font, Brushes.Black, fx, fy);

        //            Vertex vertex = vertices[i];

        //            if (showPoints)
        //            {
        //                // get the vertex position
        //                float vx = ((float)vertex.X - minX) * scale;
        //                float vy = height - ((float)(drawXYplane ? vertex.Y : vertex.Z) - minYZ) * scale;

        //                //float dx = ((float)vertex[2]) * SCALE_DIFFERENCE;
        //                //float dy = ((float)vertex[3]) * SCALE_DIFFERENCE;

        //                // draw the vertex
        //                gr.DrawEllipse(penVertex, vx - vertexRadius, vy - vertexRadius, 2 * vertexRadius, 2 * vertexRadius);

        //                // draw difference
        //                // gr.DrawLine(penDifference, vx - vertexRadius, vy - vertexRadius, vx - vertexRadius + dx, vy - vertexRadius + dy);
        //            }
        //            else if (vertex.IsFacility)
        //            {
        //                // get the facility position
        //                float fx = ((float)vertex.X - minX) * scale;
        //                float fy = height - ((float)(drawXYplane ? vertex.Y : vertex.Z) - minYZ) * scale;

        //                float radius = 0;

        //                // draw lines to facility vertices
        //                foreach (int vertexIndex in vertices[i].Facility.VertexIndices)
        //                {
        //                    // get the vertex position
        //                    Vertex vertPos = vertices[vertexIndex];
        //                    float vx = ((float)vertPos.X - minX) * scale;
        //                    float vy = height - ((float)(drawXYplane ? vertPos.Y : vertPos.Z) - minYZ) * scale;

        //                    //float dx = ((float)vertPos[2]) * SCALE_DIFFERENCE;
        //                    // float dy = ((float)vertPos[3]) * SCALE_DIFFERENCE;

        //                    // check radius
        //                    //if (vertPos.WeightedDistToFac > radius)
        //                    //    radius = (float)vertPos.WeightedDistToFac;

        //                    // draw line to the vertex
        //                    gr.DrawLine(penLine, fx, fy, vx, vy);
        //                    // draw the vertex
        //                    //gr.DrawEllipse(penVertex, vx - vertexRadius, vy - vertexRadius, 2 * vertexRadius, 2 * vertexRadius);

        //                    // draw difference
        //                    //gr.DrawLine(penDifference, vx - vertexRadius, vy - vertexRadius, vx - vertexRadius + dx, vy - vertexRadius + dy);

        //                    // draw facility index
        //                    //gr.DrawString(i.ToString(), font, Brushes.Black, vx, vy);
        //                    // draw vertex index
        //                    //gr.DrawString(vertexIndex.ToString(), font, Brushes.Black, vx, vy);
        //                }

        //                // draw the facility
        //                gr.FillEllipse(brushFacility, fx - facilityRadius, fy - facilityRadius,
        //                    2 * facilityRadius, 2 * facilityRadius);
        //                //gr.DrawString(facPos.facility.vertexIndex.ToString(), font, Brushes.Black, fx, fy);
        //                //gr.DrawString(radius.ToString(), font, Brushes.Magenta, fx, fy);

        //                if (drawSpheres)
        //                {
        //                    // draw the bounding sphere
        //                    radius *= scale;
        //                    gr.DrawEllipse(Pens.Black, fx - radius, fy - radius, 2 * radius, 2 * radius);
        //                }
        //            }
        //        }
        //    }

        //    font.Dispose();
        //}

        private void Main_Load_Click(object sender, EventArgs e)
        {
            if (Dialog_OpenFile.ShowDialog() == DialogResult.OK)
            {
                ClusterEngine.LoadData(Dialog_OpenFile.FileName);
                // bBox.AddVertices(vertices);

                // enable UI components
                showPoints = true;
                Menu_Cluster.Enabled = true;
                Menu_ClusterSolver.Enabled = true;
            }
        }

        private void Main_Cluster_Click(object sender, EventArgs e)
        {
            //int numberOfClusters = 9;
            //Vertex[] extendedVertices = new Vertex[vertices.Length + numberOfClusters];
            //for (int i = 0; i < vertices.Length; i++)
            //{
            //    extendedVertices[i] = vertices[i];
            //}
            //vertices = extendedVertices;
            // IClustering fac = new MShift();
            // IClustering fac = new KMeans();
            // IClustering fac = new CMeans();
            IClustering fac = new MShift();

            // add some properties to particular clustering algorithms
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("boundingBox", bBoxToCluster);
            properties.Add("PARAM_1", double.Parse(TextBox_Param1.Text));
            properties.Add("PARAM_2", double.Parse(TextBox_Param2.Text));
            // properties.Add("weights", new double[] { 1, 1, 1, 1, 1, 1, 1, 1 });

            fac.SetProperties(properties);

            // cluster again!
            //if ( facilities != null && facilities.Count > 0 )
            //{
            //    Vertex[] ret = new Vertex[facilities.Count];

            //    int index = -1;
            //    foreach (Facility f in facilities)
            //    {
            //        ret[++index] = new Vertex(vertices[f.VertexIndex].Coords);
            //    }

            //    Array.Clear(vertices, 0, vertices.Length);
            //    vertices = ret;
            //}

            fac.ComputeClustering(verticesToCluster);
            
            showPoints = false;

            // save clustering result
            facilities = fac.GetFacilities();

            ShowCalculateClusterConvexHull();

            CalculateFacilitiesPosition();
            
            Graph.Draw();


            if (Dialog_SaveCluster.ShowDialog() == DialogResult.OK)
            {
                // ClusterSolution.SaveClusteringSolution(fac, inputFile, Dialog_SaveCluster.FileName, facilities);
                ClusterSolution.SaveClusteringToMeshLab(fac, Dialog_SaveCluster.FileName, facilities, verticesToCluster);
            }

            // show debug window
            // new Statistics(bBox, fac.GetFacilities(), vertices);
        }
        
        private void Menu_ClusterSolver_Click(object sender, EventArgs e)
        {
            // choise correct ClusterIterator
            //AClusterIterator clusterIterator = new KMeansIterator(vertices, bBox);
            //AClusterIterator clusterIterator = new CMeansIterator(vertices, bBox);
            AClusterIterator clusterIterator = new FacilityLocationIterator(vertices, bBox);

            // find best cluster solution
            clusterIterator.Solve();
        }

        private void Menu_ClusterBatch_Click(object sender, EventArgs e)
        {
            // string inputFolder = @"D:\\__gDrive\\__PHD\\Input\\Generated\\_Vtx\\Test-Vtx";
            string outputFolder = @"D:\\__gDrive\\__PHD\\Input\\Generated\\_Vtx\\Output-CMeans";

            string[] inputFolder = new string[] {
                "D:\\__gDrive\\__PHD\\Input\\Generated\\_Vtx\\Test-Vtx\\arc50000_1r.vtx",
                "D:\\__gDrive\\__PHD\\Input\\Generated\\_Vtx\\Test-Vtx\\clus50000_4r.vtx",
                "D:\\__gDrive\\__PHD\\Input\\Generated\\_Vtx\\Test-Vtx\\gaus50000_4.vtx",
                "D:\\__gDrive\\__PHD\\Input\\Generated\\_Vtx\\Test-Vtx\\grid50000_0.vtx",
                "D:\\__gDrive\\__PHD\\Input\\Generated\\_Vtx\\Test-Vtx\\grid50000_4.vtx",
                "D:\\__gDrive\\__PHD\\Input\\Generated\\_Vtx\\Test-Vtx\\Unif50000_0.vtx",
                "D:\\__gDrive\\__PHD\\Input\\Generated\\_Vtx\\Test-Vtx\\Unif50000_4.vtx",
                "D:\\__gDrive\\__PHD\\Input\\Generated\\_Vtx\\Test-Vtx\\arc50000_0.vtx"

            };

            //if ( Dialog_InputFolder.ShowDialog() == DialogResult.OK && 
            //     Dialog_OutputFolder.ShowDialog() == DialogResult.OK )
            //{
                IClustering algorithm = new CMeans();


                foreach (string f in inputFolder)
                { 

                //foreach ( string f in Directory.GetFiles(inputFolder) )
                //{
                    string outputFile = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(f));
                    

                    Vertex[] vertices = Loader.LoadVertices(f);
                    int numberOfClusters = vertices.Length / 100;


                    BoundingBox boundingBox = new BoundingBox();
                    boundingBox.Initialize(vertices[0].Dimension);
                    boundingBox.AddVertices(vertices);
                
                    Dictionary<string, object> properties = new Dictionary<string, object>();
                    properties.Add("boundingBox", boundingBox);
                    properties.Add("numberOfClusters", numberOfClusters);
                    properties.Add("treshold", 0.5);

                    Array.Resize<Vertex>(ref vertices, vertices.Length + numberOfClusters);

                    algorithm.SetProperties(properties);
                    algorithm.ComputeClustering(vertices);

                    List<Facility> fac = algorithm.GetFacilities();
                    ClusterSolution.SaveClusteringSolution(algorithm, f, outputFile, fac);
                }
            //}

            MessageBox.Show("Batch clustering is done!", "Clustering", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Menu_ClusterLoad_Click(object sender, EventArgs e)
        {
            if (Dialog_OpenClusterResult.ShowDialog() == DialogResult.OK)
            {
                string inputFileVertices;
                facilities = ClusterSolution.LoadClusteringSolution(Dialog_OpenClusterResult.FileName, out inputFileVertices);

                DataSet vert = Loader.LoadVtx(inputFileVertices, int.MaxValue);

                vertices = vert.points;

                if (vertices == null) { return; }
                foreach (Facility fac in facilities)
                {
                    // set reference to facility
                    foreach(int i in fac.VertexIndices)
                    {
                        vertices[i].Facility = fac;
                    }

                    vertices[fac.VertexIndex].IsFacility = true;
                }
                
                bBox.Initialize(vertices[0].Dimension);
                bBox.AddVertices(vertices);

                ScaleVertices();
                ShowCalculateClusterConvexHull();
                CalculateFacilitiesPosition();

                Menu_Cluster.Enabled = true;
                showPoints = false;
                Graph.Draw();
            }
        }

        #endregion

        #region Collaborators
        #endregion

        private void Menu_Grouping_Click(object sender, EventArgs e)
        {
            showClusterGroups = true;

            double param1 = (TextBox_Param1.Text.Length > 0) ?
                                    double.Parse(TextBox_Param1.Text) : 0;
            double param2 = (TextBox_Param2.Text.Length > 0) ?
                                    double.Parse(TextBox_Param2.Text) : 0;

            // Array.Copy(verticesOriginal, vertices, verticesOriginal.Length);
            // facilities = new List<Facility>(facilitiesOriginal);

            //bool[] valid = new bool[facilities.Count];
            //for (int i = 0; i < valid.Length; ++i) { valid[i] = true; }

            //SortedList<double, Tuple<int, int>> facAgregation = new SortedList<double, Tuple<int, int>>();

            //// 1/2 agregation
            //while (valid.Length - 500 < facilities.Count )
            //{
            //    Console.WriteLine("Facilities: {0}", facilities.Count);

            //    int bestI = -1;
            //    int bestJ = -1;
            //    double bestMatch = double.PositiveInfinity;

            //    Parallel.ForEach(facilities, (facA, state, i) =>
            //    {
            //        double best = double.PositiveInfinity;
            //        int index = -1;

            //        for (int j = 0; j < facilities.Count; ++j)
            //        {
            //            // dont judge me, it's PoC!
            //            if (i != j && valid[j])
            //            {
            //                Facility facB = facilities[j];

            //                // calculate distance
            //                double dist = vertices[facA.VertexIndex].WeightedDistance(vertices[facB.VertexIndex]);

            //                if (best > dist)
            //                {
            //                    best = dist;
            //                    index = j;
            //                }
            //            }
            //        }

            //        if ( bestMatch > best )
            //        {
            //            bestMatch = best;
            //            bestI = (int)i;
            //            bestJ = index;
            //        }
            //    });

            //    //for (int i = 0; i < facilities.Count; ++i) 
            //    //{
            //    //    Facility facA = facilities[i];

            //    //    double best = double.PositiveInfinity;
            //    //    int index = -1;

            //    //    for (int j = 0; j < facilities.Count; ++j)
            //    //    {
            //    //        // dont judge me, it's PoC!
            //    //        if (i != j && valid[j])
            //    //        {
            //    //            Facility facB = facilities[j];

            //    //            // calculate distance
            //    //            double dist = vertices[facA.VertexIndex].WeightedDistance(vertices[facB.VertexIndex]);

            //    //            if (best > dist)
            //    //            {
            //    //                best = dist;
            //    //                index = j;
            //    //            }
            //    //        }
            //    //    }

            //    //    if ( bestMatch > best )
            //    //    {
            //    //        bestMatch = best;
            //    //        bestI = i;
            //    //        bestJ = index;
            //    //    }
            //    //}

            //    if (0 <= bestI && 0 <= bestJ && bestI <= valid.Length - 1 && bestJ <= valid.Length - 1)
            //    {
            //        foreach (int clB in facilities[bestJ].VertexIndices)
            //        {
            //            // add all B clients to A facility -> A is greedy!
            //            // dummy distance
            //            vertices[clB].AssignToFacility(facilities[bestI], 0);
            //            facilities[bestI].AddVertex(clB, 0);
            //        }

            //        facilities[bestJ].RemoveAllClient();

            //        facilities.RemoveAt(bestJ);

            //    }
            //    else
            //    {
            //        break;
            //    }

            //}



            // List<Facility> newFacilities = new List<Facility>();

            //for (int i = 0; i < facilities.Count; ++i) 
            //{
            //    // can be taken
            //    // dont judge me, it's PoC!
            //    if ( valid[i] )
            //    {
            //        Facility facA = facilities[i];

            //        double best = double.PositiveInfinity;
            //        int index = -1;

            //        for (int j = 0; j < facilities.Count; ++j)
            //        {
            //            // dont judge me, it's PoC!
            //            if ( i != j && valid[j] )
            //            {
            //                Facility facB = facilities[j];

            //                // calculate distance
            //                double dist = vertices[facA.VertexIndex].WeightedDistance( vertices[facB.VertexIndex] );

            //                if ( best > dist )
            //                {
            //                    best = dist;
            //                    index = j;
            //                }
            //            }
            //        }

            //        if ( 0 <= index && index <= valid.Length - 1)
            //        {
            //            valid[index] = false;

            //            foreach (int clB in facilities[index].VertexIndices)
            //            {
            //                // add all B clients to A facility -> A is greedy!
            //                // dummy distance
            //                vertices[clB].AssignToFacility(facA, 0);
            //                facA.AddVertex(clB, 0);
            //            }

            //            facilities[index].RemoveAllClient();
            //        } else
            //        {
            //            break;
            //        }
            //    }
            //}

            // foreach facilities which has valid flag
            //for (int j = facilities.Count - 1; j >= 0; --j)
            //{
            //    if ( !valid[j] )
            //    {
            //        facilities.RemoveAt(j);
            //    } else
            //    {
            //        // calculate middle of cluster
            //    }
            //}

            //showClusterGroups = true;
            //convexHull.Clear();

            //Console.WriteLine("Facilities: {0}", facilities.Count);

            //// it's time to calculate convex hull of clusters
            //foreach (Facility fac in facilities)
            //{
            //    List<Vertex> points = new List<Vertex>();
            //    foreach (int i in fac.VertexIndices)
            //    {
            //        points.Add(verticesOriginal[i]);
            //    }

            //    if ( points.Count > 3)
            //    {
            //        convexHull.Add(ConvexHull.MakeConvexHull(points));
            //    }
            //}

            //Canvas.Invalidate();

            IClustering fac = new MShift();
            // IClustering fac = new KMeans();
            // IClustering fac = new CMeans();
            // IClustering fac = new FacilityLocation();

            // add some properties to particular clustering algorithms
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("boundingBox", bBox);

            // properties.Add("numberOfClusters", int.Parse(TextBox_Param1.Text));

            properties.Add("BANDWIDTH", double.Parse(TextBox_Param1.Text));
            properties.Add("ITERATION", double.Parse(TextBox_Param2.Text));
            //properties.Add("treshold", 0.0);
            //properties.Add("weights", new double[] { 1, 1, 1, 1, 1, 1, 1, 1 });

            fac.SetProperties(properties);

            // transform facilities to vertices
            Vertex[] ret = new Vertex[facilities.Count];

            for ( int i = 0; i < facilities.Count; ++i )
            {
                ret[i] = new Vertex(vertices[facilities[i].VertexIndex].Coords);
            }

            vertices = ret;

            fac.ComputeClustering(vertices);

            showPoints = false;

            // save clustering result
            facilities = fac.GetFacilities();

            ShowCalculateClusterConvexHull();

            CalculateFacilitiesPosition();

            //Graph.Draw();

            //if (Dialog_SaveCluster.ShowDialog() == DialogResult.OK)
            //{
            //    ClusterSolution.SaveClusteringSolution(fac, inputFile, Dialog_SaveCluster.FileName, facilities);
            //}

        }
    }
}

