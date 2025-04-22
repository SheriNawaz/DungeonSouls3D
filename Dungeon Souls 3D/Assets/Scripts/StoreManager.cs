using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    public List<Item> stock;
	[SerializeField] InventoryManager inventory;
	[SerializeField] GameObject stockView;
	[SerializeField] Item healthPotion;
	[SerializeField] GameObject shopIcon;
	[SerializeField] List<Item> sellableItems;
	public bool isPopulated = false;

	private void Start()
	{
		inventory = FindFirstObjectByType<InventoryManager>();
	}

	public void PopulateStore()
	{
		if (!isPopulated)
		{
			//Create buttons with information of each item in the shop
			isPopulated = true;
			stockView = GameObject.FindWithTag("StoreView");
			foreach (Transform child in stockView.transform)
			{
				Destroy(child.gameObject);
			}
			foreach (var item in stock)
			{
				GameObject itemButton = Instantiate(shopIcon, stockView.transform);
				itemButton.GetComponentInChildren<TextMeshProUGUI>().text = item.value.ToString() + " Gold";
				itemButton.transform.Find("Image").GetComponent<Image>().sprite = item.sprite;
				itemButton.name = item.itemName;
			}
			GameObject health = Instantiate(shopIcon, stockView.transform);
			health.GetComponentInChildren<TextMeshProUGUI>().text = healthPotion.value.ToString() + " Gold";
			health.transform.Find("Image").GetComponent<Image>().sprite = healthPotion.sprite;
			health.name = healthPotion.itemName;
		}
	}

	public void Buy() //Function for player buying something
	{
		GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
		InventoryManager inventory = FindFirstObjectByType<InventoryManager>();
		PlayerInventory playerInventory = FindFirstObjectByType<PlayerInventory>();
		PlayerController player = FindFirstObjectByType<PlayerController>();

		if ((selectedObject.GetComponentInChildren<TextMeshProUGUI>().text).Split(' ')[0] == "1500" && player.currentGold >= 1500)
		{
			player.maxPotions++;
			player.healthPots++;
			player.currentGold -= 1500;
			Destroy(selectedObject);

		}
		else
		{
			//Find the item using the name of the item selected. Add it to inventory and weapons menu
			string selectedItemValue = (selectedObject.GetComponentInChildren<TextMeshProUGUI>().text).Split(' ')[0];
			Item selectedItem = sellableItems.Find(item => item.itemName == selectedObject.name);
			if(player.currentGold >= int.Parse(selectedItemValue))
			{
				playerInventory.items.Add(selectedItem);
				inventory.AddItem(selectedItem, inventory.itemView);
				inventory.AddToShopInv(selectedItem, inventory.shopInvView);
				if (selectedItem.gameObject.tag == "Sword")
				{
					playerInventory.weapons.Add(selectedItem);
				}
				player.currentGold -= int.Parse(selectedItemValue);
				Destroy(selectedObject);
			}
		}
	}

	public void Sell() //Function for player to sell items to merchant
	{
		GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
		InventoryManager inventory = FindFirstObjectByType<InventoryManager>();
		PlayerInventory playerInventory = FindFirstObjectByType<PlayerInventory>();
		PlayerController player = FindFirstObjectByType<PlayerController>();
		//Get value and information about selected item
		string selectedItemValue = (selectedObject.GetComponentInChildren<TextMeshProUGUI>().text).Split(' ')[0];
		print(selectedObject.name);
		Item selectedItem = sellableItems.Find(item => item.itemName == selectedObject.name);
		print(selectedItem.itemName);
		playerInventory.items.Remove(selectedItem);
		if (selectedItem.gameObject.tag == "Sword")
		{
			//Ensure that the weapon is removed from inventory, and if the weapon is equipped and is the only one of that type that the player has, the weapon is swapped
			int counter = 0;
			foreach (var item in playerInventory.weapons)
			{
				if(item == selectedItem)
				{
					counter++;
				
				}
			}
			print(counter + playerInventory.currentWeapon.GetComponent<Item>().itemName + selectedItem.itemName);
			if(counter <= 1 && playerInventory.currentWeapon.GetComponent<Item>().itemName == selectedItem.itemName)
			{
				playerInventory.currentWeapon = playerInventory.startingWeapon;
				playerInventory.weaponSlot.transform.Find("Image").GetComponent<Image>().sprite = playerInventory.startingWeapon.GetComponent<Item>().sprite;
				playerInventory.SwitchWeapon();
				print("Multiple detected");
			}
			playerInventory.weapons.Remove(selectedItem);
			//Clear from weapon view;
			Transform weaponView = inventory.weaponView.transform;
			for (int i = 0; i < weaponView.childCount; i++)
			{
				Transform child = weaponView.GetChild(i);
				Image itemImage = child.Find("Image")?.GetComponent<Image>();

				if (itemImage != null && itemImage.sprite == selectedItem.sprite)
				{
					Destroy(child.gameObject);
					break;
				}
			}
		}
		inventory.AddToBuyInv(selectedItem, inventory.sellView);
		//Clear from inventory
		Transform inventoryViewTransform = inventory.itemView.transform;
		for (int i = 0; i < inventoryViewTransform.childCount; i++)
		{
			Transform child = inventoryViewTransform.GetChild(i);
			Image itemImage = child.Find("Image")?.GetComponent<Image>();

			if (itemImage != null && itemImage.sprite == selectedItem.sprite)
			{
				Destroy(child.gameObject);
				break; 
			}
		}

		player.currentGold += int.Parse(selectedItemValue);
		Destroy(selectedObject);
	}
}
