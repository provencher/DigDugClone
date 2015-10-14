using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DiggerController : MonoBehaviour {
    
    enum Direction { left, right, up, down}

    const float animationTimeCooldown = 0.65f;
    bool isAttacking;
    float attackCooldown;

    Vector2 direction = Vector2.right;
    float rayDistance = 0.4f;

    Animator dAnimator;
    Rigidbody2D rb;

    private bool isDigging = false;
    GameObject lastObjectTouched;
    GameObject rayCastedObject;
    GameObject lasthose;

    Vector3 farAway;

    //Game State
    public int totalPoints, livesRemaining, currentLevel;
    GameObject score, lives;
    int enemiesRemaining;

    // Animator Booleans
    bool IsWalking;
    bool IsWalkingUp;
    bool IsWalkingDown;

    bool IsDiggingHorizontally;
    bool IsDiggingUp;
    bool IsDiggingDown;

    float moveSpeed = 0.02f;

    bool startSequence;
    int blocksBroken;
    int startBlocksTouched;

    Vector3 PlayerPos;
    Vector2 lastDirection;

	// Use this for initialization
	void Start () {
        transform.position = new Vector3(-0.65f, 0.64f, 0);
        farAway = new Vector3(-1000, -1000, 0);
        lastDirection = Vector2.right;
        dAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        IsWalking = false;
        IsWalkingDown = false;
        IsWalkingUp = false;

        IsDiggingUp = false;
        IsDiggingDown = false;
        IsDiggingHorizontally = false;        

        startSequence = true;
        blocksBroken = 0;              

        startBlocksTouched = 0;    

        totalPoints = PlayerPrefs.GetInt("score");
        livesRemaining = PlayerPrefs.GetInt("lives");
        enemiesRemaining = PlayerPrefs.GetInt("enemies");
        currentLevel = PlayerPrefs.GetInt("level");
        lasthose = GameObject.Find("HoseRight");

        /*
        if (totalPoints == default(int) || livesRemaining == default(int) || enemiesRemaining == default(int))
        {
            totalPoints = 0;
            livesRemaining = 2;
            enemiesRemaining = 3;
            currentLevel = 1;
        }
        */


        score = GameObject.Find("Score");
        lives = GameObject.Find("Lives");

        var ingnoreLayer = LayerMask.NameToLayer("Inflated");
        var defaultLayer = LayerMask.NameToLayer("Default");
        Physics.IgnoreLayerCollision(ingnoreLayer, defaultLayer, true);

        if (livesRemaining == 0)
        {
            Application.LoadLevel(0);
        }   
	}

    // Update is called once per frame
	void FixedUpdate () {

        InputHandling();
        CheckBlockInDirection(direction);
        CheckEnemiesInAttackDirection(lastDirection);
        
        UpdatePlayerPosition();
        NotifyEnemiesOfPosition();

        UpdateText();

        UpdateAnimatorData();
        DestroyBlockRay();
        WinLevel();
	}

    void DrawHose()
    {
        if (isAttacking)
        {
            Vector3 offset;
            if (lastDirection.x > 0)
            {
                lasthose = GameObject.Find("HoseRight");
                offset = Vector3.right * 0.5f;
                lasthose.transform.position = transform.position + offset;
            }
            else if (lastDirection.x < 0)
            {
                lasthose = GameObject.Find("HoseLeft");
                offset = Vector3.left * 0.5f;
                lasthose.transform.position = transform.position + offset;

            }
            else if (lastDirection.y > 0)
            {
                lasthose = GameObject.Find("HoseUp");
                offset = Vector3.up * 0.5f;
                lasthose.transform.position = transform.position + offset;
            }
            else if (lastDirection.y < 0)
            {
                lasthose = GameObject.Find("HoseDown");
                offset = Vector3.down * 0.5f;
                lasthose.transform.position = transform.position + offset;
            }
        }
    }


    void CheckEnemiesInAttackDirection(Vector2 dir)
    {
        if(isAttacking && attackCooldown == 0)
        {
            DrawHose();
            attackCooldown = 0.4f;
            isAttacking = false;

            dir *= 1.5f;

            Vector2 StartPosition = transform.position;
            Vector2 EndPosition = StartPosition + dir;

            Vector3 endLineDraw = transform.position;
            endLineDraw.x += dir.x;
            endLineDraw.y += dir.y;

            RaycastHit2D hit = Physics2D.Linecast(StartPosition, EndPosition);            
            Debug.DrawLine(transform.position, endLineDraw, Color.red, 2, false);
            //Debug.Break();
            
            if (hit.collider != null && 
                (hit.collider.gameObject.tag == "Enemy") 
                || (hit.collider.gameObject.tag == "Inflated"))
            {
                hit.collider.gameObject.SendMessage("IncreaseInflation");
            }
        }
        else if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }
        else if (attackCooldown < 0)
        {
            attackCooldown = 0;
            lasthose.transform.position = farAway;
        }
    }

    void Attacked()
    {
        LoseLife();
    }

    void WinLevel()
    {
    if(enemiesRemaining == 0)
        {
            if(currentLevel == 1)
            {                    
                    PlayerPrefs.SetInt("score", totalPoints);
                    PlayerPrefs.SetInt("lives", livesRemaining);
                    PlayerPrefs.SetInt("level", ++currentLevel);
                    Application.LoadLevel(2);
            }
            else
            {
                Application.LoadLevel(0);
            }
        }        
    }

    /*
     Check diagonal block to see if walking is possible
     */
    void InputHandling()
    {
        direction = new Vector2(0, 0);      
        

        if (Input.GetKey("right"))
        {                    
            direction = Vector2.right;
            lastDirection = direction;
        }
        else if (Input.GetKey("left"))
        {            
            direction = Vector2.left;
            lastDirection = direction;
        }
        else if (Input.GetKey("down"))
        {            
            direction = Vector2.down;
            lastDirection = direction;
        }
        else if (Input.GetKey("up"))
        {            
            direction = Vector2.up;
            lastDirection = direction;
        }
        else
        {
            direction = Vector2.zero;
        }

        if(Input.GetKey("z") && attackCooldown == 0)
        {
            isAttacking = true;            
        }     


        if (Input.GetKey("space"))
        {
            isDigging = true;
        }
        else
        {
            isDigging = false;
        }


        //Add attack key
    }

    void UpdatePlayerPosition()
    {
        if(DirectionClear(direction))
        {
            PlayerPos = transform.position;
            PlayerPos.x += direction.x * moveSpeed;
            PlayerPos.y += direction.y * moveSpeed;
            transform.position = PlayerPos; 
        }
    }

    void NotifyEnemiesOfPosition()
    {
        GameObject[] enemyArr = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemyArr)
        {
            enemy.SendMessage("DiggerPosition", transform.position);
        }
    }


    void CheckBlockInDirection(Vector2 dir)
    {
        dir *= rayDistance;

        Vector2 StartPosition = transform.position;
        Vector2 EndPosition = StartPosition + dir;

        RaycastHit2D hit = Physics2D.Linecast(StartPosition, EndPosition);
        Debug.DrawLine(StartPosition, EndPosition, Color.red, 2, false);

        if(hit.collider != null && hit.collider.gameObject.tag == "Block" && isDigging)
        {
            //Debug.Log("CheckBlockInDirection->HIT");
            rayCastedObject = hit.collider.gameObject;                                       
        }
        
    }

    bool DirectionClear(Vector2 dir)
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
                        increment = boxSize * scale / 2.2f;
                        break;
                    }
                case 2:
                    {
                        increment = (-1) * boxSize * scale / 2.2f;
                        break;
                    }
                default:
                    break;
            }

            StartPosition = transform.position;

            if (DirectionIsHorizontal(dir))
            {
                StartPosition.y += increment;
            }
            else
            {
                StartPosition.x += increment;
            }

            EndPosition = StartPosition + dir * 0.38f;

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

    bool DirectionIsHorizontal(Vector2 dir)
    {
        if (dir.x != 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void DestroyBlockRay()
    {        
        if (rayCastedObject != null)
        {
            Destroy(rayCastedObject);
            rayCastedObject = null;
            isDigging = false;
            blocksBroken++;          
        }
    }


    void DestroyBlock()
    {
        if (isDigging && lastObjectTouched != null)
        {
            Destroy(lastObjectTouched);
            lastObjectTouched = null;
            isDigging = false;

            blocksBroken++;            
        }  
    }

    /*
     On arrow key, do ray check to see if block in the way, then move through, sticking to surface
     */    
    void OnCollisionEnter2D(Collision2D other)
    {       
        if (other.gameObject.tag == "Block")
        {
            lastObjectTouched = other.gameObject;            

            if (startSequence)
            {
                startBlocksTouched++;               
            }            
        }
        else if (other.gameObject.tag == "Enemy" || other.gameObject.tag == "Rock")
        {
            LoseLife();
        }
    }  
    

    void StartSequence()
    {
        if (startBlocksTouched != 5)
        {
            PlayerPos.y -= moveSpeed;

            if (blocksBroken < 4)
            {
                isDigging = true;
            }
            else
            {
                isDigging = false;
            }
        }
        else
        {
            startSequence = false;
            PlayerPos.y += moveSpeed;
        }
    }

    void KilledEnemy(int points)
    {
        enemiesRemaining--;
        totalPoints += points;
    }


    void UpdateAnimatorData()
    {  
        if (isDigging)
        {
            IsWalking = false;
            IsWalkingUp = false;
            IsWalkingDown = false;

            if (direction.y > 0)
            {               
                IsDiggingUp = true;
                IsDiggingHorizontally = false;
                IsDiggingDown = false;               
            }
            else if (direction.y < 0)
            {               
                IsDiggingDown = true;
                IsDiggingHorizontally = false;
                IsDiggingUp = false;
            }
            else if (direction.x != 0)
            {
                IsDiggingHorizontally = true;
                IsDiggingDown = false;
                IsDiggingUp = false;
            }
        }
        else
        {
            if (direction != Vector2.zero)
            {
                IsWalking = true;
                IsDiggingHorizontally = false;
                IsDiggingUp = false;
                IsDiggingDown = false;

                if (direction.y > 0)
                {
                    IsWalkingUp = true;
                    IsWalkingDown = false;
                }
                else if (direction.y < 0)
                {
                    IsWalkingDown = true;
                    IsWalkingUp = false;
                }
                else
                {
                    IsWalkingUp = false;
                    IsWalkingDown = false;
                }
            }
            else
            {
                IsDiggingHorizontally = false;
                IsDiggingUp = false;
                IsDiggingDown = false;

                IsWalking = false;
                IsWalkingUp = false;
                IsWalkingDown = false;
            }            

        }     


        Vector3 currentScale = transform.localScale;

        if ((currentScale.x > 0 && direction.x < 0)
            || (currentScale.x < 0 && direction.x > 0))
        {
            currentScale.x *= -1;
        }
        transform.localScale = currentScale;
     


        dAnimator.SetBool("IsWalking", IsWalking);
        dAnimator.SetBool("IsWalkingUp", IsWalkingUp);
        dAnimator.SetBool("IsWalkingDown", IsWalkingDown);

        dAnimator.SetBool("IsDiggingUp", IsDiggingUp);
        dAnimator.SetBool("IsDiggingDown", IsDiggingDown);
        dAnimator.SetBool("IsDiggingHorizontally", IsDiggingHorizontally);
    }


    void UpdateText()
    {
        score.GetComponent<Text>().text = "SCORE: " + totalPoints.ToString();
        lives.GetComponent<Text>().text = "LIVES: " + livesRemaining.ToString();
    }

    void Escape()
    {
        LoseLife();
    }

    void HitByRock()
    {
        LoseLife();
    }

    void LoseLife()
    {
        livesRemaining--;

        PlayerPrefs.SetInt("score", totalPoints);
        PlayerPrefs.SetInt("lives", livesRemaining);
        PlayerPrefs.SetInt("level", currentLevel);
        Application.LoadLevel(currentLevel);
    }


    
}
