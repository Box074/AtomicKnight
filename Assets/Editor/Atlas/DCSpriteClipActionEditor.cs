using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DCSpriteClipAction))]
internal class DCSpriteClipActionEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DCAtlas atlas = null;
        string[] clipNames = null;
        var mp_atlas = property.serializedObject.FindProperty(nameof(DCSpriteClipCollection.m_atlas));
        var mp_clips = property.serializedObject.FindProperty(nameof(DCSpriteClipCollection.m_clips));
        if(mp_atlas != null)
        {
            atlas = (mp_atlas.objectReferenceValue as DCAtlasInstance)?.atlas;
        }
        if(mp_clips != null)
        {
            clipNames = new string[mp_clips.arraySize];
            for(int i = 0; i < clipNames.Length; i++)
            {
                var clip = mp_clips.GetArrayElementAtIndex(i);
                clipNames[i] = clip.FindPropertyRelative(nameof(DCSpriteClipCollection.Clip.m_name)).stringValue;
            }
        }

        var at = property.FindPropertyRelative(nameof(DCSpriteClipAction.m_actionType));
        var m_eventName = property.FindPropertyRelative(nameof(DCSpriteClipAction.m_eventName));
        var m_clipName = property.FindPropertyRelative(nameof(DCSpriteClipAction.m_clipName));
        var m_loop = property.FindPropertyRelative(nameof(DCSpriteClipAction.m_loop));
        var m_resetOnExit = property.FindPropertyRelative(nameof(DCSpriteClipAction.m_resetOnExit));
        var m_time = property.FindPropertyRelative(nameof(DCSpriteClipAction.m_time));
        var m_audio = property.FindPropertyRelative(nameof(DCSpriteClipAction.m_audioGroup));

        var m_startFrame = property.FindPropertyRelative(nameof(DCSpriteClipAction.m_startFrame));
        var m_stopFrame = property.FindPropertyRelative(nameof(DCSpriteClipAction.m_stopFrame));

        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        Rect GetGUIRect(int height = 20)
        {
            var rect = new Rect(position.x, position.y, position.width, height);
            position.y += height;
            return rect;
        }

        EditorGUI.PropertyField(GetGUIRect(), at, 
            new GUIContent(""));

        var type = (DCSpriteClipAction.ActionType)at.enumValueIndex;
        if(type == DCSpriteClipAction.ActionType.PlayClip || 
            type == DCSpriteClipAction.ActionType.PlayClipInRange)
        {
            if (atlas == null)
            {
                EditorGUI.PropertyField(GetGUIRect(), m_clipName);
            }
            else
            {
                var names = atlas.animNames.ToList();
                names.Insert(0, "None");
                var id = EditorGUI.Popup(GetGUIRect(), "Clip Name", 
                    names.IndexOf(m_clipName.stringValue), 
                    names.ToArray());
                if (id > 0)
                {
                    m_clipName.stringValue = atlas.animNames[id - 1];
                }
                if(id == 0)
                {
                    m_clipName.stringValue = "";
                }
            }
            EditorGUI.PropertyField(GetGUIRect(), m_startFrame);
            if(type == DCSpriteClipAction.ActionType.PlayClipInRange)
            {
                EditorGUI.PropertyField(GetGUIRect(), m_stopFrame);
            }
            else
            {
                EditorGUI.PropertyField(GetGUIRect(), m_loop);
                EditorGUI.BeginDisabledGroup(!m_loop.boolValue);
                EditorGUI.PropertyField(GetGUIRect(), m_time);
                EditorGUI.EndDisabledGroup();
            }
        }
        else if(type == DCSpriteClipAction.ActionType.SendFSMEvent
            || type == DCSpriteClipAction.ActionType.SendMessage)
        {
            EditorGUI.PropertyField(GetGUIRect(), m_eventName);
        }
        else if(type == DCSpriteClipAction.ActionType.JumpToClip)
        {
            var id = EditorGUI.Popup(GetGUIRect(), "Clip Name",
                    Array.IndexOf(clipNames, m_clipName.stringValue),
                    clipNames);
            if(id > 0)
            {
                m_clipName.stringValue = clipNames[id];
            }
        }
        else if(type == DCSpriteClipAction.ActionType.ActiveChild)
        {
            EditorGUI.PropertyField(GetGUIRect(), m_loop, new GUIContent("Active"));
            EditorGUI.PropertyField(GetGUIRect(), m_resetOnExit, new GUIContent("Reset On Exit"));
            EditorGUI.PropertyField(GetGUIRect(), m_eventName, new GUIContent("Game Object Key"));
        }
        else if(type == DCSpriteClipAction.ActionType.PlayAudio)
        {
            EditorGUI.PropertyField(GetGUIRect(), m_audio, new GUIContent("Audio Group"));
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var at = property.FindPropertyRelative(nameof(DCSpriteClipAction.m_actionType));
        var type = (DCSpriteClipAction.ActionType)at.enumValueIndex;

        var count = 0;

        if(type == DCSpriteClipAction.ActionType.PlayClip)
        {
            count = 4;
        }
        else if(type == DCSpriteClipAction.ActionType.PlayClipInRange)
        {
            count = 3;
        }
        else if(type == DCSpriteClipAction.ActionType.ActiveChild)
        {
            count = 3;
        }
        else
        {
            count = 1;
        }
        return (count + 1) * 20;
    }
}
