/*===============================================================================
Copyright (c) 2017 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/
using UnityEngine;

public class OptionsConfig : MonoBehaviour
{
    [System.Serializable]
    public class Option
    {
        [HideInInspector]
        public string name;
        public bool enabled;
        public GameObject m_Object;
    }

    public Option[] options;

    public bool AnyOptionsEnabled()
    {
        foreach (Option o in options)
        {
            if (o.enabled)
                return true;
        }
        return false;
    }
}
