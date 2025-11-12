using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using PhotoToSketchApp;

namespace PhotoToSketchApp
{
    public partial class MainForm : Form
    {
        private Bitmap originalImage;
        private Bitmap sketchImage;

        public MainForm()
        {
            InitializeComponent();
            InitializeProcessingMethodComboBox();
        }

        private void InitializeProcessingMethodComboBox()
        {
            if (cmbProcessingMethod != null)
            {
                cmbProcessingMethod.Items.AddRange(new object[]
                {
                    "Automatic Regions (Recommended) ⚠ Slow",
                    "Sobel Edge Detection ⚡ Fast",
                    "Prewitt Edge Detection ⚡ Fast",
                    "DoG (Difference of Gaussians) ⚡ Fast",
                    "Phase Congruency ⚠ Slow",
                    "Edge Flow ⚡ Fast",
                    "Sobel + DoG Combination ⚡ Fast",
                    "DoG + Phase Congruency + Edge Flow ⚠ Very Slow",
                    "Lindeberg Scale Space ⚡ Fast"
                });
                cmbProcessingMethod.SelectedIndex = 3; // Default to DoG (fast and S-grade quality)
            }
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.png;*.bmp;*.jpeg";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Dispose previous image
                    originalImage?.Dispose();
                    
                    originalImage = new Bitmap(openFileDialog.FileName);
                    picOriginal.Image = originalImage;
                    
                    // Dispose previous sketch
                    sketchImage?.Dispose();
                    sketchImage = null;
                    picSketch.Image = null;
                    
                    // Clear metrics
                    lblMetrics.Text = "";
                    
                    // Show image info
                    UpdateStatusMessage($"Image loaded: {originalImage.Width} x {originalImage.Height} pixels");
                }
            }
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Please load an image first.", "No Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Disable UI during processing
            btnProcess.Enabled = false;
            btnLoadImage.Enabled = false;
            btnSaveSketch.Enabled = false;
            cmbProcessingMethod.Enabled = false;
            Cursor = Cursors.WaitCursor;
            
            UpdateStatusMessage("Processing image...");
            Application.DoEvents(); // Allow UI to update

            try
            {
                // Time the processing
                var stopwatch = Stopwatch.StartNew();
                
                // Process the image with selected method
                var processor = new ImageSketchProcessor();
                var selectedMethod = cmbProcessingMethod?.SelectedIndex ?? 3;
                
                switch (selectedMethod)
                {
                    case 0: // Automatic Regions
                        sketchImage = processor.ProcessWithAutomaticRegions(originalImage);
                        break;
                    case 1: // Sobel
                        sketchImage = processor.ProcessWithSobel(originalImage);
                        break;
                    case 2: // Prewitt
                        sketchImage = processor.ProcessWithPrewitt(originalImage);
                        break;
                    case 3: // DoG
                        sketchImage = processor.ProcessWithDoG(originalImage);
                        break;
                    case 4: // Phase Congruency
                        sketchImage = processor.ProcessWithPhaseCongruency(originalImage);
                        break;
                    case 5: // Edge Flow
                        sketchImage = processor.ProcessWithEdgeFlow(originalImage);
                        break;
                    case 6: // Sobel + DoG
                        sketchImage = processor.ProcessWithSobelAndLoG(originalImage);
                        break;
                    case 7: // Triple Combo
                        sketchImage = processor.ProcessWithDoGPhaseCongruencyAndEdgeFlow(originalImage);
                        break;
                    case 8: // Lindeberg
                        sketchImage = processor.ProcessWithLindebergScaleSpace(originalImage);
                        break;
                    default:
                        sketchImage = processor.ProcessWithDoG(originalImage);
                        break;
                }
                
                stopwatch.Stop();
                
                // Display result
                picSketch.Image = sketchImage;
                
                // Show metrics
                var methodName = cmbProcessingMethod?.SelectedItem?.ToString() ?? "Unknown";
                DisplayMetrics(stopwatch.ElapsedMilliseconds, methodName);
                
                // Show success message
                MessageBox.Show(
                    $"Processing Complete!\n\n" +
                    $"Method: {methodName}\n" +
                    $"Processing Time: {stopwatch.ElapsedMilliseconds}ms ({stopwatch.ElapsedMilliseconds / 1000.0:F1}s)",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred during processing:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                
                lblMetrics.Text = $"Error: {ex.Message}";
            }
            finally
            {
                // Re-enable UI
                btnProcess.Enabled = true;
                btnLoadImage.Enabled = true;
                btnSaveSketch.Enabled = true;
                cmbProcessingMethod.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void btnSaveSketch_Click(object sender, EventArgs e)
        {
            if (sketchImage == null)
            {
                MessageBox.Show("No processed image to save.", "No Sketch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "PNG Files|*.png|JPEG Files|*.jpg|Bitmap Files|*.bmp";
                saveFileDialog.DefaultExt = "png";
                saveFileDialog.FileName = "sketch_output.png";
                
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var format = System.Drawing.Imaging.ImageFormat.Png;
                        var extension = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();
                        
                        if (extension == ".jpg" || extension == ".jpeg")
                            format = System.Drawing.Imaging.ImageFormat.Jpeg;
                        else if (extension == ".bmp")
                            format = System.Drawing.Imaging.ImageFormat.Bmp;

                        sketchImage.Save(saveFileDialog.FileName, format);
                        
                        MessageBox.Show(
                            $"Sketch saved successfully!\n\n" +
                            $"Location: {saveFileDialog.FileName}",
                            "Save Successful",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Failed to save image:\n\n{ex.Message}",
                            "Save Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DisplayMetrics(long processingTimeMs, string methodName)
        {
            var metricsText = $"Method: {methodName} | " +
                             $"Processing Time: {processingTimeMs}ms ({processingTimeMs / 1000.0:F1}s) | " +
                             $"Image Size: {originalImage.Width} x {originalImage.Height} | " +
                             $"Status: Success";

            lblMetrics.Text = metricsText;
        }

        private void UpdateStatusMessage(string message)
        {
            if (lblMetrics != null)
            {
                lblMetrics.Text = message;
            }
        }
        
        private void MainForm_Resize(object sender, EventArgs e)
        {
            int buttonY = this.ClientSize.Height - 40;  // Bottom padding
            if (btnLoadImage != null) btnLoadImage.Top = buttonY;
            if (btnProcess != null) btnProcess.Top = buttonY;
            if (btnSaveSketch != null) btnSaveSketch.Top = buttonY;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Cleanup resources
            originalImage?.Dispose();
            sketchImage?.Dispose();
            
            base.OnFormClosing(e);
        }
    }
}