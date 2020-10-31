using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float speed = 5.0f;
    public LayerMask _playArea;

    Vector3 direction;

    bool inProcess = false;


    // Start is called before the first frame update
    void Start()
    {
        //rb2D = gameObject.GetComponent<Rigidbody2D>();
        direction = transform.right;
    }


    // Update is called once per frame
    void Update()
    {
        if (!inProcess)
        {
            if (!Physics2D.OverlapCircle(transform.position + direction, 0.01f, _playArea))
            {
                StartCoroutine(Move(transform.position + direction));
            }
            else
            {
                //CheckPlayerLife(transform.position + direction);
                SwitchDirection(direction);
            }
        }
    }

    IEnumerator Move(Vector3 nextPosition)
    {
        inProcess = true;
        while (Vector3.Distance(transform.position, nextPosition) >= 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, nextPosition, (PlayerController._instance.PowerUp ? (speed / 10f) : speed) * Time.deltaTime);
            yield return null;
        }

        transform.position = nextPosition;

        inProcess = false;

    }

    void SwitchDirection(Vector3 prevDirection)
    {
        inProcess = true;

        int n = Random.Range(1, 100);

            if (n % 4 == 0)
            {
                direction = -transform.up;
            }
            else if (n % 4 == 1)
            {
                direction = transform.up;
            }

            else if (n % 4 == 2)
            {
                direction = -transform.right;
            }
            else
            {
                direction = transform.right;
            }

        if(prevDirection == direction)
        {
            SwitchDirection(direction);
        }
        else
        {
            inProcess = false;
        }
    }

    //public void CheckPlayerLife(Vector3 nextPosition)
    //{
    //    if (nextPosition == PlayerController._instance.movePoint.position)
    //    {
    //        PlayerController._instance.LifeUpdate();
    //    }
    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            PlayerController._instance.LifeUpdate();
        }
    }
}
