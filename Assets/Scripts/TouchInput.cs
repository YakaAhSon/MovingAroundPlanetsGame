using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchInput : MonoBehaviour {

    private GamePlay.ControlMode m_ControlMode;

    public RectTransform m_LeftJoyStick;
    public RectTransform m_RightJoyStick;
    public RawImage m_YawJoyStick;
    public Image m_YawSlidePole;

    private Vector2 m_LeftJSOrigPosition;
    private Vector2 m_RightJSOrigPosition;
    private Vector2 m_YawJoyStickOrigPosition;

    private int m_ScreenWidth = 0;

    public class HAHA<T> {
        public bool touched = false;
        public T value;
        public T delta;
    }

    public HAHA<Vector2> leftJoyStick { get; private set; } = new HAHA<Vector2>();
    public HAHA<Vector2> rightJoyStick {get; private set;} = new HAHA<Vector2>();
    public HAHA<float> yawSlide {get; private set;} = new HAHA<float>();
    public HAHA<Vector2> autoNavigate {get; private set;} = new HAHA<Vector2>();
    public HAHA<float> aircraftRoll {get; private set;} = new HAHA<float>();

    // Start is called before the first frame update
    void Start()
    {
        m_LeftJSOrigPosition = m_LeftJoyStick.anchoredPosition;
        m_RightJSOrigPosition = m_RightJoyStick.anchoredPosition;
        m_YawJoyStickOrigPosition = m_YawJoyStick.rectTransform.anchoredPosition;
        m_ScreenWidth = Screen.width;
    }

    private Vector2 m_VirtualRightJoyStickPosition;

    // Update is called once per frame
    void FixedUpdate()
    {
        bool leftJoyStick_touched = false;
        bool rightJoyStick_touched = false;
        bool yawSlide_touched = false;
        bool autoNavigate_touched = false;
        bool aircraftRoll_touched = false;


        if (Input.touchCount > 0) {
            Touch[] touches = Input.touches;
            foreach(Touch touch in touches) {
                Vector2 touch_wrt_right_bottom = touch.position - new Vector2(m_ScreenWidth, 0);
                // left joystick input
                if ((touch.position - m_LeftJoyStick.anchoredPosition).magnitude < 100) {
                    leftJoyStick.value = touch.position - m_LeftJSOrigPosition;
                    leftJoyStick.delta = touch.position - m_LeftJoyStick.anchoredPosition;
                    if (!leftJoyStick.touched) {
                        leftJoyStick.delta = new Vector2(0, 0);
                    }
                    leftJoyStick_touched = true;
                    m_LeftJoyStick.anchoredPosition = touch.position;
                }
                // turn
                else if ((touch_wrt_right_bottom - m_RightJoyStick.anchoredPosition).magnitude < 100) {
                    rightJoyStick.value = touch_wrt_right_bottom - m_RightJSOrigPosition;
                    rightJoyStick.delta = touch_wrt_right_bottom - m_VirtualRightJoyStickPosition;
                    if (!rightJoyStick.touched) {
                        rightJoyStick.delta = new Vector2(0, 0);
                    }
                    rightJoyStick_touched = true;
                    m_RightJoyStick.anchoredPosition = touch_wrt_right_bottom;
                    m_VirtualRightJoyStickPosition = touch_wrt_right_bottom;
                }
                // aircraft yaw slide
                else if (m_ControlMode==GamePlay.ControlMode.pilotting 
                    &&(touch_wrt_right_bottom - m_YawJoyStick.rectTransform.anchoredPosition).magnitude < 150) {
                    yawSlide_touched = true;
                    yawSlide.value = (touch_wrt_right_bottom - m_YawJoyStickOrigPosition).y;
                    yawSlide.delta = (touch_wrt_right_bottom - m_YawJoyStick.rectTransform.anchoredPosition).y;
                    m_YawJoyStick.rectTransform.anchoredPosition = touch_wrt_right_bottom;
                }
                // auto navigate
                else if(m_ControlMode==GamePlay.ControlMode.walking){
                    autoNavigate_touched = true;
                    autoNavigate.value = touch.position;
                }
                // roll, m_ControlMode = pilotting
                else {
                    aircraftRoll_touched = true;
                    aircraftRoll.value = touch.position.x - m_ScreenWidth / 2;
                }
            }
        }
        if (!leftJoyStick.touched) {
            m_LeftJoyStick.anchoredPosition = m_LeftJSOrigPosition;
        }
        if (!rightJoyStick.touched) {
            m_RightJoyStick.anchoredPosition = m_RightJSOrigPosition;
        }
        if (!yawSlide.touched) {
            m_YawJoyStick.rectTransform.anchoredPosition = m_YawJoyStickOrigPosition;
        }

        leftJoyStick.touched = leftJoyStick_touched;
        rightJoyStick.touched = rightJoyStick_touched;
        yawSlide.touched = yawSlide_touched;
        autoNavigate.touched = autoNavigate_touched;
        aircraftRoll.touched = aircraftRoll_touched;
    }

    public void SetControlMode(GamePlay.ControlMode cm) {
        m_ControlMode = cm;
        if (cm == GamePlay.ControlMode.walking) {
            m_YawJoyStick.GetComponent<CanvasGroup>().alpha = 0;
            m_YawSlidePole.GetComponent<CanvasGroup>().alpha = 0;
        }
        else {
            m_YawJoyStick.GetComponent<CanvasGroup>().alpha = 1;
            m_YawSlidePole.GetComponent<CanvasGroup>().alpha = 1;
        }
    }
}
