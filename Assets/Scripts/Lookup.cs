using System.Collections.Generic;
using UnityEngine;

// Scriptable object to be used as global variable holder
[CreateAssetMenu(fileName = "LookupScriptableObject", menuName = "ScriptableObjects/Lookup")]
public class Lookup : ScriptableObject
{
    public enum Algorithm
    {
        PositionCheck,
        RaycastCheck
    }
    
    public Algorithm algorithm;             // The algorithm to be used to check endgame
    public bool isRunning;                  // Bool to pause the input in the save menu
    
    public int difficulty;                  // 0-1-2 for Easy-Medium-Hard
    public int size;                        // According to difficulty 4-5-6
    public int shapeCount;                  // According to difficulty 6-9-12
    public Vector3 originPosition;          // Origin of the Board GameObject
    public Vector3 shapesOriginPosition;    // Origin of the displaced Shapes
    public const float ShapesSpacingX = 3f; // X Spacing for Shape placement
    public const float ShapesSpacingY = 2f; // Y Spacing for Shape placement
    public float minimumYToSnap;            // Minimum Y for the shapes to start snapping
    public float boardOffset;               // How much the position of the board should be offset according to size to put it in the middle
    public float topShapeZ;                 // Float to keep track of the z of the shape in the most front
    
    public const float SmallZ = -0.0001f;   // How much topShapeZ changes with each click

    public Triangle triangleReference;
    public Shape shapeReference;
    public Camera cam;
    
    public List<Sprite> triangleSprites;
    public Sprite hintSprite;
}