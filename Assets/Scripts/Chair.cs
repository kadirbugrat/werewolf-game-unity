using TMPro;
using UnityEngine;

public class Chair : MonoBehaviour
{
	private bool isDead = false;
	private string playerName = "";
	private string role = "";
	private TMP_Text label;

	private void Awake()
	{
		label = GetComponentInChildren<TMP_Text>();
	}

	public void SetPlayer(string name, string role, bool isSelf)
	{
		this.playerName = name;
		this.role = role;

		string displayName = isSelf ? $"{name} - {role}" : name;
		label.text = displayName;
		label.color = Color.green; // Hayattaki oyuncular yeþil görünür
	}

	public void SetPlayerName(string name, Color? color = null)
	{
		label.text = name;
		if (color.HasValue)
			label.color = color.Value;
	}

	public string GetPlayerName() => playerName;

	public void MarkAsDead()
	{
		isDead = true;
		label.color = Color.gray;
	}

	public bool IsAlive() => !isDead;
}
