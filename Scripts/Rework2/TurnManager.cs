using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviourPunCallbacks
{
    PhotonView view;
    public int[] queue = null;

    private int winner = -1;

    public GameObject endTurnButton;

    public int stage = 0;
    // 0 - ����� ����������, 1 - ��������
    // (������������ GameRework) 1 - ����������� ���� Major ������

    public int currentQueueIndex = 0;

    private string[] characters = { "Assassin", "Thief", "Magician", "King", "Bishop", "Merchant", "Architect", "Warlord" };

    private void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    public void callInit()
    {
        view.RPC("nit", RpcTarget.All);
    }

    [PunRPC]
    public void init()
    {
        callRefillQueueByPlayerID();
        var csm = GameObject.FindGameObjectWithTag("CSM");
        if (csm.GetComponent<CharacterScreenManager>().turn(queue[0], stage))
        {
            // endTurnButton.SetActive(true);
        }
    }

    public void callEndTurn()
    {
        view.RPC("endTurn", RpcTarget.All);
    }

    [PunRPC]
    private void endTurn()
    {
        currentQueueIndex++;
        var csm = GameObject.FindGameObjectWithTag("CSM");
        var dm = GameObject.FindGameObjectWithTag("DeckManager");
        if (currentQueueIndex >= queue.Length)
        {
            if (winner != -1)
            {
                var screenManager = GameObject.FindGameObjectWithTag("ScreenManager").GetComponent<ScreenManager>();
                screenManager.switchScreen("������");

                var victoryMenuManager = GameObject.FindGameObjectWithTag("VMM").GetComponent<VictoryMenuManager>();
                victoryMenuManager.endgame();
                return;
            }
            currentQueueIndex = 0;
            if (stage == 0)
            {
                stage = 1;
                refillQueueByCharacters();
                csm.GetComponent<CharacterScreenManager>().turn(queue[0], stage);
            }
            else
            {
                stage = 0;
                dm.GetComponent<DeckManager>().callGenerateDeck();
                refillQueueBySeating();
                csm.GetComponent<CharacterScreenManager>().resetCharacterAndTurn(queue[0], stage);
            }
            
        }
        else
        {
            csm.GetComponent<CharacterScreenManager>().turn(queue[currentQueueIndex], stage);
        }
    }

    public int getWinner()
    {
        return winner;
    }

    public void callRefillQueueByPlayerID()
    {
        view.RPC("refillQueueByPlayerID", RpcTarget.All);
    }

    public void callRefillQueueByCharacters()
    {
        view.RPC("refillQueueByCharacters", RpcTarget.All);
    }

    public void callRefillQueueBySeating()
    {
        view.RPC("refillQueueBySeating", RpcTarget.All);
    }

    public void callWin(int id)
    {
        if (winner != -1)
        {
            return;
        }
        view.RPC("win", RpcTarget.All, id);
    }

    [PunRPC]
    private void refillQueueByPlayerID()
    {
        queue = new int[PhotonNetwork.CurrentRoom.PlayerCount];
        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount;i++)
        {
            queue[i] = i + 1;
        }
    }

    [PunRPC]
    private void refillQueueByCharacters()
    {
        queue = new int[PhotonNetwork.CurrentRoom.PlayerCount];
        Dictionary<string, int> players = new Dictionary<string, int>();
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            players[player.GetComponent<PlayerRework>().character] = player.GetComponent<PlayerRework>().id;
        }

        int i = 0;
        foreach (string character in characters)
        {
            if (players.ContainsKey(character)) {
                queue[i] = players[character];
                i++;
            }
        }
    }

    [PunRPC]
    private void refillQueueBySeating()
    {
        // TODO: refillQueueSeating
        // ��������� ������� �� "��������" - ������ �����-������, ����� id + 1 ������������ ���� � ��� �����
        // ������:
        // [��������-ID]
        // ������� - 1
        // ����� - 2
        // ������ - 3
        // ��� - 4
        // ��� ����� "��������" ������ ���������� ������� [3, 4, 1, 2]
        // id-����� �������� - player.

        //refillQueueByPlayerID();
        //return;

        queue = new int[PhotonNetwork.CurrentRoom.PlayerCount];
        Dictionary<string, int> players = new Dictionary<string, int>();
        int max_id = 0;
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            players[player.GetComponent<PlayerRework>().character] = player.GetComponent<PlayerRework>().id;
            if (player.GetComponent<PlayerRework>().id > max_id)
            {
                max_id = player.GetComponent<PlayerRework>().id;
            }
        }

        if (!players.ContainsKey("King"))
        {
            refillQueueByPlayerID();
            return;
        }

        queue[0] = players["King"];
        int i = 1;
        for (int j = queue[0] + 1; j < max_id; j++)
        {
            queue[i] = j;
            i++;
        }
        for (int j = 1; j < queue[0]; j++)
        {
            queue[i] = j;
            i++;
        }
        // � ������� players ������ �����-���������, ��������-id �������
    }

    [PunRPC]
    private void win(int id)
    {
        winner = id;
    }
}
