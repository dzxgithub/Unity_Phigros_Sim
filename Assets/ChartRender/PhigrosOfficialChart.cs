using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using static ChartReader.chartEvents;

public class ChartReader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public class chartEvents
    {
        public class XMove
        {
            /// <summary>
            /// 移动时间，当前为关键帧实现，所以没有开始与结束，只有发生时间，单位毫秒
            /// </summary>
            public float time { get; set; }
            /// <summary>
            /// 即被移动到的X位置
            /// </summary>
            public float value { get; set; }
        }
        public class YMove
        {
            /// <summary>
            /// 移动时间，当前为关键帧实现，所以没有开始与结束，只有发生时间，单位毫秒
            /// </summary>
            public float time { get; set; }
            /// <summary>
            /// 即被移动到的Y位置
            /// </summary>
            public float value { get; set; }
        }
        public class RotateChangeEvents
        {
            /// <summary>
            /// 旋转时间，当前为关键帧实现，所以没有开始与结束，只有发生时间，单位毫秒
            /// </summary>
            public float time { get; set; }
            /// <summary>
            /// 即被旋转到的角度
            /// </summary>
            public float value { get; set; }
        }
        public class DisappearEvents
        {
            /// <summary>
            /// 透明度改变时间，当前为关键帧实现，所以没有开始与结束，只有发生时间，单位毫秒
            /// </summary>
            public float time { get; set; }
            /// <summary>
            /// 即被改变到的不透明度，0是完全透明，1是完全不透明
            /// </summary>
            public float value { get; set; }
        }
        
    }
    /// <summary>
    /// 一个note
    /// </summary>
    public class Note
    {
        /// <summary>
        /// note类型。1为Tap，2为Drag，3为Hold，4为Flick
        /// </summary>
        public float noteType { get; set; }
        /// <summary>
        /// 实际被开始打击时间，单位为毫秒
        /// </summary>
        public float clickStartTime { get; set; }
        /// <summary>
        /// 如果此note为Hold，此值为Hold的结束时间，单位毫秒，反之，此数值会与clickStartTime相同
        /// </summary>
        public float clickEndTime { get; set; }
        /// <summary>
        /// 下落方向，true时为从上方下落，反之从下方下落
        /// </summary>
        public bool above { get; set; }
        /// <summary>
        /// 速度倍率，越大越快，默认为1，即不加速。实际速度公式为当前speedEventValue * speedMultiplier
        /// </summary>
        public float speedMultiplier { get; set; }
        /// <summary>
        /// note相对于判定线的X位置
        /// </summary>
        public float X { get; set; }
    }

    public class JudgeLine
    {
        /// <summary>
        /// 所有Y移动事件，即使在官谱中，不会出现分离的情况，但是我们先进行拆分处理
        /// </summary>
        public List<YMove> yMoves { get; set; }
        /// <summary>
        /// 所有X移动事件，即使在官谱中，不会出现分离的情况，但是我们先进行拆分处理
        /// </summary>
        public List<XMove> xMoves { get; set; }
        /// <summary>
        /// 所有判定线旋转事件
        /// </summary>
        public List<RotateChangeEvents> rotateChangeEvents { get; set; }
        /// <summary>
        /// 所有不透明度事件
        /// </summary>
        public List<DisappearEvents> disappearEvents { get; set; }

    }
    public class Chart
    {
        /// <summary>
        /// 曲绘
        /// </summary>
        public Image Illustration { get; set; }
        /// <summary>
        /// 判定线数组
        /// </summary>   
        public List<JudgeLine> judgeLines { get; set; }
    }
    /// <summary>
    /// 坐标转换，将官谱中的坐标转换为Re:PhiEdit中的坐标，方便统一计算
    /// </summary>
    public class CoordinateTransformer
    {
        private const float XMin = -675f;
        private const float XMax = 675f;
        private const float YMin = -450f;
        private const float YMax = 450f;
        /// <summary>
        /// 提供官谱X坐标，返回Re:PhiEdit中的X坐标
        /// </summary>
        /// <param name="x"><summary>官谱X坐标</summary></param>
        /// <returns>Re:PhiEdit的X坐标</returns>
        public static float TransformX(float x)
        {
            return x * (XMax - XMin) + XMin;
        }
        /// <summary>
        /// 提供官谱Y坐标，返回Re:PhiEdit中的X坐标
        /// </summary>
        /// <param name="y"><summary>官谱X坐标</summary></param>
        /// <returns>Re:PhiEdit的Y坐标</returns>
        public static float TransformY(float y)
        {
            return y * (YMax - YMin) + YMin;
        }
    }
    public static float CalculateOriginalTime(float T, float bpm)
    {
        float originalTime = (1.875f / bpm) * T;//结果为秒
        originalTime = originalTime * 1000f;//转换为毫秒
        return originalTime;//返回
    }

    /// <summary>
    /// 官谱转换方法
    /// </summary>
    /// <param name="ChartFilePath">
    /// <summary>
    /// 官谱文件路径
    /// </summary>
    /// </param>
    public void ChartConvert(string ChartFilePath)
    {
        string chartString = File.ReadAllText(ChartFilePath);//读取到字符串
        dynamic chartJsonObject = JsonConvert.DeserializeObject<dynamic>(chartString);//转换为json对象
        if (chartJsonObject["formatVersion"].ToString() == "3")//检查格式，格式不正确将结束运行
        {
            for (int i = 0; i < chartJsonObject["judgeLineList"].Count; i++)//按照判定线数量运行i次
            {
                float judgeLineBPM = chartJsonObject["judgeLineList"][i]["bpm"];//读取此判定线BPM，官谱中每条线一个BPM
                for (int moveEventCount = 0; moveEventCount < chartJsonObject["judgeLineList"][i]["judgeLineMoveEvents"].Count; moveEventCount++)//读取所有移动事件
                {
                    float time = CalculateOriginalTime(chartJsonObject["judgeLineList"][i]["judgeLineMoveEvents"][moveEventCount]["endTime"], judgeLineBPM);//转换出时间，时间为毫秒
                    float xValue = CoordinateTransformer.TransformX(chartJsonObject["judgeLineList"][i]["judgeLineMoveEvents"][moveEventCount]["end"]);//狗屎官谱，我草死你的吗，end是X
                    float yValue = CoordinateTransformer.TransformY(chartJsonObject["judgeLineList"][i]["judgeLineMoveEvents"][moveEventCount]["end2"]);//狗屎官谱，我草死你的吗，end2是Y
                    
                }
            }
        }
        else
        {
            //回到MainScene，编号为0
            SceneManager.LoadScene(0);
        }
    }
}
