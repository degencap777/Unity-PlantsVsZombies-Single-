using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantSpriteDisplay : MonoBehaviour,SpriteDisplay
{

    public int orderOffset = 0;
    private SpriteRenderer shadow;
    private SpriteRenderer plant;

    void Awake()
    {
        shadow = transform.Find("Shadow").GetComponent<SpriteRenderer>();
        plant = transform.Find("Plant").GetComponent<SpriteRenderer>();
    }
    public void SetAlpha(float a)
    {
        Color color=Color.white;
        color.a = a;
        shadow.color = color;
        plant.color = color;
    }

    public void SetOrderByRow(int row)
    {
        plant.sortingOrder = 1000 * (row + 1) + orderOffset;
    }

    public void SetOrder(int order)
    {
        plant.sortingOrder = order;
    }
}
