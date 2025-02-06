# Creating item icons for your game

This repository contains two custom Unity Editor tools to help you generate previews of your assets / game:

1. **Camera Preview Generator**  
   Generates a PNG image of a selected prefab or GameObject using a custom camera. It allows you to configure the camera’s field of view, distance, rotation, resolution, background color, and layer-based rendering options.

2. **Icon Preview Generator**  
   Generates a PNG preview (icon) for a selected prefab or GameObject. You can choose to use the asset’s folder path for saving the icon automatically or manually specify a save path.

## Features

- **Customizable Camera Settings:**  
  Adjust the field of view, distance (or auto-calculate it), and rotation of the camera to capture the perfect angle.

- **Resolution Options:**  
  Set a fixed resolution or use proportional dimensions for the output image.

- **Background and Layer Control:**  
  Define the background color and clear flags, and optionally render only specific layers.

- **Easy Preview Generation:**  
  Generate quick previews of assets directly from the Unity Editor.

## How to Use

### Camera Preview Generator

1. Place the `CameraPreviewGenerator.cs` file in your Unity project's `Editor` folder.
2. In Unity, go to **Tools > PreviewTools > Generate Preview From Camera**.
3. In the window, select the object (prefab or GameObject) you wish to render.
4. Configure the camera settings, resolution, background, and layer options as needed.
5. Click **Generate Image**. The PNG will be saved at the specified output path.





