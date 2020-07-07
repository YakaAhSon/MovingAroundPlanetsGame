using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyController: MonoBehaviour
{
    public PlanetController[] m_Planets;

    private PlanetController m_CurrentPlanet = null;

    public void InitGalaxy() {
        foreach(PlanetController pc in m_Planets) {
            pc.InitPlanet();
        }
    }

    public void UpdatePlanetOrbits(float delta_time) {
        foreach(PlanetController pc in m_Planets) {
            pc.UpdatePlanet(delta_time);
        }
    }
}
