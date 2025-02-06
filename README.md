# Unity Preview Tools

This repository contains two custom Unity Editor tools to help you generate previews of your assets:

1. **Camera Preview Generator**  
   Generates a PNG image of a selected prefab or GameObject using a custom camera. It allows you to configure the camera’s field of view, distance, rotation, resolution, background color, and layer-based rendering options.

2. **Icon Generator**  
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

### Icon Generator

1. Place the `IconGenerator.cs` file in your Unity project's `Editor` folder.
2. In Unity, go to **Tools > PreviewTools > Generate Preview From Icon**.
3. In the window, select the object (prefab) for which you want to generate an icon.
4. Optionally, check **Use Object's Path** to automatically set the save path based on the asset location.
5. Click **Generate Preview** to display the generated preview in the window.
6. If you like the preview, click **Save PNG** to write the image to disk.

## Requirements

- Unity 2018.4 or later.
- The scripts must be placed inside a folder named `Editor` in your project.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
