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

    [Header("Set in Inspector")]
    public GameObject prefabLetter;
    public Rect wordArea = new Rect(-24, 19, 48, 28);
    public float letterSize = 1.5f;
    public bool showAllWyrds = true; 

    [Header("Set Dynamically")]
    public GameMode mode = GameMode.preGame;
    public WordLevel currLevel;
    public List<Wyrd> wyrds;

    private Transform letterAnchor, bigLetterAnchor;

    private void Awake()
    {
        S = this;
        letterAnchor = new GameObject("LetterAnchor").transform;
        bigLetterAnchor = new GameObject("BigLetterAnchor").transform;
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
        level.word = WordList.GET_LONG_WORD(level.longWordIndex); //
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
        level.subWords = SortWordsByLength(level.subWords).ToList();

        SubWordSearchComplete();
    }

    /// <summary>
    /// Использует LINQ для сортировки массива и возвращает его копию
    /// </summary>
    public static IEnumerable<string> SortWordsByLength(IEnumerable<string> ws)
    {
        ws = ws.OrderBy(s => s.Length);
        return ws;
    }

    public void SubWordSearchComplete()
    {
        mode = GameMode.levelPrep;
        Layout();
    }

    void Layout()
    {
        // Поместить на экран плитки с буквами каждого возможного слов текущего
        wyrds = new List<Wyrd>();

        // Объявть локальные переменные, которые будут использоваться методом
        GameObject go;
        Letter lett;
        string word;
        Vector3 pos;
        float left = 0;
        float columnWidth = 3;
        char c;
        Color col;
        Wyrd wyrd;

        // Определить, сколько рядов плиток уместится на экране
        int numRows = Mathf.RoundToInt(wordArea.height / letterSize);

        // Создать экземпляр Wyrd для каждого слова в level.subWords
        for (int i = 0; i < currLevel.subWords.Count; i++)
        {
            wyrd = new Wyrd();
            word = currLevel.subWords[i];

            // Если слово длинне, чем columnWidth, развернуть его
            columnWidth = Mathf.Max(columnWidth, word.Length);

            // Создать экземпляр PrefabLetter для каждой буквы в слове
            for (int j = 0; j < word.Length; j++)
            {
                c = word[j]; // Получить j-й символ слова
                go = Instantiate<GameObject>(prefabLetter);
                go.transform.SetParent(letterAnchor);
                lett = go.GetComponent<Letter>();
                lett.c = c; // назначить букву плитке 

                // Установить координаты плитки Letter
                pos = new Vector3(wordArea.x + left + j * letterSize, wordArea.y, 0);

                // Выстроить плитку по вертикали
                pos.y -= (i % numRows) * letterSize;

                lett.pos = pos;

                go.transform.localScale = Vector3.one * letterSize;
                wyrd.Add(lett);
            }

            if (showAllWyrds) wyrd.visible = true;

            wyrds.Add(wyrd);

            // Если достигнут последний ряд в столбце, начать новый столбей
            if(i % numRows == numRows-1)
            {
                left += (columnWidth + 0.5f) * letterSize;
            }
        }
    }
}
