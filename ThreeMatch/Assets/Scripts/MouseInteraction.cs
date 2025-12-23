using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInteraction : MonoBehaviour
{
    private Vector2 mouseStartPos;
    [SerializeField] private Slot slot;

    private void OnMouseDrag()
    {
        if (!CheckEnableTouch()) return;

        Slot.pickedSlot = slot;
    }

    private void OnMouseUp()
    {
        Slot.pickedSlot = null;
    }

    private void OnMouseEnter()
    {
        if (!CheckEnableTouch()) return;
        else if (Slot.pickedSlot == null) return;
        
        slot.ExchangeBlock(Slot.pickedSlot);
        Slot.pickedSlot = null;
    }

    private bool CheckEnableTouch()
    {
        return Time.timeScale != 0f && !BlockMover.Instance.IsMoving && !PuzzleCreator.Instance.IsCreating && !PuzzleBreaker.Instance.IsBreaking;
    }
}
