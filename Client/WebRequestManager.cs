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
    public string id; //����(ID)
    public string unit; //�Ҽ�
    public string position; //��å(����, ������..., ������)
    public string name; // �̸�
    public string password; //��й�ȣ, �������
    public string deletecode; //��������
    public string ranks; //���
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
    //���� ����� ������ IP�ּҷ� ���� �ʿ� 
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

    //������ ���� �ڱ� �����͸� ���´�
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
        //��񼳸��� ���γ��뿡 ������ ���ߵȴ�. 
        if (_operation == TrainingDefine.KIND.EQUIP_DESCRIPTION.ToString())
        {
            userhisData = new UserHisData(NetMng.Instance.MyAccountID, GroupSetting(_group), OperationSetting(_operation),
            _playLog, "", DetailOperationSetting_Description(_operationDetail));

            //Debug.Log(" _group : " + GroupSetting(_group) + " / _operation : " + OperationSetting(_operation) + " / _playLog : " + _playLog + " / _operationDetail : " + DetailOperationSetting_Description(_operationDetail));

            //������ �߰� 
            for (int i = 0; i < userAllDataList.Count; i++)
            {
                if (NetMng.Instance.MyAccountID == userAllDataList[i].userInfoData.id)
                {
                    //Debug.Log("������ �߰� �Ϸ� : " + NetMng.Instance.MyAccountID);
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

            //������ �߰� 
            for (int i = 0; i < userAllDataList.Count; i++)
            {
                if (NetMng.Instance.MyAccountID == userAllDataList[i].userInfoData.id)
                {
                    //Debug.Log("������ �߰� �Ϸ� : " + NetMng.Instance.MyAccountID);

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

        //������ ���� 
        TrainingManagementDataInsert(userhisData);
    }

    //�� ������ ��� 
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

        //������ ����
        TrainingManagementDataInsert(userhisData);

        //������ �߰� 
        for (int i = 0; i < userAllDataList.Count; i++)
        {
            if (NetMng.Instance.MyAccountID == userAllDataList[i].userInfoData.id)
            {
                //Debug.Log("������ �߰� �Ϸ� : " + NetMng.Instance.MyAccountID);

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
                groupSet = "��������ü��(CMS)";
                break;
            case "IM":
                groupSet = "���ո���Ʈ(IM)";
                break;
            case "MFR":
                groupSet = "�ٱ������迭���̴�(MFR)";
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
                operationSet = "��񼳸�";
                break;
            case "EQUIP_OPERATION":
                operationSet = "�����";
                break;
            case "SINGLE_UNIT_MAINTENANCE":
                operationSet = "����ǽ�";
                break;
            case "MULTI_UNIT_MAINTENANCE":
                operationSet = "����ǽ�";
                break;
            case "SINGLE_FIELD_MAINTENANCE":
                operationSet = "����ǽ�";
                break;
            case "MULTI_FIELD_MAINTENANCE":
                operationSet = "����ǽ�";
                break;
            case "TEST_UNIT_MAINTENANCE":
                operationSet = "����ǽ�";
                break;
            case "TEST_FIELD_MAINTENANCE":
                operationSet = "����ǽ�";
                break;

            default:
                Debug.Log("SKIP : " + _operation);
                break;

        }

        return operationSet;
    }
    //��񼳸��ΰ��
    string DetailOperationSetting_Description(string _operationDetail)
    {
        string operationDetailSet = "";

        switch (_operationDetail)
        {
            //CMS
            case "MULTI_CONSOLE":
                operationDetailSet = "�ٱ�� �ܼ�";
                break;
            case "EXINFO_CABINET":
                operationDetailSet = "��������ĳ���";
                break;
            case "TAINFO_CABINET":
                operationDetailSet = "��������ĳ���";
                break;
            case "INTE_LINK_CABINET_1":
                operationDetailSet = "���տ���ĳ��� 1";
                break;
            case "INTE_LINK_CABINET_2":
                operationDetailSet = "���տ���ĳ��� 2";
                break;
            case "INTE_LINK_CABINET_3":
                operationDetailSet = "���տ���ĳ��� 3";
                break;
            case "LINK_ANA_DEVICE":
                operationDetailSet = "�����м���ġ";
                break;
            case "LARGE_SCREEN_DISPLAY":
                operationDetailSet = "����ȭ�����ñ�";
                break;
            case "CONSOLE_DISPLAY":
                operationDetailSet = "�ֿܼ������ñ�";
                break;
            case "BRID_DISPLAY":
                operationDetailSet = "�Ա����ñ�";
                break;
            case "SYS_STATE_DISPLAY":
                operationDetailSet = "ü��������ñ�";
                break;

            //IM
            case "MAIN_STRUCTURE":
                operationDetailSet = "�ֱ�����";
                break;
            case "DEPART_EQUIP_STRUCTURE":
                operationDetailSet = "�������";
                break;
            case "INTE_COMM_ANTENAPART":
                operationDetailSet = "������ž��׳���";
                break;
            case "SUPERVISORY_CONTROL_DEVICE":
                operationDetailSet = "����������ġ";
                break;
            case "BEAM_SWITCHING_ASSEMBLY":
                operationDetailSet = "������Ī����ü";
                break;

            //MFR
            case "S_BAND_ANTENAPART":
                operationDetailSet = "S-�뿪 ���׳���";
                break;
            case "X_BAND_ANTENAPART":
                operationDetailSet = "X-�뿪 ���׳���";
                break;
            case "S_BAND_SIGNALPART":
                operationDetailSet = "S-�뿪 ��ȣó����";
                break;
            case "X_BAND_SIGNALPART":
                operationDetailSet = "X-�뿪 ��ȣó����";
                break;
            case "INTE_CONTROLPART":
                operationDetailSet = "����������";
                break;
            case "POWER_SUP_PART":
                operationDetailSet = "�������޺�";
                break;
            case "ANTENA_COOLINGPART":
                operationDetailSet = "���׳��ð���";
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

    //���������ΰ�� 
    string DetailOperationSetting(OPERMODE operMode, string _operationDetail)
    {
        string operationDetailSet = "";
        switch (_operationDetail)
        {
            //MFR
            case "POWER_SUPPLY_REPLACEMENT":
                if(operMode == OPERMODE.MULTI)
                    operationDetailSet = "2��1�� ����ǽ�/S-�뿪 ���׳���/����������ġ ��ȯ";
                else if (operMode == OPERMODE.TEST)
                    operationDetailSet = "����ǽ� ����/S-�뿪 ���׳���/����������ġ ��ȯ";
                else if (operMode == OPERMODE.SINGLE)
                    operationDetailSet = "���� ����ǽ�/S-�뿪 ���׳���/����������ġ ��ȯ";
                break;

            case "SEMICONDUCTOR_TRANSMISSION_MODULE":
                if (operMode == OPERMODE.MULTI)
                    operationDetailSet = "2��1�� ����ǽ�/X-�뿪 ���׳���/�ݵ�ü�ۼ��Ÿ�� ��ȯ";
                else if (operMode == OPERMODE.TEST)
                    operationDetailSet = "����ǽ� ����/X-�뿪 ���׳���/�ݵ�ü�ۼ��Ÿ�� ��ȯ";
                else if (operMode == OPERMODE.SINGLE)
                    operationDetailSet = "���� ����ǽ�/X-�뿪 ���׳���/�ݵ�ü�ۼ��Ÿ�� ��ȯ";
                break;

            //CMS
            case "COMBAT_SYSTEM_POWERSUPPLY":
                if (operMode == OPERMODE.OPER)
                    operationDetailSet = "����ü�� ���� ����";
                break;

            case "COMBAT_SYSTEM_POWERBLOCK":
                if (operMode == OPERMODE.OPER)
                    operationDetailSet = "����ü�� ���� ����";
                break;

            case "POWER_COMPENSATIONMODULE_REPLACE":
                if (operMode == OPERMODE.MULTI)
                    operationDetailSet = "2��1�� ����ǽ�/�ٱ�� �ܼ�/���������� ��ȯ";
                else if (operMode == OPERMODE.TEST)
                    operationDetailSet = "����ǽ� ����/�ٱ�� �ܼ�/���������� ��ȯ";
                else if (operMode == OPERMODE.SINGLE)
                    operationDetailSet = "���� ����ǽ�/�ٱ�� �ܼ�/���������� ��ȯ";
                break;
            case "POWER_FILTER_ASSEMBLE_REPLACE":
                if (operMode == OPERMODE.MULTI)
                    operationDetailSet = "2��1�� ����ǽ�/�ٱ�� �ܼ�/������������ü ��ȯ";
                else if (operMode == OPERMODE.TEST)
                    operationDetailSet = "����ǽ� ����/�ٱ�� �ܼ�/������������ü ��ȯ";
                else if (operMode == OPERMODE.SINGLE)
                    operationDetailSet = "���� ����ǽ�/�ٱ�� �ܼ�/������������ü ��ȯ";
                break;
            case "POWER_CONTROL_MODULE_REPLACE":
                if (operMode == OPERMODE.MULTI)
                    operationDetailSet = "2��1�� ����ǽ�/��������ĳ���/���������� ��ȯ";
                else if (operMode == OPERMODE.TEST)
                    operationDetailSet = "����ǽ� ����/��������ĳ���/���������� ��ȯ";
                else if (operMode == OPERMODE.SINGLE)
                    operationDetailSet = "���� ����ǽ�/��������ĳ���/���������� ��ȯ";
                break;

            //IM
            case "HORIZONTAL_DISTRIBUTIONCAP_REPLACE":
                if (operMode == OPERMODE.MULTI)
                    operationDetailSet = "2��1�� ����ǽ�/������Ī����ü/����й��� ��ȯ";
                else if (operMode == OPERMODE.TEST)
                    operationDetailSet = "����ǽ� ����/������Ī����ü/����й��� ��ȯ";
                else if (operMode == OPERMODE.SINGLE)
                    operationDetailSet = "���� ����ǽ�/������Ī����ü/����й��� ��ȯ";
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

    //POST ��� (SELECT) - user
    IEnumerator UserDataSelect_Cor(UserInfoData uid, string path)
    {
        WWWForm form = GetDataUserInfo(uid);

        UnityWebRequest www = UnityWebRequest.Post(url + path, form);

        yield return www.SendWebRequest();  // ������ �ö����� ����Ѵ�.

        if (www.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log(www.downloadHandler.text);

            userInfoDataList = JsonUtility.FromJson<UserInfoDataList>(www.downloadHandler.text);

            Debug.Log("============USER INFO SUCCESS============");

            if (userInfoDataList == null) Debug.Log("NULL");
            else
            {
                //ȸ������ his ���� userAllDataList�� ���� 
                for (int i = 0; i < userInfoDataList.list.Count; i++)
                {
                    if (userhisData != null) userhisData = null;
                    userhisData = new UserHisData(userInfoDataList.list[i].id, "", "", "", "", "");

                    yield return StartCoroutine(GetTrainingManagementData_Cor(userhisData, "/server/getTrainingManagementData.do"));

                    //������ ������ ������ 
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

    //POST ��� (INSERT)
    IEnumerator TrainingManagementDataInsert_Cor(UserHisData uhd, string path)
    {
        WWWForm form = GetDataUserHis(uhd);

        UnityWebRequest www = UnityWebRequest.Post(url + path, form);  // ���� �ּҿ� ������ �Է�

        yield return www.SendWebRequest();  // ���� ���

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            Debug.Log("TrainingManagementDataInsert_Cor ERROR !!!");
        }
    }

    bool trigger = false; //���� his ������ �������� �ð� üũ 
    //POST ��� (SELECT) - userhis
    IEnumerator GetTrainingManagementData_Cor(UserHisData uhd, string path)
    {
        WWWForm form = GetDataUserHis(uhd);

        UnityWebRequest www = UnityWebRequest.Post(url + path, form);  // ���� �ּҿ� ������ �Է�

        yield return www.SendWebRequest();  // ���� ���

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

    //�α����� ȸ�� ��� ���� (������ ���� �ڱ� ��ϸ� ��´�)
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
