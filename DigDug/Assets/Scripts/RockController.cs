using UnityEngine;
using System.Collections;

public class RockController : MonoBehaviour {

    bool stayPut = true;   
    GameObject blockBeneath;
    public float fallSpeed = 0.05f;

    Vector3 ogScale;

    bool jittering;
    float jitterTime;

    float fallTime;

    Vector3 startPosition;

	// Use this for initialization
	void Start () {
        jittering = false;
        jitterTime = Random.Range(0.3f, 1.3f);
        fallTime = Random.Range(0.5f, 1.0f);
        ogScale = transform.localScale;
        startPosition = transform.position;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        if(stayPut)
        {
            stayPut = CheckBlockUnder();
        }       
        else
        {
            if(!Jitter())
            {
                if (fallTime > 0)
                {
                    Vector3 rockPosition = transform.position;
                    rockPosition.y -= fallSpeed;
                    transform.position = rockPosition;
                    fallTime -= Time.deltaTime;
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }        
	}

    bool Jitter()
    {
        if(jitterTime > 0)
        {
            jitterTime -= Time.deltaTime;
            
            var curScale = transform.localScale;
            curScale *= Random.Range(0.95f, 1.05f);
            transform.localScale = curScale;

            return true;
        }
        else
        {
            transform.localScale = ogScale;
            return false;
        }
    }

    bool CheckBlockUnder()
    {               
        Vector2 EndPosition = startPosition;
        EndPosition.y -= 0.3f;

        RaycastHit2D hit = Physics2D.Linecast(startPosition, EndPosition);
        Debug.DrawLine(startPosition, EndPosition, Color.red, 2, false);

        if (hit.collider != null && hit.collider.gameObject.tag == "Block")
        {                        
            blockBeneath = hit.collider.gameObject;
            return true;       
        }
        return false;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        /*
        if (!stayPut)
        {
            if (!jittering)
            { 
                if (other.gameObject.tag == "Block" || other.gameObject.tag == "Wall")
                {                    
                    Destroy(gameObject);
                }
                else if (other.gameObject.tag != "Player")
                {
                    other.gameObject.SendMessage("HitByRock");
                }
            }        
        }
        */
    }
    
}
