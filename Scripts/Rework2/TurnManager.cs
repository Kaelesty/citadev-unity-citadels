using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviourPunCallbacks
{
    PhotonView view;
    private int[] queue = null;

    private string[] characters = { "Assassin", "Thief", "Magician", "King", "Bishop", "Merchant", "Architect", "Warlord" };

    private void Awake()
    {
        view = GetComponent<PhotonView>();
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

    [PunRPC]
    private void refillQueueByPlayerID()
    {
        queue = new int[PhotonNetwork.CurrentRoom.PlayerCount];
        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount;i++)
        {
            queue[i] = i;
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

        queue = new int[PhotonNetwork.CurrentRoom.PlayerCount];
        Dictionary<string, int> players = new Dictionary<string, int>();
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            players[player.GetComponent<PlayerRework>().character] = player.GetComponent<PlayerRework>().id;
        }

        // � ������� players ������ �����-���������, ��������-id �������
    }
}
