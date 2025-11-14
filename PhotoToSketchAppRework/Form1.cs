using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using EventChainsCore;
using ImageProcessing.Events;
using ImageProcessing.Middleware;

namespace PhotoToSketchRework
{
    public partial class MainForm : Form
    {
        private Bitmap originalImage;
        private Bitmap sketchImage;
        private ChainResult lastResult;

        public MainForm()
        {
            InitializeComponent();
            InitializeProcessingMethodComboBox();
        }

        private void InitializeProcessingMethodComboBox()
        {
            // Add this to your form designer or create programmatically
            if (cmbProcessingMethod != null)
            {
                cmbProcessingMethod.Items.AddRange(new object[]
                {
                    // ⚡⚡⚡ FASTEST TIER
                    "Roberts",
                    "Prewitt",
                    "Sobel",
                    "Scharr",
        
                    // ⚡⚡ FAST TIER
                    "DoG (Difference of Gaussians)",
                    "Lindeberg Scale Space",
                    "LoG",
                    "Kirsch",
        
                    // ⚙️ MEDIUM TIER
                    "Morphological Gradient",
                    "Edge Flow",
                    "Canny Edge Detection",
        
                    // 🐌 SLOW TIER
                    "Gabor Filter",
                    "Gabor Filter Bank",
                    "Phase Congruency",
        
                    // 🐢 SLOWEST TIER
                    "SIFT Feature Detection",
        
                    // COMBINATIONS
                    "Sobel + DoG",
                    "Sobel + LoG",
                    "DoG + Phase Congruency + Edge Flow",
        
                    // ADAPTIVE (uses multiple algorithms per region)
                    "Automatic Regions"
                });
                cmbProcessingMethod.SelectedIndex = 0;
            }
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
                    lastResult = null;
                    
                    // Clear previous metrics
                    if (lblMetrics != null)
                    {
                        lblMetrics.Text = "";
                    }
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

            // Disable button during processing
            btnProcess.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                // Build the appropriate chain based on selected method
                var chain = BuildProcessingChain(originalImage);

                // Execute with full observability
                lastResult = chain.ExecuteWithResults();

                if (lastResult.Success)
                {
                    sketchImage = lastResult.Context.Get<Bitmap>("sketch_bitmap");
                    picSketch.Image = sketchImage;
                    
                    // Display metrics
                    DisplayMetrics(lastResult);
                    
                    MessageBox.Show(
                        $"Processing Complete!\n\n" +
                        $"Quality Grade: {lastResult.GetGrade()}\n" +
                        $"Precision Score: {lastResult.TotalPrecisionScore:F2}/100\n" +
                        $"Processing Time: {lastResult.ExecutionTimeMs}ms\n" +
                        $"Events Executed: {lastResult.SuccessCount}/{lastResult.TotalCount}",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    // Show detailed error information
                    var failedEvent = lastResult.EventResults.Find(r => !r.Success);
                    MessageBox.Show(
                        $"Processing Failed!\n\n" +
                        $"Failed Event: {failedEvent?.EventName}\n" +
                        $"Error: {failedEvent?.ErrorMessage}\n\n" +
                        $"Events Completed: {lastResult.SuccessCount}/{lastResult.TotalCount}",
                        "Processing Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An unexpected error occurred:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnProcess.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private EventChain BuildProcessingChain(Bitmap bitmap)
        {
            var builder = new ImageProcessingChainBuilder(bitmap);
            var selectedMethod = cmbProcessingMethod?.SelectedIndex ?? 0;

            EventChain chain;

            switch (selectedMethod)
            {
                // ⚡⚡⚡ FASTEST TIER
                case 0: // Roberts
                    chain = builder.BuildSingleAlgorithmChain(
                        EdgeDetectionLib.Algorithms.RobertsProcessor.Apply,
                        "Roberts");
                    break;

                case 1: // Prewitt
                    chain = builder.BuildSingleAlgorithmChain(
                        EdgeDetectionLib.Algorithms.PrewittProcessor.Apply,
                        "Prewitt");
                    break;

                case 2: // Sobel
                    chain = builder.BuildSingleAlgorithmChain(
                        EdgeDetectionLib.Algorithms.SobelProcessor.Apply,
                        "Sobel");
                    break;

                case 3: // Scharr
                    chain = builder.BuildSingleAlgorithmChain(
                        EdgeDetectionLib.Algorithms.ScharrProcessor.Apply,
                        "Scharr");
                    break;

                // ⚡⚡ FAST TIER
                case 4: // DoG (DEFAULT - best balance)
                    chain = builder.BuildSingleAlgorithmChain(
                        EdgeDetectionLib.Algorithms.DoGProcessor.Apply,
                        "DoG");
                    break;

                case 5: // Lindeberg Scale Space
                    chain = builder.BuildSingleAlgorithmChain(
                        EdgeDetectionLib.Algorithms.LindebergScaleSpaceProcessor.Apply,
                        "LindebergScaleSpace");
                    break;

                case 6: // LoG
                    chain = builder.BuildSingleAlgorithmChain(
                        EdgeDetectionLib.Algorithms.LoGProcessor.Apply,
                        "LoG");
                    break;

                case 7: // Kirsch
                    chain = builder.BuildSingleAlgorithmChain(
                        EdgeDetectionLib.Algorithms.KirschProcessor.Apply,
                        "Kirsch");
                    break;

                // ⚙️ MEDIUM TIER
                case 8: // Morphological Gradient
                    chain = builder.BuildSingleAlgorithmChain(
                        EdgeDetectionLib.Algorithms.MorphologicalReconstructionProcessor.Apply,
                        "MorphologicalGradient");
                    break;

                case 9: // Edge Flow
                    chain = builder.BuildSingleAlgorithmChain(
                        EdgeDetectionLib.Algorithms.EdgeFlowProcessor.Apply,
                        "EdgeFlow");
                    break;

                case 10: // Canny
                    chain = builder.BuildSingleAlgorithmChain(
                        EdgeDetectionLib.Algorithms.CannyProcessor.Apply,
                        "Canny");
                    break;

                // 🐌 SLOW TIER
                case 11: // Gabor Filter
                    chain = builder.BuildSingleAlgorithmChain(
                        img => EdgeDetectionLib.Algorithms.GaborFilterProcessor.Apply(img, Math.PI / 4, 4.0, 2.0, 0.5),
                        "GaborFilter");
                    break;

                case 12: // Gabor Filter Bank
                    chain = builder.BuildSingleAlgorithmChain(
                        EdgeDetectionLib.Algorithms.GaborFilterBankProcessor.Apply,
                        "GaborFilterBank");
                    break;

                case 13: // Phase Congruency
                    chain = builder.BuildSingleAlgorithmChain(
                        EdgeDetectionLib.Algorithms.PhaseCongruencyProcessor.Apply,
                        "PhaseCongruency");
                    break;

                // 🐢 SLOWEST TIER
                case 14: // SIFT
                    chain = builder.BuildSingleAlgorithmChain(
                        EdgeDetectionLib.Algorithms.SIFTProcessor.Apply,
                        "SIFT");
                    break;

                // COMBINATIONS
                case 15: // Sobel + DoG
                    chain = builder.BuildDualAlgorithmChain(
                        EdgeDetectionLib.Algorithms.SobelProcessor.Apply,
                        "Sobel",
                        EdgeDetectionLib.Algorithms.DoGProcessor.Apply,
                        "DoG");
                    break;

                case 16: // Sobel + LoG
                    chain = builder.BuildDualAlgorithmChain(
                        EdgeDetectionLib.Algorithms.SobelProcessor.Apply,
                        "Sobel",
                        EdgeDetectionLib.Algorithms.LoGProcessor.Apply,
                        "LoG");
                    break;

                case 17: // DoG + Phase Congruency + Edge Flow (Triple)
                    chain = builder.BuildTripleAlgorithmChain(
                        EdgeDetectionLib.Algorithms.DoGProcessor.Apply,
                        "DoG",
                        EdgeDetectionLib.Algorithms.PhaseCongruencyProcessor.Apply,
                        "PhaseCongruency",
                        EdgeDetectionLib.Algorithms.EdgeFlowProcessor.Apply,
                        "EdgeFlow");
                    break;

                // ADAPTIVE
                case 18: // Automatic Regions
                    chain = builder.BuildAutomaticRegionChain(blockSize: 32);
                    break;

                default:
                    // Default to DoG - best quality/speed balance
                    chain = builder.BuildSingleAlgorithmChain(
                        EdgeDetectionLib.Algorithms.DoGProcessor.Apply,
                        "DoG");
                    break;
            }

            // Add middleware for observability
            chain.UseMiddleware(ImageProcessingMiddleware.Timing())
                 .UseMiddleware(ImageProcessingMiddleware.MemoryTracking())
                 .UseMiddleware(ImageProcessingMiddleware.Logging(message =>
                 {
                     Debug.WriteLine(message);
                     
                     // Update progress label on UI thread
                     if (lblProgress != null && !IsDisposed)
                     {
                         try
                         {
                             lblProgress.BeginInvoke((MethodInvoker)(() =>
                             {
                                 lblProgress.Text = message;
                             }));
                         }
                         catch
                         {
                             // Ignore if form is disposing
                         }
                     }
                 }));

            return chain;
        }

        private void DisplayMetrics(ChainResult result)
        {
            if (lblMetrics == null) return;

            var metricsText = $"Quality Grade: {result.GetGrade()} | " +
                             $"Precision: {result.TotalPrecisionScore:F2} | " +
                             $"Time: {result.ExecutionTimeMs}ms | " +
                             $"Success Rate: {result.SuccessCount}/{result.TotalCount}";

            // Get timing details if available
            if (result.Context.TryGet<System.Collections.Generic.List<TimingMetric>>(
                "timing_metrics", out var timingMetrics))
            {
                metricsText += "\n\nDetailed Timing:";
                foreach (var timing in timingMetrics)
                {
                    metricsText += $"\n  {timing.EventName}: {timing.ElapsedMs}ms";
                }
            }

            // Get memory details if available
            if (result.Context.TryGet<System.Collections.Generic.List<MemoryMetric>>(
                "memory_metrics", out var memoryMetrics))
            {
                metricsText += "\n\nMemory Usage:";
                foreach (var memory in memoryMetrics)
                {
                    var deltaMB = memory.MemoryDeltaBytes / (1024.0 * 1024.0);
                    metricsText += $"\n  {memory.EventName}: {deltaMB:+0.00;-0.00} MB";
                }
            }

            lblMetrics.Text = metricsText;
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
                            $"Location: {saveFileDialog.FileName}\n" +
                            (lastResult != null ? $"Quality Grade: {lastResult.GetGrade()}\n" : "") +
                            (lastResult != null ? $"Precision Score: {lastResult.TotalPrecisionScore:F2}" : ""),
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

        private void btnViewMetrics_Click(object sender, EventArgs e)
        {
            if (lastResult == null)
            {
                MessageBox.Show("No processing results available. Process an image first.");
                return;
            }

            // Create detailed metrics report
            var report = $"=== Processing Metrics Report ===\n\n";
            report += $"Overall Success: {lastResult.Success}\n";
            report += $"Quality Grade: {lastResult.GetGrade()}\n";
            report += $"Total Precision Score: {lastResult.TotalPrecisionScore:F2}/100\n";
            report += $"Execution Time: {lastResult.ExecutionTimeMs}ms\n";
            report += $"Events Executed: {lastResult.SuccessCount}/{lastResult.TotalCount}\n";
            report += $"Failed Events: {lastResult.FailureCount}\n\n";

            report += "=== Event Details ===\n";
            foreach (var eventResult in lastResult.EventResults)
            {
                var status = eventResult.Success ? "✓ SUCCESS" : "✗ FAILED";
                report += $"\n{eventResult.EventName}:\n";
                report += $"  Status: {status}\n";
                report += $"  Precision: {eventResult.PrecisionScore:F2}\n";
                
                if (!eventResult.Success && !string.IsNullOrEmpty(eventResult.ErrorMessage))
                {
                    report += $"  Error: {eventResult.ErrorMessage}\n";
                }
            }

            // Show timing metrics
            if (lastResult.Context.TryGet<System.Collections.Generic.List<TimingMetric>>(
                "timing_metrics", out var timingMetrics))
            {
                report += "\n=== Timing Breakdown ===\n";
                foreach (var timing in timingMetrics)
                {
                    report += $"{timing.EventName}: {timing.ElapsedMs}ms\n";
                }
            }

            // Show memory metrics
            if (lastResult.Context.TryGet<System.Collections.Generic.List<MemoryMetric>>(
                "memory_metrics", out var memoryMetrics))
            {
                report += "\n=== Memory Usage ===\n";
                foreach (var memory in memoryMetrics)
                {
                    var deltaMB = memory.MemoryDeltaBytes / (1024.0 * 1024.0);
                    var totalMB = memory.TotalMemoryAfter / (1024.0 * 1024.0);
                    report += $"{memory.EventName}:\n";
                    report += $"  Delta: {deltaMB:+0.00;-0.00} MB\n";
                    report += $"  Total After: {totalMB:F2} MB\n";
                }
            }

            // Display in a scrollable message box or create a new form
            var metricsForm = new Form
            {
                Text = "Detailed Processing Metrics",
                Width = 600,
                Height = 500,
                StartPosition = FormStartPosition.CenterParent
            };

            var textBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                Text = report
            };

            var closeButton = new Button
            {
                Text = "Close",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            closeButton.Click += (s, args) => metricsForm.Close();

            metricsForm.Controls.Add(textBox);
            metricsForm.Controls.Add(closeButton);
            metricsForm.ShowDialog();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            int buttonY = this.ClientSize.Height - 40;  // Bottom padding
            if (btnLoadImage != null) btnLoadImage.Top = buttonY;
            if (btnProcess != null) btnProcess.Top = buttonY;
            if (btnSaveSketch != null) btnSaveSketch.Top = buttonY;
            if (btnViewMetrics != null) btnViewMetrics.Top = buttonY;
        }
    }
}