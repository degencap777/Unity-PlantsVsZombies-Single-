using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public enum State
    {
        Normal,
        NoSun,
        CD
    }
    private HandlerForPlants plantHandler;
    public Sprite enableSprite;
    public Sprite disableSprite;
    private SpriteRenderer renderer;
    public int price;
    public float cd;
    private float cdTime;
    public bool isGrowed;
    public State state = State.Normal;
    public GameModel model;
    public GameObject plant;

    public float CdTime
    {
        get { return cdTime; }
    }
    void Awake()
    {
        plantHandler = GameObject.Find("GameController").GetComponent<HandlerForPlants>();
        model=GameModel.GetInstance();
        renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = enableSprite;
        plant.GetComponent<PlantGrow>().price = price;
    }

    void Update()
    {
        UpdateUI();
        if (state == State.CD&&isGrowed)
        {
            cdTime -= Time.deltaTime;
            if (cdTime <= 0)
            {
                state = State.Normal;
            }
        }
    }
    public void OnSelected()
    {
        if (state == State.Normal)
        {
            plantHandler.SetSelectedCard(this);
        }
    }

    void UpdateUI()
    {
        CheckSun();
        if (state == State.Normal)
        {
            renderer.sprite = enableSprite;
        }
        else
        {
            renderer.sprite = disableSprite;
        }
    }

    void CheckSun()
    {
        if (model.sun < price&& state != State.CD)
        {
            state = State.NoSun;
        }
        else
        {
            if (state == State.CD)
            {
                return;
            }
            else
            {
                state = State.Normal;
                isGrowed = false;
            }
        }
    }

    public void Growed()
    {
        isGrowed = true;
        state = State.CD;
        cdTime = cd;
        model.sun -= price;
        UpdateUI();
    }

    public void SetSprite(bool enable)
    {
        if (enable)
        {
            renderer.sprite = enableSprite;
        }
        else
        {
            renderer.sprite = disableSprite;
        }
    }
}
