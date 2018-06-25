/*===============================================================================
Copyright (c) 2015-2017 PTC Inc. All Rights Reserved.
 
Copyright (c) 2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.
 
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/
using UnityEngine;
using UnityEngine.UI;

public class SamplesLoadingScreen : MonoBehaviour
{

    #region PRIVATE_MEMBER_VARIABLES

    bool mChangeLevel = true;
    RawImage mUISpinner;

    #endregion // PRIVATE_MEMBER_VARIABLES


    #region MONOBEHAVIOUR_METHODS

    void Start()
    {
        mUISpinner = GetComponentInChildren<RawImage>();
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        mChangeLevel = true;
    }

    void Update()
    {
        if (mUISpinner)
            mUISpinner.rectTransform.Rotate(Vector3.forward, 90.0f * Time.deltaTime);

        if (mChangeLevel)
        {
            LoadNextSceneAsync();
            mChangeLevel = false;
        }
    }

    #endregion // MONOBEHAVIOUR_METHODS


    #region PRIVATE_METHODS

    void LoadNextSceneAsync()
    {
        string sceneName = SamplesMainMenu.GetSceneToLoad();

        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
    }

    #endregion // PRIVATE_METHODS

}
