using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TextAssetInspectorEdit.Editor
{
    [CustomEditor(typeof(TextAsset))]
    public class TextAssetEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
        }

        public override VisualElement CreateInspectorGUI()
        {
            var extension = Path.GetExtension(AssetDatabase.GetAssetPath(target));

            VisualElement root = new VisualElement();

            
            if (string.IsNullOrEmpty(extension))
            {
                root.Add(new IMGUIContainer(base.OnInspectorGUI));
            }
            else
            {
                foreach (TextAssetFormat format in TextAssetFormats.AllFormats)
                {
                    if (format.ExtensionMatch(extension))
                    {
                        format.CreateInspectorGUI(root,this);
                        break;
                    }
                }
            }
            return root;
        }
    }
}
