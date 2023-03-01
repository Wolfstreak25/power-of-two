using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
public class GameManager : MonoBehaviour
{
    [SerializeField] private int gridHeight = 4;
    [SerializeField] private int gridWidth = 4;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private SpriteRenderer boardPrefab;
    [SerializeField] private List<BlockType> types;
    [SerializeField] private int travelTime;
    private List<Tile> tiles;
    private List<Block> blocks;
    private GameState state;
    private int round;
    private BlockType GetBlockType(int value) => types.First(t => t.Value == value);
    private void Start() {
        ChangeState(GameState.GenerateLevel);
    }
    private void Update() {
         if(state != GameState.WaitingInput) return;
         if(Input.GetKeyDown(KeyCode.A)) Shift(Vector2.left);
         if(Input.GetKeyDown(KeyCode.D)) Shift(Vector2.right);
         if(Input.GetKeyDown(KeyCode.W)) Shift(Vector2.up);
         if(Input.GetKeyDown(KeyCode.S)) Shift(Vector2.down);
    }
    private void ChangeState (GameState newState)
    {
        state = newState;
        switch (newState)
        {
            case GameState.GenerateLevel:
                GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                SpawnBlocks(round++ == 0 ? 2 : 1);
                break; 
            case GameState.WaitingInput:
                break;
            case GameState.Moving:
                break;
            case GameState.Win:
                break;
            case GameState.Lose:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState),newState,null);
        }
    }
    private void GenerateGrid()
    {
        round = 0;
        tiles = new List<Tile>();
        blocks = new List<Block>();
        for(int x = 0; x < gridWidth; x++)
            for(int y = 0; y < gridHeight; y++)
                {
                    var tile = GameObject.Instantiate<Tile>(tilePrefab,new Vector2(x,y), Quaternion.identity);
                    tiles.Add(tile);
                }
        var centre = new Vector2((float)gridWidth/2 - 0.5f,(float)gridHeight/2 -0.5f);
        var board = GameObject.Instantiate<SpriteRenderer>(boardPrefab, centre, Quaternion.identity);
        board.size = new Vector2(gridWidth,gridHeight);
        Camera.main.transform.position = new Vector3(centre.x, centre.y, -10);
        ChangeState(GameState.SpawningBlocks);
    }
    private void SpawnBlocks(int quantity)
    {
        var freeTiles = tiles.Where(n => n.OccupiedBlock == null).OrderBy(b => UnityEngine.Random.value).ToList();

        foreach (var tile in freeTiles.Take(quantity))
        {
            var block = Instantiate(blockPrefab,tile.location,Quaternion.identity);
            block.Initialise(GetBlockType(UnityEngine.Random.value >0.8f ? 4:2));
            block.SetBlock(tile);
            blocks.Add(block);
        }
        if(freeTiles.Count() == 1)
        {
            //lose condition
            return;
        }
        ChangeState(GameState.WaitingInput);
    }
    private void SpawnBlock(Tile tile, int value) 
    {
        var block = Instantiate(blockPrefab, tile.location, Quaternion.identity);
        block.Initialise(GetBlockType(value));
        block.SetBlock(tile);
        blocks.Add(block);
    }
    private void Shift (Vector2 dir)
    {
        ChangeState(GameState.Moving);
        var blockOrder = blocks.OrderBy(b=> b.location.x).ThenBy(b =>b.location.y).ToList();
        if(dir == Vector2.right || dir == Vector2.up) blockOrder.Reverse();

        Debug.Log(blockOrder.Count);

        foreach (var block in blockOrder)
        {
            var next = block.tile;
            do
            {
                block.SetBlock(next);
                var possibleTile = GetTileLocation(next.location + dir);
                if(possibleTile != null)
                {
                    // We know a node is present
                    // If it's possible to merge, set merge
                    if (possibleTile.OccupiedBlock != null && possibleTile.OccupiedBlock.CanMerge(block.Value))
                    {
                        block.MergeBlock(possibleTile.OccupiedBlock);
                    }
                    // Otherwise, can we move to this spot?
                    else if(possibleTile.OccupiedBlock == null) next = possibleTile;
                    // None hit? End do while loop
                }
            }while(next != block.tile);
            //block.transform.position = block.tile.location;
        }
        var sequence = DOTween.Sequence();

        foreach (var block in blockOrder) {
            var movePoint = block.MergingBlock != null ? block.MergingBlock.tile.location : block.tile.location;

            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime).SetEase(Ease.InQuad));
        }

        sequence.OnComplete(() => {
            var mergeBlocks = blockOrder.Where(b => b.MergingBlock != null).ToList();
            foreach (var block in mergeBlocks) {
                MergeBlocks(block.MergingBlock,block);
            }
            ChangeState(GameState.SpawningBlocks);
        });
    }
    private void MergeBlocks(Block baseBlock, Block mergingBlock) {
        var newValue = baseBlock.Value * 2;
        SpawnBlock(baseBlock.tile, newValue);
        RemoveBlock(baseBlock);
        RemoveBlock(mergingBlock);
    }

    private void RemoveBlock(Block block) {
        blocks.Remove(block);
        Destroy(block.gameObject);
    }
    private Tile GetTileLocation(Vector2 location)
    {
        return tiles.FirstOrDefault(n => n.location == location);
    }

}
[Serializable]
public struct BlockType
{
    public int Value;
    public Color color;
}
public enum GameState
{
    GenerateLevel,
    SpawningBlocks,
    WaitingInput,
    Moving,
    Win,
    Lose
}