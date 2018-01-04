using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour {

    private bool tap, swipeLeft, swipeRight, swipeUp, swipeDown, swipeUpRight, swipeDownLeft;
    private bool isDraging = false;
    private bool canClick = true;
    private Vector2 startTouch, swipeDelta;
    private int minMagnitudeSwipe = 50;

    public float zoomSpeed = 0.5f;
    public float minCameraSize = 3.0f;
    public float maxCameraSize = 6.0f;
    public GameController gameController;

    public Vector2 SwipeDelta { get { return swipeDelta; } }
    public bool SwipeLeft { get { return swipeLeft; } }
    public bool SwipeRight { get { return swipeRight; } }
    public bool SwipeUp { get { return swipeUp; } }
    public bool SwipeDown { get { return swipeDown; } }
    public bool SwipeUpRight { get { return swipeUpRight; } }
    public bool SwipeDownLeft { get { return swipeDownLeft; } }
    public bool IsDragging { get { return isDraging; } }
    public bool Tap {  get { return tap; } }

    public void Update()
    {
        tap = swipeLeft = swipeRight = swipeUp = swipeDown = swipeUpRight = swipeDownLeft = false;


        #if UNITY_STANDALONE || UNITY_WEBPLAYER

        if (Input.GetMouseButtonDown(0))
        {
            tap = true;
            isDraging = true;
            startTouch = Input.mousePosition;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            if (swipeDelta.magnitude <= minMagnitudeSwipe && canClick)
            {
                SimpleClick();
            }

            Reset();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            Camera.main.orthographicSize -= zoomSpeed;
            Camera.main.orthographicSize = Mathf.Max(Camera.main.orthographicSize, minCameraSize);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            Camera.main.orthographicSize += zoomSpeed;
            Camera.main.orthographicSize = Mathf.Min(Camera.main.orthographicSize, maxCameraSize);
        }

#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

        if (Input.touches.Length == 1)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                tap = true;
                isDraging = true;
                startTouch = Input.touches[0].position;
            }
            else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
            {
                if (isDraging && swipeDelta.magnitude <= minMagnitudeSwipe && canClick)
                {
                    SimpleClick();
                }

                Reset();
            }
        }
        else if (Input.touches.Length == 2)
        {
            Vector2 touch0PrevPos = Input.touches[0].position - Input.touches[0].deltaPosition;
            Vector2 touch1PrevPos = Input.touches[1].position - Input.touches[1].deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touch0PrevPos - touch1PrevPos).magnitude;
            float touchDeltaMag = (Input.touches[0].position - Input.touches[1].position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            Camera.main.orthographicSize += deltaMagnitudeDiff * zoomSpeed;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minCameraSize, maxCameraSize);
        }
#endif


        //Calculate the distance
        CalculateSwipeDelta();

        //Did we cross de deadzone?
        if(swipeDelta.magnitude > minMagnitudeSwipe)
        {
            canClick = false;

            float x = swipeDelta.x;
            float y = swipeDelta.y;

            if (x > 0 && y > 0)
            {
                swipeUpRight = true;
            }
            else if (x < 0 && y < 0)
            {
                swipeDownLeft = true;
            }
        }
        
    }

    public void Reset()
    {
        startTouch = swipeDelta = Vector2.zero;
        isDraging = false;
        canClick = true;
    }

    public void SimpleClick()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            //Debug.Log(hit.transform.gameObject);
            if (hit.collider.tag == "Clickable")
            {
                gameController.SimpleClickBehavior(hit.transform.gameObject);
            }
        }/*
        else
        {
            gameController.DeselectBlock();
        }*/
    }

    public void CalculateSwipeDelta()
    {
        swipeDelta = Vector2.zero;
        if (isDraging)
        {
            if (Input.touches.Length > 0)
                swipeDelta = Input.touches[0].position - startTouch;
            else if (Input.GetMouseButton(0))
                swipeDelta = (Vector2)Input.mousePosition - startTouch;
        }
    }
}
