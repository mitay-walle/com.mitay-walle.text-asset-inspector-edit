using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TextAsset))]
public class TextAssetEditor : Editor
{

    public override void OnInspectorGUI()
    {
        string path = AssetDatabase.GetAssetPath(target);

        if (string.IsNullOrEmpty(path)) { return; }

        if (path.EndsWith(".md"))
        {
            MdInspectorGUI(path);
        }
        else
        {
            base.OnInspectorGUI();
        }
    }

    private static void MdInspectorGUI(string path)
    {
        EditorStyles.label.wordWrap = true;
        EditorStyles.label.richText = true;

        // Handle any problems that might arise when reading the text
        try
        {
            // Create a new StreamReader, tell it which file to read and what encoding the file was saved as
            // Immediately clean up the reader after this block of code is done.
            // You generally use the "using" statement for potentially memory-intensive objects
            // instead of relying on garbage collection.
            // (Do not confuse this with the using directive for namespace at the  beginning of a class!)
            using (var reader = new StreamReader(path))
                {
                    // While there's lines left in the text file, do this:
                    string line;
                    do
                    {
                        line = reader.ReadLine();

                        if (line != null)
                        {
                            //line = CommonMark.CommonMarkConverter.Convert(line);
                            CleanUpHtml(line);
                        }
                    }
                    while (line != null);

                    // Done reading, close the reader and return true to broadcast success
                    reader.Close();
                }
            }
            // If anything broke in the try block, we throw an exception with information on what didn't work
            catch (Exception e)
            {
                Debug.LogErrorFormat("{0}\n", e.Message);
            }
        }

        private static void CleanUpHtml(string line)
        {
            if (line.Contains("<hr />"))
            {
                EditorGUILayout.Separator();
                return;
            }

            line = line.Replace("&quot;", "\"");
            line = line.Replace("&bull;", "•");
            line = line.Replace("&trade;", "™");
            line = line.Replace("&copy;", "Ⓒ");
            line = line.Replace("&sum;", "∑");
            line = line.Replace("&prod;", "∏");
            line = line.Replace("&ni;", "∋");
            line = line.Replace("&notin;", "∉");
            line = line.Replace("&isin;", "∈");
            line = line.Replace("&nabla;", "∇");
            line = line.Replace("&empty;", "∅");
            line = line.Replace("&exist;", "∃");
            line = line.Replace("&part;", "∂");
            line = line.Replace("&forall;", "∀");
            line = line.Replace("&forall;", "Δ");

            // <h1> </h1> etc...
            line = line.Replace("<h1>", "<size=18>");
            line = line.Replace("</h1>", "</size>");

            line = line.Replace("<p>", "");
            line = line.Replace("</p>", "\n");

            line = line.Replace("<em>", "<i>");
            line = line.Replace("</em>", "</i>");

            // <strong> </strong>
            line = line.Replace("<strong>", "<b>");
            line = line.Replace("</strong>", "</b>");

            // <ul>
            // <li> </li>
            // </ul>

            // <ol>
            // <li> </li>
            // </ol>

            // <a herf=" ... ">

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(line);
            EditorGUILayout.EndHorizontal();
        }
    }