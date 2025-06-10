using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerFire : MonoBehaviour
{
    // 발사 위치
    public GameObject firePosition;
    // 투척 무기 오브젝트
    public GameObject bombFactory;
    // 투척 파워
    public float throwPower = 15f;
    // 피격 이펙트 오브젝트
    public GameObject bulletEffect;
    // 피격 파티클 시스템
    private ParticleSystem ps;
    // 발사 무기 공격력
    public int weaponPower = 5;

    public int ammo = 15;
    public int maxAmmo = 15;
    public float reloadTime = 2f;
    private bool isReloading = false;
    
    //애니메이터 변수
    private Animator anim;
    
    //무기 모드 변수
    enum WeaponMode
    {
        Normal,
        Sniper
    }

    private WeaponMode wMode;
    
    // 카메라 확대 확인용 변수
    private bool ZoomMode = false;

    public Text Text_WeaponMode;
    
    // 총 발사 효과 오브젝트 배열
    public GameObject[] eff_Flash;
    
    [Header("UI")]
    public Text Text_Ammo;
    public Text Text_MaxAmmo;
    public Text Text_Reload;
    
    [Header("사운드")]
    public GameObject sound_Fire;
    public GameObject sound_Reload;
    
    
    void Start()
    {
        // 피격 이펙트 오브젝트에서 파티클 시스템 컴포넌트 가져오기
        ps = bulletEffect.GetComponent<ParticleSystem>();
        anim = GetComponentInChildren<Animator>();
        //무기 기본모드를 노멀 모드로 설정한다.
        wMode = WeaponMode.Normal;
        Text_WeaponMode.text = "Normal Mode";
        Text_MaxAmmo.text = maxAmmo.ToString();
        Text_Reload.gameObject.SetActive(false);
    }

    void Update()
    {
        //게임 상태가 '게임중' 상태일떄한 조작할 수있게 한다.
        if(GameManager.gm.gState != GameManager.GameState.Run)
            return;
        
        // 마우스 오른쪽 버튼을 누르면 시선이 바라보는 방향으로 수류탄을 던지고 싶다.
        // 스나이퍼 모드 : 마우스 오른쪽 버튼을 누르면 화면을 확대하고 싶다.
        // 1. 마우스 오른쪽 버튼을 입력 받는다.
        if (Input.GetMouseButtonDown(1) && !isReloading)
        {
            switch (wMode)
            {
                case WeaponMode.Normal:
                    // 수류탄 오브젝트를 생성한 후 수류탄의 생성 위치를 발사 위치로 한다.
                    GameObject bomb = Instantiate(bombFactory) as GameObject;
                    bomb.transform.position = firePosition.transform.position;
            
                    // 수류탄 오브젝트의 Rigidbody 컴포넌트를 가져온다.
                    Rigidbody rb = bomb.GetComponent<Rigidbody>();
                    // 카메라의 정면 방향으로 수류탄에 물리적인 힘을 가한다.
                    rb.AddForce(Camera.main.transform.forward * throwPower, ForceMode.Impulse);
                    rb.angularVelocity = Random.insideUnitSphere * 10f;
                    break;
                case WeaponMode.Sniper:
                    // 만일 줌 모드 상태가 아니라면 카메라를 확대하고 줌 모드 상태로 변경한다.
                    if (!ZoomMode)
                    {
                        Camera.main.fieldOfView = 15f;
                        ZoomMode = true;
                        
                    }
                    else
                    {
                        Camera.main.fieldOfView = 60f;
                        ZoomMode = false;
                    }
                    break;
            }
            
        }
        
        // 마우스 왼쪽 버튼을 누르면 시선이 바라보는 방향으로 총을 발사하고 싶다.
        // 마우스 왼쪽 버튼을 입력 받는다.
        if (Input.GetMouseButtonDown(0) &&!isReloading)
        {
            if(ammo == 0) return;
            ammo--;
            Instantiate(sound_Fire);
            // Muzzle flash effect 이펙트를 실시한다.
            StartCoroutine(ShootEffectOn(0.05f));
            //만일 이동 블랜드 트리 파라미터의 값이 0이라면, 공격 애니메이션을 실시한다.
            if (anim.GetFloat("MoveMotion") == 0)
            {
                anim.SetTrigger("Attack");
            }
            
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            // 레이가 부딪힌 대상의 정보를 저장할 변수를 생성한다.
            RaycastHit hitInfo = new RaycastHit();
            
            // 레이를 발사한 후 만일 부딪힌 물체가 있으면 피격 이펙트를 표시한다.
            if (Physics.Raycast(ray, out hitInfo))
            {
                // 만일 레이에 부딪힌 대상의 레이어가 'Enemy'라면 데미지 메서드를 실행시킨다.
                if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    EnemyFSM eFSM = hitInfo.transform.GetComponent<EnemyFSM>();
                    eFSM.HitEnemy(weaponPower);
                }
                // 그렇지 않다면 레이에 부딪힌 지점에 피격 이펙트를 플레이한다.
                else
                {
                    // 피격 이펙트의 위치를 레이가 부딪힌 지점으로 이동시킨다.
                    bulletEffect.transform.position = hitInfo.point;
                    // 피격 이펙트의 forward 방향을 레이가 부딪힌 지점의 법선 벡터와 일치시킨다.
                    bulletEffect.transform.forward = hitInfo.normal;
                    // 피격 이펙트를 플레이한다.
                    ps.Play();    
                }
            }
        }
        
        //만일 키보드의 숫자 1번 입력을 받으면 무기모드에서 일반모드로 변경한다.
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            wMode = WeaponMode.Normal;
            // 카메라 화면을 원래대로 돌려준다
            Camera.main.fieldOfView = 60f;
            Text_WeaponMode.text = "Normal Mode";
            
        }else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            wMode = WeaponMode.Sniper;
            Text_WeaponMode.text = "Sniper Mode";
        }

        if (Input.GetKeyDown(KeyCode.R) && !isReloading && ammo < maxAmmo)
        {
            isReloading = true;
            StartCoroutine(Reload());
            Instantiate(sound_Reload);
            StartCoroutine(ReloadTextAnimation());
        }

        if (ammo == maxAmmo)
        {
            Text_Ammo.color = Color.green;
        }else if (ammo == 7)
        {
            Text_Ammo.color = Color.yellow;
        }else if (ammo == 0)
        {
            Text_Ammo.color = Color.red;
        }
        Text_Ammo.text = ammo.ToString();
    }

    IEnumerator Reload()
    {
        yield return new WaitForSeconds(reloadTime);
        ammo = maxAmmo;
        isReloading = false;
        Text_Reload.gameObject.SetActive(false);
    }
    IEnumerator ReloadTextAnimation()
    {
        Text_Reload.gameObject.SetActive(true);
        while (isReloading)
        {
            Text_Reload.text = "장전중.";
            yield return new WaitForSeconds(0.5f);
            Text_Reload.text = "장전중..";
            yield return new WaitForSeconds(0.5f);
            Text_Reload.text = "장전중...";
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    

    // Muzzle Flash effect 코루틴 메서드
    IEnumerator ShootEffectOn(float duration)
    {
        // 숫자를 랜덤하게 뽑는다
        int num = Random.Range(0, eff_Flash.Length-1);
        // 이펙트 오브젝트 배열에서 뾘 숫자에 해당하는 이펙트 오브젝트를 활성화 한다.
        eff_Flash[num].SetActive(true);
        yield return new WaitForSeconds(duration);
        eff_Flash[num].SetActive(false);
    }
}
