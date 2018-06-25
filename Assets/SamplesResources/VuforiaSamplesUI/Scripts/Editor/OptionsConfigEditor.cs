/*===============================================================================
Copyright (c) 2017 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/
using UnityEditor;

[CustomEditor(typeof(OptionsConfig))]
[CanEditMultipleObjects]
public class OptionsConfigEditor : Editor
{
    OptionsConfig _myTarget;

    #region UNITY_EDITOR_METHODS

    void OnEnable()
    {
        _myTarget = (OptionsConfig)target;
        
        UpdateOptions();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        UpdateOptions();
    }

    #endregion // UNITY_EDITOR_METHODS

    #region PRIVATE_METHODS

    void UpdateOptions()
    {
        
        if(_myTarget.options.Length > 0)
        {
			for (int o = 0; o < _myTarget.options.Length; ++o)
			{
				_myTarget.options[o].name = "Option " + (o + 1);
				
				if (_myTarget.options[o].m_Object)
				{
					_myTarget.options[o].m_Object.SetActive(_myTarget.options[o].enabled);
				}
				else
				{
					_myTarget.options[o].enabled = false;
				}
			}
        }
    }

    #endregion // PRIVATE_METHODS

}
