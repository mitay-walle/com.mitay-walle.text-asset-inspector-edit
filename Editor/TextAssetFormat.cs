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

        public TextAssetFormatter CreateFormatter(TextAssetEditor editor) => new TextAssetFormatter(this, editor);
    }

    public class TextAssetFormatter
    {
        string undoName;
        TextAssetEditor editor;
        TextAssetFormat format;
        TextField field;

        public TextAssetFormatter(TextAssetFormat format, TextAssetEditor editor)
        {
            this.editor = editor;
            this.format = format;
            undoName = $"edit TextAsset {editor.target.name}";
        }

        public void CreateInspectorGUI(VisualElement root)
        {
            var layout = new VerticalLayout();
            root.Add(layout);
            var lineNumbers = new Label("Line Numbers");
            layout.Add(lineNumbers);
            Rect rect = lineNumbers.layout;
            rect.width = 10;
            lineNumbers.style.maxWidth = new StyleLength(45);

            field = new TextField()
            {
                multiline = true,
                isDelayed = true
            };

            field.SetValueWithoutNotify((editor.target as TextAsset).text);

            field.labelElement.visible = false;

            field.RegisterCallback<ChangeEvent<string>>(OnChangeEvent);
            layout.Add(field);

            Undo.undoRedoEvent -= UndoRedoCallback;
            Undo.undoRedoEvent += UndoRedoCallback;

            var button = new Button(Repaint);
            button.text = "Repaint";

            editor.OnNeedRepaint -= Repaint;
            editor.OnNeedRepaint += Repaint;
        }

        void UndoRedoCallback(in UndoRedoInfo undo)
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

        void OnChangeEvent(ChangeEvent<string> evt)
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

        public void Repaint()
        {
            string path = AssetDatabase.GetAssetPath(editor.target);
            AssetDatabase.ImportAsset(path);
            field.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<TextAsset>(path).text);
        }
    }

    public class VerticalLayout : VisualElement
    {
        public VerticalLayout()
        {
            name = nameof(VerticalLayout);
            style.flexDirection = FlexDirection.Column;
            style.flexGrow = 1;
        }
    }
}
