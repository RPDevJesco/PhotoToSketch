namespace PhotoToSketchApp
{
    public partial class MainForm : Form
    {
        private Bitmap originalImage;
        private Bitmap sketchImage;

        public MainForm()
        {
            InitializeComponent();
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.png;*.bmp;*.jpeg";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    originalImage = new Bitmap(openFileDialog.FileName);
                    picOriginal.Image = originalImage;
                    sketchImage = null;
                    picSketch.Image = null;
                }
            }
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Please load an image first.");
                return;
            }

            var processor = new ImageSketchProcessor();
            sketchImage = processor.ProcessImage(originalImage);
            picSketch.Image = sketchImage;
        }

        private void btnSaveSketch_Click(object sender, EventArgs e)
        {
            if (sketchImage == null)
            {
                MessageBox.Show("No processed image to save.");
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "PNG Files|*.png";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    sketchImage.Save(saveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }
        
        private void MainForm_Resize(object sender, EventArgs e)
        {
            int buttonY = this.ClientSize.Height - 40;  // Bottom padding
            btnLoadImage.Top = buttonY;
            btnProcess.Top = buttonY;
            btnSaveSketch.Top = buttonY;
        }
    }
}