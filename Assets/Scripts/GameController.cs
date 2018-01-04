using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;

public class GameController : MonoBehaviour {

    /***PUBLIC VARIABLES***/
    public GameObject terrain;
    public GameObject[] blockArray; //0 = road, 1 = grass, 2 = forest, 3 = rock
    public GameObject[] unitArray; //0 = test
    public bool drawTerrain = true;
    public float heightStep = 0.1f;
    public float perlinScale = 10f;
    public Material[] selectMaterials;

    //TEST
    public float forestScale = 30f;
    [Range(0.0f, 1.0f)]
    public float forestHeightMax = 0.35f;
    [Range(0.0f, 1.0f)]
    public float forestHeightMin = 0.0f;
    [Range(0.0f, 1.0f)]
    public float rockHeightMax = 1.0f;
    [Range(0.0f, 1.0f)]
    public float rockHeightMin = 0.7f;

    public enum ModesEnum { NONE, MOVEMENT, ATTACK };
    public Color[] selectColor; //0 = white, 1 = green, 2 = red, 3 = blue

    /***PRIVATE VARIABLES***/
    private GameObject selectedBlock = null;
    private int terrainWidth = 7, terrainLength = 21;
    private GameObject[,] terrainArray;
  
    private ModesEnum mode = ModesEnum.NONE;

    private List<GameObject> onTerrainUnitList;
    private int onTerrainUnitIndex;
    private GameObject selectedUnit;



    /***AWAKE FUNCTION***/

    public void Awake()
    {
        if (drawTerrain)
        {
            DrawTerrain();
        }
    }

    /// <summary>
    /// Draw the terrain by spawning blocks
    /// </summary>
    private void DrawTerrain()
    {
        terrainArray = new GameObject[terrainWidth, terrainLength];
        float[,] blockHeightMap = CalculatePerlinMap(terrainWidth, terrainLength, perlinScale, true);

        for (int x = 0; x < terrainWidth; x++)        //Terrain width
        {
            for (int z = 0; z < terrainLength; z++)     //Terrain length
            {
                //INSTANTIATE
                GameObject block = Instantiate(ChooseBlockType(x, z), new Vector3(x, blockHeightMap[x, z], z), new Quaternion(0f, Random.Range(0, 4) * 90f, 0f, 0f));
                terrainArray[x, z] = block;

                UpdateBlockNeighbors(block);

                OrganizeBlockInInspector(block);
            }
        }
    }

    /// <summary>
    /// Choose the type of block that will be spawn
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="z">Z coordinate</param>
    /// <returns>The block type</returns>
    private GameObject ChooseBlockType(int x, int z)
    {
        float[,] blocktypeSpawnMap = CalculatePerlinMap(terrainWidth, terrainLength, forestScale, false);
        GameObject blockType = null;

        if (x == 3 /*&& z == 0*/)     //StartPoint
        {
            blockType = blockArray[0];
        }
        else
        {
            if (blocktypeSpawnMap[x, z] > forestHeightMin && blocktypeSpawnMap[x, z] < forestHeightMax)
            {
                blockType = blockArray[2];
            }
            else if (blocktypeSpawnMap[x, z] > rockHeightMin && blocktypeSpawnMap[x, z] < rockHeightMax)
            {
                blockType = blockArray[3];
            }
            else
            {
                blockType = blockArray[1];
            }
        }

        return blockType;
    }

    /// <summary>
    /// Place object under the right parent in the inspector
    /// </summary>
    /// <param name="block">Block target</param>
    private void OrganizeBlockInInspector(GameObject block)
    {
        switch (block.GetComponent<BlockBehavior>().blockType)
        {
            case BlockBehavior.BlockTypeEnum.ROAD:
                block.transform.parent = terrain.transform.GetChild(0).transform;
                break;
            case BlockBehavior.BlockTypeEnum.GRASS:
                block.transform.parent = terrain.transform.GetChild(1).transform;
                break;
            case BlockBehavior.BlockTypeEnum.FOREST:
                block.transform.parent = terrain.transform.GetChild(2).transform;
                break;
            case BlockBehavior.BlockTypeEnum.ROCK:
                block.transform.parent = terrain.transform.GetChild(3).transform;
                break;
            default:
                break;
        }
    }

    /***START FUNCTION***/

    public void Start()
    {
        onTerrainUnitList = new List<GameObject>();
        onTerrainUnitIndex = -1;

        SpawnUnitAtCoord(3, 0, "Team1");
        SpawnUnitAtCoord(3, 4, "Team2");
        SpawnUnitAtCoord(3, 7, "Team1");

        NextTurn();
    }

    /// <summary>
    /// Spawn a unit at provided coordinates
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="z">Y coordinate</param>
    /// <param name="team">Team string tag</param>
    private void SpawnUnitAtCoord(int x, int z, string team)
    {
        //Instantiate unit at block position
        GameObject unitBlock = terrainArray[x, z];
        Vector3 unitPos = unitBlock.transform.position;
        GameObject unit = Instantiate(unitArray[0], new Vector3(unitPos.x, unitPos.y + 0.5f, unitPos.z), Quaternion.identity);
        unit.tag = team;

        //Assign unit to block and vice versa
        unitBlock.GetComponent<BlockBehavior>().SetUnitOnBlock(unit);
        unit.GetComponent<Unit>().SetBlockUnderUnit(unitBlock);

        onTerrainUnitList.Add(unit);
    }

    /// <summary>
    /// Trigger next in-game turn
    /// </summary>
    private void NextTurn()
    {
        onTerrainUnitIndex = (onTerrainUnitIndex + 1 == onTerrainUnitList.Count) ? 0 : onTerrainUnitIndex + 1;

        //Select new unit
        selectedUnit = onTerrainUnitList[onTerrainUnitIndex];

        //Select the block of the unit
        SelectBlock(selectedUnit.GetComponent<Unit>().GetBlockUnderUnit());
    }



    /***SWITCH MODE***/

    /// <summary>
    /// Switch the mode to movement mode
    /// </summary>
    public void SwitchToMoveMode()
    {
        if (mode == ModesEnum.MOVEMENT)
        {
            mode = ModesEnum.NONE;
            SelectBlock(selectedBlock);
        }
        else
        {
            mode = ModesEnum.MOVEMENT;
            //selectedBlock.GetComponent<BlockBehavior>().UpdateSelectColor();
            DeselectBlockNeighbors(selectedBlock);
            SelectBlockNeighbors(selectedBlock, selectedUnit.GetComponent<Unit>().GetMovePoints(), true);
        }
    }

    /// <summary>
    /// Switch the mode to attack mode
    /// </summary>
    public void SwitchToAttackMode()
    {
        if (mode == ModesEnum.ATTACK)
        {
            mode = ModesEnum.NONE;
            SelectBlock(selectedBlock);
        }
        else
        {
            mode = ModesEnum.ATTACK;
            //selectedBlock.GetComponent<BlockBehavior>().UpdateSelectColor();
            DeselectBlockNeighbors(selectedBlock);
            selectedUnit.GetComponent<Unit>().SelectAttackBlocks(true);
        }
    }


    /***CLICK BEHAVIOR***/

    /// <summary>
    /// Select a behavior when the player click on a block
    /// </summary>
    /// <param name="newBlock">Block clicked by the player</param>
    public void SimpleClickBehavior(GameObject newBlock)
    {
        //initialization of blockbehaviors
        BlockBehavior bbNB = null;
        BlockBehavior bbSB = null;

        if (newBlock)
        {
            bbNB = newBlock.GetComponent<BlockBehavior>();
        }
        if (selectedBlock)
        {
            bbSB = selectedBlock.GetComponent<BlockBehavior>();
        }

        //If block selected and try to move
        if (mode == ModesEnum.MOVEMENT && bbNB.IsActive() && bbNB.canMoveOn && !bbNB.GetUnitOnBlock())
        {
            UnitMove(newBlock);
            mode = ModesEnum.NONE;
        }
        //If block selected and try to attack
        else if (mode == ModesEnum.ATTACK && bbNB.IsActive() && newBlock != selectedBlock)
        {
            UnitAttack(bbSB, bbNB);
            mode = ModesEnum.NONE;
        }
    }

    /// <summary>
    /// Move selected unit from a block to an other
    /// </summary>
    /// <param name="newBlock">Destination block</param>
    private void UnitMove(GameObject newBlock)
    {
        GameObject oldBlock = selectedBlock; 
        DeselectBlock(true); //deselect old block

        //move unit to the new block position
        selectedUnit.transform.position = newBlock.transform.position + new Vector3(0f, 0.5f, 0f);

        //update blocks unit reference
        oldBlock.GetComponent<BlockBehavior>().SetUnitOnBlock(null);
        newBlock.GetComponent<BlockBehavior>().SetUnitOnBlock(selectedUnit);

        //update unit block reference
        selectedUnit.GetComponent<Unit>().SetBlockUnderUnit(newBlock);
        
        SelectBlock(newBlock);
    }

    /// <summary>
    /// Make the selected unit attack the targeted block
    /// </summary>
    /// <param name="bbSB"></param>
    /// <param name="bbNB"></param>
    private void UnitAttack(BlockBehavior bbSB, BlockBehavior bbNB)
    {
        GameObject otherUnit = bbNB.GetUnitOnBlock();
        if (otherUnit)
        {
            //Deal damage to unit if ennemy
            if (otherUnit.tag != bbSB.GetUnitOnBlock().tag && otherUnit.GetComponent<Unit>().DealDamage(bbSB.GetUnitOnBlock().GetComponent<Unit>().damage))
            {
                bbNB.SetUnitOnBlock(null);
            }
        }
        else
        {
            bbNB.DealDamage(selectedUnit.GetComponent<Unit>().damage);
            Debug.Log("Unit " + selectedUnit.GetComponent<Unit>() + " deals " + selectedUnit.GetComponent<Unit>().damage + " damages to block)");
        }

        DeselectBlock(true);
        SelectBlock(selectedUnit.GetComponent<Unit>().GetBlockUnderUnit());
    }




    /***SELECT / DESELECT***/

    /// <summary>
    /// Select only one block
    /// </summary>
    /// <param name="newBlock">Target block</param>
    private void SelectBlock(GameObject newBlock)
    {
        DeselectBlock(true);
        selectedBlock = newBlock;
        selectedBlock.GetComponent<BlockBehavior>().ToggleActive(true);
    }

    /// <summary>
    /// Select neighbors of the block passed in parameter
    /// </summary>
    /// <param name="block">Target block</param>
    /// <param name="movePointsLeft">Distance to select</param>
    /// <param name="select">Activate block or not</param>
    private void SelectBlockNeighbors(GameObject block, int movePointsLeft, bool select)
    {
        BlockBehavior bb = block.GetComponent<BlockBehavior>();

        if (movePointsLeft > 0)
        {
            List<GameObject> neighbors = bb.GetNeighbors();
            for (int i = 0; i < neighbors.Count; i++)
            {
                BlockBehavior bbN = neighbors[i].GetComponent<BlockBehavior>();

                if (bbN.canMoveOn && movePointsLeft > bbN.GetWeight() && !bbN.GetUnitOnBlock())
                {
                    neighbors[i].GetComponent<BlockBehavior>().ToggleActive(select);
                    bbN.SetWeight(movePointsLeft);
                    SelectBlockNeighbors(neighbors[i], movePointsLeft - 1, select);
                }
            }
        }
        
    }

    /// <summary>
    /// Deselect selected block
    /// </summary>
    /// <param name="neighborsToo">Does the deselection has to spread through neighbors</param>
    private void DeselectBlock(bool neighborsToo)
    {
        if (selectedBlock)
        {
            BlockBehavior bbSB = selectedBlock.GetComponent<BlockBehavior>();

            //Desactivate old block
            bbSB.ToggleActive(false);

            if (neighborsToo)
            {
                DeselectBlockNeighbors(selectedBlock);
            }

            selectedBlock = null;
            mode = ModesEnum.NONE;
        }
    }

    /// <summary>
    /// Deselect all neighbors of target block
    /// </summary>
    /// <param name="block">Target block</param>
    private void DeselectBlockNeighbors(GameObject block)
    {
        BlockBehavior bb = block.GetComponent<BlockBehavior>();

        List<GameObject> neighbors = bb.GetNeighbors();
        for (int i = 0; i < neighbors.Count; i++)
        {
            BlockBehavior bbN = neighbors[i].GetComponent<BlockBehavior>();
            if (bbN.IsActive())
            {
                neighbors[i].GetComponent<BlockBehavior>().ToggleActive(false);
                DeselectBlockNeighbors(neighbors[i]);

                //Rajouter reset couleur material select ici
            }
        }

    }



    /***PERLIN***/

    /// <summary>
    /// Calculate perlin noise at a precise point
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="z">Y coordinate</param>
    /// <param name="scale">Scale of the perlin map</param>
    /// <param name="offsetX">X offset</param>
    /// <param name="offsetZ">Y offset</param>
    /// <param name="rounded">Round values or not</param>
    /// <returns>Float of the height at the precise point</returns>
    private float CalculatePerlinHeight(int x, int z, float scale, float offsetX, float offsetZ, bool rounded)
    {
        //Set perlin coord
        float xCoord = (float)x / terrainWidth * scale + offsetX;
        float zCoord = (float)z / terrainLength * scale + offsetZ;

        //Calculate perlin height
        float perlinheight = 0;
        if (rounded)
        {
            perlinheight = Mathf.Round(Mathf.PerlinNoise(xCoord, zCoord) / heightStep) * heightStep;
        }
        else
        {
            perlinheight = Mathf.PerlinNoise(xCoord, zCoord);
        }

        return perlinheight;
    }

    /// <summary>
    /// Return a random perlin map
    /// </summary>
    /// <param name="width">Width of the perlin map</param>
    /// <param name="length">Length of the perlin map</param>
    /// <param name="scale">Scale of the perlin map</param>
    /// <param name="rounded">Round values or not</param>
    /// <returns>2 dimension float array between 0 and 1</returns>
    private float[,] CalculatePerlinMap(int width, int length, float scale, bool rounded)
    {
        float perlinOffsetX = Random.Range(0f, 99999f);
        float perlinOffsetZ = Random.Range(0f, 99999f);

        float[,] perlinMap = new float[width, length];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                perlinMap[x,z] = CalculatePerlinHeight(x, z, scale, perlinOffsetX, perlinOffsetZ, rounded);
            }
        }

        return perlinMap;
    }



    /***BLOCK MANAGEMENT***/

    /// <summary>
    /// Destroy a block and replace it by a grass block
    /// </summary>
    /// <param name="oldBlock">Block to destroy</param>
    public void DestroyBlock(GameObject oldBlock)
    {
        int x = (int)oldBlock.transform.position.x;
        float y = oldBlock.transform.position.y;
        int z = (int)oldBlock.transform.position.z;

        GameObject newBlock = Instantiate(blockArray[1], new Vector3(x, y, z), new Quaternion(0f, Random.Range(0, 4) * 90f, 0f, 0f));
        terrainArray[x, z] = newBlock;
        OrganizeBlockInInspector(newBlock);

        UpdateBlockNeighbors(newBlock, oldBlock);

        Destroy(oldBlock);
    }

    /// <summary>
    /// Update neighbors of a block
    /// </summary>
    /// <param name="newBlock">Block to update</param>
    private void UpdateBlockNeighbors(GameObject newBlock)
    {
        UpdateBlockNeighbors(newBlock, null);
    }

    /// <summary>
    /// Update neighbors of a block replacing an other
    /// </summary>
    /// <param name="oldBlock">Old block if there is one</param>
    /// <param name="newBlock">Block to update</param>
    private void UpdateBlockNeighbors(GameObject newBlock, GameObject oldBlock)
    {
        int x = (int)newBlock.transform.position.x;
        int z = (int)newBlock.transform.position.z;

        //South
        if (z > 0 && terrainArray[x, z - 1])
        {
            UpdateBlocksNeighborsRelation(newBlock, oldBlock, x, z - 1);
        }
        //North
        if (z < terrainLength - 1 && terrainArray[x, z + 1])
        {
            UpdateBlocksNeighborsRelation(newBlock, oldBlock, x, z + 1);
        }
        //West
        if (x > 0 && terrainArray[x - 1, z])
        {
            UpdateBlocksNeighborsRelation(newBlock, oldBlock, x - 1, z);
        }
        //East
        if (x < terrainWidth - 1 && terrainArray[x + 1, z])
        {
            UpdateBlocksNeighborsRelation(newBlock, oldBlock, x + 1, z);
        }
    }

    /// <summary>
    /// Update neighbors relation between old and new blocks
    /// </summary>
    /// <param name="oldBlock">Block to delete</param>
    /// <param name="newBlock">Block to take the place</param>
    /// <param name="xTer">TerrainArray x coordinate</param>
    /// <param name="zTer">TerrainArray z coordinate</param>
    private void UpdateBlocksNeighborsRelation(GameObject newBlock, GameObject oldBlock, int xTer, int zTer)
    {
        newBlock.GetComponent<BlockBehavior>().AddNeighbor(terrainArray[xTer, zTer]);
        terrainArray[xTer, zTer].GetComponent<BlockBehavior>().RemoveNeighbor(oldBlock);
        terrainArray[xTer, zTer].GetComponent<BlockBehavior>().AddNeighbor(newBlock);
    }



    /***GETTERS***/

    public GameObject GetSelectedBlock()
    {
        return selectedBlock;
    }

    public ModesEnum GetMode()
    {
        return mode;
    }

    public GameObject GetSelectedUnit()
    {
        return selectedUnit;
    }
}
