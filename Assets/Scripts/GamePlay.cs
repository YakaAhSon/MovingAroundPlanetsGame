using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlay : MonoBehaviour
{
    public GalaxyController m_Galaxy;
    public TouchInput m_TouchInput;

    public PlayerController m_Player;
    public AircraftController m_Aircraft;
    public CameraKitController m_CameraKit;
    private PlanetController m_CurrentPlanet = null;
    public enum ControlMode {
        walking,
        pilotting
    }
    private ControlMode m_CurrentControlMode;

    // Start is called before the first frame update
    void Start()
    {
        m_Galaxy.InitGalaxy();

        SetCurrentPlanet(m_Galaxy.m_Planets[2]);
        ExitAircraft();
        //m_CameraKit.EnableAutoResume();
        //m_Aircraft.TurnOff();
        //m_Player.GetOutAirCraft();
        //m_Player.SetCurrentPlanet(m_Galaxy.m_Planets[2]);
        //m_Aircraft.SetCamera(null);
    }

    private void FixedUpdate() {
        m_Galaxy.UpdatePlanetOrbits(Time.fixedDeltaTime);
        if (m_CurrentControlMode == ControlMode.pilotting) {
            if (m_CurrentPlanet == null) {
                foreach (PlanetController planet in m_Galaxy.m_Planets) {
                    if (planet.ObjectInGravityField(m_Aircraft.transform.position)) {
                        SetCurrentPlanet(planet);
                        break;
                    }
                }
            }
            else {
                if (!m_CurrentPlanet.ObjectInGravityField(m_Aircraft.transform.position)) {
                    SetCurrentPlanet(null);
                }
            }
        }
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetCurrentPlanet(PlanetController currentPlanet) {
        m_Aircraft.SetCurrentPlanet(currentPlanet);
        m_Player.SetCurrentPlanet(currentPlanet);
    }

    public void EnterAircraft() {
        m_Player.GetInAirCraft();
        m_Aircraft.SetCamera(m_CameraKit.transform);
        m_Aircraft.TurnOn();
        m_CurrentControlMode = ControlMode.pilotting;
        m_TouchInput.SetControlMode(m_CurrentControlMode);
    }

    public void ExitAircraft() {
        m_Aircraft.SetCamera(null);
        m_Aircraft.TurnOff();
        m_Player.GetOutAirCraft();
        m_CurrentControlMode = ControlMode.walking;
        m_TouchInput.SetControlMode(m_CurrentControlMode);
    }
}
