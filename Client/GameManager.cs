using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QS.Global.Pattern;
using QS.Global.Define;
using System;
using Valve.VR.InteractionSystem;
using Valve.VR;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class GameManager : Singleton<GameManager>
{
    /// <summary>
    /// 전역 볼륨
    /// </summary>
    [Header("[PostProcessing]")]

    [SerializeField]
    Volume globalVolumeObject;

    /// <summary>
    /// ApplicationMenu Prefab
    /// </summary>
    [SerializeField]
    GameObject appMenuObject;

    ///// <summary>
    ///// ApplicationMenu Prefab
    ///// </summary>
    //[SerializeField]
    //GameObject appMenuObject;

    /// <summary>
    /// 페이드 인/아웃 볼륨프로파일
    /// </summary>
    [SerializeField]
    VolumeProfile fadeInOutVolumeProfile;

    Volume globalVolume;

    Camera mainCam;

    float delaySecAtSceneChage = 1f;

    bool sceneMoving = false;

    Coroutine coFadeIn;
    Coroutine coFadeOut;

    #region Action

    Action<SceneDefine.FLAG> actLoadScene;
    Action actFadeRet;

    #endregion
    
    //아직 적용 전 
    public bool canRayControll = true;
    public bool canMove = true;


    //나중에 [serializeField] 제거
    static KDDXDefine.GROUP selectSystem = KDDXDefine.GROUP.NONE;
    static TrainingDefine.KIND selectTraining = TrainingDefine.KIND.NONE;
    static KDDXDefine.KIND selectEquip = KDDXDefine.KIND.NONE;
    static KDDXDefine.OPERATION selectOperation = KDDXDefine.OPERATION.NONE;


    #region Properties
    public KDDXDefine.GROUP SELECT_SYSTEM
    {
        get
        {
            return selectSystem;
        }
    }
    public TrainingDefine.KIND SELECT_TRAINING
    {
        get
        {
            return selectTraining;
        }
    }

    public KDDXDefine.KIND SELECT_EQUIP
    {
        get
        {
            return selectEquip;
        }
        set
        {
            selectEquip = value;
        }
    }

    public KDDXDefine.OPERATION SELECT_OPERATION
    {
        get
        {
            return selectOperation;
        }
        set
        {
            selectOperation = value;
        }
    }

    #endregion


    private void Start()
    {
        SceneManager.sceneLoaded += TestSceneLoad;
    }

    bool trigger = false;

    void TestSceneLoad(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("Scene Load됨 ");

        if (SceneManager.GetActiveScene().name == "MAIN" && !trigger)
        {
            RegEvent();

            trigger = true;

            CreateGlobalVolume();

            CreateAppMenuManager();

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            actLoadScene = null;
            actLoadScene = new Action<SceneDefine.FLAG>(
                (argScene) => {
                    EventHandler.Execute(GlobalEventNameDefine.EVT_RCV_SCENE_CHANGE, argScene);
                    SceneManager.LoadScene(SceneDefine.GetFlagToSceneName(argScene));
                });
        }
    }


    private void OnDestroy()
    {
        UnRegEvent();
    }

    void RegEvent()
    {
        EventHandler.Register<KDDXDefine.GROUP>(GlobalEventNameDefine.EVT_REQ_SYSTEM_GROUP_CHANGE, OnReqSystemChangeListener);
        EventHandler.Register<TrainingDefine.KIND>(GlobalEventNameDefine.EVT_REQ_TRAINING_KIND_CHANGE, OnReqTrainingChangeListener);
        EventHandler.Register<KDDXDefine.KIND>(GlobalEventNameDefine.EVT_REQ_EQUIP_KIND_CHANGE, OnReqEquipChangeListener);

        //씬 변경 요청 이벤트
        EventHandler.Register<SceneDefine.FLAG>(GlobalEventNameDefine.EVT_REQ_SCENE_CHANGE, OnReqSceneChangeListener);

        //씬 재시작 요청 이벤트
        EventHandler.Register(GlobalEventNameDefine.EVT_CMD_APPMENU_RESTART_TRAINING, OnCmdAppMenuRestartListener);

        //씬 페이드 인 / 아웃 요청 이벤트
        EventHandler.Register<float, Action>(GlobalEventNameDefine.EVT_REQ_SCENE_FADE_INOUT, OnReqSceneFadeInOutListener);
    }

    void UnRegEvent()
    {
        EventHandler.Unregister<KDDXDefine.GROUP>(GlobalEventNameDefine.EVT_REQ_SYSTEM_GROUP_CHANGE, OnReqSystemChangeListener);
        EventHandler.Unregister<TrainingDefine.KIND>(GlobalEventNameDefine.EVT_REQ_TRAINING_KIND_CHANGE, OnReqTrainingChangeListener);
        EventHandler.Unregister<KDDXDefine.KIND>(GlobalEventNameDefine.EVT_REQ_EQUIP_KIND_CHANGE, OnReqEquipChangeListener);

        //씬 변경 요청 이벤트
        EventHandler.Unregister<SceneDefine.FLAG>(GlobalEventNameDefine.EVT_REQ_SCENE_CHANGE, OnReqSceneChangeListener);

        //씬 재시작 요청 이벤트
        EventHandler.Unregister(GlobalEventNameDefine.EVT_CMD_APPMENU_RESTART_TRAINING, OnCmdAppMenuRestartListener);

        //씬 페이드 인 / 아웃 요청 이벤트
        EventHandler.Unregister<float, Action>(GlobalEventNameDefine.EVT_REQ_SCENE_FADE_INOUT, OnReqSceneFadeInOutListener);
    }

    #region Event Callback Function

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (coFadeIn != null)
        {
            StopCoroutine(coFadeIn);
            coFadeIn = null;
        }
        if (coFadeOut != null)
        {
            StopCoroutine(coFadeOut);
            coFadeOut = null;
        }

        coFadeOut = StartCoroutine(CoFadeOut(delaySecAtSceneChage, null));

    }

    private void OnReqSystemChangeListener(KDDXDefine.GROUP argKind)
    {
        if (selectSystem == argKind ||
            argKind == KDDXDefine.GROUP.NONE)
        {
            return;
        }

        selectSystem = argKind;

        EventHandler.Execute(GlobalEventNameDefine.EVT_RCV_SYSTEM_GROUP_CHANGE, selectSystem);
    }

    private void OnCmdAppMenuRestartListener()
    {
        SceneDefine.FLAG scene = SceneDefine.GetSceneNameToFlag(SceneManager.GetActiveScene().name);

        MoveScene(scene);
        EventHandler.Execute(GlobalEventNameDefine.EVT_RCV_SCENE_CHANGE, scene);
    }

    private void OnReqTrainingChangeListener(TrainingDefine.KIND argKind)
    {
        if (selectTraining == argKind ||
            argKind == TrainingDefine.KIND.NONE)
        {
            return;
        }

        selectTraining = argKind;

        EventHandler.Execute(GlobalEventNameDefine.EVT_RCV_TRAINING_KIND_CHANGE, selectTraining);
    }

    private void OnReqEquipChangeListener(KDDXDefine.KIND argKind)
    {
        if (selectEquip == argKind ||
            argKind == KDDXDefine.KIND.NONE)
        {
            return;
        }

        selectEquip = argKind;

        EventHandler.Execute(GlobalEventNameDefine.EVT_RCV_EQUIP_KIND_CHANGE, selectEquip);
    }

    private void OnReqSceneChangeListener(SceneDefine.FLAG argScene)
    {
        MoveScene(argScene);
        EventHandler.Execute(GlobalEventNameDefine.EVT_RCV_SCENE_CHANGE, argScene);
    }

    private void OnReqSceneFadeInOutListener(float argSec, Action cb)
    {
        if (sceneMoving)
        {
            return;
        }

        if (coFadeIn != null)
        {
            StopCoroutine(coFadeIn);
            coFadeIn = null;
        }
        if (coFadeOut != null)
        {
            StopCoroutine(coFadeOut);
            coFadeOut = null;
        }

        coFadeIn = StartCoroutine(CoFadeIn(argSec * 0.5f,
            () => {
                coFadeOut = StartCoroutine(CoFadeOut(argSec * 0.5f, cb));
            }));
    }

    #endregion

    #region Private Function
    /// <summary>
    /// 전역 볼퓸 객체 생성 함수.
    /// </summary>
    void CreateGlobalVolume()
    {
        if (globalVolume != null)
        {
            return;
        }

        if (globalVolumeObject == null)
        {
            Log.e("GlobalVolume is null !!!");
            return;
        }

        if (fadeInOutVolumeProfile == null)
        {
            Log.e("fadeInOutVolumeProfile is null !!!");
            return;
        }

        GameObject go = Instantiate(globalVolumeObject.gameObject, Vector3.zero, Quaternion.identity);
        DontDestroyOnLoad(go);

        globalVolume = go.GetComponent<Volume>();
    }

    /// <summary>
    /// 어플리케이션 메뉴 메니저 생성
    /// </summary>
    void CreateAppMenuManager()
    {
        if (AppMenuManager.isArrive)
        {
            return;
        }

        if (appMenuObject == null)
        {
            Log.e("appMenuObject is null !!!");
            return;
        }

        GameObject go = Instantiate(appMenuObject, Vector3.zero, Quaternion.identity);
        DontDestroyOnLoad(go);

    }

    IEnumerator CoFadeIn(float delaySec, Action cb)
    {
        if (mainCam == null)
        {
            mainCam = Camera.main.GetComponent<Camera>();
        }

        if (mainCam != null)
        {
            mainCam.GetUniversalAdditionalCameraData().renderPostProcessing = true;

            globalVolume.profile = fadeInOutVolumeProfile;

            ColorAdjustments ca;
            if (globalVolume.profile.TryGet(out ca))
            {
                float start = 0;
                float end = -20;
                float time = 0;

                ca.postExposure.value = Mathf.Lerp(start, end, time);

                while (ca.postExposure.value > end)
                {
                    ca.postExposure.value = Mathf.Lerp(start, end, time);

                    time += Time.deltaTime / delaySec;
                    yield return null;
                }

                ca.postExposure.value = end;
            }
        }

        cb?.Invoke();

        yield return null;
    }

    IEnumerator CoFadeOut(float delaySec, Action cb)
    {
        if (mainCam == null)
        {
            if (Camera.main.GetComponent<Camera>() == null) yield return null;
            else mainCam = Camera.main.GetComponent<Camera>();
        }

        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.Skybox;

            mainCam.GetUniversalAdditionalCameraData().renderPostProcessing = true;

            ColorAdjustments ca;
            if (globalVolume.profile.TryGet(out ca))
            {
                float start = -20;
                float end = 0;
                float time = 0;

                ca.postExposure.value = Mathf.Lerp(start, end, time);

                while (ca.postExposure.value < end)
                {
                    ca.postExposure.value = Mathf.Lerp(start, end, time);

                    time += Time.deltaTime / delaySec;
                    yield return null;
                }

                ca.postExposure.value = end;
            }


        }

        cb?.Invoke();
        yield return null;
    }

    void MoveScene(SceneDefine.FLAG argScene)
    {
        sceneMoving = true;
        actFadeRet = new Action(() => {

            sceneMoving = false;
            if (NetworkUIMng.Instance.me != null)
                NetworkUIMng.Instance.me.GetComponent<PlayerMe>().vivepointer.SetActive(true);
            HideAllHintOfHands();
            DetachAllObjectOfHands();
            EnableHandsHoverSphere();

            actLoadScene.Invoke(argScene);

        });
        //NarrationManager.Instance.StopNarration();

        if (NetworkUIMng.Instance.me != null)
            NetworkUIMng.Instance.me.GetComponent<PlayerMe>().vivepointer.SetActive(false);

        if (coFadeIn != null)
        {
            StopCoroutine(coFadeIn);
            coFadeIn = null;
        }
        if (coFadeOut != null)
        {
            StopCoroutine(coFadeOut);
            coFadeOut = null;
        }
        coFadeIn = StartCoroutine(CoFadeIn(delaySecAtSceneChage, actFadeRet));
    }


    #endregion


    #region Public Function

    float dist_X, dist_Y, dist_Z;
    public void SetPlayerPosition(Vector3 argPos)
    {
        if (!FindObjectOfType<Player>()) return;

        if (mainCam == null)
        {
            mainCam = Camera.main.GetComponent<Camera>();
        }

        dist_X = FindObjectOfType<Player>().transform.position.x - mainCam.transform.position.x;
        dist_Z = FindObjectOfType<Player>().transform.position.z - mainCam.transform.position.z;

        if (FindObjectOfType<Player>() == null)
        {
            return;
        }

        FindObjectOfType<Player>().transform.position = argPos + new Vector3(dist_X, dist_Y, dist_Z);
    }

    //Vector3 prevPos;
    //Player player = null;
    //private void Update()
    //{
    //    if (Player.instance.transform != null)
    //    {
    //        if (prevPos != Player.instance.transform.position)
    //        {
    //            //Debug.Log(string.Format("UPDATE => SCENE : {0} / PlayerPosition : {1}", SceneManager.GetActiveScene().name, Player.instance.transform.position));
    //            prevPos = Player.instance.transform.position;
    //        }
    //    }
    //}


    public void SetPlayerRotation(Quaternion argRot)
    {
        if (FindObjectOfType<Player>() == null)
        {
            return;
        }

        FindObjectOfType<Player>().transform.rotation = argRot;
    }


    public static void ShowController()
    {
        if (Player.instance == null || Player.instance.hands == null)
        {
            return;
        }

        for (int i = 0; i < Player.instance.hands.Length; i++)
        {
            Player.instance.hands[i].HideSkeleton(true);
            Player.instance.hands[i].ShowController(true);
        }
    }

    public static void ShowHands()
    {
        if (Player.instance == null || Player.instance.hands == null)
        {
            return;
        }

        for (int i = 0; i < Player.instance.hands.Length; i++)
        {
            Player.instance.hands[i].HideController(true);
            Player.instance.hands[i].ShowSkeleton(true);
        }
    }

    public static void DetachAllObjectOfHands()
    {
        if (Player.instance == null || Player.instance.hands == null)
        {
            return;
        }

        for (int i = 0; i < Player.instance.hands.Length; i++)
        {
            if (Player.instance.hands[i].currentAttachedObject != null)
            {
                Player.instance.hands[i].DetachObject(Player.instance.hands[i].currentAttachedObject);
            }
            Player.instance.hands[i].ShowSkeleton(true);
        }
    }

    public static bool IsSameAttachObjectOfHands(GameObject argObj)
    {
        if (Player.instance == null || Player.instance.hands == null)
        {
            return false;
        }

        if (argObj == null)
        {
            return false;
        }

        for (int i = 0; i < Player.instance.hands.Length; i++)
        {
            if (Player.instance.hands[i].currentAttachedObject != null)
            {
                if (Player.instance.hands[i].currentAttachedObject.Equals(argObj))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsSameAttachObjectOfHand(SteamVR_Input_Sources argFromSource, GameObject argObj)
    {
        if (Player.instance == null || Player.instance.hands == null)
        {
            return false;
        }

        if (argObj == null)
        {
            return false;
        }

        for (int i = 0; i < Player.instance.hands.Length; i++)
        {
            if (Player.instance.hands[i].currentAttachedObject != null)
            {
                if (Player.instance.hands[i].handType.Equals(argFromSource) && Player.instance.hands[i].currentAttachedObject.Equals(argObj))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static void VibrationSameAttachObjectOfHand(GameObject argObj, float secondsFromNow, float durationSeconds, float frequency, float amplitude)
    {
        if (Player.instance == null || Player.instance.hands == null)
        {
            return;
        }

        if (argObj == null)
        {
            return;
        }

        for (int i = 0; i < Player.instance.hands.Length; i++)
        {
            if (Player.instance.hands[i].currentAttachedObject != null)
            {
                Player.instance.hands[i].hapticAction.Execute(secondsFromNow, durationSeconds, frequency, amplitude, Player.instance.hands[i].handType);
                return;
            }
        }
    }

    public static void HideAllHintOfHands()
    {
        if (Player.instance == null || Player.instance.hands == null)
        {
            return;
        }

        for (int i = 0; i < Player.instance.hands.Length; i++)
        {
            ControllerButtonHints.HideAllTextHints(Player.instance.hands[i]);
        }
    }

    public static bool IsTwoGrabSameAttachObject()
    {
        if (Player.instance == null || Player.instance.hands == null)
        {
            return false;
        }

        if (Player.instance.rightHand == null || Player.instance.leftHand == null)
        {
            return false;
        }

        if (Player.instance.rightHand.currentAttachedObject == null || Player.instance.leftHand.currentAttachedObject == null)
        {
            return false;
        }

        return Player.instance.rightHand.currentAttachedObject.Equals(Player.instance.leftHand.currentAttachedObject);
    }

    public static GameObject GetTwoGrabSameAttachObject()
    {
        if (Player.instance == null || Player.instance.hands == null)
        {
            return null;
        }

        if (Player.instance.rightHand == null || Player.instance.leftHand == null)
        {
            return null;
        }

        if (Player.instance.rightHand.currentAttachedObject == null || Player.instance.leftHand.currentAttachedObject == null)
        {
            return null;
        }

        return Player.instance.rightHand.currentAttachedObject.Equals(Player.instance.leftHand.currentAttachedObject) ?
                Player.instance.rightHand.currentAttachedObject : null;
    }

    public static bool DisableHandsHoverSphere()
    {
        if (Player.instance == null || Player.instance.hands == null)
        {
            return false;
        }


        if (Player.instance.rightHand != null)
        {
            Player.instance.rightHand.useHoverSphere = false;
        }

        if (Player.instance.leftHand != null)
        {
            Player.instance.leftHand.useHoverSphere = false;
        }

        return true;
    }
    public static bool EnableHandsHoverSphere()
    {
        if (Player.instance == null || Player.instance.hands == null)
        {
            return false;
        }


        if (Player.instance.rightHand != null)
        {
            Player.instance.rightHand.useHoverSphere = true;
        }

        if (Player.instance.leftHand != null)
        {
            Player.instance.leftHand.useHoverSphere = true;
        }

        return true;
    }


    public static void EnableTeleport()
    {
        Teleport.instance?.gameObject.SetActive(true);
    }

    public static void DisableTeleport()
    {
        Teleport.instance?.gameObject.SetActive(false);
    }

    /// <summary>
    /// 플레이어 위치 강제이동 
    /// </summary>
    /// <param Position="tr"></param>
    /// <param Rotation="rot"></param>
    /// <param AfterMove="afterEvent"></param>
    public void PlayerMoveTransform(Vector3 tr, Quaternion rot, UnityEvent ac = null)
    {
        StartCoroutine(PlayerMovePos(tr, rot, ac));
    }

    IEnumerator PlayerMovePos(Vector3 tr, Quaternion rot, UnityEvent ac = null)
    {
        float fadeTime = 2.0f;

        if (ac != null)
        {
            ac.Invoke();
        }

        EventHandler.Execute(GlobalEventNameDefine.EVT_REQ_SCENE_FADE_INOUT, fadeTime, new Action(() =>
        {

        }));

        yield return new WaitForSeconds(1f);

        Player player = FindObjectOfType<Player>();
        if (player)
        {
            GameManager.Instance.SetPlayerPosition(tr);
            GameManager.Instance.SetPlayerRotation(rot);
        }
    }
    #endregion
}
