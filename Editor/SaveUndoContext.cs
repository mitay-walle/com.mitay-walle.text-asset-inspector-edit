using UnityEngine;

namespace TextAssetInspectorEdit.Editor
{
    public class SaveUndoContext : ScriptableObject
    {
        [TextArea]
        public string SavedText;
    }
}
