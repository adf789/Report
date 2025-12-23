using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPooling : Singleton<BlockPooling>
{
    [SerializeField] private List<Gem> gems = new List<Gem>();
    [SerializeField] private List<Obstacle> obstacles = new List<Obstacle>();
    [SerializeField] private List<Block_Boomerang> boomerangs = new List<Block_Boomerang>();
    [SerializeField] private List<Block_Rocket> rockets = new List<Block_Rocket>();

    public void Init()
    {
        PuzzleSearch.Instance.CheckAllSlots((slot) =>
        {
            if (slot.haveBlock == null) return;
            else if (slot.haveBlock is Gem) gems.Add((Gem)slot.haveBlock);
            else if (slot.haveBlock is Obstacle) obstacles.Add((Obstacle)slot.haveBlock);
        });
    }

    public Block GetUnuseGem()
    {
        Gem gem = gems.Find((x) => !x.gameObject.activeSelf);
        if(gem == null)
        {
            GameObject obj = Instantiate(PuzzleCreator.Instance.GemPrefab, transform);
            obj.name = "Gem";
            gem = obj.GetComponent<Gem>();
            gems.Add(gem);
        }

        gem.gameObject.SetActive(true);
        gem.Reset();
        return gem;
    }

    public Block GetUnuseObstacle()
    {
        Obstacle obstacle = obstacles.Find((x) => !x.gameObject.activeSelf);
        if (obstacle == null)
        {
            GameObject obj = Instantiate(PuzzleCreator.Instance.ObstaclePrefab, transform);
            obj.name = "Obstacle";
            obstacle = obj.GetComponent<Obstacle>();
            obstacles.Add(obstacle);
        }

        obstacle.gameObject.SetActive(true);
        obstacle.Reset();
        return obstacle;
    }

    public Block GetUnuseBoomerang()
    {
        Block_Boomerang boomerang = boomerangs.Find((x) => !x.gameObject.activeSelf);
        if (boomerang == null)
        {
            GameObject obj = Instantiate(PuzzleCreator.Instance.BoomerangPrefab, transform);
            obj.name = "Boomerang";
            boomerang = obj.GetComponent<Block_Boomerang>();
            boomerangs.Add(boomerang);
        }

        boomerang.gameObject.SetActive(true);
        boomerang.Reset();
        return boomerang;
    }

    public Block GetUnuseRocket()
    {
        Block_Rocket rocket = rockets.Find((x) => !x.gameObject.activeSelf);
        if (rocket == null)
        {
            GameObject obj = Instantiate(PuzzleCreator.Instance.RocketPrefab, transform);
            obj.name = "Rocket";
            rocket = obj.GetComponent<Block_Rocket>();
            rockets.Add(rocket);
        }

        rocket.gameObject.SetActive(true);
        rocket.Reset();
        return rocket;
    }

    public void ReturnBlock(Block block)
    {
        block.gameObject.SetActive(false);
        block.transform.SetParent(transform);
    }
}
