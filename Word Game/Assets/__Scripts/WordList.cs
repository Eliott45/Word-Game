using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WordList : MonoBehaviour
{
    private static WordList S;

    [Header("Set in Inspector")]
    public TextAsset wordListText;
    public int numToParseBeforeYield = 10000;
    public int wordLenghtMin = 3;
    public int wordLenghtMax = 7;

    [Header("Set Dynamically")]
    public int currLine = 0;
    public int totalLines;
    public int longWordCount;
    public int wordCount;

    // Скрытые поля
    private string[] lines;
    private List<string> longWords;
    private List<string> words;

    private void Awake()
    {
        S = this;
    }

    private void Init()
    {
        lines = wordListText.text.Split('\n');
        totalLines = lines.Length;

        StartCoroutine(ParseLines());
    }

    static public void INIT()
    {
        S.Init();
    }

    /// <summary>
    /// Все сопрограммы возращают значение IEnumerator
    /// </summary>
    public IEnumerator ParseLines()
    {
        string word;
        // Инициализировать списко для хранения длиннейший слов из числа допустимых
        longWords = new List<string>();
        words = new List<string>();

        for (currLine = 0; currLine < totalLines; currLine++)
        {
            word = lines[currLine];

            // Если длина слова равна wordLenght
            if (word.Length == wordLenghtMax)
            {
                longWords.Add(word);
            }


            // Если длина слова равна wordLenghtMax добавить его в список допустимых слов
            if (word.Length >= wordLenghtMin && word.Length <= wordLenghtMax)
            {
                words.Add(word);
            }

            // Определить, не пора ли сделать перерыв 
            if(currLine % numToParseBeforeYield == 0)
            {
                // Подсчитать слова в каждом списке, чтобы показать, как протекает процесс анализа
                longWordCount = longWords.Count;
                wordCount = words.Count;
                // Приостановить выполнение сопрограммы до следующего кадра
                yield return null;

                // Инструкция yield приостановит выполнение этого метода, даст возможность выполниться другому коду и возобновит выполнение сопрограммы с этой точки, начав слудующую итерациию цикла for
            }
        }
        longWordCount = longWords.Count;
        wordCount = words.Count;

        // Послать игровому объекту gameObject сообщение об окончании анализа
        gameObject.SendMessage("WordListParseComplete");
    }

    // Эти методы позволяют другим классам обращаться к скрытым полям List<string>
    static public List<string> GET_WORDS()
    {
        return (S.words);
    }

    static public string GET_WORD(int ndx)
    {
        return ( S.words[ndx] );
    }

    static public List<string> GET_LONG_WORDS()
    {
        return (S.longWords);
    }
    static public string GET_LONG_WORD(int ndx)
    {
        return (S.longWords[ndx]);
    }

    static public int WORD_COUNT
    {
        get { return S.wordCount; }
    }

    static public int LONG_WORD_COUNT
    {
        get { return S.longWordCount; }
    }

    static public int NUM_TO_PARSE_BEFORE_YIELD
    {
        get { return S.numToParseBeforeYield; }
    }

    static public int WORD_LENGHT_MIN
    {
        get { return S.wordLenghtMin; }
    }

    static public int WORD_LENGHT_MAX
    {
        get { return S.wordLenghtMax; }
    }
}
