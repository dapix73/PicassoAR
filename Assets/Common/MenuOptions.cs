/*===============================================================================
Copyright (c) 2015-2017 PTC Inc. All Rights Reserved.
 
Copyright (c) 2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.
 
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Vuforia;

public class MenuOptions : MonoBehaviour
{
    #region PRIVATE_MEMBERS
    private CameraSettings mCamSettings = null;
    private TrackableSettings mTrackableSettings = null;
    private MenuAnimator mMenuAnim = null;
    #endregion //PRIVATE_MEMBERS


    #region MONOBEHAVIOUR_METHODS
    protected virtual void Start()
    {
        mCamSettings = FindObjectOfType<CameraSettings>();
        mTrackableSettings = FindObjectOfType<TrackableSettings>();
        mMenuAnim = FindObjectOfType<MenuAnimator>();

        var vuforia = VuforiaARController.Instance;
        vuforia.RegisterOnPauseCallback(OnPaused);
    }
    #endregion //MONOBEHAVIOUR_METHODS


    #region PUBLIC_METHODS
    public void ShowAboutPage()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("1-About");
    }

    public void ToggleAutofocus()
    {
        Toggle autofocusToggle = FindUISelectableWithText<Toggle>("Autofocus");
        if (autofocusToggle && mCamSettings)
            mCamSettings.SwitchAutofocus(autofocusToggle.isOn);
    }

    public void ToggleTorch()
    {
        Toggle flashToggle = FindUISelectableWithText<Toggle>("Flash");
        if (flashToggle && mCamSettings)
        {
            mCamSettings.SwitchFlashTorch(flashToggle.isOn);

            // Update UI toggle status (ON/OFF) in case the flash switch failed
            flashToggle.isOn = mCamSettings.IsFlashTorchEnabled();
        }
    }

    public void ToggleFrontCamera()
    {
        if (mCamSettings)
        {
            mCamSettings.SelectCamera(mCamSettings.IsFrontCameraActive() ? CameraDevice.CameraDirection.CAMERA_BACK : CameraDevice.CameraDirection.CAMERA_FRONT);

            // Toggle flash if it is on while switching to front camera
            Toggle flashToggle = FindUISelectableWithText<Toggle>("Flash");
            if (mCamSettings.IsFrontCameraActive() && flashToggle && flashToggle.isOn)
                ToggleTorch();
        }
    }

    public void ToggleExtendedTracking()
    {
        Toggle extTrackingToggle = FindUISelectableWithText<Toggle>("Extended");
        if (extTrackingToggle && mTrackableSettings)
            mTrackableSettings.SwitchExtendedTracking(extTrackingToggle.isOn);
    }

    public void ActivateDataset(string datasetName)
    {
        if (mTrackableSettings)
            mTrackableSettings.ActivateDataSet(datasetName);
    }

    public void UpdateUI()
    {
        Toggle extTrackingToggle = FindUISelectableWithText<Toggle>("Extended");
        if (extTrackingToggle && mTrackableSettings)
            extTrackingToggle.isOn = mTrackableSettings.IsExtendedTrackingEnabled();

        Toggle flashToggle = FindUISelectableWithText<Toggle>("Flash");
        if (flashToggle && mCamSettings)
            flashToggle.isOn = mCamSettings.IsFlashTorchEnabled();

        Toggle autofocusToggle = FindUISelectableWithText<Toggle>("Autofocus");
        if (autofocusToggle && mCamSettings)
            autofocusToggle.isOn = mCamSettings.IsAutofocusEnabled();

        Toggle frontCamToggle = FindUISelectableWithText<Toggle>("FrontCamera");
        if (frontCamToggle && mCamSettings)
            frontCamToggle.isOn = mCamSettings.IsFrontCameraActive();

    }

    public void RestartObjectTracker()
    {
        var objTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        if (objTracker != null && objTracker.IsActive)
        {
            objTracker.Stop();

            foreach(DataSet dataset in objTracker.GetDataSets())
            {
                objTracker.DeactivateDataSet(dataset);
                objTracker.ActivateDataSet(dataset);
            }

            objTracker.Start();
        }
    }

    public void CloseMenu()
    {
        if (mMenuAnim)
            mMenuAnim.Hide();
    }
    #endregion //PUBLIC_METHODS


    #region PROTECTED_METHODS
    protected T FindUISelectableWithText<T>(string text) where T : UnityEngine.UI.Selectable
    {
        T[] uiElements = GetComponentsInChildren<T>(true);
        foreach (var uielem in uiElements)
        {
            string childText = uielem.GetComponentInChildren<Text>().text;
            if (childText.Contains(text))
                return uielem;
        }
        return null;
    }
    #endregion //PROTECTED_METHODS

    #region PRIVATE_METHODS
    private void OnPaused(bool paused)
    {
        bool appResumed = !paused;
        if (appResumed)
        {
            // The flash torch is switched off by the OS automatically when app is paused.
            // On resume, update torch UI toggle to match torch status.
            Toggle flashToggle = FindUISelectableWithText<Toggle>("Flash");

            if (flashToggle != null)
                flashToggle.isOn = mCamSettings.IsFlashTorchEnabled();
        }
    }
    #endregion //PRIVATE_METHODS

}
