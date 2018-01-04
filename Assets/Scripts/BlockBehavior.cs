using cakeslice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBehavior : MonoBehaviour {

    //PUBLIC
    public GameObject select;
    public enum BlockTypeEnum {ROAD, GRASS, FOREST, ROCK};
    public BlockTypeEnum blockType;

    public bool canBeDamage;
    public bool canMoveOn;
    public float health;


    //PRIVATE
    private GameController gameController;

    private List<GameObject> neighborsBlocks = new List<GameObject>();
    private GameObject unitOnBlock;
    private bool active;

    private int weight;




    private void Awake()
    {
        gameController = Camera.main.GetComponent<GameController>();
    }

    void Start () {
        active = false;
        weight = 0;
	}
	
	void Update () {
		
	}

    public void AddNeighbor(GameObject n)
    {
        neighborsBlocks.Add(n);
    }

    public void RemoveNeighbor(GameObject n)
    {
        neighborsBlocks.Remove(n);
    }

    public List<GameObject> GetNeighbors()
    {
        return neighborsBlocks;
    }

    public void SetUnitOnBlock(GameObject u)
    {
        unitOnBlock = u;
    }

    public GameObject GetUnitOnBlock()
    {
        return unitOnBlock;
    }

    public void ToggleActive(bool b)
    {
        active = b;
        select.SetActive(b);

        if (b)
        {
            //UpdateSelectColor();          //SET COLOR CASSE LES DRAW CALLS
        }
        else
        {
            //select.GetComponent<Renderer>().material.SetColor("_Color", gameController.selectColor[0]);
            SetWeight(0);
        }
    }

    public void UpdateSelectColor()
    {
        switch (gameController.GetMode())
        {
            case GameController.ModesEnum.NONE:
                select.GetComponent<Renderer>().material.SetColor("_Color", gameController.selectColor[0]);
                break;
            case GameController.ModesEnum.MOVEMENT:
                if (!canMoveOn)
                {
                    select.GetComponent<Renderer>().material.SetColor("_Color", gameController.selectColor[2]);
                }
                else if (gameController.GetSelectedBlock() == transform.gameObject)
                {
                    select.GetComponent<Renderer>().material.SetColor("_Color", gameController.selectColor[1]);
                }
                break;
            case GameController.ModesEnum.ATTACK:
                if (canBeDamage || GetUnitOnBlock()) //Need to verify team
                {
                    
                    if (gameController.GetSelectedBlock() == transform.gameObject)
                    {
                        select.GetComponent<Renderer>().material.SetColor("_Color", gameController.selectColor[1]);
                    }
                    else if (GetUnitOnBlock() && GetUnitOnBlock().tag == gameController.GetSelectedUnit().tag)
                    {
                        select.GetComponent<Renderer>().material.SetColor("_Color", gameController.selectColor[3]);
                    }
                    else
                    {
                        select.GetComponent<Renderer>().material.SetColor("_Color", gameController.selectColor[2]);
                    }
                }
                break;
            default:
                break;
        }
    }

    public bool IsActive()
    {
        return active;
    }


    public void DealDamage(float dmg)
    {
        if (canBeDamage)
        {
            health -= dmg;

            if (health <= 0)
            {
                gameController.DestroyBlock(transform.gameObject);
            }
        }
        
    }



    public int GetWeight()
    {
        return weight;
    }

    public void SetWeight(int w)
    {
        weight = w;
    }
}
