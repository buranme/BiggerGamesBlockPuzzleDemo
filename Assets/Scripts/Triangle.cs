using System;
using UnityEngine;

// One of size*size*4 units that form shapes
// When clicked they call on the parent shape
public class Triangle : MonoBehaviour
{
    public Shape parentShape;
    public Tuple<int, int, int> Coordinates;
    
    [SerializeField] private SpriteRenderer triangleImage; // Sprites are saved in the scriptable object

    private void OnMouseDown()
    {
        parentShape.StartMoving();
    }

    private void OnMouseUp()
    {
        parentShape.StopMoving();
    }

    public void SetParent(Shape parent, Sprite sprite)
    {
        parent.triangles.Add(this);
        parentShape = parent;
        transform.parent = parent.transform;
        PaintTo(sprite);
    }

    public void PaintTo(Sprite sprite)
    {
        triangleImage.sprite = sprite;
    }
}
