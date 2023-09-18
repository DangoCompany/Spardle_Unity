using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MatchOverPanel : MonoBehaviour
{
    [SerializeField] private Text _resultText;
    public void SetResultText(bool isWinner)
    {
        _resultText.text = isWinner ? "Win!" : "Lose...";
    }

    public void OnClickReturnToHome()
    {
        // ルームから退出する
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Title");
    }
}
