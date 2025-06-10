using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    //사용자 데이터를 새로 저장하거나 저장된 데이터를 읽어 사용자의 입력과 일치하는지 검사하게 하고 싶다.
    
    //사용자 아이디 변수
    public InputField id;
    //사용자 패스워드 변수
    public InputField password;
    
    // 검사 텍스트 변수
    public Text notify;
    
    void Start()
    {
        // 검사 텍스트 창을 비운다
        notify.text = "";
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    
    void Update()
    {
        
    }

    //아이디와 패스워드 저장 매서드
    public void SaveUserData()
    {
        // 만일 검사에 문제가 있으면 메서드 종료
        if(!CheckInput(id.text,password.text)) return;
        
        // 만일 시스템에 저장되어 있는 아이디가 존재하지 않는다면.
        if (!PlayerPrefs.HasKey(id.text))
        {
            // 사용자의 아이디는 키(key)러 패스워드는 값(value)로 설정해 저장한다.
            PlayerPrefs.SetString(id.text, password.text);
            notify.text = "아이디 생성이 완료 되었습니다";
        }
        else
        {
            notify.text = "이미 존재하는 아이디 입니다.";
        }

    }
    
    //로그인 매서드 
    public void CheckUserData()
    {
        // 만일 입력 검사에서 문제가 있으면 메서드 종료
        if (!CheckInput(id.text, password.text)) return;
        
        //사용자가 입력한 아이디를 키로 사용해 시스템에 저장된 값을 불러온다
        string pass = PlayerPrefs.GetString(id.text);
        //만일 사용자가 입력한 패스워드와 시스템에서 불러온 값을 비교해서 동일하다면
        if (password.text == pass)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            //다음 씬(1번씬)으로 로드한다.
            SceneManager.LoadScene(1);
        }
        //그렇지 않고 두 데디어의 ㄱ밧이 다르면 , 사용자 정보 불일치 메세지를 띄운다
        else
        {
            notify.text = "입력하신 아이디와 패스워드가 일치하지 않습니다.";
        }
    }
    
    //입력 완료 확인 메서드
    bool CheckInput(string id, string pwd)
    {
        //만일 아이디와 패스워드 입력안이 하나라도 비어있으면 사용자 정보 입력을 요구한다
        if(id == "" || pwd == "")
        {
            notify.text = "아이디 또는 패스워드를 입력해주세요";
            return false;
        }
        else
        {
            return true;
        }
    }
}
