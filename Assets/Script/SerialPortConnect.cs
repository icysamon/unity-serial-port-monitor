using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using static UnityEngine.GridBrushBase;
using System.Data.SqlClient;
using System.Text;

public class SerialPortConnect : MonoBehaviour
{
    public class DHT20_Data
    {
        public float temperature;
        public float humidity;
    }

    // UI
    [Header("DisplayData")]
    public TMP_Text temperatureTMPText;
    public TMP_Text humidityTMPText;

    [Header("Serial Config")]  
    public TMP_Dropdown portName; // Serial Name
    public TMP_Dropdown baudRate; // 波特率
    public TMP_Dropdown parity; // 奇偶校验
    public TMP_Dropdown dataBits; // 数据位
    public TMP_Dropdown stopBits; // 停止位
    public TMP_Dropdown handShake; // 握手协议

    [Header("串口开关")]
    public UnityEngine.UI.Toggle toogle; // 串口开关

    [Header("消息")]
    public Text text;


    // Serial Port
    static SerialPort _serialPort = new SerialPort();

    // Data
    List<string> dataPortName;
    List<string> dataParity = new List<string>();
    List<string> dataStopBits = new List<string>();
    List<string> dataHandShake = new List<string>();

    bool flag_cofig;
    bool flag_connect;


    // 在第一帧前初始化
    void Start()
    {
        toogle.onValueChanged.AddListener(OnToggleValueChanged); // 事件监听
        Init();
    }

    private void Update()
    {

    }

    // 初始化
    private void Init() 
    {
        // 串口名
        dataPortName = new List<string>(SerialPort.GetPortNames()); // 将获取的串口数组转化为列表
        portName.AddOptions(dataPortName);

        // 奇偶校验
        Parity[] enumParity = (Parity[])Enum.GetValues(typeof(Parity)); // 将枚举转成枚举数组
        foreach (Parity value in enumParity) // 遍历，将枚举数组转化为字符列表
        {
            dataParity.Add(value.ToString());
        }
        parity.AddOptions(dataParity);

        // 停止位
        StopBits[] enumStopBits = (StopBits[])Enum.GetValues(typeof(StopBits)); // 将枚举转成枚举数组
        foreach (StopBits value in enumStopBits) // 遍历，将枚举数组转化为字符列表
        {
            dataStopBits.Add(value.ToString());
        }
        stopBits.AddOptions(dataStopBits);

        // 握手协议
        Handshake[] enumHandshake = (Handshake[])Enum.GetValues(typeof(Handshake)); // 将枚举转成枚举数组
        foreach (Handshake value in enumHandshake) // 遍历，将枚举数组转化为字符列表
        {
            dataHandShake.Add(value.ToString());
        }
        handShake.AddOptions(dataHandShake);

        toogle.isOn = false;

    }

    // 配置
    public void Set()
    {
        _serialPort.PortName = portName.options[portName.value].text; // 串口名
        _serialPort.BaudRate = int.Parse(baudRate.options[baudRate.value].text)   ; // 波特率
        _serialPort.Parity = (Parity)parity.value; // 奇偶校验
        _serialPort.DataBits = int.Parse(dataBits.options[dataBits.value].text); // 数据位
        _serialPort.StopBits = (StopBits)stopBits.value; // 停止位
        _serialPort.Handshake = (Handshake)handShake.value; // 握手协议

        // 设置读写速率
        _serialPort.ReadTimeout = 500;
        _serialPort.WriteTimeout = 500;

        flag_cofig = true;

    }

    // 打开串口
    public void Open()
    {

        if(_serialPort.IsOpen)_serialPort.Close();
        _serialPort.Open();
        
        StartCoroutine(Connecting());
    }

    // 关闭串口
    public void Close()
    {
        _serialPort.Close();
        StopCoroutine(Connecting());
    }

    // 串口线程
    IEnumerator Connecting()
    {
        while(_serialPort.IsOpen)
        {
            if (_serialPort.BytesToRead > 0)
            {
                Thread.Sleep(50);
                try
                {
                    List<string> str = new List<string>();
                    byte[] buf = new byte[_serialPort.BytesToRead];
                    _serialPort.Read(buf, 0, buf.Length);

                    if (buf.Length >= _serialPort.BytesToRead)
                    {
                        foreach (byte b in buf)
                        {
                            if (b != 0)
                            {
                                //Debug.Log(Encoding.UTF8.GetString(new byte[] { b }));
                                str.Add(Encoding.UTF8.GetString(new byte[] { b }));
                            } 
                        }
                        //Debug.Log(string.Concat(str.ToArray()));
                        JsonAnalysis(string.Concat(str.ToArray()));
                        text.text = text.text + string.Concat(str.ToArray());

                    }
                }
                catch (TimeoutException) { }

                
            }

            yield return null;

        }

    }

    // 串口开关事件
    private void OnToggleValueChanged(bool value)
    {
        if (value && flag_cofig)
        {
            flag_connect = true;
            Open();
        }
        else if (!value && flag_cofig)
        {
            Close();
        }
        else toogle.isOn = false;

    }

    // Application Quit
    private void OnApplicationQuit()
    {      
        if (_serialPort.IsOpen) _serialPort.Close(); // Close Serial
        Debug.Log("Application Quit!");
    }

    private void JsonAnalysis(string serial_data)
    {
        try
        {
            DHT20_Data dht20_data = JsonUtility.FromJson<DHT20_Data>(serial_data);
            temperatureTMPText.text = "Temperature : " + dht20_data.temperature.ToString();
            humidityTMPText.text = "Humidity : " + dht20_data.humidity.ToString();
        }
        catch
        {

        }
       
    }

}
