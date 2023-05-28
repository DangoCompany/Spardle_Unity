using System;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Pun;

public static class GameProperties
{
    private static readonly Hashtable RoomProperties = new Hashtable();

    public static async UniTask<object> GetCustomPropertyValueAsync(ConfigConstants.CustomPropertyKey customPropertyKey)
    {
        while (true)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(
                    DictionaryConstants.CustomPropertyKeysString[(int)customPropertyKey], out object value))
            {
                return value;
            }
            else
            {
                await UniTask.Delay(1);
            }
        }
    }

    public static object GetCustomPropertyValue(ConfigConstants.CustomPropertyKey customPropertyKey)
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(
                DictionaryConstants.CustomPropertyKeysString[(int)customPropertyKey], out object value))
        {
            return value;
        }
        else
        {
            throw new Exception("Custom Property Not Found");
        }
    }

    public static void SetCustomPropertyValue(ConfigConstants.CustomPropertyKey customPropertyKey, object value)
    {
        RoomProperties[DictionaryConstants.CustomPropertyKeysString[(int)customPropertyKey]] = value;
        PhotonNetwork.CurrentRoom.SetCustomProperties(RoomProperties);
    }
}