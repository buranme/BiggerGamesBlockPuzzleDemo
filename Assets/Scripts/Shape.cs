using System;
using System.Collections.Generic;
using UnityEngine;

// Object that is formed by triangles and in turn forms the board
public class Shape : MonoBehaviour
{
    private bool _beingDragged;
    private Vector3 _clickMousePos;
    private GameMaster _gm;
    private Lookup _lookup;
    private Vector3 _correctPosition;
    private Transform _selfTransform;

    public List<Triangle> triangles;
    public List<Triangle> possibleTriangles; // Neighboring parentless triangles that can be added to the shape
    
    public bool isInitiallySmall;
    public bool isCorrectlyPlaced;
    public bool usedAsHint;

    private void Awake()
    {
        _selfTransform = transform;
    }
    
    public void Initialize(GameMaster gm, Board board, Lookup lookup, int i, Vector3 correctPosition)
    {
        _gm = gm;
        _lookup = lookup;
        name = i.ToString();
        _correctPosition = correctPosition;
        _selfTransform.SetParent(board.transform);
        triangles = new List<Triangle>();
        possibleTriangles = new List<Triangle>();
        _beingDragged = false;
        isInitiallySmall = true;
        isCorrectlyPlaced = false;
        usedAsHint = false;
    }

    private void Update()
    {
        if (_beingDragged)
        {
            _selfTransform.position = _lookup.cam.ScreenToWorldPoint(Input.mousePosition) + _clickMousePos;
        }
    }

    public void StartMoving()
    {
        if (!_lookup.isRunning || usedAsHint) return;
        _beingDragged = true;
        if (isInitiallySmall)
        {
            transform.localScale *= 2;
            isInitiallySmall = false;
        }

        // While holding the shape, the shape's Z is made to be the smallest among the shapes to make sure it is always visible
        _lookup.topShapeZ += Lookup.SmallZ;
        var position = _selfTransform.position;
        position = new Vector3(position.x, position.y, _lookup.topShapeZ);
        _selfTransform.position = position;

        _clickMousePos = position - _lookup.cam.ScreenToWorldPoint(Input.mousePosition);
    }

    public void StopMoving()
    {
        if (!_lookup.isRunning || usedAsHint) return;
        
        _beingDragged = false;
        _gm.DecreaseScore();
        
        // If y of the drop position is larger than a number it snaps into the grid
        var position = _selfTransform.position;
        if (position.y > _lookup.minimumYToSnap)
        {
            var x = _lookup.originPosition.x;
            var y = _lookup.originPosition.y;
            position = new Vector3((int)Math.Round(position.x + x, 0) - x, (int)Math.Round(position.y + y, 0) - y, 0);
        }
        _selfTransform.position = position;
        
        // If the position check algorithm is used the shape checks if it is in the correct spot, if it is on the correct spot
        // It calls the gm to check if the puzzle is finished
        // If the raycast check algorithm is used it directly calls on the gm
        if ((int)_lookup.algorithm == 0)
        {
            isCorrectlyPlaced = (position - _correctPosition).magnitude < 0.01f;
            if (!isCorrectlyPlaced) return;
        }
        _gm.ProcessMove();
    }

    // Gets called when the shape is called by the gm to be used as a hint
    // It snaps to its correct place and can not be moved
    public void UseAsHint()
    {
        _selfTransform.position = _correctPosition;
        isCorrectlyPlaced = true;
        usedAsHint = true;
        foreach (var triangle in triangles)
        {
            triangle.PaintTo(_lookup.hintSprite);
        }
    }
}
