using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class PeruMapGenerator
{
    [MenuItem("Food to Go/Generate Peru Map")]
    public static void Generate()
    {
        GameObject existing = GameObject.Find("PeruMap");
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("Peru Map Generator",
                "A PeruMap object already exists. Replace it?", "Replace", "Cancel"))
                return;
            Object.DestroyImmediate(existing);
        }

        GameObject root = new GameObject("PeruMap");

        GameObject terrain = BuildTerrain();
        terrain.transform.SetParent(root.transform, false);

        GameObject waypoints = BuildWaypoints();
        waypoints.transform.SetParent(root.transform, false);

        GameObject pins = BuildCityPins();
        pins.transform.SetParent(root.transform, false);

        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(root);
        Debug.Log("PeruMap generated successfully.");
    }

    static readonly Vector2[] PeruOutline = new Vector2[]
    {
        new Vector2(-1.8f,  4.5f),
        new Vector2(-1.2f,  5.0f),
        new Vector2( 0.0f,  5.2f),
        new Vector2( 1.0f,  5.0f),
        new Vector2( 2.0f,  4.8f),
        new Vector2( 3.2f,  5.5f),
        new Vector2( 4.0f,  5.2f),
        new Vector2( 4.5f,  4.5f),
        new Vector2( 4.2f,  3.5f),
        new Vector2( 4.8f,  2.8f),
        new Vector2( 5.0f,  1.5f),
        new Vector2( 4.5f,  0.5f),
        new Vector2( 4.0f, -0.5f),
        new Vector2( 3.5f, -1.5f),
        new Vector2( 3.0f, -2.5f),
        new Vector2( 2.5f, -3.5f),
        new Vector2( 2.0f, -4.5f),
        new Vector2( 1.5f, -5.5f),
        new Vector2( 1.0f, -6.5f),
        new Vector2( 0.5f, -7.5f),
        new Vector2( 0.0f, -8.5f),
        new Vector2(-0.3f, -9.0f),
        new Vector2(-0.8f, -8.0f),
        new Vector2(-1.2f, -7.0f),
        new Vector2(-1.5f, -6.0f),
        new Vector2(-2.0f, -5.0f),
        new Vector2(-2.5f, -4.0f),
        new Vector2(-3.0f, -3.0f),
        new Vector2(-3.5f, -2.0f),
        new Vector2(-4.0f, -1.0f),
        new Vector2(-4.2f,  0.0f),
        new Vector2(-4.0f,  1.0f),
        new Vector2(-3.8f,  2.0f),
        new Vector2(-3.5f,  3.0f),
        new Vector2(-3.0f,  3.8f),
        new Vector2(-2.5f,  4.2f),
        new Vector2(-1.8f,  4.5f),
    };

    static readonly (string name, Vector2 uv, Color color, float elevation)[] Cities = new[]
    {
        ("Lima",     new Vector2(-3.5f,  0.0f), new Color(0.95f, 0.85f, 0.65f), 0.05f),
        ("Trujillo", new Vector2(-3.2f,  2.5f), new Color(0.95f, 0.85f, 0.65f), 0.05f),
        ("Arequipa", new Vector2(-2.5f, -3.5f), new Color(0.85f, 0.65f, 0.45f), 1.2f),
        ("Cusco",    new Vector2(-0.5f, -2.5f), new Color(0.85f, 0.65f, 0.45f), 1.5f),
        ("Iquitos",  new Vector2( 3.0f,  3.0f), new Color(0.35f, 0.65f, 0.30f), 0.15f),
    };

    static Color GetZoneColor(Vector2 p)
    {
        if (p.x < -1.5f)
            return new Color(0.95f, 0.85f, 0.65f);
        if (p.x < 1.0f)
            return new Color(0.85f, 0.65f, 0.45f);
        return new Color(0.35f, 0.65f, 0.30f);
    }

    static float GetZoneElevation(Vector2 p)
    {
        if (p.x < -1.5f) return 0.05f;
        if (p.x < 1.0f)  return 1.2f;
        return 0.15f;
    }

    static GameObject BuildTerrain()
    {
        Vector2[] outline = PeruOutline;
        int n = outline.Length - 1;

        int topCount = n;
        int bottomCount = n;
        int sideVertCount = n * 4;
        int totalVerts = topCount + bottomCount + sideVertCount;

        Vector3[] verts = new Vector3[totalVerts];
        Color[] colors = new Color[totalVerts];
        Vector3[] normals = new Vector3[totalVerts];

        for (int i = 0; i < n; i++)
        {
            float elev = GetZoneElevation(outline[i]);
            verts[i] = new Vector3(outline[i].x, elev, outline[i].y);
            colors[i] = GetZoneColor(outline[i]);
            normals[i] = Vector3.up;
        }

        int botBase = n;
        for (int i = 0; i < n; i++)
        {
            verts[botBase + i] = new Vector3(outline[i].x, 0f, outline[i].y);
            colors[botBase + i] = new Color(0.55f, 0.45f, 0.35f);
            normals[botBase + i] = Vector3.down;
        }

        int sideBase = n * 2;
        for (int i = 0; i < n; i++)
        {
            int next = (i + 1) % n;
            float elev0 = GetZoneElevation(outline[i]);
            float elev1 = GetZoneElevation(outline[next]);

            Vector3 a = new Vector3(outline[i].x,    elev0, outline[i].y);
            Vector3 b = new Vector3(outline[next].x, elev1, outline[next].y);
            Vector3 c = new Vector3(outline[next].x, 0f,    outline[next].y);
            Vector3 d = new Vector3(outline[i].x,    0f,    outline[i].y);

            Vector3 edge = b - a;
            Vector3 sideNormal = Vector3.Cross(edge, Vector3.down).normalized;

            int s = sideBase + i * 4;
            verts[s + 0] = a; colors[s + 0] = new Color(0.7f, 0.6f, 0.5f); normals[s + 0] = sideNormal;
            verts[s + 1] = b; colors[s + 1] = new Color(0.7f, 0.6f, 0.5f); normals[s + 1] = sideNormal;
            verts[s + 2] = c; colors[s + 2] = new Color(0.55f, 0.45f, 0.35f); normals[s + 2] = sideNormal;
            verts[s + 3] = d; colors[s + 3] = new Color(0.55f, 0.45f, 0.35f); normals[s + 3] = sideNormal;
        }

        List<int> tris = new List<int>();

        int[] topTris = Triangulate(outline, n);
        tris.AddRange(topTris);

        for (int i = 0; i < topTris.Length; i += 3)
        {
            tris.Add(botBase + topTris[i + 2]);
            tris.Add(botBase + topTris[i + 1]);
            tris.Add(botBase + topTris[i + 0]);
        }

        for (int i = 0; i < n; i++)
        {
            int s = sideBase + i * 4;
            tris.Add(s); tris.Add(s + 1); tris.Add(s + 2);
            tris.Add(s); tris.Add(s + 2); tris.Add(s + 3);
        }

        Mesh mesh = new Mesh();
        mesh.name = "PeruTerrainMesh";
        mesh.vertices = verts;
        mesh.colors = colors;
        mesh.normals = normals;
        mesh.triangles = tris.ToArray();
        mesh.RecalculateBounds();

        AssetDatabase.CreateAsset(mesh, "Assets/_Assets/Meshes/Environment/PeruTerrainMesh.asset");

        GameObject go = new GameObject("Base_Terrain");
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mf.sharedMesh = mesh;

        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Assets/Materials/PeruMapMat.mat");
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = "PeruMapMat_Default";
        }
        mr.sharedMaterial = mat;

        return go;
    }

    static int[] Triangulate(Vector2[] pts, int count)
    {
        List<int> result = new List<int>();
        List<int> indices = new List<int>();
        for (int i = 0; i < count; i++) indices.Add(i);

        int safety = count * count;
        int iter = 0;

        while (indices.Count > 3 && iter++ < safety)
        {
            bool earFound = false;
            for (int i = 0; i < indices.Count; i++)
            {
                int prev = (i - 1 + indices.Count) % indices.Count;
                int next = (i + 1) % indices.Count;

                int ip = indices[prev];
                int ic = indices[i];
                int in_ = indices[next];

                Vector2 a = pts[ip];
                Vector2 b = pts[ic];
                Vector2 c = pts[in_];

                if (Cross2D(a, b, c) <= 0) continue;

                bool hasPoint = false;
                for (int j = 0; j < indices.Count; j++)
                {
                    if (j == prev || j == i || j == next) continue;
                    if (PointInTriangle(pts[indices[j]], a, b, c)) { hasPoint = true; break; }
                }

                if (!hasPoint)
                {
                    result.Add(ip); result.Add(ic); result.Add(in_);
                    indices.RemoveAt(i);
                    earFound = true;
                    break;
                }
            }
            if (!earFound) break;
        }

        if (indices.Count == 3)
        {
            result.Add(indices[0]); result.Add(indices[1]); result.Add(indices[2]);
        }

        return result.ToArray();
    }

    static float Cross2D(Vector2 o, Vector2 a, Vector2 b)
        => (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);

    static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Cross2D(p, a, b);
        float d2 = Cross2D(p, b, c);
        float d3 = Cross2D(p, c, a);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
    }

    static GameObject BuildWaypoints()
    {
        GameObject container = new GameObject("Waypoints");
        foreach (var (name, uv, color, elev) in Cities)
        {
            GameObject wp = new GameObject("Waypoint_" + name);
            wp.transform.SetParent(container.transform, false);
            wp.transform.localPosition = new Vector3(uv.x, elev + 0.05f, uv.y);
        }
        return container;
    }

    static GameObject BuildCityPins()
    {
        GameObject container = new GameObject("CityPins");
        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Assets/Materials/PeruMapMat.mat");

        foreach (var (name, uv, color, elev) in Cities)
        {
            GameObject pin = BuildPin(name, color, mat);
            pin.transform.SetParent(container.transform, false);
            pin.transform.localPosition = new Vector3(uv.x, elev, uv.y);
        }
        return container;
    }

    static GameObject BuildPin(string cityName, Color color, Material mat)
    {
        GameObject pin = new GameObject("Pin_" + cityName);

        Mesh mesh = new Mesh();
        mesh.name = "Pin_" + cityName + "_Mesh";

        float r = 0.12f;
        float h = 0.5f;
        int segs = 8;

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Color> colors = new List<Color>();
        List<Vector3> normals = new List<Vector3>();

        verts.Add(new Vector3(0, h, 0));
        colors.Add(color);
        normals.Add(Vector3.up);

        for (int i = 0; i < segs; i++)
        {
            float angle = i * Mathf.PI * 2f / segs;
            verts.Add(new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r));
            colors.Add(color);
            normals.Add(new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)));
        }

        for (int i = 0; i < segs; i++)
        {
            tris.Add(0);
            tris.Add(i + 1);
            tris.Add((i + 1) % segs + 1);
        }

        int baseCenter = verts.Count;
        verts.Add(new Vector3(0, 0, 0));
        colors.Add(color * 0.7f);
        normals.Add(Vector3.down);
        for (int i = 0; i < segs; i++)
        {
            float angle = i * Mathf.PI * 2f / segs;
            verts.Add(new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r));
            colors.Add(color * 0.7f);
            normals.Add(Vector3.down);
        }
        for (int i = 0; i < segs; i++)
        {
            tris.Add(baseCenter);
            tris.Add(baseCenter + (i + 1) % segs + 1);
            tris.Add(baseCenter + i + 1);
        }

        mesh.vertices = verts.ToArray();
        mesh.colors = colors.ToArray();
        mesh.normals = normals.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateBounds();

        AssetDatabase.CreateAsset(mesh, $"Assets/_Assets/Meshes/Environment/Pin_{cityName}_Mesh.asset");

        MeshFilter mf = pin.AddComponent<MeshFilter>();
        MeshRenderer mr = pin.AddComponent<MeshRenderer>();
        mf.sharedMesh = mesh;
        if (mat != null) mr.sharedMaterial = mat;

        return pin;
    }
}
