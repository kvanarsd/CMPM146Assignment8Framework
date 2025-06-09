using UnityEngine;

public class Door 
{
    public enum Direction
    {
        NORTH, EAST, SOUTH, WEST
    }

    Vector2Int coordinates;
    Direction direction;

    public Door(Vector2Int coordinates, Direction direction)
    {
        this.coordinates = coordinates;
        this.direction = direction;
    }

    public Direction GetDirection()
    {
        return this.direction;
    }

    public Vector2Int GetGridCoordinates() {
        static int fdiv(int a, int b) => Mathf.FloorToInt((float)a / b);
        return new Vector2Int(
            fdiv(coordinates.x, Room.GRID_SIZE), 
            fdiv(coordinates.y, Room.GRID_SIZE)
        );
    }

    public Door GetMatching()
    {
        switch (direction)
        {
            case Direction.EAST:  return new Door(coordinates + new Vector2Int( 2,0), Direction.WEST);
            case Direction.WEST:  return new Door(coordinates + new Vector2Int(-2,0), Direction.EAST);
            case Direction.NORTH: return new Door(coordinates + new Vector2Int(0, 2), Direction.SOUTH);
            default:              return new Door(coordinates + new Vector2Int(0,-2), Direction.NORTH); // Direction.SOUTH
        }
    }

    public bool IsMatching(Door other)
    {
        Door match = GetMatching();
        return (match.coordinates == other.coordinates && match.direction == other.direction);
    }

    public Direction GetMatchingDirection()
    {
        switch (direction)
        {
            case Direction.EAST: return Direction.WEST;
            case Direction.WEST: return Direction.EAST;
            case Direction.NORTH: return Direction.SOUTH;
            case Direction.SOUTH: return Direction.NORTH;
        }
        return Direction.NORTH;
    }

    public override string ToString()
    {
        return GetGridCoordinates().ToString() + " " + direction.ToString();
    }

    public bool IsVertical()
    {
        return (direction == Direction.NORTH || direction == Direction.SOUTH);
    }

    public bool IsHorizontal()
    {
        return (direction == Direction.EAST || direction == Direction.WEST);
    }
}
