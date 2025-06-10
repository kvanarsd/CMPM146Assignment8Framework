using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class MapGenerator : MonoBehaviour
{
    public List<Room> rooms;
    public Hallway vertical_hallway;
    public Hallway horizontal_hallway;
    public Room start;
    public Room target;
    public int MIN_SIZE = 10;

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
        Dictionary<Vector2Int, Room> occupied = new() // all occupied positions
        {
            { new Vector2Int(0, 0), start }
        };
        iterations = 0;
        GenerateWithBacktracking(occupied, doors, 1);

        // Place rooms after done with backtracking
        // when placing a room upwards or to the left you have to adjusts the coords 
    }

    public static List<T> CloneList<T>(List<T> input)
    {
        var output = new List<T>();
        foreach (var item in input) output.Add(item);
        return output;
    }

    public static Dictionary<K, V> CloneDictionary<K, V>(Dictionary<K, V> input)
    {
        var output = new Dictionary<K, V>();
        foreach (var (key, value) in input) output.Add(key, value);
        return output;
    }

    bool GenerateWithBacktracking(Dictionary<Vector2Int, Room> occupied, List<Door> doors, int depth)
    {
        if (iterations > THRESHOLD) throw new System.Exception("Iteration limit exceeded");
        iterations++;

        if (occupied.Count > MAX_SIZE)
        {
            return false;
        }

        Door door = null;

        // Find a door on the frontier (this is so that we don't need to remove anything form doors as that seems hard to do accuratly in all scenarios.)
        for (int i = 0; i < doors.Count; i++)
        {
            Door tempDoor = doors[i];
            if (!occupied.ContainsKey(tempDoor.GetMatching().GetGridCoordinates()))
            {
                door = tempDoor;
                break;
            }
        }

        if (door == null)
        {
            if (depth < MIN_SIZE)
            {
                Debug.Log("Dungeon is too small keep going >:(");
                return false;
            }
            //Debug.Log("no door found!");
            return true;
        }

        Vector2Int pos = door.GetMatching().GetGridCoordinates();

        List<Room> options = FindValidRooms(pos, occupied);

        foreach (var room in options)
        {
            Dictionary<Vector2Int, Room> newOccupied = CloneDictionary(occupied);
            List<Door> newDoors = CloneList(doors);
            newOccupied.Add(pos, room);
            foreach (Door newDoor in room.GetDoors(pos)) newDoors.Add(newDoor);
            bool worked = GenerateWithBacktracking(newOccupied, newDoors, depth + 1);
            if (worked)
            {
                
                generated_objects.Add(room.Place(pos));
                foreach (var hall_door in room.GetDoors(room.position))
                {
                    if (hall_door.GetDirection() == Door.Direction.NORTH)
                    {
                        generated_objects.Add(vertical_hallway.Place(hall_door));
                    }

                    if (hall_door.GetDirection() == Door.Direction.WEST)
                    {
                        generated_objects.Add(horizontal_hallway.Place(hall_door));
                    }

                }
                return true;
            }
        }
        
        return false;
    }

    public List<Room> FindValidRooms(Vector2Int pos, Dictionary<Vector2Int, Room> occupied)
    {
        Vector2Int eastPos = pos + new Vector2Int(1, 0);
        Vector2Int westPos = pos + new Vector2Int(-1, 0);
        Vector2Int northPos = pos + new Vector2Int(0, 1);
        Vector2Int southPos = pos + new Vector2Int(0, -1);
        Room east = occupied.GetValueOrDefault(eastPos);
        Room west = occupied.GetValueOrDefault(westPos);
        Room north = occupied.GetValueOrDefault(northPos);
        Room south = occupied.GetValueOrDefault(southPos);

        List<Room> options = new();
        foreach (var room in rooms)
        {

            if (east && (room.HasDoorOnSide(Door.Direction.EAST) != east.HasDoorOnSide(Door.Direction.WEST))) continue;
            if (west && (room.HasDoorOnSide(Door.Direction.WEST) != west.HasDoorOnSide(Door.Direction.EAST))) continue;
            if (north && (room.HasDoorOnSide(Door.Direction.NORTH) != north.HasDoorOnSide(Door.Direction.SOUTH))) continue;
            if (south && (room.HasDoorOnSide(Door.Direction.SOUTH) != south.HasDoorOnSide(Door.Direction.NORTH))) continue;
            options.Add(room);
        }
        Shuffle(options);
        return options;
    }

    // TODO PAIC
    public static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[j], list[i]) = (list[i], list[j]);
        }
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
