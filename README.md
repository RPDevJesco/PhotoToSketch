# PhotoToSketch Converter

A powerful C# application that transforms photographs into high-quality sketch-like images using advanced edge detection algorithms.

## Overview

PhotoToSketch is a sophisticated image processing application that uses a combination of edge detection techniques to convert regular photographs into artistic sketch-like renderings. The application analyzes different regions of an image separately and applies the most appropriate edge detection algorithms based on the image's characteristics in each region.

## Features

- **Multiple Edge Detection Algorithms**: Implements over 10 different edge detection algorithms including Sobel, Prewitt, Roberts, Scharr, Kirsch, LoG (Laplacian of Gaussian), DoG (Difference of Gaussians), Gabor filters, Phase Congruency, Edge Flow, and Lindeberg Scale Space.
- **Intelligent Region Processing**: Automatically divides the image into regions and applies the optimal edge detection algorithm for each region based on texture and detail analysis.
- **Algorithm Combination**: Combines multiple edge detection algorithms for enhanced results.
- **Customizable Processing**: Offers both automatic region-based processing and manual algorithm selection.
- **User-Friendly Interface**: Simple UI with image preview and save functionality.

## Architecture

The application is structured into two main libraries:

### EdgeDetectionLib

Contains the core functionality and algorithm implementations:

- **Core**: Fundamental data structures (GrayscaleImage)
- **Algorithms**: Multiple edge detection algorithm implementations
  - Classical edge detectors (Sobel, Prewitt, Roberts, Scharr, Kirsch)
  - Advanced algorithms (LoG, DoG, Gabor filter, Phase Congruency)
  - Multi-scale methods (Lindeberg Scale Space)
  - Flow-based approaches (Edge Flow)
 
### ✅ Implemented Algorithms

| Algorithm               | Description |
|-------------------|--------------------|
| **Sobel**            | Detects edges using horizontal and vertical intensity differences. |
| **Prewitt**          | Similar to Sobel, but with a simpler kernel. |
| **Roberts**         | A fast 2x2 kernel for quick edge detection. |
| **Scharr**          | An optimized Sobel variant for better rotational symmetry. |
| **Kirsch**           | Uses multiple directional kernels to detect edges in all directions. |
| **Laplacian of Gaussian (LoG)** | Detects edges using second derivatives, combined with Gaussian smoothing. |
| **Difference of Gaussians (DoG)** | Edge detection based on subtracting blurred images at different scales. |
| **Gaussian Blur** | Basic Gaussian blur (used in multiple algorithms). |
| **Gabor Filter** | Frequency-based edge detector with directional selectivity. |
| **Phase Congruency** | Finds edges based on consistent phase information (good for low-contrast edges). |
| **Edge Flow** | Follows natural flow patterns to detect edges. |
| **Lindeberg Scale Space** | Multi-scale edge detection sensitive to object size. |

### PhotoToSketchApp

The user interface and high-level processing:

- **MainForm**: User interface for loading, processing, and saving images
- **ImageSketchProcessor**: Coordinates the application of algorithms
- **AutomaticRegionProcessor**: Analyzes image regions and applies appropriate algorithms
- **ImageConversionHelper**: Handles conversion between Bitmap and GrayscaleImage

## Technical Details

### Edge Detection Algorithms

1. **Sobel, Prewitt, Roberts, Scharr, Kirsch**: Classical gradient-based detectors using convolution kernels that identify intensity changes in horizontal and vertical directions.

2. **Laplacian of Gaussian (LoG)**: Detects edges by finding zero crossings after filtering with a Laplacian of Gaussian filter, which helps to reduce noise.

3. **Difference of Gaussians (DoG)**: Enhances edges by subtracting two differently blurred versions of the original image.

4. **Gabor Filter**: Detects edges at specific orientations and scales using sinusoidal wave modulated by a Gaussian envelope.

5. **Phase Congruency**: Identifies features where phase components are maximally in phase, providing illumination-invariant edge detection.

6. **Edge Flow**: Tracks edges along the gradient direction, resulting in better connected edge contours.

7. **Lindeberg Scale Space**: Multi-scale approach that detects edges at different levels of detail.

### Region Analysis

The `AutomaticRegionProcessor` divides the image into smaller regions (blocks) and analyzes each for:

- **Variance**: Determines the level of detail or texture
- **Edge Density**: Measures the concentration of edges
- **Gradient**: Calculates the average intensity change

Based on these characteristics, the processor selects the most appropriate algorithm combination:

- Smooth backgrounds: Simple Gaussian blur
- Highly textured areas (clothing, grass, hair): Sobel + DoG
- Curved/flowing edges (hair, natural objects): Edge Flow + Phase Congruency
- Default areas (skin, gentle edges): Phase Congruency

## Getting Started

### Prerequisites

- .NET 6.0 or higher
- Visual Studio 2019 or higher

### Installation

1. Clone the repository
   ```
   git clone https://github.com/yourusername/photo-to-sketch.git
   ```

2. Open the solution file in Visual Studio
   ```
   PhotoToSketch.sln
   ```

3. Build the solution
   ```
   Build > Build Solution
   ```

### Usage

1. Launch the application
2. Click "Load Image" to select an image file
3. Click "Process" to convert the photo to a sketch
4. Click "Save Sketch" to save the result

## Example Results

The application produces high-quality sketch-like renderings with the following characteristics:

- Clean, well-defined edges
- Preserved details in complex areas
- Natural-looking line work
- Reduced noise in smooth regions
- Good contrast between important and less important features

## Performance Considerations

- Processing large images may take time, especially when using advanced algorithms like Phase Congruency
- The automatic region processor balances quality and performance by applying heavier algorithms only where needed
- For faster processing, simpler algorithms like Sobel or Prewitt can be used

## Algorithm Selection Guide

For different types of images, consider these algorithm combinations:

- **Portraits**: DoG + Phase Congruency + Edge Flow
- **Landscapes**: Sobel + LoG + Phase Congruency
- **Architecture**: Prewitt + LoG + Phase Congruency
- **Detailed Textures**: Sobel + Kirsch + Scharr
- **Line Art**: DoG + Phase Congruency + Edge Flow + Gabor

## Extending the Application

The modular design makes it easy to add new algorithms:

1. Create a new processor class in the `EdgeDetectionLib.Algorithms` namespace
2. Implement the edge detection algorithm with an `Apply` method that accepts and returns a `GrayscaleImage`
3. Add corresponding methods in `ImageSketchProcessor` to expose the new algorithm

### How It Works

1. Converts the image to **grayscale**.
2. Divides the image into **regions**.
3. For each region:
    - Measures **variance** (texture complexity).
    - Measures **edge density** (how many edges exist).
    - Measures **average gradient strength**.
    - Chooses the best **single algorithm** or **combination of algorithms** for that region.
4. **Applies the selected algorithms.**
5. Reconstructs the **final combined image**.

## 📄 Future Enhancements

- Implement **overlapping block blending** to reduce hard edges between regions.
- Add optional **manual region labeling tools (for user-specified face/hair regions)**.
- Introduce **adaptive block sizing (quadtree-like division)** based on variance.

## License

[MIT License](LICENSE)

## Acknowledgments

- Various edge detection algorithms are implemented based on academic research in the field of computer vision and image processing.
