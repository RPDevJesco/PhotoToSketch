namespace PhotoToSketchApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel tableLayoutPanel;
        private PictureBox picOriginal;
        private PictureBox picSketch;
        private Panel buttonPanel;
        private Button btnLoadImage;
        private Button btnProcess;
        private Button btnSaveSketch;

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
            this.tableLayoutPanel = new TableLayoutPanel();
            this.picOriginal = new PictureBox();
            this.picSketch = new PictureBox();
            this.buttonPanel = new Panel();
            this.btnLoadImage = new Button();
            this.btnProcess = new Button();
            this.btnSaveSketch = new Button();

            // TableLayoutPanel (for images)
            this.tableLayoutPanel.ColumnCount = 2;
            this.tableLayoutPanel.RowCount = 1;
            this.tableLayoutPanel.Dock = DockStyle.Fill;
            this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            this.picOriginal.Dock = DockStyle.Fill;
            this.picOriginal.SizeMode = PictureBoxSizeMode.Zoom;
            this.picOriginal.BorderStyle = BorderStyle.FixedSingle;

            this.picSketch.Dock = DockStyle.Fill;
            this.picSketch.SizeMode = PictureBoxSizeMode.Zoom;
            this.picSketch.BorderStyle = BorderStyle.FixedSingle;

            this.tableLayoutPanel.Controls.Add(this.picOriginal, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.picSketch, 1, 0);

            // Bottom Panel (for buttons)
            this.buttonPanel.Dock = DockStyle.Bottom;
            this.buttonPanel.Height = 60;
            this.buttonPanel.Padding = new Padding(15);

            this.btnLoadImage.Text = "Load Image";
            this.btnLoadImage.Location = new Point(10, 13);
            this.btnLoadImage.Click += new EventHandler(this.btnLoadImage_Click);

            this.btnProcess.Text = "Process";
            this.btnProcess.Location = new Point(120, 13);
            this.btnProcess.Click += new EventHandler(this.btnProcess_Click);

            this.btnSaveSketch.Text = "Save Sketch";
            this.btnSaveSketch.Location = new Point(230, 13);
            this.btnSaveSketch.Click += new EventHandler(this.btnSaveSketch_Click);

            // Add buttons to the buttonPanel
            this.buttonPanel.Controls.Add(this.btnLoadImage);
            this.buttonPanel.Controls.Add(this.btnProcess);
            this.buttonPanel.Controls.Add(this.btnSaveSketch);

            // Form Configuration
            this.Controls.Add(this.tableLayoutPanel);
            this.Controls.Add(this.buttonPanel);

            this.MinimumSize = new Size(800, 600);
            this.Text = "Photo to Sketch Converter";

            this.ResumeLayout(false);
        }
    }
}