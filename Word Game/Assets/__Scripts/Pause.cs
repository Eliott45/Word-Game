﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    [Header("Set in Inspector")]
    public GameObject UIPause;

    [Header("Set Dynamically")]
    public GameObject music;
    public AudioSource musicVol;

    private bool see = false;
    private float volume;

    private void Start()
    {
        music = GameObject.Find("Music");
        musicVol = music.GetComponent<AudioSource>();
        volume = musicVol.volume;
    }

    private void Update()
    {
        musicVol.volume = volume;
        if (Input.GetKeyDown("escape"))
        {
            see = !see;
            UIPause.SetActive(see);
        }

    }

    public void SetValume(float aim)
    {
        volume = aim;
    }

    public void ToMenu()
    {
        Destroy(music);
        SceneManager.LoadScene("_WordGame_Menu");
    }

    public void Restart()
    {
        SceneManager.LoadScene("__WordGame_Scene_0");
        DontDestroyOnLoad(music);
    }

    
}






