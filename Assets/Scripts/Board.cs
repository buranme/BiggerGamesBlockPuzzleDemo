using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// Object to create and manipulate triangles and shapes. Some heavy methods are divided to the Helper class 
public class Board : MonoBehaviour
{
    [SerializeField] private Lookup lookup;
    [SerializeField] private GameMaster gm;

    private Triangle[,,] _triangleGrid;             // 3D array to hold the triangles
    private List<Triangle> _availableTriangles;     // The triangles that don't belong to a shape
    private List<Shape> _shapes;                    // All the shapes
    private List<Shape> _availableShapes;           // Shapes that have potential child triangles
    private Transform _boardTransform;
    public bool initialized;

    private Helper _helper;

    public void InitializeBoard()
    {
        _availableTriangles = new List<Triangle>();
        _shapes = new List<Shape>();
        _availableShapes = new List<Shape>();
        _helper = new Helper(gm, this, lookup);

        _boardTransform = transform;
        _boardTransform.localScale *= lookup.size;
        
        // Helper creates the triangles and saves them on the 3D array. All triangles are initially available
        _triangleGrid = new Triangle[lookup.size, lookup.size, 4];
        _helper.CreateTriangles(ref _triangleGrid, ref _availableTriangles);
        initialized = true;
    }

    // Gets called by the gm if the player successfully chose a save file to load
    // shapeCount amount of shapes are initialized and the triangles specified in the file are given to them as children
    public void LoadShapes(ref List<List<Tuple<int, int, int>>> shapesData)
    {
        for (var i = 0; i < lookup.shapeCount; i++)
        {
            var (a,b,c) = shapesData[i][0];
            var shapePosition = _triangleGrid[a, b, c].transform.position;
            var shape = _helper.InitializeShape(ref _shapes, ref _availableShapes, i, shapePosition);

            foreach (var (x,y,z) in shapesData[i])
            {
                var triangle = _triangleGrid[x, y, z];
                triangle.SetParent(shape, lookup.triangleSprites[i]);
            }
        }
        Helper.ShuffleShapes(ref _shapes);
    }

    // Gets called by the gm if the player chose one of the three difficulties
    // shapeCount amount of shapes are initiated and one random triangle is given to each of them
    public void CreateShapes()
    {
        for (var i = 0; i < lookup.shapeCount; i++)
        {
            var triangle = _availableTriangles[Random.Range(0, _availableTriangles.Count)];
            var shapePosition = triangle.transform.position;
            var shape = _helper.InitializeShape(ref _shapes, ref _availableShapes, i, shapePosition);
            
            AddTriangleToShape(shape, i, triangle);
        }
    }

    // Method to give triangles to the shapes until there is no triangle available
    // Randomly picks an available shape, then randomly picks a neighboring parentless triangle and adds it to the shape
    public void FillShapes()
    {
        while (_availableTriangles.Count > 0)
        {
            var shape = _availableShapes[Random.Range(0, _availableShapes.Count)];
            var triangle = shape.possibleTriangles[Random.Range(0, shape.possibleTriangles.Count)];
            
            AddTriangleToShape(shape, _shapes.IndexOf(shape), triangle);
        }
        Helper.ShuffleShapes(ref _shapes);
    }

    // Method to add a given triangle to a given shape, afterwards checks if there are any new neighboring parentless triangles to be considered
    // How these triangles are found and the method behind x, y and z calculations are explained in the addendum file
    private void AddTriangleToShape(Shape shape, int shapeIndex, Triangle triangle)
    {
        _availableTriangles.Remove(triangle);
        triangle.SetParent(shape, lookup.triangleSprites[shapeIndex]);
        
        var (x, y, z) = triangle.Coordinates;
        
        AddPossibleTriangleToShape(shape, _triangleGrid[x, y, (z + 1) % 4]);
        AddPossibleTriangleToShape(shape, _triangleGrid[x, y, (z + 3) % 4]);

        x += (z - 1) % 2;
        y += (z - 2) % 2;

        if (x > -1 && x < lookup.size && y > -1 && y < lookup.size)
        {
            AddPossibleTriangleToShape(shape, _triangleGrid[x, y, (z + 2) % 4]);
        }
        
        // Removes the triangles from all the shapes' possible triangle lists so that no shape ever considers this triangle again
        Helper.RemovePossibleTriangle(ref _availableShapes, triangle);
    }

    // Method to add a given potential triangle to a given shape so that the shape might consider the triangle later
    // Checks if the given triangle is parentless, and the shape shouldn't have the triangle or had been considering it
    private void AddPossibleTriangleToShape(Shape shape, Triangle triangle)
    {
        if (_availableTriangles.Contains(triangle) && !shape.triangles.Contains(triangle) && !shape.possibleTriangles.Contains(triangle))
        {
            shape.possibleTriangles.Add(triangle);
        }
    }

    // Gets called by the gm when the user asks for a hint. Picks random shape and uses it as hint
    public void PlaceAShapeCorrectly()
    {
        var shape = _shapes[Random.Range(0, _shapes.Count)];
        shape.UseAsHint();
    }

    // The first algorithm to check for end game. Just checks if all the shapes are in the positions they are supposed to be in
    // The problems and further discussion is in the addendum
    public bool PositionCheckAlgorithm()
    {
        foreach (var shape in _shapes)
        {
            if (!shape.isCorrectlyPlaced)
                return false;
        }
        return true;
    }

    // The second algorithm to check for end game. Simply raycasts through every single triangle position and checks if there is only one shape
    // The problems and further discussion is in the addendum
    public bool RaycastCheckAlgorithm()
    {
        var hit = new RaycastHit2D[2];
        for (var i = 0; i < lookup.size; i++)
        {
            for (var j = 0; j < lookup.size; j++)
            {
                if (!_helper.RayCastSquare(ref hit, i, j))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void RefreshLists()
    {
        _availableShapes = new List<Shape>();
        _availableTriangles = new List<Triangle>();
        foreach (var shape in _shapes)
        {
            _availableShapes.Add(shape);
        }
        foreach (var triangle in _triangleGrid)
        {
            triangle.transform.SetParent(transform);
            _availableTriangles.Add(triangle);
        }
    }

    public ref List<Shape> GetShapes()
    {
        return ref _shapes;
    }
}
