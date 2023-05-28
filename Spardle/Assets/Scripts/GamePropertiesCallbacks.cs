using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class GamePropertiesCallbacks : MonoBehaviourPunCallbacks
{
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {
        foreach (var prop in propertiesThatChanged) {
            if (prop.Key.Equals(DictionaryConstants.CustomPropertyKeysString[(int)ConfigConstants.CustomPropertyKey.IsMasterClientTurnKey]))
            {
                TurnManager.Instance.UpdateMyTurn((bool)prop.Value);
                Debug.Log($"{prop.Key}: {prop.Value}");
            }
        }
    }
}
