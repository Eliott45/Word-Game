using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter : MonoBehaviour
{
    [Header("Set Dynamically")]
    public TextMesh tMesh; // TextMesh отображает символ
    public Renderer tRend; // Компонент Rederer объекта 3D Text. Он будет определять видимость символа
    public bool big = false; // Большие и малые плитки действуют по-разному

    private char _c; // Символ, отображаемый на этой плитке
    private Renderer rend;

    private void Awake()
    {
        tMesh = GetComponentInChildren<TextMesh>();
        tRend = tMesh.GetComponent<Renderer>();
        rend = GetComponent<Renderer>();
        visible = false;
    }

    /// <summary>
    /// Для чтения/записи буквы в поле _с, отображаемой объектом 3D Text
    /// </summary>
    public char c
    {
        get { return (_c); }
        set
        {
            _c = value;
            tMesh.text = _c.ToString();
        }
    }


    /// <summary>
    /// Для чтения/записи буквы в поле _с в виде строки
    /// </summary>
    public string str
    {
        get
        {
            return (_c.ToString());
        }
        set
        {
            c = value[0];
        }
    }

    /// <summary>
    /// Разрешает или запрещеает отображение буквы
    /// </summary>
    public bool visible
    {
        get { return (tRend.enabled); }
        set { tRend.enabled = value; }
    }

    /// <summary>
    /// Свойство для чтения/записи цвета плитки
    /// </summary>
    public Color color
    {
        get { return (rend.material.color); }
        set { rend.material.color = value; }
    }

    /// <summary>
    /// Свойство для чтения/записи координат плитки
    /// </summary>
    public Vector3 pos
    {
        set
        {
            transform.position = value;
        }
    }
}
