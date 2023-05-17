using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.UI;

public class SpyScreenManager : MonoBehaviour
{

    List<string> players = new List<string>();

    public GameObject targetSelector;
    public void init()
    {
        var csm = GameObject.FindGameObjectWithTag("CSM").GetComponent<CharacterScreenManager>();
        var master = csm.getMasterPlayer().GetComponent<PlayerRework>();
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (master.nickname == player.GetComponent<PlayerRework>().nickname)
            {
                continue;
            }
            players.Add(player.GetComponent<PlayerRework>().nickname);
        }

        var dropdown = targetSelector.GetComponent<Dropdown>();
        dropdown.ClearOptions();
        for (int i = 0; i < players.Count; i++)
        {
            var option = new Dropdown.OptionData();
            option.text = players[i];
            dropdown.options.Add(option);
        }
    }

    public void targetSelectorOnChange()
    {
        var csm = GameObject.FindGameObjectWithTag("CSM").GetComponent<CharacterScreenManager>();
        var master = csm.getMasterPlayer().GetComponent<PlayerRework>();
        List<PlayerRework> targets = new List<PlayerRework>();
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            targets.Add(player.GetComponent<PlayerRework>());
        }
        var dropdown = targetSelector.GetComponent<Dropdown>();
        var targetNickname = dropdown.options[dropdown.value].text;
        PlayerRework target;

        // TODO : ����� ��������
        // � ���������� targetNickname ����� ������� ������, �� ������� ����� ��������
        // � ���������� targets ����� ������ �������� �������
        // �����: �������� �� ������ targets, ����� ������ � �������� nickname == targetNickname, �������� ��� ������ � target
        // �����, ���� target.characterShown == true, ������� ����� ��������� � �������� target.Character, 
        // ����� ������� ����� ��������� � �������� "unknown"
        // (������� ����� ��������� -> var card = csm.InstantiateCharCard(<������>))
        // ����������� ��� ����� � ������ ����� �� ������ (� ������ "��������")
        // �����, �������� �� ������ target.buildedDistricts (� ��� ����� ������� ������������ �������)
        // ��� ������� �� ��� ������� ����� ������
        // (������� ����� ������ -> var card = csm.InstantiateDistCard(<������>))
        // ������ �� ���� ���� ����������� �� ������ ������� �� ������ (� ������ "��������")
        // ��������� ���� �� �� ��� ��� ������� �������
    }
}
