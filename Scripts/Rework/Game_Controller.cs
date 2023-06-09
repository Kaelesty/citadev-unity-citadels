using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;

public class Game_Controller : MonoBehaviour
{
    private System.Random random = new System.Random();

    public GameObject canvas;
    public GameObject camera;
    public GameObject gameStateIndicator;
    public GameObject takeMoneyButton;
    public GameObject moneyIndicator;
    public GameObject skipButton;
    public GameObject endgameIndicator;
    public GameObject endgameMessage;
    public GameObject endgameBackground;
    public GameObject selector;

    private String[] priorityList = { "Assassin", "Thief", "Magician", "King", "Bishop", "Merchant", "Architect", "Warlord" };

    //SpawnManager
    public GameObject[] positions;

    // TurnManager
    private int freePosition = 0;
    public int currentTurn = 1;
    public string gameState = "CharacterSelecting";

    //DeckManager
    public string[] characters = { "Assassin", "Thief", "Magician", "Bishop", "Architect", "Merchant", "Warlord" };
    public string[] districts = { "Tavern", "Market", "Trading Post", "Docks", "Harbor", "Town Hall", "Temple", "Church", "Monastery", "Cathedral", "Watchtower", "Prison", "Battlefield", "Fortress", "Manor", "Castle", "Palace", "Haunted", "Keep", "Laboratory", "Smithy", "Graveyard", "Observatory", "School of Magic", "Library", "Great Wall", "University", "Dragon Gate" };
    public Dictionary<string, int> queue = new Dictionary<string, int>(){
            { "Assassin", 0},
            { "Thief", 0},
            { "Bishop", 0},
            { "Magician", 0},
            { "Architect", 0},
            { "Merchant", 0},
            { "Warlord", 0},
            { "King", 0},
        };
    public string[] deck;
    public string[] districtDeck;
    public string laidOutPrivate;
    public string laidOutPublic;
    public GameObject charPrefab;
    public GameObject distPrefab;
    private bool deckRendered = false;
    private bool districtDeckRendered = false;
    private bool assassinUIrendered = false;
    public int assassinMarker = 0;



    PhotonView view;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
    }
    
    public int queueFirst()
    {
        // ���������� player.id � ��������� ����������� ����
        foreach (String character in priorityList)
        {
            if (queue[character] != 0)
            {
                //Debug.Log(character);
                //Debug.Log(queue[character]);
                return queue[character];
            }
        }
        return -1;
    }

    [PunRPC]
    public void queueComeThrough()
    {
        // �������� player.id � ��������� ����������� ����
        // �������� ������ ����� view.RPC
        foreach (String character in priorityList)
        {
            if (queue[character] != 0)
            {
                queue[character] = 0;
                break;
            }
        }
    }

    [PunRPC]
    public void queueInterfere(string cardName, int cardOwnerID)
    {
        // �������� ������ ����� view.RPC
        queue[cardName] = cardOwnerID;
    }

    public GameObject getFreePosition()
    {
        freePosition++;
        positions[freePosition].SetActive(true);
        return positions[freePosition];
    }
    public GameObject getZeroPosition()
    {
        positions[0].SetActive(true);
        return positions[0];
    }

    public int checkTurn()
    {
        return currentTurn;
    }

    public void callNextTurn()
    {
        view.RPC("nextTurn", RpcTarget.All);
    }

    [PunRPC]
    public void nextTurn()
    {
        switch (gameState)
        {
            case "CharacterSelecting":
                currentTurn++;
                if (currentTurn > PhotonNetwork.CurrentRoom.PlayerCount)
                {
                    currentTurn = queueFirst();
                    //view.RPC("queueComeThrough", RpcTarget.All);
                    gameState = "Major: Skills";
                    gameStateIndicator.GetComponent<Text>().text = "Major: Skills";
                    handleSkills();
                }
                break;
            case "Major: Resources":
                Debug.Log("Resources -> Building");
                gameState = "Major: Building";
                playerSwitchSkipping();
                gameStateIndicator.GetComponent<Text>().text = "Major: Building";
                renderResourcesUI(false);
                break;
            case "Major: Building":
                view.RPC("queueComeThrough", RpcTarget.All);
                currentTurn = queueFirst();
                if (currentTurn != -1)
                {
                    gameState = "Major: Skills";
                    gameStateIndicator.GetComponent<Text>().text = "Major: Skills";
                    handleSkills();
                }
                else
                {
                    if (!checkEndgame())
                    {
                        gameState = "CharacterSelecting";
                        currentTurn = 1;
                        characterSelectingInit();
                    }
                }
                break;
            case "Major: Skills":
                if (assassinUIrendered)
                {
                    var dropdown = selector.GetComponent<Dropdown>();
                    foreach (GameObject i in GameObject.FindGameObjectsWithTag("Player"))
                    {
                        var prey = getCharNames()[i.GetComponent<Player>().characterPreset];
                        if (prey == dropdown.options[dropdown.value].text)
                        {
                            assassinMarker = i.GetComponent<Player>().id;
                            break;
                        }
                    }
                    renderAssassinUI(false);
                }
                Debug.Log("Skills -> Resources");
                gameState = "Major: Resources";
                gameStateIndicator.GetComponent<Text>().text = "Major: Resources";
                break;
        }
    }

    private bool checkEndgame()
    {
        foreach (var i in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (i.GetComponent<Player>().buildedDistricts.Length == 8)
            {
                renderEndgame(i.GetComponent<Player>().id);
                return true;
            }
        }
        return false;
    }

    public void playerSwitchSkipping()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            player.GetComponent<Player>().playerSwitchSkipping();
        }
    }

    public void switchGameState(String gs)
    {
        view.RPC("switchGameStateSync", RpcTarget.All, gs);
    }

    [PunRPC]
    private void switchGameStateSync(String gs)
    {
        gameState = gs;
    }

    private void renderEndgame(int id)
    {
        endgameBackground.SetActive(true);
        endgameMessage.SetActive(true);
        endgameIndicator.SetActive(true);
        switchSkipping(false);
        endgameMessage.GetComponent<Text>().text = "Winner: " + id.ToString();
    }
    private void handleSkills ()
    {
        // Debug.Log("Handle skills");
        Player activePlayer = null;
        foreach (GameObject i in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (i.GetComponent<Player>().id == queueFirst())
            {
                activePlayer = i.GetComponent<Player>();
                break;
            }
        }
        switch (activePlayer.characterPreset)
        {
            case "Bishop":
                for (int i = 0; i < activePlayer.buildedDistricts.Length; i++)
                {
                    if (getDistColor()[activePlayer.buildedDistricts[i]] == "blue")
                    {
                        activePlayer.addMoney(1);
                    }
                }
                callNextTurn();
                break;
            case "Merchant":
                activePlayer.addMoney(1);
                for (int i = 0; i < activePlayer.buildedDistricts.Length; i++)
                {
                    if (getDistColor()[activePlayer.buildedDistricts[i]] == "green")
                    {
                        activePlayer.addMoney(1);
                    }
                }
                callNextTurn();
                break;
            case "Architect":
                callNextTurn();
                break;
            case "Assassin":
                playerRenderAssassinUI();
                break;
            case "King":
                for (int i = 0; i < activePlayer.buildedDistricts.Length; i++)
                {
                    if (getDistColor()[activePlayer.buildedDistricts[i]] == "yellow")
                    {
                        activePlayer.addMoney(1);
                    }
                }
                callNextTurn();
                break;
            case "Magician":
                callNextTurn();
                break;
            case "Thief":
                callNextTurn();
                break;
            case "Warlord":
                for (int i = 0; i < activePlayer.buildedDistricts.Length; i++)
                {
                    if (getDistColor()[activePlayer.buildedDistricts[i]] == "red")
                    {
                        activePlayer.addMoney(1);
                    }
                }
                callNextTurn();
                break;
        }
    }

    public void characterSelectingInit()
    {
        view.RPC("resetCharacters", RpcTarget.All);
        gameStateIndicator.GetComponent<Text>().text = "CharacterSelecting";
        switchSkipping(false);
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            player.GetComponent<Player>().resetCharacter();
            generateDeck();
            deckRendered = false;
        }
    }

    [PunRPC]
    public void resetCharacters()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            player.GetComponent<Player>().resetCharacter();
        }
    }

    public void playerRenderAssassinUI()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            player.GetComponent<Player>().playerRenderAssassinUI();
        }
    }

    public void renderAssassinUI(bool activity)
    {
            selector.SetActive(activity);
            assassinUIrendered = activity;
            // Debug.Log("Assassin UI");
            if (activity)
            {
                playerSwitchSkipping();
                selector.GetComponent<Dropdown>().ClearOptions();
                foreach (String i in characters)
                {
                    if (i == "Assassin")
                    {
                        continue;
                    }
                    var option = new Dropdown.OptionData();
                    option.text = getCharNames()[i];
                    selector.GetComponent<Dropdown>().options.Add(option);
                }
                var noneOption = new Dropdown.OptionData();
                noneOption.text = "<------->";
                selector.GetComponent<Dropdown>().options.Add(noneOption);
                noneOption = new Dropdown.OptionData();
                noneOption.text = "<------->";
                selector.GetComponent<Dropdown>().options.Add(noneOption);
                selector.GetComponent<Dropdown>().RefreshShownValue();
            }
            else
            {
                switchSkipping(false);
                assassinUIrendered = false;
            }
    }

    public void switchSkipping(bool skipping)
    {
        skipButton.SetActive(skipping);
        // Debug.Log(skipping);
    }

    public void renderResourcesUI(bool activity)
    {
        //Debug.Log("renderResourcesUI");
        takeMoneyButton.SetActive(activity);
        
        if (activity)
        {
            switchSkipping(false);
            for (int i = 0; i < 2; i++)
            {
                var card = InstantiateDistrictCard(takeRandomDistrict());
                card.tag = "DistrictCard";
                card.transform.position = new Vector3(-(260 / 2) + 140 * i + 60, 0, 0);
            }
        }
        else
        {
            foreach (var i in GameObject.FindGameObjectsWithTag("DistrictCard"))
            {
                Destroy(i);
            }
        }

    }
    public void generateDeck()
    {
        view.RPC("generateDeckSync", RpcTarget.All);
    }

    [PunRPC]
    private void generateDeckSync()
    {
        deck = characters;
        deck = deck.OrderBy(x => random.Next()).ToArray();

        laidOutPrivate = deck[0];
        laidOutPublic = deck[1];

        deck = deck.Skip(1).ToArray();
        deck = deck.Skip(1).ToArray();

        deck = deck.Append("King").ToArray();

        deck = deck.OrderBy(x => random.Next()).ToArray();
    }
    public string[] getDeck()
    {
        return deck;
    }

    [PunRPC]
    private void deleteCardSync(string cardName)
    {
        // �������� ������ ����� view.RPC
        deck = deck.Where(e => e != cardName).ToArray();
    }

    public string takeRandomDistrict() {
        var district = districtDeck[0];
        view.RPC("takeRandomDistrictSync", RpcTarget.All);
        return district;
    }

    [PunRPC]
    private void takeRandomDistrictSync()
    {
        districtDeck = districtDeck.Skip(1).ToArray();
    }

    public void addToDistrictDeck(String preset)
    {
        view.RPC("addToDistrictDeckSync", RpcTarget.All, preset);
    }

    [PunRPC]
    private void addToDistrictDeckSync(String preset)
    {
        districtDeck = districtDeck.Append(preset).ToArray();
    }

    public void Start()
    {
        generateDeck();
        generateDistrictDeck();
    }

    public void renderDeck()
    {
        if (!deckRendered)
        {
            deckRendered = true;
            var len = deck.Length * 200 - 20;
            for (int i = 0; i < deck.Length; i++)
            {
                var charCard = InstantiateCharCard(deck[i]);
                charCard.transform.position = new Vector3(-(len / 2) + 200*i + 60, 0, 0);

            }
        }
    }

    public GameObject InstantiateCharCard(string presetName)
    {
        GameObject charCard = Instantiate(charPrefab, transform.position, Quaternion.identity);

        var cardScript = charCard.GetComponent<CharacterCard>();
        cardScript.canvas.GetComponent<Canvas>().worldCamera = camera.GetComponent<Camera>();

        //cardScript.controller = this;
        //cardScript.owner = positions[0].GetComponent<Game_Position>().owner;

        cardScript.loadPreset(presetName);
        return charCard;
    }
    public GameObject InstantiateDistrictCard(string presetName)
    {
        GameObject charCard = Instantiate(distPrefab, transform.position, Quaternion.identity);

        var cardScript = charCard.GetComponent<DistrictCard>();
        cardScript.canvas.GetComponent<Canvas>().worldCamera = camera.GetComponent<Camera>();
        cardScript.controller = this;
        cardScript.preset = presetName;
        cardScript.owner = positions[0].GetComponent<Game_Position>().owner;
        cardScript.loadPreset(presetName);
        return charCard;
    }

    public void takeMoney()
    {
        var playerID = queueFirst();
        foreach (GameObject position in positions)
        {
            if (position.GetComponent<Game_Position>().owner.id == playerID)
            {
                position.GetComponent<Game_Position>().owner.addMoney(2);
                break;
            }
        }
        view.RPC("nextTurn", RpcTarget.All);
    }

    public void updateMoneyIndicator(int amount) 
    {
        moneyIndicator.GetComponent<Text>().text = "Money: " + amount.ToString();
    }

    public void characterSelected(string cardName, int cardOwnerID)
    {
        view.RPC("deleteCardSync", RpcTarget.All, cardName);
        // Debug.Log("Queue interfere");
        view.RPC("queueInterfere", RpcTarget.All, cardName, cardOwnerID);
        foreach (var i in GameObject.FindGameObjectsWithTag("CharacterCard"))
        {
            Destroy(i);
        }
        view.RPC("nextTurn", RpcTarget.All);
    }


    public Dictionary<string, string> getCharNames()
    {
        return new Dictionary<string, string>() {
            { "Assassin", "��������"},
            { "Thief", "���"},
            { "Bishop", "�������"},
            { "Magician", "�������"},
            { "Architect", "������"},
            { "Merchant", "�����"},
            { "Warlord", "���������"},
            { "King", "������"},
        };
    }

    public Dictionary<string, string> getCharDesc()
    {
        return new Dictionary<string, string>() {
            { "Assassin", "������� ������ ��������� �� ���� �� ���� �����"},
            { "Thief", "�������� ������� ������ � ������� ��������� (����� �������� � ��� ������)"},
            { "Bishop", "�������� ������ ������ �� ���������� � ���� ����� � ���������� �������� � ����� �� ����� ��������� ������ �����."},
            { "Magician", "��� ����������� �������� ����� ����� �� ������ � ����� �� ����, ���� �������� ��� ��������� � ���� � ���� ����� �� ��� �����, ��������� � ���� � ������� ������."},
            { "Architect", "������������� �������� �������������� ��� �������� �� ������ ����� ���������� �������� (�� ���� ����� ������ ����� ��� ���� ��������� ����� ����������) � ����� ��������� ������ �� ��� ��������� (��������� ��������� ������ ������ ���� ������� � ���)."},
            { "Merchant", "������������� �������� �������������� �������� ����� � 1 ������ � ����� �� ����� ��������� ������� �����."},
            { "Warlord", "�������� ����� � ������� (�������) ���������. � ����� ������ ���� ����� ��������� ������ ������ ������ (����� ��������) �� �����, �� 1 ������ �������, ��� ��������� ��������� ��������. �������� ���������� 1 ����� ��������� ���������."},
            { "King", "�������� �������������� ����� �� ����� ��������� ������������ (������) ����� � ��������� ��������� ������ ��������� ����� ��������� � ��������� ������."},
        };
    }

    public string getGameState()
    {
        return gameState;
    }

    public void generateDistrictDeck()
    {
        districtDeck = districts;
        districtDeck = districtDeck.OrderBy(x => random.Next()).ToArray();
    }

    public Dictionary<string, string> getDistName()
    {
        return new Dictionary<string, string>() {
            {"Tavern", "�������"},
            {"Market", "�����"},
            {"Trading Post", "�����"},
            {"Docks", "����"},
            {"Harbor", "������"},
            {"Town Hall", "������"},
            {"Temple", "����"},
            {"Church", "�������"},
            {"Monastery", "���������"},
            {"Cathedral", "�����"},
            {"Watchtower", "�������� �����"},
            {"Prison", "������"},
            {"Battlefield", "������� ����"},
            {"Fortress", "��������"},
            {"Manor", "��������"},
            {"Castle", "�����"},
            {"Palace", "������"},
            {"Haunted City", "����� ���������"},
            {"Keep", "����"},
            {"Laboratory", "�����������"},
            {"Smithy", "�����"},
            {"Observatory", "������������"},
            {"Graveyard", "��������"},
            {"School of Magic", "����� �����"},
            {"Library", "����������"},
            {"Great Wall", "������� �����"},
            {"University", "�����������"},
            {"Dragon Gate", "����� �������"},
        };
    }

    public Dictionary<string, string> getDistColor()
    {
        return new Dictionary<string, string>() {
            {"Tavern", "green"},
            {"Market", "green"},
            {"Trading Post", "green"},
            {"Docks", "green"},
            {"Harbor", "green"},
            {"Town Hall", "green"},
            {"Temple", "blue"},
            {"Church", "blue"},
            {"Monastery", "blue"},
            {"Cathedral", "blue"},
            {"Watchtower", "red"},
            {"Prison", "red"},
            {"Battlefield", "red"},
            {"Fortress", "red"},
            {"Manor", "yellow"},
            {"Castle", "yellow"},
            {"Palace", "yellow"},
            {"Haunted City", "purple"},
            {"Keep", "purple"},
            {"Laboratory", "purple"},
            {"Smithy", "purple"},
            {"Observatory", "purple"},
            {"Graveyard", "purple"},
            {"School of Magic", "purple"},
            {"Library", "purple"},
            {"Great Wall", "purple"},
            {"University", "purple"},
            {"Dragon Gate", "purple"},
        };
    }

    public Dictionary<string, int> getDistPrice()
    {
        return new Dictionary<string, int>() {
            {"Tavern", 1},
            {"Market", 2},
            {"Trading Post", 2},
            {"Docks", 3},
            {"Harbor", 4},
            {"Town Hall", 5},
            {"Temple", 1},
            {"Church", 2},
            {"Monastery", 3},
            {"Cathedral", 5},
            {"Watchtower", 1},
            {"Prison", 2},
            {"Battlefield", 3},
            {"Fortress", 5},
            {"Manor", 3},
            {"Castle", 4},
            {"Palace", 5},
            {"Haunted City", 2},
            {"Keep", 3},
            {"Laboratory", 5},
            {"Smithy", 5},
            {"Observatory", 5},
            {"Graveyard", 5},
            {"School of Magic", 6},
            {"Library", 6},
            {"Great Wall", 6},
            {"University", 8},
            {"Dragon Gate", 8},
        };
    }

    public Dictionary<string, string> getDistDesc()
    {
        return new Dictionary<string, string>() {
            {"Haunted City", "��� ��������� �������� ����� ����� ��������� ��������� ��������� ������ ���������� ����� �����. �� �������� ��� ��������, ���� �������� ���� ������� � ��������� �������"},
            {"Keep", "��������� �� ����� ���������� ����"},
            {"Laboratory", "��� � ���� ��� �� ������ �������� ����� �������� � ���� � �������� ���� ������� �� �����"},
            {"Smithy", "��� � ���� ��� �� ������ ��������� ��� ������� �� ����� �������� ��� ����� ��������"},
            {"Observatory", "���� �� ����� ��������� ������� ����� �����, ������ ��� �����, ������ ���� ���� �� �����, ������ ��� ���������� ��� ��� ������"},
            {"Graveyard", "����� ��������� ���������� �������, �� ������ ��������� ���� �������, ����� ������� ������������ ������� �� ����. �� �� ������ ����� ������, ���� �� ���������"},
            {"School of Magic", "��� ������� ������ ����� ��������� ��������� ������ ����� �� ������ ������. ��������, ���� � ������� ���� �� ������, ����� ����� ��������� ���������� (������) ���������"},
            {"Library", "���� �� ������� ����� ��������� ������ �����, �� ���������� �� ���� ��� ��������� �����"},
            {"Great Wall", "����� ��������� �����-���� �� ����� ���������, ���������� �������� ��������� �� ���� ������� ������"},
            {"University", "��������� ��������� ����� �������� 6 �������, � ����� ���� �� �������� 8 �����"},
            {"Dragon Gate", "���� ������� ����� 6 ������� ��� ��������� � �������� 8 ����� ��� �������� � ����� ����"},
        };
    }
}