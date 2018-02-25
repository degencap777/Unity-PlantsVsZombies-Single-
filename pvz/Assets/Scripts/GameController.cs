using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public enum ZombieType
{
    Zombie1,
    Zombie2,
    FlagZombie,
    ConeHeadZombie,
    BucketHeadZombie
}
[Serializable]
public struct Wave
{
    [Serializable]
    public struct Data
    {
        public ZombieType zombieType;
        public uint count;
    }

    public bool isLargeWave;
    [Range(0f,1f)]
    public float percentage;
    public Data[] zombieData;
}

public class GameController : MonoBehaviour
{

    public GameObject zombie1;
    private GameModel model;
    public AudioClip readySound;
    public AudioClip zombieComing;
    public AudioClip hugeWaveSound;
    public AudioClip finalWaveSound;
    public AudioClip lostMusic;
    public AudioClip winMusic;

    public string nextStage;
    public GameObject progressBar;
    public GameObject gameLabel;
    public GameObject sunPrefab;
    public GameObject cardDialog;
    public GameObject sunLabel;
    public GameObject shovelBG;
    public GameObject BtnSubmitObj;
    public GameObject BtnResetObj;

    public float readyTime;
    public float elapsedTime;
    public float playTime;
    public float sunInterval;
    public Wave[] waves;
    public int initSun;
    private bool isLostGame = false;

    void Awake()
    {
        model = GameModel.GetInstance();
    }
	// Use this for initialization
	void Start ()
	{
        model.Clear();
	    model.sun = initSun;
        ArrayList flags=new ArrayList();
	    for (int i = 0; i < waves.Length; i++)
	    {
	        if (waves[i].isLargeWave||i+1==waves.Length)
	        {
	            flags.Add(waves[i].percentage);
	        }
	    }
	    progressBar.GetComponent<ProgressBar>().InitWithFlag((float[])flags.ToArray(typeof(float)));
        progressBar.SetActive(false);
        cardDialog.SetActive(false);
        sunLabel.SetActive(false);
        shovelBG.SetActive(false);
        BtnResetObj.SetActive(false);
        BtnSubmitObj.SetActive(false);
	    GetComponent<HandlerForShovel>().enabled = false;
	    GetComponent<HandlerForPlants>().enabled = false;
	    StartCoroutine(GameReady());
    }
    
    Vector3 origin
    {
        get { return new Vector3(-2,-2.6f);}
    }
    void OnDrawGizmos()
    {
        //DebugDrawGrid(origin,0.8f,1,9,5,Color.blue);
    }

    void DebugDrawGrid(Vector3 origin,float x,float y,int col,int row,Color color)
    {
        for (int i = 0; i < col+1; i++)
        {
            Vector3 startPoint = origin + Vector3.right * i * x;
            Vector3 endPoint = startPoint + Vector3.up * row * y;
            Debug.DrawLine(startPoint,endPoint,color);
        }
        for (int i = 0; i < row + 1; i++)
        {
            Vector3 startPoint = origin + Vector3.up * i * y;
            Vector3 endPoint = startPoint + Vector3.right * col * x;
            Debug.DrawLine(startPoint, endPoint, color);
        }
    }

    public void AfterSelectedCard()
    {
        BtnResetObj.SetActive(false);
        BtnSubmitObj.SetActive(false);
        Destroy(cardDialog);
        GetComponent<HandlerForShovel>().enabled = true;
        GetComponent<HandlerForPlants>().enabled = true;
        Camera.main.transform.position=new Vector3(1.1f,0,-1);
        StartCoroutine(WorkFlow());
        InvokeRepeating("ProduceSun", sunInterval, sunInterval);

    }
    IEnumerator GameReady()
    {
        yield return new WaitForSeconds(0.5f);
        MoveBy move = Camera.main.gameObject.AddComponent<MoveBy>();
        move.offset=new Vector3(3.55f,0,0);
        move.time = 1;
        move.Begin();
        yield return new WaitForSeconds(1.5f);
        sunLabel.SetActive(true);
        shovelBG.SetActive(true);
        cardDialog.SetActive(true);
        BtnResetObj.SetActive(true);
        BtnSubmitObj.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            model.sun += 50;
        }
        if (!isLostGame)
        {
            for (int row = 0; row < model.zombieList.Length; row++)
            {
                foreach (GameObject zombie in model.zombieList[row])
                {
                    if (zombie.transform.position.x < StageMap.GRID_LEFT - 0.4f)
                    {
                        LostGame();
                        isLostGame = true;
                        return;
                    }
                }
            }
        }
    }
    IEnumerator WorkFlow()
    {
        gameLabel.GetComponent<GameTips>().ShowStartTip();
        AudioManager.GetInstance().PlaySound(readySound);

        yield return new WaitForSeconds(readyTime);
        ShowProgressBar();
        AudioManager.GetInstance().PlaySound(zombieComing);
        for (int i = 0; i < waves.Length; i++)
        {
            yield return StartCoroutine(WaitForWavePercentage(waves[i].percentage));
            if (waves[i].isLargeWave)
            {
                yield return new WaitForSeconds(0.3f);
                gameLabel.GetComponent<GameTips>().ShowApproachingTip();
                AudioManager.GetInstance().PlaySound(hugeWaveSound);
                yield return new WaitForSeconds(3);
            }
            if (i + 1 == waves.Length)
            {
                gameLabel.GetComponent<GameTips>().ShowFinalTip();
                AudioManager.GetInstance().PlaySound(finalWaveSound);
            }
            CreateZombies(ref waves[i]);
        }
        yield return StartCoroutine(WaitForZombieClear());
        yield return new WaitForSeconds(2);
        WinGame();
    }

    IEnumerator WaitForZombieClear()
    {
        while (true)
        {
            bool hasZombie = false;
            for (int row = 0; row < StageMap.ROW_MAX; row++)
            {
                if (model.zombieList[row].Count != 0)
                {
                    hasZombie = true;
                    break;
                }
            }
            if (hasZombie)
            {
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                break;
            }
        }
    }
    IEnumerator WaitForWavePercentage(float percentage)
    {
        while (true)
        {
            if (elapsedTime / playTime >= percentage)
            {
                break;
            }
            else
            {
                yield return 0;
            }
        }
    }
    IEnumerator UpdateProgress()
    {
        while (true)
        {
            elapsedTime += Time.deltaTime;
            progressBar.GetComponent<ProgressBar>().SetProgress(elapsedTime/playTime);
            yield return 0;
        }
    }

    void ShowProgressBar()
    {
        progressBar.SetActive(true);
        StartCoroutine(UpdateProgress());
    }

    void CreateZombies(ref Wave wave)
    {
        foreach (Wave.Data data in wave.zombieData)
        {
            for (int i = 0; i < data.count; i++)
            {
                CreateOneZombie(data.zombieType);
            }
        }
    }
    void CreateOneZombie(ZombieType type)
    {
        GameObject zombie = null;
        switch (type)
        {
            case ZombieType.Zombie1:
                zombie = Instantiate(zombie1);
                break;
            //case ZombieType.Zombie2:
            //    break;
            //case ZombieType.FlagZombie:
            //    break;
            //case ZombieType.ConeHeadZombie:
            //    break;
            //case ZombieType.BucketHeadZombie:
            //    break;
        }
        int row = Random.Range(0, StageMap.ROW_MAX);
        zombie.transform.position = StageMap.SetZombiePos(row);
        zombie.GetComponent<ZombieMove>().row = row;
        zombie.GetComponent<SpriteDisplay>().SetOrderByRow(row);
        model.zombieList[row].Add(zombie);
    }

    void ProduceSun()
    {
        float x = Random.Range(StageMap.GRID_LEFT, StageMap.GRID_RIGHT);
        float y = Random.Range(StageMap.GRID_BOTTOM, StageMap.GRID_TOP);
        float startY = StageMap.GRID_TOP + 1.5f;
        GameObject sun = Instantiate(sunPrefab);
        sun.transform.position=new Vector3(x,startY,0);
        MoveBy move = sun.AddComponent<MoveBy>();
        move.offset=new Vector3(0,y-startY,0);
        move.time = (startY - y) / 1f;
        move.Begin();
    }

    void LostGame()
    {
        gameLabel.GetComponent<GameTips>().ShowLostTip();
        GetComponent<HandlerForPlants>().enabled = false;
        CancelInvoke("ProduceSun");
        AudioManager.GetInstance().PlayMusic(lostMusic,false);

    }

    void WinGame()
    {
        CancelInvoke("ProduceSun");
        AudioManager.GetInstance().PlayMusic(winMusic,false);
        Invoke("GotoNextStage",3);
    }

    void GotoNextStage()
    {
        SceneManager.LoadScene(nextStage);
    }
}
