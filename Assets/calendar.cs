using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json;
using KMHelper;
using System;
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

    private Color red = new Color(255, 0, 0), green = new Color(0, 255, 0), blue = new Color(0, 0, 255), yellow = new Color(255, 255, 0);
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

    public String[] holidayNames = new String[18];
    public GameObject[] holidayCircles = new GameObject[18];
    public GameObject goldenLeft;
    public GameObject goldenRight;
    public GameObject kwanLeft;
    public GameObject kwanRight, colorblindObj;
    private int holiday = 0;
    public GameObject holidayObject;
    private int groundhogPressIndex = 0;

    private bool leapYear = false;
    private int currentMonthIndex = 0;
    public String[] monthNames = new String[12];
    public KMSelectable[] days = new KMSelectable[31];
    public GameObject[] daysObj = new GameObject[31];
    public KMSelectable left;
    public KMSelectable right;
    public TextMesh monthText, colorblindText;
    private int LEDColor;
    public GameObject LED;

    public Material[] LEDColors; //0=Green,1=Yellow,2=Red,3=Blue
    public Material off;

    private int season = 0;       //0=Spring,1=Summer,2=Autumn,3=Winter

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
        left.OnInteract += delegate ()
        {
            handlePressLeft();
            return false;
        };
        right.OnInteract += delegate ()
        {
            handlePressRight();
            return false;
        };
        for (int i = 0; i < 31; i++)
        {
            int j = i;
            days[i].OnInteract += delegate ()
            {
                handleDayPress(j);
                return false;
            };
        }
    }

    void handleDayPress(int index)
    {
        newAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, days[index].transform);
        if (!_lightsOn || _isSolved) return;
        if (holiday == 10)
        {
            if (currentMonthIndex == correctMonthIndex)
            {
                groundhogPressIndex++;
                if (groundhogPressIndex == 3)
                {
                    module.HandlePass();
                    newAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, days[index].transform);
                } else
                {
                    newAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, days[index].transform);
                }
            } else
            {
                Debug.LogFormat("[Calendar #{0}] Incorrect month selected. Input: {1}. Expected: {2}", _moduleId, currentMonthIndex + 1, correctMonthIndex + 1);
                Debug.LogFormat("[Calendar #{0}] If you feel that this strike is an error, please contact AAces as soon as possible so we can get this error sorted out (Remember, the groundhog rule is in play). Have a copy of this log file handy. Discord: AAces#0908", _moduleId);
                module.HandleStrike();
                groundhogPressIndex = 0;
            }
            return;
        }
        if(currentMonthIndex == correctMonthIndex)
        {
            if(index == correctDayIndex)
            {
                module.HandlePass();
                newAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, days[index].transform);
                _isSolved = true;
            }
            else
            {
                module.HandleStrike();
                Debug.LogFormat("[Calendar #{0}] Incorrect day selected. Input: {1}. Expected: {2}", _moduleId, index+1, correctDayIndex+1);
                Debug.LogFormat("[Calendar #{0}] If you feel that this strike is an error, please contact AAces as soon as possible so we can get this error sorted out. Have a copy of this log file handy. Discord: AAces#0908", _moduleId);
            }
        } else
        {
            module.HandleStrike();
            Debug.LogFormat("[Calendar #{0}] Incorrect month selected. Input: {1}. Expected: {2}", _moduleId, currentMonthIndex + 1, correctMonthIndex + 1);
            if(index != correctDayIndex)
            {
                Debug.LogFormat("[Calendar #{0}] Incorrect day selected. Input: {1}. Expected: {2}", _moduleId, index + 1, correctDayIndex + 1);
            }
            Debug.LogFormat("[Calendar #{0}] If you feel that this strike is an error, please contact AAces as soon as possible so we can get this error sorted out. Have a copy of this log file handy. Discord: AAces#0908", _moduleId);
        }
    }

    void handlePressRight()
    {
        newAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, right.transform);
        if (!_lightsOn || _isSolved) return;
        currentMonthIndex++;
        if (currentMonthIndex > 11)
        {
            currentMonthIndex = 0;
        }
        displayMonth();
    }

    void handlePressLeft()
    {
        newAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, left.transform);
        if (!_lightsOn || _isSolved) return;
        currentMonthIndex--;
        if (currentMonthIndex < 0)
        {
            currentMonthIndex = 11;
        }
        displayMonth();
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
        setupLED();
        int leapChance = Random.Range(0, 4);
        if (leapChance == 0)
        {
            leapYear = true;
            Debug.LogFormat("[Calendar #{0}] Is a leap year. (This is seperate from real life)", _moduleId);
        } else
        {
            leapYear = false;
            Debug.LogFormat("[Calendar #{0}] Is not a leap year. (This is seperate from real life)", _moduleId);
        }
        holidayObject.SetActive(true);
        holiday = Random.Range(0, 18);
        Debug.LogFormat("[Calendar #{0}] Holiday selected: {1}", _moduleId, holidayNames[holiday]);
        for(int i=0; i<18; i++)
        {
            holidayCircles[i].SetActive(false);
        }
        goldenLeft.SetActive(false);
        goldenRight.SetActive(false);
        kwanLeft.SetActive(false);
        kwanRight.SetActive(false);
        displayMonth();
        getCorrectAnswer();
        Debug.LogFormat("[Calendar #{0}] Correct month: {1}. Correct day: {2}. NOTE: If the holiday is groundhog day, the day has no effect.", _moduleId, correctMonthIndex+1, correctDayIndex+1);
    }

    void setupLED()
    {
        LEDColor = Random.Range(0, 4);
        switch (LEDColor)
        {
            case 0:
                Debug.LogFormat("[Calendar #{0}] LED Color: Green", _moduleId);
                colorblindText.color = green;
                colorblindText.text = "Green";
                break;
            case 1:
                Debug.LogFormat("[Calendar #{0}] LED Color: Yellow", _moduleId);
                colorblindText.color = yellow;
                colorblindText.text = "Yellow";
                break;
            case 2:
                Debug.LogFormat("[Calendar #{0}] LED Color: Red", _moduleId);
                colorblindText.color = red;
                colorblindText.text = "Red";
                break;
            case 3:
                Debug.LogFormat("[Calendar #{0}] LED Color: Blue", _moduleId);
                colorblindText.color = blue;
                colorblindText.text = "Blue";
                break;
        }
        if (GetComponent<KMColorblindMode>().ColorblindModeActive)
        {
            colorblindObj.SetActive(true);
            Debug.LogFormat("[Calendar #{0}] Colorblind mode enabled.", _moduleId);
        }
        else
        {
            LED.GetComponent<MeshRenderer>().material = LEDColors[LEDColor];
            colorblindObj.SetActive(false);
        }
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
                Debug.LogFormat("[Calendar #{0}] If you feel that this is an error, please contact AAces as soon as possible so we can get this error sorted out. Have a copy of this log file handy. Discord: AAces#0908", _moduleId);
                module.HandlePass();
                _isSolved = true;
                return 4;
        }

    }

    void displayMonth()
    {
        monthText.text = monthNames[currentMonthIndex];
        switch (currentMonthIndex)
        {
            case 0:
                for(int i = 0; i<31; i++) {
                    daysObj[i].SetActive(true);
                }
                for (int i = 0; i < 18; i++)
                {
                    holidayCircles[i].SetActive(false);
                }
                goldenLeft.SetActive(false);
                goldenRight.SetActive(false);
                kwanLeft.SetActive(false);
                kwanRight.SetActive(false);
                if (holiday == 1)
                {
                    holidayCircles[1].SetActive(true);
                } else if(holiday == 8)
                {
                    holidayCircles[8].SetActive(true);
                } else if(holiday == 12)
                {
                    kwanRight.SetActive(true);
                }else if (holiday == 17)
                {
                    holidayCircles[17].SetActive(true);
                }
                break;
            case 1:
                if (leapYear)
                {
                    daysObj[30].SetActive(false);
                    daysObj[29].SetActive(false);
                    daysObj[28].SetActive(true);
                } else
                {
                    daysObj[30].SetActive(false);
                    daysObj[29].SetActive(false);
                    daysObj[28].SetActive(false);
                }
                for (int i = 0; i < 18; i++)
                {
                    holidayCircles[i].SetActive(false);
                }
                goldenLeft.SetActive(false);
                goldenRight.SetActive(false);
                kwanLeft.SetActive(false);
                kwanRight.SetActive(false);
                if (holiday == 10)
                {
                    holidayCircles[10].SetActive(true);
                }
                else if (holiday == 15)
                {
                    holidayCircles[15].SetActive(true);
                }
                break;
            case 2:
                for (int i = 0; i < 31; i++)
                {
                    daysObj[i].SetActive(true);
                }
                for (int i = 0; i < 18; i++)
                {
                    holidayCircles[i].SetActive(false);
                }
                goldenLeft.SetActive(false);
                goldenRight.SetActive(false);
                kwanLeft.SetActive(false);
                kwanRight.SetActive(false);
                if (holiday == 14)
                {
                    holidayCircles[14].SetActive(true);
                }
                break;
            case 3:
                daysObj[30].SetActive(false);
                for (int i = 0; i < 18; i++)
                {
                    holidayCircles[i].SetActive(false);
                }
                goldenLeft.SetActive(false);
                goldenRight.SetActive(false);
                kwanLeft.SetActive(false);
                kwanRight.SetActive(false);
                if (holiday == 0)
                {
                    holidayCircles[0].SetActive(true);
                }
                else if (holiday == 7)
                {
                    holidayCircles[7].SetActive(true);
                }
                else if (holiday == 9)
                {
                    goldenLeft.SetActive(true);
                }
                break;
            case 4:
                for (int i = 0; i < 31; i++)
                {
                    daysObj[i].SetActive(true);
                }
                for (int i = 0; i < 18; i++)
                {
                    holidayCircles[i].SetActive(false);
                }
                goldenLeft.SetActive(false);
                goldenRight.SetActive(false);
                kwanLeft.SetActive(false);
                kwanRight.SetActive(false);
                if (holiday == 4)
                {
                    holidayCircles[4].SetActive(true);
                }
                else if (holiday == 9)
                {
                    goldenRight.SetActive(true);
                }
                break;
            case 5:
                daysObj[30].SetActive(false);
                for (int i = 0; i < 18; i++)
                {
                    holidayCircles[i].SetActive(false);
                }
                goldenLeft.SetActive(false);
                goldenRight.SetActive(false);
                kwanLeft.SetActive(false);
                kwanRight.SetActive(false);
                if (holiday == 13)
                {
                    holidayCircles[13].SetActive(true);
                }
                break;
            case 6:
                for (int i = 0; i < 31; i++)
                {
                    daysObj[i].SetActive(true);
                }
                for (int i = 0; i < 18; i++)
                {
                    holidayCircles[i].SetActive(false);
                }
                goldenLeft.SetActive(false);
                goldenRight.SetActive(false);
                kwanLeft.SetActive(false);
                kwanRight.SetActive(false);
                if (holiday == 2)
                {
                    holidayCircles[2].SetActive(true);
                }
                break;
            case 7:
                for (int i = 0; i < 31; i++)
                {
                    daysObj[i].SetActive(true);
                }
                for (int i = 0; i < 18; i++)
                {
                    holidayCircles[i].SetActive(false);
                }
                goldenLeft.SetActive(false);
                goldenRight.SetActive(false);
                kwanLeft.SetActive(false);
                kwanRight.SetActive(false);
                break;
            case 8:
                daysObj[30].SetActive(false);
                for (int i = 0; i < 18; i++)
                {
                    holidayCircles[i].SetActive(false);
                }
                goldenLeft.SetActive(false);
                goldenRight.SetActive(false);
                kwanLeft.SetActive(false);
                kwanRight.SetActive(false);
                break;
            case 9:
                for (int i = 0; i < 31; i++)
                {
                    daysObj[i].SetActive(true);
                }
                for (int i = 0; i < 18; i++)
                {
                    holidayCircles[i].SetActive(false);
                }
                goldenLeft.SetActive(false);
                goldenRight.SetActive(false);
                kwanLeft.SetActive(false);
                kwanRight.SetActive(false);
                if (holiday == 5)
                {
                    holidayCircles[5].SetActive(true);
                }
                else if (holiday == 6)
                {
                    holidayCircles[6].SetActive(true);
                }
                break;
            case 10:
                daysObj[30].SetActive(false);
                for (int i = 0; i < 18; i++)
                {
                    holidayCircles[i].SetActive(false);
                }
                goldenLeft.SetActive(false);
                goldenRight.SetActive(false);
                kwanLeft.SetActive(false);
                kwanRight.SetActive(false);
                if (holiday == 11)
                {
                    holidayCircles[11].SetActive(true);
                }
                else if (holiday == 16)
                {
                    holidayCircles[16].SetActive(true);
                }
                break;
            case 11:
                for (int i = 0; i < 31; i++)
                {
                    daysObj[i].SetActive(true);
                }
                for (int i = 0; i < 18; i++)
                {
                    holidayCircles[i].SetActive(false);
                }
                goldenLeft.SetActive(false);
                goldenRight.SetActive(false);
                kwanLeft.SetActive(false);
                kwanRight.SetActive(false);
                if (holiday == 3)
                {
                    holidayCircles[3].SetActive(true);
                }
                else if (holiday == 12)
                {
                    kwanLeft.SetActive(true);
                }
                break;
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
        int num = serialRightmost;
        int x = 1;
        if (holiday == 0)
        {
            num = serialLeftmost;
        }
        switch (holiday)
        {
            case 0:
                switch (num)
                {
                    case 0:
                        x = 0;
                        break;
                    case 1:
                        x = 1;
                        break;
                    case 2:
                        x = 2;
                        break;
                    case 3:
                        x = 3;
                        break;
                    case 4:
                        x = 4;
                        break;
                    case 5:
                        x = 5;
                        break;
                    case 6:
                        x = 6;
                        break;
                    case 7:
                        x = 7;
                        break;
                    case 8:
                        x = 8;
                        break;
                    case 9:
                        x = 9;
                        break;
                }
                break;
            case 1:
                switch (num)
                {
                    case 0:
                        x = 18;
                        break;
                    case 1:
                        x = 4;
                        break;
                    case 2:
                        x = 23;
                        break;
                    case 3:
                        x = 2;
                        break;
                    case 4:
                        if (!leapYear && correctMonthIndex == 1)
                        {
                            x = 0;
                        }
                        else
                        { 
                            x = 28;
                        }
                        break;
                    case 5:
                        x = 27;
                        break;
                    case 6:
                        x = 17;
                        break;
                    case 7:
                        if (correctMonthIndex == 1)
                        {
                            x = 3;
                        }
                        else
                        {
                            x = 29;
                        }
                        break;
                    case 8:
                        x = 12;
                        break;
                    case 9:
                        x = 11;
                        break;
                }
                break;
            case 2:
                switch (num)
                {
                    case 0:
                        x = 21;
                        break;
                    case 1:
                        x = 13;
                        break;
                    case 2:
                        x = 5;
                        break;
                    case 3:
                        x = 10;
                        break;
                    case 4:
                        x = 7;
                        break;
                    case 5:
                        x = 18;
                        break;
                    case 6:
                        if (correctMonthIndex == 1 || correctMonthIndex == 3 || correctMonthIndex == 5 || correctMonthIndex == 8 || currentMonthIndex == 10)
                        {
                            x = 6;
                        }
                        else
                        {
                            x = 30;
                        }
                        break;
                    case 7:
                        x = 22;
                        break;
                    case 8:
                        x = 27;
                        break;
                    case 9:
                        x = 25;
                        break;
                }
                break;
            case 3:
                switch (num)
                {
                    case 0:
                        x = 11;
                        break;
                    case 1:
                        x = 1;
                        break;
                    case 2:
                        x = 10;
                        break;
                    case 3:
                        x = 6;
                        break;
                    case 4:
                        x = 17;
                        break;
                    case 5:
                        x = 23;
                        break;
                    case 6:
                        x = 3;
                        break;
                    case 7:
                        x = 13;
                        break;
                    case 8:
                        x = 9;
                        break;
                    case 9:
                        x = 19;
                        break;
                }
                break;
            case 4:
                switch (num)
                {
                    case 0:
                        if (!leapYear && correctMonthIndex == 1)
                        {
                            x = 2;
                        }
                        else
                        {
                            x = 28;
                        }
                        break;
                    case 1:
                        x = 18;
                        break;
                    case 2:
                        x = 26;
                        break;
                    case 3:
                        x = 14;
                        break;
                    case 4:
                        x = 8;
                        break;
                    case 5:
                        x = 15;
                        break;
                    case 6:
                        x = 18;
                        break;
                    case 7:
                        x = 13;
                        break;
                    case 8:
                        x = 8;
                        break;
                    case 9:
                        x = 2;
                        break;
                }
                break;
            case 5:
                switch (num)
                {
                    case 0:
                        x = 3;
                        break;
                    case 1:
                        x = 26;
                        break;
                    case 2:
                        x = 7;
                        break;
                    case 3:
                        x = 21;
                        break;
                    case 4:
                        x = 9;
                        break;
                    case 5:
                        x = 13;
                        break;
                    case 6:
                        x = 12;
                        break;
                    case 7:
                        x = 27;
                        break;
                    case 8:
                        x = 12;
                        break;
                    case 9:
                        x = 20;
                        break;
                }
                break;
            case 6:
                switch (num)
                {
                    case 0:
                        x = 3;
                        break;
                    case 1:
                        x = 15;
                        break;
                    case 2:
                        x = 20;
                        break;
                    case 3:
                        x = 14;
                        break;
                    case 4:
                        x = 26;
                        break;
                    case 5:
                        x = 5;
                        break;
                    case 6:
                        x = 24;
                        break;
                    case 7:
                        x = 12;
                        break;
                    case 8:
                        x = 1;
                        break;
                    case 9:
                        x = 8;
                        break;
                }
                break;
            case 7:
                switch (num)
                {
                    case 0:
                        x = 22;
                        break;
                    case 1:
                        x = 12;
                        break;
                    case 2:
                        x = 24;
                        break;
                    case 3:
                        if (correctMonthIndex == 1)
                        {
                            x = 2;
                        }
                        else
                        {
                            x = 29;
                        }
                        break;
                    case 4:
                        x = 3;
                        break;
                    case 5:
                        x = 10;
                        break;
                    case 6:
                        x = 26;
                        break;
                    case 7:
                        x = 14;
                        break;
                    case 8:
                        x = 20;
                        break;
                    case 9:
                        if (correctMonthIndex == 1 || correctMonthIndex == 3 || correctMonthIndex == 5 || correctMonthIndex == 8 || currentMonthIndex == 10)
                        {
                            x = 4;
                        }
                        else
                        {
                            x = 30;
                        }
                        break;
                }
                break;
            case 8:
                switch (num)
                {
                    case 0:
                        x = 14;
                        break;
                    case 1:
                        x = 0;
                        break;
                    case 2:
                        if (correctMonthIndex == 1 || correctMonthIndex == 3 || correctMonthIndex == 5 || correctMonthIndex == 8 || currentMonthIndex == 10)
                        {
                            x = 6;
                        }
                        else
                        {
                            x = 30;
                        }
                        break;
                    case 3:
                        x = 16;
                        break;
                    case 4:
                        x = 25;
                        break;
                    case 5:
                        if (correctMonthIndex == 1)
                        {
                            x = 7;
                        }
                        else
                        {
                            x = 29;
                        }
                        break;
                    case 6:
                        x = 23;
                        break;
                    case 7:
                        x = 8;
                        break;
                    case 8:
                        x = 2;
                        break;
                    case 9:
                        x = 24;
                        break;
                }
                break;
            case 9:
                switch (num)
                {
                    case 0:
                        x = 7;
                        break;
                    case 1:
                        x = 19;
                        break;
                    case 2:
                        x = 16;
                        break;
                    case 3:
                        x = 15;
                        break;
                    case 4:
                        x = 22;
                        break;
                    case 5:
                        x = 15;
                        break;
                    case 6:
                        x = 0;
                        break;
                    case 7:
                        x = 21;
                        break;
                    case 8:
                        x = 23;
                        break;
                    case 9:
                        x = 4;
                        break;
                }
                break;
            case 11:
                switch (num)
                {
                    case 0:
                        x = 25;
                        break;
                    case 1:
                        x = 15;
                        break;
                    case 2:
                        x = 2;
                        break;
                    case 3:
                        x = 25;
                        break;
                    case 4:
                        if (!leapYear && correctMonthIndex == 1)
                        {
                            x = 6;
                        }
                        else
                        {
                            x = 28;
                        }
                        break;
                    case 5:
                        x = 17;
                        break;
                    case 6:
                        x = 21;
                        break;
                    case 7:
                        x = 24;
                        break;
                    case 8:
                        x = 16;
                        break;
                    case 9:
                        x = 10;
                        break;
                }
                break;
            case 12:
                switch (num)
                {
                    case 0:
                        x = 20;
                        break;
                    case 1:
                        x = 8;
                        break;
                    case 2:
                        if (correctMonthIndex == 1)
                        {
                            x = 5;
                        }
                        else
                        {
                            x = 29;
                        }
                        break;
                    case 3:
                        x = 23;
                        break;
                    case 4:
                        x = 27;
                        break;
                    case 5:
                        x = 5;
                        break;
                    case 6:
                        x = 20;
                        break;
                    case 7:
                        x = 25;
                        break;
                    case 8:
                        if (correctMonthIndex == 1 || correctMonthIndex == 3 || correctMonthIndex == 5 || correctMonthIndex == 8 || currentMonthIndex == 10)
                        {
                            x = 1;
                        }
                        else
                        {
                            x = 30;
                        }
                        break;
                    case 9:
                        x = 7;
                        break;
                }
                break;
            case 13:
                switch (num)
                {
                    case 0:
                        x = 9;
                        break;
                    case 1:
                        if (!leapYear && correctMonthIndex == 1)
                        {
                            x = 1;
                        }
                        else
                        { 
                            x = 28;
                        }
                        break;
                    case 2:
                        x = 11;
                        break;
                    case 3:
                        x = 23;
                        break;
                    case 4:
                        x = 14;
                        break;
                    case 5:
                        x = 19;
                        break;
                    case 6:
                        x = 4;
                        break;
                    case 7:
                        x = 26;
                        break;
                    case 8:
                        x = 24;
                        break;
                    case 9:
                        x = 6;
                        break;
                }
                break;
            case 14:
                switch (num)
                {
                    case 0:
                        x = 1;
                        break;
                    case 1:
                        x = 27;
                        break;
                    case 2:
                        x = 17;
                        break;
                    case 3:
                        x = 12;
                        break;
                    case 4:
                        x = 20;
                        break;
                    case 5:
                        x = 11;
                        break;
                    case 6:
                        x = 2;
                        break;
                    case 7:
                        x = 9;
                        break;
                    case 8:
                        x = 19;
                        break;
                    case 9:
                        x = 0;
                        break;
                }
                break;
            case 15:
                switch (num)
                {
                    case 0:
                        x = 10;
                        break;
                    case 1:
                        x = 5;
                        break;
                    case 2:
                        x = 21;
                        break;
                    case 3:
                        x = 13;
                        break;
                    case 4:
                        x = 18;
                        break;
                    case 5:
                        x = 26;
                        break;
                    case 6:
                        x = 19;
                        break;
                    case 7:
                        x = 6;
                        break;
                    case 8:
                        x = 15;
                        break;
                    case 9:
                        x = 22;
                        break;
                }
                break;
            case 16:
                switch (num)
                {
                    case 0:
                        x = 13;
                        break;
                    case 1:
                        x = 6;
                        break;
                    case 2:
                        x = 22;
                        break;
                    case 3:
                        x = 16;
                        break;
                    case 4:
                        x = 4;
                        break;
                    case 5:
                        if (correctMonthIndex == 1 || correctMonthIndex == 3 || correctMonthIndex == 5 || correctMonthIndex == 8 || currentMonthIndex == 10)
                        {
                            x = 0;
                        }
                        else
                        {
                            x = 30;
                        }
                        break;
                    case 6:
                        x = 1;
                        break;
                    case 7:
                        x = 24;
                        break;
                    case 8:
                        x = 16;
                        break;
                    case 9:
                        x = 10;
                        break;
                }
                break;
            case 17:
                switch (num)
                {
                    case 0:
                        x = 16;
                        break;
                    case 1:
                        x = 23;
                        break;
                    case 2:
                        x = 14;
                        break;
                    case 3:
                        x = 19;
                        break;
                    case 4:
                        x = 0;
                        break;
                    case 5:
                        if (correctMonthIndex == 1)
                        {
                            x = 8;
                        }
                        else
                        {
                            x = 29;
                        }
                        break;
                    case 6:
                        x = 27;
                        break;
                    case 7:
                        x = 5;
                        break;
                    case 8:
                        x = 6;
                        break;
                    case 9:
                        x = 13;
                        break;
                }
                break;
            default:
                break;
        }
        correctDayIndex = x;
    }
#pragma warning disable 414
    private string TwitchHelpMessage = "Use '!{0} holiday' to locate the circled holiday. Use '!{0} left' or '!{0} right' to cycle left or right one month at a time. Use '!{0} press #' to press a certain day. Use '!{0} [month name]' to cycle to the selected month. Use !{0} colorblind to enable colorblind mode.";
#pragma warning restore 414
    public KMSelectable[] ProcessTwitchCommand(string command)
    {
        KMSelectable[] ans;

        if (command.Trim().ToLowerInvariant().StartsWith("press"))
        {
            ans = new KMSelectable[1];
            command = command.Substring(6, command.Length - 6);
            int buttonIndex;
            if(!Int32.TryParse(command, out buttonIndex))
            {
                return null;
            }
            buttonIndex--;
            
            if (buttonIndex >= 0 && buttonIndex < 31)
            {
                ans[0] = days[buttonIndex];
                if(buttonIndex==30 && (currentMonthIndex == 1 || currentMonthIndex == 3 || currentMonthIndex == 5 || currentMonthIndex == 8 || currentMonthIndex == 10))
                {
                    return null;
                }
                if(buttonIndex==29 && currentMonthIndex == 1)
                {
                    return null;
                }
                if (buttonIndex == 28 && !leapYear && currentMonthIndex == 1)
                {
                    return null;
                }
                return ans;
            }
            else
            {
                return null;
            }
        } else if (command.Trim().ToLowerInvariant().StartsWith("left"))
        {
            Debug.LogFormat("Left entered");
            ans = new KMSelectable[1];
            ans[0] = left;
            return ans;
        }
        else if (command.Trim().ToLowerInvariant().StartsWith("right"))
        {
            Debug.LogFormat("Right entered");
            ans = new KMSelectable[1];
            ans[0] = right;
            return ans;
        } else if (command.Trim().ToLowerInvariant().StartsWith("holiday"))
        {
            switch (holiday)
            {
                case 1: case 8: case 17:
                    return ProcessTwitchCommand("jan");
                case 10: case 15:
                    return ProcessTwitchCommand("feb");
                case 14:
                    return ProcessTwitchCommand("mar");
                case 0: case 7: case 9:
                    return ProcessTwitchCommand("apr");
                case 4:
                    return ProcessTwitchCommand("may");
                case 13:
                    return ProcessTwitchCommand("jun");
                case 2:
                    return ProcessTwitchCommand("jul");
                case 5: case 6:
                    return ProcessTwitchCommand("oct");
                case 11: case 16:
                    return ProcessTwitchCommand("nov");
                case 3: case 12:
                    return ProcessTwitchCommand("dec");
            }
        }
        else if (command.Trim().ToLowerInvariant().StartsWith("jan"))
        {
            if (currentMonthIndex == 0)
            {
                return null;
            } else
            {
                int count = Math.Abs(currentMonthIndex - 0);
                ans = new KMSelectable[count];
                for (int i=0; i<count; i++)
                {
                    if (currentMonthIndex - 0 > 0)
                    {
                        ans[i] = left;
                    }
                    else
                    {
                        ans[i] = right;
                    }
                }
                return ans;
            } 
        }
        else if (command.Trim().ToLowerInvariant().StartsWith("feb"))
        {
            if (currentMonthIndex == 1)
            {
                return null;
            }
            else
            {
                int count = Math.Abs(currentMonthIndex - 1);
                ans = new KMSelectable[count];
                for (int i = 0; i < count; i++)
                {
                    if (currentMonthIndex - 1 > 0)
                    {
                        ans[i] = left;
                    }
                    else
                    {
                        ans[i] = right;
                    }
                }
                return ans;
            }
        }
        else if (command.Trim().ToLowerInvariant().StartsWith("mar"))
        {
            if (currentMonthIndex == 2)
            {
                return null;
            }
            else
            {
                int count = Math.Abs(currentMonthIndex - 2);
                ans = new KMSelectable[count];
                for (int i = 0; i < count; i++)
                {
                    if (currentMonthIndex - 2 > 0)
                    {
                        ans[i] = left;
                    } else
                    {
                        ans[i] = right;
                    }
                }
                return ans;
            }
        }
        else if (command.Trim().ToLowerInvariant().StartsWith("apr"))
        {
            if (currentMonthIndex == 3)
            {
                return null;
            }
            else
            {
                int count = Math.Abs(currentMonthIndex - 3);
                ans = new KMSelectable[count];
                for (int i = 0; i < count; i++)
                {
                    if (currentMonthIndex - 3 > 0)
                    {
                        ans[i] = left;
                    }
                    else
                    {
                        ans[i] = right;
                    }
                }
                return ans;
            }
        }
        else if (command.Trim().ToLowerInvariant().StartsWith("may"))
        {
            if (currentMonthIndex == 4)
            {
                return null;
            }
            else
            {
                int count = Math.Abs(currentMonthIndex - 4);
                ans = new KMSelectable[count];
                for (int i = 0; i < count; i++)
                {
                    if (currentMonthIndex - 4 > 0)
                    {
                        ans[i] = left;
                    }
                    else
                    {
                        ans[i] = right;
                    }
                }
                return ans;
            }
        }
        else if (command.Trim().ToLowerInvariant().StartsWith("jun"))
        {
            if (currentMonthIndex == 5)
            {
                return null;
            }
            else
            {
                int count = Math.Abs(currentMonthIndex - 5);
                ans = new KMSelectable[count];
                for (int i = 0; i < count; i++)
                {
                    if (currentMonthIndex - 5 > 0)
                    {
                        ans[i] = left;
                    }
                    else
                    {
                        ans[i] = right;
                    }
                }
                return ans;
            }
        }
        else if (command.Trim().ToLowerInvariant().StartsWith("jul"))
        {
            if (currentMonthIndex == 6)
            {
                return null;
            }
            else
            {
                int count = Math.Abs(currentMonthIndex - 6);
                ans = new KMSelectable[count];
                for (int i = 0; i < count; i++)
                {
                    if (currentMonthIndex - 6 > 0)
                    {
                        ans[i] = left;
                    }
                    else
                    {
                        ans[i] = right;
                    }
                }
                return ans;
            }
        }
        else if (command.Trim().ToLowerInvariant().StartsWith("aug"))
        {
            if (currentMonthIndex == 7)
            {
                return null;
            }
            else
            {
                int count = Math.Abs(currentMonthIndex - 7);
                ans = new KMSelectable[count];
                for (int i = 0; i < count; i++)
                {
                    if (currentMonthIndex - 7 > 0)
                    {
                        ans[i] = left;
                    }
                    else
                    {
                        ans[i] = right;
                    }
                }
                return ans;
            }
        }
        else if (command.Trim().ToLowerInvariant().StartsWith("sep"))
        {
            if (currentMonthIndex == 8)
            {
                return null;
            }
            else
            {
                int count = Math.Abs(currentMonthIndex - 8);
                ans = new KMSelectable[count];
                for (int i = 0; i < count; i++)
                {
                    if (currentMonthIndex - 8 > 0)
                    {
                        ans[i] = left;
                    }
                    else
                    {
                        ans[i] = right;
                    }
                }
                return ans;
            }
        }
        else if (command.Trim().ToLowerInvariant().StartsWith("oct"))
        {
            if (currentMonthIndex == 9)
            {
                return null;
            }
            else
            {
                int count = Math.Abs(currentMonthIndex - 9);
                ans = new KMSelectable[count];
                for (int i = 0; i < count; i++)
                {
                    if (currentMonthIndex - 9 > 0)
                    {
                        ans[i] = left;
                    }
                    else
                    {
                        ans[i] = right;
                    }
                }
                return ans;
            }
        }
        else if (command.Trim().ToLowerInvariant().StartsWith("nov"))
        {
            if (currentMonthIndex == 10)
            {
                return null;
            }
            else
            {
                int count = Math.Abs(currentMonthIndex - 10);
                ans = new KMSelectable[count];
                for (int i = 0; i < count; i++)
                {
                    if (currentMonthIndex - 10 > 0)
                    {
                        ans[i] = left;
                    }
                    else
                    {
                        ans[i] = right;
                    }
                }
                return ans;
            }
        }
        else if (command.Trim().ToLowerInvariant().StartsWith("dec"))
        {
            if (currentMonthIndex == 11)
            {
                return null;
            }
            else
            {
                int count = Math.Abs(currentMonthIndex - 11);
                ans = new KMSelectable[count];
                for (int i = 0; i < count; i++)
                {
                    if (currentMonthIndex - 11 > 0)
                    {
                        ans[i] = left;
                    }
                    else
                    {
                        ans[i] = right;
                    }
                }
                return ans;
            }
        }else if (command.Trim().ToLowerInvariant().StartsWith("colorblind"))
        {
            colorblindObj.SetActive(true);
            Debug.LogFormat("[Calendar #{0}] Colorblind mode enabled via TP command.", _moduleId);
            LED.GetComponent<MeshRenderer>().material = off;
            return new KMSelectable[0];
        }
        else
        {
            return null;
        }
        return null;
    }
}
