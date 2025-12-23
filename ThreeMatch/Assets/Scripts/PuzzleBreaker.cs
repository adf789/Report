using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System;

public class PuzzleBreaker : Singleton<PuzzleBreaker>
{
    private Queue<Slot> readyToBreakBlocks = new Queue<Slot>(); // �ı� ��� ���� ť
    private Queue<(Block, int)> scoreSpawns = new Queue<(Block, int)>();
    private Queue<Slot> bufferBreakBlocks = new Queue<Slot>();
    private Queue<Slot> waitingCheckSlots = new Queue<Slot>();
    private Dictionary<int, Slot> signalSlots = new Dictionary<int, Slot>();
    public int waitingSpecialBlockCount;
    private bool isBreaking;
    public bool IsBreaking { get => isBreaking; }

    public void AddBreakBlock(Slot slot)
    {
        if (slot == null) return;

        InsertExistBreakBlocks(slot);
    }

    public int StartBreakBlocks()
    {
        if (readyToBreakBlocks.Count == 0) return 0;

        Queue<Slot> obstacles = new Queue<Slot>();
        signalSlots.Clear();

        int breakCount = 0;
        // 파괴 가능한 블록이 없을때까지 체크
        while (readyToBreakBlocks.Count != 0)
        {
            Slot slot = readyToBreakBlocks.Dequeue();

            // 파괴 개수 카운팅
            breakCount++;

            // 슬롯 내 블록 파괴
            slot.BreakBlock();

            // 주위 슬롯 탐색
            slot.ForeachNearSlot((nearSlot, dir) =>
            {
                // 주위 블록이 장애물이라면
                if (!nearSlot.IsReadyBreak() && nearSlot.haveBlock is Obstacle)
                {
                    // 파괴 블록 대기에 추가
                    obstacles.Enqueue(nearSlot);
                    nearSlot.ReadyBreakBlock();
                }
            });

            // 파괴 후 수직 라인에 Notify
            AddSlotForSignalLine(slot);
        }

        // 추가 장애물 파괴 시작
        while (obstacles.Count != 0)
        {
            Slot slot = obstacles.Dequeue();
            slot.BreakBlock();
            if (slot.haveBlock == null)
            {
                AddSlotForSignalLine(slot);
                breakCount++;
            }
        }

        // 파괴된 블록이 있는 경우 이벤트 호출
        if (!isBreaking) StartCoroutine(AfterBreak(breakCount));

        // 이펙트 호출
        InstantiateScoreText();
        return breakCount;
    }

    public void StartBreakBlockWithOneBlock(Slot slot)
    {
        if (slot == null || slot.haveBlock == null) return;

        ScoreText scoreText = ExtraPooling.Instance.GetUnUseScoreText();
        scoreText.transform.position = slot.transform.position;
        scoreText.Show(30, slot.haveBlock.Block_Color);
        ScreenUI.Instance.AddScore(30);

        slot.BreakBlock();
        AddSlotForSignalLine(slot);

        if (!isBreaking) StartCoroutine(AfterBreak(1));
    }

    public void AddSlotForSignalLine(Slot slot)
    {
        if (slot == null) return;

        if (!signalSlots.ContainsKey(slot.lineIndex)) signalSlots.Add(slot.lineIndex, slot);
        else if (signalSlots[slot.lineIndex].transform.position.y > slot.transform.position.y) signalSlots[slot.lineIndex] = slot;
    }

    private void InsertExistBreakBlocks(Slot startSlot)
    {
        if (startSlot.haveBlock is Obstacle) return;

        ResetCheck();

        int clusterCount = FindCluster(startSlot);
        int addedLineCount = FindLine(startSlot);

        AddScoreInQueue(startSlot.haveBlock, clusterCount + addedLineCount);
    }

    private void AddScoreInQueue(Block block, int blockCount)
    {
        if (blockCount < 1) return;
        else if (block == null) return;

        int score = blockCount * 30;

        (Block, int) tuple = (block, score);
        scoreSpawns.Enqueue(tuple);
    }

    /// <summary>
    /// ���� �ı� ������ ������ ã�� ����Ʈ�� ����
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    private void InsertBreakableSlots_Cluster(Slot slot)
    {
        Slot.Direction dir = FindNotSameBlockDirection(slot);

        // ���� ��� ������ ���� ���� ���� ���
        if (dir == Slot.Direction.None)
        {
            slot.ForeachNearSlot((nearSlot, dir) =>
            {
                waitingCheckSlots.Enqueue(nearSlot);

                if (!nearSlot.IsReadyBreak())
                {
                    bufferBreakBlocks.Enqueue(nearSlot);
                    nearSlot.ReadyBreakBlock();
                }
            });

            if (!slot.IsReadyBreak())
            {
                bufferBreakBlocks.Enqueue(slot);
                slot.ReadyBreakBlock();
            }
        }
        else
        {
            FindClusterInOneSlot(slot, dir);
        }
    }

    /// <summary>
    /// ���� �ı� ������ ������ ã�� ����Ʈ�� ����
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    private void InsertBreakableSlots_Line(Slot slot)
    {
        slot.ForeachNearSlot(Slot.Direction.Up, Slot.Direction.Down_Right, (nearSlot, dir) =>
        {
            TryGetSlotLine(slot, dir);
        });
    }

    private Slot.Direction FindNotSameBlockDirection(Slot slot)
    {
        Slot.Direction endDir = Slot.Direction.None;
        Slot.Direction startDir = Slot.Direction.Up;

        // Cluster üũ�ϱ����� ������ġ Ž��
        do
        {
            if (!slot.IsSameBlockWithNearSlot(startDir))
            {
                endDir = startDir;
                break;
            }
            startDir = Slot.RotateCounterClockWise(startDir, 1);
        } while (startDir != Slot.Direction.Up);

        return endDir;
    }

    /// <summary>
    /// ���� ������ ������ �̷���� Ž��
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="startDir"></param>
    /// <returns></returns>
    private void FindClusterInOneSlot(Slot slot, Slot.Direction startDir)
    {
        Queue<Slot> clusterSlots = new Queue<Slot>();

        Slot.Direction endDir = Slot.RotateCounterClockWise(startDir, 1);
        slot.ForeachNearSlot(startDir, endDir, (nearSlot, dir) =>
        {
            if (nearSlot != null && nearSlot.IsReadyBreak()) return;

            if (slot.IsSameBlock(nearSlot))
            {
                clusterSlots.Enqueue(nearSlot);
                return;
            }

            if (clusterSlots.Count > 2 && !slot.IsReadyBreak())
            {
                bufferBreakBlocks.Enqueue(slot);
                slot.ReadyBreakBlock();
            }
            AddClusterSlots(clusterSlots);

            clusterSlots.Clear();
        });

        if (clusterSlots.Count > 2 && !slot.IsReadyBreak())
        {
            bufferBreakBlocks.Enqueue(slot);
            slot.ReadyBreakBlock();
        }
        AddClusterSlots(clusterSlots);
    }

    private void TryGetSlotLine(Slot slot, Slot.Direction dir)
    {
        if (slot == null) return;

        int lineCount = 1;
        Slot.Direction crossDir = Slot.RotateClockWise(dir, 3);
        Slot nearSlot_Dir = slot.GetNearSlot(dir);
        Slot nearSlot_CrossDir = slot.GetNearSlot(crossDir);
        Slot calculateSlot = slot;

        bool possibleNearSlot_Dir = false;
        bool possibleNearSlot_CrossDir = false;

        do
        {
            possibleNearSlot_Dir = slot.IsSameBlock(nearSlot_Dir) && !nearSlot_Dir.IsReadyBreak();
            possibleNearSlot_CrossDir = slot.IsSameBlock(nearSlot_CrossDir) && !nearSlot_CrossDir.IsReadyBreak();

            if (possibleNearSlot_Dir)
            {
                lineCount++;
                calculateSlot = nearSlot_Dir;
                nearSlot_Dir = nearSlot_Dir.GetNearSlot(dir);
            }

            if (possibleNearSlot_CrossDir)
            {
                lineCount++;
                nearSlot_CrossDir = nearSlot_CrossDir.GetNearSlot(crossDir);
            }
        } while (possibleNearSlot_Dir || possibleNearSlot_CrossDir);

        if (lineCount >= 3)
        {
            for (int i = 0; i < lineCount; i++)
            {
                if (!calculateSlot.Equals(slot))
                {
                    waitingCheckSlots.Enqueue(calculateSlot);
                }

                if (!calculateSlot.IsReadyBreak())
                {
                    bufferBreakBlocks.Enqueue(calculateSlot);
                    calculateSlot.ReadyBreakBlock();
                }
                calculateSlot = calculateSlot.GetNearSlot(crossDir);
            }
        }
    }

    private void AddClusterSlots(Queue<Slot> addableSlots)
    {
        if (addableSlots.Count > 1)
        {
            int clusterSize = addableSlots.Count;
            foreach (Slot clusterSlot in addableSlots)
            {
                waitingCheckSlots.Enqueue(clusterSlot);

                if (clusterSize > 2 && !clusterSlot.IsReadyBreak())
                {
                    bufferBreakBlocks.Enqueue(clusterSlot);
                    clusterSlot.ReadyBreakBlock();
                }
            }
        }
    }

    private void InstantiateScoreText()
    {
        while (scoreSpawns.Count != 0)
        {
            (Block, int) scoreTuple = scoreSpawns.Dequeue();
            ScoreText scoreText = ExtraPooling.Instance.GetUnUseScoreText();
            scoreText.transform.position = scoreTuple.Item1.transform.position;
            scoreText.Show(scoreTuple.Item2, scoreTuple.Item1.Block_Color);
            ScreenUI.Instance.AddScore(scoreTuple.Item2);
        }
    }

    private void InsertToReadyListFromBuffer()
    {
        while (bufferBreakBlocks.Count != 0)
        {
            readyToBreakBlocks.Enqueue(bufferBreakBlocks.Dequeue());
        }
    }

    private void ResetCheck()
    {
        PuzzleSearch.Instance.CheckAllSlots((slot) =>
        {
            slot.clusterCheck = false;
            slot.lineCheck = false;
        });
    }

    private int FindCluster(Slot startSlot)
    {
        if (startSlot == null) return 0;

        waitingCheckSlots.Clear();
        bufferBreakBlocks.Clear();
        waitingCheckSlots.Enqueue(startSlot);
        while (waitingCheckSlots.Count != 0)
        {
            Slot baseSlot = waitingCheckSlots.Dequeue();

            if (baseSlot.clusterCheck) continue;
            baseSlot.clusterCheck = true;

            InsertBreakableSlots_Cluster(baseSlot);
        }
        int clusterCount = bufferBreakBlocks.Count;
        bool isSpecialBlock = startSlot.haveBlock is SpecialBlock;
        if (!isSpecialBlock && PuzzleCreator.Instance.CreateSpecialBlockForCluster(startSlot, ref bufferBreakBlocks)) clusterCount = 0;
        InsertToReadyListFromBuffer();

        return clusterCount;
    }

    private int FindLine(Slot startSlot)
    {
        if (startSlot == null) return 0;

        waitingCheckSlots.Clear();
        bufferBreakBlocks.Clear();
        waitingCheckSlots.Enqueue(startSlot);
        while (waitingCheckSlots.Count != 0)
        {
            Slot baseSlot = waitingCheckSlots.Dequeue();

            if (baseSlot.lineCheck) continue;
            baseSlot.lineCheck = true;

            InsertBreakableSlots_Line(baseSlot);
        }
        int lineCount = bufferBreakBlocks.Count;
        InsertToReadyListFromBuffer();

        return lineCount;
    }


    /// <summary>
    /// ������ �μ� �� ȣ��
    /// </summary>
    /// <param name="breakCount"></param>
    /// <returns></returns>
    IEnumerator AfterBreak(int breakCount)
    {
        if (isBreaking) yield break;
        isBreaking = true;

        while (breakCount > 0)
        {
            yield return new WaitUntil(() => waitingSpecialBlockCount == 0);

            foreach (Slot slot in signalSlots.Values)
            {
                BlockMover.Instance.SendSignalToUpLine(slot);
            }
            BlockMover.Instance.StartMoveBlocks();
            PuzzleCreator.Instance.CreateBlocks(breakCount);

            yield return new WaitWhile(() => PuzzleCreator.Instance.IsCreating);
            yield return new WaitWhile(() => BlockMover.Instance.IsMoving);

            PuzzleSearch.Instance.CheckAllSlots((slot) =>
            {
                AddBreakBlock(slot);
            });
            breakCount = StartBreakBlocks();

            yield return null;
        }

        ScreenUI.Instance.CheckEndForStage();
        isBreaking = false;
    }
}
