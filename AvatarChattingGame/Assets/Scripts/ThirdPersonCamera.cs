using UnityEngine;
using UnityEngine.EventSystems;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    public int mouseButton = 1; // right button by default

    public float distance = 20;
    public float minDistance = 3;
    public float maxDistance = 20;

    public float zoomSpeedMouse = 1;
    public float zoomSpeedTouch = 0.2f;
    public float rotationSpeed = 2;

    public float xMinAngle = -40;
    public float xMaxAngle = 80;

    // the target position can be adjusted by an offset in order to foucs on a
    // target's head for example
    public Vector3 offset = Vector3.zero;

    // view blocking
    // note: only works against objects with colliders.
    //       uMMORPG has almost none by default for performance reasons
    // note: remember to disable the entity layers so the camera doesn't zoom in
    //       all the way when standing inside another entity
    public LayerMask viewBlockingLayers;

    // store rotation so that unity never modifies it, otherwise unity will put
    // it back to 360 as soon as it's <0, which makes a negative min angle
    // impossible
    Vector3 rotation;
    bool rotationInitialized;

    void LateUpdate()
    {
        if (!target) return;

        Vector3 targetPos = target.position + offset;

        // rotation and zoom should only happen if not in a UI right now
        if (!IsCursorOverUserInterface())
        {
            // right mouse rotation if we have a mouse
            if (Input.mousePresent)
            {
                if (Input.GetMouseButton(mouseButton))
                {
                    // initialize the base rotation if not initialized yet.
                    // (only after first mouse click and not in Awake because
                    //  we might rotate the camera inbetween, e.g. during
                    //  character selection. this would cause a sudden jump to
                    //  the original rotation from Awake otherwise.)
                    if (!rotationInitialized)
                    {
                        rotation = transform.eulerAngles;
                        rotationInitialized = true;
                    }

                    // note: mouse x is for y rotation and vice versa
                    rotation.y += Input.GetAxis("Mouse X") * rotationSpeed;
                    rotation.x -= Input.GetAxis("Mouse Y") * rotationSpeed;
                    rotation.x = Mathf.Clamp(rotation.x, xMinAngle, xMaxAngle);
                    transform.rotation = Quaternion.Euler(rotation.x, rotation.y, 0);
                }
            }
            else
            {
                // forced 45 degree if there is no mouse to rotate (for mobile)
                transform.rotation = Quaternion.Euler(new Vector3(45, 0, 0));
            }

            // zoom
            float speed = Input.mousePresent ? zoomSpeedMouse : zoomSpeedTouch;
            float step = GetZoomUniversal() * speed;
            distance = Mathf.Clamp(distance - step, minDistance, maxDistance);
        }

        // target follow
        transform.position = targetPos - (transform.rotation * Vector3.forward * distance);

        // avoid view blocking (disabled, see comment at the top)
        if (Physics.Linecast(targetPos, transform.position, out RaycastHit hit, viewBlockingLayers))
        {
            // calculate a better distance (with some space between it)
            float d = Vector3.Distance(targetPos, hit.point) - 0.1f;

            // set the final cam position
            transform.position = targetPos - (transform.rotation * Vector3.forward * d);
        }
    }

    public static bool IsCursorOverUserInterface()
    {
        // IsPointerOverGameObject check for left mouse (default)
        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        // IsPointerOverGameObject check for touches
        for (int i = 0; i < Input.touchCount; ++i)
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                return true;

        // OnGUI check
        return GUIUtility.hotControl != 0;
    }

    public static float GetZoomUniversal()
    {
        if (Input.mousePresent)
            return GetAxisRawScrollUniversal();
        else if (Input.touchSupported)
            return GetPinch();
        return 0;
    }

    public static float GetAxisRawScrollUniversal()
    {
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scroll < 0) return -1;
        if (scroll > 0) return 1;
        return 0;
    }

    public static float GetPinch()
    {
        if (Input.touchCount == 2)
        {
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            return touchDeltaMag - prevTouchDeltaMag;
        }
        return 0;
    }
}
