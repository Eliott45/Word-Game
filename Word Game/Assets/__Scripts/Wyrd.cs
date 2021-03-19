using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wyrd 
{
    public string str; // Строковое представление слова
    public List<Letter> letters = new List<Letter>();
    public bool found = false; // Получит true, если игрок нашел это слово

    /// <summary>
    /// Свойство, управляющиее видимостью компонента 3D Text каждой плиткии Letter
    /// </summary>
    public bool visible
    {
        get
        {
            if (letters.Count == 0) return false;
            return (letters[0].visible);
        }
        set
        {
            foreach(Letter l in letters)
            {
                l.visible = value;
            }
        }
    }
    
    /// <summary>
    /// Свойство для назначения цвета каждой плитке Letter
    /// </summary>
    public Color color
    {
        get
        {
            if (letters.Count == 0) return (Color.black);
            return (letters[0].color);
        }
        set
        {
            foreach(Letter l in letters)
            {
                l.color = value;
            }
        }
    }

    /// <summary>
    /// Добавляет плитку в список letters
    /// </summary>
    public void Add(Letter l)
    {
        letters.Add(l);
        str += l.c.ToString();
    }
}
