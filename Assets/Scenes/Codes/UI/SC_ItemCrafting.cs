using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SlotContainer
{
    public Sprite itemSprite; // �������� ��������Ʈ (������ �迭�� �ִ� �Ͱ� �����ؾ� ��), �Ǵ� null�� �θ� ������ ����
    public int itemCount; // �� ���Կ� �ִ� ������ ��, 1 ���ϸ� 1���� ���������� �ؼ���
    [HideInInspector]
    public int tableID;
    [HideInInspector]
    public SC_SlotTemplate slot;
}

[System.Serializable]
public class Item
{
    public Sprite itemSprite;
    public bool stackable = false; // �� �������� ����(���� ��) �ֳ���?
    public string craftRecipe; // �� �������� �����ϱ� ���� �ʿ��� ������ Ű, ��ǥ�� ���� (��: �÷��� ��忡�� ���� ��ư�� ����ϰ� �ֿܼ��� �μ�� �����Ǹ� Ȯ���ϼ���)
}

public class SC_ItemCrafting : MonoBehaviour
{
    public RectTransform playerSlotsContainer;
    public RectTransform craftingSlotsContainer;
    public RectTransform resultSlotContainer;
    public Button craftButton;
    public SC_SlotTemplate slotTemplate;
    public SC_SlotTemplate resultSlotTemplate;



    public SlotContainer[] playerSlots;
    SlotContainer[] craftSlots = new SlotContainer[9];
    SlotContainer resultSlot = new SlotContainer();
    // ��� ������ ��� ������ ���
    public Item[] items;

    SlotContainer selectedItemSlot = null;

    int craftTableID = -1; // �������� �ϳ��� ��ġ�� ���̺��� ID (��: ���� ���̺�)
    int resultTableID = -1; // �������� ������ �� ������ ���� �� ���� ���̺��� ID

    ColorBlock defaultButtonColors;

    // ù ��° ������ ������Ʈ ���� ȣ��˴ϴ�
    void Start()
    {
        // ���� ��� ���ø� ����
        slotTemplate.container.rectTransform.pivot = new Vector2(0, 1);
        slotTemplate.container.rectTransform.anchorMax = slotTemplate.container.rectTransform.anchorMin = new Vector2(0, 1);
        slotTemplate.craftingController = this;
        slotTemplate.gameObject.SetActive(false);
        // ��� ���� ��� ���ø� ����
        resultSlotTemplate.container.rectTransform.pivot = new Vector2(0, 1);
        resultSlotTemplate.container.rectTransform.anchorMax = resultSlotTemplate.container.rectTransform.anchorMin = new Vector2(0, 1);
        resultSlotTemplate.craftingController = this;
        resultSlotTemplate.gameObject.SetActive(false);

        // ���� ��ư�� Ŭ�� �̺�Ʈ ����
        craftButton.onClick.AddListener(PerformCrafting);
        // ���� ��ư �⺻ ���� ����
        defaultButtonColors = craftButton.colors;

        // ���� ���� �ʱ�ȭ
        InitializeSlotTable(craftingSlotsContainer, slotTemplate, craftSlots, 5, 0);
        UpdateItems(craftSlots);
        craftTableID = 0;

        // �÷��̾� ���� �ʱ�ȭ
        InitializeSlotTable(playerSlotsContainer, slotTemplate, playerSlots, 5, 1);
        UpdateItems(playerSlots);

        // ��� ���� �ʱ�ȭ
        InitializeSlotTable(resultSlotContainer, resultSlotTemplate, new SlotContainer[] { resultSlot }, 0, 2);
        UpdateItems(new SlotContainer[] { resultSlot });
        resultTableID = 2;

        // �Ĺ��� ��ҷ� ���� ���� ��� ���ø� ����
        slotTemplate.container.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        slotTemplate.container.raycastTarget = slotTemplate.item.raycastTarget = slotTemplate.count.raycastTarget = false;
    }

    void InitializeSlotTable(RectTransform container, SC_SlotTemplate slotTemplateTmp, SlotContainer[] slots, int margin, int tableIDTmp)
    {
        int resetIndex = 0;
        int rowTmp = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = new SlotContainer();
            }
            GameObject newSlot = Instantiate(slotTemplateTmp.gameObject, container.transform);
            slots[i].slot = newSlot.GetComponent<SC_SlotTemplate>();
            slots[i].slot.gameObject.SetActive(true);
            slots[i].tableID = tableIDTmp;

            float xTmp = (int)((margin + slots[i].slot.container.rectTransform.sizeDelta.x) * (i - resetIndex));
            if (xTmp + slots[i].slot.container.rectTransform.sizeDelta.x + margin > container.rect.width)
            {
                resetIndex = i;
                rowTmp++;
                xTmp = 0;
            }
            slots[i].slot.container.rectTransform.anchoredPosition = new Vector2(margin + xTmp, -margin - ((margin + slots[i].slot.container.rectTransform.sizeDelta.y) * rowTmp));
        }
    }

    // ���̺� UI ������Ʈ
    void UpdateItems(SlotContainer[] slots)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            Item slotItem = FindItem(slots[i].itemSprite);
            if (slotItem != null)
            {
                if (!slotItem.stackable)
                {
                    slots[i].itemCount = 1;
                }
                // �� ������ �� ����
                if (slots[i].itemCount > 1)
                {
                    slots[i].slot.count.enabled = true;
                    slots[i].slot.count.text = slots[i].itemCount.ToString();
                }
                else
                {
                    slots[i].slot.count.enabled = false;
                }
                // ������ ������ ����
                slots[i].slot.item.enabled = true;
                slots[i].slot.item.sprite = slotItem.itemSprite;
            }
            else
            {
                slots[i].slot.count.enabled = false;
                slots[i].slot.item.enabled = false;
            }
        }
    }

    // ������ ��Ͽ��� ��������Ʈ�� �����Ͽ� ������ ã��
    Item FindItem(Sprite sprite)
    {
        if (!sprite)
            return null;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].itemSprite == sprite)
            {
                return items[i];
            }
        }

        return null;
    }

    // ������ ��Ͽ��� �����Ǹ� �����Ͽ� ������ ã��
    Item FindItem(string recipe)
    {
        if (recipe == "")
            return null;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].craftRecipe == recipe)
            {
                return items[i];
            }
        }

        return null;
    }

    public void ClickEventRecheck()
    {
        if (selectedItemSlot == null)
        {
            HandleNullSelectedItemSlot();
        }
        else
        {
            HandleSelectedItemSlot();
        }
    }

    private void HandleNullSelectedItemSlot()
    {
        selectedItemSlot = GetClickedSlot();
        if (selectedItemSlot != null && selectedItemSlot.itemSprite != null)
        {
            selectedItemSlot.slot.count.color = selectedItemSlot.slot.item.color = new Color(1, 1, 1, 0.5f);
        }
        else
        {
            selectedItemSlot = null;
        }
    }

    private void HandleSelectedItemSlot()
    {
        SlotContainer newClickedSlot = GetClickedSlot();
        if (newClickedSlot != null)
        {
            bool swapPositions = false;
            bool releaseClick = true;

            if (newClickedSlot != selectedItemSlot)
            {
                swapPositions = HandleDifferentSlots(newClickedSlot, swapPositions, ref releaseClick);
            }

            if (swapPositions)
            {
                SwapPositions(newClickedSlot);
            }

            if (releaseClick)
            {
                ReleaseClick();
            }

            UpdateItems(playerSlots);
            UpdateItems(craftSlots);
            UpdateItems(new SlotContainer[] { resultSlot });
        }
    }

    private bool HandleDifferentSlots(SlotContainer newClickedSlot, bool swapPositions, ref bool releaseClick)
    {
        // ���� ���̺��� �ٸ� ������ Ŭ���� ���
        if (newClickedSlot.tableID == selectedItemSlot.tableID)
        {
            swapPositions = HandleSameTableClicked(newClickedSlot, swapPositions);
        }
        else
        {
            // �ٸ� ���̺�� �̵��ϴ� ���
            swapPositions = HandleDifferentTableClicked(newClickedSlot, swapPositions, ref releaseClick);
        }
        return swapPositions;
    }
    private bool HandleSameTableClicked(SlotContainer newClickedSlot, bool swapPositions)
    {
        // ���� Ŭ���� �������� ���� ���, �ױ�; �ƴϸ� ��ȯ (���� ���̺��� �ƴ϶�� �ƹ��͵� ���� ����)
        if (newClickedSlot.itemSprite == selectedItemSlot.itemSprite)
        {
            Item slotItem = FindItem(selectedItemSlot.itemSprite);
            if (slotItem.stackable)
            {
                // �������� ���� ���� �� �ִ� ���, ���� ��ġ���� �����ϰ� �� ��ġ�� ���� �߰�
                selectedItemSlot.itemSprite = null;
                newClickedSlot.itemCount += selectedItemSlot.itemCount;
                selectedItemSlot.itemCount = 0;
            }
            else
            {
                swapPositions = true;
            }
        }
        else
        {
            swapPositions = true;
        }
        return swapPositions;
    }

    private bool HandleDifferentTableClicked(SlotContainer newClickedSlot, bool swapPositions, ref bool releaseClick)
    {
        // �ٸ� ���̺�� �̵��ϴ� ���
        if (resultTableID != newClickedSlot.tableID)
        {
            if (craftTableID != newClickedSlot.tableID)
            {
                if (newClickedSlot.itemSprite == selectedItemSlot.itemSprite)
                {
                    Item slotItem = FindItem(selectedItemSlot.itemSprite);
                    if (slotItem.stackable)
                    {
                        // �������� ���� ���� �� �ִ� ���, ���� ��ġ���� �����ϰ� �� ��ġ�� ���� �߰�
                        selectedItemSlot.itemSprite = null;
                        newClickedSlot.itemCount += selectedItemSlot.itemCount;
                        selectedItemSlot.itemCount = 0;
                    }
                    else
                    {
                        swapPositions = true;
                    }
                }
                else
                {
                    swapPositions = true;
                }
            }
            else
            {
                if (newClickedSlot.itemSprite == null || newClickedSlot.itemSprite == selectedItemSlot.itemSprite)
                {
                    // selectedItemSlot���� 1���� ������ �߰�
                    newClickedSlot.itemSprite = selectedItemSlot.itemSprite;
                    newClickedSlot.itemCount++;
                    selectedItemSlot.itemCount--;
                    if (selectedItemSlot.itemCount <= 0)
                    {
                        // ������ �������� ��ġ�� ���
                        selectedItemSlot.itemSprite = null;
                    }
                    else
                    {
                        releaseClick = false;
                    }
                }
                else
                {
                    swapPositions = true;
                }
            }
        }
        return swapPositions;
    }

    // ���� ���̺��� �ٸ� ������ Ŭ���� ���� �ٸ� ���̺�� �̵��ϴ� ��쿡 ���� �޼ҵ带 �����Ͻø� �ǰڽ��ϴ�.
    // ���� �ڵ带 �����Ͽ� �����Ͻø� �˴ϴ�.

    private void SwapPositions(SlotContainer newClickedSlot)
    {
        Sprite previousItemSprite = selectedItemSlot.itemSprite;
        int previousItemConunt = selectedItemSlot.itemCount;

        selectedItemSlot.itemSprite = newClickedSlot.itemSprite;
        selectedItemSlot.itemCount = newClickedSlot.itemCount;

        newClickedSlot.itemSprite = previousItemSprite;
        newClickedSlot.itemCount = previousItemConunt;
    }

    private void ReleaseClick()
    {
        selectedItemSlot.slot.count.color = selectedItemSlot.slot.item.color = Color.white;
        selectedItemSlot = null;
    }

    private SlotContainer GetClickedSlot()
    {
        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (playerSlots[i].slot.hasClicked)
            {
                playerSlots[i].slot.hasClicked = false;
                return playerSlots[i];
            }
        }

        for (int i = 0; i < craftSlots.Length; i++)
        {
            if (craftSlots[i].slot.hasClicked)
            {
                craftSlots[i].slot.hasClicked = false;
                return craftSlots[i];
            }
        }

        if (resultSlot.slot.hasClicked)
        {
            resultSlot.slot.hasClicked = false;
            return resultSlot;
        }

        return null;
    }

    private void PerformCrafting()
    {
        string[] combinedItemRecipe = new string[craftSlots.Length];

        craftButton.colors = defaultButtonColors;

        for (int i = 0; i < craftSlots.Length; i++)
        {
            Item slotItem = FindItem(craftSlots[i].itemSprite);
            if (slotItem != null)
            {
                combinedItemRecipe[i] = slotItem.itemSprite.name + (craftSlots[i].itemCount > 1 ? "(" + craftSlots[i].itemCount + ")" : "");
            }
            else
            {
                combinedItemRecipe[i] = "";
            }
        }

        string combinedRecipe = string.Join(",", combinedItemRecipe);
        print(combinedRecipe);

        // �����ǰ� ������ �����ǿ� ��ġ�ϴ��� Ȯ��
        Item craftedItem = FindItem(combinedRecipe);
        if (craftedItem != null)
        {
            // ���� ���� �ʱ�ȭ
            for (int i = 0; i < craftSlots.Length; i++)
            {
                craftSlots[i].itemSprite = null;
                craftSlots[i].itemCount = 0;
            }

            resultSlot.itemSprite = craftedItem.itemSprite;
            resultSlot.itemCount = 1;

            UpdateItems(craftSlots);
            UpdateItems(new SlotContainer[] { resultSlot });
        }
        else
        {
            ColorBlock colors = craftButton.colors;
            colors.selectedColor = colors.pressedColor = new Color(0.8f, 0.55f, 0.55f, 1);
            craftButton.colors = colors;
        }
    }

    // �� �����Ӹ��� ȣ��˴ϴ�
    void Update()
    {
        // ���� UI�� ���콺 ��ġ�� ����
        if (selectedItemSlot != null)
        {
            if (!slotTemplate.gameObject.activeSelf)
            {
                slotTemplate.gameObject.SetActive(true);
                slotTemplate.container.enabled = false;

                // ���õ� ������ ���� ���� ���ø��� ����
                slotTemplate.count.color = selectedItemSlot.slot.count.color;
                slotTemplate.item.sprite = selectedItemSlot.slot.item.sprite;
                slotTemplate.item.color = selectedItemSlot.slot.item.color;
            }

            // ���ø� ������ ���콺 ��ġ�� ���󰡰� ��
            slotTemplate.container.rectTransform.position = Input.mousePosition;
            // ������ ���� ������Ʈ
            slotTemplate.count.text = selectedItemSlot.slot.count.text;
            slotTemplate.count.enabled = selectedItemSlot.slot.count.enabled;
        }
        else
        {
            if (slotTemplate.gameObject.activeSelf)
            {
                slotTemplate.gameObject.SetActive(false);
            }
        }
    }
}
