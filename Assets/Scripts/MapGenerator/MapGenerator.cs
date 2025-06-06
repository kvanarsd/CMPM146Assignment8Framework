using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class MapGenerator : MonoBehaviour
{
    public List<Room> rooms;
    public Hallway vertical_hallway;
    public Hallway horizontal_hallway;
    public Room start;
    public Room target;

    // Constraint: How big should the dungeon be at most
    // this will limit the run time (~10 is a good value 
    // during development, later you'll want to set it to 
    // something a bit higher, like 25-30)
    public int MAX_SIZE;

    // set this to a high value when the generator works
    // for debugging it can be helpful to test with few rooms
    // and, say, a threshold of 100 iterations
    public int THRESHOLD;

    // keep the instantiated rooms and hallways here 
    private List<GameObject> generated_objects;
    
    int iterations;

    public void Generate()
    {
        // dispose of game objects from previous generation process
        foreach (var go in generated_objects)
        {
            Destroy(go);
        }
        generated_objects.Clear();

        generated_objects.Add(start.Place(new Vector2Int(0, 0))); // first room
        List<Door> doors = start.GetDoors(); // doors with no matching door
        List<Vector2Int> occupied = new List<Vector2Int>(); // all occupied positions 
        occupied.Add(new Vector2Int(0, 0));
        iterations = 0;
        GenerateWithBacktracking(occupied, doors, 1);


        // Place rooms after done with backtracking
        // when placing a room upwards or to the left you have to adjusts the coords 
    }


    bool GenerateWithBacktracking(List<Vector2Int> occupied, List<Door> doors, int depth)
    {
        if (iterations > THRESHOLD) throw new System.Exception("Iteration limit exceeded");

        // if not open doors then we can stop
        if (doors.Count == 0)
        {
            return true;
        }

        var door = doors[0];
        foreach (var room in rooms)
        {
            if (room.HasDoorOnSide(door.GetMatchingDirection()))
            {
                Debug.Log("Found a Matching room");
                var coordinates = door.GetMatching().GetGridCoordinates();
                
                // coords should be adjusted if were going up or left
                Debug.Log(room.GetGridSize());
                if (door.GetDirection() == Door.Direction.NORTH)
                {
                    coordinates -= new Vector2Int(0, room.GetGridSize()[1]);
                }

                if (door.GetDirection() == Door.Direction.WEST)
                {
                    coordinates -= new Vector2Int(room.GetGridSize()[0], 0);
                }
                // update list of open doors ( add new and remove old )
                // check occupied, make sure this new room is not on any old rooms
                // updated occupied add this room

                // backtrack -- pass in copy of lists
                // if return true then continue
                // if return false undo changes
                room.Place(coordinates);
                // instantiate a hallway with the correct direction
            }
        }
        // foreach (var door in doors) {
        //     Debug.Log(door.GetMatching().GetGridCoordinates());
        // }
        // 
        

        iterations++;
        return false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        generated_objects = new List<GameObject>();
        Generate();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.gKey.wasPressedThisFrame)
            Generate();
    }
}
