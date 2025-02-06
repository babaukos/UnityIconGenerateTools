using UnityEngine;
using UnityEditor;
using System.IO;

public class IconGenerator : EditorWindow
{
    // Object for rendering (prefab or GameObject)
    private Object objectToPreview;
    private Texture2D previewTexture;
    private string savePath = "Assets/PreviewIcon.png";
    private bool useObjectPath = false; // Checkbox: use the object's path

    [MenuItem("Tools/PreviewTools/Generate Preview From Icon")]
    private static void Init()
    {
        IconGenerator window = (IconGenerator)GetWindow(typeof(IconGenerator));
        window.minSize = window.maxSize = new Vector2(280, 320);
        window.titleContent = new GUIContent("Preview Icon Generator");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate PNG Preview (Icon) for Object", EditorStyles.boldLabel);

        // Select object for preview generation
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Object (Prefab)");
        Object newObject = EditorGUILayout.ObjectField(objectToPreview, typeof(Object), false);
        if(newObject != objectToPreview)
        {
            objectToPreview = newObject;
            // If the checkbox is enabled and the object has changed, automatically update the save path
            if(useObjectPath && objectToPreview != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(objectToPreview);
                if(!string.IsNullOrEmpty(assetPath))
                {
                    // Build path: take the object's folder and append the filename with _Icon.png extension
                    string directory = Path.GetDirectoryName(assetPath);
                    string fileName = Path.GetFileNameWithoutExtension(assetPath) + "_Icon.png";
                    savePath = Path.Combine(directory, fileName).Replace("\\", "/");
                }
            }
        }
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Generate Preview"))
        {
            GeneratePreview();
        }

        if (previewTexture != null)
        {
            GUILayout.Label("Preview:");
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.Label(previewTexture, GUILayout.Width(128), GUILayout.Height(128));
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        // Checkbox for using the object's path
        useObjectPath = EditorGUILayout.Toggle("Use Object's Path", useObjectPath);

        // If checkbox is enabled, update the path automatically (if an object is selected)
        if(useObjectPath && objectToPreview != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(objectToPreview);
            if(!string.IsNullOrEmpty(assetPath))
            {
                string directory = Path.GetDirectoryName(assetPath);
                string fileName = Path.GetFileNameWithoutExtension(assetPath) + "_Icon.png";
                savePath = Path.Combine(directory, fileName).Replace("\\", "/");
            }
        }

        // If checkbox is disabled, allow manual editing of the save path
        if (!useObjectPath)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Save Path", GUILayout.Width(100));
            GUILayout.BeginHorizontal();
        
            savePath = GUILayout.TextField(savePath);

            // Button with ellipsis for selecting a path
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string chosenPath = EditorUtility.SaveFilePanel("Select path for saving PNG", Application.dataPath, "PreviewIcon", "png");
                if (!string.IsNullOrEmpty(chosenPath))
                {
                    // Convert absolute path to relative if it is inside Assets
                    if (chosenPath.StartsWith(Application.dataPath))
                    {
                        savePath = "Assets" + chosenPath.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        savePath = chosenPath;
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        if (previewTexture != null && GUILayout.Button("Save PNG"))
        {
            SaveTextureAsPNG(previewTexture, savePath);
        }
    }

    private void GeneratePreview()
    {
        if (objectToPreview == null)
        {
            Debug.LogWarning("Please specify an object to generate the preview.");
            return;
        }

        // Attempt to get the preview as seen in the Project window
        previewTexture = AssetPreview.GetAssetPreview(objectToPreview);
        if (previewTexture == null)
        {
            previewTexture = AssetPreview.GetMiniThumbnail(objectToPreview);
        }

        if (previewTexture == null)
        {
            Debug.LogWarning("Failed to generate preview. Please try again.");
        }
        else
        {
            Repaint();
        }
    }

    private void SaveTextureAsPNG(Texture2D texture, string path)
    {
        if (texture == null)
        {
            Debug.LogWarning("No texture available for saving.");
            return;
        }

        byte[] pngData = texture.EncodeToPNG();
        if (pngData != null)
        {
            File.WriteAllBytes(path, pngData);
            Debug.Log("PNG saved at path: " + path);
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogWarning("Failed to encode texture to PNG.");
        }
    }
}
