using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MovePathList
{
    private List<Queue<Slot>> movePaths;
    private List<bool> pathLocks;

    public int GetIndex()
    {
        if(movePaths == null)
        {
            movePaths = new List<Queue<Slot>>();
            pathLocks = new List<bool>();
            movePaths.Add(new Queue<Slot>());
            pathLocks.Add(false);
            return 0;
        }

        for(int i = 0; i < movePaths.Count; i++)
        {
            if (movePaths[i].Count == 0 && !pathLocks[i]) return i;
        }

        movePaths.Add(new Queue<Slot>());
        pathLocks.Add(false);
        return movePaths.Count - 1;
    }

    public Slot GetNextPath(int index)
    {
        if (movePaths.Count <= index) throw new Exception("경로 인덱스 에러");
        else if (movePaths[index].Count == 0) return null;

        return movePaths[index].Dequeue();
    }

    public Slot GetLastSlotInPath(int index)
    {
        if (movePaths.Count <= index) throw new Exception("경로 인덱스 에러");
        else if (movePaths[index].Count == 0) return null;

        return movePaths[index].Last();
    }

    public int PathCount(int index)
    {
        if (movePaths.Count <= index) throw new Exception("경로 인덱스 에러");

        return movePaths[index].Count;
    }

    /// <summary>
    /// 먼저, 인덱스 할당부터 받고 호출할 것
    /// </summary>
    /// <param name="index"></param>
    /// <param name="slot"></param>
    public void AddPath(int index, Slot slot)
    {
        if (movePaths.Count < index) throw new Exception("경로 인덱스 에러");
        else if (movePaths.Count == index)
        {
            movePaths.Add(new Queue<Slot>());
            movePaths[index].Enqueue(slot);
            pathLocks.Add(false);
        }
        else if (pathLocks[index]) return;

        movePaths[index].Enqueue(slot);
    }

    public void Lock(int index)
    {
        if (pathLocks.Count <= index) return;
        pathLocks[index] = true;
    }

    public void UnLock(int index)
    {
        if (pathLocks.Count <= index) return;
        pathLocks[index] = false;
    }
}
