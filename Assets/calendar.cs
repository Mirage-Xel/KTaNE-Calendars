using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json;
using KMHelper;
using System;
using System.Threading;
using Random = UnityEngine.Random;
using UnityEngine;

public class calendar : MonoBehaviour {

    private static int _moduleIdCounter = 1;
    public KMAudio newAudio;
    public KMBombModule module;
    public KMBombInfo info;
    private int _moduleId = 0;
    private bool _isSolved = false, _lightsOn = false;

    private int[] serialNumbers;
    private int serialLeftmost;
    private int serialRightmost;

    private int Jan = 0;
    private int Feb = 1;
    private int Mar = 2;
    private int Apr = 3;
    private int May = 4;
    private int Jun = 5;
    private int Jul = 6;
    private int Aug = 7;
    private int Sep = 8;
    private int Oct = 9;
    private int Nov = 10;
    private int Dec = 11;

    public String[] holidays = new String[18];
    private int holiday = 0;

    private bool leapYear = false;
    private int currentMonthIndex = 0;
    public String[] monthNames = new String[12];
    public KMSelectable[] days = new KMSelectable[31];
    public KMSelectable left;
    public KMSelectable right;
    public TextMesh monthText;
    private int LEDColor;
    public GameObject LED;

    public Material[] LEDColors; //0=Green,1=Yellow,2=Red,3=Blue

    public int season = 0;       //0=Spring,1=Summer,2=Autumn,3=Winter

    private int correctDayIndex;
    private int correctMonthIndex;

    DateTime today;
    private int todayDay;

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        module.OnActivate += Activate;
    }

    private void Awake()
    {
        //handle button presses
    }

    void Activate()
    {
        Init();
        _lightsOn = true;
    }

    void Init()
    {
        today = DateTime.Today;
        todayDay = today.Day;
        int month = today.Month;
        Debug.LogFormat("[Calendar #{0}] The int 'day' returns: {1}. The int 'month' returns: {2}.",_moduleId, todayDay, month);
        season = getSeason(todayDay, month);
        serialNumbers = info.GetSerialNumberNumbers().ToArray();
        serialLeftmost = serialNumbers[0];
        serialRightmost = serialNumbers[serialNumbers.Length - 1];
        LEDColor = Random.Range(0,4);
        LED.GetComponent<MeshRenderer>().material = LEDColors[LEDColor];
        switch (LEDColor)
        {
            case 0:
                Debug.LogFormat("[Calendar #{0}] LED Color: Green", _moduleId);
                break;
            case 1:
                Debug.LogFormat("[Calendar #{0}] LED Color: Yellow", _moduleId);
                break;
            case 2:
                Debug.LogFormat("[Calendar #{0}] LED Color: Red", _moduleId);
                break;
            case 3:
                Debug.LogFormat("[Calendar #{0}] LED Color: Blue", _moduleId);
                break;
        }
        int leapChance = Random.Range(0, 4);
        if (leapChance == 0)
        {
            leapYear = true;
        } else
        {
            leapYear = false;
        }
        holiday = Random.Range(0, 18);
        Debug.LogFormat("[Calendar #{0}] Holiday selected: {1}", _moduleId, holidays[holiday]);
        getCorrectAnswer();
    }

    int getSeason(int day, int month)
    {
        switch (month)
        {
            case 1:
                return 3;
            case 2:
                return 3;
            case 3:
                if(day > 21)
                {
                    return 0;
                } else
                {
                    return 3;
                }
            case 4:
                return 0;
            case 5:
                return 0;
            case 6:
                if (day > 21)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            case 7:
                return 1;
            case 8:
                return 1;
            case 9:
                if (day > 21)
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            case 10:
                return 2;
            case 11:
                return 2;
            case 12:
                if (day > 21)
                {
                    return 3;
                }
                else
                {
                    return 0;
                }
            default:
                Debug.LogFormat("[Calendar #{0}] Your current month doesn't seem to exist. I got {1} as your month. Auto solving.", _moduleId, month);
                module.HandlePass();
                return 4;
        }

    }

    void getCorrectAnswer()
    {
        switch (season)
        {
            case 0:
                switch (LEDColor)
                {
                    case 0:
                        if (todayDay < 11)
                        {
                            correctMonthIndex = Jan;
                        }
                        else if(todayDay > 20)
                        {
                            correctMonthIndex = Aug;
                        }
                        else
                        {
                            correctMonthIndex = Nov;
                        }
                        break;
                    case 1:
                        if (todayDay < 11)
                        {
                            correctMonthIndex = Dec;
                        }
                        else if (todayDay > 20)
                        {
                            correctMonthIndex = Apr;
                        }
                        else
                        {
                            correctMonthIndex = Jun;
                        }
                        break;
                    case 2:
                        if (todayDay < 11)
                        {
                            correctMonthIndex = Feb;
                        }
                        else if (todayDay > 20)
                        {
                            correctMonthIndex = Oct;
                        }
                        else
                        {
                            correctMonthIndex = Jul;
                        }
                        break;
                    case 3:
                        if (todayDay < 11)
                        {
                            correctMonthIndex = May;
                        }
                        else if (todayDay > 20)
                        {
                            correctMonthIndex = Sep;
                        }
                        else
                        {
                            correctMonthIndex = Mar;
                        }
                        break;
                }
                break;
            case 1:
                switch (LEDColor)
                {
                    case 0:
                        if (todayDay < 11)
                        {
                            correctMonthIndex = Jun;
                        }
                        else if (todayDay > 20)
                        {
                            correctMonthIndex = Dec;
                        }
                        else
                        {
                            correctMonthIndex = Mar;
                        }
                        break;
                    case 1:
                        if (todayDay < 11)
                        {
                            correctMonthIndex = Oct;
                        }
                        else if (todayDay > 20)
                        {
                            correctMonthIndex = Feb;
                        }
                        else
                        {
                            correctMonthIndex = May;
                        }
                        break;
                    case 2:
                        if (todayDay < 11)
                        {
                            correctMonthIndex = Jan;
                        }
                        else if (todayDay > 20)
                        {
                            correctMonthIndex = Aug;
                        }
                        else
                        {
                            correctMonthIndex = Sep;
                        }
                        break;
                    case 3:
                        if (todayDay < 11)
                        {
                            correctMonthIndex = Apr;
                        }
                        else if (todayDay > 20)
                        {
                            correctMonthIndex = Nov;
                        }
                        else
                        {
                            correctMonthIndex = Jul;
                        }
                        break;
                }
                break;
            case 2:
                switch (LEDColor)
                {
                    case 0:
                        if (todayDay < 11)
                        {
                            correctMonthIndex = Feb;
                        }
                        else if (todayDay > 20)
                        {
                            correctMonthIndex = Sep;
                        }
                        else
                        {
                            correctMonthIndex = Jul;
                        }
                        break;
                    case 1:
                        if (todayDay < 11)
                        {
                            correctMonthIndex = Aug;
                        }
                        else if (todayDay > 20)
                        {
                            correctMonthIndex = Mar;
                        }
                        else
                        {
                            correctMonthIndex = Nov;
                        }
                        break;
                    case 2:
                        if (todayDay < 11)
                        {
                            correctMonthIndex = Dec;
                        }
                        else if (todayDay > 20)
                        {
                            correctMonthIndex = Jul;
                        }
                        else
                        {
                            correctMonthIndex = Apr;
                        }
                        break;
                    case 3:
                        if (todayDay < 11)
                        {
                            correctMonthIndex = Jun;
                        }
                        else if (todayDay > 20)
                        {
                            correctMonthIndex = Jan;
                        }
                        else
                        {
                            correctMonthIndex = Oct;
                        }
                        break;
                }
                break;
            case 3:
                switch (LEDColor)
                {
                    case 0:
                        if (todayDay < 11)
                        {
                            correctMonthIndex = May;
                        }
                        else if (todayDay > 20)
                        {
                            correctMonthIndex = Apr;
                        }
                        else
                        {
                            correctMonthIndex = Oct;
                        }
                        break;
                    case 1:
                        if (todayDay < 11)
                        {
                            correctMonthIndex = Jul;
                        }
                        else if (todayDay > 20)
                        {
                            correctMonthIndex = Sep;
                        }
                        else
                        {
                            correctMonthIndex = Jan;
                        }
                        break;
                    case 2:
                        if (todayDay < 11)
                        {
                            correctMonthIndex = Mar;
                        }
                        else if (todayDay > 20)
                        {
                            correctMonthIndex = Jun;
                        }
                        else
                        {
                            correctMonthIndex = Nov;
                        }
                        break;
                    case 3:
                        if (todayDay < 11)
                        {
                            correctMonthIndex = Dec;
                        }
                        else if (todayDay > 20)
                        {
                            correctMonthIndex = Feb;
                        }
                        else
                        {
                            correctMonthIndex = Aug;
                        }
                        break;
                }
                break;
        }
        //Get correct day
    }
}
