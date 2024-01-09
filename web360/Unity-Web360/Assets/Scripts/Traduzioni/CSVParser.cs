using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CSVParser : MonoBehaviour
{
    static List<string> languageList = new List<string>();
    static Dictionary<string, List<string>> languageDictionary = new Dictionary<string, List<string>>();

    static public string[] SplitLine(string line)
    {
        return line.Split(",");
        return (from System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(line,
            @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)",
            System.Text.RegularExpressions.RegexOptions.ExplicitCapture)
                select m.Groups[1].Value).ToArray();
    }

    static public List<string> GetAvailableLanguages()
    {
        if (languageList.Count == 0)
        {
            var cvsFile = Resources.Load<TextAsset>("Localization");
            string[] lines = cvsFile.text.Split("\n"[0]);
            languageList = new List<string>(SplitLine(lines[0]));
            languageList.RemoveAt(0);
        }

        return languageList;
    }

    static public string GetTextFromID(string Id, int languageIndex)
    {
        if (languageDictionary.Count == 0)
        {
            var cvsFile = Resources.Load<TextAsset>("Localization");
            string[] lines = cvsFile.text.Split("\n"[0]);

            for (int i = 1; i < lines.Length; i++)
            {
                string[] col = SplitLine(lines[i]);

                if (col.Length > 1)
                {
                    List<string> words = new List<string>(col);
                    words.RemoveAt(0);
                    languageDictionary.Add(col[0], words);
                }
            }
        }

        var values = languageDictionary[Id];
        return values[languageIndex];
    }
}
