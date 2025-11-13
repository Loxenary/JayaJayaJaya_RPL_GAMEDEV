#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(GameBootstrap))]
public class GameBootstrapEditor : Editor
{
    private string _resetStatusMessage = "";
    private bool _isResetting = false;
    private bool _advancedResetActive = false;
    private SerializedProperty _saveDataScriptsToResetProp;

    private void OnEnable()
    {
        _saveDataScriptsToResetProp = serializedObject.FindProperty("saveDataScriptsToReset");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw all properties except the ones we handle manually
        DrawPropertiesExcluding(serializedObject, "isTestingEnabled", "testSceneToLoad", "saveDataScriptsToReset", "m_Script");

        SerializedProperty isTestingEnabled = serializedObject.FindProperty("isTestingEnabled");
        EditorGUILayout.PropertyField(isTestingEnabled);

        if (isTestingEnabled.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("testSceneToLoad"));
            EditorGUILayout.Space(10);

            _advancedResetActive = EditorGUILayout.ToggleLeft("Advanced Reset", _advancedResetActive);
            EditorGUILayout.Space();

            GUI.enabled = !_isResetting;

            if (_advancedResetActive)
            {
                DrawAdvancedResetUI();
            }
            else
            {
                DrawGlobalResetUI();
            }

            GUI.enabled = true;

            if (!string.IsNullOrEmpty(_resetStatusMessage))
            {
                // Use different message types based on success or failure
                bool success = _resetStatusMessage.StartsWith("Success");
                EditorGUILayout.HelpBox(_resetStatusMessage, success ? MessageType.Info : MessageType.Error);
            }
            
            EditorGUI.indentLevel--;
        }
        else
        {
            _resetStatusMessage = "";
            _advancedResetActive = false;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawAdvancedResetUI()
    {
        EditorGUILayout.LabelField("Selective Data Reset", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Drag script files that inherit from 'BaseSaveData' here. Invalid scripts will be removed automatically.", MessageType.None);
        
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_saveDataScriptsToResetProp, true);
        
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties(); 
            ValidateSaveDataScriptsList();
        }

        EditorGUILayout.Space();

        int validScriptCount = 0;
        for (int i = 0; i < _saveDataScriptsToResetProp.arraySize; i++)
        {
            if (_saveDataScriptsToResetProp.GetArrayElementAtIndex(i).objectReferenceValue != null)
                validScriptCount++;
        }

        EditorGUI.BeginDisabledGroup(validScriptCount == 0);
        if (GUILayout.Button(_isResetting ? "Resetting..." : $"Reset Selected Data ({validScriptCount})"))
        {
            HandleResetSelectedData();
        }
        EditorGUI.EndDisabledGroup();
    }
    
    private void DrawGlobalResetUI()
    {
        EditorGUILayout.LabelField("Global Data Reset", EditorStyles.boldLabel);
        if (GUILayout.Button(_isResetting ? "Resetting..." : "Reset All Resettable Data"))
        {
            HandleGlobalReset();
        }
    }

    private void ValidateSaveDataScriptsList()
    {
        bool invalidScriptAssigned = false;
        for (int i = 0; i < _saveDataScriptsToResetProp.arraySize; i++)
        {
            var element = _saveDataScriptsToResetProp.GetArrayElementAtIndex(i);
            var monoScript = element.objectReferenceValue as MonoScript;

            if (monoScript != null)
            {
                Type scriptType = monoScript.GetClass();
                bool isValid = scriptType != null && scriptType.IsSubclassOf(typeof(BaseSaveData)) && !scriptType.IsAbstract;
                if (!isValid)
                {
                    element.objectReferenceValue = null;
                    invalidScriptAssigned = true;
                }
            }
        }

        if (invalidScriptAssigned)
            Debug.LogWarning("[GameBootstrapEditor] An invalid script was assigned and cleared. Please only use scripts inheriting from BaseSaveData.");
    }

    private void HandleGlobalReset()
    {
        _isResetting = true;
        GameBootstrap bootstrap = (GameBootstrap)target;
        bootstrap.ResetData(result =>
        {
            _resetStatusMessage = result.message; // Use the new detailed message
            _isResetting = false;
            Repaint();
        });
    }

    private void HandleResetSelectedData()
    {
        var scriptList = new List<MonoScript>();
        for (int i = 0; i < _saveDataScriptsToResetProp.arraySize; i++)
        {
            var element = _saveDataScriptsToResetProp.GetArrayElementAtIndex(i).objectReferenceValue as MonoScript;
            if (element != null)
                scriptList.Add(element);
        }

        if (scriptList.Count == 0)
        {
            _resetStatusMessage = "No valid scripts were selected to reset.";
            return;
        }
        
        _isResetting = true;
        GameBootstrap bootstrap = (GameBootstrap)target;
        bootstrap.ResetSelectedData(scriptList, result =>
        {
            _resetStatusMessage = result.message; // Use the new detailed message
            _isResetting = false;
            Repaint();
        });
    }
}
#endif

