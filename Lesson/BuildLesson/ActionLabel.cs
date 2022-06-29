using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionLabel : MonoBehaviour
{
    public Button btnEdit;
    public Button btnAdd;
    public Button btnMove;
    public Button btnDelete;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        InitEvents();
    }
    void InitEvents()
    {
        btnEdit.onClick.AddListener(HanderBtnEdit);
        btnAdd.onClick.AddListener(HanderBtnAdd);
        btnMove.onClick.AddListener(HanderBtnMove);
        btnDelete.onClick.AddListener(HanderBtnDelete);
    }
    void HanderBtnEdit()
    {
        Debug.Log("Edit: ");
    }
    void HanderBtnAdd()
    {
        Debug.Log("Add: ");
    }

    void HanderBtnMove()
    {
        Debug.Log("Move: ");
    }
    void HanderBtnDelete()
    {
        Debug.Log("Delete: ");
    }
}
