using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Pushable

{
    [SerializeField]
    private float wanderRadius;
    [SerializeField]
    private bool DEBUG_BOOL;
    Vector3 nextPosition;


    public NavMeshAgent agent;

    public bool isIdle;

	[SerializeField]
	private Animator enemyAnimator;

    [SerializeField]
    private static string SelectedTag = "Player";
    [SerializeField]
    private float endPushSpeed = 0.1f;

    [SerializeField]
    float radiusToTarget = 1.25f;

    [SerializeField]
    private int attackDamage = 1;

    [SerializeField] [Range(0,1)]
    private float hitChance = 0.5f;

    [SerializeField]
    private float attackRange = 2;

    [SerializeField]
    private float attackCD = 1;
    private float attackTime = 0;

    [SerializeField]
    private float pullRange = 5;
    [SerializeField]
    private float pullDuration = Mathf.Infinity;
    [SerializeField]
    private float pullCD = 3;
    [SerializeField]
    private float pullForce = 1;

    private float pullStart = 0;
    private bool isPulling = false;

	public bool isScared;

    [SerializeField]
    private GameObject bulletPrefab;

    [SerializeField]
    bool shoot = false;

    [SerializeField]
    float shootMaxAngle = 30;

    [SerializeField]
    Material material;

    [SerializeField]
    Transform gun;

    GameObject[] targets;
    GameObject closest;

    Rigidbody rb;

	

	//public int speed;
	
    private void Start()
    {
        nextPosition = transform.position;
		enemyAnimator = GetComponent<Animator>();

        rb = GetComponent<Rigidbody>();
        //isIdle = true;
    }

    void FixedUpdate()
    {
		if (isIdle) {
			EnemyWander();
		}
        //if (isScared)
        //{
        //    EnemyScared();
        //}
        //else if (IsPushActive())
        //{
        //    Vector3 horiz = rb.velocity;
        //    horiz.y = 0;

        //    //This function can be called before rb.AddForce() actually changes the velocity
        //    //hence the > 0 check
        //    bool slowedDown = horiz.sqrMagnitude > 0 && horiz.sqrMagnitude < endPushSpeed * endPushSpeed;
        //    if (slowedDown)
        //    {
        //        EndPush();
        //    }
        //}
        //else if (CanAttack())
        //{
        //    Attack();
        //}
        //else if (CanPull())
        //{
        //    Pull();
        //}
        //else
        //{
        //    ClosestTarget();
        //    MoveToClosest();
        //}
        //if (material)
        //{
        //    Color color = Time.time >= attackTime ? Color.red : Color.black;
        //    material.color = color;
        //}
        
    }

    private void Idle()
    {

		if (enemyAnimator) {
			enemyAnimator.SetBool("isMoving", false);
		}
		isIdle = true;
		//if (isIdle == true && agent.speed >= 0)
		//{
		//    agent.speed.Equals(0);
		//    agent.angularSpeed.Equals(0);
		//    agent.acceleration.Equals(0);
		//    agent.isStopped = true;
		//    agent.SetDestination(this.gameObject.transform.position);
		//    if (enemyAnimator)
		//    {
		//        enemyAnimator.SetBool("isIdle", true);
		//    }
		//    Invoke("IdleOff", 10);
		//    Debug.Log("Idle");
		//}
	}

    private void EnemyWander()
    {
		//if (isIdle == true && agent.speed != 0)
		//{
		//    agent.speed.Equals(3);
		//    agent.angularSpeed.Equals(120);
		//    agent.acceleration.Equals(8);
		//    isScared = false;
		//    agent.isStopped = false;
		//    nextPosition = RandomPointGenerator.PointGen(transform.position, wanderRadius);
		//    agent.SetDestination(nextPosition);

		//}

		isIdle = false;
        if (Vector3.Distance(nextPosition, transform.position) <= 1.5f)
        {
            nextPosition = RandomPointGenerator.PointGen(transform.position, wanderRadius);
            agent.SetDestination(nextPosition);
        }
        if (enemyAnimator)
        {
            enemyAnimator.SetBool("isMoving", true);
        }
    }

    public void IdleOn()
    {
        if (isIdle == true)
        {
            isIdle = false;
        }
    }

    public void IdleOff()
    {
        if (isIdle == false)
        {
            isIdle = true;
        }
    }
    GameObject ClosestTarget()
    {
        targets = GameObject.FindGameObjectsWithTag(SelectedTag);

        float distance = Mathf.Infinity;
        Vector3 pos = transform.position;

        foreach (GameObject target in targets)
        {
            Player player = target.GetComponent<Player>();
            if (!player)
                continue;

            Vector3 difference = target.transform.position - pos;
            float currentDistance = difference.sqrMagnitude;
            if (currentDistance < distance)
            {
                closest = target;
                distance = currentDistance;
            }
        }
        return closest;
	}

	bool IsPushActive()
    {
        return !agent.enabled;
    }

    void StartPush()
    {
        agent.enabled = false;
        rb.isKinematic = false;

        //interrupt pull, if active
        StopPull();
    }

    void EndPush()
    {
        agent.enabled = true;
        rb.isKinematic = true;
    }

    void MoveToClosest()
    {
        if (enemyAnimator)
        {
            enemyAnimator.SetBool("isMoving", true);
        }
		//radius round target
		Vector3 target = transform.position - closest.transform.position;
        target = target.normalized * radiusToTarget + closest.transform.position;

        //move to target radius
        agent.SetDestination(target);
        Debug.DrawLine(transform.position, target);

        //rotate towards target
        Vector3 forward = closest.transform.position - transform.position;
        float speed = agent.angularSpeed / 180f * Time.fixedDeltaTime;
        Quaternion look = Quaternion.LookRotation(forward);
        agent.transform.rotation = Quaternion.Slerp(transform.rotation, look, speed);

        //Debug.DrawLine(target, transform.position);
    }

    

    private bool TargetInRange()
    {
        if (!closest)
        {
            return false;
        }

        Vector3 distance = transform.position - closest.transform.position;
        return distance.magnitude <= attackRange;
    }

    private bool ShootAngleInRange()
    {
        if (!TargetInRange())
        {
            return false;
        }

        Vector3 direction = closest.transform.position - transform.position;
        float angle = Vector3.Angle(transform.forward, direction);
        return angle < shootMaxAngle;
    }

    private bool CanAttack()
    {
        return Time.time >= attackTime && TargetInRange() && (!shoot || ShootAngleInRange());
    }

    private void Attack()
    {
        if (enemyAnimator)
        {
            enemyAnimator.SetBool("isAttacking", true);
        }
        bool hit = Random.value <= hitChance;
        if (hit)
        {
            if (shoot)
            {
                // Fire a bullet towards the player
                Vector3 direction = closest.transform.position - transform.position;
                GameObject bullet = GameObject.Instantiate(bulletPrefab, gun.position, gun.rotation);
                bullet.GetComponent<Bullet>().Direction = direction;
                bullet.GetComponent<Bullet>().Damage = attackDamage;
            }
            else
            {
                Player player = closest.GetComponent<Player>();
                player.TakeDamage(attackDamage);
            }
        }
        attackTime = Time.time + attackCD;
		Invoke("SwitchAnimationBools", 0.2f);
    }

    public void StartPull()
    {
        if (!isPulling)
        {
            isPulling = true;
            pullStart = Time.time;
        }

    }

    public void StopPull()
    {
        if (isPulling)
        {
            isPulling = false;
            pullStart = Time.time + pullCD;
        }
    }

    private bool CanPull()
    {
        bool can = true;
        if (!closest ||
            Time.time < pullStart ||
            (isPulling && Time.time > (pullStart + pullDuration)))
        {
            can = false;
        }
        else
        {
            Vector3 distance = transform.position - closest.transform.position;
            can = distance.magnitude <= pullRange;
        }

        if (!can)
        {
            StopPull();
        }
        return can;
    }

    private void Pull()
    {
        StartPull();

        Player player = closest.GetComponent<Player>();
        Vector3 direction = transform.position - closest.transform.position;
        player.AddPull(direction.normalized * pullForce);
    }

    public override void Push(Vector3 force)
    {
        StartPush();
        rb.AddForce(force);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = CanPull() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, pullRange);

        if (DEBUG_BOOL == true)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, nextPosition);
        }


        if (shoot && TargetInRange())
        {
            Gizmos.color = ShootAngleInRange() ? Color.green : Color.red;

            Vector3 direction = closest.transform.position - transform.position;
            Gizmos.DrawRay(transform.position, direction);
        }

    }

	private void OnTriggerEnter(Collider other) {
		if (other.tag == "Torch") {
			EnemyPause();
			isScared = true;
		}
		if (other.tag == "Player") {
			isIdle = false;
			MoveToClosest();
			Debug.Log("SeenPlayer");
		}
	}

	private void OnTriggerExit(Collider other) {
		if (other.tag == "Torch") {
			EnemyUnpause();
		}
		if (other.tag == "Player") {
			isIdle = true;
			Idle();
			Debug.Log("LostPlayer");
		}
	}

	public void EnemyPause() {
		agent.speed.Equals(0);
		agent.angularSpeed.Equals(0);
		agent.acceleration.Equals(0);
		isScared = true;
		agent.isStopped = true;

		//if (enemyAnimator) {
		//	enemyAnimator.SetBool("isScared", true);
		//}

		//Destroy(gameObject);
	}

	public void EnemyUnpause() {
		agent.speed.Equals(3);
		agent.angularSpeed.Equals(120);
		agent.acceleration.Equals(8);
		isScared = false;
		agent.isStopped = false;
	}

	private void SwitchAnimationBools() {
		if (enemyAnimator) {
			enemyAnimator.SetBool("isAttacking", false);
		}
		if (enemyAnimator) {
			enemyAnimator.SetBool("isMoving", false);
		}
	}

	
}
