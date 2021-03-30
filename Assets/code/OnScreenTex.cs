using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnScreenTex : MonoBehaviour
{
    public Texture2D texture;
    // Use this for initialization
    void OnGUI()
    {
        Rect rect = new Rect(0,0,Screen.width, Screen.height);

        GUI.DrawTexture(rect, texture);
    }
}
