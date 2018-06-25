/*===============================================================================
Copyright (c) 2016-2017 PTC Inc. All Rights Reserved.
 
Copyright (c) 2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.
===============================================================================*/
using UnityEngine;
using System.Collections;
using Vuforia;

public class InitErrorHandler : MonoBehaviour
{
    #region PUBLIC_MEMBER_VARIABLES
    public UnityEngine.UI.Text errorText;
    #endregion //PUBLIC_MEMBER_VARABLES


    #region PRIVATE_MEMBER_VARIABLES
    private Canvas errorCanvas;
    private string key;
    private string errorMsg;
    #endregion //PRIVATE_MEMBER_VARIABLES


    #region MONOBEHAVIOUR_METHODS
    void Awake()
    {
        // Get the UI Canvas that contains (parent of) the error text box
        if (errorText)
        {
            errorCanvas = errorText.GetComponentsInParent<Canvas>(true)[0];
        }

        VuforiaRuntime.Instance.RegisterVuforiaInitErrorCallback(OnInitError);
    }
    #endregion //MONOBEHAVIOUR_METHODS


    #region PRIVATE_METHODS
    private void OnInitError(VuforiaUnity.InitError error)
    {
        if (error != VuforiaUnity.InitError.INIT_SUCCESS)
        {
            ShowErrorMessage(error);
        }
    }

    private void ShowErrorMessage(VuforiaUnity.InitError errorCode)
    {
        switch (errorCode)
        {
            case VuforiaUnity.InitError.INIT_EXTERNAL_DEVICE_NOT_DETECTED:
                errorMsg =
                    "Failed to initialize Vuforia because this " +
                    "device is not docked with required external hardware.";
                break;
            case VuforiaUnity.InitError.INIT_LICENSE_ERROR_MISSING_KEY:
                errorMsg =
                    "Vuforia App Key is missing. \n" +
                    "Please get a valid key, by logging into your account at " +
                    "developer.vuforia.com and creating a new project.";
                break;
            case VuforiaUnity.InitError.INIT_LICENSE_ERROR_INVALID_KEY:
                errorMsg =
                    "Vuforia App key is invalid. \n" +
                    "Please get a valid key, by logging into your account at " +
                    "developer.vuforia.com and creating a new project. \n\n" +
                    getKeyInfo();
                break;
            case VuforiaUnity.InitError.INIT_LICENSE_ERROR_NO_NETWORK_TRANSIENT:
                errorMsg = "Unable to contact server. Please try again later.";
                break;
            case VuforiaUnity.InitError.INIT_LICENSE_ERROR_NO_NETWORK_PERMANENT:
                errorMsg = "No network available. Please make sure you are connected to the Internet.";
                break;
            case VuforiaUnity.InitError.INIT_LICENSE_ERROR_CANCELED_KEY:
                errorMsg =
                    "This App license key has been cancelled and may no longer be used. " +
                    "Please get a new license key. \n\n" +
                    getKeyInfo();
                break;
            case VuforiaUnity.InitError.INIT_LICENSE_ERROR_PRODUCT_TYPE_MISMATCH:
                errorMsg =
                    "Vuforia App key is not valid for this product. Please get a valid key, " +
                    "by logging into your account at developer.vuforia.com and choosing the " +
                    "right product type during project creation. \n\n" +
                    getKeyInfo() + "\n\n" +
                    "Note that Universal Windows Platform (UWP) apps require " +
                    "a license key created on or after August 9th, 2016.";
                break;
            case VuforiaUnity.InitError.INIT_NO_CAMERA_ACCESS:
                errorMsg =
                    "User denied Camera access to this app.\n" +
                    "To restore, enable Camera access in Settings:\n" +
                    "Settings > Privacy > Camera > " + Application.productName + "\n" +
                    "Also verify that the Camera is enabled in:\n" +
                    "Settings > General > Restrictions.";
                break;
            case VuforiaUnity.InitError.INIT_DEVICE_NOT_SUPPORTED:
                errorMsg = "Failed to initialize Vuforia because this device is not supported.";
                break;
            case VuforiaUnity.InitError.INIT_ERROR:
                errorMsg = "Failed to initialize Vuforia.";
                break;
        }

        // Prepend the error code in red
        errorMsg = "<color=red>" + errorCode.ToString().Replace("_", " ") + "</color>\n\n" + errorMsg;

        // Remove rich text tags for console logging
        var errorTextConsole = errorMsg.Replace("<color=red>", "").Replace("</color>", "");

        Debug.LogError("Vuforia initialization failed: " + errorCode + "\n\n" + errorTextConsole);

        if (errorText)
            errorText.text = errorMsg;

        if (errorCanvas)
        {
            // Show the error message UI canvas
            errorCanvas.transform.parent.position = Vector3.zero;
            errorCanvas.gameObject.SetActive(true);
            errorCanvas.enabled = true;
        }
    }

    string getKeyInfo()
    {
        string key = VuforiaConfiguration.Instance.Vuforia.LicenseKey;
        string keyInfo;
        if (key.Length > 10)
            keyInfo =
                "Your current key is <color=red>" + key.Length + "</color> characters in length. " +
                "It begins with <color=red>" + key.Substring(0, 5) + "</color> " +
                "and ends with <color=red>" + key.Substring(key.Length - 5, 5) + "</color>.";
        else
            keyInfo =
                "Your current key is <color=red>" + key.Length + "</color> characters in length. \n" +
                "The key is: <color=red>" + key + "</color>.";
        return keyInfo;
    }
    #endregion //PRIVATE_METHODS


    #region PUBLIC_METHODS
    public void OnErrorDialogClose()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion //PUBLIC_METHODS
}
