using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    public float speed;
    public int startIndex;
    public int endIndex;

    float viewHeight;

    private void Awake()
    {
        viewHeight = Camera.main.orthographicSize * 2;
    }

    public Transform[] sprites;
    // Update is called once per frame
    void Update()
    {
        Move();
        Scrolling();
    }

    void Move()
    {
        Vector3 curPos = transform.position;
        Vector3 nextPos = Vector3.down * speed * Time.deltaTime;
        transform.position = curPos + nextPos;
    }

    void Scrolling()
    {
        //sprite reuse
        if (sprites[endIndex].position.y < 25*(-1))
        {
            //sprite reuse
            Vector3 backSpritePos = sprites[startIndex].localPosition;
            Vector3 frontSpritePos = sprites[endIndex].localPosition;

            sprites[endIndex].transform.localPosition = backSpritePos + Vector3.up * 25;

            //cursor indexs change
            int startIndexSave = startIndex;
            startIndex = endIndex;
            endIndex = (startIndexSave-1 == -1) ? sprites.Length-1 : startIndexSave -1;
        }
    }
}
