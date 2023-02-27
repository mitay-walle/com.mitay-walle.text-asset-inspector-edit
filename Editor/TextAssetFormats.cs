using System.Collections.Generic;

namespace TextAssetInspectorEdit.Editor
{
    public static class TextAssetFormats
    {
        public static TextAssetFormat MD = new TextAssetFormat(fileExtension: ".md");
        public static List<TextAssetFormat> AllFormats = new List<TextAssetFormat>() { MD };
    }
}
