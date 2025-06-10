using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    // 이동 속도 변수
    public float moveSpeed = 7f;
    // 캐릭터 컨트롤러 변수
    private CharacterController cc;
    // 중력 변수
    private float gravity = -20f;
    // 수직 속력 변수
    private float yVelocity = 0;
    // 점프력 변수
    public float jumpPower = 10f;
    // 점프 상태 변수
    public bool isJumping = false;
    // 플레이어 체력 변수
    public int hp = 50;
    // 플레이어 최대 체력 변수
    public int maxHp = 50;
    
    // hp 슬라이더 변수
    public Slider hpSlider;
    // Hit 효과 오브젝트
    public GameObject hitEffect;
    // 애니메이터 변수
    private Animator anim;
    
    [Header("Sound")]
    public GameObject sound_footstep;
    [SerializeField] private AudioClip[] footstepClips;
    private float footstepTimer = 0f;
    private float footstepInterval = 0.5f; // 발걸음 소리 간격
    
    void Start()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // 4. 현제 플레이어 hp(%)를 hp 스라이더의 value에 반영한다.
        hpSlider.value = (float)hp / (float)maxHp;
        
        //게임 상태가 '게임중' 상태일떄한 조작할 수있게 한다.
        if(GameManager.gm.gState != GameManager.GameState.Run)
            return;
            
        // [WASD] 키를 입력하면 캐릭터를 그 방향으로 이동시키고 싶다.
        // [Spacebar] 키를 입력하면 캐릭터를 수직으로 점프 시키고 싶다.
        
        // 1. 사용자의 입력을 받는다.
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        // 2. 이동 방향을 설정한다.
        Vector3 dir = new Vector3(h, 0, v);
        dir = dir.normalized;
        
        //이동 블랜딩 크리를 호출하고 백터의 크기의 값을 넘겨준다
        anim.SetFloat("MoveMotion", dir.magnitude);
        
        // 2-1. 메인 카메라를 기준으로 방향을 변환한다.
        dir = Camera.main.transform.TransformDirection(dir);
        
        // 2-2. 만일, 점프 중이었고, 다시 바닥에 착지했다면
        if (isJumping && cc.collisionFlags == CollisionFlags.Below)
        {
            // 점프 전 상태로 초기화 한다.
            isJumping = false;
            // 캐릭터 수직 속도를 0으로 초기화한다.
            yVelocity = 0;
        }
        
        // 2-3. 만일, 키보트 [Spacebar]를 입력했고, 점프하지 않은 상태라면
        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            // 캐릭터 수직 속도에 점프력을 적용하고, 점프상태로 변경한다.
            yVelocity = jumpPower;
            isJumping = true;
        }
        
        // 2-3.캐릭터 수직 속도에 중력 값을 적용한다.
        yVelocity += gravity * Time.deltaTime;
        dir.y = yVelocity;
        
        // 3. 이동 속도에 맞춰 이동한다.
        // p = p0 + vt
        // transform.position += dir * moveSpeed * Time.deltaTime;
        cc.Move(dir * moveSpeed * Time.deltaTime);
        
        
        
        
        if ((h != 0f || v!=0f) && cc.isGrounded)
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                PlayFootstepSound();
                footstepTimer = 0f;
            }
        }
        
    }
    
    private void PlayFootstepSound()
    {
        if (footstepClips.Length > 0)
        {
            int randomIndex = Random.Range(0, footstepClips.Length);
            GameObject sound = Instantiate(sound_footstep, transform.position, Quaternion.identity);
            AudioSource audioSource = sound.GetComponent<AudioSource>();
            audioSource.clip = footstepClips[randomIndex];
            audioSource.Play();
        }
    }


    public void DamageAction(int damage)
    {
        // 에너미의 공격력 만큼 플레이어의 체력을 깎는다.
        hp -= damage;
        
        //만일 플레이어의 체력이 0보다 크면 피격화를 출력한다.
        if (hp > 0)
        {
            StartCoroutine(PlayerHitEffect());
        }
    }
    
    //피격효과 코루틴 메서드
    IEnumerator PlayerHitEffect()
    {
        //1. 피격 UI를 활성화 한다.
        hitEffect.SetActive(true);
        //2. 0.3초간 대기한다
        yield return new WaitForSeconds(0.3f);
        //3. 피격 UI를 비활성화 한다,
        hitEffect.SetActive(false);
    }
}
