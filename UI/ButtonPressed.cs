using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
     
public class ButtonPressed : MonoBehaviour, IPointerDownHandler 
{
     
    
    public void OnPointerDown(PointerEventData eventData)
	{
		GameController.instance.resetPlayer();
    }
}