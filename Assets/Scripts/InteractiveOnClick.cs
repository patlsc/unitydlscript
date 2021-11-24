using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class InteractiveOnClick : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent activator;
    
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        activator.Invoke();
    }
}
