using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour
{
    public int stage;
    public Animator stageAnim;
    public Animator clearAnim;
    public Animator fadeAnim;
    public Transform playerPos;

    public string[] enemyObjs;
    public Transform[] spawnPoints;

    public float nextSpawnDelay;
    public float curSpawnDelay;

    public GameObject player;
    public Text scoreText;
    public Image[] lifeImage;
    public Image[] BoomImage;
    public GameObject gameOverSet;
    public ObjectManager objectManager;

    public List<Spawn> spawnList;
    public int spawnIndex;
    public bool spawnEnd;

    void Awake()
    {
        spawnList = new List<Spawn>();
        enemyObjs = new string[] {"enemyS", "enemyM", "enemyL", "enemyB"};
        StageStart();
    }

    public void StageStart()
    {
        //stage ui load
        stageAnim.SetTrigger("On");
        stageAnim.GetComponent<Text>().text = "Stage " + stage + "\nStart";
        clearAnim.GetComponent<Text>().text = "Stage " + stage + "\nClear!!";
        //enemy spawn file read
        ReadSpawnFile();

        //fade in
        fadeAnim.SetTrigger("In");
    }

    public void StageEnd()
    {
        //clear ui load
        clearAnim.SetTrigger("On");

        //fade out
        fadeAnim.SetTrigger("Out");

        //player repos
        player.transform.position = playerPos.position;

        //stage increament
        stage++;
        if (stage > 2)
            Invoke("GameOver",6);
        else
            Invoke("StageStart",5);
    }

    void ReadSpawnFile()
    {
        //1. 변수초기화
        spawnList.Clear();
        spawnIndex = 0;
        spawnEnd = false;

        //2. file read
        TextAsset textFile = Resources.Load("Stage "+ stage) as TextAsset;
        StringReader stringReader = new StringReader(textFile.text);

        while(stringReader != null)
        {//respawn data create
            string line = stringReader.ReadLine();

            if (line == null) break;

            Spawn spawnData = new Spawn();
            spawnData.delay = float.Parse(line.Split(',')[0]);
            spawnData.type = line.Split(',')[1];
            spawnData.point = int.Parse(line.Split(',')[2]);
            spawnList.Add(spawnData);
        }
        // file close
        stringReader.Close();

        //first spawn delay
        nextSpawnDelay = spawnList[0].delay;
    }
    void Update()
    {
        curSpawnDelay += Time.deltaTime;

        if(curSpawnDelay > nextSpawnDelay && !spawnEnd)
        {
            SpawnEnemy();
            curSpawnDelay = 0;
        }

        //#ui score update
        Player playerLogic = player.GetComponent<Player>();
        scoreText.text = string.Format("{0:n0}", playerLogic.score);
    }

    void SpawnEnemy()
    {
        int enemyIndex = 0;
        switch (spawnList[spawnIndex].type)
        {
            case "S":
                enemyIndex = 0; 
                break;
            case "M":
                enemyIndex = 1; 
                break;
            case "L":
                enemyIndex = 2;
                break;
            case "B":
                enemyIndex = 3;
                break;

        }
        int enemyPoint = spawnList[spawnIndex].point;
        GameObject enemy = objectManager.MakeObj(enemyObjs[enemyIndex]);
        enemy.transform.position = spawnPoints[enemyPoint].position;

        Rigidbody2D rigid = enemy.GetComponent<Rigidbody2D>();
        Enemy enemyLogic = enemy.GetComponent<Enemy>();
        enemyLogic.player = player;
        enemyLogic.objectManager = objectManager;
        enemyLogic.gameManager = this;

        if (enemyPoint == 5 || enemyPoint == 6)
        { //right spawn
            enemy.transform.Rotate(Vector3.back * 90);
            rigid.velocity = new Vector2(enemyLogic.speed * (-1), -1);
        }
        else if (enemyPoint == 7  || enemyPoint == 8)
        {//left spawn
            enemy.transform.Rotate(Vector3.forward * 90);
            rigid.velocity = new Vector2(enemyLogic.speed, -1);
        }
        else
        {//front spawn
            rigid.velocity = new Vector2(0, enemyLogic.speed * (-1));
        }

        //respawn index up
        spawnIndex++;
        if(spawnIndex == spawnList.Count)
        {
            spawnEnd = true;
            return;
        }

        //next respawn delay
        nextSpawnDelay = spawnList[spawnIndex].delay;
    }

    public void UpdateLifeIcon(int life)
    {
        //ui life init disable
        for(int index=0; index<3; index++)
        {
            lifeImage[index].color = new Color(1, 1, 1, 0);
        }

        //ui life init disable
        for (int index = 0; index < life; index++)
        {
            lifeImage[index].color = new Color(1, 1, 1, 1);
        }

    }
    public void UpdateBoomIcon(int boom)
    {
        //ui life init disable
        for (int index = 0; index < 3; index++)
        {
            BoomImage[index].color = new Color(1, 1, 1, 0);
        }

        //ui life init disable
        for (int index = 0; index < boom; index++)
        {
            BoomImage[index].color = new Color(1, 1, 1, 1);
        }

    }

    public void RespawnPlayer()
    {
        Invoke("RespawnPlayerExe", 2f);
    }
    void RespawnPlayerExe()
    {
        player.transform.position = Vector3.down;
        player.SetActive(true);

        Player playerLogic = player.GetComponent<Player>();
        playerLogic.isHit = false;
    }

    public void CallExplosion(Vector3 pos, string type)
    {
        GameObject explosion = objectManager.MakeObj("Explosion");
        Explosion explosionLogic = explosion.GetComponent<Explosion>();

        explosion.transform.position = pos;
        explosionLogic.StartExplosion(type);
    }

    public void GameOver()
    {
        gameOverSet.SetActive(true);
    }

    public void GameRetry()
    {
        SceneManager.LoadScene(0);
    }
}
