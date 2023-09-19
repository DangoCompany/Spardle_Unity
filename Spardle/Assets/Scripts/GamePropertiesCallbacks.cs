using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class GamePropertiesCallbacks : MonoBehaviourPunCallbacks
{
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {
        foreach (var prop in propertiesThatChanged) {
            Debug.Log($"(Key: Value) = ({prop.Key}: {prop.Value})");
            if (prop.Key.Equals(DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsMasterClientTurnKey]))
            {
                TurnManager.Instance.UpdateMyTurn((bool)prop.Value);
            }

            if ((int)prop.Value == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                if ((string)prop.Key == DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsCardPlayingKey])
                {
                    CardManager.Instance.PlayCard();
                }
                else if ((string)prop.Key == DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsWrongPlayingKey])
                {
                    CardManager.Instance.PlayWrongly();
                }
                else if ((string)prop.Key == DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsActionInProgressKey])
                {
                    CardManager.Instance.OnReceiveCardAction();
                }
            }
        }
    }
}
