using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotGenerator : MonoBehaviour
{
    [SerializeField] private Slot slot;

    public void Generate(Slot.Direction direction)
    {
        if(slot.GetNearSlot(direction) != null)
        {
            Debug.LogError("이미 존재함");
            return;
        }

        GameObject obj = Instantiate<GameObject>(Resources.Load<GameObject>("Slot"));
        Vector3 originPos = slot.transform.position;

        switch (direction)
        {
            case Slot.Direction.Up:
                originPos.y += 2.15f;
                break;
            case Slot.Direction.Up_Right:
                originPos.x += 1.87f;
                originPos.y += 1.07f;
                break;
            case Slot.Direction.Down_Right:
                originPos.x += 1.87f;
                originPos.y -= 1.07f;
                break;
            case Slot.Direction.Down:
                originPos.y -= 2.15f;
                break;
            case Slot.Direction.Down_Left:
                originPos.x -= 1.87f;
                originPos.y -= 1.07f;
                break;
            case Slot.Direction.Up_Left:
                originPos.x -= 1.87f;
                originPos.y += 1.07f;
                break;
        }

        obj.name = "Slot";
        obj.transform.parent = slot.transform.parent;
        obj.transform.localPosition = originPos;
        Slot createdSlot = obj.GetComponent<Slot>();

        createdSlot.ClearNearSlot();
        slot.SetNearSlot(direction, createdSlot);

        // 생성된 슬롯 연결
        createdSlot.SetNearSlot(Slot.RotateClockWise(direction, 3), slot);
        createdSlot.SetNearSlot(Slot.RotateClockWise(direction, 2), slot.GetNearSlot(Slot.RotateClockWise(direction, 1)));
        createdSlot.SetNearSlot(Slot.RotateCounterClockWise(direction, 2), slot.GetNearSlot(Slot.RotateCounterClockWise(direction, 1)));

        // 주변 슬롯 연결
        Slot nearCreatedSlot = createdSlot.GetNearSlot(Slot.RotateClockWise(direction, 2));
        if(nearCreatedSlot != null) nearCreatedSlot.SetNearSlot(Slot.RotateClockWise(direction, 5), createdSlot);
        nearCreatedSlot = createdSlot.GetNearSlot(Slot.RotateCounterClockWise(direction, 2));
        if(nearCreatedSlot != null) nearCreatedSlot.SetNearSlot(Slot.RotateCounterClockWise(direction, 5), createdSlot);
    }
}
