using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MakeItButton : MonoBehaviour
{
    public UnityEvent unityEvent = new UnityEvent();
    public GameObject btnEdit;
    // Start is called before the first frame update
    void Start()
    {
        btnEdit = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit; 
        if(Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
            {
                unityEvent.Invoke();  // Cái này Invoke cai gi, mi biet k ? 
            }
        }
    }
}
