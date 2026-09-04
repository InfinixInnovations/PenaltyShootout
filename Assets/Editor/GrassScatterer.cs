using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GrassScatterer : EditorWindow
{
    [MenuItem("Tools/Generate Pitch Grass")]
    public static void ShowWindow()
    {
        GenerateGrass();
    }

    private static void GenerateGrass()
    {
        // 1. Find Pitch.001
        GameObject pitchObj = GameObject.Find("Pitch.001");
        if (pitchObj == null)
        {
            Debug.LogError("Pitch.001 not found!");
            return;
        }

        // Get MeshRenderer and MeshFilter
        MeshRenderer pitchRenderer = pitchObj.GetComponent<MeshRenderer>();
        MeshFilter pitchFilter = pitchObj.GetComponent<MeshFilter>();
        if (pitchRenderer == null || pitchFilter == null)
        {
            Debug.LogError("Pitch.001 is missing renderer or filter!");
            return;
        }

        // Get bounds
        Bounds bounds = pitchRenderer.bounds;
        
        // Remove existing grass if any
        GameObject existingGrass = GameObject.Find("BakedGrassObject");
        if (existingGrass != null)
        {
            DestroyImmediate(existingGrass);
        }

        // Create new grass object
        GameObject grassObj = new GameObject("BakedGrassObject");
        MeshFilter grassFilter = grassObj.AddComponent<MeshFilter>();
        MeshRenderer grassRenderer = grassObj.AddComponent<MeshRenderer>();

        // Set up material
        Material grassMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/StylizedGrassMaterial.mat");
        if (grassMat == null)
        {
            grassMat = new Material(Shader.Find("Custom/StylizedGrass"));
            grassMat.SetColor("_BaseColor", new Color(0.12f, 0.4f, 0.12f));
            grassMat.SetColor("_TipColor", new Color(0.28f, 0.65f, 0.22f));
            grassMat.SetFloat("_WindSpeed", 2.0f);
            grassMat.SetFloat("_WindStrength", 0.08f); // subtle sway
            grassMat.SetFloat("_WindFrequency", 1.0f);
            
            if (!System.IO.Directory.Exists("Assets/Materials")) {
                System.IO.Directory.CreateDirectory("Assets/Materials");
            }
            AssetDatabase.CreateAsset(grassMat, "Assets/Materials/StylizedGrassMaterial.mat");
        }
        
        grassRenderer.sharedMaterial = grassMat;

        // Generate grass mesh data
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        // We'll spawn a dense grid of grass clumps with random offsets
        int countX = 140; // Density along length
        int countZ = 100; // Density along width
        float spacingX = bounds.size.x / countX;
        float spacingZ = bounds.size.z / countZ;
        
        float startX = bounds.min.x;
        float startZ = bounds.min.z;
        float posY = bounds.center.y + 0.01f; // Pitch height slightly offset to prevent z-fighting

        int bladesPerClump = 4;
        float bladeWidth = 0.08f;
        float bladeHeight = 0.16f; // Short pitch-cut grass

        for (int i = 0; i < countX; i++)
        {
            for (int j = 0; j < countZ; j++)
            {
                // Calculate position with random jitter
                float jitterX = Random.Range(-spacingX * 0.45f, spacingX * 0.45f);
                float jitterZ = Random.Range(-spacingZ * 0.45f, spacingZ * 0.45f);
                Vector3 pos = new Vector3(startX + i * spacingX + jitterX, posY, startZ + j * spacingZ + jitterZ);

                // Add grass clump at pos
                for (int b = 0; b < bladesPerClump; b++)
                {
                    float angle = Random.Range(0f, 360f);
                    float scale = Random.Range(0.7f, 1.3f);
                    AddBlade(pos, angle, scale, bladeWidth, bladeHeight, vertices, triangles, normals);
                }
            }
        }

        // Create Mesh
        Mesh mesh = new Mesh();
        mesh.name = "BakedGrassMesh";
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Support > 65k vertices
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        
        mesh.RecalculateBounds();

        // Save mesh asset
        if (!System.IO.Directory.Exists("Assets/Meshes")) {
            System.IO.Directory.CreateDirectory("Assets/Meshes");
        }
        AssetDatabase.CreateAsset(mesh, "Assets/Meshes/BakedGrassMesh.asset");
        AssetDatabase.SaveAssets();

        grassFilter.sharedMesh = mesh;
        
        Debug.Log($"Baked {vertices.Count / 5} grass blades successfully!");
    }

    private static void AddBlade(Vector3 center, float angle, float scale, float width, float height, List<Vector3> verts, List<int> tris, List<Vector3> norms)
    {
        int baseIndex = verts.Count;

        // Pre-rotate vector helper
        Quaternion rot = Quaternion.Euler(0, angle, 0);

        // Blade vertices local coordinates (slightly bent forward in local Z)
        // 0, 1: Bottom (left, right)
        // 2, 3: Middle (left, right)
        // 4: Tip (center, bent slightly)
        float w = width * scale;
        float h = height * scale;

        Vector3 v0 = center + rot * new Vector3(-w * 0.5f, 0, 0);
        Vector3 v1 = center + rot * new Vector3(w * 0.5f, 0, 0);
        Vector3 v2 = center + rot * new Vector3(-w * 0.35f, h * 0.5f, 0.015f);
        Vector3 v3 = center + rot * new Vector3(w * 0.35f, h * 0.5f, 0.015f);
        Vector3 v4 = center + rot * new Vector3(0, h, 0.05f); // Bent forward at the tip

        verts.Add(v0);
        verts.Add(v1);
        verts.Add(v2);
        verts.Add(v3);
        verts.Add(v4);

        // Normals (pointing slightly up/out)
        Vector3 normal = rot * Vector3.up;
        norms.Add(normal);
        norms.Add(normal);
        norms.Add(normal);
        norms.Add(normal);
        norms.Add(normal);

        // Front Face (Double-sided rendering by duplicating geometry with reversed triangles)
        // Bottom quad: (0, 1, 3) and (0, 3, 2)
        tris.Add(baseIndex + 0);
        tris.Add(baseIndex + 3);
        tris.Add(baseIndex + 1);

        tris.Add(baseIndex + 0);
        tris.Add(baseIndex + 2);
        tris.Add(baseIndex + 3);

        // Top tri: (2, 3, 4)
        tris.Add(baseIndex + 2);
        tris.Add(baseIndex + 4);
        tris.Add(baseIndex + 3);

        // Back Face (Reversed index order)
        tris.Add(baseIndex + 0);
        tris.Add(baseIndex + 1);
        tris.Add(baseIndex + 3);

        tris.Add(baseIndex + 0);
        tris.Add(baseIndex + 3);
        tris.Add(baseIndex + 2);

        tris.Add(baseIndex + 2);
        tris.Add(baseIndex + 3);
        tris.Add(baseIndex + 4);
    }
}
