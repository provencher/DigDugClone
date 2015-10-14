using UnityEngine;
using System.Collections;

public class FrygarController : MonoBehaviour {

    Vector3 diggerPosition;
    Vector3 exitLocation;

    Vector3[] possibleDirections = { Vector3.left, Vector3.right, Vector3.up, Vector3.down };

    Vector2 lastDirection;

    BoxCollider2D selfCollider;  

    public float speed;
    float timeCheck;
    int inflationLvl;
    bool ghostMode;
    float ghostTimer;
    Vector3 ghostDestination;

    int alliesRemaining;

    bool reachedStartLocation;

    // Use this for initialization
    void Start()
    {
        diggerPosition = Vector3.zero;
        exitLocation = new Vector3(-4.74f, 3.58f, 0f);
        lastDirection = Vector2.left;
        selfCollider = GetComponent<BoxCollider2D>();
     
        inflationLvl = 0;
        ghostMode = false;
        ghostTimer = 0.0f;

        reachedStartLocation = false;

        alliesRemaining = 2;

        speed = PlayerPrefs.GetFloat("enemySpeed");
        timeCheck = 0.0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (inflationLvl == 0)
        {
            if (!ghostMode)
            {
                ghostMode = GoGhost();
            }

            //Debug.Log("Gostmode: " + ghostMode.ToString() + "Target: " + ghostDestination.ToString());
            MoveEnemy();
        }
        else
        {
            if (timeCheck > 0.5)
            {
                inflationLvl--;
                timeCheck = 0;
            }
            timeCheck += Time.deltaTime;
        }

        
    }

    void MoveEnemy()
    {
        Vector3 PookaPos = transform.position;
        Vector3 targetLocation;
        Vector2 dir = Vector2.zero;

        if (alliesRemaining > 0)
        {
            targetLocation = diggerPosition;
        }
        else
        {
            if(!reachedStartLocation)
            {
                targetLocation = exitLocation;
            }
            else
            {
               
            }
            
        }

        if (!ghostMode)
        {
            dir = lastDirection;


            int openPaths = 0;
            foreach (Vector2 direct in possibleDirections)
            {
                if (DirectionClear(direct))
                {
                    openPaths++;
                }
            }

            if (!DirectionClear(lastDirection) || openPaths > 2)
            {
                //dir = FindDirectionToMove(targetLocation);
            }

            openPaths = 0;


            lastDirection = dir;
            PookaPos.x += dir.x * speed;
            PookaPos.y += dir.y * speed;
            transform.position = PookaPos;
        }
        else
        {
            float distanceX = Mathf.Abs(ghostDestination.x - transform.position.x);
            float distanceY = Mathf.Abs(ghostDestination.y - transform.position.y);

            if (Mathf.Max(distanceX, distanceY) > 0.05)
            {
                gameObject.GetComponent<Collider2D>().enabled = false;
                dir = new Vector2(ghostDestination.x - transform.position.x, ghostDestination.y - transform.position.y);

                dir = NormalizeDirection(dir);

                PookaPos.x += dir.x * speed;
                PookaPos.y += dir.y * speed;
                transform.position = PookaPos;
            }
            else
            {
                gameObject.GetComponent<Collider2D>().enabled = true;
                ghostMode = false;
                ghostTimer = 0;
            }
        }
    }

    bool GoGhost()
    {
        ghostTimer += Time.deltaTime;
        int randomNum1 = Mathf.CeilToInt(Random.Range(0.1f, 0.9f) * 99);
        int randomNum2 = Mathf.CeilToInt(Random.Range(0.1f, 0.9f) * 99);

        if (Mathf.CeilToInt(ghostTimer * Mathf.Sqrt(randomNum2)) % randomNum1 == 0)
        {
            if (randomNum2 % Mathf.CeilToInt(Mathf.Sqrt(randomNum1) + randomNum1) == 0)
            {
                ghostDestination.x = diggerPosition.x;
                ghostDestination.y = diggerPosition.y;
                ghostDestination.z = 0;
                return true;
            }
        }

        return false;
    }


    Vector2 FindDirectionToMove(Vector3 target)
    {
        // Vector difference from position to Digger
        Vector3 direction = target - transform.position;

        Vector2 ret = new Vector2();

        direction.x /= 2;
        direction.y /= 2;

        Vector2 dir1 = NormalizeDirection(new Vector2(direction.x, 0));
        Vector2 dir2 = NormalizeDirection(new Vector2(0, direction.y));

        bool dir1Clear = DirectionClear(dir1);
        bool dir2Clear = DirectionClear(dir2);

        float x = Mathf.Abs(direction.x);
        float y = Mathf.Abs(direction.y);

        if (x > y && dir1Clear)
        {
            ret = dir1;

        }
        else if (y > x && dir2Clear)
        {
            ret = dir2;
        }
        else
        {
            if (dir2Clear && !dir1Clear)
            {
                ret = dir2;
            }
            else if (dir1Clear && !dir2Clear)
            {
                ret = dir1;
            }
            else if (DirectionClear(-1 * dir1))
            {
                ret = -1 * dir1;
            }
            else
            {
                ret = -1 * dir2;
            }

        }

        return ret;
    }


    bool DirectionClear(Vector2 direction)
    {
        bool isClear = true;
        Vector2 StartPosition;
        Vector2 EndPosition;
        RaycastHit2D hit;
        float increment;
        float boxSize = GetComponent<BoxCollider2D>().size.x;
        float scale = transform.localScale.x;

        for (int i = 0; i < 3; i++)
        {
            increment = 0;
            switch (i)
            {
                case 1:
                    {
                        increment = boxSize * scale / 2;
                        break;
                    }
                case 2:
                    {
                        increment = (-1) * boxSize * scale / 2;
                        break;
                    }
                default:
                    break;
            }

            StartPosition = transform.position;

            if (DirectionIsHorizontal(direction))
            {
                StartPosition.y += increment;
            }
            else
            {
                StartPosition.x += increment;
            }

            EndPosition = StartPosition + direction * 0.38f;

            //Check if clear
            hit = Physics2D.Linecast(StartPosition, EndPosition);
            Debug.DrawLine(StartPosition, EndPosition, Color.red, 2, false);

            isClear = isClear && !(hit.collider != null &&
               (hit.collider.gameObject.tag == "Block" ||
               hit.collider.gameObject.tag == "Enemy" ||
               hit.collider.gameObject.tag == "Wall" ||
               hit.collider.gameObject.tag == "Rock"
               ));

        }

        return isClear;
    }

    bool DirectionIsHorizontal(Vector2 direction)
    {
        if (direction.x != 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void DiggerPosition(Vector3 dPos)
    {
        diggerPosition = dPos;
    }


    void HitByRock()
    {
        InitiateDeath();
    }

    void InitiateDeath()
    {
        //Award Points
        float depth;
        if (transform.position.y > 0)
        {
            depth = transform.position.y;
        }
        else
        {
            depth = Mathf.Abs(transform.position.y) * 2;
        }

        NotifyOfDeath((int)(depth * 500));

        //Destroy Enemy
        Destroy(gameObject);
    }

    void NotifyOfDeath(int points)
    {
        GameObject[] enemyArr = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemyArr)
        {
            enemy.SendMessage("AllyDead");
        }

        GameObject.FindGameObjectWithTag("Player").SendMessage("KilledEnemy", points);
    }

    void AllyDead()
    {
        alliesRemaining--;
    }


    Vector2 NormalizeDirection(Vector2 dir)
    {
        Vector2 ret = new Vector2();

        if (dir.x > 0)
        {
            ret.x = 1;
        }
        else if (dir.x < 0)
        {
            ret.x = -1;
        }
        else
        {
            ret.x = 0;
        }

        if (dir.y > 0)
        {
            ret.y = 1;
        }
        else if (dir.y < 0)
        {
            ret.y = -1;
        }
        else
        {
            ret.y = 0;
        }

        return ret;
    }
}
