/*===============================================================================
Copyright (c) 2015-2017 PTC Inc. All Rights Reserved.
 
Copyright (c) 2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.
 
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/
using UnityEngine;

public class SamplesTapHandler : MonoBehaviour
{
    #region PRIVATE_MEMBERS

    const float DOUBLE_TAP_MAX_DELAY = 0.5f;
    //seconds
    float mTimeSinceLastTap;
    MenuAnimator mMenuAnim;
    OptionsConfig optionsConfig;

    #endregion //PRIVATE_MEMBERS


    #region PROTECTED_MEMBERS

    protected int mTapCount;

    #endregion //PROTECTED_MEMBERS


    #region MONOBEHAVIOUR_METHODS

    void Start()
    {
        mTapCount = 0;
        mTimeSinceLastTap = 0;
        mMenuAnim = FindObjectOfType<MenuAnimator>();
        optionsConfig = FindObjectOfType<OptionsConfig>();
    }

    void Update()
    {
        if (mMenuAnim && mMenuAnim.IsVisible())
        {
            mTapCount = 0;
            mTimeSinceLastTap = 0;
        }
        else
        {
            HandleTap();
        }
    }

    #endregion //MONOBEHAVIOUR_METHODS


    #region PRIVATE_METHODS

    void HandleTap()
    {
        if (mTapCount == 1)
        {
            mTimeSinceLastTap += Time.deltaTime;
            if (mTimeSinceLastTap > DOUBLE_TAP_MAX_DELAY)
            {
                // too late for double tap, 
                // we confirm it was a single tap
                OnSingleTapConfirmed();

                // reset touch count and timer
                mTapCount = 0;
                mTimeSinceLastTap = 0;
            }
        }
        else if (mTapCount == 2)
        {
            // we got a double tap
            OnDoubleTap();

            // reset touch count and timer
            mTimeSinceLastTap = 0;
            mTapCount = 0;
        }

        if (Input.GetMouseButtonUp(0))
        {
            mTapCount++;
            if (mTapCount == 1)
            {
                OnSingleTap();
            }
        }
    }

    #endregion // PRIVATE_METHODS


    #region PROTECTED_METHODS

    /// <summary>
    /// This method can be overridden by custom (derived) TapHandler implementations,
    /// to perform special actions upon single tap.
    /// </summary>
    protected virtual void OnSingleTap()
    {
    }

    protected virtual void OnSingleTapConfirmed()
    {
        CameraSettings camSettings = GetComponentInChildren<CameraSettings>();
        if (camSettings)
        {
            camSettings.TriggerAutofocusEvent();
        }
    }

    protected virtual void OnDoubleTap()
    {
        if (mMenuAnim && !mMenuAnim.IsVisible() && optionsConfig.AnyOptionsEnabled())
        {
            mMenuAnim.Show();
        }
    }

    #endregion // PROTECTED_METHODS
}
