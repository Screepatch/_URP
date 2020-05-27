using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class ProjBookmarks : EditorWindow
{
    private readonly string[] _separators = {" @R:", "G:", "B:"};
    
    private Vector2 _scrollPos;
    private string[] _bookmarks;
    
    private bool _changed;
    private List<string> _bookmarksLst;
    private List<Color32> _buttonsColors;

    private static string BookmarkLocation
    {
        get
        {
            string pathString = Path.Combine(Application.dataPath, "_IgnoredStuff_/Bookmarks/bookmarks.txt");
            pathString = pathString.Replace(@"\",Path.DirectorySeparatorChar.ToString());
            pathString = pathString.Replace(@"/",Path.DirectorySeparatorChar.ToString());

            return Path.GetInvalidPathChars()
                .Aggregate(pathString,
                    (current,
                        pathChar) => current.Replace(pathChar.ToString(),
                        string.Empty));
        }
    }

	
    [MenuItem("TA Tools/Misc/Project Bookmarks", false, 6)]
    internal static void Init()
    {
        var window = (ProjBookmarks)GetWindow(typeof(ProjBookmarks), false, "Bookmarks");
    }


    private void LoadBookmarks()
    {
        _buttonsColors = new List<Color32>();
        for (var i = 0; i < _bookmarks.Length; i++)
        {
            if (_bookmarks[i].Contains(_separators[0]))
            {
                string[] pathAndColors = _bookmarks[i].Split(_separators, StringSplitOptions.None);
                
                _bookmarks[i] = pathAndColors[0];
                
                var buttonColor = new Color32(
                    byte.Parse(pathAndColors[1]),
                    byte.Parse(pathAndColors[2]),
                    byte.Parse(pathAndColors[3]),
                    255);
                _buttonsColors.Add(buttonColor);
            }
            else
            {
                _buttonsColors.Add(Color.white);
            }
        }
        
        _bookmarksLst = new List<string>(_bookmarks);
    }

    
    private void SaveBookmarks()
    {
        _bookmarks = _bookmarksLst.ToArray();
        
        for (var i = 0; i < _bookmarks.Length; i++)
        {
            if (!_buttonsColors[i].Color32IsEqualTo(Color.white))
            {
                _bookmarks[i] += _separators[0] + _buttonsColors[i].r +
                                 _separators[1] + _buttonsColors[i].g +
                                 _separators[2] + _buttonsColors[i].b;
            }
        }
        
        File.WriteAllLines(BookmarkLocation, _bookmarks);
    }
    
	
    private void OnEnable()
    {
        if(string.IsNullOrEmpty(BookmarkLocation))
        {
            return;
        }
		
        string directoryPath = Path.GetDirectoryName(BookmarkLocation);
        if (directoryPath == null)
        {
            return;
        }

        if(File.Exists(BookmarkLocation))
        {
            File.Copy(BookmarkLocation, Path.ChangeExtension(BookmarkLocation, "bak"), true);
            AssetDatabase.Refresh();
			
            _bookmarks = File.ReadAllLines(BookmarkLocation);
        }
        else
        {
            if (Directory.Exists(directoryPath))
            {
                _bookmarks = new string[] { };
            }
            else
            {
                Directory.CreateDirectory(directoryPath);
                AssetDatabase.Refresh();
            }
            if (Directory.Exists(directoryPath))
            {
                _bookmarks = new string[] { };
            }
        }
        
        if (_bookmarks == null)
        {
            return;
        }
        
        LoadBookmarks();
    }


    private void OnSelectionChange()
    {
        Repaint();
    }

	
    public void OnGUI()
    {
        if (_bookmarks == null)
        {
            GUILayout.Label("Bookmarks file error");
            return;
        }
	
        _changed = false;

        EditorGUILayout.BeginVertical();

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, false);
		
        GUILayout.Label("Bookmarks", EditorStyles.boldLabel);

        for (int stringIndex = 0; stringIndex < _bookmarksLst.Count; stringIndex++)
        {
            if (string.IsNullOrEmpty(_bookmarksLst[stringIndex]))
            {
                _bookmarksLst.RemoveAt(stringIndex);
                stringIndex--;
                _changed = true;
                continue;
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            _buttonsColors[stringIndex] = EditorGUILayout.ColorField(GUIContent.none,
                _buttonsColors[stringIndex],
                false,
                false,
                false,
                GUILayout.MaxWidth(10));
            if (EditorGUI.EndChangeCheck())
            {
                _changed = true;
            }
            
            GUI.backgroundColor = _buttonsColors[stringIndex];

            if (GUILayout.Button(_bookmarksLst[stringIndex].Substring(7), new GUIStyle(GUI.skin.GetStyle("Button"))
            {
                alignment = TextAnchor.MiddleLeft,
            }))
            {
                if (Event.current.button == 1)
                {
                    return;
                }

                Selection.activeObject = null;

                string[] objs = AssetDatabase.FindAssets("", new string[]{_bookmarksLst[stringIndex]});
                foreach (string c in objs)
                {
                    Object cObj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(c), typeof(Object));
                    if (cObj == null)
                    {
                        continue;
                    }
                    
                    string currAssetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(cObj))
                        .Replace(Path.DirectorySeparatorChar.ToString(), "/");
                    if (currAssetPath != _bookmarksLst[stringIndex])
                    {
                        continue;
                    }

                    Selection.activeObject = cObj;
                    break;
                }

                if (Selection.activeObject == null)
                {
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath(_bookmarksLst[stringIndex], typeof(Object));
                }

                EditorGUIUtility.PingObject(Selection.activeObject);

            }
            
            if (GUILayout.Button("X",GUILayout.Width(20.0f)))
            {
                if (Event.current.button == 1)
                {
                    return;
                }
                _bookmarksLst.RemoveAt(stringIndex);
                stringIndex--;
                _changed = true;
            }

            EditorGUILayout.EndHorizontal();
        }
        
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Separator ();

        string curr = AssetDatabase.GetAssetPath (Selection.activeObject);

        EditorGUILayout.HelpBox ("Current : " + curr, MessageType.None);
		
        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(curr));
        if(GUILayout.Button("Add from current selection"))
        {
            if(!_bookmarksLst.Contains(curr))
            {
                _bookmarksLst.Add(curr);
                _buttonsColors.Add(Color.white);

                _changed = true;
            }
        }
        EditorGUI.EndDisabledGroup();
		
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();

        if (_changed)
        {
            SaveBookmarks();
        }
    }
}