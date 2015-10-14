using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PookaController : MonoBehaviour {

    Vector3 diggerPosition;
    Vector3 exitLocation;

    Vector2[] possibleDirections = { Vector2.left, Vector2.right, Vector2.up, Vector2.down };

    Vector2 lastDirection;

    BoxCollider2D selfCollider;
    Rigidbody2D rb;
    Animator anim;


    public float speed;
    float doubleSpeed;

    float timeCheck;
    int inflationLvl;
    bool ghostMode;
    float ghostTimer;
    Vector3 ghostDestination;

    LayerMask ignoreLayer, defaultLayer;

    GameObject fire;

    int enemyType;

    float atkTimer;
    float exitTimer;

    int alliesRemaining;

    bool waypointReached;
    Vector3 wayPoint;

    List<GameObject> lastBlocksTouched;    

    // Use this for initialization
    void Start()
    {
        diggerPosition = Vector3.zero;
        wayPoint = new Vector3(-0.65f, 0.64f, 0);
        exitLocation = new Vector3(-4.29f, 3.64f, 0f);
        lastDirection = Vector2.left;
        selfCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        fire = GameObject.Find("Fire");

        waypointReached = false;

        inflationLvl = 0;
        ghostMode = false;
        ghostTimer = 0.0f;
        atkTimer = 0.0f;
        exitTimer = 0;

        if (gameObject.name == "Frygar")
        {
            enemyType = 2;
        }
        else
        {
            enemyType = 1;
        }

        alliesRemaining = 2;
        int currentLevel = PlayerPrefs.GetInt("level");
        if(currentLevel == 2)
        {
            alliesRemaining = 4;
        }


        speed = PlayerPrefs.GetFloat("enemySpeed");

        if (speed == default(int))
        {
            speed = 0.012f;
        }

        Debug.Log(speed);
        doubleSpeed = 2 * speed;
        timeCheck = 0.0f;

        defaultLayer = LayerMask.NameToLayer("Default");
        ignoreLayer = LayerMask.NameToLayer("Inflated");
        Physics.IgnoreLayerCollision(ignoreLayer, defaultLayer, true);

        NameBlocks();
        lastBlocksTouched = new List<GameObject>();      
    }

   


    // Update is called once per frame
    void FixedUpdate()
    {
        if (inflationLvl == 0)
        {
            SetCollisionWithPlayer(true);
            if (!ghostMode)
            {
                ghostMode = GoGhost();
            }

            //Debug.Log("Gostmode: " + ghostMode.ToString() + "Target: " + ghostDestination.ToString());
            MoveEnemy();
        }
        else
        {
            SetCollisionWithPlayer(false);
            timeCheck += Time.deltaTime;
            if (inflationLvl > 3)
            {
                InitiateDeath(1);
            }
            else
            {
                if (timeCheck > 1.0f)
                {
                    inflationLvl--;
                    timeCheck = 0;
                }

            }
        }

        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        anim.SetInteger("inflation", inflationLvl);
        anim.SetBool("ghost", ghostMode);
    }

    void RandomAttack(Vector2 direct)
    {
        if (enemyType == 2)
        {
            if (atkTimer > 5)
            {
                atkTimer = 0;

                int randomNum1 = Mathf.CeilToInt(Random.Range(0.1f, 0.9f) * 99);
                int randomNum2 = Mathf.CeilToInt(Random.Range(0.1f, 0.9f) * 99);

                if (Mathf.CeilToInt(atkTimer * Mathf.Sqrt(randomNum2)) % randomNum1 == 0)
                {
                    DrawFire(direct);

                    Vector2 StartPosition = transform.position;
                    Vector2 EndPosition = StartPosition + direct;

                    Vector3 endLineDraw = transform.position;
                    endLineDraw.x += direct.x;
                    endLineDraw.y += direct.y;

                    RaycastHit2D hit = Physics2D.Linecast(StartPosition, EndPosition);
                    Debug.DrawLine(transform.position, endLineDraw, Color.red, 2, false);
                    //Debug.Break();

                    if (hit.collider != null && hit.collider.gameObject.tag == "Player")
                    {
                        hit.collider.gameObject.SendMessage("Attacked");                        
                    }
                }
            }
            else
            {
                atkTimer += Time.deltaTime;
                fire.transform.position = new Vector3(-100, -100, 0);
            }


        }
    }



    void DrawFire(Vector2 direct)
    {        
        Vector3 offset;
        if (direct.x > 0)
        {
            fire.transform.localScale = new Vector3(3.5f, 3.5f, 0);
            offset = Vector3.right * 0.5f;
            fire.transform.position = transform.position + offset;
        }
        else if (direct.x < 0)
        {
            fire.transform.localScale = new Vector3(-3.5f, 3.5f, 0);
            offset = Vector3.left * 0.5f;
            fire.transform.position = transform.position + offset;
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
            speed = doubleSpeed;
            if(exitTimer < 0 && false)
            {
                exitTimer += Time.deltaTime;
                targetLocation = diggerPosition;
            }
            else
            {
                Escape();

                if (waypointReached)
                {
                    targetLocation = exitLocation;
                }
                else
                {
                    targetLocation = wayPoint;
                }
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

            if (!DirectionClear(lastDirection))
            {
                dir = FindDirectionToMove(targetLocation);
            }
            else if (openPaths > 2)
            {
                dir = FindDirectionToMove(targetLocation);                
            }

            openPaths = 0;


            lastDirection = dir;
            PookaPos.x += dir.x * speed;
            PookaPos.y += dir.y * speed;
            transform.position = PookaPos;     

            if(dir.x != 0)
            {
                RandomAttack(dir);

                Vector3 currentScale = transform.localScale;

                if ((currentScale.x < 0 && dir.x < 0)
                    || (currentScale.x > 0 && dir.x > 0))
                {
                    currentScale.x *= -1;
                }
                transform.localScale = currentScale;
            }


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
            if (randomNum2 % Mathf.CeilToInt(Mathf.Sqrt(randomNum1)+randomNum1) == 0)
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

        int coinFlip = 0;

        if (dir1Clear && dir1Clear)
        {
            coinFlip = Random.Range(0, 1);
            if (coinFlip == 0)
            {
                ret = dir1;
            }
            else
            {
                ret = dir2;
            }
        }
        else if (x < y && dir1Clear)
        {
            ret = dir1;

        }
        else if (y < x && dir2Clear)
        {
            ret = dir2;
        }

        else if (dir2Clear && !dir1Clear)
        {
            ret = dir2;
        }
        else if (dir1Clear && !dir2Clear)
        {
            ret = dir1;
        }        
        else
        {            
            dir1 = -1 * dir1;
            dir2 = -1 * dir2;
            dir1Clear = DirectionClear(dir1);
            dir2Clear = DirectionClear(dir2);

            if (dir1Clear && dir1Clear)
            {
                coinFlip = Random.Range(0, 1);
                if (coinFlip == 0)
                {
                    ret = dir1;
                }
                else
                {
                    ret = dir2;
                }
            }
            else
            {
                int loopCount = 0;
                while (true)
                {
                    loopCount++;
                    coinFlip = Random.Range(0, 99);
                    var direct = possibleDirections[coinFlip%possibleDirections.Length];
                    if (DirectionClear(direct))
                    {
                        ret = direct;
                        break;
                    }else if (loopCount > 15)
                    {
                        if(dir1Clear && !dir2Clear)
                        {
                            ret = dir1;
                        }
                        else
                        {
                            ret = dir2;
                        }
                        break;
                    }
                }
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

        for(int i=0; i < 3; i++)
        {
            increment = 0;
            switch(i)
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

            if(DirectionIsHorizontal(direction))
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
        if(direction.x != 0)
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

    void IncreaseInflation()
    {
        inflationLvl++;
    }

    void HitByRock()
    {
        InitiateDeath(2);       
    }

    void InitiateDeath(int modifier)
    {
        //Award Points
        float depth;
        if(transform.position.y > 0)
        {
            depth = transform.position.y;
        }else{
            depth = Mathf.Abs(transform.position.y)*2;
        }

        NotifyOfDeath((int)(enemyType * modifier * depth * 500));

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

    void Escape()
    {
        Vector3 targetLocation = (waypointReached) ? (exitLocation) : wayPoint;

        float distanceX = Mathf.Abs(targetLocation.x - transform.position.x);
        float distanceY = Mathf.Abs(targetLocation.y - transform.position.y);

        float boxSize = GetComponent<BoxCollider2D>().size.x;
        float scale = transform.localScale.x;
        float dist = 1.1f*boxSize * scale;

        if (distanceY <= dist && distanceX <= dist)
        {   if (waypointReached)
            {
                GameObject.FindGameObjectWithTag("Player").SendMessage("Escape");
            }
            else
            {
                lastDirection = Vector2.up;
                waypointReached = true;
            }
        }
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


    void SetCollisionWithPlayer(bool enabled)
    {           
        if(enabled)
        {
            gameObject.tag = "Enemy";
            gameObject.layer = defaultLayer;  
        }
        else
        {
            gameObject.tag = "Inflated";
            gameObject.layer = ignoreLayer; 
        }
    }

    void NameBlocks()
    {
        var blocks = GameObject.FindGameObjectsWithTag("Block");

        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].name = ("block" + i.ToString());
        }
    }
}
