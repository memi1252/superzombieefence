using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // 싱글턴 변수
    public static GameManager gm;

    // 게임 상태 상수
    public enum GameState
    {
        Ready,
        Run,
        Puase,
        Clear,
        GameOver
    }
    
    // 현재의 게임 상태 변수
    public GameState gState;
    
    // 게임 상태 UI 오브젝트 변수
    public GameObject gameLabel;
    //게임 상태 UI 텍스트 컴포넌트 변수
    private Text gameText;
    
    //PlayerMove 클래스 변수 
    private PlayerMove player;
    
    //옵션 화면 UI 오브젝트 변수
    public GameObject gameOption;
    
    public Text killCountText;
    public Text TimeText;
    
    public int killCount = 0;
    public float playTime = 0f;


    [Header("Ui")] 
    public GameObject ClearUi;
    public Text ClearTimerText;
    public Text ClearKillCountText;
    
    private void Awake()
    {
        if (gm == null)
        {
            gm = this;
        }
    }

    private void Start()
    {
         Cursor.visible = true;
         Cursor.lockState = CursorLockMode.Locked;
        
        //초기 게임 상태는 준비 상태로 설정한다.
        gState = GameState.Ready;
        
        // 게임 상태 UI 오브젝트에서 Text 컴포넌트를 가져온다
        gameText = gameLabel.GetComponent<Text>();
        // 상태 텍스트의 내용을 'Ready'로 한다.
        gameText.text = "Ready...";
        //상태 텍스트의 색상을 주황색으로 한다. 
        gameText.color = new Color32(255, 185, 0, 255);
        //게임 준비 -> 게임중 상태로 전환하기
        StartCoroutine(ReadyToStart());
        // 플레이어 오브젝트를 찾은 후 플레이의 playermove컴포넌드 가져오기
        player = GameObject.Find("Player").GetComponent<PlayerMove>();
    }

    private void Update()
    {
        if (killCount >= 100)
        {
            gState = GameState.Clear;
            ClearUi.SetActive(true);
            Time.timeScale = 0f;
            TimeText.gameObject.SetActive(false);
            killCountText.gameObject.SetActive(false);
            ClearTimerText.text = $"플레이 타임 {(int)playTime / 60:00}:{(int)playTime % 60:00}";
            ClearKillCountText.text = $"{killCount} 킬";
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        killCountText.text = $"{killCount}킬";

        if (gState == GameState.Run)
        {
            playTime += Time.deltaTime;
        }
        
        TimeText.text = $"{(int)playTime / 60:00}:{(int)playTime % 60:00}";
        
        //만일 플레이어 hp 가 0이하라면 
        if (player.hp <= 0)
        {
            //플레이어ㅡ 애니메이션을 멈춘다
            player.GetComponentInChildren<Animator>().SetTrigger("MoveMotion");
            //상태 텍스트를 활성화 한다.
            gameLabel.SetActive(true);
            //상태 텍스트의 내용을 'Game Over'로 한다.
            gameText.text = "Game Over";
            //상태 텍스트의 색상을 빨간색으로 한다.
            gameText.color = new Color32(255, 0, 0, 255);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
            //상태 텍스트의 자식 오브젝트의 트랜스폼 컴포넌트를 가져온다.
            //Transform buttons = gameText.transform.GetChild(0);
            //버튼 오브젝트를 황성화 한다.
            gameOption.SetActive(true);
            
            //상태를 '게임오버'상태로 변경한다.
            gState = GameState.GameOver;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameOption.activeSelf)
            {
                CloseOptionWindow();
            }else
            {
                OpenOptionWindow();
            }
        }
    }

    IEnumerator ReadyToStart()
    {
        //2초간 대기 
        yield return new WaitForSeconds(2f);
        //상태 텍스트의 내용을 'Go!'로 한다.
        gameText.text = "Go!";
        //0.5초간 대기
        yield return new WaitForSeconds(0.5f);
        //상태 텍스트를 비황성화 한다.
        gameLabel.SetActive(false);
        //상태를 '게임중'상태로 변경한다.
        gState = GameState.Run;
    }
    
    // 옵션 화면 켜기
    public void OpenOptionWindow()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // 옵션 차을 활성화 ㅎ나다.
        gameOption.SetActive(true);
        // 게임 속도를 0배속으로 전환한다.
        Time.timeScale = 0f;
        //게임 상태를  일시정지 상태로 변경한다.
        gState = GameState.Puase;
    }
    // 계속하기
    public void CloseOptionWindow()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // 옵션 창을 비활성화 한다.
        gameOption.SetActive(false);
        //게임 속도를 1배속으로 전환한다.
        Time.timeScale = 1f;
        //게임 상태를  게임중 상태로 변경한다.
        gState = GameState.Run;
    }
    // 다시하기 옵션
    public void ReStartGame()
    {
        // 게임 속도를 1배속으로 전환한다.
        Time.timeScale = 1f;
        //현재 씬을 다시 로드한다.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // 로딩 화면 씬을 로드한다.
        SceneManager.LoadScene(1);
    }
    
    // 게임 종료 옵션
    public void QuitGame()
    {
        //애플리케이션을 종료한다
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
