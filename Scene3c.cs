using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene3c : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // UIControllerにUI表示を命令
        SceneManager.Instance.ShowUIPrompt(UIController.UIScreenID.Scene3c_Happy); 
    }

    // Update is called once per frame
    void Update()
    {

    }
}