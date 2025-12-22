using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConditionTypeLv
{
    public int ConditionType;
    public int ConditionLv;

    public ConditionTypeLv(int type, int lv)
    {
        ConditionType = type;
        ConditionLv = lv;
    }
}

[System.Serializable]
public class EnemyPattern
{
    [HideInInspector] public Enemy host;
    public int patternId;       // 패턴 ID
    public int patternNum;      // 패턴 실행번호
    public bool isManagerPat;   // 매니저가 패턴을 실행할지
    public bool isMovePat;      // 이동 패턴인지
    public int phase;           // 해당 페이즈

    [Header("weight이 0이면 발동안됨")]
    public int weight;          // 발동 확률

    // Todo : 창현_250408_미사용 변수 같음
    public bool isCancelabe;    // 발동중 취소 되는 패턴인지

    protected bool isFinished;  // 패턴이 끝났는지

    public float patAfterDelay; // 패턴 시전후 대기시간
    protected WaitForSeconds patAfterDelaySec;

    [HideInInspector] public bool isActiveBuff = false;
    public List<ConditionTypeLv> ConditionBuffs;
    public List<ConditionTypeLv> StatBuffs;

    [HideInInspector] public IEnumerator HostPatCoroutine;      // 호스트 기반 패턴 실행이 담기는 코루틴
    [HideInInspector] public IEnumerator ManagerPatCoroutine;   // 매니저 기반 패턴 실행이 담기는 코루틴

    public float PatAfterDelay
    {
        get => patAfterDelay;
        set { patAfterDelay = value; patAfterDelaySec = new WaitForSeconds(patAfterDelay); }
    }

    public bool IsFinished { get => isFinished; }

    public virtual void Init() { patAfterDelaySec = new WaitForSeconds(patAfterDelay); }

    /// <summary>
    /// 기존 공격 패턴 실행하는 함수
    /// </summary>
    public virtual void HostExecute() { HostStartCoroutine(PatternExecuter()); }
    /// <summary>
    /// 기존 이동 패턴 실행하는 함수
    /// </summary>
    public void HostMoveExecute() { isMovePat = true;  HostStartCoroutine(PatternExecuter()); }

    public virtual IEnumerator Execute() { ApplyConditionBuffs(); isFinished = false; yield return null; }

    public virtual void PatternExceptionStart() { RemoveConditionBuffs(); isFinished = true; }

    public virtual void OnEnable() { }

    public virtual void OnDisable() { }

    public void ApplyConditionBuffs()
    {
        foreach (var item in ConditionBuffs)
        {
            switch (item.ConditionType)
            {
                case 0:
                    host.moveCondition.SetAddConditionLv(item.ConditionLv);
                    break;

                case 1:
                    host.stunCondition.SetAddConditionLv(item.ConditionLv);
                    break;

                case 2:
                    host.superArmoredCondition.SetAddConditionLv(item.ConditionLv);
                    break;

                case 3:
                    host.knockBackCondition.SetAddConditionLv(item.ConditionLv);
                    break;

                case 4:
                    host.IsInvincibility = true;
                    break;
            }
        }
        foreach (var item in StatBuffs)
        {
            host.unitStats.StatsAdd(item.ConditionLv, item.ConditionType);
        }
        isActiveBuff = true;
    }

    public void RemoveConditionBuffs()
    {
        if (!isActiveBuff) return;
        foreach (var item in ConditionBuffs)
        {
            switch (item.ConditionType)
            {
                case 0:
                    host.moveCondition.SetSubConditionLv(item.ConditionLv);
                    break;

                case 1:
                    host.stunCondition.SetSubConditionLv(item.ConditionLv);
                    break;

                case 2:
                    host.superArmoredCondition.SetSubConditionLv(item.ConditionLv);
                    break;

                case 3:
                    host.knockBackCondition.SetSubConditionLv(item.ConditionLv);
                    break;

                case 4:
                    host.IsInvincibility = false;
                    break;
            }
        }
        foreach (var item in StatBuffs)
        {
            host.unitStats.StatsSubtract(item.ConditionLv, item.ConditionType);
        }
        isActiveBuff = false;
    }

    // 호스트 기반으로 실행되는 코루틴
    public void HostStartCoroutine(IEnumerator enumerator)
    {
        HostPatCoroutine = enumerator;
        host.StartCoroutine(HostPatCoroutine);
    }

    // 매니저 기반으로 실행되는 코루틴
    public void ManagerStartCoroutine(IEnumerator enumerator)
    {
        ManagerPatCoroutine = enumerator;
        CoroutineManager.Instance.StartCoroutine(ManagerPatCoroutine);
    }

    /// <summary>
    /// Execute 호출 및 대기 후 다른 패턴 호출 하는 함수
    /// </summary>
    /// <returns></returns>
    public IEnumerator PatternExecuter()
    {
        // 이동 패턴이 아니면 에이전트 스탑
        if (isMovePat)
        {
            if (host.Agent != null) host.Agent.isStopped = false;
        }
        else
        {
            if (host.Agent != null) host.Agent.isStopped = true;
        }

        yield return Execute();

        yield return null;

        PatternExceptionStart();

        yield return patAfterDelaySec;

        // 현재 패턴이 이동 패턴이면 공격 패턴 실행, 공격 패턴이면 처음으로
        if (isMovePat)
        {
            host.AttackPattern();
        }
        else
        {
            host.SetStartCoroutine(host.PatternStart());
        }
    }
    /// <summary>
    /// 패턴 노드에서 패턴을 실행시키는 코루틴
    /// </summary>
    /// <returns></returns>
    public IEnumerator NodePatternExecuter()
    {
        // 이동 패턴이 아니면 에이전트 스탑
        if (isMovePat)
        {
            if (host.Agent != null) host.Agent.isStopped = false;
        }
        else
        {
            if (host.Agent != null) host.Agent.isStopped = true;
        }

        yield return Execute();

        yield return null;

        PatternExceptionStart();
    }
}

[System.Serializable]
public class AttackPattern : EnemyPattern
{
    [SerializeField]
    protected UnitySerializedDictionary<int, string> variantMapping;

    public UnitySerializedDictionary<int, string> VariantMapping { get => variantMapping; }

    protected ObjectPooling objPool;
    protected Character character;
    protected Transform chTrans;
    protected Transform hostTrans;
    protected Animator animator;

    //        float rangeSqr = 2.5f;      // 기준 범위 : 1.58f, 제곱근을 위해 2.5f로 미리 설정
    public float AtkRange;              // 사거리(피해를 주는)

    public float AtkDistance;           // 시전 거리(범위에 들면 스킬 시전)
    public float Cooldown;              // 패턴 재사용 대기시간
    public float damageMultiplier;      // 공격력 배수
    public float fixedDamage;           // 고정 피해량

    private bool isCooldownDone;        // 쿨타임이 끝났는지
    private float nowCooldown;          // 현재 쿨타임

    public bool IsCooldownDone { get => isCooldownDone; }

    public float PatternDamage()
    {
        return host.Damage * damageMultiplier + fixedDamage;
    }

    /// <summary>
    /// 에디터에서 호출 시 사용
    /// </summary>
    public virtual void EditorInit() { }

    public override void Init()
    {
        base.Init();
        objPool = Management.Instance.SpawnManager.ObjectPooling;
        character = Management.Instance.SpawnManager.Character;
        chTrans = character.transform;
        hostTrans = host.transform;
        animator = host.Animator;
    }

    private void StartCooldown()
    {
        nowCooldown = Cooldown;
        isCooldownDone = false;
    }

    public void CheckAtkPatCooldown()
    {
        nowCooldown -= Time.deltaTime;
        if (nowCooldown <= 0) isCooldownDone = true;
    }

    public override void PatternExceptionStart()
    {
        base.PatternExceptionStart();

        StartCooldown();
    }
}