using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WordLevel 
{
    public int levelNum;
    public int longWordIndex;
    public string word;
    // Словарь со всеми буквами в слове
    public Dictionary<char, int> charDict;
    // Все слова, которые можно составить из букв в charDict
    public List<string> subWords;

    /// <summary>
    /// Подсчитывает количество вхождений символов в строку и возращает словарь Dictionary <char, int> с информацией.
    /// </summary>
    static public Dictionary<char,int> MakeCharDict(string w)
    {
        Dictionary<char, int> dict = new Dictionary<char, int>();
        char c;
        for (int i = 0; i < w.Length; i++)
        {
            c = w[i];
            if(dict.ContainsKey(c))
            {
                dict[c]++;
            } else
            {
                dict.Add(c, 1);
            }
        }
        return (dict);
    }

    /// <summary>
    /// Проверяет возможность составить слово из символов в level.charDict
    /// </summary>
    public static bool CheckWordInLevel(string str, WordLevel level)
    {
        Dictionary<char, int> counts = new Dictionary<char, int>();
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            // Если charDict содержит символ с 
            if (level.charDict.ContainsKey(c))
            {
                // Если counts еще не содержит ключа с символом с
                if (!counts.ContainsKey(c))
                {
                    // Добавить новый ключ со значением 1
                    counts.Add(c, 1);
                } else
                {
                    // Или прибавить 1 к текущему значению
                    counts[c]++;
                }

                // Если число вхождений символа с в строку str превысило доступное количество в level.charDict
                if (counts[c] > level.charDict[c])
                {
                    return false;
                }
            } else
            {
                // Символ с отсутствует в level.word
                return false; 
            }
        }
        return true;
    }
}
