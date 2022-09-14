using System;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

// Class to hold helper functions for the board
public class Helper
{
    private readonly GameMaster _gm;
    private readonly Board _board;
    private readonly Lookup _lookup;
    
    public Helper(GameMaster gm, Board board, Lookup lookup)
    {
        _gm = gm;
        _board = board;
        _lookup = lookup;
    }

    // Method to instantiate the triangles and initialize the triangles
    public void CreateTriangles(ref Triangle[,,] triangleGrid, ref List<Triangle> availableTriangles)
    {
        Vector3[] triangleYaw = {new (0,0,0), new (0,0,90), new (0,0,180), new (0,0,270)};
        
        for (var i = 0; i < _lookup.size; i++)
        {
            for (var j = 0; j < _lookup.size; j++)
            {
                for (var k = 0; k < 4; k++)
                {
                    var position = _lookup.originPosition + new Vector3(i, j, 0);
                    var triangle = Object.Instantiate(_lookup.triangleReference, position, Quaternion.Euler(triangleYaw[k]));
                    triangle.Coordinates = new Tuple<int, int, int>(i, j, k);
                    triangle.name = $"{i}, {j}, {k}";
                    triangleGrid[i, j, k] = triangle;
                    availableTriangles.Add(triangle);
                }
            }
        }
    }

    // Method to get the shapes ready for the triangles. If it is the first call of the function instantiates the shapes,
    // If not uses the shapes it has instantiated before
    public Shape InitializeShape(ref List<Shape> shapes, ref List<Shape> availableShapes, int i, Vector3 position)
    {
        Shape shape;
        if (shapes.Count > i)
        {
            shape = shapes[i];
            shape.transform.position = position;
        }
        else
        {
            shape = Object.Instantiate(_lookup.shapeReference, position, Quaternion.identity);
            shapes.Add(shape);
            availableShapes.Add(shape);
        }
        shape.Initialize(_gm, _board, _lookup, i, position);
        return shape;
    }

    // Method to remove a given triangle from all the shapes' possible triangle lists so that
    // No shape considers the triangle anymore. Called after the triangle is added to a shape
    public static void RemovePossibleTriangle(ref List<Shape> availableShapes, Triangle triangle)
    {
        var copyList = new List<Shape>();
        copyList.AddRange(availableShapes);
        foreach (var elem in copyList)
        {
            if (elem.possibleTriangles.Contains(triangle))
            {
                elem.possibleTriangles.Remove(triangle);
                if (elem.possibleTriangles.Count == 0)
                {
                    availableShapes.Remove(elem);
                }
            }
        }
    }

    // Method to place the shapes randomly after creation
    public void ShuffleShapes(ref List<Shape> shapes)
    {
        var start = _lookup.shapesOriginPosition;
        for (var i = 0; i < shapes.Count; i++)
        {
            shapes[i].transform.localScale *= 0.5f;
            var offsetX = i / (_lookup.shapeCount / 3) * Lookup.ShapesSpacingX;
            var offsetY = i % (_lookup.shapeCount / 3) * Lookup.ShapesSpacingY;
            var offsetZ = i * Lookup.SmallZ;
            var pos = new Vector3( offsetX, offsetY, offsetZ);
            //var temp = Object.Instantiate(_lookup.triangleReference, start + pos, Quaternion.identity);
            //temp.PaintTo(_lookup.hintSprite);
            shapes[i].transform.position = start + pos;
        }
    }

    // Method to raycast 4 triangles in a square. Gets called size * size times in total for a complete check
    // If at any point there is 0 or more than 1 it returns without checking the rest
    public bool RayCastSquare(ref RaycastHit2D[] hit, int i, int j)
    {
        if (RayCastTriangle(ref hit, i, j, Vector2.down) != 1) return false;
        if (RayCastTriangle(ref hit, i, j, Vector2.up) != 1) return false;
        if (RayCastTriangle(ref hit, i, j, Vector2.left) != 1) return false;
        if (RayCastTriangle(ref hit, i, j, Vector2.right) != 1) return false;
        return true;
    }

    // Method to raycast a single triangle, returns the count of shapes hit
    // Used non allocated version, the results array is initialized as length of 2 for efficiency
    // Because if the result is more than 2 the count doesn't matter, it should be exactly 1
    private int RayCastTriangle(ref RaycastHit2D[] hit, int i, int j, Vector2 k)
    {
        var position = (Vector2)_lookup.originPosition + new Vector2(i, j) + k * 0.25f;
        Physics2D.RaycastNonAlloc(position, Vector3.back, hit);
        var count = 0;
        
        foreach (var h in hit)
        {
            if (h.collider == null)
            {
                hit = new RaycastHit2D[2];
                break;
            }
            count++;
        }
        return count;
    }
}
