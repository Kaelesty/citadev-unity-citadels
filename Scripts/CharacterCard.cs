using System.Collections;
using System.Collections.Generic;
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

    public Game_Controller controller;
    private string preset;


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
        Debug.Log("onTaken");
        controller.characterSelected();
        owner.cardSelected(preset);
    }

    public void loadPreset(string presetName)
    {
        preset = presetName;
        nameField.GetComponent<Text>().text = presetName;
        /*
        TO-DO 
        ������� ��������� �������� ������� (���������): "Assassin", "Thief", "Bishop", "Magican", "Architect", "Merchant", "Warlord"
        � ��������� ���� � ������������ � ������ ��������:
        ���         (����� �������� � nameField) (���������� �� ������� deckController.getCharNames())
        ��������    (����� ��������� � ���������� description) (��������, ���� ��� �������� ���������� ��� �����) (���������� �� ������� deckController.getCharDesc())
        ��������    (����� ���������� � imageSlot) (�������� �������� ����� � ����� images)
        ����(���)   (����� �������� � ���������� color, ��������, ���� ��� �������� ���������� ��� �����) (����� ���������� ��� � ����, gold � ������, red � ����������, green � �������, blue � ��������, white � ���������)

        */
    }
}
