using UnityEngine;
using System.Collections;

public class RockController : MonoBehaviour {

    bool stayPut = true;   
    GameObject blockBeneath;
    public float fallSpeed = 0.05f;

    Vector3 ogScale;

    bool jittering;
    float jitterTime;

	// Use this for initialization
	void Start () {
        jittering = false;
        jitterTime = Random.Range(2.1f, 5.1f);
        ogScale = transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
        if(stayPut)
        {
            stayPut = CheckBlockUnder();
        }       
        else
        {
            if(!Jitter())
            {
                Vector3 rockPosition = transform.position;
                rockPosition.y -= fallSpeed;
                transform.position = rockPosition;
            }
        }        
	}

    bool Jitter()
    {
        if(jitterTime > 0)
        {
            jitterTime -= Time.deltaTime;
            
            var curScale = transform.localScale;
            curScale *= Random.Range(0.90f, 1.10f);
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
        Vector2 StartPosition = transform.position;
        Vector2 EndPosition = StartPosition;
        EndPosition.y -= 0.3f;

        RaycastHit2D hit = Physics2D.Linecast(StartPosition, EndPosition);
        Debug.DrawLine(StartPosition, EndPosition, Color.red, 2, false);

        if (hit.collider != null && hit.collider.gameObject.tag == "Block")
        {                        
            blockBeneath = hit.collider.gameObject;
            return true;       
        }
        return false;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!stayPut)
        {
            if(other.gameObject.tag == "Block" || other.gameObject.tag == "Wall")
            {
                Destroy(gameObject);
            }
            else if (other.gameObject.tag != "Player")
            {
                other.gameObject.SendMessage("HitByRock");
            }            
        }
    }
}
