using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Player : MonoBehaviourPunCallbacks
{
    PhotonView view;
    public int positionID; // id ������� ������ �� �����, ���������� �� SpawnManager (�� ����������������)
    public int networkID; // id ������� � ���� (������ �� ���������� � 1)

    public GameObject charPrefab;

    public GameObject Camera;
    public GameObject Cursor; // ��������� ��������� ������ (����� �����������)
    public GameObject Body; // ��������� ������� ������ (������� �����������)
    public GameObject UI;
    public GameObject Indicator; // ��������� �������� ���� (����� �����������)

    public Player scriptPlayer;

    private TurnManager turnManager;
    private DeckController deckController;


    private bool flag_deckTaken; // ����� �������������� ������, ������� ������� �������
    private string[] deck; // �������� ���� ������� ����, ���������� �� DeckController
    private GameObject characterCard; // ����� ��������� �� ������� ����


    private void Awake() {
        view = GetComponent<PhotonView>();

        turnManager = GameObject.Find("TurnManager").GetComponent<TurnManager>();
        deckController = GameObject.Find("DeckManager").GetComponent<DeckController>();

        networkID = PhotonNetwork.LocalPlayer.ActorNumber;

        positionID = GameObject.Find("SpawnManager").GetComponent<SpawnManager>().takeID();
        transform.eulerAngles = new Vector3(
            transform.eulerAngles.x,
            transform.eulerAngles.y,
            (transform.eulerAngles.z + 60) * positionID
        );

        if (!view.IsMine)
        {
            Camera.SetActive(false);
            scriptPlayer.enabled = false;
            Cursor.SetActive(false);
        } else
        {
            Body.SetActive(false);
        }
        Indicator.SetActive(false);

        UI.SetActive(false);
        flag_deckTaken = false;
    }

    void Update()
    {
        //
        // TurnManager �������������� �������� ActivePlayerID. ��� ����� id ������� => ��� ���
        if (turnManager.getActivePlayerID() == networkID)
        {
            UI.SetActive(true);
            view.RPC("enableIndicator", RpcTarget.All);
            if (!flag_deckTaken)
            {
                flag_deckTaken = true;
                deck = deckController.getDeck();

                for (int i = 0; i < deck.Length; i++)
                {
                    GameObject charCard = Instantiate(charPrefab, transform.position, Quaternion.identity);
                    charCard.transform.position += new Vector3(140 * i - ((140 * deck.Length / 2) - 50 - 20), 0, 0);
                    var cardScript = charCard.GetComponent<CharacterCard>();

                    // ���� ����� ���� ������� ����� ��������� ������ CharacterCard.loadPreset
                    var text = cardScript.nameField.GetComponent<Text>();
                    cardScript.canvas.GetComponent<Canvas>().worldCamera = Camera.GetComponent<Camera>();
                    cardScript.owner = this;
                    text.text = deck[i];
                    // ^^^^^^^^^^^^^^^^^^^^^^^

                    cardScript.loadPreset(deck[i]); // �� ��������
                }
            }
        }
        else
        {
            UI.SetActive(false);
            view.RPC("disableIndicator", RpcTarget.All);
        }


    }

    // ���������� �� CharacterCard, ��������� ��������� ����� ���������
    public void addCharacterCard(GameObject card)
    {
        view.RPC("addCharacterCard_RPC", RpcTarget.All, card);

        // RPC �� ����� ���������� ��������� ���� ��� ���������
        /*
        TO-DO
        ��� ������� ��������� string � ���������� ��� � addCharacterCard_RPC, ������� ������ �������������� ������ �����
        */
    }

    [PunRPC]
    void addCharacterCard_RPC(GameObject card)
    {
        Destroy(card.GetComponent<CharacterCard>().takeButton);
        characterCard = card;
        characterCard.tag = "PlayerCharacterCard";
        characterCard.transform.parent = transform;
        characterCard.transform.position = new Vector3(0, 0, 0);
        endTurn();
    }


    // ������ ������������ ��� ������������� ����������� ����
    [PunRPC]
    void disableIndicator()
    {
        Indicator.SetActive(false);
    }

    [PunRPC]
    void enableIndicator()
    {
        Indicator.SetActive(true);
    }

    public void endTurn()
    {
        foreach (GameObject card in GameObject.FindGameObjectsWithTag("CharacterCard"))
        {
            Destroy(card);
        }
        turnManager.switchActivePlayerID();
    }
}
