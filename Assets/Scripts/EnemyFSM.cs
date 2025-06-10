using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Pool;

public class EnemyFSM : MonoBehaviour
{
    // 에너미 상태 상수
    enum EnemyState
    {
        Idle,
        Move,
        Attack,
        Return,
        Damaged,
        Die
    }
    
    // 에너미 상태 변수
    private EnemyState m_State;
    
    // 플레이어 발견 범위
    public float findDistance = 8f;
    // 플레이어 트랜스폼
    private Transform player;
    
    // 공격 가능 범위
    public float attackDistance = 2f;
    // 이동 속도
    public float moveSpeed = 5f;
    // 캐릭터 컨트롤러 컴포넌트
    private CharacterController cc;
    
    // 누적 시간
    private float currentTime = 0f;
    // 공격 딜레이 시간
    private float attackDelay = 2f;
    // 에너미의 공격력
    public int attackPower = 3;
    // 초기 위치 저장용 변수
    private Vector3 originPos;
    // 초기 회전 저장용 변수
    private Quaternion originRot;
    // 이동 가능 범위
    public float moveDistance = 20f;
    // 에너미의 체력
    public int hp = 15;
    private int maxHp = 25;
    
    // hp 슬라이더 변수
    public Image hpSlider;
    
    public Transform hpBarTransform;
    
    //에니메이터 변수
    private Animator anim;
    
    //내비게이션 에니전트 변수
    private NavMeshAgent smith;
    
    public EnemySpawn enemySpawn;
    
    private AudioSource audio;

    public float soundMaxTime;
    private float soundTime;
    private float soundcurrentTime = 0f;

    public void Init()
    {
        m_State = EnemyState.Idle;
        
        originPos = transform.position;
        
        
        
        originRot = transform.rotation;
        
        hp = maxHp;
        if (cc != null)
        {
            cc.enabled = true;
        }
    }
    
    void Start()
    {
        m_State = EnemyState.Idle;
        
        originPos = transform.position;
        
        originRot = transform.rotation;
        
        soundTime = Random.Range(0f, soundMaxTime);
        
        audio = GetComponent<AudioSource>();
        cc = GetComponent<CharacterController>();
        // 플레이어의 트랜스폼의 컴포넌트 받아오기
        player = GameObject.Find("Player").transform;
        // 캐릭터 컨트롤러 컴포넌트 받아오기
        // 자식 오브젝트로부터 애니메이션 변수 받아오기
        anim = transform.GetComponentInChildren<Animator>();
        // 내비게이션 에이전트 컴포넌트 받아오기
        smith = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        soundcurrentTime += Time.deltaTime;
        if(soundcurrentTime >= soundTime)
        {
            soundcurrentTime = 0;
            soundTime = Random.Range(0f, soundMaxTime);
            audio.Play();
        }
        
        // 현재 상태를 체크해 해당 상태별로 정해진 기능을 수행하고 싶다.
        switch (m_State)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Move:
                Move();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.Return:
                Return();
                break;
            case EnemyState.Damaged:
                break;
            case EnemyState.Die:
                break;
        }
        // 현재 h[(%)를 슬라이더의 value에 반영한다.
        hpSlider.fillAmount = (float)hp / (float)maxHp;
        
        hpSlider.gameObject.transform.parent.position = hpBarTransform.position;
    }

    private void Idle()
    {
        if (Vector3.Distance(transform.position, player.position) < findDistance)
        {
            m_State = EnemyState.Move;
            
            //이동 애니메이션으로 전환하가
            anim.SetTrigger("IdleToMove");
        }
    }

    private void Move()
    {
        // 만일 현재 위치가 초기 위치에서 이동 가능 범위를 넘어간다면
        if (Vector3.Distance(transform.position, originPos) > moveDistance)
        {
            // 현재 상태를 복귀(Return)로 전환한다.
            m_State = EnemyState.Return;
            
        }
        // 만일 플레이어와의 거리가 공격 범위 밖이라면 플레이어를 향해 이동한다.
        else if (Vector3.Distance(transform.position, player.position) > attackDistance)
        {
            // 이동 방향 설정
            // Vector3 dir = (player.position - transform.position).normalized;
            // // 캐릭터 컨트롤러를 이용해 이동하기
            // cc.Move(dir * moveSpeed * Time.deltaTime);
            // //플레이어를 향해 방향을 전환한다
            // transform.forward = dir;
            
            // 내비게이션 에이전트의 이동을 멈추고 경로를 초기화 한다.
            //smith.isStopped = true;
            //smith.ResetPath();
            
            // 내비게이션으로 접근하는 최소 거리를 공격 가능 거리로 설정한다.
            smith.stoppingDistance = attackDistance;
            //  내비게이션의 목적지를 플레이어의 위치로 설정한다.
            smith.destination = player.position;
            Debug.Log("이동중");
        }
        else // 그렇지 않다면 현재 상태를 공격(Attack)상태로 전환한다.
        {
            m_State = EnemyState.Attack;
            // 누적 시간을 공격 딜레이 시간만큼 미리 진행시켜 놓는다.
            currentTime = attackDelay;
            // 공격 대기 애니메이션 플레이
            anim.SetTrigger("MoveToAttackDelay");
        }
    }

    private void Attack()
    {
        // 만일 플레이어가 공격 범위 이내에 있다면 플레이어를 공격한다.
        if (Vector3.Distance(transform.position, player.position) < attackDistance)
        {
            // 일정한 시간만다 플레이어를 공격한다.
            currentTime += Time.deltaTime;
            if (currentTime > attackDelay)
            {
                //player.GetComponent<PlayerMove>().DamageAction(attackPower);
                currentTime = 0;
                // 공격 애니메이션 플레이
                anim.SetTrigger("StartAttack");
            }
        }
        else // 그렇지 않다면 현재 상태를 이동으로 전환한다. (재추격 실시)
        {
            m_State = EnemyState.Move;
            currentTime = 0;
            // 이동 애니메이션 플레이
            anim.SetTrigger("AttackToMove");
        }
    }
    
    // 플레이어의 스크립트의 데미지 처리 메서드 실행하기
    public void AttackAction()
    {
        player.GetComponent<PlayerMove>().DamageAction(attackPower);
    }

    private void Return()
    {
        // 만일 초기 위치에서의 거리가 0.1f이상이라면 초기 위치 쪽으로 이동한다.
        if (Vector3.Distance(transform.position, originPos) > 0.1f)
        {
            // Vector3 dir = (originPos - transform.position).normalized;
            // cc.Move(dir * moveSpeed * Time.deltaTime);
            // // 방향을 복귀 지점으로 전환한다.
            // transform.forward = dir;
            
            // 내비게이션의 목적지를 초기 위치로 설정한다.
            smith.destination = originPos;
            // 내비게이샨으로 접근하는 최소거리를 '0'으로 설정한다.
            smith.stoppingDistance = 0;
            
        }
        else // 그렇지 않다면 자기 위치를 초기 위치로 조정하고 현재 상태를 대기로 전환한다.
        {
            //내비게이션 에이전트의 이동을 멈추고 경로를 초기화한다.
            smith.isStopped = true;
            smith.ResetPath();
            
            transform.position = originPos;
            transform.rotation = originRot;
            // hp를 다시 회복한다.
            hp = maxHp;
            m_State = EnemyState.Idle;
            //대기 애니메이션으로 전환하는 트렌지션을 호출한다.
            anim.SetTrigger("MoveToIdle");
        }
    }

    private void Die()
    {
        // 진행 중인 피격 코루틴을 중지한다.
        StopAllCoroutines();
        GameManager.gm.killCount++;
        // 죽음 상태를 처리하기 위한 코루틴을 실행한다.
        StartCoroutine(DieProcess());
    }

    IEnumerator DieProcess()
    {
        // 캐릭터 컨트로러 컴포넌트를 비활성화 시킨다.
        cc.enabled = false;
        // 2초동안 기다린 후에 자기 자신을 제거한다.
        yield return new WaitForSeconds(2f);
        enemySpawn.ReleaseEnemy(gameObject);
        Destroy(hpSlider.transform.parent.gameObject);
        //Destroy(gameObject);
    }

    private void Damaged()
    {
        // 피격 상태를 처리하기 위한 코루틴을 실행한다.
        StartCoroutine(DamageProcess());
    }
    
    // 데미지 처리용 코루틴 메서드
    IEnumerator DamageProcess()
    {
        // 피격 모션 시간 만큼 기다린다.
        yield return new WaitForSeconds(1f);
        // 현재 상태를 이동 상태로 전환한다.
        m_State = EnemyState.Move;
    }
    
    // 데미지 실행 메서드
    public void HitEnemy(int hitPower)
    {
        // 만일 이미 피격 상태이거나 사망 상태 또는 복귀 상태라면 아무 처리도 하지 않고 메서드를 종료한다.
        if (m_State == EnemyState.Damaged || m_State == EnemyState.Die || m_State == EnemyState.Return)
        {
            return;
        }
        
        // 플레이어의 공격력 만큼 에너미의 체력을 감소시킨디ㅏ.
        hp -= hitPower;
        
        //내비게이션 에이전트의 이동을 멈추고 경로를 초기화 한다.
        smith.isStopped = true;
        smith.ResetPath();
        
        // 에너미의 체력이 0보다 크면 피격 상태로 전환한다.
        if (hp > 0)
        {
            //m_State = EnemyState.Damaged;
            //피격 애니메이션을 플레이한다.
            anim.SetTrigger("Damaged");
            //Damaged();
        }
        else // 그렇지 않다면 죽음 상태로 전환한다.
        {
            m_State = EnemyState.Die;
            //죽음 애니메이션을 플레이한다.
            anim.SetTrigger("Die");
            Die();
        }
    }
}
