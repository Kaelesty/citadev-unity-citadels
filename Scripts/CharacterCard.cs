using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCard : MonoBehaviour
{
    public GameObject nameField;
    public GameObject takeButton;
    public GameObject imageSlot;

    public Player owner;
    public CharacterCard script;
    public GameObject cardObject;

    public GameObject canvas;

    public string description;
    public string color;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnTaken()
    {

        owner.addCharacterCard(nameField.GetComponent<Text>().text);
    }

    public void loadPreset(string presetName)
    {
        /*
        TO-DO 
        ������� ��������� �������� ������� (���������), � ���������� �� �����/�������/... :

        ���         (����� �������� � nameField)
        ��������    (����� ��������� � ���������� description) (��������, ���� ��� �������� ���������� ��� �����)
        ��������    (����� ���������� � imageSlot)
        ����(���)   (����� �������� � ���������� color)

        */
    }
}
