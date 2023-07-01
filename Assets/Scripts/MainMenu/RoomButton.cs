using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour {

    [Header("Room Data")]
    private RoomInfo roomInfo;
    private Button button;

    [Header("UI References")]
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text currentPlayersText;
    [SerializeField] private TMP_Text maxPlayersText;

    private void Start() {

        button = GetComponent<Button>();
        button.onClick.AddListener(JoinRoom);

    }

    public void Initialize(RoomInfo roomInfo) {

        this.roomInfo = roomInfo;

        roomNameText.text = "Room Name: " + roomInfo.Name;
        currentPlayersText.text = "Current Players: " + roomInfo.PlayerCount + "";
        maxPlayersText.text = "Max Players: " + roomInfo.MaxPlayers + "";

    }

    private void JoinRoom() {

        PhotonNetwork.JoinRoom(roomInfo.Name);

    }
}
