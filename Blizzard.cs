using System.Collections.Generic;
using UnityEngine;

public class Blizzard : UniqueSkillComponentBase
{
    public Blizzard()
    {
        SelectAction = CompositeAction.eSelectAction.onPointerUp;
        SkillExecutionType = 4;

        SkillEffect = Resources.Load<EffectLevelActivator>("Prefab/Effect/SkillEffect/Effect_49_IceBlockFissure_Skill");
        audioClips.Add(Resources.Load<AudioClip>("Audio/Skill/RPG3_IceMagicEpic_Blizzard"));

        CurrentAttribute = ElementalManager.AttributeType.Ice; // 기본 속성 설정
    }

    public override void AddSkillSetting(SkillShape skill, int clickNum, bool isFirstSkill)
    {
        if (isFirstSkill)
        {
            SkillCommandData commandData = new();
            commandData.SetFixedTargetPoint(skill, skill.SkillButton, skillStat.AttackDistance);
            skill.SkillStep.AddCommanData(commandData, clickNum);
        }

        SkillExecutionType = 4;
        SetEffectLevel();

        SkillDataWrapper skillDataWrapper = new();

        float startDelay = 0.35f;
        bool isAtkNumSkill = true;
        bool isFirstAtkSkip = true;

        RangeDamageAction rangeDamageAction
            = new(false, skillStat, startDelay, skillStat.DestoryDelay, damageModifier, null, () => skill.CurSkillCommand.skillInputData.SkillHitboxTrans.position,
            isAtkNumSkill, isFirstAtkSkip,
            (target) =>
            {
                ElementalManager.Instance.ApplyEffect(CurrentAttribute, target, ch);
                if (CurrentElementalStack >= 3) ElementalManager.Instance.ApplyDebuffEffect(CurrentAttribute, target, ch);
            }, null,
            ApplyConditionBuffs, RemoveConditionBuffs, null, skillDataWrapper);
        rangeDamageAction.IsSkip = true;

        BulletEffectAction bulletEffectAction
            = new(SkillEffect, storedSkillEffects, 0, 0, null, () => skillStat.AttackRange, () => skillStat.DurationTime, null, null, skillDataWrapper);
        bulletEffectAction.IsSkip = true;

        SoundAction soundAction = new(audioClips[0]);
        rangeDamageAction.AddProgressCallback(0f, soundAction);

        SetWaitAction(skill, clickNum);
        skill.SkillStep.AddSelectAction(SelectAction, rangeDamageAction, clickNum);
        skill.SkillStep.AddSelectAction(SelectAction, bulletEffectAction, clickNum);
        skill.SkillStep.SetPointAddAtkNum(SelectAction, clickNum);

        rangeDamageAction.OnExecuted += () =>
        {
            allocatedSlot.StartSkillWaitDuration();
            allocatedSlot.SetSlotValue(SkillUtils.CalculateMaxDuration(skillStat, isAtkNumSkill, isFirstAtkSkip, startDelay));
            allocatedSlot.StartSliderDuration();
        };
    }

    public override int FinalDamage(int attackOrder = 0)
    {
        int damage = 0;
        switch (attackOrder)
        {
            case 0: // 기본 데미지
                damage = skillStat.SkillDamage + (int)(ch.Damage * 0.1f);
                break;
        }
        return damage;
    }

    public override void ApplyConditionBuffs()
    {
        ch.normalAtackCondition.ActiveConditionLv(2);   // 2레벨 평타 불가
        ch.superArmoredCondition.SetAddConditionLv(4);  // 5레벨 슈퍼아머 면역
    }

    public override void RemoveConditionBuffs()
    {
        ch.normalAtackCondition.RemoveConditionLevel(2);                  // 평타 불가 해제
        ch.superArmoredCondition.SetSubConditionLv(4);  // 5레벨 슈퍼아머 면역 해제
    }
}