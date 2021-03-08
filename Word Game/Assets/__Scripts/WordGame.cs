using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum GameMode
{
    preGame, // Перед началом игры
    loading, // Список слов загружается и анализируется
    makeLevel, // Создается отдельный WordLevel
    levelPrep, // Создается уровень с визуальным представлением
    inLevel // Уровень запущен
}

public class WordGame : MonoBehaviour
{
    static public WordGame S; // Одиночка

    [Header("Set Dynamically")]
    public GameMode mode = GameMode.preGame;
    public WordLevel currLevel;

    private void Awake()
    {
        S = this;
    }

    private void Start()
    {
        mode = GameMode.loading;
        WordList.INIT();
    }

    public void WordListParseComplete()
    {
        mode = GameMode.makeLevel;

        // Создать уровень и сохранить в currLevel текущий WordLevel
        currLevel = MakeWordLevel();
    }

    public WordLevel MakeWordLevel(int levelNum = -1)
    {
        WordLevel level = new WordLevel();
        if(levelNum == -1)
        {
            // Выбрать случайный уровень
            level.longWordIndex = Random.Range(0, WordList.LONG_WORD_COUNT);
        } else
        {

        }
        level.levelNum = levelNum;
        level.word = WordList.GET_LONG_WORD(level.longWordIndex);
        level.charDict = WordLevel.MakeCharDict(level.word);

        StartCoroutine(FindSubWordsCoroutine(level));
        return level;
    }

    /// <summary>
    /// Отыскивает слова, которые можно составить на этом уровне
    /// </summary>
    public IEnumerator FindSubWordsCoroutine(WordLevel level)
    {
        level.subWords = new List<string>();
        string str;

        List<string> words = WordList.GET_WORDS();

        // Выполнить обход всех слов в WordList
        for (int i = 0; i < WordList.WORD_COUNT; i++)
        {
            str = words[i];
            // Проверить, можно ли его составить из символов в level.charDict
            if(WordLevel.CheckWordInLevel(str, level)) { 
                level.subWords.Add(str);
            }
            // Приостановиться после анализа заданного числа слов в этом кадре
            if(i%WordList.NUM_TO_PARSE_BEFORE_YIELD == 0)
            {
                yield return null;
            }
        }

        level.subWords.Sort();
        level.subWords = SortWordByLength(level.subWords).ToList();

        SubWordSearchComplete();
    }

    /// <summary>
    /// Использует LINQ для сортировки массива и возвращает его копию
    /// </summary>
    public static IEnumerable<string> SortWordByLength(IEnumerable<string> ws)
    {
        ws = ws.OrderBy(s => s.Length);
        return ws;
    }

    public void SubWordSearchComplete()
    {
        mode = GameMode.levelPrep;
    }
}
