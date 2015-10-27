using UnityEngine;
using System.Collections;

public class LoadOnClick : MonoBehaviour {

	public void LoadScene(int level)
    {
        PlayerPrefs.SetInt("score", 0);
        PlayerPrefs.SetInt("lives", 2);

        float speed = 0.008f;
        int enemies = 3;
        if(level == 2)
        {
            speed = 0.015f;
            enemies = 5;
        }

        PlayerPrefs.SetFloat("enemySpeed", speed);
        PlayerPrefs.SetInt("enemies", enemies);
        PlayerPrefs.SetInt("level", level);

        Application.LoadLevel(level);
    }
}
