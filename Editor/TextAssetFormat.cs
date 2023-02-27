using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TextAssetInspectorEdit.Editor
{
    [Serializable]
    public class TextAssetFormat
    {
        public string FileExtension;
        public TextAssetReplaceData[] ReplaceData;

        public TextAssetFormat()
        {
        }

        public TextAssetFormat(string fileExtension)
        {
            FileExtension = fileExtension;
        }

        public bool ExtensionMatch(string extension)
        {
            return string.Equals(extension, FileExtension, StringComparison.InvariantCultureIgnoreCase);
        }

        public void CreateInspectorGUI(VisualElement root, TextAssetEditor editor)
        {
            TextField field = new TextField()
            {
                multiline = true,
                isDelayed = true
            };

            field.SetValueWithoutNotify((editor.target as TextAsset).text);

            field.labelElement.visible = false;

            void OnChangeEventLocal(ChangeEvent<string> evt)
            {
                OnChangeEvent(evt, editor, field);
            }

            field.RegisterCallback<ChangeEvent<string>>(OnChangeEventLocal);
            root.Add(field);
        }

        void OnChangeEvent(ChangeEvent<string> evt, TextAssetEditor editor, TextField field)
        {
            string path = AssetDatabase.GetAssetPath(editor.target);
            File.WriteAllText(path, evt.newValue);
            AssetDatabase.ImportAsset(path);

            field.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<TextAsset>(path).text);
        }
    }
}
