using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class UniqueSkillComponentBase : IUniqueSkillComponent
{
    public Character ch { get; set; }
    public int SkillID { get; set; }
    public int SkillLevel { get; set; }
    public EffectLevelActivator SkillEffect { get; set; }
    public List<AudioClip> audioClips { get; set; }
    public SkillStat skillStat { get; set; }
    public SkillDamageModifier damageModifier { get; set; }
    public CompositeAction.eSelectAction SelectAction { get; set; }
    public int SkillExecutionType { get; set; }
    public Func<float> MaxDuration { get; set; }
    public float NowDuration { get; set; }
    public bool UseallocatedSlot { get; set; }
    public UniqueSkillSlot allocatedSlot { get; set; }
    public float FinalAfterDelay { get; set; }

    public List<EffectLevelActivator> storedSkillEffects = new();  // 이펙트가 담기는 곳
    public ElementalManager.AttributeType CurrentAttribute { get; set; }
    public int CurrentElementalStack { get; set; }
    public UniqueSkillComponentBase()
    {
        ch = Management.Instance.SkillManager.character;
        audioClips = new();

        NowDuration = 0;
        UseallocatedSlot = true;
        damageModifier = new();
        damageModifier.myUniqueSkill = this;
        damageModifier.DamageFormula = () => FinalDamage();
    }

    public abstract void AddSkillSetting(SkillShape skill, int clickNum, bool isFirstSkill);
    public abstract void ApplyConditionBuffs();
    public abstract int FinalDamage(int attackOrder = 0);
    public abstract void RemoveConditionBuffs();
    public void SetAfterDelay(float addAfterDelay)
    {
        FinalAfterDelay = addAfterDelay;
    }
    public void SetWaitAction(SkillShape skill, int clickNum)
    {
        WaitSkillAction waitSkillAction = new(FinalAfterDelay);
        waitSkillAction.OnExecuted += () =>
        {
            if (!UseallocatedSlot) return;
            allocatedSlot.SetMaxWaitTime(waitSkillAction.WaitTime);
            allocatedSlot.StartSkillWaitDuration();
        };

        skill.SkillStep.AddSelectAction(SelectAction, waitSkillAction, clickNum);
    }
    /// <summary>
    /// 이펙트 레벨 설정
    /// </summary>
    public void SetEffectLevel()
    {
        if (SkillEffect != null) SkillEffect.SetLevel(SkillLevel);
        foreach (var item in storedSkillEffects)
        {
            item.SetLevel(SkillLevel);
        }
    }
    /// <summary>
    /// 이펙트 레벨 설정
    /// </summary>
    /// <param name="effectOrigin"></param>
    /// <param name="storedEffects"></param>
    public void SetEffectLevel(EffectLevelActivator effectOrigin, List<EffectLevelActivator> storedEffects)
    {
        if (effectOrigin != null) effectOrigin.SetLevel(SkillLevel);
        foreach (var item in storedEffects)
        {
            item.SetLevel(SkillLevel);
        }
    }
    /// <summary>
    /// 지정한 값으로 이펙트 레벨 설정
    /// </summary>
    /// <param name="effectOrigin"></param>
    /// <param name="storedEffects"></param>
    public void SetEffectLevel(EffectLevelActivator effectOrigin, List<EffectLevelActivator> storedEffects, int level)
    {
        if (effectOrigin != null) effectOrigin.SetLevel(SkillLevel);
        foreach (var item in storedEffects)
        {
            item.SetLevel(level);
        }
    }
}