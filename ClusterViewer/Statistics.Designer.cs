namespace ClusterViewer
{
    partial class Statistics
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
            this.text_Box = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // text_Box
            // 
            this.text_Box.Dock = System.Windows.Forms.DockStyle.Fill;
            this.text_Box.Location = new System.Drawing.Point(0, 0);
            this.text_Box.Name = "text_Box";
            this.text_Box.Size = new System.Drawing.Size(285, 268);
            this.text_Box.TabIndex = 0;
            this.text_Box.Text = "";
            // 
            // Statistics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(285, 268);
            this.Controls.Add(this.text_Box);
            this.Name = "Statistics";
            this.Text = "Statistics";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox text_Box;
    }
}