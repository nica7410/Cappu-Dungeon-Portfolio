using System.Collections.Generic;
using UnityEngine;

public class FlameBreath : UniqueSkillComponentBase
{
    public FlameBreath()
    {
        SelectAction = CompositeAction.eSelectAction.onPointerDown;
        SkillExecutionType = 2;

        SkillEffect = Resources.Load<EffectLevelActivator>("Prefab/Effect/SkillEffect/Effect_19_FlameBreath_Skill");
        audioClips.Add(Resources.Load<AudioClip>("Audio/Skill/RPG3_FireMagicFlameThrower_P2_Loop_short"));

        CurrentAttribute = ElementalManager.AttributeType.Fire; // 기본 속성 설정
    }

    public override void AddSkillSetting(SkillShape skill, int clickNum, bool isFirstSkill)
    {
        if (isFirstSkill)
        {
            SkillCommandData commandData = new();
            commandData.SetNonTargetPointerDown(skill, skill.SkillButton, skillStat.AttackDistance);
            skill.SkillStep.AddCommanData(commandData, clickNum);
        }

        SkillExecutionType = 2;
        SetEffectLevel();

        SkillDataWrapper skillDataWrapper = new();

        BulletAction bulletAction = new(false, () => skillStat.DurationTime,
            () => new Vector3(skillStat.AttackRange / 4f + 0.5f, skillStat.AttackRange + 0.5f, 1f));
        bulletAction.IsSkip = true;

        BulletPosAndRotAction bulletPosAndRot
            = new(() => (ch.transform.position + skill.CurSkillCommand.skillInputData.ChNowNormalVector * 3f, skill.CurSkillCommand.skillInputData.ChNowNormalVector, true),
            bulletAction.ReturnBullet);
        bulletAction.AddDurationActions(bulletPosAndRot);

        BulletEffectAction bulletEffectAction
            = new(SkillEffect, storedSkillEffects, 0, 0, bulletAction.ReturnBullet, () => skillStat.AttackRange, () => skillStat.DurationTime, null);
        bulletAction.AddProgressCallback(0f, bulletEffectAction);

        BulletEffectPosAndRotAction bulletEffectPosAndRot
            = new(() => bulletEffectAction.GetBulletEffect().transform,
            () => ch.transform.position,
            null,
            () => skill.CurSkillCommand.skillInputData.ChNowNormalVector);
        bulletEffectAction.AddDurationActions(bulletEffectPosAndRot);

        RangeDamageAction rangeDamageAction
            = new(false, skillStat, 0.25f, skillStat.DestoryDelay, damageModifier, () => bulletAction.ReturnBullet().transform, null, true, true,
            (target) =>
            {
                ElementalManager.Instance.ApplyEffect(CurrentAttribute, target, ch);
                if (CurrentElementalStack >= 3) ElementalManager.Instance.ApplyDebuffEffect(CurrentAttribute, target, ch);
            }, null,
            ApplyConditionBuffs, RemoveConditionBuffs, null, null, true);
        bulletAction.AddProgressCallback(0f, rangeDamageAction);

        SoundAction soundAction = new(audioClips[0]);
        bulletAction.AddProgressCallback(0f, soundAction);

        SetWaitAction(skill, clickNum);
        skill.SkillStep.AddSelectAction(SelectAction, bulletAction, clickNum);
        skill.SkillStep.SetPointAddAtkNum(SelectAction, clickNum);

        bulletAction.OnExecuted += () =>
        {
            allocatedSlot.SetSlotValue(SkillUtils.CalculateMaxDuration(skillStat));
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