using NUnit.Framework.Internal;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
	public GameObject currentWeapon;
	public List<Item> items;
	public List<Item> weapons;
	public GameObject weaponSlot;
	public GameObject startingWeapon;
	public Image currentWeaponSlot;
	public Item key;
	public Item map;

	private InventoryManager inventoryManager;
	private PlayerController player;

	private void Start()
	{
		inventoryManager = FindFirstObjectByType<InventoryManager>();
		currentWeapon = startingWeapon;
		Instantiate(currentWeapon, weaponSlot.transform);
		inventoryManager.AddItem(currentWeapon.GetComponent<Item>(), inventoryManager.itemView);
		inventoryManager.AddItem(currentWeapon.GetComponent<Item>(), inventoryManager.weaponView);
		items.Add(currentWeapon.GetComponent<Item>());
		weapons.Add(currentWeapon.GetComponent<Item>());
		player = GetComponent<PlayerController>();
	}

	private void Update()
	{
		currentWeaponSlot.sprite = currentWeapon.GetComponent<Item>().sprite;
	}

	public void SwitchWeapon()
	{
		Transform oldWeapon = weaponSlot.transform.GetChild(0);
		Destroy(oldWeapon.gameObject);
		Instantiate(currentWeapon, weaponSlot.transform);
	}

	private void OnCollisionStay(Collision collision)
	{
		if(collision.gameObject.tag == "Chest" && Input.GetKey(KeyCode.E))
		{
			Destroy(collision.gameObject);
			float healthChance = Random.Range(0f, 10f);

			int goldGained = Random.Range(0, 100);
			if(healthChance <= 1)
			{
				player.healthPots++;
				player.maxPotions++;
			}
			else
			{
				int randomIndex = Random.Range(0, inventoryManager.itemPool.Count);
				Item randomItem = inventoryManager.itemPool[randomIndex];
				inventoryManager.AddItem(randomItem, inventoryManager.itemView);
				items.Add(randomItem);
				if (randomItem.gameObject.tag == "Sword")
				{
					inventoryManager.AddItem(randomItem, inventoryManager.weaponView);
					weapons.Add(randomItem);
				}
				player.currentGold += goldGained;
			}
		}
		else if (collision.gameObject.tag == "Key" && Input.GetKey(KeyCode.E))
		{
			Destroy(collision.gameObject);
			player.hasBossKey = true;
			inventoryManager.AddItem(key, inventoryManager.itemView);
		}
		else if (collision.gameObject.tag == "Map" && Input.GetKey(KeyCode.E))
		{
			Destroy(collision.gameObject);
			player.hasMapFragment = true;
			inventoryManager.AddItem(map, inventoryManager.itemView);
		}
	}

}