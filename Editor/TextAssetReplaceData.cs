using System;

namespace TextAssetInspectorEdit.Editor
{
    [Serializable]
    public class TextAssetReplaceData
    {
        public string Before;
        public string After;

        public TextAssetReplaceData()
        {
        }

        public TextAssetReplaceData(string before, string after)
        {
            Before = before;
            After = after;
        }
    }
}
