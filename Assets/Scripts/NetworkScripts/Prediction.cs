using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prediction : MonoBehaviour
{
 
    private Vector3 position0;
    private Vector3 position1;
    private int count = 0;
    private float time0 = int.MinValue;
    private float time1 = int.MinValue;
    private Vector3 predictedVelocityX = new Vector3(0, 0, 0);


    float BONUS_GRAV = 25f;
    private bool scaled = true;
    private bool coloured = true;
    private Rigidbody rb;
    private float stunnedTime = 0;
    private float jumpSpeed = 35f;
    [SerializeField] private GameObject firePoint;
    private bool stunned = false;
    private Material playerMat;
    private GameObject bulletPrefab;
    private Vector3 impactDirection;
    [SerializeField] private Color lightBlue;
    private bool connected = false;
    private int hitCount = 0;
    public void Initialise()
    {
        rb = GetComponent<Rigidbody>();
        playerMat = transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material;
        firePoint = transform.GetChild(1).transform.GetChild(0).transform.GetChild(1).transform.GetChild(0).gameObject;
        connected = true;
    }
    public void Update()
    {
        if (!connected)
        {
            return;
        }
        
        if (predictedVelocityX != new Vector3(0, 0, 0))
        {
            if (!float.IsNaN(predictedVelocityX.x) && !float.IsInfinity(predictedVelocityX.x))
            {
                transform.position += predictedVelocityX * Time.deltaTime;

            }
        }
    }
    private void FixedUpdate()
    {
        if (!connected)
        {
            return;
        }
        IncreaseGravityScale();
    
        rb.velocity = new Vector2(0, rb.velocity.y);
       
      

    }
    private void IncreaseGravityScale()
    {
        Vector3 vel = rb.velocity;
        vel.y -= BONUS_GRAV * Time.deltaTime * 3.5f;
        rb.velocity = vel;
    }
    private IEnumerator Shoot()
    {
        Vector3 currentScale = transform.localScale;
        StartCoroutine(ScaleAnimation(0.25f, transform.localScale, currentScale * 1.25f, true));
        yield return new WaitUntil(() => scaled == true);
        InstantiateBullet();
        StartCoroutine(ScaleAnimation(0.25f, transform.localScale, currentScale, true));
    }
    private void InstantiateBullet()
    {
        Vector3 direction = GetShotDirection();
        direction.z = 0;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.transform);
        bullet.transform.position = new Vector3(bullet.transform.position.x, bullet.transform.position.y, 0);
        Rigidbody bulletBody = bullet.GetComponent<Rigidbody>();
        bullet.transform.parent = null;
        bullet.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        bulletBody.velocity = direction.normalized * 35;
        Destroy(bullet, 10f);
    }
    IEnumerator ScaleAnimation(float time, Vector3 fromScale, Vector3 toScale, bool isShooting)
    {
        float i = 0;
        float rate = 1 / time;
        scaled = false;

        while (i < 1)
        {
            i += Time.deltaTime * rate;
            transform.localScale = Vector3.Lerp(fromScale, toScale, i);
            yield return 0;
        }
        if (isShooting)
        {
            scaled = true;
        }
    }
    private IEnumerator Jump()
    {

        if (GameManager.ping < 0.1f)
        {
            StartCoroutine(ChangeColour(Color.red, 0.1f - GameManager.ping));
        }
        else
        {
            coloured = true;
        }

        yield return new WaitUntil(() => coloured == true);
        rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        StartCoroutine(ChangeColour(Color.white, 0.25f));
    }
    public void StartJump()
    {
        StartCoroutine(Jump());
    }
    public void StartShooting()
    {
        StartCoroutine(Shoot());
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
        stunned = true;
        StartCoroutine(ScaleAnimation(3f, transform.localScale, transform.localScale * 2, true));
        StartCoroutine(ChangeColour(lightBlue, 3f));
        yield return new WaitUntil(() => coloured == true);
        transform.localScale = new Vector3(0, 0, 0);
        GetComponent<Explosion>().Explode(transform.position, true);
    }
    private IEnumerator Hurt()
    {
        StartCoroutine(ChangeColour(lightBlue, 0.2f));
        yield return new WaitUntil(() => coloured == true);
        StartCoroutine(ChangeColour(Color.white, 0.2f));
    }
    public void SetRotation(float rotationX, bool leftOf)
    {
        if (stunned)
        {
            return;
        }
        if (!leftOf)
        {
            gameObject.transform.GetChild(1).transform.eulerAngles = new Vector3(rotationX, -90, 0);
        }
        else
        {
            gameObject.transform.GetChild(1).transform.eulerAngles = new Vector3(rotationX, 90, 0);
        }
    }
    public void SetPosition(Vector3 _position, float _elaspedTime)
    {
       
        if (count % 30 == 0 || Vector3.Distance(_position, transform.position)>5)
        {
            transform.position = _position;
        }
        Vector3 clientPosition = _position;
        AssignPreviousPositionsAndTimes(clientPosition, _elaspedTime);
        predictedVelocityX = CalculateVelocityX();
    }
  
    private void AssignPreviousPositionsAndTimes(Vector3 _position, float _elapsedTime)
    {
        if (count < 1)
        {
            position0 = _position;
            time0 = _elapsedTime;
        }
        else if (count == 1)
        {
            position1 = _position;
            time1 = _elapsedTime;
        }
        else
        {
            CascadeTimes(_position, _elapsedTime);
        }
        count++;
    }
    private void CascadeTimes(Vector3 _position, float _elapsedTime)
    {
        position0 = position1;
        time0 = time1;

        position1 = _position;
        time1 = _elapsedTime;

    }
    private Vector3 CalculateVelocityX()
    {
        if (time0 != int.MinValue && time1 != int.MinValue)
        {
            float displacementX = position1.x - position0.x;
            float timeElapsed = time1 - time0;
            float xVelocity = displacementX / timeElapsed;
            return new Vector3(xVelocity, 0, 0);
        }
        else return new Vector3(0, 0, 0);
    }
 
    public Vector3 GetShotDirection()
    {
        Vector3 firePointPos = firePoint.transform.position;
        firePointPos += firePoint.transform.forward * 2;
        return firePointPos - transform.position;
    }
    public void SetBulletPrefab(GameObject _bullet)
    {
        bulletPrefab = _bullet;
    }
    public void SetStunned(bool _stunned)
    {
        stunned = _stunned;
    }
    public void SetImpactDirection(Vector3 _impactDirection)
    {
        impactDirection = _impactDirection;
    }




}
