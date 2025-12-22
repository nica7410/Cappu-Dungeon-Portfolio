using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum PatternNodeType
{
    NoChild,
    Sequence,
    Parallel,
}

[Serializable]
public class PatternNode
{
    public bool isRootNode = true;     // 부모 노드인지

    // 에디터 사용 멤버
    public string id = Guid.NewGuid().ToString();   // 노드 guid

    public string nodeTitle = "Pattern Node";       // 에디터 내 노드 타이틀
    public Rect nodePosition;               // 에디터 내 노드의 위치를 저장하는 필드

    [NonSerialized] public Enemy host;      // 패턴 실행하는 에너미
    public PatternNodeType nodeType;        // 노드 타입
    [SerializeReference] public List<PatternNode> children = new();

    [SerializeReference] public AttackPattern pattern = new();       // 기존 AttackPattern 객체 참조
    public IEnumerator nowNodeCoroutine;    // 현재 노드가 담기는 코루틴
    public IEnumerator nowPatCoroutine;     // 현재 패턴이 담기는 코루틴

    public bool useOwnData;             // 어택패턴이 아닌 자신의 데이터 사용인지
    public int phase;                   // 해당 페이즈
    public int weight;                  // 발동 확률
    public float AtkDistance;           // 시전 거리(범위에 들면 스킬 시전)
    public float Cooldown;              // 패턴 재사용 대기시간

    public float delayNum;              // 패턴 실행 전 대기시간, 패턴 시작 타이밍 조절
    public WaitForSeconds delaySec;

    public float patAfterDelay;         // 패턴 시전 후 대기시간, 패턴 종료 후 다음 패턴 타이밍 조절
    protected WaitForSeconds patAfterDelaySec;

    public bool IsAnimPat;              // 애니메이션을 사용하는 패턴인지
    public bool IsAnimStart;            // 애니메이션이 실제로 시작을 했는지
    public AnimationClip animClip;      // 외부 애니메이션 연동용 필드

    private bool isCooldownDone;        // 쿨타임이 끝났는지
    private float nowCooldown;          // 현재 쿨타임

    private bool isNodeFinished;        // 노드의 Execute 실행이 끝났는지
    private bool useManagerNodeCo;      // 노드 코루틴 실행을 코루틴 매니저로

    public bool IsCooldownDone { get => isCooldownDone; }

    public void Init()
    {
        delaySec = new WaitForSeconds(delayNum);
        patAfterDelaySec = new WaitForSeconds(patAfterDelay);

        if (pattern != null)
        {
            pattern.host = host;
            pattern.Init();
        }

        foreach (var item in children)
        {
            item.isRootNode = false;
            item.host = host;
            item.Init();
        }
    }

    public void OnEnable()
    {
        if (pattern != null) pattern.OnEnable();
        foreach (var item in children)
        {
            item.OnEnable();
        }
    }

    public void OnDisable()
    {
        if (pattern != null) pattern.OnDisable();
        foreach (var item in children)
        {
            item.OnDisable();
        }
    }

    /// <summary>
    /// 현재 패턴 노드 실행하는 함수
    /// </summary>
    public void HostStartCoroutine()
    {
        nowNodeCoroutine = Execute();

        useManagerNodeCo = IsChildManagerPat();
        if (useManagerNodeCo) CoroutineManager.Instance.StartCoroutine(nowNodeCoroutine);
        else host.StartCoroutine(nowNodeCoroutine);
    }

    // 자식 중 매니저 사용 패턴이 있으면 참 반환
    private bool IsChildManagerPat()
    {
        bool isManger = false;
        if (pattern != null && pattern.isManagerPat) return true;
        foreach (var child in children)
        {
            if (child.pattern != null && child.pattern.isManagerPat) return true;
            //if (child.IsChildManagerPat()) return true;
        }
        return isManger;
    }

    /// <summary>
    /// 현재 노드를 실행하는 코루틴.
    /// (호출할 때 반드시 MonoBehaviour를 통해 StartCoroutine으로 감싸서 실행)
    /// </summary>
    private IEnumerator Execute()
    {
        isNodeFinished = false;

        yield return delaySec;

        // 1) 먼저 패턴이 있으면 패턴을 실행하고 기다린다
        if (pattern != null)
        {
            nowPatCoroutine = pattern.NodePatternExecuter();
            if (pattern.isManagerPat) CoroutineManager.Instance.StartCoroutine(nowPatCoroutine);
            else host.StartCoroutine(nowPatCoroutine);

            while (!pattern.IsFinished)
            {
                yield return null;
            }
        }

        switch (nodeType)
        {
            case PatternNodeType.NoChild:
                break;

            case PatternNodeType.Sequence:
                // 자식 노드를 순서대로(직렬) 실행
                foreach (var child in children)
                {
                    child.HostStartCoroutine();
                    while (!child.isNodeFinished)
                    {
                        yield return null;
                    }
                }
                break;

            case PatternNodeType.Parallel:
                bool isFirstChild = true;
                // 자식 노드 전체 시작
                // 첫번째 노드가 애니메이션이면 실제 시작까지 대기
                foreach (var child in children)
                {
                    child.HostStartCoroutine();
                    if (isFirstChild && child.IsAnimPat)
                    {
                        // 첫 번째 child가 애니메이션 패턴일 경우, 애니메이션 시작 신호(IsAnimStart)가 true가 될 때까지 대기
                        while (!child.IsAnimStart)
                        {
                            yield return null;
                        }
                        isFirstChild = false;  // 첫 번째 반복 후에는 무조건 false로 설정
                    }
                }
                break;
        }
        // 자식이 전부 끝날 때까지 대기
        bool allFinished = false;
        while (!allFinished)
        {
            allFinished = true;
            foreach (var child in children)
            {
                if (!child.isNodeFinished)
                {
                    allFinished = false;
                    break;
                }
            }
            yield return null;
        }

        // 노드 끝
        isNodeFinished = true;

        // 루트 노드면 대기 및 쿨타임 호출
        if (isRootNode)
        {
            host.SetStartCoroutine(StopPatternWithStart());
        }
    }

    public IEnumerator StopPatternWithStart()
    {
        StopPattern();
        yield return patAfterDelaySec;
        if (!host.IsDeath) host.SetStartCoroutine(host.PatternStart());
    }

    /// <summary>
    /// 매니저 코루틴을 제외한 모든 패턴 및 노드 정지
    /// </summary>
    public void StopPattern()
    {
        if (isRootNode) StartCooldown();

        if (nowNodeCoroutine != null && !useManagerNodeCo) host.StopCoroutine(nowNodeCoroutine);
        isNodeFinished = true;

        // 패턴이 존재하고, 매니저 코루틴이 아니면 스탑
        if (pattern != null && !pattern.isManagerPat)
        {
            if (nowPatCoroutine != null) host.StopCoroutine(nowPatCoroutine);
            pattern.PatternExceptionStart();
        }

        foreach (var child in children)
        {
            child.StopPattern();
        }
    }

    /// <summary>
    /// 모든 패턴 및 노드 정지
    /// </summary>
    public void StopAllPattern()
    {
        StopPattern();
        if (pattern != null && nowPatCoroutine != null)
        {
            if (pattern.isManagerPat) CoroutineManager.Instance.StopCoroutine(nowPatCoroutine);
            else host.StopCoroutine(nowPatCoroutine);

            pattern.PatternExceptionStart();
        }
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
}