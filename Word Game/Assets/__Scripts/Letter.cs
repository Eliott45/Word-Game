using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float timeDuration = 0.5f;
    public string easingCuve = Easing.InOut;

    [Header("Set Dynamically")]
    public TextMesh tMesh; // TextMesh отображает символ
    public Renderer tRend; // Компонент Rederer объекта 3D Text. Он будет определять видимость символа
    public bool big = false; // Большие и малые плитки действуют по-разному

    // Поля для линейной иинтерполяции
    public List<Vector3> pts = null;
    public float timeStart = -1;

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
    /// Настраивает кривую Безье для плавного переммещения в новые координаты
    /// </summary>
    public Vector3 pos
    {
        set
        {
            // transform.position = value;

            // Найти среднюю точку на случайном расстоянии от фактической средней точки между текущей и новой (valve) позициями

            Vector3 mid = (transform.position + value) / 2f;

            // Случайное расстояние не превышает 1/4 расстояния до фактической средней точки
            float mag = (transform.position - value).magnitude;
            mid += Random.insideUnitSphere * mag * 0.25f;

            // Создать List<Vector3> точек, определяющих кривую Безье
            pts = new List<Vector3>() { transform.position, mid, value };

            // Если timeStart содержит значение по умолчанию -1, установить текущее времмя
            if (timeStart == -1) timeStart = Time.time;
        }
    }

    /// <summary>
    /// Немедленно перемещает в новую позицию
    /// </summary>
    public Vector3 posImmediate
    {
        set
        {
            transform.position = value;
        }
    }

    private void Update()
    {
        if (timeStart == -1) return;

        // Стандартная линейная интерполяция
        float u = (Time.time - timeStart) / timeDuration;
        u = Mathf.Clamp01(u);
        float u1 = Easing.Ease(u, easingCuve);
        Vector3 v = Utils.Bezier(u1, pts);
        transform.position = v;

        // Если интерполяция закончена, запись -1 в timeStart
        if (u == 1) timeStart = -1;
    }
}
