using System.Collections.Generic;

namespace TextAssetInspectorEdit.Editor
{
    public static class TextAssetFormats
    {
        public static TextAssetFormat MD = new TextAssetFormat(fileExtension: ".md")
        {
            ReplaceData = new[]
            {
                new TextAssetReplaceData("", "")
            }
        };
        public static List<TextAssetFormat> AllFormats = new List<TextAssetFormat>() { MD };
    }
}
