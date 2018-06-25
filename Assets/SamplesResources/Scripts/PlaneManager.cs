/*============================================================================== 
Copyright (c) 2017-2018 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.   
==============================================================================*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Vuforia;

public class PlaneManager : MonoBehaviour
{
    enum PlaneMode
    {
        GROUND,
        MIDAIR,
        PLACEMENT
    }

    #region PUBLIC_MEMBERS
    public PlaneFinderBehaviour m_PlaneFinder;
    public MidAirPositionerBehaviour m_MidAirPositioner;
    public GameObject m_PlaneAugmentation, m_MidAirAugmentation, m_PlacementPreview, m_PlacementAugmentation;
    public Text m_TitleMode;
    public Text m_OnScreenMessage;
    public UnityEngine.UI.Image m_PlaneModeIcon;
    public Toggle m_GroundToggle, m_MidAirToggle, m_PlacementToggle;
    public Button m_ResetButton;
    public CanvasGroup m_ScreenReticleGround;
    public GameObject m_TranslationIndicator;
    public GameObject m_RotationIndicator;
    public Transform Floor;

    // Placement Augmentation Size Range
    [Range(0.1f, 2.0f)]
    public float ProductSize = 0.65f;
    #endregion // PUBLIC_MEMBERS


    #region PRIVATE_MEMBERS
    const string TITLE_GROUNDPLANE = "Ground Plane";
    const string TITLE_MIDAIR = "Mid-Air";
    const string TITLE_PLACEMENT = "Product Placement";

    const string unsupportedDeviceTitle = "Unsupported Device";
    const string unsupportedDeviceBody =
        "This device has failed to start the Positional Device Tracker. " +
        "Please check the list of supported Ground Plane devices on our site: " +
        "\n\nhttps://library.vuforia.com/articles/Solution/ground-plane-supported-devices.html";

    const string EmulatorGroundPlane = "Emulator Ground Plane";

    Sprite m_IconGroundMode, m_IconMidAirMode, m_IconPlacementMode;

    StateManager m_StateManager;
    SmartTerrain m_SmartTerrain;
    PositionalDeviceTracker m_PositionalDeviceTracker;
    TouchHandler m_TouchHandler;

    PlaneMode planeMode = PlaneMode.PLACEMENT;

    GameObject m_PlaneAnchor, m_MidAirAnchor, m_PlacementAnchor;

    float m_PlacementAugmentationScale;
    int AutomaticHitTestFrameCount;
    int m_AnchorCounter;
    Vector3 ProductScaleVector;

    GraphicRaycaster m_GraphicRayCaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    Camera mainCamera;
    Ray cameraToPlaneRay;
    RaycastHit cameraToPlaneHit;
    #endregion // PRIVATE_MEMBERS


    #region MONOBEHAVIOUR_METHODS

    void Start()
    {
        Debug.Log("Start() called.");

        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
        VuforiaARController.Instance.RegisterOnPauseCallback(OnVuforiaPaused);
        DeviceTrackerARController.Instance.RegisterTrackerStartedCallback(OnTrackerStarted);
        DeviceTrackerARController.Instance.RegisterDevicePoseStatusChangedCallback(OnDevicePoseStatusChanged);

        m_PlaneFinder.HitTestMode = HitTestMode.AUTOMATIC;

        m_PlacementAugmentationScale = VuforiaRuntimeUtilities.IsPlayMode() ? 0.1f : ProductSize;
        ProductScaleVector =
            new Vector3(m_PlacementAugmentationScale,
                        m_PlacementAugmentationScale,
                        m_PlacementAugmentationScale);
        m_PlacementPreview.transform.localScale = ProductScaleVector;
        m_PlacementAugmentation.transform.localScale = ProductScaleVector;

        m_GroundToggle.interactable = false;
        m_MidAirToggle.interactable = false;
        m_PlacementToggle.interactable = false;
        m_ResetButton.interactable = false;

        // Enable floor collider if running on device; Disable if running in PlayMode
        Floor.gameObject.SetActive(!VuforiaRuntimeUtilities.IsPlayMode());

        m_IconGroundMode = Resources.Load<Sprite>("icon_ground_mode");
        m_IconMidAirMode = Resources.Load<Sprite>("icon_midair_mode");
        m_IconPlacementMode = Resources.Load<Sprite>("icon_placement_mode");

        m_TitleMode.text = TITLE_PLACEMENT;
        m_PlaneModeIcon.sprite = m_IconPlacementMode;

        mainCamera = Camera.main;

        m_TouchHandler = FindObjectOfType<TouchHandler>();
        m_GraphicRayCaster = FindObjectOfType<GraphicRaycaster>();
        m_EventSystem = FindObjectOfType<EventSystem>();
    }

    void Update()
    {
        if (planeMode == PlaneMode.PLACEMENT && m_PlacementAugmentation.activeInHierarchy)
        {
            m_RotationIndicator.SetActive(Input.touchCount == 2);
            m_TranslationIndicator.SetActive(TouchHandler.IsSingleFingerDragging());

            if (TouchHandler.IsSingleFingerDragging() || (VuforiaRuntimeUtilities.IsPlayMode() && Input.GetMouseButton(0)))
            {
                if (!IsCanvasButtonPressed())
                {
                    cameraToPlaneRay = mainCamera.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(cameraToPlaneRay, out cameraToPlaneHit))
                    {
                        if (cameraToPlaneHit.collider.gameObject.name ==
                            (VuforiaRuntimeUtilities.IsPlayMode() ? EmulatorGroundPlane : Floor.name))
                        {
                            m_PlacementAugmentation.PositionAt(cameraToPlaneHit.point);
                        }
                    }
                }
            }
        }
    }

    void LateUpdate()
    {
        if (AutomaticHitTestFrameCount == Time.frameCount)
        {
            // We got an automatic hit test this frame

            // Hide the onscreen reticle when we get a hit test
            m_ScreenReticleGround.alpha = 0;

            // Set visibility of the surface indicator
            SetSurfaceIndicatorVisible(
                planeMode == PlaneMode.GROUND ||
                (planeMode == PlaneMode.PLACEMENT && Input.touchCount == 0));

            m_OnScreenMessage.transform.parent.gameObject.SetActive(true);
            m_OnScreenMessage.enabled = true;

            if (planeMode == PlaneMode.GROUND)
            {
                m_OnScreenMessage.text = "Tap to place Astronaut";
            }
            else if (planeMode == PlaneMode.PLACEMENT)
            {
                m_OnScreenMessage.text = (m_PlacementAugmentation.activeInHierarchy) ?
                    "• Touch and drag to move Chair" +
                    "\n• Two fingers to rotate" +
                    ((m_TouchHandler.enablePinchScaling) ? " or pinch to scale" : "") +
                    "\n• Double-tap to reset the Ground Plane"
                    :
                    "Tap to place Chair";
            }
        }
        else
        {
            // No automatic hit test, so set alpha based on which plane mode is active
            m_ScreenReticleGround.alpha =
                (planeMode == PlaneMode.GROUND || planeMode == PlaneMode.PLACEMENT) ? 1 : 0;

            SetSurfaceIndicatorVisible(false);

            m_OnScreenMessage.transform.parent.gameObject.SetActive(true);
            m_OnScreenMessage.enabled = true;

            // Hide the placement preview when there's no hit
            // (during reset or pointing device above horizon line)
            m_PlacementPreview.SetActive(false);

            if (planeMode == PlaneMode.GROUND || planeMode == PlaneMode.PLACEMENT)
            {
                m_OnScreenMessage.text = "Point device towards ground";
            }
            else if (planeMode == PlaneMode.MIDAIR)
            {
                m_OnScreenMessage.text = "Tap to place Drone";
            }
        }
    }

    void OnDestroy()
    {
        Debug.Log("OnDestroy() called.");

        VuforiaARController.Instance.UnregisterVuforiaStartedCallback(OnVuforiaStarted);
        VuforiaARController.Instance.UnregisterOnPauseCallback(OnVuforiaPaused);
        DeviceTrackerARController.Instance.UnregisterTrackerStartedCallback(OnTrackerStarted);
        DeviceTrackerARController.Instance.UnregisterDevicePoseStatusChangedCallback(OnDevicePoseStatusChanged);
    }

    #endregion // MONOBEHAVIOUR_METHODS


    #region GROUNDPLANE_CALLBACKS

    public void HandleAutomaticHitTest(HitTestResult result)
    {
        AutomaticHitTestFrameCount = Time.frameCount;

        if (!m_PlacementToggle.interactable)
        {
            // Runs only once after first successful Automatic hit test
            m_PlacementToggle.interactable = true;
            m_GroundToggle.interactable = true;
            // Make the PlacementToggle active
            m_PlacementToggle.isOn = true;
        }

        if (planeMode == PlaneMode.PLACEMENT && !m_PlacementAugmentation.activeInHierarchy)
        {
            SetSurfaceIndicatorVisible(false);
            m_PlacementPreview.SetActive(true);
            m_PlacementPreview.PositionAt(result.Position);
            RotateTowardCamera(m_PlacementPreview);
        }
        else
        {
            m_PlacementPreview.SetActive(false);
        }
    }

    public void HandleInteractiveHitTest(HitTestResult result)
    {
        // If the PlaneFinderBehaviour's Mode is Automatic, then the Interactive HitTestResult will be centered.

        Debug.Log("HandleInteractiveHitTest() called.");

        if (result == null)
        {
            Debug.LogError("Invalid hit test result!");
            return;
        }

        // Place object based on Ground Plane mode
        switch (planeMode)
        {
            case PlaneMode.GROUND:

                if (m_PositionalDeviceTracker != null && m_PositionalDeviceTracker.IsActive)
                {
                    DestroyAnchors();

                    m_PlaneAnchor = m_PositionalDeviceTracker.CreatePlaneAnchor("MyPlaneAnchor_" + (++m_AnchorCounter), result);
                    m_PlaneAnchor.name = "PlaneAnchor";

                    if (!m_MidAirToggle.interactable)
                    {
                        // Runs only once after first successful Ground Anchor is created
                        m_MidAirToggle.interactable = true;
                        m_ResetButton.interactable = true;
                    }
                }

                if (!m_PlaneAugmentation.activeInHierarchy)
                {
                    Debug.Log("Setting Plane Augmentation to Active");
                    // On initial run, unhide the augmentation
                    m_PlaneAugmentation.SetActive(true);
                }

                Debug.Log("Positioning Plane Augmentation at: " + result.Position);
                // parent the augmentation to the anchor

                m_PlaneAugmentation.transform.SetParent(m_PlaneAnchor.transform);
                m_PlaneAugmentation.transform.localPosition = Vector3.zero;
                RotateTowardCamera(m_PlaneAugmentation);

                break;

            case PlaneMode.PLACEMENT:

                if (m_PositionalDeviceTracker != null && m_PositionalDeviceTracker.IsActive)
                {

                    if (m_PlacementAnchor == null || TouchHandler.DoubleTap)
                    {
                        DestroyAnchors();

                        m_PlacementAnchor = m_PositionalDeviceTracker.CreatePlaneAnchor("MyPlacementAnchor_" + (++m_AnchorCounter), result);
                        m_PlacementAnchor.name = "PlacementAnchor";

                        if (!VuforiaRuntimeUtilities.IsPlayMode())
                        {
                            Floor.position = m_PlacementAnchor.transform.position;
                        }
                        m_PlacementAugmentation.transform.SetParent(m_PlacementAnchor.transform);
                        m_PlacementAugmentation.transform.localPosition = Vector3.zero;
                    }

                    if (!m_MidAirToggle.interactable)
                    {
                        // Runs only once after first successful Ground Anchor is created
                        m_MidAirToggle.interactable = true;
                        m_ResetButton.interactable = true;
                    }
                }

                if (!m_PlacementAugmentation.activeInHierarchy)
                {
                    Debug.Log("Setting Placement Augmentation to Active");
                    // On initial placement, unhide the augmentation
                    m_PlacementAugmentation.SetActive(true);

                    Debug.Log("Positioning Placement Augmentation at: " + result.Position);
                    // parent the augmentation to the anchor
                    m_PlacementAugmentation.transform.SetParent(m_PlacementAnchor.transform);
                    m_PlacementAugmentation.transform.localPosition = Vector3.zero;
                    RotateTowardCamera(m_PlacementAugmentation);
                    m_TouchHandler.enableRotation = true;
                }

                break;
        }
    }

    public void PlaceObjectInMidAir(Transform midAirTransform)
    {
        Debug.Log("PlaceObjectInMidAir() called.");

        if (planeMode != PlaneMode.MIDAIR)
        {
            Debug.Log("Invalid Ground Plane Mode:" + planeMode);
            return;
        }

        if (m_PositionalDeviceTracker != null && m_PositionalDeviceTracker.IsActive)
        {
            DestroyAnchors();

            m_MidAirAnchor = m_PositionalDeviceTracker.CreateMidAirAnchor("MyMidAirAnchor_" + (++m_AnchorCounter), midAirTransform);
            m_MidAirAnchor.name = "MidAirAnchor";

            if (!m_MidAirAugmentation.activeInHierarchy)
            {
                Debug.Log("Setting Mid-Air Augmentation to Active");
                // On initial run, unhide the augmentation
                m_MidAirAugmentation.SetActive(true);
            }

            Debug.Log("Positioning Mid-Air Augmentation at: " + midAirTransform.position.ToString());
            // parent the augmentation to the anchor
            m_MidAirAugmentation.transform.SetParent(m_MidAirAnchor.transform);
            m_MidAirAugmentation.transform.localPosition = Vector3.zero;
            RotateTowardCamera(m_MidAirAugmentation);
        }
    }

    #endregion // GROUNDPLANE_CALLBACKS


    #region PUBLIC_BUTTON_METHODS

    public void SetGroundMode(bool active)
    {
        if (active)
        {
            planeMode = PlaneMode.GROUND;
            m_TitleMode.text = TITLE_GROUNDPLANE;
            m_PlaneModeIcon.sprite = m_IconGroundMode;
            m_PlaneFinder.gameObject.SetActive(true);
            m_MidAirPositioner.gameObject.SetActive(false);
            m_TouchHandler.enableRotation = false;
        }
    }

    public void SetMidAirMode(bool active)
    {
        if (active)
        {
            planeMode = PlaneMode.MIDAIR;
            m_TitleMode.text = TITLE_MIDAIR;
            m_PlaneModeIcon.sprite = m_IconMidAirMode;
            m_PlaneFinder.gameObject.SetActive(false);
            m_MidAirPositioner.gameObject.SetActive(true);
            m_TouchHandler.enableRotation = false;
        }
    }

    public void SetPlacementMode(bool active)
    {
        if (active)
        {
            planeMode = PlaneMode.PLACEMENT;
            m_TitleMode.text = TITLE_PLACEMENT;
            m_PlaneModeIcon.sprite = m_IconPlacementMode;
            m_PlaneFinder.gameObject.SetActive(true);
            m_MidAirPositioner.gameObject.SetActive(false);
            m_TouchHandler.enableRotation = m_PlacementAugmentation.activeInHierarchy;
        }
    }

    public void ResetScene()
    {
        Debug.Log("ResetScene() called.");
        
        // reset augmentations
        m_PlaneAugmentation.transform.position = Vector3.zero;
        m_PlaneAugmentation.transform.localEulerAngles = Vector3.zero;
        m_PlaneAugmentation.SetActive(false);

        m_MidAirAugmentation.transform.position = Vector3.zero;
        m_MidAirAugmentation.transform.localEulerAngles = Vector3.zero;
        m_MidAirAugmentation.SetActive(false);

        m_PlacementAugmentation.transform.position = Vector3.zero;
        m_PlacementAugmentation.transform.localEulerAngles = Vector3.zero;
        m_PlacementAugmentation.transform.localScale = ProductScaleVector;
        m_PlacementAugmentation.SetActive(false);

        // reset buttons
        m_PlacementToggle.isOn = true;
        m_MidAirToggle.interactable = false;
        m_ResetButton.interactable = false;

        m_PlacementAnchor = null;
        m_TouchHandler.enableRotation = false;
    }

    public void ResetTrackers()
    {
        Debug.Log("ResetTrackers() called.");

        m_SmartTerrain = TrackerManager.Instance.GetTracker<SmartTerrain>();
        m_PositionalDeviceTracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();

        // Stop and restart trackers
        m_SmartTerrain.Stop(); // stop SmartTerrain tracker before PositionalDeviceTracker
        m_PositionalDeviceTracker.Stop();
        m_PositionalDeviceTracker.Start();
        m_SmartTerrain.Start(); // start SmartTerrain tracker after PositionalDeviceTracker
    }

    #endregion // PUBLIC_BUTTON_METHODS

    #region PRIVATE_METHODS

    void DestroyAnchors()
    {
        if (!VuforiaRuntimeUtilities.IsPlayMode())
        {
            IEnumerable<TrackableBehaviour> trackableBehaviours = m_StateManager.GetActiveTrackableBehaviours();

            string destroyed = "Destroying: ";

            foreach (TrackableBehaviour behaviour in trackableBehaviours)
            {
                Debug.Log(behaviour.name +
                          "\n" + behaviour.Trackable.Name +
                          "\n" + behaviour.Trackable.ID +
                          "\n" + behaviour.GetType());

                if (behaviour is AnchorBehaviour)
                {
                    // First determine which mode (Plane or MidAir) and then delete only the anchors for that mode
                    // Leave the other mode's anchors intact
                    // PlaneAnchor_<GUID>
                    // Mid AirAnchor_<GUID>

                    switch (planeMode)
                    {
                        case PlaneMode.GROUND:
                            m_PlaneAugmentation.transform.parent = null;
                            break;
                        case PlaneMode.MIDAIR:
                            m_MidAirAugmentation.transform.parent = null;
                            break;
                        case PlaneMode.PLACEMENT:
                            m_PlacementAugmentation.transform.parent = null;
                            break;
                    }


                    if ((behaviour.Trackable.Name.Contains("PlaneAnchor") && planeMode == PlaneMode.GROUND) ||
                        (behaviour.Trackable.Name.Contains("MidAirAnchor") && planeMode == PlaneMode.MIDAIR) ||
                        (behaviour.Trackable.Name.Contains("PlacementAnchor") && planeMode == PlaneMode.PLACEMENT))
                    {
                        destroyed +=
                            "\nGObj Name: " + behaviour.name +
                            "\nTrackable Name: " + behaviour.Trackable.Name +
                            "\nTrackable ID: " + behaviour.Trackable.ID +
                            "\nPosition: " + behaviour.transform.position.ToString();

                        m_StateManager.DestroyTrackableBehavioursForTrackable(behaviour.Trackable);
                        m_StateManager.ReassociateTrackables();
                    }
                }
            }

            Debug.Log(destroyed);
        }
        else
        {
            switch (planeMode)
            {
                case PlaneMode.GROUND:
                    m_PlaneAugmentation.transform.parent = null;
                    DestroyObject(m_PlaneAnchor);
                    break;
                case PlaneMode.MIDAIR:
                    m_MidAirAugmentation.transform.parent = null;
                    DestroyObject(m_MidAirAnchor);
                    break;
                case PlaneMode.PLACEMENT:
                    m_PlacementAugmentation.transform.parent = null;
                    DestroyObject(m_PlacementAnchor);
                    break;
            }
        }

    }

    void SetSurfaceIndicatorVisible(bool isVisible)
    {
        Renderer[] renderers = m_PlaneFinder.PlaneIndicator.GetComponentsInChildren<Renderer>(true);
        Canvas[] canvas = m_PlaneFinder.PlaneIndicator.GetComponentsInChildren<Canvas>(true);

        foreach (Canvas c in canvas)
            c.enabled = isVisible;

        foreach (Renderer r in renderers)
            r.enabled = isVisible;
    }

    void RotateTowardCamera(GameObject augmentation)
    {
        var lookAtPosition = mainCamera.transform.position - augmentation.transform.position;
        lookAtPosition.y = 0;
        var rotation = Quaternion.LookRotation(lookAtPosition);
        augmentation.transform.rotation = rotation;
    }

    bool IsCanvasButtonPressed()
    {
        m_PointerEventData = new PointerEventData(m_EventSystem)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        m_GraphicRayCaster.Raycast(m_PointerEventData, results);

        bool resultIsButton = false;
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponentInParent<Toggle>() ||
                result.gameObject.GetComponent<Button>())
            {
                resultIsButton = true;
                break;
            }
        }
        return resultIsButton;
    }

    #endregion // PRIVATE_METHODS


    #region VUFORIA_CALLBACKS

    void OnVuforiaStarted()
    {
        Debug.Log("OnVuforiaStarted() called.");

        m_StateManager = TrackerManager.Instance.GetStateManager();

        // Check trackers to see if started and start if necessary
        m_PositionalDeviceTracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();
        m_SmartTerrain = TrackerManager.Instance.GetTracker<SmartTerrain>();

        if (m_PositionalDeviceTracker != null && m_SmartTerrain != null)
        {
            if (!m_PositionalDeviceTracker.IsActive)
                m_PositionalDeviceTracker.Start();
            if (m_PositionalDeviceTracker.IsActive && !m_SmartTerrain.IsActive)
                m_SmartTerrain.Start();
        }
        else
        {
            if (m_PositionalDeviceTracker == null)
                Debug.Log("PositionalDeviceTracker returned null. GroundPlane not supported on this device.");
            if (m_SmartTerrain == null)
                Debug.Log("SmartTerrain returned null. GroundPlane not supported on this device.");

            MessageBox.DisplayMessageBox(unsupportedDeviceTitle, unsupportedDeviceBody, false, null);
        }
    }

    void OnVuforiaPaused(bool paused)
    {
        Debug.Log("OnVuforiaPaused(" + paused.ToString() + ") called.");

        if (paused)
            ResetScene();
    }

    #endregion // VUFORIA_CALLBACKS


    #region DEVICE_TRACKER_CALLBACKS

    void OnTrackerStarted()
    {
        Debug.Log("OnTrackerStarted() called.");

        m_PositionalDeviceTracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();
        m_SmartTerrain = TrackerManager.Instance.GetTracker<SmartTerrain>();

        if (m_PositionalDeviceTracker != null)
        {
            if (!m_PositionalDeviceTracker.IsActive)
                m_PositionalDeviceTracker.Start();

            Debug.Log("PositionalDeviceTracker is Active?: " + m_PositionalDeviceTracker.IsActive +
                      "\nSmartTerrain Tracker is Active?: " + m_SmartTerrain.IsActive);
        }
    }

    void OnDevicePoseStatusChanged(TrackableBehaviour.Status status)
    {
        Debug.Log("OnDevicePoseStatusChanged(" + status.ToString() + ")");
    }

    #endregion // DEVICE_TRACKER_CALLBACK_METHODS
}
