using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetController : MonoBehaviour
{
    // Planet Terrain 
    [UnityEngine.SerializeField]
    private TerrainGenerator m_TerrainGenerator;
    [UnityEngine.SerializeField]
    private TerrainGenerator.PlanetType m_PlanetType;
    [UnityEngine.SerializeField]
    private int m_PlanetLevel;

    public GameObject m_PlantContainer;

    [UnityEngine.SerializeField]
    private PlanetMaterial m_PlanetMaterial;

    [System.Serializable]
    public class PlantSetting {
        public PlanetMaterial.PlantType m_PlantType;
        public PlanetMaterial.Material m_InhabitMaterial;
        public int m_PlantNumber;
    }
    [UnityEngine.SerializeField]
    private PlantSetting[] m_PlantSettings;

    private struct TerrainTriangle {
        public Vector3[] vertices;
        public PlanetMaterial.Material material;
    }
    private TerrainTriangle[] m_Terrain;

    private float m_Radius;
    private float m_GravityFieldRadius;

    // Planet Orbit
    private Transform m_Orbit;
    // revolution variables
    [UnityEngine.SerializeField]
    private Vector3 m_RevolutionAxis;
    [UnityEngine.SerializeField]
    private float m_RevolutionRadius;
    [UnityEngine.SerializeField]
    private float m_RevolutionPeriod;

    private Quaternion m_RevAxisTilt;
    private float m_RevRadSpeed;
    private float m_CurrentRevRad;

    // rotation variables
    [UnityEngine.SerializeField]
    private Vector3 m_RotationAxis;
    [UnityEngine.SerializeField]
    private float m_RotationPeriod;

    private Quaternion m_RotAxisTilt;
    private float m_RotAngularSpeed;
    private float m_CurrentRotAngle;

    // Start is called before the first frame update
    public void InitPlanet() {
        PlanetMaterial.Material[] material_map;
        // generate terrain
        Mesh mesh = m_TerrainGenerator.GenerateRandomPlanet(m_PlanetLevel, UnityEngine.Random.Range(0, 0x7fffffff),
            m_PlanetType, out material_map);
        GetComponent<MeshFilter>().mesh = mesh;
        // copy the planet terrain
        m_Terrain = new TerrainTriangle[mesh.triangles.Length / 3];
        Vector3[] vertices = mesh.vertices;
        int n_triangles = mesh.triangles.Length / 3;
        for (int i = 0; i < n_triangles; ++i) {
            m_Terrain[i].vertices = new Vector3[3] { vertices[i * 3], vertices[i * 3 + 1], vertices[i * 3 + 2] };
            m_Terrain[i].material = material_map[i];
        }

        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().mesh;

        m_Radius = Mathf.Pow(2, m_PlanetLevel + 3);
        float[] level_to_gravity_radius = new float[4] { 16, 16, 32, 32 };
        m_GravityFieldRadius = m_Radius + level_to_gravity_radius[m_PlanetLevel];

        m_Orbit = transform.parent;

        // initialize orbit
        m_RevolutionAxis.Normalize();
        m_RevAxisTilt = Quaternion.FromToRotation(Vector3.up, m_RevolutionAxis);
        m_RevRadSpeed = Mathf.PI * 2f / m_RevolutionPeriod;
        m_CurrentRevRad = UnityEngine.Random.value * Mathf.PI * 2f;

        m_RotationAxis.Normalize();
        m_RotAxisTilt = Quaternion.FromToRotation(Vector3.up, m_RotationAxis);
        m_RotAngularSpeed  = 360 / m_RotationPeriod;
        m_CurrentRotAngle = UnityEngine.Random.value * 360;

        ComputeOrbit();
        GeneratePlants();
    }

    private void GeneratePlants() {
        m_PlantContainer = new GameObject();
        m_PlantContainer.transform.SetParent(transform,true);
        m_PlantContainer.transform.localPosition = Vector3.zero;
        m_PlantContainer.transform.localRotation = Quaternion.identity;
        m_PlantContainer.transform.localScale = Vector3.one;
        // input:
        // m_Terrain
        // m_PlantSettings
        int material_count = (int)PlanetMaterial.Material.end;
        List<int>[] material_triangles = new List<int>[material_count];
        for(int i = 0; i < material_count; ++i) {
            material_triangles[i] = new List<int>();
        }
        for(int i = 0; i < m_Terrain.Length; ++i) {
            material_triangles[(int)m_Terrain[i].material].Add(i);
        }
        int[][] material_triangle_array = new int[material_count][];
        for(int i = 0; i < material_count; ++i) {
            material_triangle_array[i] = material_triangles[i].ToArray();
        }
        foreach(var plant_setting in m_PlantSettings) {
            int[] triangles = material_triangle_array[(int)plant_setting.m_InhabitMaterial];
            for(int i = 0; i < plant_setting.m_PlantNumber; ++i) {
                GameObject new_plant = Instantiate(m_PlanetMaterial.m_PlantPrefabs[(int)plant_setting.m_PlantType]);
                int triangle_id = triangles[UnityEngine.Random.Range(0, triangles.Length - 1)];
                // random uvw
                float u = Random.value;
                float v = Random.value;
                if (u + v > 1) {
                    u = 1 - u;
                    v = 1 - v;
                }
                float w = 1 - u - v;
                // compute position
                Vector3 position = m_Terrain[triangle_id].vertices[0] * u 
                    + m_Terrain[triangle_id].vertices[1] * v 
                    + m_Terrain[triangle_id].vertices[2] * w;
                new_plant.transform.SetParent(m_PlantContainer.transform,true);
                new_plant.transform.localPosition = position;
                new_plant.transform.localRotation = Quaternion.FromToRotation(Vector3.up, position);
                new_plant.transform.Rotate(Vector3.up, UnityEngine.Random.value * 360);
            }

        }

    }

    public void UpdatePlanet(float delta_time) {
        m_CurrentRevRad += m_RevRadSpeed * delta_time;
        m_CurrentRotAngle += m_RotAngularSpeed * delta_time;
        ComputeOrbit();
    }

    private void ComputeOrbit() {
        Vector3 rev_local = Mathf.Cos(m_CurrentRevRad) * Vector3.right + Mathf.Sin(m_CurrentRevRad) * Vector3.forward;
        m_Orbit.localPosition = (m_RevAxisTilt *rev_local*m_RevolutionRadius);

        m_Orbit.localRotation = Quaternion.identity;

        transform.localPosition = Vector3.zero;
        transform.localRotation = (m_RotAxisTilt * Quaternion.AngleAxis(m_CurrentRotAngle, Vector3.up)).normalized;
    }

    public bool ObjectInGravityField(Vector3 position) {
        return (position - transform.position).magnitude < m_GravityFieldRadius;
    }

}
