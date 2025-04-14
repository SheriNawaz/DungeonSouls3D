using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
	public List<Item> itemPool = new List<Item>();
	public GameObject itemView;
	public GameObject weaponView;
	public GameObject icon;
	[SerializeField] GameObject weaponSlot;

	private PlayerInventory playerInventory;

	private void Start()
	{
		playerInventory = FindFirstObjectByType<PlayerInventory>();
		if (weaponSlot == null)
		{
			print("weaponSlot null");
		}
	}

	public void AddItem(Item item, GameObject view)
	{
		GameObject itemIcon = Instantiate(icon, view.transform);
		itemIcon.GetComponentInChildren<TextMeshProUGUI>().text = item.name;
		itemIcon.transform.Find("Image").GetComponent<Image>().sprite = item.sprite;
	}

	public void AddWeapon()
	{
		if (playerInventory == null)
		{
			playerInventory = FindFirstObjectByType<PlayerInventory>();
		}
		GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
		string weaponName = selectedObject.GetComponentInChildren<TextMeshProUGUI>().text;
		Item selectedWeapon = playerInventory.weapons.Find(weapon => weapon.name == weaponName);
		if(weaponSlot == null)
		{
			weaponSlot = GameObject.FindWithTag("WeaponSlot");
		}
		if (selectedWeapon != null)
		{
			Sprite weaponSprite = selectedObject.transform.Find("Image").GetComponent<Image>().sprite;
			weaponSlot.transform.Find("Image").GetComponent<Image>().sprite = weaponSprite;
			playerInventory.currentWeapon = selectedWeapon.gameObject;
			playerInventory.SwitchWeapon();
		}
	}
}
