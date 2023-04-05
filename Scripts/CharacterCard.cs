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
    public DeckController deckController;

    public GameObject canvas;

    public string description;
    public string color;

    void Start()
    {
        deckController = GameObject.Find("DeckManager").GetComponent<DeckController>();
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
        ������� ��������� �������� ������� (���������): "Assassin", "Thief", "Bishop", "Magican", "Architect", "Merchant", "Warlord"
        � ��������� ���� � ������������ � ������ ��������:
        ���         (����� �������� � nameField) (���������� �� ������� deckController.getCharNames())
        ��������    (����� ��������� � ���������� description) (��������, ���� ��� �������� ���������� ��� �����) (���������� �� ������� deckController.getCharDesc())
        ��������    (����� ���������� � imageSlot) (�������� �������� ����� � ����� images)
        ����(���)   (����� �������� � ���������� color, ��������, ���� ��� �������� ���������� ��� �����) (����� ���������� ��� � ����, gold � ������, red � ����������, green � �������, blue � ��������, white � ���������)

        */
    }
}
