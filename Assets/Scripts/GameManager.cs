using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
	public GameObject chairPrefab;
	public RectTransform tableTransform;

	public GameObject nightOverlay;
	public GameObject nightActionPanel;
	public TMP_Text nightRoleText;
	public Button submitButton;
	public TMP_Dropdown targetDropdown;

	public GameObject dayVotePanel;
	public TMP_Text voteText;
	public TMP_Dropdown voteDropdown;
	public Button voteSubmitButton;

	public TMP_Text gameOverText;

	private bool isNight = false;
	private bool isGameOver = false;
	private int currentDay = 1;

	private string myRole = "";
	private string selectedTarget = "";
	private string vampireTarget = "";
	private string wizardSaveTarget = "";

	private bool playerSubmitted = false;

	private string[] playerNames = { "Sen", "Bot1", "Bot2", "Bot3", "Bot4" };
	private string[] roles = { "Vampir", "Büyücü", "Köylü", "Köylü", "Köylü" };
	private List<string> shuffledRoles = new();

	private Dictionary<string, Chair> playerChairs = new();
	private Dictionary<string, string> playerRoles = new();
	private Dictionary<string, int> voteCounts = new();

	void Start()
	{
		shuffledRoles = new List<string>(roles);
		ShuffleList(shuffledRoles);
		SpawnChairs();
		SetNight(true);

		// Eğer oyuncu köylüyse otomatik olarak 'hazır' say
		if (myRole != "Vampir" && myRole != "Büyücü")
		{
			playerSubmitted = true;
			TryResolveNight();
		}
	}



	void ShuffleList(List<string> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			int rand = Random.Range(i, list.Count);
			(list[i], list[rand]) = (list[rand], list[i]);
		}
	}

	void SpawnChairs()
	{
		float radius = 200f;
		for (int i = 0; i < playerNames.Length; i++)
		{
			float angle = i * Mathf.PI * 2 / playerNames.Length;
			Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

			GameObject chair = Instantiate(chairPrefab, tableTransform);
			chair.GetComponent<RectTransform>().anchoredPosition = pos;

			string name = playerNames[i];
			string role = shuffledRoles[i];
			playerRoles[name] = role;

			Chair chairScript = chair.GetComponent<Chair>();
			chairScript.SetPlayer(name, role, name == "Sen");
			playerChairs[name] = chairScript;

			if (name == "Sen")
				myRole = role;
		}
	}

	void SetNight(bool night)
	{
		isNight = night;
		nightOverlay.SetActive(night);
		nightActionPanel.SetActive(night);

		if (!night) return;

		if (myRole == "Vampir" || myRole == "Büyücü")
		{
			nightRoleText.text = $"{myRole}'sin! Birini seç.";
			submitButton.interactable = true;
			targetDropdown.gameObject.SetActive(true);
			PopulateDropdown();
		}
		else
		{
			nightRoleText.text = $"{myRole} rolündesin. Gece bekleniyor...";
			submitButton.interactable = false;
			targetDropdown.gameObject.SetActive(false);
		}
	}

	void PopulateDropdown()
	{
		targetDropdown.ClearOptions();
		List<string> targets = new();

		foreach (var name in playerNames)
		{
			if (name != "Sen" && playerChairs[name].IsAlive())
				targets.Add(name);
		}

		targetDropdown.AddOptions(targets);
		targetDropdown.onValueChanged.RemoveAllListeners();
		targetDropdown.onValueChanged.AddListener(delegate { selectedTarget = targets[targetDropdown.value]; });
	}

	public void OnSubmitTarget()
	{
		if (myRole == "Vampir") vampireTarget = selectedTarget;
		else if (myRole == "Büyücü") wizardSaveTarget = selectedTarget;

		playerSubmitted = true;
		TryResolveNight();
	}

	void TryResolveNight()
	{
		// Botlar seçimlerini yapar
		foreach (var name in playerNames)
		{
			if (name == "Sen" || !playerChairs[name].IsAlive()) continue;
			var alive = GetAlivePlayers(name);
			string role = playerRoles[name];

			if (role == "Vampir")
				vampireTarget = alive[Random.Range(0, alive.Count)];
			else if (role == "Büyücü")
				wizardSaveTarget = alive[Random.Range(0, alive.Count)];
		}

		// Eğer oyuncu rolü pasifse veya submit ettiyse gece çözülür
		if ((myRole != "Vampir" && myRole != "Büyücü") || playerSubmitted)
		{
			SetNight(false);
			ResolveNightActions();
			StartDayVoting();
			playerSubmitted = false;
		}
	}


	List<string> GetAlivePlayers(string exclude = "")
	{
		List<string> alive = new();
		foreach (var name in playerNames)
		{
			if (name != exclude && playerChairs[name].IsAlive())
				alive.Add(name);
		}
		return alive;
	}

	void ResolveNightActions()
	{
		if (!string.IsNullOrEmpty(vampireTarget))
		{
			if (vampireTarget != wizardSaveTarget)
			{
				playerChairs[vampireTarget].MarkAsDead();
			}
		}

		vampireTarget = "";
		wizardSaveTarget = "";
	}

	void StartDayVoting()
	{
		voteCounts.Clear();
		foreach (var name in playerNames)
		{
			if (!playerChairs[name].IsAlive()) continue;

			if (name == "Sen")
			{
				OpenDayVotePanel();
				return;
			}
			else
			{
				var alive = GetAlivePlayers(name);
				var vote = alive[Random.Range(0, alive.Count)];
				voteCounts[vote] = voteCounts.ContainsKey(vote) ? voteCounts[vote] + 1 : 1;
			}
		}

		ProcessVoteResults();
	}

	void OpenDayVotePanel()
	{
		dayVotePanel.SetActive(true);
		voteText.text = "Gündüz: Oy kullan";
		List<string> options = GetAlivePlayers("Sen");
		voteDropdown.ClearOptions();
		voteDropdown.AddOptions(options);
		voteSubmitButton.onClick.RemoveAllListeners();
		voteSubmitButton.onClick.AddListener(() =>
		{
			string selected = options[voteDropdown.value];
			voteCounts[selected] = voteCounts.ContainsKey(selected) ? voteCounts[selected] + 1 : 1;
			dayVotePanel.SetActive(false);
			ProcessVoteResults();
		});
	}

	void ProcessVoteResults()
	{
		List<string> top = new();
		int max = -1;
		foreach (var kvp in voteCounts)
		{
			if (kvp.Value > max)
			{
				top = new List<string> { kvp.Key };
				max = kvp.Value;
			}
			else if (kvp.Value == max)
			{
				top.Add(kvp.Key);
			}
		}

		if (top.Count == 1)
		{
			string voted = top[0];
			playerChairs[voted].MarkAsDead();
			playerChairs[voted].SetPlayerName(voted + " (linç) - " + playerRoles[voted], Color.gray);
		}

		CheckGameOver();
		if (!isGameOver)
		{
			currentDay++;
			SetNight(true);
		}
	}

	void CheckGameOver()
	{
		int vampir = 0, diger = 0;
		foreach (var kvp in playerChairs)
		{
			if (!kvp.Value.IsAlive()) continue;
			if (playerRoles[kvp.Key] == "Vampir") vampir++;
			else diger++;
		}

		if (vampir == 0)
		{
			gameOverText.text = "Köylüler kazandı!";
			gameOverText.gameObject.SetActive(true);
			isGameOver = true;
		}
		else if (vampir >= diger)
		{
			gameOverText.text = "Vampirler kazandı!";
			gameOverText.gameObject.SetActive(true);
			isGameOver = true;
		}
	}
}
