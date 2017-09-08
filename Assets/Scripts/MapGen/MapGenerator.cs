﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MapGen {

    public class MapGenerator : MonoBehaviour
    {
       


        [Serializable]
        public enum TileType
        {
            NONE,
            Tile5m,
            Tile10m,
            Tile20m,
            Tile30m,
            Tile40m,
            TileRoad
        }

        [Serializable]
        public class SourceTilePreset
        {
            public TileType Type;
            public List<GameObject> Tiles;

            public GameObject GetRandomTile()
            {
                int index = UnityEngine.Random.Range(0, Tiles.Count);
                return Tiles[index];
            }

            public int getWidth()
            {
                switch (Type)
                {
                    case TileType.Tile5m: return 1;
                    case TileType.Tile10m: return 2;
                    case TileType.Tile20m: return 4;
                    case TileType.Tile30m: return 6;
                    case TileType.Tile40m: return 8;

                        // TODO  all other sizes
                }
                return -1; // this should never happen!
            }
        }
        public Vector2 bounds;

        private Vector3 TileOriginOffset;

        private Transform Structures;
        private Transform Roads;
        
        /// <summary>
        /// 
        /// </summary>
        public List<SourceTilePreset> SourceTilePresets;
        public List<GameObject> RoadTilePresets;

        /// <summary>
        ///
        /// </summary>
        public void GenerateGrid()
        {
            int columns = (int)bounds.x;
            int rows = (int)bounds.y;



            // make an array of gameobjects
            GameObject[,] mapObjects = new GameObject[columns, rows];

            AddRoads(mapObjects);
            AddBuildings(mapObjects);
        }

        void AddRoads(GameObject[,] mapObjects)
        {
            int columns = (int)bounds.x;
            int rows = (int)bounds.y;

            bool[,] isRoad = new bool[columns, rows];

            int numRoads = 70;

            // prepick some starting values
            List<int> indicesX = new List<int>(numRoads);
            List<int> indicesY = new List<int>(numRoads);

            // fill these in -TODO

            int priorX = 0;
            int priorY = 0;

            for (int i = 0; i < numRoads; i++)
            {
                indicesX.Add( priorX += UnityEngine.Random.Range(3, 4) ); // Generaete offset from prior value, store in indeces and update prior value
                indicesY.Add( priorY += UnityEngine.Random.Range(3, 4) );
            }

            //add roads

            int verticalCount = 0;
            int verticalWeight = 5;

            for (int i = 0; i < numRoads; i++)
            {
                if (indicesX.Count == 0 || indicesY.Count == 0) break; // If run out of parallels, stop generating

                bool isVertical = (UnityEngine.Random.Range(0, 100) < 50 - verticalCount);
                int indexX, indexY;

                if (isVertical)
                {
                    indexX = indicesX[UnityEngine.Random.Range(0, indicesX.Count)];
                    indexY = UnityEngine.Random.Range(0, rows);

                    indicesX.Remove(indexX);
                    verticalCount += verticalWeight; // increase chance of horizontal
                }
                else
                {
                    // get a random point (or use pre-picked values)
                    indexX = UnityEngine.Random.Range(0, columns);
                    indexY = indicesY[UnityEngine.Random.Range(0, indicesY.Count)];

                    indicesY.Remove(indexY);
                    verticalWeight -= verticalWeight; // decrease chance of horizontal
                }

                if (indexX >= bounds.x || indexY >= bounds.y)
                {
                    i--;
                    continue; // Conform to bounds
                }


                // move up and down* filling in road markers till we hit a road
                int ix = indexX;
                int iy = indexY;
                if (isVertical)
                {
                    // go up
                    while (iy < rows && CanPlaceRoad(isRoad, ix, iy, 30))
                    {
                        isRoad[ix, iy] = true;
                        iy++;
                    }
                    // go back to start and go down
                    iy = indexY-1;
                    while (iy >= 0 && CanPlaceRoad(isRoad, ix, iy, 30))
                    {
                        isRoad[ix, iy] = true;
                        iy--;
                    }
                }
                else
                {
                    // go up
                    while (ix < columns && CanPlaceRoad(isRoad, ix, iy, 30))
                    {
                        isRoad[ix, iy] = true;
                        ix++;
                    }
                    // go back to start and go down
                    ix = indexX-1;
                    while (ix >= 0 && CanPlaceRoad(isRoad, ix, iy, 30))
                    {
                        isRoad[ix, iy] = true;
                        ix--;
                    }
                }

                
                // flip the up/down directions
                isVertical = !isVertical;


            }

            // quick printout (debug)
            for (int c = 0; c < columns; c++)
            {
                string line = "";
                for (int r = 0; r < rows; r++)
                {
                    line += isRoad[c, r] ? "o" : ".";

                    // if returns "o" (that means a road is there, do this code 
                    // {
                    if (isRoad[c, r])
                    {
                        // Check adjacent tiles for other roads
                        var LeftCheck = c > 0 && isRoad[c - 1, r];
                        var RightCheck = c < isRoad.GetLength(0) - 1 && isRoad[c + 1, r]; // should work tm (get length)
                        var BottomCheck = r > 0 && isRoad[c, r - 1];
                        var TopCheck = r < isRoad.GetLength(1) - 1 && isRoad[c, r + 1];

                        var adjacents = 0;
                        if (LeftCheck) adjacents+=1;
                        if (RightCheck) adjacents+=2;
                        if (BottomCheck) adjacents+=4;
                        if (TopCheck) adjacents+=8;

                        // make sure these indices match in the inspector or it wont work!!!
                        // tile indexs should be 0 = end, 1 = turn, 2 = straight, 3 = Tjunction, 4 = intersection. -1 means nothing at all

                        int[] tiles ={-1, 0, 0, 2,
                                       0, 1, 1, 3,
                                       };
                        float[] angles = { 0, 0, 90,};

                        if (adjacents != 0)
                        {
                            // Instantiate:
                            GameObject prefab = RoadTilePresets[adjacents];

                            GameObject go = Instantiate(prefab);
                            go.transform.parent = Roads;
                            go.transform.localPosition = TileOriginOffset + new Vector3(5 * c + 2.5f, 0, 5 * r + 2.5f); // TODO cheeky 5m hack, probably OK
                            go.transform.localEulerAngles = new Vector3(0, angles[adjacents], 0);
                            go.name = "MapTile_" + c + "_" + r;

                            mapObjects[c, r] = go; // fills array
                        }
                    }
                    // }
                }
               // print(line);
            }


            //fill in gameObjects
          

        }

        private bool CanPlaceRoad(bool[,] isRoad, int ix, int iy, int crossChance)
        {
            return (isRoad[ix, iy] == false || UnityEngine.Random.Range(0, 100) < crossChance);
        }

        void AddBuildings(GameObject[,] mapObjects)
        {
            int columns = (int)bounds.x;
            int rows = (int)bounds.y;

            // add buildings
            for (int c = 0; c < columns; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    if (mapObjects[c, r] == null) // if squares empty, put it in
                    {
                        // find all the pieces that fit and store in a local list
                        List<SourceTilePreset> onesThatFit = new List<SourceTilePreset>();

                        foreach (SourceTilePreset pre in SourceTilePresets)
                        {
                            // does it fit?
                            bool fits = true;
                            int w = pre.getWidth();
                            if (c + w - 1 >= columns)
                                fits = false;
                            if (r + w - 1 >= rows)
                                fits = false;

                            // check all squares that this one will fill, to make sure they're all empty
                            if (fits)
                            {
                                for (int i = 0; i < w; i++)
                                {
                                    for (int j = 0; j < w; j++)
                                    {
                                        if (mapObjects[c + i, r + j] != null)
                                            fits = false;
                                    }
                                }
                            }
                            if (fits)
                            {
                                onesThatFit.Add(pre);
                            }
                        }

                        int index = UnityEngine.Random.Range(0, onesThatFit.Count);
                        SourceTilePreset preset = onesThatFit[index];
                        GameObject prefab = preset.GetRandomTile();

                        // instansiates a prefab
                        GameObject go = Instantiate(prefab);
                        go.transform.parent = Structures;
                        go.transform.localPosition = TileOriginOffset + new Vector3(5 * c, 0, 5 * r); // TODO cheeky 5m hack, probably OK
                        go.transform.localRotation = Quaternion.identity;
                        go.name = "MapTile_" + c + "_" + r;

                        //write to mapObjects array
                        int width = preset.getWidth();
                        for (int i = 0; i < width; i++)
                        {
                            for (int j = 0; j < width; j++)
                            {
                                mapObjects[c + i, r + j] = go; // fills array
                            }
                        }
                    }
                }
            }
        }
        void Start()
        {

            Structures = transform.Find("Structures") ?? transform; // Find "Structures" child, else use "this" transform
            Roads = transform.Find("Roads") ?? transform;

            TileOriginOffset = new Vector3(5 * bounds.x * -0.5f, 0, 5 * bounds.y * -0.5f);
            GenerateGrid();
            
            NavMeshGen.BuildNavMesh(Roads, new Bounds( transform.position, new Vector3(5 * bounds.x, 1,5 *  bounds.y)));
        }
    }
}

