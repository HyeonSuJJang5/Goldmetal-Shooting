using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public float maxShotDelay;
    public float curShotDelay;

    public int life;
    public int score;
    public int power;
    public int maxPower;
    public int boom;
    public int maxBoom;

    public bool isTouchTop;
    public bool isTouchBottom;
    public bool isTouchRight;
    public bool isTouchLeft;
    public bool isHit;
    public bool isBoomTime;
    public bool isRespawnTime;
    public bool isControl;
    public bool[] joyControl;
    public bool isButtonA;
    public bool isButtonB;

    public GameObject bulletObjA;
    public GameObject bulletObjB;
    public GameObject BoomEffect;

    public GameManager gameManager;
    public ObjectManager objectManager;
    public bl_Joystick js;

    Animator anim;
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        Unbeatable();
        Invoke("Unbeatable", 3);
    }

    void Unbeatable()
    {
        isRespawnTime = !isRespawnTime;

        if (isRespawnTime)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.5f);
        } else
        {
            spriteRenderer.color = new Color(1, 1, 1, 1);
        }
    }
    void Update()
    {
        Move();
        Fire();
        Boom();
        Reload();
    }

    public void JoyPanel(int type)
    {
        for (int index = 0; index<9; index++)
        {
            joyControl[index] = index == type;
        }
    }

    public void JoyDown()
    {
        isControl = true;
    }

    public void JoyUp()
    {
        isControl = false;
    }

    void Move()
    {
        //asset joystick
        Vector3 dir = new Vector3(js.Horizontal, js.Vertical, 0);
        dir.Normalize();
        transform.position += dir * speed * Time.deltaTime;

        //keyboard control value
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        
        //joy control value
        if (joyControl[0]) { h = -1; v = 1; }
        if (joyControl[1]) { h = 0; v = 1; }
        if (joyControl[2]) { h = 1; v = 1; }
        if (joyControl[3]) { h = -1; v = 0; }
        if (joyControl[4]) { h = 0; v = 0; }
        if (joyControl[5]) { h = 1; v = 0; }
        if (joyControl[6]) { h = -1; v = -1; }
        if (joyControl[7]) { h = 0; v = -1; }
        if (joyControl[8]) { h = 1; v = -1; }
        
        if ((isTouchRight && h == 1) || (isTouchLeft && h == -1) || !isControl)
            h = 0;

        if ((isTouchTop && v == 1) || (isTouchBottom && v == -1) || !isControl)
            v = 0;

        Vector3 curPos = transform.position;
        Vector3 nextPos = new Vector3(h, v, 0) * speed * Time.deltaTime;

        transform.position = curPos + nextPos;

        if (Input.GetButtonDown("Horizontal") || Input.GetButtonUp("Horizontal"))
        {
            anim.SetInteger("Input", (int)h);
        }
    }

    public void ButtonADown()
    {
        isButtonA = true;
    }

    public void ButtonAUp()
    {
        isButtonA = false;
    }

    public void ButtonBDown()
    {
        isButtonB = true;
    }

    public void ButtonBUp()
    {
        isButtonB = false;
    }

    void Fire()
    {
        //if (!Input.GetButton("Fire1"))
        //    return;

        if (!isButtonA)
            return;

        if (curShotDelay < maxShotDelay)
            return;

        switch (power)
        {
            case 1:
                //Power One
                GameObject bullet = objectManager.MakeObj("bulletPlayerA");
                bullet.transform.position = transform.position;

                Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                break;
            case 2:
                GameObject bulletR = objectManager.MakeObj("bulletPlayerA");
                bulletR.transform.position = transform.position + Vector3.right * 0.1f;

                GameObject bulletL = objectManager.MakeObj("bulletPlayerA");
                bulletL.transform.position = transform.position + Vector3.left * 0.1f;

                Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
                Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();
                rigidR.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                rigidL.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                break;
            case 3:
                GameObject bulletRR = objectManager.MakeObj("bulletPlayerA");
                bulletRR.transform.position = transform.position + Vector3.right * 0.3f;

                GameObject bulletCC = objectManager.MakeObj("bulletPlayerB");
                bulletCC.transform.position = transform.position;

                GameObject bulletLL = objectManager.MakeObj("bulletPlayerA");
                bulletLL.transform.position = transform.position + Vector3.left * 0.3f;

                Rigidbody2D rigidRR = bulletRR.GetComponent<Rigidbody2D>();
                Rigidbody2D rigidCC = bulletCC.GetComponent<Rigidbody2D>();
                Rigidbody2D rigidLL = bulletLL.GetComponent<Rigidbody2D>();
                rigidRR.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                rigidCC.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                rigidLL.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                break;

        }
        curShotDelay = 0;
    }

    void Reload()
    {
        curShotDelay += Time.deltaTime;
    }

    void Boom()
    {
        //if (!Input.GetButton("Fire2"))
        //    return;
        
        if (!isButtonB)
            return;

        if (isBoomTime)
            return;

        if (boom == 0)
            return;

        boom--;
        isBoomTime = true;
        gameManager.UpdateBoomIcon(boom);

        //1. Effect visible
        BoomEffect.SetActive(true);
        Invoke("OffBoomEffect", 4f);

        //2. remove enemy
        GameObject[] enemiesL = objectManager.GetPool("enemyL");
        GameObject[] enemiesM = objectManager.GetPool("enemyM");
        GameObject[] enemiesS = objectManager.GetPool("enemyS");

        for(int index = 0; index <enemiesL.Length; index++)
        {
            if (enemiesL[index].activeSelf)
            {
                Enemy enemyLogic = enemiesL[index].GetComponent<Enemy>();
                enemyLogic.OnHit(100);
            }
        }
        for (int index = 0; index < enemiesM.Length; index++)
        {
            if (enemiesM[index].activeSelf)
            {
                Enemy enemyLogic = enemiesM[index].GetComponent<Enemy>();
                enemyLogic.OnHit(100);
            }
        }
        for (int index = 0; index < enemiesS.Length; index++)
        {
            if (enemiesS[index].activeSelf)
            {
                Enemy enemyLogic = enemiesS[index].GetComponent<Enemy>();
                enemyLogic.OnHit(100);
            }
        }

        //3. remove enemy bullet
        GameObject[] bulletsA = objectManager.GetPool("enemyBulletA");
        GameObject[] bulletsB = objectManager.GetPool("enemyBulletB");

        for (int index = 0; index < bulletsA.Length; index++)
        {
            if (bulletsA[index].activeSelf)
                bulletsA[index].SetActive(false);
        }
        for (int index = 0; index < bulletsB.Length; index++)
        {
            if (bulletsB[index].activeSelf)
                bulletsB[index].SetActive(false);
        }

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Border")
        {
            switch(collision.gameObject.name)
            {
                case "Top":
                    isTouchTop = true; 
                    break;
                case "Bottom":
                    isTouchBottom = true; 
                    break;
                case "Right":
                    isTouchRight = true;
                    break;
                case "Left":
                    isTouchLeft = true;
                    break;
            }
        } else if (collision.gameObject.tag == "Enemy" || collision.gameObject.tag == "EnemyBullet")
        {
            if(isRespawnTime)
                return;

            if (isHit)
                return;

            isHit = true;
            life--;
            gameManager.UpdateLifeIcon(life);
            gameManager.CallExplosion(transform.position, "P");

            if(life == 0)
            {
                gameManager.GameOver();
            } else
            {
                gameManager.RespawnPlayer();
            }

            gameObject.SetActive(false);
            collision.gameObject.SetActive(false);
        } else if (collision.gameObject.tag == "Item")
        {
            Item item= collision.gameObject.GetComponent<Item>();
            switch(item.type)
            {
                case "Coin":
                    score += 1000;
                    break;
                case "Power":
                    if(maxPower == power)
                        score += 500;
                    else
                        power++;
                    break;
                case "Boom":
                    if (maxBoom == boom)
                        score += 500;
                    else
                        boom++;
                        gameManager.UpdateBoomIcon(boom);
                    break;
            }
            collision.gameObject.SetActive(false);
        }
    }

    void OffBoomEffect()
    {
        BoomEffect.SetActive(false);
        isBoomTime = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Border")
        {
            switch (collision.gameObject.name)
            {
                case "Top":
                    isTouchTop = false;
                    break;
                case "Bottom":
                    isTouchBottom = false;
                    break;
                case "Right":
                    isTouchRight = false;
                    break;
                case "Left":
                    isTouchLeft = false;
                    break;
            }
        }
    }

}
