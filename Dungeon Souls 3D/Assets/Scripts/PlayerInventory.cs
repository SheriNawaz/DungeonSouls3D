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
	public GameObject mapIcon;
	public GameObject minimapCover;

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
		//Switch weapons 
		Transform oldWeapon = weaponSlot.transform.GetChild(0);
		Destroy(oldWeapon.gameObject);
		Instantiate(currentWeapon, weaponSlot.transform);
	}

	private void OnCollisionStay(Collision collision)
	{
		if(collision.gameObject.tag == "Chest" && Input.GetKey(KeyCode.E))
		{
			//When opening a chest
			Destroy(collision.gameObject);
			float healthChance = Random.Range(0f, 10f);

			int goldGained = Random.Range(0, 100);
			if(healthChance <= 1)
			{
				//Either increase max potions
				player.healthPots++;
				player.maxPotions++;
			}
			else
			{
				//Or give the player a random item from the itempool
				int randomIndex = Random.Range(0, inventoryManager.itemPool.Count);
				Item randomItem = inventoryManager.itemPool[randomIndex];
				inventoryManager.AddItem(randomItem, inventoryManager.itemView);
				inventoryManager.AddToShopInv(randomItem, inventoryManager.shopInvView);
				items.Add(randomItem);
				if (randomItem.gameObject.tag == "Sword")
				{
					inventoryManager.AddItem(randomItem, inventoryManager.weaponView);
					weapons.Add(randomItem);
				}
				else if (randomItem.gameObject.tag == "Charm")
				{
					inventoryManager.AddItem(randomItem, inventoryManager.charmView);
				}
				player.currentGold += goldGained;
			}
		}
		else if (collision.gameObject.tag == "Key" && Input.GetKey(KeyCode.E))
		{
			//When opening Key chest, give player the key to the bossroom
			Destroy(collision.gameObject);
			player.hasBossKey = true;
			inventoryManager.AddItem(key, inventoryManager.itemView);
		}
		else if (collision.gameObject.tag == "Map" && Input.GetKey(KeyCode.E))
		{
			//When opening Map chest, set the icon in GUI for map to truem remove the minimap fog, and allow for the player to open the map
			Destroy(collision.gameObject);
			mapIcon.SetActive(true);
			player.hasMapFragment = true;
			inventoryManager.AddItem(map, inventoryManager.itemView);
			minimapCover.SetActive(false);
		}
		else if (collision.gameObject.tag == "Merchant" && Input.GetKey(KeyCode.E))
		{
			//Open merchant shop menu
			print("Accessing Merchant");
			PauseMenu pauseMenu = FindFirstObjectByType<PauseMenu>();
			pauseMenu.EnterShop();
			StoreManager storeManager = collision.gameObject.GetComponent<StoreManager>();
			if (storeManager != null)
			{
				storeManager.PopulateStore();
			}
		}
		else if (collision.gameObject.tag == "Spawn" && Input.GetKey(KeyCode.E))
		{
			//Reset health, stamina, health potions. Mark spawnpoint as visited. Open spawn UI
			SpawnPoint spawnPoint = collision.gameObject.GetComponent<SpawnPoint>();
			player.currentHealth = player.maxHealth;
			player.currentStamina = player.maxStamina;
			player.healthPots = player.maxPotions;
			player.currentSpawnPoint = spawnPoint;
			spawnPoint.MarkAsVisited();
			FindFirstObjectByType<SpawnpointManager>().CreateSpawnUI();
			PauseMenu pauseMenu = FindFirstObjectByType<PauseMenu>();
			pauseMenu.EnterSpawn();
		}
	}

}