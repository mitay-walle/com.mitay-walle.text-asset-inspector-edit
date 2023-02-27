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

            void UndoRedoCallbackLocal(in UndoRedoInfo undo)
            {
                UndoRedoCallback(undo, $"edit TextAsset {editor.target.name}", editor, field);
            }
            Undo.undoRedoEvent -= UndoRedoCallbackLocal;
            Undo.undoRedoEvent += UndoRedoCallbackLocal;

            var button = new Button(RepaintLocal);
            button.text = "Repaint";
            root.Add(button);

            void RepaintLocal()
            {
                Repaint(editor, field);
            }

            editor.OnNeedRepaint -= RepaintLocal;
            editor.OnNeedRepaint += RepaintLocal;
        }

        void UndoRedoCallback(in UndoRedoInfo undo, string undoName, TextAssetEditor editor, TextField field)
        {
            if (undo.undoName == undoName)
            {
                string path = AssetDatabase.GetAssetPath(editor.target);

                string text = TextAssetEditor.SaveUndoContext.SavedText;
                field.value = text;
                File.WriteAllText(path, text);
                EditorUtility.SetDirty(editor.target);
                AssetDatabase.ImportAsset(path);
            }
        }

        void OnChangeEvent(ChangeEvent<string> evt, TextAssetEditor editor, TextField field)
        {
            TextAssetEditor.SaveUndoContext.SavedText = (editor.target as TextAsset).text;
            Undo.RecordObject(TextAssetEditor.SaveUndoContext, $"edit TextAsset {editor.target.name}");
            string path = AssetDatabase.GetAssetPath(editor.target);
            File.WriteAllText(path, evt.newValue);
            EditorUtility.SetDirty(editor.target);
            EditorUtility.SetDirty(TextAssetEditor.SaveUndoContext);
            AssetDatabase.ImportAsset(path);
            field.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<TextAsset>(path).text);
        }

        public void Repaint(TextAssetEditor editor, TextField field)
        {
            string path = AssetDatabase.GetAssetPath(editor.target);
            AssetDatabase.ImportAsset(path);
            field.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<TextAsset>(path).text);
        }
    }
}
