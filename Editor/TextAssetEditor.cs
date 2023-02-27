using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TextAssetInspectorEdit.Editor
{

    [CustomEditor(typeof(TextAsset))]
    public class TextAssetEditor : UnityEditor.Editor
    {
        public static SaveUndoContext SaveUndoContext;
        private string _assetGuid;
        private Hash128 _lastDependencyHash;
        private MethodInfo _method;
        private object[] _arguments = new object[1];
        public event Action OnNeedRepaint;

        private void OnEnable()
        {
            SaveUndoContext ??= CreateInstance<SaveUndoContext>();
            _assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(target as TextAsset));
            _method = typeof(AssetDatabase).GetMethod("GetSourceAssetFileHash", BindingFlags.Static | BindingFlags.NonPublic);
            _arguments[0] = _assetGuid;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var extension = Path.GetExtension(AssetDatabase.GetAssetPath(target));

            VisualElement root = new VisualElement();

            if (targets.Length > 1 || string.IsNullOrEmpty(extension))
            {
                root.Add(new IMGUIContainer(base.OnInspectorGUI));
            }
            else
            {
                // var saveContextField = new ObjectField("Save Undo Context");
                // saveContextField.objectType = typeof(SaveUndoContext);
                // saveContextField.value = SaveUndoContext;
                // root.Add(saveContextField);
                root.Add(new IMGUIContainer(() =>
                {
                    Hash128 sourceAssetFileHash = (Hash128)_method.Invoke(null, _arguments);
                    if (_lastDependencyHash != sourceAssetFileHash)
                    {
                        _lastDependencyHash = sourceAssetFileHash;
                        OnNeedRepaint?.Invoke();
                    }
                }));

                
                foreach (TextAssetFormat format in TextAssetFormats.AllFormats)
                {
                    if (format.ExtensionMatch(extension))
                    {
                        format.CreateFormatter(this).CreateInspectorGUI(root);
                        break;
                    }
                }
            }
            return root;
        }
    }
}
