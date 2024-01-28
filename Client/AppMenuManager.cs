using QS.Global.Define;
using QS.Global.Event;
using QS.Global.Pattern;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Valve.VR;
using Valve.VR.InteractionSystem;

/// <summary>
/// [ApplicationMenu 매니저 클래스]
/// 
/// 컨트롤의 어플리케이션 메뉴 버튼 클릭시
/// 공통 메뉴 팝업 UI생성
/// </summary>
public class AppMenuManager : Singleton<AppMenuManager>
{
    TrainingDefine.KIND loading = TrainingDefine.KIND.LOADING;

    #region Fields

    [Header("[Prefabs]")]
    /// <summary>
    /// 어플리케이션 메뉴 UI 프로퍼티
    /// </summary>
    [SerializeField]
    Canvas canvas;

    [SerializeField]
    GameObject bg;

    //[SerializeField]
    //GameObject roomExitBtn;

    [SerializeField]
    GameObject trainingMenu;

    [Header("[Vive Controller Action]")]
    //바이브 컨트롤러 액션 프로퍼티        
    [SerializeField]
    SteamVRActionEvent R_HandAction;
    [SerializeField]
    SteamVRActionEvent L_HandAction;
    #endregion

    #region Variables


    /// <summary>
    /// 메인 카메라 변수
    /// </summary>
    [SerializeField] Camera mainCam;

    /// <summary>
    /// 이전 카메라 BackgroundType 저장 변수.
    /// </summary>
    CameraClearFlags prevCamBgType;

    /// <summary>
    /// 레이어 마스크 정보 변수
    /// </summary>
    string[] layerShow = new string[] { "AppMenu", "LayserBeam" };
    string[] layerHide = new string[] { "AppMenu" };

    bool isSceneChage = false;
    /// <summary>
    /// 텔레포트 컴포넌트 변수
    /// </summary>
    Teleport teleport;

    #endregion

    #region Property
    /// <summary>
    /// 메뉴 UI 상태 변수
    /// </summary>
    public static bool isShowUI
    {
        get;
        private set;
    }

    bool IsMenuScene
    {
        get
        {
            return SceneDefine.GetSceneNameToFlag(SceneManager.GetActiveScene().name) == SceneDefine.FLAG.MAIN;
        }

    }
    #endregion

    #region LifeCycle Function

    IEnumerator Start()
    {
        yield return new WaitForSeconds(2.0f);
        Initialize();
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        teleport = GameObject.FindObjectOfType<Teleport>();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenuUI();
        }
    }
#endif

    #endregion

    #region Public Function

    /// <summary>
    /// 초기화 함수.
    /// </summary>
    public void Initialize()
    {
        RegEvent();

        HideUI();

        teleport = GameObject.FindObjectOfType<Teleport>();
    }

    #endregion

    #region Regist / UnRegist Function

    /// <summary>
    /// 매뉴 버튼 클릭 이벤트 등록
    /// </summary>
    void RegEvent()
    {
        if (R_HandAction != null)
        {
            R_HandAction.action.RemoveOnChangeListener(OnApplicationButtonPressed, R_HandAction.input);
            R_HandAction.action.AddOnChangeListener(OnApplicationButtonPressed, R_HandAction.input);
        }
        if (L_HandAction != null)
        {
            L_HandAction.action.RemoveOnChangeListener(OnApplicationButtonPressed, L_HandAction.input);
            L_HandAction.action.AddOnChangeListener(OnApplicationButtonPressed, L_HandAction.input);
        }

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        EventHandler.Register<SceneDefine.FLAG>(GlobalEventNameDefine.EVT_RCV_SCENE_CHANGE, OnRcvSceneChangeListener);
    }

    /// <summary>
    /// 매뉴 버튼 클릭 이벤트 해제
    /// </summary>
    void UnRegEvent()
    {
        if (R_HandAction != null)
        {
            R_HandAction.action.RemoveOnChangeListener(OnApplicationButtonPressed, R_HandAction.input);
        }
        if (L_HandAction != null)
        {
            L_HandAction.action.RemoveOnChangeListener(OnApplicationButtonPressed, L_HandAction.input);
        }

        EventHandler.Unregister<SceneDefine.FLAG>(GlobalEventNameDefine.EVT_RCV_SCENE_CHANGE, OnRcvSceneChangeListener);
    }


    #endregion

    #region Event Callback Function


    /// <summary>
    /// 버튼 클릭시 이벤트 발생
    /// </summary>
    /// <param name="fromAction"></param>
    /// <param name="fromSource"></param>
    /// <param name="newState"></param>
    private void OnApplicationButtonPressed(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
    {
        if (newState)
        {
            if (IsMenuScene)
            {
                Debug.Log("Is Main Scene");
                return;
            }

            ToggleMenuUI();
        }
    }

    private void OnRcvSceneChangeListener(SceneDefine.FLAG argFlag)
    {
        HideUI();
    }

    #endregion

    #region Menu UI Visible Function

    void ToggleMenuUI()
    {
        isShowUI = !isShowUI;
        if (isShowUI)
        {

            ShowUI();
        }
        else
        {
            HideUI();
        }

    }

    /// <summary>
    /// 메뉴 UI 보이기
    /// </summary>
    void ShowUI()
    {
        NarrationManager.Instance.Pause();

        isShowUI = true;

        if (mainCam == null)
        {
            mainCam = Camera.main.GetComponent<Camera>();
        }

        if (bg != null)
        {
            bg.GetComponent<SphereCollider>().enabled = true;
        }

        if (canvas != null)
        {
            transform.position = mainCam.transform.position;
            transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);

            canvas.gameObject.SetActive(true);
            canvas.worldCamera = mainCam;

            if (trainingMenu)
            {
                trainingMenu.SetActive(true);
            }

        }

        prevCamBgType = mainCam.clearFlags;
        mainCam.clearFlags = CameraClearFlags.Color;
        mainCam.backgroundColor = Color.black;
        mainCam.cullingMask = LayerMask.GetMask(layerShow);

        if (teleport != null)
        {
            teleport.gameObject.SetActive(false);
        }

        EventHandler.Execute(GlobalEventNameDefine.EVT_CMD_STEAMVR_INPUT_CLICK_APPLICATIONMENU, isShowUI);

        //if(SceneManager.GetActiveScene().name == TrainingDefine.KIND.MULTI_FIELD_MAINTENANCE.ToString() ||
        //    SceneManager.GetActiveScene().name == TrainingDefine.KIND.MULTI_UNIT_MAINTENANCE.ToString())
        //{

            //if (roomExitBtn != null)
            //{
            //    roomExitBtn.SetActive(true);
            //}
            //else
            //{
            //    roomExitBtn.SetActive(false);
            //}
        //}
    }

    private void OnEnterRoomListener(APIError error, object data)
    {
        EnterRoomData info = (EnterRoomData)data;

        bool isSquareRoom = info.RoomIndex == 0 ? true : false;

    }

    public void RoomExitButtonClick()
    {
        if (FindObjectOfType<RoomMenuUI>())
        {
            FindObjectOfType<RoomMenuUI>().ExitTraining(false);
        }
        else
        {
            Debug.Log("RoomMenuUI.cs를 찾을수없음");
        }
    }

    /// <summary>
    /// 메뉴 UI 감추기
    /// </summary>
    void HideUI()
    {
        NarrationManager.Instance.Resume();

        isShowUI = false;
        StartCoroutine(FindCamera()) ;

        if (bg != null)
        {
            bg.GetComponent<SphereCollider>().enabled = false;
        }

        if (canvas != null)
        {
            canvas.gameObject.SetActive(false);
        }

        if (mainCam != null)
        {
            mainCam.clearFlags = prevCamBgType;
            mainCam.cullingMask = ~LayerMask.GetMask(layerHide);
        }

        if (teleport != null)
        {
            teleport.gameObject.SetActive(true);
        }
        EventHandler.Execute(GlobalEventNameDefine.EVT_CMD_STEAMVR_INPUT_CLICK_APPLICATIONMENU, isShowUI);

        //roomExitBtn.SetActive(false);
    }

    IEnumerator FindCamera()
    {
        yield return new WaitForEndOfFrame();

        if (mainCam == null)
        {
            if (Camera.main.GetComponent<Camera>() == null) yield return null;
            else mainCam = Camera.main.GetComponent<Camera>();
        }


    }

    #endregion

    #region Menu Button Events Function
    /// <summary>
    /// 훈련 재시작 버튼 이벤트
    /// </summary>
    public void OnReStartTraning()
    {
        if (FindObjectOfType<RoomMenuUI>())
        {
            FindObjectOfType<RoomMenuUI>().ExitTraining(true);
        }
        else
        {
            Debug.Log("RoomMenuUI.cs를 찾을수없음");
        }

        EventHandler.Execute(GlobalEventNameDefine.EVT_REQ_TRAINING_KIND_CHANGE, loading);
        EventHandler.Execute(GlobalEventNameDefine.EVT_REQ_SCENE_CHANGE, SceneDefine.GetTrainingDefToFlag(loading));

        //EventHandler.Execute(GlobalEventNameDefine.EVT_CMD_APPMENU_RESTART_TRAINING);
        NarrationManager.Instance.StopNarration();
        ToggleMenuUI();
    }

    /// <summary>
    /// 메인이동 버튼 이벤트
    /// </summary>
    public void OnGoMenuScene()
    {
        if (FindObjectOfType<RoomMenuUI>())
        {
            FindObjectOfType<RoomMenuUI>().ExitTraining(true);
        }
        else
        {
            Debug.Log("RoomMenuUI.cs를 찾을수없음");
        }

        CheckSceneData.CheckSceneDataSting = SceneDefine.FLAG.MAIN.ToString();
        //Debug.Log("Check Scene : " + CheckSceneData.CheckSceneDataSting);

        EventHandler.Execute(GlobalEventNameDefine.EVT_REQ_TRAINING_KIND_CHANGE, loading);
        EventHandler.Execute(GlobalEventNameDefine.EVT_REQ_SCENE_CHANGE, SceneDefine.GetTrainingDefToFlag(loading));


        //EventHandler.Execute(GlobalEventNameDefine.EVT_REQ_SCENE_CHANGE, SceneDefine.FLAG.MAIN);
        NarrationManager.Instance.StopNarration();
        ToggleMenuUI();
    }

    public void OnGoTutorial()
    {
        CheckSceneData.CheckSceneDataSting = SceneDefine.FLAG.TUTORIAL.ToString();
        //Debug.Log("Check Scene : " + CheckSceneData.CheckSceneDataSting);

        EventHandler.Execute(GlobalEventNameDefine.EVT_REQ_TRAINING_KIND_CHANGE, loading);
        EventHandler.Execute(GlobalEventNameDefine.EVT_REQ_SCENE_CHANGE, SceneDefine.GetTrainingDefToFlag(loading));

        //EventHandler.Execute(GlobalEventNameDefine.EVT_REQ_SCENE_CHANGE, SceneDefine.FLAG.TUTORIAL);
        NarrationManager.Instance.StopNarration();
        ToggleMenuUI();
    }


    public void OnCancel()
    {
        ToggleMenuUI();
    }

    #endregion

}

