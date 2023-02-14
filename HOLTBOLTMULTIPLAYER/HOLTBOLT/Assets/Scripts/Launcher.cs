using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;
    private void Awake()
    {
        instance = this;
    }

    public GameObject LoadingScreen;
    public TMP_Text LoadingText;

    public GameObject MenuButtons;

    public GameObject CreateRoomPanel;
    public TMP_InputField roomNameInput;

    public GameObject roomScreen;
    public TMP_Text roomName, PlayerLabel;
    public List<TMP_Text> allPlayerNames = new List<TMP_Text>();

    public TMP_Text errorText;

    public GameObject roomBrowserScreen;
    public RoomButtonScript theRoomButton;
    public List<RoomButtonScript> AllRoomButtons = new List<RoomButtonScript>();

    // Start is called before the first frame update
    void Start()
    {
        CloseMenus();

        LoadingScreen.SetActive(true);
        LoadingText.text = "Connecting to network...";

        PhotonNetwork.ConnectUsingSettings();
    }
    void CloseMenus()
    {
        CreateRoomPanel.SetActive(false);
        LoadingScreen.SetActive(false);
        MenuButtons.SetActive(false);
        roomScreen.SetActive(false);
        roomBrowserScreen.SetActive(false);
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();

        LoadingText.text = "Joining lobby...";
    }
    public override void OnJoinedLobby()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();
    }

    public void openRoomCreate()
    {
        CloseMenus();
        CreateRoomPanel.SetActive(true);
    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(roomNameInput.text))
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 8;
            PhotonNetwork.CreateRoom(roomNameInput.text,options);
            CloseMenus();
            LoadingText.text = "Creating Room...";
            LoadingScreen.SetActive(true);
        }
    }
    public override void OnJoinedRoom()
    {
        CloseMenus();
        roomScreen.SetActive(true);
        roomName.text = PhotonNetwork.CurrentRoom.Name;
        ListPlayers();
    }

    private void ListPlayers()
    {
        foreach(TMP_Text player in allPlayerNames)
        {
            Destroy(player.gameObject);
        }
        allPlayerNames.Clear();
        Player[] players = PhotonNetwork.PlayerList;
        for(int i = 0; i < players.Length; i++)
        {
            TMP_Text newLabel = Instantiate(PlayerLabel, PlayerLabel.transform.parent);
            newLabel.text = players[i].NickName;
            newLabel.gameObject.SetActive(true);
            allPlayerNames.Add(newLabel);
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        ListPlayers();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListPlayers();
    }
    /*public override void OnPlayerEnteredRoom(Player newPlayer)
{
   foreach (TMP_Text label in allPlayerNames)
   {
       Destroy(label.gameObject);
   }
   allPlayerNames.Clear();

   PlayerLabel.gameObject.SetActive(false);
   for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
   {
           TMP_Text playerLabel = Instantiate(PlayerLabel, PlayerLabel.transform.parent);
           PlayerLabel.text = newPlayer.NickName;
           PlayerLabel.gameObject.SetActive(true);
           allPlayerNames.Add(PlayerLabel);
   }
} */

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CloseMenus();
        CreateRoomPanel.SetActive(true);
        errorText.text = $"Failed to create a room: {message}";
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        LoadingText.text = "Leaving room...";
        LoadingScreen.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
    }
    public void openRoomBrowser()
    {
        CloseMenus();
        roomBrowserScreen.SetActive(true);
    }
    public void closeRoomBrowser()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(RoomButtonScript rb in AllRoomButtons)
        {
            Destroy(rb.gameObject);
        }
        AllRoomButtons.Clear();

        theRoomButton.gameObject.SetActive(false);
        for(int i = 0; i < roomList.Count; i++)
        {
            if(roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButtonScript roomButton = Instantiate(theRoomButton, theRoomButton.transform.parent);
                roomButton.SetButtonDetails(roomList[i]);
                roomButton.gameObject.SetActive(true);
                AllRoomButtons.Add(roomButton);

            }
        }
    }
 
    public void JoinRoom(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);
        CloseMenus();
        LoadingText.text = "Joining room";
        LoadingScreen.SetActive(true);

    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
