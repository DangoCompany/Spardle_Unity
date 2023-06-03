using UnityEngine;

public class ActionButton : MonoBehaviour
{
    [SerializeField] private GameObject[] _buttons;

    public void SetButtonPosition(int[] buttonColors)
    {
        (_buttons[0].transform.rotation, _buttons[1].transform.rotation, _buttons[2].transform.rotation) = (
            _buttons[buttonColors[0]].transform.rotation,
            _buttons[buttonColors[1]].transform.rotation,
            _buttons[buttonColors[2]].transform.rotation);
    }
}