using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetMaterial:MonoBehaviour
{
    public enum Material {
        water,
        sand,
        grass,
        dirt,
        stone,
        scortch,
        snow,
        end
    }

    private Dictionary<Material, Color> m_MaterialColors = new Dictionary<Material, Color>() {
        { Material.water,Color.blue },
        { Material.sand,new Color(1,0.8f,0.5f) },
        { Material.grass,Color.green },
        { Material.dirt,new Color(0.6f,0.4f,0f) },
        { Material.stone,new Color(0.4f,0.4f,0.4f) },
        { Material.snow,new Color(1f,1f,1f) },
        { Material.scortch,new Color(170/256f,59/256f,25/256.0f) },
    };

    public Color GetMaterialColor(Material m) {
        return m_MaterialColors[m];
    }

    public enum PlantType {
        flower1,
        flower2,
        flower3,
        flower4,
        flower5,
        flower6,
        grass1,
        grass2,
        bush1,
        bush2,
        mushroom1,
        mushroom2,
        stone1,
        stone2,
        tree1,
        tree2
    }

    public GameObject[] m_PlantPrefabs;
}
