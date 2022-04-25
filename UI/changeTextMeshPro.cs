using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class changeTextMeshPro : MonoBehaviour 
{
	private TextMeshProUGUI textObject;
	public string staticValue;


	void Start()
	{
		textObject = GetComponent<TextMeshProUGUI>();
	}
	public void changeText(float text)
	{
		textObject.text = staticValue + (text/10).ToString();
	}
}
