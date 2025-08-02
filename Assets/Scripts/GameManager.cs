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

	private bool isNight = false;
	private List<string> availableTargets = new List<string>();
	private string selectedTarget = "";

	private string myRole = "";

	private string[] playerNames = { "Sen", "Bot1", "Bot2" };
	private string[] roles = { "Köylü", "Vampir", "Büyücü" };
	private List<string> shuffledRoles = new List<string>();

	void Start()
	{
		shuffledRoles = new List<string>(roles);
		ShuffleList(shuffledRoles);
		SpawnChairs();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.N))
			SetNight(true);
		else if (Input.GetKeyDown(KeyCode.M))
			SetNight(false);
	}

	void SetNight(bool night)
	{
		isNight = night;
		nightOverlay.SetActive(night);

		if (night)
		{
			nightActionPanel.SetActive(true);

			if (myRole == "Vampir" || myRole == "Büyücü")
			{
				nightRoleText.text = $"{myRole}'sin! Birini seç.";
				submitButton.interactable = true;

				// Dropdown'u göster ve doldur
				targetDropdown.gameObject.SetActive(true);
				PopulateDropdown();
			}
			else
			{
				nightRoleText.text = $"{myRole} rolündesin. Gece bekleniyor...";
				submitButton.interactable = false;

				// Dropdown'u gizle
				targetDropdown.gameObject.SetActive(false);
			}
		}
		else
		{
			nightActionPanel.SetActive(false);
		}
	}


	void PopulateDropdown()
	{
		targetDropdown.ClearOptions();
		availableTargets.Clear();

		foreach (string name in playerNames)
		{
			if (name != "Sen")
			{
				availableTargets.Add(name);
			}
		}

		targetDropdown.AddOptions(availableTargets);
		targetDropdown.onValueChanged.RemoveAllListeners(); // 🔄 tekrar tekrar eklenmesin
		targetDropdown.onValueChanged.AddListener(delegate { OnDropdownValueChanged(); });
	}


	void OnDropdownValueChanged()
	{
		int index = targetDropdown.value;
		selectedTarget = availableTargets[index];
	}

	public void OnSubmitTarget()
	{
		Debug.Log($"{myRole} → hedefi: {selectedTarget}");
		SetNight(false);
	}

	void SpawnChairs()
	{
		float radius = 200f;
		int count = playerNames.Length;

		for (int i = 0; i < count; i++)
		{
			float angle = i * Mathf.PI * 2 / count;
			Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

			GameObject chair = Instantiate(chairPrefab, tableTransform);
			RectTransform rt = chair.GetComponent<RectTransform>();
			rt.anchoredPosition = pos;

			Chair chairScript = chair.GetComponent<Chair>();
			if (chairScript != null)
			{
				string role = shuffledRoles[i];
				string displayName = $"{playerNames[i]} - {role}";
				chairScript.SetPlayerName(displayName);

				if (playerNames[i] == "Sen")
					myRole = role;
			}
		}
	}

	void ShuffleList(List<string> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			int rand = Random.Range(i, list.Count);
			string temp = list[i];
			list[i] = list[rand];
			list[rand] = temp;
		}
	}
}
