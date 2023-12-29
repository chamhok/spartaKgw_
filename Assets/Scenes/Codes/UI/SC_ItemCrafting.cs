using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SlotContainer
{
    public Sprite itemSprite; // 아이템의 스프라이트 (아이템 배열에 있는 것과 동일해야 함), 또는 null로 두면 아이템 없음
    public int itemCount; // 이 슬롯에 있는 아이템 수, 1 이하면 1개의 아이템으로 해석됨
    [HideInInspector]
    public int tableID;
    [HideInInspector]
    public SC_SlotTemplate slot;
}

[System.Serializable]
public class Item
{
    public Sprite itemSprite;
    public bool stackable = false; // 이 아이템을 결합(쌓을 수) 있나요?
    public string craftRecipe; // 이 아이템을 제작하기 위해 필요한 아이템 키, 쉼표로 구분 (팁: 플레이 모드에서 제작 버튼을 사용하고 콘솔에서 인쇄된 레시피를 확인하세요)
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
    // 사용 가능한 모든 아이템 목록
    public Item[] items;

    SlotContainer selectedItemSlot = null;

    int craftTableID = -1; // 아이템이 하나씩 배치될 테이블의 ID (예: 제작 테이블)
    int resultTableID = -1; // 아이템을 가져갈 수 있지만 놓을 수 없는 테이블의 ID

    ColorBlock defaultButtonColors;

    // 첫 번째 프레임 업데이트 전에 호출됩니다
    void Start()
    {
        // 슬롯 요소 템플릿 설정
        slotTemplate.container.rectTransform.pivot = new Vector2(0, 1);
        slotTemplate.container.rectTransform.anchorMax = slotTemplate.container.rectTransform.anchorMin = new Vector2(0, 1);
        slotTemplate.craftingController = this;
        slotTemplate.gameObject.SetActive(false);
        // 결과 슬롯 요소 템플릿 설정
        resultSlotTemplate.container.rectTransform.pivot = new Vector2(0, 1);
        resultSlotTemplate.container.rectTransform.anchorMax = resultSlotTemplate.container.rectTransform.anchorMin = new Vector2(0, 1);
        resultSlotTemplate.craftingController = this;
        resultSlotTemplate.gameObject.SetActive(false);

        // 제작 버튼에 클릭 이벤트 연결
        craftButton.onClick.AddListener(PerformCrafting);
        // 제작 버튼 기본 색상 저장
        defaultButtonColors = craftButton.colors;

        // 제작 슬롯 초기화
        InitializeSlotTable(craftingSlotsContainer, slotTemplate, craftSlots, 5, 0);
        UpdateItems(craftSlots);
        craftTableID = 0;

        // 플레이어 슬롯 초기화
        InitializeSlotTable(playerSlotsContainer, slotTemplate, playerSlots, 5, 1);
        UpdateItems(playerSlots);

        // 결과 슬롯 초기화
        InitializeSlotTable(resultSlotContainer, resultSlotTemplate, new SlotContainer[] { resultSlot }, 0, 2);
        UpdateItems(new SlotContainer[] { resultSlot });
        resultTableID = 2;

        // 후버링 요소로 사용될 슬롯 요소 템플릿 리셋
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

    // 테이블 UI 업데이트
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
                // 총 아이템 수 적용
                if (slots[i].itemCount > 1)
                {
                    slots[i].slot.count.enabled = true;
                    slots[i].slot.count.text = slots[i].itemCount.ToString();
                }
                else
                {
                    slots[i].slot.count.enabled = false;
                }
                // 아이템 아이콘 적용
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

    // 아이템 목록에서 스프라이트를 참조하여 아이템 찾기
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

    // 아이템 목록에서 레시피를 참조하여 아이템 찾기
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
        // 같은 테이블의 다른 슬롯을 클릭한 경우
        if (newClickedSlot.tableID == selectedItemSlot.tableID)
        {
            swapPositions = HandleSameTableClicked(newClickedSlot, swapPositions);
        }
        else
        {
            // 다른 테이블로 이동하는 경우
            swapPositions = HandleDifferentTableClicked(newClickedSlot, swapPositions, ref releaseClick);
        }
        return swapPositions;
    }
    private bool HandleSameTableClicked(SlotContainer newClickedSlot, bool swapPositions)
    {
        // 새로 클릭한 아이템이 같은 경우, 쌓기; 아니면 교환 (제작 테이블이 아니라면 아무것도 하지 않음)
        if (newClickedSlot.itemSprite == selectedItemSlot.itemSprite)
        {
            Item slotItem = FindItem(selectedItemSlot.itemSprite);
            if (slotItem.stackable)
            {
                // 아이템이 같고 쌓을 수 있는 경우, 이전 위치에서 제거하고 새 위치에 수량 추가
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
        // 다른 테이블로 이동하는 경우
        if (resultTableID != newClickedSlot.tableID)
        {
            if (craftTableID != newClickedSlot.tableID)
            {
                if (newClickedSlot.itemSprite == selectedItemSlot.itemSprite)
                {
                    Item slotItem = FindItem(selectedItemSlot.itemSprite);
                    if (slotItem.stackable)
                    {
                        // 아이템이 같고 쌓을 수 있는 경우, 이전 위치에서 제거하고 새 위치에 수량 추가
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
                    // selectedItemSlot에서 1개의 아이템 추가
                    newClickedSlot.itemSprite = selectedItemSlot.itemSprite;
                    newClickedSlot.itemCount++;
                    selectedItemSlot.itemCount--;
                    if (selectedItemSlot.itemCount <= 0)
                    {
                        // 마지막 아이템을 배치한 경우
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

    // 같은 테이블의 다른 슬롯을 클릭한 경우와 다른 테이블로 이동하는 경우에 대한 메소드를 구현하시면 되겠습니다.
    // 위의 코드를 참조하여 구현하시면 됩니다.

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

        // 레시피가 아이템 레시피와 일치하는지 확인
        Item craftedItem = FindItem(combinedRecipe);
        if (craftedItem != null)
        {
            // 제작 슬롯 초기화
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

    // 매 프레임마다 호출됩니다
    void Update()
    {
        // 슬롯 UI가 마우스 위치를 따라감
        if (selectedItemSlot != null)
        {
            if (!slotTemplate.gameObject.activeSelf)
            {
                slotTemplate.gameObject.SetActive(true);
                slotTemplate.container.enabled = false;

                // 선택된 아이템 값을 슬롯 템플릿에 복사
                slotTemplate.count.color = selectedItemSlot.slot.count.color;
                slotTemplate.item.sprite = selectedItemSlot.slot.item.sprite;
                slotTemplate.item.color = selectedItemSlot.slot.item.color;
            }

            // 템플릿 슬롯이 마우스 위치를 따라가게 함
            slotTemplate.container.rectTransform.position = Input.mousePosition;
            // 아이템 수량 업데이트
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
