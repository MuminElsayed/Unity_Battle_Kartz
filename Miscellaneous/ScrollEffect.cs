using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollEffect : MonoBehaviour {

	public Renderer render;
	public string[] textures;

	void Start()
	{
		render = GetComponent<Renderer>();
		textures = render.material.GetTexturePropertyNames();
	}

	void Update () 
	{
		// render.material.mainTextureOffset = new Vector2(0, Time.time);
		render.material.SetTextureOffset("_BaseMap", new Vector2(0, -Time.time));
	}
}
