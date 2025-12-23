using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class BlockMover : Singleton<BlockMover>
{
    [SerializeField] private bool isMoving;
    private Dictionary<Block, Slot> movingBlocks = new Dictionary<Block, Slot>();
    private MovePathList movePaths = new MovePathList();
    private int movingCount;
    public int MovingCount { get => movingCount; }

    public bool IsMoving { get => isMoving || movingCount > 0; }


    public void PlayExchangeBlock(Block fromBlock, Block toBlock, UnityAction resultAction = null)
    {
        StartCoroutine(BlockExchangingAnim(fromBlock, toBlock, resultAction));
    }

    IEnumerator BlockExchangingAnim(Block fromBlock, Block toBlock, UnityAction resultAction = null)
    {
        isMoving = true;
        Transform fromTransform = fromBlock.transform;
        Transform toTransform = toBlock.transform;

        while (true)
        {
            fromTransform.localPosition = Vector3.MoveTowards(fromTransform.localPosition, Vector3.zero, 10 * Time.deltaTime);
            toTransform.localPosition = Vector3.MoveTowards(toTransform.localPosition, Vector3.zero, 10 * Time.deltaTime);

            if (fromTransform.localPosition.Equals(Vector3.zero) && toTransform.localPosition.Equals(Vector3.zero)) break;

            yield return null;
        }

        resultAction?.Invoke();
        isMoving = false;
    }

    public void SendSignalToUpLine(Slot slot)
    {
        Slot upSlot = slot;

        while (upSlot != null)
        {
            if(upSlot.haveBlock != null)
            {
                if (upSlot.haveBlock.isMoving || upSlot.IsReadyBreak()) return;
                else AddMoveBlock(upSlot);
            }

            Slot prevSlot = upSlot;
            upSlot = upSlot.GetNearSlot(Slot.Direction.Up);
            upSlot = upSlot ?? prevSlot.GetNearSlot(Slot.Direction.Up_Left);
            upSlot = upSlot ?? prevSlot.GetNearSlot(Slot.Direction.Up_Right);
        }
    }

    public void AddMoveBlock(Block block)
    {
        if (block.isMoving) return;
        block.isMoving = true;
        block.moveDirection = Slot.Direction.Down;
        movingBlocks.Add(block, null);
    }

    public void AddMoveBlock(Slot fromSlot)
    {
        if (fromSlot.haveBlock == null) return;
        Block block = fromSlot.ReleaseBlock();
        if (block.isMoving) return;
        block.isMoving = true;
        float rootSlotPos_X = PuzzleSearch.Instance.RootSlot.transform.position.x;
        if (rootSlotPos_X == block.transform.position.x) block.moveDirection = Slot.Direction.Down;
        else if (rootSlotPos_X > block.transform.position.x) block.moveDirection = Slot.Direction.Down_Left;
        else  block.moveDirection = Slot.Direction.Down_Right;

        movingBlocks.Add(block, fromSlot);
    }

    public void StartMoveBlocks()
    {
        foreach (KeyValuePair<Block, Slot> pair in movingBlocks.OrderBy(pair => pair.Key.transform.position.y).ToDictionary(x => x.Key, x => x.Value))
        {
            int pathIndex = FindDestinationSlotQ(pair.Key, pair.Value);
            Slot destinationSlot = movePaths.GetLastSlotInPath(pathIndex);
            destinationSlot.reservedForBlock = pair.Key;
            
            StartCoroutine(BlockMoving(pair.Key, pathIndex));
        }

        movingBlocks.Clear();
    }

    IEnumerator BlockMoving(Block block, int pathIndex)
    {
        movingCount++;
        movePaths.Lock(pathIndex);

        Slot nextSlot = movePaths.GetNextPath(pathIndex);
        Transform blockTransform = block.transform;
        float speed = 7f;

        while (true)
        {
            blockTransform.position = Vector3.MoveTowards(blockTransform.position, nextSlot.transform.position, speed * Time.deltaTime);
            speed += 0.02f;
            if (speed > 9f) speed = 9f;

            if (blockTransform.position == nextSlot.transform.position)
            {
                if(movePaths.PathCount(pathIndex) == 0)
                {
                    nextSlot.SetBlock(block);
                    nextSlot.reservedForBlock = null;
                    break;
                }
                nextSlot = movePaths.GetNextPath(pathIndex);
            }

            yield return null;
        }

        movePaths.UnLock(pathIndex);
        movingCount--;
    }

    private int FindDestinationSlotQ(Block block, Slot startSlot = null)
    {
        int pathIndex = movePaths.GetIndex();
        if (startSlot == null) movePaths.AddPath(pathIndex, PuzzleSearch.Instance.RootSlot);
        Slot destinationSlot = startSlot ?? PuzzleSearch.Instance.RootSlot;
        while (true)
        {
            Slot tempSlot = GetNextDestination(block, destinationSlot);
            if (tempSlot == null) break;

            destinationSlot = tempSlot;
            movePaths.AddPath(pathIndex, destinationSlot);
        }

        return pathIndex;
    }

    private Slot GetNextDestination(Block block, Slot slot)
    {
        if (slot == null) return PuzzleSearch.Instance.RootSlot;

        Slot downSlot = slot.GetNearSlot(Slot.Direction.Down);
        if (PossibleDestination(block, downSlot)) return downSlot;

        Slot leftSlot = slot.GetNearSlot(Slot.Direction.Down_Left);
        Slot rightSlot = slot.GetNearSlot(Slot.Direction.Down_Right);

        if(block.moveDirection == Slot.Direction.Down_Left)
        {
            if (PossibleDestination(block, leftSlot)) return leftSlot;
            else return null;
        }
        else if (block.moveDirection == Slot.Direction.Down_Right)
        {
            if (PossibleDestination(block, rightSlot)) return rightSlot;
            else return null;
        }

        if (PossibleDestination(block, leftSlot) && PossibleDestination(block, rightSlot))
        {
            if (leftSlot.reservedForBlock != null) return leftSlot;
            else if (rightSlot.reservedForBlock != null) return rightSlot;
            else return (Random.Range(0, 2) == 0 ? leftSlot : rightSlot);
        }
        else if (PossibleDestination(block, leftSlot)) return leftSlot;
        else if (PossibleDestination(block, rightSlot)) return rightSlot;
        else return null;
    }

    private bool PossibleDestination(Block block, Slot slot)
    {
        return slot != null && slot.haveBlock == null && (slot.reservedForBlock == null || slot.reservedForBlock.Equals(block));
    }
}
