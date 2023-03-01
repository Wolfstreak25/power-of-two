using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Block : MonoBehaviour
{
    public int Value;
    public Tile tile;
    public bool merging;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private TextMeshPro PointCount;
    public Vector2 location => transform.position;
    public Block MergingBlock;
    public void Initialise(BlockType type)
    {
        Value = type.Value;
        sprite.color = type.color;
        PointCount.text = type.Value+"";
    }
    public void SetBlock(Tile _tile)
    {
        if(tile != null ) tile.OccupiedBlock = null;
        tile =_tile;
        tile.OccupiedBlock = this;
    }
    public void MergeBlock(Block blockToMerge)
    {
        MergingBlock = blockToMerge;
        tile.OccupiedBlock = null;
        blockToMerge.merging = true;
    }
    public bool CanMerge(int value) => value == Value && !merging && MergingBlock == null; 
}
