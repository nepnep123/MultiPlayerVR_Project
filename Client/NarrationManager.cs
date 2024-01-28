using UnityEngine;
using UnityEngine.Events;
using QS.Global.Define;
using QS.Global.Pattern;
using System;
using System.Collections;
public class NarrationManager : Singleton<NarrationManager>
{

    #region Variables

    AudioClip selectClip;
    AudioSource _as;

    DateTime stime = DateTime.MinValue;
    #endregion


    #region Property

    /// <summary>
    /// 오디오 재생 상태 반환
    /// </summary>
    public bool IsPlaying
    {
        get
        {
            //오디오 소스,클립이 없을 경우 
            if (_as == null || selectClip == null)
            {
                return false;
                Log.d("IsPlaying false");
            }

            //PlayNarration 하지 않았을 경우
            if (stime == DateTime.MinValue)
            {
                return false;
                Log.d("IsPlaying false");
            }

            //PlayNarration 즉시 체크 시
            TimeSpan ts = DateTime.Now - stime;
            if (ts.TotalSeconds <= 1f)
            {
                return true;
            }

            float totalTime = GetAudioClipLength();
            if (totalTime == 0 || totalTime.Equals(_as.time) || _as.time == 0)
            {
                return false;
                Log.d("IsPlaying false");
            }

            return true;
        }
    }

    #endregion

    private void Start()
    {
        RegEvent();
    }

    void RegEvent()
    {
        EventHandler.Register<SceneDefine.FLAG>(GlobalEventNameDefine.EVT_RCV_SCENE_CHANGE, OnRcvSceneChangeListener);
    }

    private void OnRcvSceneChangeListener(SceneDefine.FLAG obj)
    {

    }

    void UnRegEvent()
    {
        EventHandler.Unregister<SceneDefine.FLAG>(GlobalEventNameDefine.EVT_RCV_SCENE_CHANGE, OnRcvSceneChangeListener);
    }


    /// <summary>
    /// 해당 Scene Flag, 오디오 소스 파일명을 이용해 나레이션 오디오 재생
    /// </summary>
    public void PlayNarration(SceneDefine.FLAG _flag, string _clipName, AudioClip ownClip = null)
    {
        //if (_clipName == "") return;

        if (_as == null)
        {
            _as = gameObject.AddComponent<AudioSource>();
            _as.playOnAwake = false;
        }

        StopNarration();

        if (ownClip)
        {
            selectClip = ownClip;
            if (selectClip == null)
            {
                Log.d("<color=red>" + string.Format("Sound File Not Found ") + "</color>");
                return;
            }
        }
        else
        {
            //실행중인 오디오클립은 중단되고 새로운 클립 실행
            string path = "Narrations/" + _flag.ToString() + "/" + _clipName;
            selectClip = Resources.Load<AudioClip>(path);
            if (selectClip == null)
            {
                Log.d("<color=red>" + string.Format("Sound File Not Found ( {0} )+ ", path) + "</color>");
                return;
            }
        }


        _as.clip = selectClip;
        _as.Play();

        stime = DateTime.Now;
    }


    //실행중인 오디오클립 길이 반환(실행된 오디오소스가 있어야 추출 가능)
    public float GetAudioClipLength()
    {
        if (selectClip == null) return float.NaN;
        return selectClip.length;
    }

    //나레이션 실행 도중 멈추고 싶을때 
    public void StopNarration()
    {
        if (_as != null)
        {
            _as.Stop();
        }

        stime = DateTime.MinValue;
    }

    public void Pause()
    {
        if (_as != null)
        {
            _as.Pause();
        }

    }

    public void Resume()
    {
        if (stime == DateTime.MinValue)
        {
            return;
        }

        if (_as != null)
        {
            _as.UnPause();
        }

    }
}

