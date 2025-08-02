using UnityEngine;
using TMPro;

public class Chair : MonoBehaviour
{
	public TMP_Text playerNameText;

	public void SetPlayerName(string name)
	{
		if (playerNameText != null)
		{
			playerNameText.text = name;
		}
	}
}
