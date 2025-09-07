using System;
using System.Collections.Generic;
using UnityEngine;

public interface IUniqueSkillComponent
{
    public Character ch { get; set; }
    public int SkillID { get; set; }
    public int SkillLevel { get; set; }

    /// <summary>
    /// 0: "X"<br/>
    /// 1: "일반"<br/>
    /// 2: "방향 지정"<br/>
    /// 3: "대상 지정"<br/>
    /// 4: "범위 지정"<br/>
    /// 5: "홀딩"<br/>
    /// 6: "패시브"<br/>
    /// 100: "타수 연결"<br/>
    /// 101: "정보"<br/>
    /// </summary>
    public int SkillExecutionType { get; set; }

    public EffectLevelActivator SkillEffect { get; set; }
    public List<AudioClip> audioClips { get; set; }
    public SkillStat skillStat { get; set; }
    public SkillDamageModifier damageModifier { get; set; }
    public CompositeAction.eSelectAction SelectAction { get; set; }
    public Func<float> MaxDuration { get; set; }
    public float NowDuration { get; set; }
    public bool UseallocatedSlot { get; set; }
    public UniqueSkillSlot allocatedSlot { get; set; }
    public int CurrentElementalStack { get; set; }
    /// <summary>
    /// 스킬 세팅을 위한 함수
    /// </summary>
    /// <param name="skill">세팅될 스킬</param>
    /// <param name="clickNum">타수</param>
    /// <param name="isFirstSkill">첫번째 스킬인지</param>
    public void AddSkillSetting(SkillShape skill, int clickNum, bool isFirstSkill);

    public int FinalDamage(int attackOrder = 0);

    public void ApplyConditionBuffs();

    public void RemoveConditionBuffs();
    public void SetAfterDelay(float addAfterDelay);
    public ElementalManager.AttributeType CurrentAttribute { get; set; }
}

public interface IUniqueBridgeComponent
{
    public CompositeAction.eSelectAction SelectAction { get; set; }
    public CompositeAction.eSelectAction ConditionAction { get; set; }
    public UniqueBridgeSlot allocatedDownSlot { get; set; }
    public UniqueBridgeSlot allocatedUpSlot { get; set; }
}
