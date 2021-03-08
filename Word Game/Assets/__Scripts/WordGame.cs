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
    }
}
