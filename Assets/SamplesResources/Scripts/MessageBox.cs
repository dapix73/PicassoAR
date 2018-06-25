/*============================================================================== 
Copyright (c) 2018 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.   
==============================================================================*/

using System;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour
{
    #region PRIVATE_MEMBERS
    Text[] textComponents;
    delegate void DelegateMessageBoxButtonAction();
    DelegateMessageBoxButtonAction m_DelegateMessageBoxAction;
    #endregion // PRIVATE_MEMBERS


    #region PUBLIC_METHODS

    public static void DisplayMessageBox(string title, string body, bool dismissable, Action dismissAction)
    {
        GameObject prefab = (GameObject)Resources.Load("MessageBox");
        if (prefab)
        {
            MessageBox messageBox = Instantiate(prefab.GetComponent<MessageBox>());
            messageBox.Setup(title, body, dismissable, dismissAction);
        }
    }

    public void MessageBoxButton()
    {
        // This method called by the UI Canvas Button

        // If there's a custom method, run it first
        if (m_DelegateMessageBoxAction != null)
        {
            m_DelegateMessageBoxAction();
        }

        // Destroy MessageBox
        Destroy(gameObject);
    }

    #endregion // PUBLIC_METHODS


    #region PRIVATE_METHODS

    void Setup(string title, string body, bool dismissable = true, Action closeButton = null)
    {
        textComponents = GetComponentsInChildren<Text>();

        if (textComponents.Length >= 2)
        {
            textComponents[0].text = title;
            textComponents[1].text = body;
        }

        if (closeButton != null)
            m_DelegateMessageBoxAction = new DelegateMessageBoxButtonAction(closeButton);

        Button button = GetComponentInChildren<Button>();
        if (button != null)
            button.gameObject.SetActive(dismissable);
    }

    #endregion // PRIVATE_METHODS
}
