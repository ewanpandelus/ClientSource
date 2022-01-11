using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpSpeed = 5f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private RotateTowardsMouse playerLookRotation;
    [SerializeField] private GameObject firePoint;
    [SerializeField] private Color lightBlue;
    private Material playerMat;
    private bool scaled = true;
    private bool coloured = true;
    private bool canShoot = true;
    private bool connected = false;
    private Rigidbody rb;
    private Vector3 movement;
    float BONUS_GRAV = 25f;
    bool inAir = true;
    private float elapsedTime = 0;
    private float previousTimeCheck = 0;
    private float previousTimeCheck2 = 0;
    private float timeSinceLastPacket = 0;
    private int hitCount = 0;
    private bool dead = false;
    private Vector3 initialPosition;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerMat = transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material;
    }
    private void ResetPosition()
    {
        float rand = Random.Range(-3, 3);
        transform.position = new Vector3(rand, 3, 0);
        rb.velocity = Vector3.zero;
    }
    private void Update()
    {
        if (!connected)
        {
            return;
        }
        elapsedTime += Time.deltaTime;
        timeSinceLastPacket += Time.deltaTime;
        SendPositionWithVariableInterval(0.1f);
        SendRotationWithVariableInterval(0.15f);
        EvaluateInput();
        CheckIfReceived();
       
    }
    private void CheckIfReceived()
    {
        if (timeSinceLastPacket > 10)
        {
            ClientSend.Disconnect();
        }
    }
    private void EvaluateInput()
    {
        movement = MovementInput();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Jump());
        }
        if (Input.GetMouseButtonDown(0) && canShoot && scaled)
        {
            StartCoroutine(Shoot());
        }
    }
    private void SendPositionWithVariableInterval(float interval)
    {
        if(elapsedTime - previousTimeCheck > interval)
        {
            previousTimeCheck = elapsedTime;
            ClientSend.PlayerPosition(elapsedTime);
        }
    }
    private void SendRotationWithVariableInterval(float interval)
    {
        if (elapsedTime - previousTimeCheck2 > interval)
        {
            previousTimeCheck2 = elapsedTime;
            ClientSend.PlayerRotation(playerLookRotation.GetRotation(), playerLookRotation.GetLeftOf());
        }
    }

    private Vector3 MovementInput()
    {
        return transform.right * Input.GetAxisRaw("Horizontal");
    }
    private void FixedUpdate()
    {
        if (!connected)
        {
            return;
        }
        if (inAir)
        {
           IncreaseGravityScale();
        }
        rb.velocity = new Vector2(movement.x * speed, rb.velocity.y);
        if (transform.position.y < -10) ResetPosition();
    }
   
    private void IncreaseGravityScale()
    {
        Vector3 vel = rb.velocity;
        vel.y -= BONUS_GRAV * Time.deltaTime * 3.5f;
        rb.velocity = vel;
    }

    private IEnumerator Jump()
    {
        if (!inAir)
        {
            ClientSend.Jump();
            StartCoroutine(ChangeColour(lightBlue, 0.1f));
            yield return new WaitUntil(() => coloured == true);
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            StartCoroutine(ChangeColour(Color.white, 0.25f));
        }
    }

   
    private IEnumerator Shoot()
    {
        if (dead) yield break;
        Vector3 currentScale = transform.localScale;
        canShoot = false;
        StartCoroutine(ScaleAnimation(0.25f, transform.localScale, currentScale*1.25f,true));
        ClientSend.Shoot();
        yield return new WaitUntil(() => scaled == true);
        InstantiateBullet();
        StartCoroutine(ScaleAnimation(0.25f, transform.localScale, currentScale,true));
        yield return new WaitUntil(() => scaled == true);
        canShoot = true;
    }
    private void InstantiateBullet()
    {
        Vector3 direction = playerLookRotation.GetShotDirection();
        direction.z = 0;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.transform);
        bullet.transform.position = new Vector3(bullet.transform.position.x, bullet.transform.position.y, 0);
        Rigidbody bulletBody = bullet.GetComponent<Rigidbody>();
        bullet.transform.parent = null;
        bullet.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        bulletBody.velocity = direction.normalized * 35;
        Destroy(bullet, 10f);
    }
    private void OnCollisionStay(Collision collision)
    {
        if(collision.transform.CompareTag("floor"))
        {
            inAir = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        inAir = true;
    }
    IEnumerator ScaleAnimation(float time, Vector3 fromScale, Vector3 toScale, bool isShooting)
    {
        float i = 0;
        float rate = 1 / time;
        scaled = false;
        float timeElapsed = 0;

        while (i < 1)
        {
            i += Time.deltaTime * rate;
            timeElapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(fromScale, toScale, i);
            yield return 0;
        }
        if (isShooting)
        {
            scaled = true;
        }
    }
    private IEnumerator ChangeColour(Color endColour, float time)
    {
        float i = 0;
        float rate = 1 / time;
        coloured = false;
        Color startColor = playerMat.color;
        while (i < 1)
        {
            i += Time.deltaTime * rate;
            playerMat.color = Color.Lerp(startColor, endColour, i);
            yield return 0;
        }
        coloured = true;
    }
    public void CollisionScale()
    {
        hitCount++;
        if (hitCount == 3)
        {
            StartCoroutine(Die());
       
        }
        else if(hitCount<3)
        {
            StartCoroutine(ScaleAnimation(1f, transform.localScale, transform.localScale * 1.25f, true));
            StartCoroutine(Hurt());
        }
    }
    private IEnumerator Die()
    {
 
        StartCoroutine(ScaleAnimation(3f, transform.localScale, transform.localScale * 2, true));
        StartCoroutine(ChangeColour(Color.red, 3f));
        yield return new WaitUntil(() => coloured == true);
        transform.localScale = new Vector3(0, 0, 0);
        GetComponent<Explosion>().Explode(transform.position , false);
        dead = true;
    }
    private IEnumerator Hurt()
    {
        StartCoroutine(ChangeColour(Color.red, 0.2f));
        yield return new WaitUntil(() => coloured == true);
        StartCoroutine(ChangeColour(Color.white, 0.2f));
    }
    public void SetConnected(bool _connected)
    {
        connected = _connected;
    }
    public void ReturnToInitialPosition()
    {
        transform.position = initialPosition;
    }
    public void SetTimeSinceLastPacket(float time)
    {
        timeSinceLastPacket = time;
    }
}
