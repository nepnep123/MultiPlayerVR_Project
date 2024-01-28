using LitJson;
using QS.Global.Define;
using QS.Global.Pattern;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using BestHTTP;
using System.Text;
using System;

[System.Serializable]
public class UserInfoData
{
    public string id; //군번(ID)
    public string unit; //소속
    public string position; //직책(교관, 교육생..., 마스터)
    public string name; // 이름
    public string password; //비밀번호, 생년월일
    public string deletecode; //삭제여부
    public string ranks; //계급
    public string token;
    public string expiretoken;

    public UserInfoData(string _id, string _unit, string _position,
        string _name, string _password, string _deletecode, string _ranks, string _token,
        string _expiretoken)
    {
        id = _id;
        unit = _unit;
        position = _position;
        name = _name;
        password = _password;
        deletecode = _deletecode;
        ranks = _ranks;
        token = _token;
        expiretoken = _expiretoken;
    }
}

[System.Serializable]
public class UserHisData
{
    public string userID;
    public string subsystem;
    public string operation;
    public string playLog;
    public string testScore;
    public string detailedcontents;

    public UserHisData(string _userID, string _subsystem, string _operation,
        string _playLog, string _testScore, string _detailedcontents)
    {
        userID = _userID;
        subsystem = _subsystem;
        operation = _operation;
        playLog = _playLog;
        testScore = _testScore;
        detailedcontents = _detailedcontents;
    }
}

[System.Serializable]
public class UserInfoDataList
{
    public List<UserInfoData> list = new List<UserInfoData>();
}

[System.Serializable]
public class UserHisDataList
{
    //public string result;
    public List<UserHisData> dataList = new List<UserHisData>();
}

[System.Serializable] 
public class UserFullData
{
    public UserInfoData userInfoData;
    public List<UserHisData> userHisDatas;

    public UserFullData(UserInfoData _userInfoData, UserHisData[] _userHisDatas)
    {
        this.userInfoData = _userInfoData;

        userHisDatas = new List<UserHisData>();
        userHisDatas.AddRange(_userHisDatas);
    }
}

public class WebRequestManager : Singleton<WebRequestManager>
{
    //폐쇠망 연결시 서버망 IP주소로 수정 필요 
    string url = "http://192.168.1.225:8190";
    //string url = "http://192.168.1.134:8190";


    //string url = "http://192.168.0.51:8190";

    //SEND POST DATA
    UserInfoData userInfoData;
    UserHisData userhisData;

    //GET ALL DATA
    UserInfoDataList userInfoDataList;
    UserHisDataList userhisDataList;

    //ALL USER DATA
    [SerializeField] public List<UserFullData> userAllDataList = new List<UserFullData>();

    //관리자 또한 자기 데이터만 갖는다
    //CURRENT LOGIN USER INFO DATA
    [SerializeField] public UserInfoData current_userInfo;
    //CURRENT LOGIN USER HIS DATA
    [SerializeField] public List<UserHisData> current_userHis = new List<UserHisData>();

    bool isMaster = false; //IF MASTER LOGIN 
    public bool IsMaster { get { return isMaster; } set { isMaster = value; } }

    private void Start()
    {
        userInfoData = new UserInfoData("", "", "", "", "", "", "", "", "");
        UserDataSelect(userInfoData);
    }

    #region PUBLIC FUNCTION
    public void SetUserHisData(string _group, string _operation, string _playLog, string _operationDetail)
    {
        //장비설명은 세부내용에 장비명이 들어가야된다. 
        if (_operation == TrainingDefine.KIND.EQUIP_DESCRIPTION.ToString())
        {
            userhisData = new UserHisData(NetMng.Instance.MyAccountID, GroupSetting(_group), OperationSetting(_operation),
            _playLog, "", DetailOperationSetting_Description(_operationDetail));

            //Debug.Log(" _group : " + GroupSetting(_group) + " / _operation : " + OperationSetting(_operation) + " / _playLog : " + _playLog + " / _operationDetail : " + DetailOperationSetting_Description(_operationDetail));

            //데이터 추가 
            for (int i = 0; i < userAllDataList.Count; i++)
            {
                if (NetMng.Instance.MyAccountID == userAllDataList[i].userInfoData.id)
                {
                    //Debug.Log("데이터 추가 완료 : " + NetMng.Instance.MyAccountID);
                    userAllDataList[i].userHisDatas.Add(new UserHisData(NetMng.Instance.MyAccountID, GroupSetting(_group), OperationSetting(_operation),
                        _playLog, "", DetailOperationSetting_Description(_operationDetail)));
                }
            }
        }
        else
        {
            if (_operation == TrainingDefine.KIND.MULTI_FIELD_MAINTENANCE.ToString() || _operation == TrainingDefine.KIND.MULTI_UNIT_MAINTENANCE.ToString())
            {
                userhisData = new UserHisData(NetMng.Instance.MyAccountID, GroupSetting(_group), OperationSetting(_operation),
                    _playLog, "", DetailOperationSetting(OPERMODE.MULTI, _operationDetail));
            }
            else if (_operation == TrainingDefine.KIND.SINGLE_FIELD_MAINTENANCE.ToString() || _operation == TrainingDefine.KIND.SINGLE_UNIT_MAINTENANCE.ToString())
            {
                userhisData = new UserHisData(NetMng.Instance.MyAccountID, GroupSetting(_group), OperationSetting(_operation),
                    _playLog, "", DetailOperationSetting(OPERMODE.SINGLE, _operationDetail));
            }
            else if (_operation == TrainingDefine.KIND.TEST_FIELD_MAINTENANCE.ToString() || _operation == TrainingDefine.KIND.TEST_UNIT_MAINTENANCE.ToString())
            {
                userhisData = new UserHisData(NetMng.Instance.MyAccountID, GroupSetting(_group), OperationSetting(_operation),
                    _playLog, "", DetailOperationSetting(OPERMODE.TEST, _operationDetail));
            }
            else if (_operation == TrainingDefine.KIND.EQUIP_OPERATION.ToString())
            {
                userhisData = new UserHisData(NetMng.Instance.MyAccountID, GroupSetting(_group), OperationSetting(_operation),
                    _playLog, "", DetailOperationSetting(OPERMODE.OPER, _operationDetail));
            }

            //Debug.Log(" _group : " + GroupSetting(_group) + " / _operation : " + OperationSetting(_operation) + " / _playLog : " + _playLog + " / _operationDetail : " + DetailOperationSetting(_operationDetail));

            //데이터 추가 
            for (int i = 0; i < userAllDataList.Count; i++)
            {
                if (NetMng.Instance.MyAccountID == userAllDataList[i].userInfoData.id)
                {
                    //Debug.Log("데이터 추가 완료 : " + NetMng.Instance.MyAccountID);

                    if (_operation == TrainingDefine.KIND.MULTI_FIELD_MAINTENANCE.ToString() || _operation == TrainingDefine.KIND.MULTI_UNIT_MAINTENANCE.ToString())
                    {
                        userAllDataList[i].userHisDatas.Add(new UserHisData(NetMng.Instance.MyAccountID, GroupSetting(_group), OperationSetting(_operation),
                            _playLog, "", DetailOperationSetting(OPERMODE.MULTI, _operationDetail)));
                    }
                    else if (_operation == TrainingDefine.KIND.SINGLE_FIELD_MAINTENANCE.ToString() || _operation == TrainingDefine.KIND.SINGLE_UNIT_MAINTENANCE.ToString())
                    {
                        userAllDataList[i].userHisDatas.Add(new UserHisData(NetMng.Instance.MyAccountID, GroupSetting(_group), OperationSetting(_operation),
                            _playLog, "", DetailOperationSetting(OPERMODE.SINGLE, _operationDetail)));
                    }
                    else if (_operation == TrainingDefine.KIND.TEST_FIELD_MAINTENANCE.ToString() || _operation == TrainingDefine.KIND.TEST_UNIT_MAINTENANCE.ToString())
                    {
                        userAllDataList[i].userHisDatas.Add(new UserHisData(NetMng.Instance.MyAccountID, GroupSetting(_group), OperationSetting(_operation),
                            _playLog, "", DetailOperationSetting(OPERMODE.TEST, _operationDetail)));
                    }
                    else if (_operation == TrainingDefine.KIND.EQUIP_OPERATION.ToString())
                    {
                        userAllDataList[i].userHisDatas.Add(new UserHisData(NetMng.Instance.MyAccountID, GroupSetting(_group), OperationSetting(_operation),
                            _playLog, "", DetailOperationSetting(OPERMODE.OPER, _operationDetail)));
                    }
                }
            }
        }

        //서버에 전송 
        TrainingManagementDataInsert(userhisData);
    }

    //평가 버전인 경우 
    public void SetUserHisData_WithScore(string _group, string _operation, string _playLog, string _totalScore, string _operationDetail)
    {
        if (_operation == TrainingDefine.KIND.MULTI_FIELD_MAINTENANCE.ToString() || _operation == TrainingDefine.KIND.MULTI_UNIT_MAINTENANCE.ToString())
        {
            userhisData = new UserHisData(NetMng.Instance.MyAccountID, GroupSetting(_group), OperationSetting(_operation),
                _playLog, _totalScore, DetailOperationSetting(OPERMODE.MULTI, _operationDetail));
        }
        else if (_operation == TrainingDefine.KIND.SINGLE_FIELD_MAINTENANCE.ToString() || _operation == TrainingDefine.KIND.SINGLE_UNIT_MAINTENANCE.ToString())
        {
            userhisData = new UserHisData(NetMng.Instance.MyAccountID, GroupSetting(_group), OperationSetting(_operation),
                _playLog, _totalScore, DetailOperationSetting(OPERMODE.SINGLE, _operationDetail));
        }
        else if (_operation == TrainingDefine.KIND.TEST_FIELD_MAINTENANCE.ToString() || _operation == TrainingDefine.KIND.TEST_UNIT_MAINTENANCE.ToString())
        {
            userhisData = new UserHisData(NetMng.Instance.MyAccountID, GroupSetting(_group), OperationSetting(_operation),
                _playLog, _totalScore, DetailOperationSetting(OPERMODE.TEST, _operationDetail));
        }
        else if (_operation == TrainingDefine.KIND.EQUIP_OPERATION.ToString())
        {
            userhisData = new UserHisData(NetMng.Instance.MyAccountID, GroupSetting(_group), OperationSetting(_operation),
                _playLog, _totalScore, DetailOperationSetting(OPERMODE.OPER, _operationDetail));
        }

        //Debug.Log(" _group : " + GroupSetting(_group) + " / _operation : " + OperationSetting(_operation) + " / _playLog : " + _playLog + " / _totalScore : " + _totalScore
        //    + " / _operationDetail : " + DetailOperationSetting(_operationDetail));

        //서버에 전송
        TrainingManagementDataInsert(userhisData);

        //데이터 추가 
        for (int i = 0; i < userAllDataList.Count; i++)
        {
            if (NetMng.Instance.MyAccountID == userAllDataList[i].userInfoData.id)
            {
                //Debug.Log("데이터 추가 완료 : " + NetMng.Instance.MyAccountID);

                if (_operation == TrainingDefine.KIND.MULTI_FIELD_MAINTENANCE.ToString() || _operation == TrainingDefine.KIND.MULTI_UNIT_MAINTENANCE.ToString())
                {
                    userAllDataList[i].userHisDatas.Add(new UserHisData(NetMng.Instance.MyAccountID, GroupSetting(_group), OperationSetting(_operation),
                        _playLog, _totalScore, DetailOperationSetting(OPERMODE.MULTI, _operationDetail)));
                }
                else if (_operation == TrainingDefine.KIND.SINGLE_FIELD_MAINTENANCE.ToString() || _operation == TrainingDefine.KIND.SINGLE_UNIT_MAINTENANCE.ToString())
                {
                    userAllDataList[i].userHisDatas.Add(new UserHisData(NetMng.Instance.MyAccountID, GroupSetting(_group), OperationSetting(_operation),
                        _playLog, _totalScore, DetailOperationSetting(OPERMODE.SINGLE, _operationDetail)));
                }
                else if (_operation == TrainingDefine.KIND.TEST_FIELD_MAINTENANCE.ToString() || _operation == TrainingDefine.KIND.TEST_UNIT_MAINTENANCE.ToString())
                {
                    userAllDataList[i].userHisDatas.Add(new UserHisData(NetMng.Instance.MyAccountID, GroupSetting(_group), OperationSetting(_operation),
                        _playLog, _totalScore, DetailOperationSetting(OPERMODE.TEST, _operationDetail)));
                }
                else if (_operation == TrainingDefine.KIND.EQUIP_OPERATION.ToString())
                {
                    userAllDataList[i].userHisDatas.Add(new UserHisData(NetMng.Instance.MyAccountID, GroupSetting(_group), OperationSetting(_operation),
                        _playLog, _totalScore, DetailOperationSetting(OPERMODE.OPER, _operationDetail)));
                }

            }
        }
    }

    #endregion

    #region STRING CHECK
    string GroupSetting(string _group)
    {
        string groupSet = "";

        switch (_group)
        {
            case "CMS":
                groupSet = "전투관리체계(CMS)";
                break;
            case "IM":
                groupSet = "통합마스트(IM)";
                break;
            case "MFR":
                groupSet = "다기능위상배열레이더(MFR)";
                break;

            default:
                Debug.Log("SKIP : " + _group);
                break;
        }

        return groupSet;
    }

    string OperationSetting(string _operation)
    {
        string operationSet = "";

        switch (_operation)
        {
            case "EQUIP_DESCRIPTION":
                operationSet = "장비설명";
                break;
            case "EQUIP_OPERATION":
                operationSet = "장비운용";
                break;
            case "SINGLE_UNIT_MAINTENANCE":
                operationSet = "정비실습";
                break;
            case "MULTI_UNIT_MAINTENANCE":
                operationSet = "정비실습";
                break;
            case "SINGLE_FIELD_MAINTENANCE":
                operationSet = "정비실습";
                break;
            case "MULTI_FIELD_MAINTENANCE":
                operationSet = "정비실습";
                break;
            case "TEST_UNIT_MAINTENANCE":
                operationSet = "정비실습";
                break;
            case "TEST_FIELD_MAINTENANCE":
                operationSet = "정비실습";
                break;

            default:
                Debug.Log("SKIP : " + _operation);
                break;

        }

        return operationSet;
    }
    //장비설명인경우
    string DetailOperationSetting_Description(string _operationDetail)
    {
        string operationDetailSet = "";

        switch (_operationDetail)
        {
            //CMS
            case "MULTI_CONSOLE":
                operationDetailSet = "다기능 콘솔";
                break;
            case "EXINFO_CABINET":
                operationDetailSet = "전시정보캐비닛";
                break;
            case "TAINFO_CABINET":
                operationDetailSet = "전술정보캐비닛";
                break;
            case "INTE_LINK_CABINET_1":
                operationDetailSet = "통합연동캐비닛 1";
                break;
            case "INTE_LINK_CABINET_2":
                operationDetailSet = "통합연동캐비닛 2";
                break;
            case "INTE_LINK_CABINET_3":
                operationDetailSet = "통합연동캐비닛 3";
                break;
            case "LINK_ANA_DEVICE":
                operationDetailSet = "연동분석장치";
                break;
            case "LARGE_SCREEN_DISPLAY":
                operationDetailSet = "대형화면전시기";
                break;
            case "CONSOLE_DISPLAY":
                operationDetailSet = "콘솔원격전시기";
                break;
            case "BRID_DISPLAY":
                operationDetailSet = "함교전시기";
                break;
            case "SYS_STATE_DISPLAY":
                operationDetailSet = "체계상태전시기";
                break;

            //IM
            case "MAIN_STRUCTURE":
                operationDetailSet = "주구조부";
                break;
            case "DEPART_EQUIP_STRUCTURE":
                operationDetailSet = "기반장비부";
                break;
            case "INTE_COMM_ANTENAPART":
                operationDetailSet = "통합통신안테나부";
                break;
            case "SUPERVISORY_CONTROL_DEVICE":
                operationDetailSet = "감시제어장치";
                break;
            case "BEAM_SWITCHING_ASSEMBLY":
                operationDetailSet = "빔스위칭조립체";
                break;

            //MFR
            case "S_BAND_ANTENAPART":
                operationDetailSet = "S-대역 안테나부";
                break;
            case "X_BAND_ANTENAPART":
                operationDetailSet = "X-대역 안테나부";
                break;
            case "S_BAND_SIGNALPART":
                operationDetailSet = "S-대역 신호처리부";
                break;
            case "X_BAND_SIGNALPART":
                operationDetailSet = "X-대역 신호처리부";
                break;
            case "INTE_CONTROLPART":
                operationDetailSet = "통합통제부";
                break;
            case "POWER_SUP_PART":
                operationDetailSet = "전원공급부";
                break;
            case "ANTENA_COOLINGPART":
                operationDetailSet = "안테나냉각부";
                break;

            default:
                Debug.Log("SKIP : " + _operationDetail);
                break;
        }

        return operationDetailSet;
    }

    public enum OPERMODE
    {
        MULTI,
        TEST,
        SINGLE, 
        OPER
    };

    //정비절차인경우 
    string DetailOperationSetting(OPERMODE operMode, string _operationDetail)
    {
        string operationDetailSet = "";
        switch (_operationDetail)
        {
            //MFR
            case "POWER_SUPPLY_REPLACEMENT":
                if(operMode == OPERMODE.MULTI)
                    operationDetailSet = "2인1조 정비실습/S-대역 안테나부/전원공급장치 교환";
                else if (operMode == OPERMODE.TEST)
                    operationDetailSet = "정비실습 진단/S-대역 안테나부/전원공급장치 교환";
                else if (operMode == OPERMODE.SINGLE)
                    operationDetailSet = "개인 정비실습/S-대역 안테나부/전원공급장치 교환";
                break;

            case "SEMICONDUCTOR_TRANSMISSION_MODULE":
                if (operMode == OPERMODE.MULTI)
                    operationDetailSet = "2인1조 정비실습/X-대역 안테나부/반도체송수신모듈 교환";
                else if (operMode == OPERMODE.TEST)
                    operationDetailSet = "정비실습 진단/X-대역 안테나부/반도체송수신모듈 교환";
                else if (operMode == OPERMODE.SINGLE)
                    operationDetailSet = "개인 정비실습/X-대역 안테나부/반도체송수신모듈 교환";
                break;

            //CMS
            case "COMBAT_SYSTEM_POWERSUPPLY":
                if (operMode == OPERMODE.OPER)
                    operationDetailSet = "전투체계 전원 공급";
                break;

            case "COMBAT_SYSTEM_POWERBLOCK":
                if (operMode == OPERMODE.OPER)
                    operationDetailSet = "전투체계 전원 차단";
                break;

            case "POWER_COMPENSATIONMODULE_REPLACE":
                if (operMode == OPERMODE.MULTI)
                    operationDetailSet = "2인1조 정비실습/다기능 콘솔/전원보상모듈 교환";
                else if (operMode == OPERMODE.TEST)
                    operationDetailSet = "정비실습 진단/다기능 콘솔/전원보상모듈 교환";
                else if (operMode == OPERMODE.SINGLE)
                    operationDetailSet = "개인 정비실습/다기능 콘솔/전원보상모듈 교환";
                break;
            case "POWER_FILTER_ASSEMBLE_REPLACE":
                if (operMode == OPERMODE.MULTI)
                    operationDetailSet = "2인1조 정비실습/다기능 콘솔/전원필터조립체 교환";
                else if (operMode == OPERMODE.TEST)
                    operationDetailSet = "정비실습 진단/다기능 콘솔/전원필터조립체 교환";
                else if (operMode == OPERMODE.SINGLE)
                    operationDetailSet = "개인 정비실습/다기능 콘솔/전원필터조립체 교환";
                break;
            case "POWER_CONTROL_MODULE_REPLACE":
                if (operMode == OPERMODE.MULTI)
                    operationDetailSet = "2인1조 정비실습/전시정보캐비닛/전원제어모듈 교환";
                else if (operMode == OPERMODE.TEST)
                    operationDetailSet = "정비실습 진단/전시정보캐비닛/전원제어모듈 교환";
                else if (operMode == OPERMODE.SINGLE)
                    operationDetailSet = "개인 정비실습/전시정보캐비닛/전원제어모듈 교환";
                break;

            //IM
            case "HORIZONTAL_DISTRIBUTIONCAP_REPLACE":
                if (operMode == OPERMODE.MULTI)
                    operationDetailSet = "2인1조 정비실습/빔스위칭조립체/수평분배모듈 교환";
                else if (operMode == OPERMODE.TEST)
                    operationDetailSet = "정비실습 진단/빔스위칭조립체/수평분배모듈 교환";
                else if (operMode == OPERMODE.SINGLE)
                    operationDetailSet = "개인 정비실습/빔스위칭조립체/수평분배모듈 교환";
                break;


            default:
                Debug.Log("SKIP : " + _operationDetail);
                break;
        }

        return operationDetailSet;
    }

    #endregion

    #region URL FUNCTION
    void TrainingManagementDataInsert(UserHisData uhd)
    {
        StartCoroutine(TrainingManagementDataInsert_Cor(uhd, "/server/trainingManagementDataInsert.do"));
    }

    void UserDataSelect(UserInfoData uid)
    {
        StartCoroutine(UserDataSelect_Cor(uid, "/server/userDataSelect.do"));
    }

    #endregion

    WWWForm GetDataUserHis(UserHisData uhd)
    {
        WWWForm form = new WWWForm();

        form.AddField("userID", uhd.userID);
        form.AddField("subsystem", uhd.subsystem);
        form.AddField("operation", uhd.operation);
        form.AddField("playLog", uhd.playLog);
        form.AddField("testScore", uhd.testScore);
        form.AddField("detailedcontents", uhd.detailedcontents);

        return form;
    }

    WWWForm GetDataUserInfo(UserInfoData uid)
    {
        WWWForm form = new WWWForm();

        form.AddField("id", uid.id);
        form.AddField("unit", uid.unit);
        form.AddField("position", uid.position);
        form.AddField("name", uid.name);
        form.AddField("password", uid.password);
        form.AddField("deletecode", uid.deletecode);
        form.AddField("ranks", uid.ranks);
        form.AddField("token", uid.token);
        form.AddField("expiretoken", uid.expiretoken);
        return form;
    }

    //POST 방식 (SELECT) - user
    IEnumerator UserDataSelect_Cor(UserInfoData uid, string path)
    {
        WWWForm form = GetDataUserInfo(uid);

        UnityWebRequest www = UnityWebRequest.Post(url + path, form);

        yield return www.SendWebRequest();  // 응답이 올때까지 대기한다.

        if (www.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log(www.downloadHandler.text);

            userInfoDataList = JsonUtility.FromJson<UserInfoDataList>(www.downloadHandler.text);

            Debug.Log("============USER INFO SUCCESS============");

            if (userInfoDataList == null) Debug.Log("NULL");
            else
            {
                //회원마다 his 정보 userAllDataList에 저장 
                for (int i = 0; i < userInfoDataList.list.Count; i++)
                {
                    if (userhisData != null) userhisData = null;
                    userhisData = new UserHisData(userInfoDataList.list[i].id, "", "", "", "", "");

                    yield return StartCoroutine(GetTrainingManagementData_Cor(userhisData, "/server/getTrainingManagementData.do"));

                    //위에서 가져온 데이터 
                    if (userhisDataList == null)
                    {
                        Debug.Log("USER : " + userInfoDataList.list[i].id + " NO HISTORY !!!");
                    }
                    else
                    {
                        userAllDataList.Add(new UserFullData(userInfoDataList.list[i], userhisDataList.dataList.ToArray()));
                    }
                }
            }
        }
        else
        {
            Debug.Log("UserDataSelect_Cor ERROR");
        }
    }

    //POST 방식 (INSERT)
    IEnumerator TrainingManagementDataInsert_Cor(UserHisData uhd, string path)
    {
        WWWForm form = GetDataUserHis(uhd);

        UnityWebRequest www = UnityWebRequest.Post(url + path, form);  // 보낼 주소와 데이터 입력

        yield return www.SendWebRequest();  // 응답 대기

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            Debug.Log("TrainingManagementDataInsert_Cor ERROR !!!");
        }
    }

    bool trigger = false; //유저 his 정보를 가져오는 시간 체크 
    //POST 방식 (SELECT) - userhis
    IEnumerator GetTrainingManagementData_Cor(UserHisData uhd, string path)
    {
        WWWForm form = GetDataUserHis(uhd);

        UnityWebRequest www = UnityWebRequest.Post(url + path, form);  // 보낼 주소와 데이터 입력

        yield return www.SendWebRequest();  // 응답 대기

        if (www.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log(www.downloadHandler.text);

            userhisDataList = JsonUtility.FromJson<UserHisDataList>(www.downloadHandler.text);

            Debug.Log("============USER HIS SUCCESS============");
            trigger = false;

            if (userhisDataList == null)
                Debug.Log("NULL");
            else
            {
                //foreach (UserHisData item in userhisDataList.dataList)
                //{
                //    Debug.Log("-------------------------------");
                //    Debug.Log(item.userID + " / " + item.subsystem + " / " + item.operation + " / " + item.detailedcontents);
                //}
            }

            trigger = true;

            //if (userhisDataList != null)
            //    Debug.Log("userhisDataList : " + userhisDataList.result);
        }
        else
        {
            Debug.Log("GetTrainingManagementData_Cor ERROR !!!");
        }
    }

    //로그인한 회원 기록 저장 (관리자 또한 자기 기록만 담는다)
    public void CurrentUserHisDataSet(string id)
    {
        if (current_userHis != null) current_userHis = null;

        for (int i = 0; i < userAllDataList.Count; i++)
        {
            if(userAllDataList[i].userInfoData.id == id)
            {
                if(userAllDataList[i].userHisDatas.Count != 0)
                {
                    current_userHis = userAllDataList[i].userHisDatas;
                    //Debug.Log("LOGIN USER HIS DATA LIST : " + current_userHis[i].userID);
                }
                else
                {
                    return;
                }

                break;
            }
        }
    }
}
