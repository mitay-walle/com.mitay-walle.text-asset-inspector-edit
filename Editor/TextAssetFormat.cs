using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
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
        TextField fieldFiltered;
        Label numbers;

        public TextAssetFormatter(TextAssetFormat format, TextAssetEditor editor)
        {
            this.editor = editor;
            this.format = format;
            undoName = $"edit TextAsset {editor.target.name}";
        }

        public void CreateInspectorGUI(VisualElement root)
        {
            ToolbarSearchField search = new ToolbarSearchField();
            root.Add(search);
            search.RegisterCallback<ChangeEvent<string>>(Search);
            HorizontalLayout layout = new HorizontalLayout();
            root.Add(layout);
            numbers = new Label("Line Numbers");
            layout.Add(numbers);
            Rect rect = numbers.layout;
            rect.width = 10;
            numbers.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.UpperRight);
            numbers.enableRichText = true;

            fieldFiltered = new TextField()
            {
                multiline = true,
                isDelayed = true
            };
            fieldFiltered.labelElement.visible = false;

            field = new TextField()
            {
                multiline = true,
                isDelayed = true
            };

            string text = (editor.target as TextAsset).text;

            field.SetValueWithoutNotify(text);
            field.labelElement.visible = false;
            fieldFiltered.labelElement.visible = false;
            field.RegisterCallback<ChangeEvent<string>>(OnChangeEvent);
            var box = new Box();
            box.Add(field);
            box.Add(fieldFiltered);
            layout.Add(box);

            Undo.undoRedoEvent -= UndoRedoCallback;
            Undo.undoRedoEvent += UndoRedoCallback;

            ValidateNumbers(text);

            editor.OnNeedRepaint -= Repaint;
            editor.OnNeedRepaint += Repaint;
        }

        void Search(ChangeEvent<string> evt)
        {
            string filter = evt.newValue.Trim();

            if (string.IsNullOrEmpty(filter))
            {
                field.visible = true;
                field.BringToFront();
                fieldFiltered.visible = false;
            }
            else
            {
                field.visible = false;
                fieldFiltered.visible = true;
                field.BringToFront();
            }

            if (fieldFiltered.visible)
            {
                string text = (editor.target as TextAsset).text;
                string[] lines = text.Split('\n').ToArray();

                int count = lines.Length;
                List<int> indexies = new List<int>();

                for (int i = 0; i < count; i++)
                {
                    if (lines[i].Contains(filter))
                    {
                        indexies.Add(i);
                    }
                }
                text = string.Join('\n', lines.Where(line => line.Contains(filter)));

                fieldFiltered.SetValueWithoutNotify(text);
                ValidateNumbers(indexies);
            }
            else
            {
                ValidateNumbers(field.value);
            }
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
            string text = AssetDatabase.LoadAssetAtPath<TextAsset>(path).text;
            field.SetValueWithoutNotify(text);
            ValidateNumbers(evt.newValue);
        }

        private void ValidateNumbers(string text)
        {
            int count = text.Split('\n').Length;
            int[] indexies = new int[count];
            for (int i = 0; i < count; i++)
            {
                indexies[i] = i;
            }
            ValidateNumbers(indexies);
        }

        private void ValidateNumbers(IEnumerable<int> indexies)
        {
            if (indexies.Count() == 0)
            {
                return;
            }
            numbers.text = $"<mspace=0.7em>{string.Join(":\n", indexies)}:\n";
            numbers.style.maxWidth = (indexies.Max().ToString().Length + 1) * 10;
        }

        public void Repaint()
        {
            string path = AssetDatabase.GetAssetPath(editor.target);
            AssetDatabase.ImportAsset(path);
            field.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<TextAsset>(path).text);
        }
    }

    public class HorizontalLayout : VisualElement
    {
        public HorizontalLayout()
        {
            name = nameof(HorizontalLayout);
            style.flexDirection = FlexDirection.Row;
            style.flexGrow = 1;
        }
    }
}
