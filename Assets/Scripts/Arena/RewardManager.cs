﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Responsible for presenting the reward options to the player and handle player's choice
public class RewardManager : MonoBehaviour
{
    public GameObject rewardUIParent;
    public GameObject[] choiceButtons;

    // Lists for the different rarities of items, if time allows should use item database to make it cleaner and
    // would also let database be responsible for item pools
    public List<so_Item> normalItems;
    public List<so_Item> rareItems;
    public List<so_Item> epicItems;

    //private List<so_Item> _itemPool;
    // Current item pool from which the rewards are chosen
    private List<List<so_Item>> _itemPool;
    private InventorySlot[] _icons;
    private TextMeshProUGUI[] _descriptions;

    [SerializeField] private so_NPCStats playerStats;
    [SerializeField] private so_Item nullItem;

    private so_Item _item1;
    private int _item1Rarity = 0;
    private so_Item _item2;
    private int _item2Rarity = 0;

    // first index indicates which stat is to be improved
    // second index indicates by how much
    private int[] _statReward1;
    private int[] _statReward2;
    // first and third index indicates which stat is to be improved
    // second and fourth index indicates by how much
    private int[] _statReward3;
    private int _healReward = 0;

    // Defines ranges for stats e.g. low stat reward is 1-3, average 3-5 etc.
    private int _lowStatStartRange = 1;
    private int _averageStatStartRange = 3;
    private int _highStatStartRange = 5;
    private int _veryHighStatStartRange = 8;
    private int _extremeStatStartRange = 11;

    private readonly string[] _statStrings = { " max HP", " attack damage", " base armor" };

    [SerializeField] private so_GameEvent onRewardUIOpen;
    [SerializeField] private so_GameEvent onRewardUIClose;

    private void Start()
    {
        _icons = new InventorySlot[choiceButtons.Length];
        _descriptions = new TextMeshProUGUI[choiceButtons.Length];

        for (int i = 0; i < 4; ++i)
        {
            _icons[i] = choiceButtons[i].GetComponentInChildren<InventorySlot>();
            foreach (TextMeshProUGUI t in choiceButtons[i].GetComponentsInChildren<TextMeshProUGUI>())
            {
                if (!t.gameObject.name.Equals("Description")) continue;
                _descriptions[i] = t;
                break;
            }
        }

        _itemPool = new List<List<so_Item>> {normalItems, rareItems, epicItems};

        _statReward1 = new int[2];
        _statReward2 = new int[2];
        _statReward3 = new int[4];

        DisableButtons();
    }

    /*
    private void Update()
    {
        // for testing
        if(Input.GetKeyDown(KeyCode.R))
        {
            OpenRewardScreen();
        }
    }*/

    public void OpenRewardScreen()
    {
        if (rewardUIParent.activeSelf)
        {
            return;
        }
        
        StartCoroutine(nameof(RewardSetup));
    }

    private IEnumerator RewardSetup()
    {
        RollNewItemRewards();
        RollStatReward();
        RollHealReward();
        EnableButtons();

        yield return new WaitForSeconds(1f);
        
        AudioManager.instance.PlayOneShotSound("RewardUI");
        rewardUIParent.SetActive(true);
    }

    public void PickReward(int whichReward)
    {
        int healAmount = 5;
        DisableButtons();
        // Need to add back the item(s) that were not chosen to be available for future rewards.
        switch (whichReward)
        {
            case 0:
                // Picked first item
                if (_item1 != null && _item1.itemType != ItemType.NULL)
                {
                    PlayerManager.instance.playerInventory.Add(_item1);
                    //_itemPool[_item1Rarity].Remove(_item1);
                    if (_item2 != null && _item2.itemType != ItemType.NULL)
                    {
                        _itemPool[_item2Rarity].Add(_item2);
                    }
                }

                ApplyStatReward(0);
                break;
            case 1:
                // Picked second item
                if (_item1 != null && _item1.itemType != ItemType.NULL)
                {
                    PlayerManager.instance.playerInventory.Add(_item2);
                    //_itemPool[_item2Rarity].Remove(_item2);
                    if (_item1 != null && _item1.itemType != ItemType.NULL)
                    {
                        _itemPool[_item1Rarity].Add(_item1);
                    }
                }

                ApplyStatReward(1);
                break;
            case 2:
                // Picked stat reward
                if (_item1 != null && _item1.itemType != ItemType.NULL)
                {
                    _itemPool[_item1Rarity].Add(_item1);
                }
                
                if (_item2 != null && _item2.itemType != ItemType.NULL)
                {
                    _itemPool[_item2Rarity].Add(_item2);
                }
                
                ApplyStatReward(2);
                break;
            case 3:
                // Picked the heal
                if (_item1 != null && _item1.itemType != ItemType.NULL)
                {
                    _itemPool[_item1Rarity].Add(_item1);
                }
                
                if (_item2 != null && _item2.itemType != ItemType.NULL)
                {
                    _itemPool[_item2Rarity].Add(_item2);
                }
                
                healAmount = _healReward;
                break;
            default:
                // Somehow picked non existent reward :(
                Debug.LogError("Picked reward with index out of bounds!");
                return;
        }

        _item1 = nullItem;
        _item2 = nullItem;
        _icons[0].AddItem(_item1);
        _icons[1].AddItem(_item2);
        
        rewardUIParent.SetActive(false);
        onRewardUIClose.Raise();

        //Debug.Log("Healing after picking reward!");
        PlayerManager.instance.player.GetComponent<PlayerController>().Heal(healAmount);
    }

    /*
     *  2 % chance for epic item
     *  68% chance for normal item
     *  30% chance for rare item
     *  Will give a reward from the rolled rarity pool, if no items left in that pool it will try to give from 1 lower rarity
     *  If no items found it will not present a reward
     * 
     */
    private void RollNewItemRewards()
    {
        int firstRanNum = 0;
        int itemRarity = 0;
        float probabilityRoll = Random.Range(0, 100);
        
        if (probabilityRoll > 97)
            itemRarity = 2;
        else if (probabilityRoll > 67)
            itemRarity = 1;
        
        while (itemRarity > 0 && _itemPool[itemRarity].Count < 1)
        {
            itemRarity--;
        }
        
        // If no items left assign dummy items to rewards
        // If 1 item left assign that to reward 1 and then assign dummy item to reward 2
        // Else just assign random items to the rewards
        if (_itemPool[itemRarity].Count < 1)
        {
            _item1 = nullItem;
            _item2 = nullItem;
        }
        else
        {
            do
            {
                firstRanNum = Random.Range(0, _itemPool[itemRarity].Count);
            } while (_itemPool[itemRarity][firstRanNum] == null);

            _item1 = _itemPool[itemRarity][firstRanNum];
            _item1Rarity = itemRarity;
            _itemPool[itemRarity].Remove(_item1);
            //allItems[ranNum] = null;

            itemRarity = 0;
            probabilityRoll = Random.Range(0, 100);
        
            if (probabilityRoll > 97)
                itemRarity = 2;
            else if (probabilityRoll > 67)
                itemRarity = 1;
        
            while (itemRarity > 0 && _itemPool[itemRarity].Count < 1)
            {
                itemRarity--;
            }
            
            if (_itemPool[itemRarity].Count < 1)
            {
                _item2 = nullItem;
            }
            else
            {
                int secondRanNum = 0;
                do
                {
                    secondRanNum = Random.Range(0, _itemPool[itemRarity].Count);
                } while (_itemPool[itemRarity][secondRanNum] == null);

                _item2 = _itemPool[itemRarity][secondRanNum];
                _item2Rarity = itemRarity;
                _itemPool[itemRarity].Remove(_item2);
                //allItems[ranNum] = null;
            }
        }

        _icons[0].AddItem(_item1);
        _icons[1].AddItem(_item2);
    }

    private void RollStatReward()
    {
        // assume no extra stat award was gained
        _statReward1[0] = -1;
        _statReward2[0] = -1;
        _statReward3[2] = -1;

        int randomNum = 0;
        int statValue = 0;
        int multiplier = 0;
        int actualStatValue = 0;

        // 0 = maxhp
        // 1 = damage
        // 2 = armor
        int whichStat = 0;

        // odds for additional stat reward with items
        // 5% for negative stat reward
        // 10% for positive stat reward
        // 85% for no stat reward

        // Additional stat with items
        // Currently only 2 item rewards planned.
        for (int i = 0; i < 2; ++i)
        {
            // If item reward is null then dont roll for additional stat for that item
            switch (i)
            {
                case 0 when _item1 == null || _item1 == nullItem:
                    continue;
                case 1 when _item2 == null || _item2 == nullItem:
                    continue;
            }

            randomNum = Random.Range(0, 100);
            if (randomNum < 5)
            {
                multiplier = -1;
            }
            else if (randomNum < 15)
            {
                multiplier = 1;
            }
            else
            {
                multiplier = 0;
            }

            // odds for level of stat reward
            // low       - 15%      1-2
            // average   - 70%      3-4
            // high      - 10%      5-7
            // very high - 4%       8-10
            // extreme   - 1%       11-12

            if (multiplier != 0)
            {
                statValue = GetStatValue();
                actualStatValue = statValue * multiplier;
                whichStat = Random.Range(0, 3);

                _descriptions[i].SetText("and\n" + actualStatValue + _statStrings[whichStat]);

                switch (i)
                {
                    case 0:
                        _statReward1[0] = whichStat;
                        _statReward1[1] = actualStatValue;
                        break;
                    case 1:
                        _statReward2[0] = whichStat;
                        _statReward2[1] = actualStatValue;
                        break;
                    default:
                        Debug.LogError("Error with assigning stat rewards to items.");
                        break;
                }
            }
            else
            {
                _descriptions[i].SetText("");
            }
        }

        // odds for stat reward i.e. third reward
        // 80% positive
        // 20% negative
        // odds for additional stat reward with the stat reward
        // 10% for additional stat reward
        // 30% for additional stat reward to be negative
        // 70% for additional stat reward to be positive

        randomNum = Random.Range(0, 100);
        // positive or negative stat reward
        if (randomNum < 20)
        {
            multiplier = -1;
        }
        else
        {
            multiplier = 1;
        }

        statValue = GetStatValue();

        actualStatValue = statValue * multiplier;
        whichStat = Random.Range(0, 3);

        string tempStr = actualStatValue + _statStrings[whichStat];
        _statReward3[0] = whichStat;
        _statReward3[1] = actualStatValue;

        // additional stat reward
        randomNum = Random.Range(0, 100);
        if (randomNum < 10)
        {
            randomNum = Random.Range(0, 100);
            if (randomNum < 30)
            {
                multiplier = -1;
            }
            else
            {
                multiplier = 1;
            }
        }
        else
        {
            multiplier = 0;
        }

        if (multiplier != 0)
        {
            statValue = GetStatValue();
            actualStatValue = statValue * multiplier;
            do
            {
                whichStat = Random.Range(0, 3);
            } while (_statReward3[0] == whichStat);

            tempStr += "\nand\n" + actualStatValue + _statStrings[whichStat];
            _statReward3[2] = whichStat;
            _statReward3[3] = actualStatValue;
        }

        _descriptions[2].SetText(tempStr);
    }

    private void RollHealReward()
    {
        // currently heal player for 25% of their max hp
        _healReward = playerStats.maxHp / 4 + 5;
        _descriptions[3].SetText("Heal for " + _healReward);
    }

    private void ApplyStatReward(int whichReward)
    {
        if(whichReward == 0)
        {
            switch(_statReward1[0])
            {
                case 0:
                    playerStats.UpdateMaxHp(_statReward1[1]);
                    break;
                case 1:
                    playerStats.UpdateBaseDamage(_statReward1[1]);
                    break;
                case 2:
                    playerStats.UpdateBaseArmor(_statReward1[1]);
                    break;
                default:
                    break;
            }
        }
        else if(whichReward == 1)
        {
            switch (_statReward2[0])
            {
                case 0:
                    playerStats.UpdateMaxHp(_statReward2[1]);
                    break;
                case 1:
                    playerStats.UpdateBaseDamage(_statReward2[1]);
                    break;
                case 2:
                    playerStats.UpdateBaseArmor(_statReward2[1]);
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (_statReward3[0])
            {
                case 0:
                    playerStats.UpdateMaxHp(_statReward3[1]);
                    break;
                case 1:
                    playerStats.UpdateBaseDamage(_statReward3[1]);
                    break;
                case 2:
                    playerStats.UpdateBaseArmor(_statReward3[1]);
                    break;
                default:
                    break;
            }

            switch (_statReward3[2])
            {
                case 0:
                    playerStats.UpdateMaxHp(_statReward3[3]);
                    break;
                case 1:
                    playerStats.UpdateBaseDamage(_statReward3[3]);
                    break;
                case 2:
                    playerStats.UpdateBaseArmor(_statReward3[3]);
                    break;
                default:
                    break;
            }
        }
    }

    private int GetStatValue()
    {
        int randomNum = Random.Range(0, 100);
        int statValue = 0;

        if (randomNum < 15)
        {
            statValue = Random.Range(_lowStatStartRange, _averageStatStartRange);
        }
        else if (randomNum < 85)
        {
            statValue = Random.Range(_averageStatStartRange, _highStatStartRange);
        }
        else if (randomNum < 95)
        {
            statValue = Random.Range(_highStatStartRange, _veryHighStatStartRange);
        }
        else if (randomNum < 99)
        {
            statValue = Random.Range(_veryHighStatStartRange, _extremeStatStartRange);
        }
        else
        {
            statValue = Random.Range(_extremeStatStartRange, _extremeStatStartRange + 2);
        }

        return statValue;
    }

    private void EnableButtons()
    {
        foreach(GameObject g in choiceButtons)
        {
            g.GetComponent<Button>().interactable = true;
        }

        if (_item1 == null || _item1 == nullItem)
            choiceButtons[0].GetComponent<Button>().interactable = false;
        
        if (_item2 == null || _item2 == nullItem)
            choiceButtons[1].GetComponent<Button>().interactable = false;
    }

    private void DisableButtons()
    {
        foreach (GameObject g in choiceButtons)
        {
            g.GetComponent<Button>().interactable = false;
        }
    }
}
