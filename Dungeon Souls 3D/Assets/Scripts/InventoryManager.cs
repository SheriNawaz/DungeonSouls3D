using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
	public List<Item> itemPool = new List<Item>();
	public GameObject itemView;
	public GameObject shopInvView;
	public GameObject weaponView;
	public GameObject sellView;
	public GameObject icon;
	public GameObject shopInvIcon;
	public GameObject sellIcon;
	[SerializeField] GameObject weaponSlot;
	[SerializeField] GameObject charmSlot;
	public GameObject shopIcon;

	private PlayerInventory playerInventory;
	public GameObject charmView;

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
		//Add item icon to inventory 
		GameObject itemIcon = Instantiate(icon, view.transform);
		itemIcon.GetComponentInChildren<TextMeshProUGUI>().text = item.name;
		itemIcon.transform.Find("Image").GetComponent<Image>().sprite = item.sprite;
		itemIcon.name = item.itemName;
	}

	public void AddToShopInv(Item item, GameObject view)
	{
		//Add an item to the inventory whilst in merchant shop
		GameObject itemIcon = Instantiate(sellIcon, view.transform);
		itemIcon.GetComponentInChildren<TextMeshProUGUI>().text = item.value.ToString() + " Gold";
		itemIcon.transform.Find("Image").GetComponent<Image>().sprite = item.sprite;
		itemIcon.name = item.itemName;
	}

	public void AddToBuyInv(Item item, GameObject view)
	{
		//Add an item to the merchants shop menu
		GameObject itemIcon = Instantiate(shopIcon, view.transform);
		itemIcon.GetComponentInChildren<TextMeshProUGUI>().text = item.value.ToString() + " Gold";
		itemIcon.transform.Find("Image").GetComponent<Image>().sprite = item.sprite;
		itemIcon.name = item.itemName;
	}
	public void AddWeapon()
	{
		//Add weapon to weapon slot
		if (playerInventory == null)
		{
			playerInventory = FindFirstObjectByType<PlayerInventory>();
		}
		GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
		string weaponName = selectedObject.GetComponentInChildren<TextMeshProUGUI>().text;
		//Locate weapon in weapons list using the name as the search key
		Item selectedWeapon = playerInventory.weapons.Find(weapon => weapon.name == weaponName);
		if(weaponSlot == null)
		{
			weaponSlot = GameObject.FindWithTag("WeaponSlot");
		}
		if (selectedWeapon != null)
		{
			//Add weapon information to weapon slot icon
			Sprite weaponSprite = selectedObject.transform.Find("Image").GetComponent<Image>().sprite;
			weaponSlot.transform.Find("Image").GetComponent<Image>().sprite = weaponSprite;
			playerInventory.currentWeapon = selectedWeapon.gameObject;
			playerInventory.SwitchWeapon();
		}
	}

	public void AddCharm()
	{
		//Function to add a charm from inventory to charmslot
		playerInventory = FindFirstObjectByType<PlayerInventory>();
		GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
		string charmName = selectedObject.GetComponentInChildren<TextMeshProUGUI>().text;
		//Locate the charm Item from inventory 
		Item selectedCharm = playerInventory.items.Find(charm => charm.name == charmName);
		charmSlot = GameObject.FindWithTag("CharmSlot");
		if (selectedCharm != null && selectedCharm.tag == "Charm")
		{
			//Put charm in charmslot
			Image image = charmSlot.transform.Find("Image").GetComponent<Image>();
			Sprite charmSprite = selectedObject.transform.Find("Image").GetComponent<Image>().sprite;
			Color color = image.color;
			color.a = 255;
			image.color = color;
			image.sprite = charmSprite;
			FindFirstObjectByType<PlayerController>().UseCharmStats(charmSlot.GetComponentInChildren<TMP_Text>().text, selectedCharm.itemName);
			charmSlot.GetComponentInChildren<TMP_Text>().text = charmName;
		}
	}
}
