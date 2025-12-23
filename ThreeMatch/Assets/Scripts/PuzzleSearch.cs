using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleSearch : Singleton<PuzzleSearch>
{
    [SerializeField] private Slot rootSlot;
    [SerializeField] private Transform destroyPoint;
    public Vector3 DestoryPoint { get => destroyPoint.position; }
    public Slot RootSlot { get => rootSlot; }


    public void CheckAllSlots(UnityAction<Slot> foreachAction)
    {
        CheckSlotRecursive(Slot.Direction.None, rootSlot, foreachAction);
    }

    public Slot SelectSomeSlot()
    {
        Slot tempSlot = null;
        CheckAllSlots((slot) =>
        {
            if (slot != null && !slot.IsReadyBreak() && slot.haveBlock != null) tempSlot = slot;
        });

        return tempSlot;
    }

    private void CheckSlotRecursive(Slot.Direction dir, Slot curSlot, UnityAction<Slot> foreachAction)
    {
        if (curSlot == null) return;

        if (dir == Slot.Direction.None || dir == Slot.Direction.Down_Left)
        {
            CheckSlotRecursive(Slot.Direction.Down_Left, curSlot.GetNearSlot(Slot.Direction.Down_Left), foreachAction);
        }
        if (dir == Slot.Direction.None || dir == Slot.Direction.Down_Right)
        {
            CheckSlotRecursive(Slot.Direction.Down_Right, curSlot.GetNearSlot(Slot.Direction.Down_Right), foreachAction);
        }

        CheckSlotRecursive(Slot.Direction.Down, curSlot.GetNearSlot(Slot.Direction.Down), foreachAction);

        foreachAction?.Invoke(curSlot);
    }
}
