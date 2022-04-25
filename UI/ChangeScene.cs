using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//Button name has to be same name of loaded scene


public class ChangeScene : MonoBehaviour {

	public string SceneToLoad;
	private Button btn;

	void Start ()
	{
		btn = gameObject.GetComponent<Button>();
		btn.onClick.AddListener(TaskOnClick);
	}
	
	void Update ()
	{
		
	}

	void TaskOnClick()
	{
		SceneManager.LoadScene(SceneToLoad);
	}
}