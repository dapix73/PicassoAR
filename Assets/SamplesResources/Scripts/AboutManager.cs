/*============================================================================== 
Copyright (c) 2017-2018 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.   
==============================================================================*/

using UnityEngine;
using UnityEngine.UI;

public class AboutManager : MonoBehaviour
{
    #region PUBLIC_MEMBERS
    public Text aboutText;
    #endregion //PUBLIC_MEMBERS


    #region PRIVATE_METHODS
    void LoadNextScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
    }
    #endregion //PRIVATE_METHODS


    #region MONOBEHAVIOUR_METHODS

    void Start()
    {
        UpdateAboutText();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            LoadNextScene();
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID
			// On Android, the Back button is mapped to the Esc key
			// Exit app
            Application.Quit();
#endif
        }
    }
    #endregion // MONOBEHAVIOUR_METHODS

    void UpdateAboutText()
    {
        if (!aboutText) return;

        string vuforiaVersion = Vuforia.VuforiaUnity.GetVuforiaLibraryVersion();
        string unityVersion = Application.unityVersion;

        string vuforia = Vuforia.VuforiaRuntime.Instance.HasInitialized
                                ? "<color=green>Yes</color>"
                                : "<color=red>No (enable Vuforia in XR Settings)</color>";

        string about =
            "\n<size=26>Description:</size>" +
            "\nThe Ground Plane sample demonstrates how to place " +
            "content on surfaces and in mid-air using anchor points." +
            "\n" +
            "\n<size=26>Key Functionality:</size>" +
            "\n• Hit testing places the Chair or the Astronaut on an intersecting plane in " +
            "the environment. Try this mode by pressing the Chair or Astronaut buttons." +
            "\n• Mid-Air anchoring places the drone on an anchor point created " +
            "at a fixed distance relative to the user. Select this mode by " +
            "pressing the Drone button." +
            "\n" +
            "\n<size=26>Physical Targets:</size>" +
            "\n• None required" +
            "\n" +
            "\n<size=26>Instructions:</size>" +
            "\n• Launch the app and view your environment" +
            "\n• Look around until the indicator shows that you have found a surface" +
            "\n• Tap to place Chair on the ground" +
            "\n• Drag with one finger to move Chair along ground" +
            "\n• Touch and hold with two fingers to rotate Chair" +
            "\n• Select Ground Plane mode" +
            "\n• Tap to place Astronaut on the ground" +
            "\n• Tap again to move Astronaut to second point" +
            "\n• Select Mid-Air mode" +
            "\n• Tap to place Drone in the air" +
            "\n• Tap again to move Drone to the desired position" +
            "\n" +
            "\n<size=26>Build Version Info:</size>" +
            "\n• Vuforia " + vuforiaVersion +
            "\n• Unity " + unityVersion +
            "\n" +
            "\n<size=26>Project Settings Info:</size>" +
            "\n• Vuforia Enabled: " + vuforia +
            "\n" +
            "\n<size=26>Statistics:</size>" +
            "\nData collected is used solely for product quality improvements" +
            "\nhttps://developer.vuforia.com/legal/statistics" +
            "\n" +
            "\n<size=26>Developer Agreement:</size>" +
            "\nhttps://developer.vuforia.com/legal/vuforia-developer-agreement" +
            "\n" +
            "\n<size=26>Privacy Policy:</size>" +
            "\nhttps://developer.vuforia.com/legal/privacy" +
            "\n" +
            "\n<size=26>Terms of Use:</size>" +
            "\nhttps://developer.vuforia.com/legal/EULA" +
            "\n" +
            "\n© 2018 PTC Inc. All Rights Reserved." +
            "\n";

        aboutText.text = about;

        Debug.Log("Vuforia " + vuforiaVersion + "\nUnity " + unityVersion);

    }

}