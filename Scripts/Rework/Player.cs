using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviourPunCallbacks
{
    PhotonView view;
    public Game_Controller controller;
    public int id;
    public GameObject position;

    public int money;

    public GameObject character;
    public String characterPreset;
    public string[] districts;
    public string[] buildedDistricts;

    // ������ �� ������ �������� �������, ����� ���������� -_-
    private bool characterSelected = false;
    private bool districtsTaken = false;
    private bool districtsRendered = false;
    public bool resourcesRendered = false;
    private bool buildingRendered = false;
    private bool skillsRendered = false;
    private bool endgameRendered = false;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
        controller = GameObject.Find("Controller").GetComponent<Game_Controller>();
        gameObject.tag = "Player";
        if (view.IsMine)
        {
            id = PhotonNetwork.LocalPlayer.ActorNumber;
            position = controller.getZeroPosition();
        }
        else
        {
            id = -1;
            position = controller.getFreePosition();
        }
        position.GetComponent<Game_Position>().owner = this;
        transform.position = new Vector3(0, 0, 0);

    }

    public void buildDistrict(string preset)
    {
        view.RPC("buildDistrictSync", RpcTarget.All, preset);
    }

    [PunRPC]
    private void buildDistrictSync(string preset)
    {
        // Debug.Log("BuildDistrictSync");
        buildedDistricts = buildedDistricts.Append(preset).ToArray();
        money -= controller.getDistPrice()[preset];
        controller.updateMoneyIndicator(money);
    }

    // ��-�� ������������ RPC ���������� ������������ �������
    // TODO: ���������, ��� ��� ������
    public void resetRenderedFlags()
    {
        view.RPC("resetRenderedFlagsSync", RpcTarget.All);
    }

    [PunRPC]
    public void resetRenderedFlagsSync()
    {
        districtsRendered = false;
        resourcesRendered = false;
        buildingRendered = false;
        skillsRendered = false;
        endgameRendered = false;
    }

    

    private void Update()
    {
        if (id != -1)
        {
            view.RPC("idSync", RpcTarget.Others, id);
        }
        if (controller.checkTurn() == id) {
            position.GetComponent<Game_Position>().Activate();
            if (view.IsMine)
            {
                switch (controller.getGameState())
                {
                    case "CharacterSelecting":
                        controller.renderDeck();
                        if (!districtsTaken)
                        {
                            districtsTaken=true;
                            for (int i = 0;i<4;i++)
                            {
                                districts = districts.Append(controller.takeRandomDistrict()).ToArray();
                            }
                        }
                        renderDistricts();
                        break;
                    case "Major: Resources":
                        if (!resourcesRendered) {
                            controller.renderResourcesUI(true);
                            resourcesRendered=true;
                        }
                        break;
                    case "Major: Building":
                        if (controller.assassinMarker == id && controller.currentTurn == id)
                        {
                            Debug.Log("Assassin's prey");
                            controller.assassinMarker = 0;
                            controller.switchGameState("Major: Building");
                            controller.callNextTurn();
                            controller.switchSkipping(false);
                        }
                        else
                        {
                            controller.renderResourcesUI(false);
                            renderBuildingUI(true);
                        }
                        break;
                    case "Major: Skills":
                        renderBuildingUI(false);
                        break;
                }
            }
        }
        else
        {
            position.GetComponent<Game_Position>().Unactivate();
        }
    }

    private void renderBuildingUI(bool activity)
    {
        if (!buildingRendered || !activity)
        {
            buildingRendered=!activity;
            controller.switchSkipping(true);
            // Debug.Log("Skipping switched");
            foreach (var i in GameObject.FindGameObjectsWithTag("PlayerDistrictCard"))
            {
                if (i.GetComponent<DistrictCard>().price <= money || !activity)
                {
                    i.GetComponent<DistrictCard>().activateBuilding(activity);
                }
            }
        }
    }

    public void resetCharacter()
    {
        view.RPC("resetCharacterSync", RpcTarget.All);
        var charCard = GameObject.FindGameObjectWithTag("PlayerCharacterCard");
        Destroy(charCard);
        resetRenderedFlags();
    }

    [PunRPC]
    private void resetCharacterSync()
    {
        character = null;
        characterSelected = false;
    }

    [PunRPC]
    private void idSync(int nuID)
    {
        id = nuID;
    }

    public void addMoney(int amount)
    {
        view.RPC("addMoneySync", RpcTarget.All, amount);
        controller.updateMoneyIndicator(money);
    }

    [PunRPC]
    public void addMoneySync(int amount)
    {
        money += amount;
    }

    public void playerRenderAssassinUI()
    {
        if (controller.currentTurn == id && view.IsMine)
        {
            controller.renderAssassinUI(true);
        }
    }

    public void playerSwitchSkipping()
    {
        if (controller.currentTurn == id && view.IsMine)
        {
            controller.switchSkipping(true);
        }
    }

    public void cardSelected(string preset)
    {
        
        view.RPC("setCharacter", RpcTarget.All, preset);
    }

    [PunRPC]
    private void setCharacter(string preset)
    {
        character = controller.InstantiateCharCard(preset);
        character.tag = "PlayerCharacterCard";
        character.GetComponent<CharacterCard>().takeButton.SetActive(false);
        characterSelected = true;
        if (view.IsMine)
        {
            character.transform.position = new Vector3(-850, -418, 0);
        }
        else
        {
            character.transform.position = position.transform.position + new Vector3(0, -150, 0);
        }
        characterPreset = preset;
    }

    public void renderDistricts(bool force=false)
    {
        if (districtsRendered && !force)
        {
            //Debug.Log("RenderDistricts Abadoned");
            return;
        }
        Debug.Log("RenderDistricts");
        foreach (var i in GameObject.FindGameObjectsWithTag("PlayerDistrictCard"))
        {
            Destroy(i);
        }

        districtsRendered = true;
        //Debug.Log(districts.Length);
        string[] lowerDistricts = { };
        string[] upperDistricts = { };
        for (int i = 0;i< districts.Length; i++)
        {
            if (buildedDistricts.Contains(districts[i])) {
                upperDistricts = upperDistricts.Append(districts[i]).ToArray();
            }
            else
            {
                lowerDistricts = lowerDistricts.Append(districts[i]).ToArray();
            }
        }
        for (int i =0;i< lowerDistricts.Length;i++)
        {
            var district = controller.InstantiateDistrictCard(lowerDistricts[i]);
            var script = district.GetComponent<DistrictCard>();
            script.buildButton.SetActive(false);
            script.takeButton.SetActive(false);
            district.tag = "PlayerDistrictCard";
            district.transform.position = new Vector3(860 - 170 * i - 60, -390, 0);
        }
        for (int i = 0; i < upperDistricts.Length; i++)
        {
            var district = controller.InstantiateDistrictCard(upperDistricts[i]);
            var script = district.GetComponent<DistrictCard>();
            script.buildButton.SetActive(false);
            script.takeButton.SetActive(false);
            district.tag = "PlayerDistrictCard";
            district.transform.position = new Vector3(860 - 170 * i - 60, -100, 0);
            script.buildedButton.SetActive(true);
        }
    }

    public void addToDistricts(string district)
    {
        //Debug.Log("AddToDistricts: " + id.ToString() + " // " + district);
        view.RPC("AddToDistrictsSync", RpcTarget.All, district);
        renderDistricts();
    }

    [PunRPC]
    private void AddToDistrictsSync(string district)
    {
        districts = districts.Append(district).ToArray();
        districtsRendered = false;
    }
}
