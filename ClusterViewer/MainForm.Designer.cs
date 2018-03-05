namespace ClusterViewer
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.Dialog_OpenFile = new System.Windows.Forms.OpenFileDialog();
            this.Menu = new System.Windows.Forms.ToolStrip();
            this.Menu_Load = new System.Windows.Forms.ToolStripButton();
            this.Menu_Cluster = new System.Windows.Forms.ToolStripButton();
            this.Menu_ClusterSolver = new System.Windows.Forms.ToolStripButton();
            this.Menu_ClusterBatch = new System.Windows.Forms.ToolStripButton();
            this.Menu_ClusterLoad = new System.Windows.Forms.ToolStripButton();
            this.TextBox_Param1 = new System.Windows.Forms.ToolStripTextBox();
            this.TextBox_Param2 = new System.Windows.Forms.ToolStripTextBox();
            this.Menu_Grouping = new System.Windows.Forms.ToolStripButton();
            this.Dialog_InputFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.Dialog_OutputFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.Dialog_OpenClusterResult = new System.Windows.Forms.OpenFileDialog();
            this.Dialog_SaveCluster = new System.Windows.Forms.SaveFileDialog();
            this.Graph = new ClusterViewer.Canvas();
            this.Menu.SuspendLayout();
            this.SuspendLayout();
            // 
            // Dialog_OpenFile
            // 
            this.Dialog_OpenFile.FileName = "Open file dialog";
            // 
            // Menu
            // 
            this.Menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Menu_Load,
            this.Menu_Cluster,
            this.Menu_ClusterSolver,
            this.Menu_ClusterBatch,
            this.Menu_ClusterLoad,
            this.TextBox_Param1,
            this.TextBox_Param2,
            this.Menu_Grouping});
            this.Menu.Location = new System.Drawing.Point(0, 0);
            this.Menu.Name = "Menu";
            this.Menu.Size = new System.Drawing.Size(570, 25);
            this.Menu.TabIndex = 2;
            this.Menu.Text = "toolStrip1";
            // 
            // Menu_Load
            // 
            this.Menu_Load.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Menu_Load.Image = global::ClusterViewer.Properties.Resources.folder;
            this.Menu_Load.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Menu_Load.Name = "Menu_Load";
            this.Menu_Load.Size = new System.Drawing.Size(23, 22);
            this.Menu_Load.Text = "Load file";
            this.Menu_Load.Click += new System.EventHandler(this.Main_Load_Click);
            // 
            // Menu_Cluster
            // 
            this.Menu_Cluster.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Menu_Cluster.Enabled = false;
            this.Menu_Cluster.Image = global::ClusterViewer.Properties.Resources.cluster;
            this.Menu_Cluster.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Menu_Cluster.Name = "Menu_Cluster";
            this.Menu_Cluster.Size = new System.Drawing.Size(23, 22);
            this.Menu_Cluster.Text = "Cluster";
            this.Menu_Cluster.Click += new System.EventHandler(this.Main_Cluster_Click);
            // 
            // Menu_ClusterSolver
            // 
            this.Menu_ClusterSolver.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Menu_ClusterSolver.Enabled = false;
            this.Menu_ClusterSolver.Image = ((System.Drawing.Image)(resources.GetObject("Menu_ClusterSolver.Image")));
            this.Menu_ClusterSolver.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Menu_ClusterSolver.Name = "Menu_ClusterSolver";
            this.Menu_ClusterSolver.Size = new System.Drawing.Size(23, 22);
            this.Menu_ClusterSolver.Text = "Cluster solver";
            this.Menu_ClusterSolver.Click += new System.EventHandler(this.Menu_ClusterSolver_Click);
            // 
            // Menu_ClusterBatch
            // 
            this.Menu_ClusterBatch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Menu_ClusterBatch.Image = ((System.Drawing.Image)(resources.GetObject("Menu_ClusterBatch.Image")));
            this.Menu_ClusterBatch.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Menu_ClusterBatch.Name = "Menu_ClusterBatch";
            this.Menu_ClusterBatch.Size = new System.Drawing.Size(23, 22);
            this.Menu_ClusterBatch.Text = "Cluster folder";
            this.Menu_ClusterBatch.Click += new System.EventHandler(this.Menu_ClusterBatch_Click);
            // 
            // Menu_ClusterLoad
            // 
            this.Menu_ClusterLoad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Menu_ClusterLoad.Image = ((System.Drawing.Image)(resources.GetObject("Menu_ClusterLoad.Image")));
            this.Menu_ClusterLoad.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Menu_ClusterLoad.Name = "Menu_ClusterLoad";
            this.Menu_ClusterLoad.Size = new System.Drawing.Size(23, 22);
            this.Menu_ClusterLoad.Text = "Load clustering result";
            this.Menu_ClusterLoad.Click += new System.EventHandler(this.Menu_ClusterLoad_Click);
            // 
            // TextBox_Param1
            // 
            this.TextBox_Param1.Name = "TextBox_Param1";
            this.TextBox_Param1.Size = new System.Drawing.Size(100, 25);
            this.TextBox_Param1.Text = "1";
            // 
            // TextBox_Param2
            // 
            this.TextBox_Param2.Name = "TextBox_Param2";
            this.TextBox_Param2.Size = new System.Drawing.Size(100, 25);
            this.TextBox_Param2.Text = "1";
            // 
            // Menu_Grouping
            // 
            this.Menu_Grouping.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Menu_Grouping.Image = ((System.Drawing.Image)(resources.GetObject("Menu_Grouping.Image")));
            this.Menu_Grouping.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Menu_Grouping.Name = "Menu_Grouping";
            this.Menu_Grouping.Size = new System.Drawing.Size(23, 22);
            this.Menu_Grouping.Text = "Grouping";
            this.Menu_Grouping.Click += new System.EventHandler(this.Menu_Grouping_Click);
            // 
            // Dialog_InputFolder
            // 
            this.Dialog_InputFolder.Description = "Select input folder with data sets:";
            // 
            // Dialog_OutputFolder
            // 
            this.Dialog_OutputFolder.Description = "Select output folder for results:";
            // 
            // Dialog_OpenClusterResult
            // 
            this.Dialog_OpenClusterResult.FileName = "Cluster result";
            // 
            // Graph
            // 
            this.Graph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Graph.Location = new System.Drawing.Point(0, 25);
            this.Graph.Name = "Graph";
            this.Graph.Size = new System.Drawing.Size(570, 394);
            this.Graph.TabIndex = 3;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(570, 419);
            this.Controls.Add(this.Graph);
            this.Controls.Add(this.Menu);
            this.Name = "MainForm";
            this.Text = "Cluster Viewer";
            this.Menu.ResumeLayout(false);
            this.Menu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
        private System.Windows.Forms.OpenFileDialog Dialog_OpenFile;
        private System.Windows.Forms.ToolStrip Menu;
        private System.Windows.Forms.ToolStripButton Menu_Load;
        private System.Windows.Forms.ToolStripButton Menu_Cluster;
        private System.Windows.Forms.ToolStripButton Menu_ClusterSolver;
        private System.Windows.Forms.ToolStripButton Menu_ClusterBatch;
        private System.Windows.Forms.FolderBrowserDialog Dialog_InputFolder;
        private System.Windows.Forms.FolderBrowserDialog Dialog_OutputFolder;
        private System.Windows.Forms.ToolStripButton Menu_ClusterLoad;
        private System.Windows.Forms.OpenFileDialog Dialog_OpenClusterResult;
        private System.Windows.Forms.SaveFileDialog Dialog_SaveCluster;
        private System.Windows.Forms.ToolStripTextBox TextBox_Param1;
        private System.Windows.Forms.ToolStripTextBox TextBox_Param2;
        private System.Windows.Forms.ToolStripButton Menu_Grouping;
        private Canvas Graph;
    }
}

