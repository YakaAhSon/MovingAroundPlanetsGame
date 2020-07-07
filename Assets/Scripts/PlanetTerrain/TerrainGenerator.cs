using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {
    public Mesh[] m_IcoSphereMesh;

    public PlanetMaterial m_PlanetMaterial;

    public enum PlanetType {
        earth,// water-sand-dirt/grass-stone, polar:snow
        moon,// only dirt/sand
        mars,// very little water - mainly scortch, some dirt, polar snow
        ocean,// mainly water, some sand-grass
        sand,// sand only
        bumpy_stone,// mercury
        snow,
        spring_grass, // some water - grass/dirt- some stone
    }

    // Start is called before the first frame update
    void Start() {
        //Mesh mesh = GenerateRandomPlanet(2, 300, PlanetType.mars);
        //GetComponent<MeshFilter>().mesh = mesh;
        //GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().mesh;
    }

    // Update is called once per frame
    void Update() {

    }

    public Mesh GenerateRandomPlanet(int level, int seed, PlanetType t, out PlanetMaterial.Material[] material_map) {
        if (t == PlanetType.earth) {
            return GenerateRandomPlanet_earth(level, seed,out material_map);
        }
        else if (t == PlanetType.moon) {
            return GenerateRandomPlanet_moon(level, seed, out material_map);
        }
        else if (t == PlanetType.mars) {
            return GenerateRandomPlanet_mars(level, seed, out material_map);
        }
        else if (t == PlanetType.ocean) {
            return GenerateRandomPlanet_ocean(level, seed, out material_map);
        }
        else if (t == PlanetType.sand) {
            return GenerateRandomPlanet_sand(level, seed, out material_map);
        }
        else if (t == PlanetType.snow) {
            return GenerateRandomPlanet_snow(level, seed, out material_map);
        }
        else if (t == PlanetType.bumpy_stone) {
            return GenerateRandomPlanet_bumpyStone(level, seed, out material_map);
        }
        else {
            throw new System.Exception("Invalid planet type");
        }
    }

    // random origional height map
    public Mesh GenerateRandomPlanet_earth(int level, int seed, out PlanetMaterial.Material[] material_map) {
        float radius = Mathf.Pow(2, level + 3);
        int[] original_triangles = m_IcoSphereMesh[level].triangles;
        Vector3[] original_vetices = m_IcoSphereMesh[level].vertices;
        Mesh mesh = new Mesh();

        int n_vertices = original_triangles.Length;
        int n_triangles = n_vertices / 3;

        // random vertices
        Vector3[] vertices = new Vector3[n_vertices];
        for (int i = 0; i < n_vertices; ++i) {
            vertices[i] = original_vetices[original_triangles[i]];
        }

        System.Random random = new System.Random(seed);

        SimplexNoise.Noise n1 = new SimplexNoise.Noise();
        n1.Seed = random.Next();
        SimplexNoise.Noise n2 = new SimplexNoise.Noise();
        n2.Seed = random.Next();
        SimplexNoise.Noise n3 = new SimplexNoise.Noise();
        n3.Seed = random.Next();

        float[] height_map = new float[n_vertices];

        //SimplexNoise.Noise.Seed = seed;
        for (int i = 0; i < n_vertices; i++) {
            int l = level;
            l = Mathf.Max(0, l);
            Vector3 v1 = (vertices[i] * 0.5f + new Vector3(0.5f, 0.5f, 0.5f)) * radius * 0.125f;
            Vector3 v2 = v1 * 0.5f;
            Vector3 v3 = v2 * 0.5f;
            height_map[i] = n1.Generate(v1.x, v1.y, v1.z) * 0.25f + n2.Generate(v2.x, v2.y, v2.z) * 0.5f + n3.Generate(v3.x, v3.y, v3.z);
            height_map[i] /= 1.75f;
        }
        // generate material map

        material_map = new PlanetMaterial.Material[n_triangles];
        SimplexNoise.Noise noise_grass = new SimplexNoise.Noise();
        noise_grass.Seed = random.Next();
        for (int tid = 0; tid < n_triangles; ++tid) {
            Vector3 center = (vertices[tid * 3] + vertices[tid * 3 + 1] + vertices[tid * 3 + 2]) / 3;
            float height = (height_map[tid * 3] + height_map[tid * 3 + 1] + height_map[tid * 3 + 2]) / 3;
            // water
            if (height < -0.15f) {
                material_map[tid] = PlanetMaterial.Material.water;
            }
            // sand
            else if (height < 0f) {
                material_map[tid] = PlanetMaterial.Material.sand;
            }
            // grass/dirt
            else if (height < 0.5f) {
                Vector3 v = (center * 0.5f + new Vector3(0.5f, 0.5f, 0.5f)) * 4;
                float grass_value = noise_grass.Generate(v.x, v.y, v.z);
                if (grass_value > 0) {
                    material_map[tid] = PlanetMaterial.Material.grass;
                }
                else {
                    material_map[tid] = PlanetMaterial.Material.dirt;
                }
            }
            // stone
            else {
                material_map[tid] = PlanetMaterial.Material.stone;
            }
            // override polar
            if (center.y > 0.9f || center.y < -0.9f) {
                material_map[tid] = PlanetMaterial.Material.snow;
            }
        }

        Color[] color_map = new Color[n_vertices];
        // write colors
        for (int i = 0; i < n_vertices; ++i) {
            color_map[i] = m_PlanetMaterial.GetMaterialColor(material_map[i / 3]);
        }

        // write new vertices
        for (int i = 0; i < n_vertices; i++) {
            float h = height_map[i];
            h = Mathf.Max(h, 0);
            h = h * h * radius * 0.125f;
            vertices[i] = vertices[i] * (h + radius);
        }

        int[] triangles = new int[n_vertices];
        for (int i = 0; i < n_vertices; ++i) {
            triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.colors = color_map;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        mesh.RecalculateBounds();
        return mesh;
    }


    public Mesh GenerateRandomPlanet_moon(int level, int seed, out PlanetMaterial.Material[] material_map) {
        float radius = Mathf.Pow(2, level + 3);
        int[] original_triangles = m_IcoSphereMesh[level].triangles;
        Vector3[] original_vetices = m_IcoSphereMesh[level].vertices;
        Mesh mesh = new Mesh();

        int n_vertices = original_triangles.Length;
        int n_triangles = n_vertices / 3;

        // copy vertices
        Vector3[] vertices = new Vector3[n_vertices];
        for (int i = 0; i < n_vertices; ++i) {
            vertices[i] = original_vetices[original_triangles[i]];
        }

        System.Random random = new System.Random(seed);

        SimplexNoise.Noise n1 = new SimplexNoise.Noise();
        n1.Seed = random.Next();
        SimplexNoise.Noise n2 = new SimplexNoise.Noise();
        n2.Seed = random.Next();
        SimplexNoise.Noise n3 = new SimplexNoise.Noise();
        n3.Seed = random.Next();

        float[] height_map = new float[n_vertices];

        //SimplexNoise.Noise.Seed = seed;
        for (int i = 0; i < n_vertices; i++) {
            int l = level;
            l = Mathf.Max(0, l);
            Vector3 v1 = (vertices[i] * 0.5f + new Vector3(0.5f, 0.5f, 0.5f)) * radius * 0.125f;
            Vector3 v2 = v1 * 0.5f;
            Vector3 v3 = v2 * 0.5f;
            height_map[i] = n1.Generate(v1.x, v1.y, v1.z) * 0.25f + n2.Generate(v2.x, v2.y, v2.z) * 0.5f + n3.Generate(v3.x, v3.y, v3.z);
            height_map[i] /= 1.75f;
        }
        // generate material map

        material_map = new PlanetMaterial.Material[n_triangles];
        SimplexNoise.Noise noise_sand = new SimplexNoise.Noise();
        noise_sand.Seed = random.Next();
        for (int tid = 0; tid < n_triangles; ++tid) {
            Vector3 center = (vertices[tid * 3] + vertices[tid * 3 + 1] + vertices[tid * 3 + 2]) / 3;
            float height = (height_map[tid * 3] + height_map[tid * 3 + 1] + height_map[tid * 3 + 2]) / 3;
            // dirt/sand
            if (height < 0.5f) {
                Vector3 v = (center * 0.5f + new Vector3(0.5f, 0.5f, 0.5f)) * 4;
                float sand_value = noise_sand.Generate(v.x, v.y, v.z);
                if (sand_value > 0.3f) {
                    material_map[tid] = PlanetMaterial.Material.sand;
                }
                else {
                    material_map[tid] = PlanetMaterial.Material.dirt;
                }
            }
            // stone
            else {
                material_map[tid] = PlanetMaterial.Material.stone;
            }
        }

        Color[] color_map = new Color[n_vertices];
        // write colors
        for (int i = 0; i < n_vertices; ++i) {
            color_map[i] = m_PlanetMaterial.GetMaterialColor(material_map[i / 3]);
        }

        // write new vertices
        for (int i = 0; i < n_vertices; i++) {
            float h = height_map[i];
            h = Mathf.Max(h, 0);
            h = h * h * radius * 0.125f;
            vertices[i] = vertices[i] * (h + radius);
        }

        int[] triangles = new int[n_vertices];
        for (int i = 0; i < n_vertices; ++i) {
            triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.colors = color_map;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        mesh.RecalculateBounds();
        return mesh;
    }


    public Mesh GenerateRandomPlanet_mars(int level, int seed, out PlanetMaterial.Material[] material_map) {
        float radius = Mathf.Pow(2, level + 3);
        int[] original_triangles = m_IcoSphereMesh[level].triangles;
        Vector3[] original_vetices = m_IcoSphereMesh[level].vertices;
        Mesh mesh = new Mesh();

        int n_vertices = original_triangles.Length;
        int n_triangles = n_vertices / 3;

        // copy vertices
        Vector3[] vertices = new Vector3[n_vertices];
        for (int i = 0; i < n_vertices; ++i) {
            vertices[i] = original_vetices[original_triangles[i]];
        }

        System.Random random = new System.Random(seed);

        SimplexNoise.Noise n1 = new SimplexNoise.Noise();
        n1.Seed = random.Next();
        SimplexNoise.Noise n2 = new SimplexNoise.Noise();
        n2.Seed = random.Next();
        SimplexNoise.Noise n3 = new SimplexNoise.Noise();
        n3.Seed = random.Next();

        float[] height_map = new float[n_vertices];

        //SimplexNoise.Noise.Seed = seed;
        for (int i = 0; i < n_vertices; i++) {
            int l = level;
            l = Mathf.Max(0, l);
            Vector3 v1 = (vertices[i] * 0.5f + new Vector3(0.5f, 0.5f, 0.5f)) * radius * 0.125f;
            Vector3 v2 = v1 * 0.5f;
            Vector3 v3 = v2 * 0.5f;
            height_map[i] = n1.Generate(v1.x, v1.y, v1.z) * 0.25f + n2.Generate(v2.x, v2.y, v2.z) * 0.5f + n3.Generate(v3.x, v3.y, v3.z);
            height_map[i] /= 1.75f;
        }
        // generate material map

        material_map = new PlanetMaterial.Material[n_triangles];
        SimplexNoise.Noise noise_sand = new SimplexNoise.Noise();
        noise_sand.Seed = random.Next();
        for (int tid = 0; tid < n_triangles; ++tid) {
            Vector3 center = (vertices[tid * 3] + vertices[tid * 3 + 1] + vertices[tid * 3 + 2]) / 3;
            float height = (height_map[tid * 3] + height_map[tid * 3 + 1] + height_map[tid * 3 + 2]) / 3;
            // water
            if (height < -0.5) {
                material_map[tid] = PlanetMaterial.Material.water;
            }
            //scortch / dirt
            if (height < 0.5f) {
                Vector3 v = (center * 0.5f + new Vector3(0.5f, 0.5f, 0.5f)) * 4;
                float sand_value = noise_sand.Generate(v.x, v.y, v.z);
                if (sand_value < 0.5f) {
                    material_map[tid] = PlanetMaterial.Material.scortch;
                }
                else {
                    material_map[tid] = PlanetMaterial.Material.dirt;
                }
            }
            // stone
            else {
                material_map[tid] = PlanetMaterial.Material.stone;
            }
            if (center.y > 0.95 || center.y < -0.95) {
                material_map[tid] = PlanetMaterial.Material.snow;
            }
        }

        Color[] color_map = new Color[n_vertices];
        // write colors
        for (int i = 0; i < n_vertices; ++i) {
            color_map[i] = m_PlanetMaterial.GetMaterialColor(material_map[i / 3]);
        }

        // write new vertices
        for (int i = 0; i < n_vertices; i++) {
            float h = height_map[i];
            h = Mathf.Max(h, 0);
            h = h * h * radius * 0.125f;
            vertices[i] = vertices[i] * (h + radius);
        }

        int[] triangles = new int[n_vertices];
        for (int i = 0; i < n_vertices; ++i) {
            triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.colors = color_map;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        mesh.RecalculateBounds();
        return mesh;
    }

    // random origional height map
    public Mesh GenerateRandomPlanet_ocean(int level, int seed, out PlanetMaterial.Material[] material_map) {
        float radius = Mathf.Pow(2, level + 3);
        int[] original_triangles = m_IcoSphereMesh[level].triangles;
        Vector3[] original_vetices = m_IcoSphereMesh[level].vertices;
        Mesh mesh = new Mesh();

        int n_vertices = original_triangles.Length;
        int n_triangles = n_vertices / 3;

        // random vertices
        Vector3[] vertices = new Vector3[n_vertices];
        for (int i = 0; i < n_vertices; ++i) {
            vertices[i] = original_vetices[original_triangles[i]];
        }

        System.Random random = new System.Random(seed);

        SimplexNoise.Noise n1 = new SimplexNoise.Noise();
        n1.Seed = random.Next();
        SimplexNoise.Noise n2 = new SimplexNoise.Noise();
        n2.Seed = random.Next();
        SimplexNoise.Noise n3 = new SimplexNoise.Noise();
        n3.Seed = random.Next();

        float[] height_map = new float[n_vertices];

        //SimplexNoise.Noise.Seed = seed;
        for (int i = 0; i < n_vertices; i++) {
            int l = level;
            l = Mathf.Max(0, l);
            Vector3 v1 = (vertices[i] * 0.5f + new Vector3(0.5f, 0.5f, 0.5f)) * radius * 0.125f;
            Vector3 v2 = v1 * 0.5f;
            Vector3 v3 = v2 * 0.5f;
            height_map[i] = n1.Generate(v1.x, v1.y, v1.z) * 0.25f + n2.Generate(v2.x, v2.y, v2.z) * 0.5f + n3.Generate(v3.x, v3.y, v3.z);
            height_map[i] /= 1.75f;
            height_map[i] -= 0.5f;
        }
        // generate material map

        material_map = new PlanetMaterial.Material[n_triangles];
        SimplexNoise.Noise noise_grass = new SimplexNoise.Noise();
        noise_grass.Seed = random.Next();
        for (int tid = 0; tid < n_triangles; ++tid) {
            Vector3 center = (vertices[tid * 3] + vertices[tid * 3 + 1] + vertices[tid * 3 + 2]) / 3;
            float height = (height_map[tid * 3] + height_map[tid * 3 + 1] + height_map[tid * 3 + 2]) / 3;
            // water
            if (height < 0.0f) {
                material_map[tid] = PlanetMaterial.Material.water;
            }
            // sand
            else {
                material_map[tid] = PlanetMaterial.Material.sand;
            }
            // override polar
            if (center.y > 0.9f || center.y < -0.9f) {
                material_map[tid] = PlanetMaterial.Material.snow;
            }
        }

        Color[] color_map = new Color[n_vertices];
        // write colors
        for (int i = 0; i < n_vertices; ++i) {
            color_map[i] = m_PlanetMaterial.GetMaterialColor(material_map[i / 3]);
        }

        // write new vertices
        for (int i = 0; i < n_vertices; i++) {
            float h = height_map[i];
            h = Mathf.Max(h, 0);
            h = h * h * radius * 0.125f;
            vertices[i] = vertices[i] * (h + radius);
        }

        int[] triangles = new int[n_vertices];
        for (int i = 0; i < n_vertices; ++i) {
            triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.colors = color_map;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        mesh.RecalculateBounds();
        return mesh;
    }

    // random origional height map
    public Mesh GenerateRandomPlanet_sand(int level, int seed, out PlanetMaterial.Material[] material_map) {
        float radius = Mathf.Pow(2, level + 3);
        int[] original_triangles = m_IcoSphereMesh[level].triangles;
        Vector3[] original_vetices = m_IcoSphereMesh[level].vertices;
        Mesh mesh = new Mesh();

        int n_vertices = original_triangles.Length;
        int n_triangles = n_vertices / 3;

        // random vertices
        Vector3[] vertices = new Vector3[n_vertices];
        for (int i = 0; i < n_vertices; ++i) {
            vertices[i] = original_vetices[original_triangles[i]];
        }

        System.Random random = new System.Random(seed);

        SimplexNoise.Noise n1 = new SimplexNoise.Noise();
        n1.Seed = random.Next();
        SimplexNoise.Noise n2 = new SimplexNoise.Noise();
        n2.Seed = random.Next();
        SimplexNoise.Noise n3 = new SimplexNoise.Noise();
        n3.Seed = random.Next();

        float[] height_map = new float[n_vertices];

        //SimplexNoise.Noise.Seed = seed;
        for (int i = 0; i < n_vertices; i++) {
            int l = level;
            l = Mathf.Max(0, l);
            Vector3 v1 = (vertices[i] * 0.5f + new Vector3(0.5f, 0.5f, 0.5f)) * radius * 0.125f;
            Vector3 v2 = v1 * 0.5f;
            Vector3 v3 = v2 * 0.5f;
            height_map[i] = n1.Generate(v1.x, v1.y, v1.z) * 0.25f + n2.Generate(v2.x, v2.y, v2.z) * 0.5f + n3.Generate(v3.x, v3.y, v3.z);
            height_map[i] /= 1.75f;
            height_map[i] -= 0.5f;
        }
        // generate material map

        material_map = new PlanetMaterial.Material[n_triangles];
        SimplexNoise.Noise noise_grass = new SimplexNoise.Noise();
        noise_grass.Seed = random.Next();
        for (int tid = 0; tid < n_triangles; ++tid) {
            Vector3 center = (vertices[tid * 3] + vertices[tid * 3 + 1] + vertices[tid * 3 + 2]) / 3;
            float height = (height_map[tid * 3] + height_map[tid * 3 + 1] + height_map[tid * 3 + 2]) / 3;
            // water
            if (height < -1f) {
                material_map[tid] = PlanetMaterial.Material.water;
            }
            // sand
            else {
                material_map[tid] = PlanetMaterial.Material.sand;
            }
            // override polar
            if (center.y > 0.9f || center.y < -0.9f) {
                material_map[tid] = PlanetMaterial.Material.snow;
            }
        }

        Color[] color_map = new Color[n_vertices];
        // write colors
        for (int i = 0; i < n_vertices; ++i) {
            color_map[i] = m_PlanetMaterial.GetMaterialColor(material_map[i / 3]);
        }

        // write new vertices
        for (int i = 0; i < n_vertices; i++) {
            float h = height_map[i];
            h = Mathf.Max(h, 0);
            h = h * h * radius * 0.125f;
            vertices[i] = vertices[i] * (h + radius);
        }

        int[] triangles = new int[n_vertices];
        for (int i = 0; i < n_vertices; ++i) {
            triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.colors = color_map;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        mesh.RecalculateBounds();
        return mesh;
    }

    // random origional height map
    public Mesh GenerateRandomPlanet_snow(int level, int seed, out PlanetMaterial.Material[] material_map) {
        float radius = Mathf.Pow(2, level + 3);
        int[] original_triangles = m_IcoSphereMesh[level].triangles;
        Vector3[] original_vetices = m_IcoSphereMesh[level].vertices;
        Mesh mesh = new Mesh();

        int n_vertices = original_triangles.Length;
        int n_triangles = n_vertices / 3;

        // random vertices
        Vector3[] vertices = new Vector3[n_vertices];
        for (int i = 0; i < n_vertices; ++i) {
            vertices[i] = original_vetices[original_triangles[i]];
        }

        System.Random random = new System.Random(seed);

        SimplexNoise.Noise n1 = new SimplexNoise.Noise();
        n1.Seed = random.Next();
        SimplexNoise.Noise n2 = new SimplexNoise.Noise();
        n2.Seed = random.Next();
        SimplexNoise.Noise n3 = new SimplexNoise.Noise();
        n3.Seed = random.Next();

        float[] height_map = new float[n_vertices];

        //SimplexNoise.Noise.Seed = seed;
        for (int i = 0; i < n_vertices; i++) {
            int l = level;
            l = Mathf.Max(0, l);
            Vector3 v1 = (vertices[i] * 0.5f + new Vector3(0.5f, 0.5f, 0.5f)) * radius * 0.125f;
            Vector3 v2 = v1 * 0.5f;
            Vector3 v3 = v2 * 0.5f;
            height_map[i] = n1.Generate(v1.x, v1.y, v1.z) * 0.25f + n2.Generate(v2.x, v2.y, v2.z) * 0.5f + n3.Generate(v3.x, v3.y, v3.z);
            height_map[i] /= 1.75f;
            height_map[i] -= 0.7f;
        }
        // generate material map

        material_map = new PlanetMaterial.Material[n_triangles];
        SimplexNoise.Noise noise_grass = new SimplexNoise.Noise();
        noise_grass.Seed = random.Next();
        for (int tid = 0; tid < n_triangles; ++tid) {

            material_map[tid] = PlanetMaterial.Material.snow;
        }

        Color[] color_map = new Color[n_vertices];
        // write colors
        for (int i = 0; i < n_vertices; ++i) {
            color_map[i] = m_PlanetMaterial.GetMaterialColor(material_map[i / 3]);
        }

        // write new vertices
        for (int i = 0; i < n_vertices; i++) {
            float h = height_map[i];
            h = Mathf.Max(h, 0);
            h = h * h * radius * 0.125f;
            vertices[i] = vertices[i] * (h + radius);
        }

        int[] triangles = new int[n_vertices];
        for (int i = 0; i < n_vertices; ++i) {
            triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.colors = color_map;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        mesh.RecalculateBounds();
        return mesh;
    }

    // random origional height map
    public Mesh GenerateRandomPlanet_bumpyStone(int level, int seed, out PlanetMaterial.Material[] material_map) {
        float radius = Mathf.Pow(2, level + 3);
        int[] original_triangles = m_IcoSphereMesh[level].triangles;
        Vector3[] original_vetices = m_IcoSphereMesh[level].vertices;
        Mesh mesh = new Mesh();

        int n_vertices = original_triangles.Length;
        int n_triangles = n_vertices / 3;

        // random vertices
        Vector3[] vertices = new Vector3[n_vertices];
        for (int i = 0; i < n_vertices; ++i) {
            vertices[i] = original_vetices[original_triangles[i]];
        }

        System.Random random = new System.Random(seed);

        SimplexNoise.Noise n1 = new SimplexNoise.Noise();
        n1.Seed = random.Next();
        SimplexNoise.Noise n2 = new SimplexNoise.Noise();
        n2.Seed = random.Next();
        SimplexNoise.Noise n3 = new SimplexNoise.Noise();
        n3.Seed = random.Next();

        float[] height_map = new float[n_vertices];

        //SimplexNoise.Noise.Seed = seed;
        for (int i = 0; i < n_vertices; i++) {
            int l = level;
            l = Mathf.Max(0, l);
            Vector3 v1 = (vertices[i] * 0.5f + new Vector3(0.5f, 0.5f, 0.5f)) * radius * 0.125f;
            Vector3 v2 = v1 * 0.5f;
            Vector3 v3 = v2 * 0.5f;
            height_map[i] = n1.Generate(v1.x, v1.y, v1.z) * 0.25f + n2.Generate(v2.x, v2.y, v2.z) * 0.5f + n3.Generate(v3.x, v3.y, v3.z);
        }
        // generate material map

        material_map = new PlanetMaterial.Material[n_triangles];
        SimplexNoise.Noise noise_grass = new SimplexNoise.Noise();
        noise_grass.Seed = random.Next();
        for (int tid = 0; tid < n_triangles; ++tid) {

            material_map[tid] = PlanetMaterial.Material.stone;
        }

        Color[] color_map = new Color[n_vertices];
        // write colors
        for (int i = 0; i < n_vertices; ++i) {
            color_map[i] = m_PlanetMaterial.GetMaterialColor(material_map[i / 3]);
        }

        // write new vertices
        for (int i = 0; i < n_vertices; i++) {
            float h = height_map[i];
            h = h * h * radius * 0.125f;
            vertices[i] = vertices[i] * (h + radius);
        }

        int[] triangles = new int[n_vertices];
        for (int i = 0; i < n_vertices; ++i) {
            triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.colors = color_map;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        mesh.RecalculateBounds();
        return mesh;
    }

}


