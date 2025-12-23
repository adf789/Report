using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Slot : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Up_Right,
        Down_Right,
        Down,
        Down_Left,
        Up_Left,
        None
    }
    public static Slot pickedSlot;
    public Block haveBlock;
    public Block reservedForBlock;
    public int index;
    public int lineIndex;
    public bool lineCheck;
    public bool clusterCheck;

    [SerializeField] private Slot[] nearSlots;
    private bool readyBreak;

    private void Start()
    {
        haveBlock = GetComponentInChildren<Block>();
    }

    public void SetBlock(Block block)
    {
        if (haveBlock != null) return;

        haveBlock = block;
        haveBlock.transform.SetParent(transform);
        haveBlock.transform.localPosition = Vector3.zero;
        haveBlock.isMoving = false;
        readyBreak = false;
    }

    public Block ReleaseBlock()
    {
        if (haveBlock == null) return null;

        Block block = haveBlock;
        block.transform.SetParent(null);
        haveBlock = null;

        return block;
    }

    public void ClearNearSlot()
    {
        for(int dir = 0; dir < 6; dir++)
        {
            SetNearSlot((Direction)dir, null);
        }
    }

    public Slot GetNearSlot(Direction direction)
    {
        return nearSlots[(int)direction];
    }

    public void SetNearSlot(Direction direction, Slot slot)
    {
        nearSlots[(int)direction] = slot;
    }

    public void ForeachNearSlot(UnityAction<Slot, Direction> action)
    {
        ForeachNearSlot(Direction.Up, Direction.Up_Left, action);
    }

    public void ForeachNearSlot(Direction startDir, Direction endDir, UnityAction<Slot, Direction> action)
    {
        bool endReady = false;
        while (true)
        {
            if (startDir == endDir) endReady = true;

            action(GetNearSlot(startDir), startDir);

            if (endReady) return;

            startDir = RotateClockWise(startDir, 1);
        }
    }

    public void ForeachCheckCluster(UnityAction<Slot, int> action)
    {
        Direction endDir = Direction.None;
        Direction startDir = Direction.Up;

        // Cluster 체크하기위한 시작위치 탐색
        do
        {
            if (!IsSameBlockWithNearSlot(startDir))
            {
                endDir = startDir;
                break;
            }
            startDir = RotateCounterClockWise(startDir, 1);
        } while (startDir != Direction.Up);

        // 군집된 개수만큼 
        if(endDir == Direction.None)
        {
            GatherCluster(Direction.Up, 6, action);
            return;
        }

        int clusterSize = 0;
        startDir = RotateClockWise(endDir, 1);
        ForeachNearSlot(startDir, endDir, (slot, dir) =>
        {
            if (this.IsSameBlock(slot))
            {
                clusterSize++;
                return;
            }

            if (clusterSize > 1)
            {
                GatherCluster(RotateCounterClockWise(dir, clusterSize), clusterSize, action);
            }

            if(clusterSize > 0) clusterSize = 0;
        });

        if(clusterSize > 1) GatherCluster(RotateCounterClockWise(startDir, clusterSize), clusterSize, action);
    }

    public bool IsSameBlockWithNearSlot(Direction direction)
    {
        return IsSameBlock(nearSlots[(int)direction]);
    }

    public bool IsSameBlock(Slot slot)
    {
        if (slot == null) return false;
        else if (slot.haveBlock == null || haveBlock == null) return false;

        return haveBlock.IsSame(slot.haveBlock);
    }

    public void ExchangeBlock(Slot slot, bool doingBreak = true)
    {
        if (IsSameBlock(slot)) return;
        else if (haveBlock == null || slot.haveBlock == null) return;

        bool foundNearSlot = false;
        ForeachNearSlot((nearSlot, direction) =>
        {
            if (nearSlot == null) return;

            if (nearSlot.Equals(slot))
            {
                foundNearSlot = true;
            }
        });

        if (!foundNearSlot) return;

        ExchangeBlockWithSlot(slot);

        BlockMover.Instance.PlayExchangeBlock(haveBlock, slot.haveBlock, (doingBreak ? Break : null));

        void Break()
        {
            PuzzleBreaker.Instance.AddBreakBlock(slot);
            PuzzleBreaker.Instance.AddBreakBlock(this);
            int breakCount = PuzzleBreaker.Instance.StartBreakBlocks();
            if (breakCount == 0) ExchangeBlock(slot, false);
            else ScreenUI.Instance.MinusMoveCount();
        }
    }

    public void ReadyBreakBlock()
    {
        readyBreak = true;
    }

    public bool IsReadyBreak()
    {
        return readyBreak;
    }

    public void BreakBlock()
    {
        if (haveBlock == null) return;
        readyBreak = false;
        if (haveBlock.Break()) ReleaseBlock();
    }

    public void TryGetSlotLine(Direction dir, ref List<Slot> slotsInLine)
    {
        int lineCount = 1;
        Direction crossDir = RotateClockWise(dir, 3);
        Slot nearSlot_Dir = GetNearSlot(dir);
        Slot nearSlot_CrossDir = GetNearSlot(crossDir);
        Slot calculateSlot = this;

        while (IsSameBlock(nearSlot_Dir) || IsSameBlock(nearSlot_CrossDir))
        {
            if (IsSameBlock(nearSlot_Dir))
            {
                lineCount++;
                calculateSlot = nearSlot_Dir;
                nearSlot_Dir = nearSlot_Dir.GetNearSlot(dir);
            }

            if (IsSameBlock(nearSlot_CrossDir))
            {
                lineCount++;
                nearSlot_CrossDir = nearSlot_CrossDir.GetNearSlot(crossDir);
            }
        }

        if(lineCount >= 3)
        {
            if (slotsInLine == null) slotsInLine = new List<Slot>();

            for(int i = 0; i < lineCount; i++)
            {
                slotsInLine.Add(calculateSlot);
                calculateSlot = calculateSlot.GetNearSlot(crossDir);
            }
        }
    }

    private void GatherCluster(Direction startDir, int size, UnityAction<Slot, int> action)
    {
        if (size < 2) return;

        Direction endDir = RotateClockWise(startDir, size - 1);
        ForeachNearSlot(startDir, endDir, (slot, dir) =>
        {
            action?.Invoke(GetNearSlot(dir), size);
        });
    }

    private void ExchangeBlockWithSlot(Slot targetSlot)
    {
        Block tempBlock = haveBlock;
        haveBlock = targetSlot.haveBlock;
        targetSlot.haveBlock = tempBlock;

        haveBlock.transform.SetParent(transform);
        targetSlot.haveBlock.transform.SetParent(targetSlot.transform);
    }

    /// <summary>
    /// 시계 방향으로 칸 수 만큼 회전
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="num">칸 수</param>
    /// <returns></returns>
    public static Direction RotateClockWise(Direction baseDirection, int num)
    {
        int tempNum = (int)baseDirection + num % 6;
        if (tempNum > (int)Direction.Up_Left) return tempNum - (Direction.Up_Left + 1);

        return (Direction)tempNum;
    }

    /// <summary>
    /// 반시계 방향으로 칸 수 만큼 회전
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="num">칸 수</param>
    /// <returns></returns>
    public static Direction RotateCounterClockWise(Direction baseDirection, int num)
    {
        int tempNum = (int)baseDirection - num % 6;
        if (tempNum < 0) return Direction.Up_Left + (tempNum + 1);

        return (Direction)tempNum;
    }
}
