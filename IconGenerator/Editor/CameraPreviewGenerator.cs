using UnityEngine;
using UnityEditor;
using System.IO;

public class CameraPreviewGenerator : EditorWindow
{
    // Object for rendering (prefab or GameObject)
    private Object targetObject;

    // Camera settings
    [Header("Camera Settings")]
    [Tooltip("Camera field of view")]
    private float fieldOfView = 60f;
    [Tooltip("Camera distance (used if Auto Distance is disabled)")]
    private float cameraDistance = 5f;
    [Tooltip("Automatically calculate distance to the object")]
    private bool autoCalculateDistance = true;
    [Tooltip("Camera rotation offset (Euler angles) relative to the object")]
    private Vector3 cameraRotationOffset = new Vector3(0f, 0f, 0f);

    private bool proportionalSize = true;
    // Final image resolution
    [Header("Resolution")]
    private int resolutionWidth = 512;
    private int resolutionHeight = 512;

    // Background settings
    [Header("Background Settings")]
    [Tooltip("Background color")]
    private Color backgroundColor = new Color(0, 0, 0, 0);  // transparent background
    [Tooltip("Camera clear flags")]
    private CameraClearFlags clearFlags = CameraClearFlags.SolidColor;

    // Layer-based rendering settings
    [Header("Layer-based Rendering")]
    [Tooltip("Render only the specified layer")]
    private bool useLayerFiltering = false;
    [Tooltip("Camera layer (cullingMask)")]
    private int renderLayer = 0;
    [Tooltip("Layer assigned to the instantiated model")]
    private int modelLayer = 0;

    // PNG output path (relative to Assets or absolute)
    [Header("Output Path")]
    private string outputPath = "Assets/CameraPreview.png";

    [MenuItem("Tools/PreviewTools/Generate Preview From Camera")]
    private static void Init()
    {
        CameraPreviewGenerator window = (CameraPreviewGenerator)GetWindow(typeof(CameraPreviewGenerator));
        window.minSize = window.maxSize = new Vector2(280, 320);
        window.titleContent = new GUIContent("Camera Preview Generator");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate Image from Camera", EditorStyles.boldLabel);

        // Object for rendering
        targetObject = EditorGUILayout.ObjectField("Object (Prefab)", targetObject, typeof(Object), false);

        // Camera settings
        fieldOfView = EditorGUILayout.Slider("Field of View", fieldOfView, 1f, 120f);
        autoCalculateDistance = EditorGUILayout.Toggle("Auto Distance", autoCalculateDistance);
        if (!autoCalculateDistance)
        {
            cameraDistance = EditorGUILayout.FloatField("Camera Distance", cameraDistance);
        }
        cameraRotationOffset = EditorGUILayout.Vector3Field("Camera Angle (Euler)", cameraRotationOffset);

        // Resolution settings
        proportionalSize = EditorGUILayout.Toggle("Proportional Image Size", proportionalSize);
        if (!proportionalSize)
        {
            resolutionWidth = EditorGUILayout.IntField("Width (px)", resolutionWidth);
            resolutionHeight = EditorGUILayout.IntField("Height (px)", resolutionHeight);
        }
        else
        {
            resolutionWidth = resolutionHeight = EditorGUILayout.IntField("Size (Width and Height)", resolutionWidth);
        }

        // Background settings
        backgroundColor = EditorGUILayout.ColorField("Background Color", backgroundColor);
        clearFlags = (CameraClearFlags)EditorGUILayout.EnumPopup("Clear Flags", clearFlags);

        // Layer-based rendering settings
        useLayerFiltering = EditorGUILayout.Toggle("Render Only Specified Layer", useLayerFiltering);
        if (useLayerFiltering)
        {
            renderLayer = EditorGUILayout.LayerField("Camera Layer", renderLayer);
            modelLayer = EditorGUILayout.LayerField("Model Layer", modelLayer);
        }

        // PNG output path
        GUILayout.BeginVertical();
        EditorGUILayout.LabelField("Output Path");
        GUILayout.BeginHorizontal();
        outputPath = EditorGUILayout.TextField("", outputPath);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            string chosenPath = EditorUtility.SaveFilePanel("Select path for saving PNG", Application.dataPath, "CameraPreview", "png");
            if (!string.IsNullOrEmpty(chosenPath))
            {
                // If the selected path is inside Assets, convert it to a relative path.
                if (chosenPath.StartsWith(Application.dataPath))
                {
                    outputPath = "Assets" + chosenPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    outputPath = chosenPath;
                }
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        if (GUILayout.Button("Generate Image"))
        {
            GeneratePreviewImage();
        }
    }

    private void GeneratePreviewImage()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("Please specify an object to generate the image.");
            return;
        }

        // Load the prefab using AssetDatabase
        string assetPath = AssetDatabase.GetAssetPath(targetObject);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null)
        {
            Debug.LogError("The selected object is not a valid prefab or GameObject.");
            return;
        }
        
        // Instantiate the object (hidden in the scene)
        GameObject instance = Instantiate(prefab);
        instance.hideFlags = HideFlags.HideAndDontSave;
        
        // If layer-based rendering is enabled, assign the specified layer to the model
        if (useLayerFiltering)
        {
            SetLayerRecursively(instance, modelLayer);
        }
        
        // Calculate the bounds of the instance (combining all Renderers)
        Bounds bounds = new Bounds(instance.transform.position, Vector3.zero);
        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            foreach (Renderer rend in renderers)
            {
                bounds.Encapsulate(rend.bounds);
            }
        }
        else
        {
            bounds = new Bounds(instance.transform.position, Vector3.one);
        }

        // If auto distance is enabled, calculate the distance
        if (autoCalculateDistance)
        {
            float radius = bounds.extents.magnitude;
            // Formula: distance = radius / sin(FOV/2)
            cameraDistance = radius / Mathf.Sin(fieldOfView * 0.5f * Mathf.Deg2Rad);
        }

        // Create a temporary camera
        GameObject camGO = new GameObject("TempPreviewCamera");
        camGO.hideFlags = HideFlags.HideAndDontSave;
        Camera cam = camGO.AddComponent<Camera>();
        cam.backgroundColor = backgroundColor;
        cam.clearFlags = clearFlags;
        cam.fieldOfView = fieldOfView;
        cam.orthographic = false;
        cam.enabled = false;
        
        // If layer-based rendering is enabled, set the cullingMask to the specified layer only
        if (useLayerFiltering)
        {
            cam.cullingMask = 1 << renderLayer;
        }
        else
        {
            cam.cullingMask = ~0; // everything
        }

        // Calculate camera direction with the given rotation offset
        Vector3 baseDirection = new Vector3(0, 0, -1);
        Vector3 camDirection = Quaternion.Euler(cameraRotationOffset) * baseDirection;
        cam.transform.position = bounds.center - camDirection * cameraDistance;
        cam.transform.LookAt(bounds.center);

        // Create a RenderTexture
        RenderTexture rt = new RenderTexture(resolutionWidth, resolutionHeight, 24);
        cam.targetTexture = rt;
        cam.Render();

        // Read the image from the RenderTexture
        RenderTexture.active = rt;
        Texture2D image = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.ARGB32, false);
        image.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), 0, 0);
        image.Apply();

        // Clean up RenderTexture
        cam.targetTexture = null;
        RenderTexture.active = null;

        // Save the image as PNG
        byte[] pngData = image.EncodeToPNG();
        if (pngData != null)
        {
            File.WriteAllBytes(outputPath, pngData);
            Debug.Log("PNG saved at path: " + outputPath);
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogWarning("Failed to encode image to PNG.");
        }

        // Destroy temporary objects
        DestroyImmediate(image);
        DestroyImmediate(rt);
        DestroyImmediate(camGO);
        DestroyImmediate(instance);
    }

    /// <summary>
    /// Recursively sets the layer for an object and all its children.
    /// </summary>
    private static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
