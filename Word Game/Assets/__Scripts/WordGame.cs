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
    public float bigLetterSize = 4f;
    public Color bigColorDim = new Color(0.8f, 0.8f, 0.8f);
    public Color bigColorSelected = new Color(1f, 0.9f, 0.7f);
    public Vector3 bigLetterCenter = new Vector3(0, -16, 0);
    public Color[] wyrdPalette; 

    [Header("Set Dynamically")]
    public GameMode mode = GameMode.preGame;
    public WordLevel currLevel;
    public List<Wyrd> wyrds;
    public List<Letter> bigLetters;
    public List<Letter> bigLettersActive;
    public string testWord;
    public string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

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

                // Переместить плитку lett немедленно за верхний край экрана 
                lett.posImmediate = pos + Vector3.up * (20 + i % numRows);

                // Затем начать ее перемещение в новую позицию
                lett.pos = pos;
                // Увеличить lett.timeStart для перемещения слов в разные времена
                lett.timeStart = Time.time + i * 0.05f;

                go.transform.localScale = Vector3.one * letterSize;
                wyrd.Add(lett);
            }

            if (showAllWyrds) wyrd.visible = true;

            // Определить цвет слова, исходя из его длины

            wyrd.color = wyrdPalette[word.Length - WordList.WORD_LENGHT_MIN];
            wyrds.Add(wyrd);

            // Если достигнут последний ряд в столбце, начать новый столбей
            if(i % numRows == numRows-1)
            {
                left += (columnWidth + 0.5f) * letterSize;
            }
        }

        // Поместить на экран большие плитки с буквами
        // Инициализировать список больших букв
        bigLetters = new List<Letter>();
        bigLettersActive = new List<Letter>();

        // Создать большую плитку для каждой буквы в целевом слове
        for (int i = 0; i < currLevel.word.Length; i++)
        {
            // Напоминает процедурур создания маленьких плиток 
            c = currLevel.word[i];
            go = Instantiate<GameObject>(prefabLetter);
            go.transform.SetParent(bigLetterAnchor);
            lett = go.GetComponent<Letter>();
            lett.c = c;
            go.transform.localScale = Vector3.one * bigLetterSize;

            // Первоначально поместить большие плитки ниже края экрана
            pos = new Vector3(0, -100, 0);
            lett.posImmediate = pos;
            lett.pos = pos;
            lett.timeStart = Time.time + currLevel.subWords.Count * 0.05f;
            lett.easingCuve = Easing.Sin + "-0.18";
            col = bigColorDim;
            lett.color = col;
            lett.visible = true;
            lett.big = true;
            bigLetters.Add(lett);
        }

        // Перемешать плитки
        bigLetters = ShuffleLetters(bigLetters);

        // Вывести на экран
        ArrangeBigLetters();

        // Установить режим mode -- "в игре"
        mode = GameMode.inLevel;
    }

    /// <summary>
    /// Перемешивает элементы в списке и вовращает результат
    /// </summary>
    List<Letter> ShuffleLetters(List<Letter> letts)
    {
        List<Letter> newL = new List<Letter>();
        int ndx;
        while(letts.Count > 0) {
            ndx = Random.Range(0, letts.Count);
            newL.Add(letts[ndx]);
            letts.RemoveAt(ndx);
        }
        return (newL);
    }

    /// <summary>
    /// Выводит большие плитки на экран
    /// </summary>
    void ArrangeBigLetters()
    {
        // Найти середину для вывода ряда больших плиток с центрированием по горизонтали
        float halfWidth = ((float)bigLetters.Count) / 2f - 0.5f;
        Vector3 pos;
        for (int i = 0; i < bigLetters.Count; i++)
        {
            pos = bigLetterCenter;
            pos.x += (i - halfWidth) * bigLetterSize;
            bigLetters[i].pos = pos;
        }

        halfWidth = ((float)bigLettersActive.Count) / 2f - 0.5f;
        for (int i = 0; i< bigLettersActive.Count; i++)
        {
            pos = bigLetterCenter;
            pos.x += (i - halfWidth) * bigLetterSize;
            pos.y += bigLetterSize * 1.25f;
            bigLettersActive[i].pos = pos;
        }
    }

    private void Update()
    {
        // Объявить пару вспомогательных переменных
        Letter ltr;
        char c;

        switch(mode)
        {
            case GameMode.inLevel:
                // Выполнить обход всех символов, введенных игроком в этом кадре
                foreach (char cIt in Input.inputString)
                {
                    // Преобразовать в верхний регистр
                    c = System.Char.ToUpperInvariant(cIt);

                    // Проверить, есть ли такая буква верхнего регистра
                    if (upperCase.Contains(c))
                    {
                        // Найти доступную плитку с этой буквой 
                        ltr = FindNextLetterByChar(c);
                        // Если плитка найдена 
                        if (ltr != null)
                        {
                            // Добавить этот символ в testWord и переместить соответствующую плитку Letter в bigLettersActive
                            testWord += c.ToString();
                            // Переместить из списка неактивных в список активных плиток
                            bigLettersActive.Add(ltr);
                            bigLetters.Remove(ltr);
                            ltr.color = bigColorSelected; // Придать плитке активный вид 
                            ArrangeBigLetters(); // Отобразить плитки
                        }
                    }

                    if (c == '\b') // Backspace
                    {
                        // Удалить последнюю плитку Letter из bigLetterActive
                        if (bigLettersActive.Count == 0) return;
                        if (testWord.Length > 1)
                        {
                            // Удалить последнюю букву из testWord
                            testWord = testWord.Substring(0, testWord.Length - 1);
                        } else
                        {
                            testWord = "";
                        }

                        ltr = bigLettersActive[bigLettersActive.Count - 1];
                        // Переместить из списка активных в список неактивных плиток
                        bigLettersActive.Remove(ltr);
                        bigLetters.Add(ltr);
                        ltr.color = bigColorDim; // Придать плитке неактиный вид
                        ArrangeBigLetters(); // Отобразить плитки
                    }

                    if (c == '\n' || c == '\r') // Return/Enter
                    {
                        // Проверить наличие сконструированного слова в WordLevel
                        CheckWord();
                    }

                    if(c == ' ') // Space
                    {
                        // Перемешать плитки в bigLetters
                        bigLetters = ShuffleLetters(bigLetters);
                        ArrangeBigLetters();
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Этот метод отыскивает плитку Letter с символом с в bigLetters
    /// </summary>
    Letter FindNextLetterByChar(char c)
    {
        // Проверить каждую плитку Letter в bigLetters
        foreach (Letter ltr in bigLetters)
        {
            // Если содержит тот же символ, что указан в с
            if(ltr.c == c)
            {
                // то вернуть его
                return (ltr);
            }
        }
        return null; // Иначе
    }

    public void CheckWord()
    {
        // Проверяет присутвие слова testWord в списке level.subWords
        string subWord;
        bool foundTestWord = false;

        // Создать списко для хранения индексов других слов, присутствующих в testWord
        List<int> containedWords = new List<int>();

        // Обойти все слова в currLevel.subWords
        for(int i = 0; i < currLevel.subWords.Count; i++)
        {
            // Проверить, было ли уже найдено Wyrd 
            if (wyrds[i].found)
            {
                continue;
            }

            subWord = currLevel.subWords[i];
            // Проверить, входит ли это слово subWord в testWord
            if (string.Equals(testWord,subWord))
            {
                HighlightWyrd(i);
                ScoreManager.SCORE(wyrds[i], 1); // Подситать очки
                foundTestWord = true;
            } else if (testWord.Contains(subWord))
            {
                containedWords.Add(i);
            }
        }

        if (foundTestWord) // Если проверяемое слово присутствует в списке подсветить другие слова, содержащиеся в testWord
        {
            int numContained = containedWords.Count;
            int ndx;
            // Подсвечивать слова в обратном порядке
            for (int i = 0; i < containedWords.Count; i++)
            {
                ndx = numContained - i - 1;
                HighlightWyrd(containedWords[ndx]);
                ScoreManager.SCORE(wyrds[containedWords[ndx]], i + 2);
            }
        }

        // Очистить список актиных плиток Letters
        ClearBigLettersActive();
    }
    
    /// <summary>
    /// Подвечивает экземпляр Wyrd
    /// </summary>
    void HighlightWyrd(int ndx)
    {
        // Активировать слово
        wyrds[ndx].found = true; // Установить признак, что оно найдено
        // Выделить цветом
        wyrds[ndx].color = (wyrds[ndx].color + Color.white) / 2f;
        wyrds[ndx].visible = true; // Сделать компонент 3D text видимым
    }

    /// <summary>
    /// Удаляет все плитки Letter из bigLettersActive
    /// </summary>
    void ClearBigLettersActive()
    {
        testWord = ""; // Очистить 
        foreach (Letter ltr in bigLettersActive)
        {
            bigLetters.Add(ltr); // Добавить каждую плитку в bigLetters
            ltr.color = bigColorDim; // Придать ей неактивный вид
        }
        bigLettersActive.Clear(); // Очистить список
        ArrangeBigLetters(); // Повторно вывести плитки на экран
    }
}
