using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterScreenManager : MonoBehaviour
{
    PhotonView view;

    public GameObject characterMenu;
    public GameObject characterMenuHeader;
    public GameObject gamestateIndicator;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
        characterMenu.transform.LeanScale(new Vector3(0.06f, 0.12f), 0);
        characterMenu.transform.LeanRotateAroundLocal(new Vector3(0, 0, 180), 360, 3).setEaseInCubic().setLoopPingPong();


        // ���� ��� ��������� � ����� ������ ����
        gamestateIndicator.GetComponent<Text>().text = "���� ���!";
        LeanTween.cancel(characterMenu);
        characterMenu.transform.LeanScale(new Vector3(1f, 1f), 6).setEaseInCubic().setOnComplete(
            delegate()
            {
                characterMenuHeader.SetActive(true);
            }
            );
    }


}
