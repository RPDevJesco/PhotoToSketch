namespace PhotoToSketchRework
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.PictureBox picOriginal;
        private System.Windows.Forms.PictureBox picSketch;
        private System.Windows.Forms.Button btnLoadImage;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.Button btnSaveSketch;
        private System.Windows.Forms.Button btnViewMetrics;
        private System.Windows.Forms.ComboBox cmbProcessingMethod;
        private System.Windows.Forms.Label lblOriginal;
        private System.Windows.Forms.Label lblSketch;
        private System.Windows.Forms.Label lblMethod;
        private System.Windows.Forms.Label lblMetrics;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.Panel pnlMetrics;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.picOriginal = new System.Windows.Forms.PictureBox();
            this.picSketch = new System.Windows.Forms.PictureBox();
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.btnProcess = new System.Windows.Forms.Button();
            this.btnSaveSketch = new System.Windows.Forms.Button();
            this.btnViewMetrics = new System.Windows.Forms.Button();
            this.cmbProcessingMethod = new System.Windows.Forms.ComboBox();
            this.lblOriginal = new System.Windows.Forms.Label();
            this.lblSketch = new System.Windows.Forms.Label();
            this.lblMethod = new System.Windows.Forms.Label();
            this.lblMetrics = new System.Windows.Forms.Label();
            this.lblProgress = new System.Windows.Forms.Label();
            this.pnlMetrics = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.picOriginal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSketch)).BeginInit();
            this.pnlMetrics.SuspendLayout();
            this.SuspendLayout();
            // 
            // picOriginal
            // 
            this.picOriginal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picOriginal.Location = new System.Drawing.Point(12, 35);
            this.picOriginal.Name = "picOriginal";
            this.picOriginal.Size = new System.Drawing.Size(450, 450);
            this.picOriginal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picOriginal.TabIndex = 0;
            this.picOriginal.TabStop = false;
            // 
            // picSketch
            // 
            this.picSketch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picSketch.Location = new System.Drawing.Point(480, 35);
            this.picSketch.Name = "picSketch";
            this.picSketch.Size = new System.Drawing.Size(450, 450);
            this.picSketch.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picSketch.TabIndex = 1;
            this.picSketch.TabStop = false;
            // 
            // btnLoadImage
            // 
            this.btnLoadImage.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnLoadImage.Location = new System.Drawing.Point(12, 610);
            this.btnLoadImage.Name = "btnLoadImage";
            this.btnLoadImage.Size = new System.Drawing.Size(130, 35);
            this.btnLoadImage.TabIndex = 2;
            this.btnLoadImage.Text = "Load Image";
            this.btnLoadImage.UseVisualStyleBackColor = true;
            this.btnLoadImage.Click += new System.EventHandler(this.btnLoadImage_Click);
            // 
            // btnProcess
            // 
            this.btnProcess.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnProcess.Location = new System.Drawing.Point(360, 610);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(130, 35);
            this.btnProcess.TabIndex = 3;
            this.btnProcess.Text = "Process";
            this.btnProcess.UseVisualStyleBackColor = true;
            this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
            // 
            // btnSaveSketch
            // 
            this.btnSaveSketch.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSaveSketch.Location = new System.Drawing.Point(650, 610);
            this.btnSaveSketch.Name = "btnSaveSketch";
            this.btnSaveSketch.Size = new System.Drawing.Size(130, 35);
            this.btnSaveSketch.TabIndex = 4;
            this.btnSaveSketch.Text = "Save Sketch";
            this.btnSaveSketch.UseVisualStyleBackColor = true;
            this.btnSaveSketch.Click += new System.EventHandler(this.btnSaveSketch_Click);
            // 
            // btnViewMetrics
            // 
            this.btnViewMetrics.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnViewMetrics.Location = new System.Drawing.Point(800, 610);
            this.btnViewMetrics.Name = "btnViewMetrics";
            this.btnViewMetrics.Size = new System.Drawing.Size(130, 35);
            this.btnViewMetrics.TabIndex = 5;
            this.btnViewMetrics.Text = "View Metrics";
            this.btnViewMetrics.UseVisualStyleBackColor = true;
            this.btnViewMetrics.Click += new System.EventHandler(this.btnViewMetrics_Click);
            // 
            // cmbProcessingMethod
            // 
            this.cmbProcessingMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProcessingMethod.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbProcessingMethod.FormattingEnabled = true;
            this.cmbProcessingMethod.Location = new System.Drawing.Point(150, 617);
            this.cmbProcessingMethod.Name = "cmbProcessingMethod";
            this.cmbProcessingMethod.Size = new System.Drawing.Size(200, 23);
            this.cmbProcessingMethod.TabIndex = 6;
            // 
            // lblOriginal
            // 
            this.lblOriginal.AutoSize = true;
            this.lblOriginal.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblOriginal.Location = new System.Drawing.Point(12, 9);
            this.lblOriginal.Name = "lblOriginal";
            this.lblOriginal.Size = new System.Drawing.Size(103, 20);
            this.lblOriginal.TabIndex = 7;
            this.lblOriginal.Text = "Original Image";
            // 
            // lblSketch
            // 
            this.lblSketch.AutoSize = true;
            this.lblSketch.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblSketch.Location = new System.Drawing.Point(480, 9);
            this.lblSketch.Name = "lblSketch";
            this.lblSketch.Size = new System.Drawing.Size(94, 20);
            this.lblSketch.TabIndex = 8;
            this.lblSketch.Text = "Sketch Result";
            // 
            // lblMethod
            // 
            this.lblMethod.AutoSize = true;
            this.lblMethod.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMethod.Location = new System.Drawing.Point(150, 599);
            this.lblMethod.Name = "lblMethod";
            this.lblMethod.Size = new System.Drawing.Size(107, 15);
            this.lblMethod.TabIndex = 9;
            this.lblMethod.Text = "Processing Method:";
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = true;
            this.lblProgress.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblProgress.ForeColor = System.Drawing.Color.Blue;
            this.lblProgress.Location = new System.Drawing.Point(500, 620);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(0, 15);
            this.lblProgress.TabIndex = 10;
            // 
            // pnlMetrics
            // 
            this.pnlMetrics.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlMetrics.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlMetrics.Controls.Add(this.lblMetrics);
            this.pnlMetrics.Location = new System.Drawing.Point(12, 495);
            this.pnlMetrics.Name = "pnlMetrics";
            this.pnlMetrics.Size = new System.Drawing.Size(918, 95);
            this.pnlMetrics.TabIndex = 11;
            // 
            // lblMetrics
            // 
            this.lblMetrics.AutoSize = true;
            this.lblMetrics.Font = new System.Drawing.Font("Consolas", 9F);
            this.lblMetrics.Location = new System.Drawing.Point(5, 5);
            this.lblMetrics.Name = "lblMetrics";
            this.lblMetrics.Size = new System.Drawing.Size(0, 14);
            this.lblMetrics.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(944, 661);
            this.Controls.Add(this.pnlMetrics);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.lblMethod);
            this.Controls.Add(this.lblSketch);
            this.Controls.Add(this.lblOriginal);
            this.Controls.Add(this.cmbProcessingMethod);
            this.Controls.Add(this.btnViewMetrics);
            this.Controls.Add(this.btnSaveSketch);
            this.Controls.Add(this.btnProcess);
            this.Controls.Add(this.btnLoadImage);
            this.Controls.Add(this.picSketch);
            this.Controls.Add(this.picOriginal);
            this.MinimumSize = new System.Drawing.Size(960, 700);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Photo to Sketch - EventChain Powered";
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.picOriginal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSketch)).EndInit();
            this.pnlMetrics.ResumeLayout(false);
            this.pnlMetrics.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}