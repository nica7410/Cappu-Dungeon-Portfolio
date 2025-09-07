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

public enum OldeSelectUniqueComponent
{
    None,
    BridgeChainAction,
    BridgeChargeAction,
    BridgeCastAction,
    WhirlwindAction,
    ShadowFuryAction,
    SwordForceAction,
    GuillotineAction,
    ShoulderTackleAction,
    StompingAction,
    StrongWindAction,
    TwinAttackAction,
    LightningAction,
    LumenWaveAction,
    TargetThunderAction,
}

public enum eSelectUniqueComponent
{
    None,
	DemonicPrisonAction,
	EMPAttackAction,
	EnergyReleaseAction,
	EyeOfHellAction,
	LightningTornadoAction,
	PoisonExplosionAction,
	RisingAtkActionAction,
	MadnessofDarknessAction,
	SwordDanceAction,
	FearnessOfAbyssAction,
	BloodFloodAction,
	MadnessBloodBoomAction,
	HugeExplosionAction,
	AirAttackAction,
	IceBlockCrashAction,
	BlastFlameAction,
	LumenJudgementAction,
	PowerOfGravityAction,
	HoloOrbitalstrikeAction,
	LightningFieldAction,
	FlameArrowDropAction,
    BlizzardAction,
	DangerCloseAction,
	GloryBoundaryAction,
	LightningShockAction,
	WindBladeAction,
	FloatingWindStormAction,
	GroundScatterAction,
	FlameBreathAction,
	StormTornadoAction,
	PulseShotAction,
	MageSwordForceAction,
	IceRingAction,
	FlameTsunamiAction,
	GroundSlashAction,
    BridgeChainAction,
	IceAgeAction,
	IceFieldAction,
}

public class UniqueSkillComponentSelector
{
    public static IUniqueSkillComponent OldSelectUniqueSkillComponent(OldeSelectUniqueComponent selectUniqueComponent)
    {
        return selectUniqueComponent switch
        {
            OldeSelectUniqueComponent.None => null,
            OldeSelectUniqueComponent.BridgeChainAction => new BridgeChainAction(),
            OldeSelectUniqueComponent.BridgeChargeAction => new BridgeChargeAction(),
            OldeSelectUniqueComponent.BridgeCastAction => new BridgeCastAction(),
            OldeSelectUniqueComponent.WhirlwindAction => new WhirlwindAction(),
            OldeSelectUniqueComponent.ShadowFuryAction => new ShadowFuryAction(),
            OldeSelectUniqueComponent.SwordForceAction => new SwordForceAction(),
            OldeSelectUniqueComponent.GuillotineAction => new GuillotineAction(),
            OldeSelectUniqueComponent.ShoulderTackleAction => new ShoulderTackleAction(),
            OldeSelectUniqueComponent.StompingAction => new StompingAction(),
            OldeSelectUniqueComponent.StrongWindAction => new StrongWindAction(),
            OldeSelectUniqueComponent.TwinAttackAction => new TwinAttackAction(),
            OldeSelectUniqueComponent.LightningAction => new LightningAction(),
            OldeSelectUniqueComponent.LumenWaveAction => new LumenWaveAction(),
            OldeSelectUniqueComponent.TargetThunderAction => new TargetThunderAction(),
            _ => null,
        };
    }

    public static IUniqueSkillComponent SelectUniqueSkillComponent(eSelectUniqueComponent component)
	{
		return component switch
		{
        	eSelectUniqueComponent.DemonicPrisonAction => new DemonicPrison(),
        	eSelectUniqueComponent.EMPAttackAction => new EMPAttack(),
        	eSelectUniqueComponent.EnergyReleaseAction => new EnergyRelease(),
        	eSelectUniqueComponent.EyeOfHellAction => new EyeOfHell(),
        	eSelectUniqueComponent.LightningTornadoAction => new LightningTornado(),
        	eSelectUniqueComponent.PoisonExplosionAction => new PoisonExplosion(),
        	eSelectUniqueComponent.RisingAtkActionAction => new RisingAtkAction(),
        	eSelectUniqueComponent.MadnessofDarknessAction => new MadnessofDarkness(),
        	eSelectUniqueComponent.SwordDanceAction => new SwordDance(),
        	eSelectUniqueComponent.FearnessOfAbyssAction => new FearnessOfAbyss(),
        	eSelectUniqueComponent.BloodFloodAction => new BloodFlood(),
        	eSelectUniqueComponent.MadnessBloodBoomAction => new MadnessBloodBoom(),
        	eSelectUniqueComponent.HugeExplosionAction => new HugeExplosion(),
        	eSelectUniqueComponent.AirAttackAction => new AirAttack(),
        	eSelectUniqueComponent.IceBlockCrashAction => new IceBlockCrash(),
        	eSelectUniqueComponent.BlastFlameAction => new BlastFlame(),
        	eSelectUniqueComponent.LumenJudgementAction => new LumenJudgement(),
        	eSelectUniqueComponent.PowerOfGravityAction => new PowerOfGravity(),
        	eSelectUniqueComponent.HoloOrbitalstrikeAction => new HoloOrbitalstrike(),
        	eSelectUniqueComponent.LightningFieldAction => new LightningField(),
        	eSelectUniqueComponent.FlameArrowDropAction => new FlameArrowDrop(),
        	eSelectUniqueComponent.BlizzardAction => new Blizzard(),
        	eSelectUniqueComponent.DangerCloseAction => new DangerClose(),
        	eSelectUniqueComponent.GloryBoundaryAction => new GloryBoundary(),
        	eSelectUniqueComponent.LightningShockAction => new LightningShock(),
        	eSelectUniqueComponent.WindBladeAction => new WindBlade(),
        	eSelectUniqueComponent.FloatingWindStormAction => new FloatingWindStorm(),
        	eSelectUniqueComponent.GroundScatterAction => new GroundScatter(),
        	eSelectUniqueComponent.FlameBreathAction => new FlameBreath(),
        	eSelectUniqueComponent.StormTornadoAction => new StormTornado(),
        	eSelectUniqueComponent.PulseShotAction => new PulseShot(),
        	eSelectUniqueComponent.MageSwordForceAction => new MageSwordForce(),
        	eSelectUniqueComponent.IceRingAction => new IceRing(),
        	eSelectUniqueComponent.FlameTsunamiAction => new FlameTsunami(),
        	eSelectUniqueComponent.GroundSlashAction => new GroundSlash(),
            eSelectUniqueComponent.BridgeChainAction => new BridgeChainAction(),
            	eSelectUniqueComponent.IceAgeAction => new IceAge(),
        	eSelectUniqueComponent.IceFieldAction => new IceField(),
        _ => null,
		};
	}
}