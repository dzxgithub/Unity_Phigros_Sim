using E7.Native;
using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ChartReader;
using static UnityEditor.Progress;

public class StartPlay : MonoBehaviour
{
    public RectTransform canvasRectTransform;//打击画布
    //public Image Background_Board;//背景板
    public GameObject JudgeLine;//判定线预制件
    public GameObject TapNote;//Tap音符预制件
    public GameObject FlickNote;//Flick音符预制件
    public GameObject DragNote;//Drag音符预制件
    public GameObject HoldNote;//Hold音符预制件
    public TMP_Text TimeReads;
    public bool MusicisPlay = false;
    // Start is called before the first frame update
    // 此方法会在第一帧前调用
    void Start()
    {
        Chart chart = ChartCache.Instance.chart;
        if (chart == null)
        {
            //新建一个Exception，表示缓存是空的
            throw new FileNotFoundException("没有找到文件，缓存为空\nCache is empty");
        }
        else
        {
            //绘制谱面到屏幕
            DrawPlayScene(chart);
        }
        
    }

    // Update is called once per frame
    // 此方法会被每一帧调用
    void Update()
    {
        
    }
    IEnumerator WindowsWaitAndPlay(AudioSource aS, double time)
    {
        while (true)
        {
            TimeReads.text = "NowTime:" + (System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds - time).ToString();
            if (time <= System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds && !MusicisPlay)
            {
                MusicisPlay = true;
                aS.Play();
            }
            yield return null;
        }
    }
    IEnumerator AndroidWaitAndPlay(AudioClip music, double time)
    {
        //预加载音乐
        AudioClip aC = music;
        NativeAudioPointer audioPointer;
        audioPointer = NativeAudio.Load(aC);
        NativeSource nS = new NativeSource();
        while (true)
        {
            TimeReads.text = "NowTime:" + (System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds - time).ToString();
            if (time <= System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds && !MusicisPlay)
            {
                MusicisPlay = true;
                nS.Play(audioPointer);
            }
            yield return null;
        }
    }
    public void DrawPlayScene(Chart chart)
    {

        //获取当前unix时间戳，单位毫秒
        double unixTime = System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
        unixTime = unixTime + 10000f;
        for (int i = 0; i < chart.judgeLineList.Count; i++)
        {
            // 实例化预制件，位置为 (0, 0, 0)，旋转为零旋转
            GameObject instance = Instantiate(JudgeLine);

            // 找到父 GameObject
            GameObject parent = GameObject.Find("Canvas");
            // 将实例化的预制件设置为父 GameObject 的子对象
            instance.transform.SetParent(parent.transform);

            // 设置实例化的预制件的位置
            instance.transform.position = new Vector3(0, 0, 0f);

            //设置预制件的位置位于画布中间
            RectTransform prefabRectTransform = instance.GetComponent<RectTransform>();
            prefabRectTransform.anchoredPosition = canvasRectTransform.rect.center;

            // 获取预制件的脚本组件
            JudgeLineScr script = instance.GetComponent<JudgeLineScr>();

            // 设置脚本中的公共变量
            script.playStartUnixTime = unixTime;
            script.judgeLine = chart.judgeLineList[i];
            script.whoami = i;
            //SpriteRenderer sprRend = instance.GetComponent<SpriteRenderer>();
            //sprRend.size = new Vector2(6220.8f, 8.11f);
            for (int noteIndex = 0; noteIndex < chart.judgeLineList[i].noteList.Count; noteIndex++)
            {
                if (chart.judgeLineList[i].noteList[noteIndex].noteType == 1)
                {
                    GameObject Tnote = Instantiate(TapNote);
                    Tnote.transform.SetParent(instance.transform);
                    NotesScr TnScr = Tnote.GetComponent<NotesScr>();
                    TnScr.above = chart.judgeLineList[i].noteList[noteIndex].above;
                    TnScr.fatherJudgeLine = instance;
                    TnScr.clickStartTime = chart.judgeLineList[i].noteList[noteIndex].clickStartTime;
                    TnScr.speedMultiplier = chart.judgeLineList[i].noteList[noteIndex].speedMultiplier;
                    TnScr.xValue = chart.judgeLineList[i].noteList[noteIndex].X;
                    TnScr.playStartUnixTime = unixTime;
                }
                else if (chart.judgeLineList[i].noteList[noteIndex].noteType == 2)
                {
                    GameObject Dnote = Instantiate(DragNote);
                    Dnote.transform.SetParent(instance.transform);
                    NotesScr DnScr = Dnote.GetComponent<NotesScr>();
                    DnScr.above = chart.judgeLineList[i].noteList[noteIndex].above;
                    DnScr.fatherJudgeLine = instance;
                    DnScr.clickStartTime = chart.judgeLineList[i].noteList[noteIndex].clickStartTime;
                    DnScr.speedMultiplier = chart.judgeLineList[i].noteList[noteIndex].speedMultiplier;
                    DnScr.xValue = chart.judgeLineList[i].noteList[noteIndex].X;
                    DnScr.playStartUnixTime = unixTime;
                }
                else if (chart.judgeLineList[i].noteList[noteIndex].noteType == 3)
                {
                    GameObject Hnote = Instantiate(DragNote);
                    Hnote.transform.SetParent(instance.transform);
                    NotesScr HnScr = Hnote.GetComponent<NotesScr>();
                    HnScr.above = chart.judgeLineList[i].noteList[noteIndex].above;
                    HnScr.fatherJudgeLine = instance;
                    HnScr.clickStartTime = chart.judgeLineList[i].noteList[noteIndex].clickStartTime;
                    HnScr.clickEndTime = chart.judgeLineList[i].noteList[noteIndex].clickEndTime;
                    HnScr.speedMultiplier = chart.judgeLineList[i].noteList[noteIndex].speedMultiplier;
                    HnScr.xValue = chart.judgeLineList[i].noteList[noteIndex].X;
                    HnScr.playStartUnixTime = unixTime;
                }
                else if (chart.judgeLineList[i].noteList[noteIndex].noteType == 4)
                {
                    GameObject Fnote = Instantiate(DragNote);
                    Fnote.transform.SetParent(instance.transform);
                    NotesScr FnScr = Fnote.GetComponent<NotesScr>();
                    FnScr.above = chart.judgeLineList[i].noteList[noteIndex].above;
                    FnScr.fatherJudgeLine = instance;
                    FnScr.clickStartTime = chart.judgeLineList[i].noteList[noteIndex].clickStartTime;
                    FnScr.speedMultiplier = chart.judgeLineList[i].noteList[noteIndex].speedMultiplier;
                    FnScr.xValue = chart.judgeLineList[i].noteList[noteIndex].X;
                    FnScr.playStartUnixTime = unixTime;
                }
                else
                {
                    throw new ArgumentException("不是Tap，不是Flick，不是Drag，不是Hold，你是谁？");
                }
            }
        }
#if UNITY_EDITOR
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = chart.music;
        audioSource.loop = false; //控制循环播放
        StartCoroutine(WindowsWaitAndPlay(audioSource, unixTime));
#elif UNITY_ANDROID
        //初始化
        NativeAudio.Initialize();
        //调用方法准备播放
        StartCoroutine(AndroidWaitAndPlay(chart.music, unixTime));
#elif UNITY_STANDALONE_WIN
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = chart.music;
        audioSource.loop = false; //控制循环播放
        StartCoroutine(WindowsWaitAndPlay(audioSource, unixTime));
#endif
    }
}
