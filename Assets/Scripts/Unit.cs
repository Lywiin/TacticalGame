using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {

    //PUBLIC
    public float moveSpeed = 1f;
    public float health = 100f;
    public float damage = 100f;
    public int movePoints = 2;
    public int movePointsLeft = 2;
    public int range = 5;
    public bool needLineOfSight;

    public enum UnitTypeEnum { RANGE, MELEE, CANON };
    public UnitTypeEnum unitType;


    //PRIVATE
    private GameController gameController;

    private GameObject blockUnderUnit;

    void Start()
    {
        gameController = Camera.main.GetComponent<GameController>();
    }

    public void ResetUnitTurn()
    {
        movePointsLeft = movePoints;
    }

    public void SetBlockUnderUnit(GameObject b)
    {
        blockUnderUnit = b;
    }

    public GameObject GetBlockUnderUnit()
    {
        return blockUnderUnit;
    }

    public int GetMovePoints()
    {
        return movePoints;
    }

    public void SelectAttackBlocks(bool select)
    {
        switch(unitType)
        {
            case UnitTypeEnum.RANGE:
                GameObject selectedBlock = gameController.GetSelectedBlock();
                SelectLineAttackBlock(selectedBlock, (int)selectedBlock.transform.position.x, (int)selectedBlock.transform.position.z, range, select);
                break;

            case UnitTypeEnum.MELEE:

                break;

            case UnitTypeEnum.CANON:

                break;
        }
    }

    private void SelectLineAttackBlock(GameObject block, int xOrigin, int zOrigin, int rangeLeft, bool select)
    {
        BlockBehavior bb = block.GetComponent<BlockBehavior>();

        if (rangeLeft > 0)
        {
            List<GameObject> neighbors = bb.GetNeighbors();
            for (int i = 0; i < neighbors.Count; i++)
            {
                if (neighbors[i].transform.position.x == xOrigin || neighbors[i].transform.position.z == zOrigin)
                {
                    BlockBehavior bbN = neighbors[i].GetComponent<BlockBehavior>();
                    bbN.ToggleActive(select);
                    if (bbN.canMoveOn || !needLineOfSight)
                    {
                        SelectLineAttackBlock(neighbors[i], xOrigin, zOrigin, rangeLeft - 1, select);
                    }
                }
            }
        }
    }

    public bool DealDamage(float dmg)
    {
        health -= dmg;  //Debug.Log(health);
        if (health <= 0)
        {
            Destroy(transform.gameObject);
            return true;    //if target dead
        }else
        {
            return false;
        }
    }
    
}
