﻿// Slot Action
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

#region 메인 메뉴 관련
public enum SlotType { M_Field, M_Up, M_Mat, E_FieldOri, E_FieldEquipment, E_UpEquipment, L_Field, L_Up, None};// M - Merge, E - Equip, L - Launch
public enum EquipSlotType { FieldOri, FieldEquipment, UpEquipment, None }
public enum LaunchSlotType { Field, Up, None }

// Card Slot
public enum TargetSlot { UpField, MatField, UpSlot, MatSlot } // Target Slot to move to when a slot is clicked on.

// Card Data
public enum CardType { Weapon, Item, none }
public enum Grade { Common, Rare, Epic, Unique, Legendary }
public enum EquipmentType { Head, Chest, Face, Hand, Ori }
public enum EssentialEquip { Head, Chest, Face, Hand, Essential }
public enum StartingMember { Zero, First, Second, Third, Forth, Fifth }
public enum DefaultItem { Default }
#endregion

#region 필드 관련
// 필드에서 플레이어의 위치
public enum Region
{
    None, TopLeft, TopRight, BottomLeft, BottomRight
}

public enum SpawnItem { enemy, subBoss, egggulp, enemyGroup, bossSlime } // Stage Event Manager 에서 사용

public enum EnemyType { Melee, Ranged, Explode, Projectile } // 적을 스폰할 때 어떤 타입의 적을 스폰할지 정하기 위해
#endregion

#region 음악 관련
/// <summary>
/// 드롭다운 메뉴로 만들어서 선택하기 위해서 enum으로 
/// </summary>
public enum StageMusicType
{
    [Description("Ghost Alley")]
    GhostAlley,
    [Description("Go Go Go")]
    GoGoGo,
    [Description("Go Go Faster Faster")]
    GoGoFasterFaster,
    [Description("Whenever Aliens!!!")]
    WhereverAnliens,
    [Description("I've Got Your Back!")]
    IveGotYourBack,
    [Description("Battle Mage")]
    BattleMage,
    [Description("Battle with Beasts")]
    BattleWithBeasts,
    [Description("Final Showdown")]
    FinalShowdown
}
#endregion

#region 보석 관련
public class GemProperties
{
    public Sprite gemSprite;
    public float gemSize;
    public int gemExp;

    public GemProperties(Sprite _gemSprite, float _gemSize, int _gemExp)
    {
        gemSprite = _gemSprite;
        gemSize = _gemSize;
        gemExp = _gemExp; 
    }
}
public class GemExp
{
    static int expBlue = 10000;
    static int expGreen = 20000;
    static int expPurple = 30000;
    static int expBigBlue = 55000;
    static int expBigGreen = 95000;
    static int expBigPurple = 135000;

    int[] expValues = new int[] { expBlue, expGreen, expPurple, expBigBlue, expBigGreen, expBigPurple };

    public int GetGemIndex(int _exp)
    {
        for (int i = 0; i < expValues.Length; i++)
        {
            if (_exp < expValues[i])
            {
                return i;
            }
        }
        return expValues.Length; // 모든 값보다 클 경우
    }
}

#endregion

#region 힌트 관련

#endregion

#region 슬롯 관련
public enum Slots { CardSlot, EquipSlot, LaunchSlot }
public class MyGrade
{
    public static int Common = 0;
    public static int Rare = 1;
    public static int Epic = 2;
    public static int Unique = 3;
    public static int Legendary = 4;

    //public static string[] mGrades = { "Common", "Rare", "Epic", "Legendary", "Mythic" };
    public static string[] mGrades = { "일반", "희귀", "고급", "전설", "신화" };

    public static Color[] GradeColors = new Color[]
    {
        new Color(.6f,.6f,.6f), // white #999999
        new Color(1,0.8f,0), // yellow  #FFCC00
        new Color(0,0.8f,1), // blue #00CCFF
        new Color(0.5f,1,0), // green #80FF00
        new Color(.7f,0.3f,1) // purple #B34DFF
    };

    public static Color[] GradeGlowColors = new Color[]
    {
        new Color(1f,1f,1f,0.25f), // white
        new Color(1,0.9f,0.6f,0.25f), // yellow
        new Color(0.5f,0.9f,0.7f,0.25f), // blue
        new Color(1,0.9f,0.7f, 0.25f), // green
        new Color(1,0.5f,0.9f, 0.25f) // purple
    };
}
#endregion

#region 스킬 관련
public class Skills
{
    public static string[] SkillNames = new string[]
    {
        "강철 피부",
        "느림보 최면술",
        "넓은 공격",
        "천하 무적",
        "파티 타임"
        //"Steel Body",
        //"Sluggish Slumber",
        //"Flash Damage",
        //"Invincible Body",
        //"Spicy Booster"
    };
    public static string[] SkillDescriptions = new string[]
    {
        "동료들이 몸으로 적들의 공격을 막아줍니다.",
        "잠시 최면을 걸어 적들을 느려지게 합니다.",
        "화면 안의 모든 적들에게 데미지를 줍니다.",
        "잠시동안 무적이 됩니다.",
        "잠시동안 자신과 동료들의 공격력을 올려줍니다."
        //"Your friends' body shield you from enemies.",
        //"Temporarily slow down all enemies on the screen for a duration.",
        //"Inflict periodic damage to all enemies on the screen.",
        //"Grant periodic invincibility to the player.",
        //"Periodically boost the attack power of both the player and allies."
    };
}

public class StaticValues
{
    public static int MaxLevel = 30;
    public static int MaxEvoStage = 3; // evo stage 0, 1, 2
    public static int MaxGrade = 5; // grade 0, 1, 2, 3, 4
    public static int MaxItemGrade = 3; // grade 0, 1, 2
    public static int MaxSkillNumbers = 5; // skill 1, 2, 3, 4, 5 : 3자리수의 백자리가 0이 될 수 없음
    public static int MaxEnemyNumbers;
    public static int MaxGemNumbers;

    public static float GemDropRate = .5f; // 보석 드롭 확률 60%
}
#endregion

#region 색깔 관련
public class Colors
{
    public static Color[] randomColors = 
        { Color.red, Color.yellow, Color.magenta, Color.white };
}
#endregion

#region 공식 관련
public class Equation
{
    public int GetDamage(int _originalDamage, int _damageBonus)
    {
        int damage = (int)(_originalDamage + (_originalDamage * _damageBonus / 100));
        return damage;
    }

    public int GetCriticalDamage(int _damage)
    {
        int criticalCoefficient = UnityEngine.Random.Range(5, 9);
        int criticalConstant = UnityEngine.Random.Range(1, 100);
        int _cDamage = (_damage * criticalCoefficient) + criticalConstant;
        return _cDamage;
    }

    public float GetCoolDownTime(float _rate, int _grade, int _evoStage, float _defaultCoolDownTime)
    {
        return _defaultCoolDownTime - (_rate * ((_grade + 4) + _evoStage));
    }

    public int GetSkillDamage(float _rate, int _grade, int _evoStage, float _defaultDamage)
    {
        return (int)(_defaultDamage * (int)(_rate * ((_grade + 4) + _evoStage)));
    }

    public float GetSkillDuration(float _rate, int _grade, int _evoStage, float _defaultDuration)
    {
        return _defaultDuration + (_rate * ((_grade + 4) + _evoStage));
    }

    public int GetSkillDamageBonus(float _rate, int _grade, int _evoStage, float _defaultDamageBonus)
    {
        return (int)(_defaultDamageBonus * (int)(8 * (((_grade + 1) * 1.5f) + _evoStage)));
    }

    public float GetSlowSpeedFactor(int _grade, int _evoStage)
    {
        // grade 0 부터 evo stage0 => 0,1,2,3,4,5.... 가 되도록 식을 만들었음.
        float slownessFactor = _evoStage * (3f + _grade);

        return (float)(.3f + (0.01 * slownessFactor));
    }

    public Vector2 GetSpawnablePos(float spawnConst, float offset)
    {
        Vector2 position = new Vector2();
        float f = UnityEngine.Random.value > .5f ? 1f : -1f;

        if (UnityEngine.Random.value > .5f)
        {
            position.x = UnityEngine.Random.Range(-spawnConst + offset, spawnConst - offset);
            position.y = f > 0 ? (spawnConst * f) - offset : (spawnConst * f) + offset;
        }
        else
        {
            position.y = UnityEngine.Random.Range(-spawnConst + offset, spawnConst - offset);
            position.x = f > 0 ? (spawnConst * f) - offset : (spawnConst * f) + offset;
        }

        return position;
    }
    public bool IsOutOfRange(Vector2 posToCheck, float _spawnConst)
    {
        if (posToCheck.x > _spawnConst || posToCheck.x < -_spawnConst
            || posToCheck.y > _spawnConst || posToCheck.y < -_spawnConst)
        {
            return true;
        }
        return false;
    }
}
#endregion

#region 피드백 관련
public enum EnemyColor { yellow, green, red, blue, purple, pink}
#endregion

#region 확장 함수
// enum description 사용
public static class EnumExtensions
{
    private static readonly Dictionary<Enum, string> _descriptionCache = new Dictionary<Enum, string>();

    public static string GetDescription(this Enum value)
    {
        if (_descriptionCache.TryGetValue(value, out string description))
        {
            return description;
        }

        FieldInfo field = value.GetType().GetField(value.ToString());
        DescriptionAttribute attribute = (DescriptionAttribute)field.GetCustomAttribute(typeof(DescriptionAttribute));
        description = attribute == null ? value.ToString() : attribute.Description;
        _descriptionCache[value] = description;
        return description;
    }
}
#endregion