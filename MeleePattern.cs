using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleePattern : AttackPattern
{
    public override void EditorInit()
    {
        if (variantMapping == null || variantMapping.Count == 0)
        {
            variantMapping = new UnitySerializedDictionary<int, string>()
            {
                {0, "Normal Melee" },
                {1, "Rush" },
                {2, "Smash" }
            };
        }
    }

    public override void Init()
    {
        base.Init();

        switch (patternId)
        {
            case 1:
                if (bulletIndicator == null) bulletIndicator = Resources.Load<SkillIndicator>(bulletIndicatorAddr);
                if (rushHitEffect == null) rushHitEffect = Resources.Load<GameObject>(bulletHitEffectAddr);
                if (rushEffect == null) rushEffect = Resources.Load<GameObject>(rushEffectAddr);
                break;

            case 2:
                if (smashArriveEffect == null) smashArriveEffect = Resources.Load<SkillIndicator>(smashArriveEffectAddr);
                if (smashHitEffect == null) smashHitEffect = Resources.Load<GameObject>(smashHitEffectAddr);

                smashEffectWaitTime = 0.4f;
                smashEffectWaitSeconds = new WaitForSeconds(smashEffectWaitTime);
                break;
        }
    }

    public override IEnumerator Execute()
    {
        yield return base.Execute();

        switch (patternId)
        {
            case 0:
                yield return NormalMeleeAtk();
                break;

            case 1:
                yield return RushPattern();
                break;

            case 2:
                yield return SmashPattern();
                break;
        }
    }

    private IEnumerator NormalMeleeAtk()
    {
        animator.SetInteger("animation", 3);

        float waitTime = 0; // 대기 시간
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            waitTime += Time.deltaTime;
            if (waitTime > 0.1f)
            {
                break;
            }
            yield return null;
        }

        float dis;      // 거리
        bool hit = false;
        float range = AtkRange;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.normalizedTime < 1.0f)
        {
            if (stateInfo.normalizedTime > 0.7f && !hit)
            {
                dis = (hostTrans.position - chTrans.position).sqrMagnitude;
                if (dis <= range * range) // 제곱근 계산
                {
                    host.isAttack = true;
                    host.GiveCharacterDamage(PatternDamage());

                    Management.Instance.StageSoundManager.MonsterAttackSoundtPlay(0);
                    character.CharacterkHit(hostTrans.position);
                    hit = true;
                }
            }
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }

        host.isAttack = false;
        animator.SetInteger("animation", 0);
    }

    [SerializeField] private List<SkillIndicator> storedSkillIndicators = new();  // 돌진 방향 화살표 저장 리스트
    [SerializeField] private List<GameObject> storedBulletHit = new();  // 탄환 이펙트 저장 리스트
    [SerializeField] private List<GameObject> storedRushEffect = new();  // 탄환 이펙트 저장 리스트

    public string bulletIndicatorAddr;
    public string bulletHitEffectAddr;
    public string rushEffectAddr;

    private SkillIndicator bulletIndicator;
    private GameObject rushHitEffect;
    private GameObject rushEffect;

    private SkillIndicator nowIndicator;
    private GameObject nowBulletHit;
    private GameObject nowRushEffect;

    private IEnumerator RushPattern()
    {
        // ---[ 설정 값들 ]------------------------------------------------
        float arrowFillTime = 0.5f;    // 화살표 게이지가 차는 데 걸리는 시간
        float dashDuration = 0.5f;     // 돌진에 걸리는 시간
        float dashDistance = 15f;      // 돌진 거리
        float dashSpeed = 32f;         // 돌진 속도
        Vector3 arrowScale = new(1f, 3f, 1f);
        float arrowSpawnForwardOffset = 2f;
        float arrowSpawnHeightOffset = 0.5f;
        Vector3 bulletHitScale = new(2, 2, 2);

        // (2) 돌진 목표 지점 계산 (y 고정)
        Vector3 targetPos = new(chTrans.position.x, hostTrans.position.y, chTrans.position.z);
        hostTrans.LookAt(targetPos);

        Vector3 arrivePos = hostTrans.position + hostTrans.forward * dashDistance;
        arrivePos.y = hostTrans.position.y;

        // (3) 화살표 인디케이터 소환
        nowIndicator = objPool.ReturnSkillBulletPooling(storedSkillIndicators, bulletIndicator);
        if (nowIndicator != null)
        {
            nowIndicator.transform.localScale = arrowScale;

            nowIndicator.transform.position
                = hostTrans.position + hostTrans.forward * arrowSpawnForwardOffset + Vector3.up * arrowSpawnHeightOffset;

            // 각도 회전 (기존 로직 유지: y축을 -90으로 고정)
            Vector3 dashDir = (arrivePos - hostTrans.position).normalized;
            Vector3 weirdYFix = new(dashDir.x, -90f, dashDir.z);
            nowIndicator.transform.rotation = Quaternion.LookRotation(weirdYFix);

            // (4) 화살표 게이지 채우기 (Shader _FillNum)
            nowIndicator.SetFillNum(0f);
        }

        float elapsed = 0f;
        while (elapsed < arrowFillTime && nowIndicator != null)
        {
            elapsed += Time.deltaTime;
            float ratio = Mathf.Clamp01(elapsed / arrowFillTime);
            nowIndicator.SetFillNum(ratio);
            yield return null;
        }

        // (5) 돌진 시작
        animator.SetInteger("animation", 1);

        nowRushEffect = objPool.ReturnSkillBulletPooling(storedRushEffect, rushEffect);

        elapsed = 0f;
        bool hasHit = false;

        float range = AtkRange;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            // 몬스터를 arrivePos로 이동
            hostTrans.position = Vector3.MoveTowards(
                hostTrans.position,
                arrivePos,
                dashSpeed * Time.deltaTime
            );

            if (nowRushEffect != null) nowRushEffect.transform.SetPositionAndRotation(hostTrans.position, hostTrans.rotation);

            // 아직 히트가 안 일어났으면 사거리 체크
            if (!hasHit)
            {
                float sqrDist = (hostTrans.position - chTrans.position).sqrMagnitude;
                if (sqrDist <= range * range)
                {
                    hasHit = true;
                    host.isAttack = true;
                    host.GiveCharacterDamage(PatternDamage());
                    character.CharacterkHit(hostTrans.position);

                    // 히트 이펙트 소환
                    nowBulletHit = objPool.ReturnSkillBulletPooling(storedBulletHit, rushHitEffect);
                    if (nowBulletHit != null)
                    {
                        nowBulletHit.transform.localScale = bulletHitScale;
                        nowBulletHit.transform.position = hostTrans.position + Vector3.up;
                    }

                    Management.Instance.StageSoundManager.MonsterAttackSoundtPlay(0);
                    yield return null;
                    host.isAttack = false;
                }
            }

            yield return null;
        }

        // (6) 마무리 처리
        animator.SetInteger("animation", 0);
    }

    [SerializeField] private List<SkillIndicator> storedSmashArrives = new();  // 스매시 도착 저장 리스트
    [SerializeField] private List<GameObject> storedSmashHit = new();  // 스매시 폭발 이펙트 저장 리스트

    public string smashArriveEffectAddr;
    public string smashHitEffectAddr;

    private SkillIndicator smashArriveEffect;   // 스매시 도착 원본
    private GameObject smashHitEffect;          // 스매시 폭발 원본

    private SkillIndicator nowSmashArrive;      // 현재 스매시 도착
    private GameObject nowSmashHit;             // 현재 스매시 폭발

    private float smashEffectWaitTime;
    private WaitForSeconds smashEffectWaitSeconds;    // 스매시 폭발 이펙트 대기시간

    private IEnumerator SmashPattern()
    {
        Vector3 start_pos = hostTrans.position;
        Vector3 end_pos = chTrans.position + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
        end_pos.y = hostTrans.position.y;
        float max_height = 5;
        float g = 50f;
        float tx, ty, tz, dat;
        var dh = end_pos.y - start_pos.y;
        var mh = max_height - start_pos.y;

        ty = Mathf.Sqrt(2 * g * mh);

        float a = g;
        float b = -2 * ty;
        float c = 2 * dh;

        dat = (-b + Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
        tx = -(start_pos.x - end_pos.x) / dat;
        tz = -(start_pos.z - end_pos.z) / dat;
        float elapsed_time = 0, posX, posY, posZ;
        Vector3 tpos;

        nowSmashArrive = objPool.ReturnSkillBulletPooling(storedSmashArrives, smashArriveEffect);// 에러 생기면 리턴형으로 바꾸기
        if (nowSmashArrive != null)
        {
            nowSmashArrive.transform.position = end_pos + Vector3.up * 0.5f;

            // 기준 범위 : 3.5f임으로 *2를 미리 계산해서 7으로 설정
            nowSmashArrive.transform.localScale = new Vector3(7, 7, 7);
            nowSmashArrive.SetFillNum(0f);
        }

        float durationRatio, arriveTime = dat;

        hostTrans.LookAt(end_pos);

        float speedMultiplier = 1.2f;  // 속도 배율
        while (elapsed_time <= arriveTime && nowSmashArrive != null)
        {
            elapsed_time += Time.deltaTime * speedMultiplier;

            posX = start_pos.x + tx * elapsed_time;
            posY = start_pos.y + ty * elapsed_time - 0.5f * g * elapsed_time * elapsed_time;
            posZ = start_pos.z + tz * elapsed_time;

            tpos = new Vector3(posX, posY, posZ);
            hostTrans.position = tpos;

            //_FillNum
            durationRatio = elapsed_time / arriveTime;
            nowSmashArrive.SetFillNum(durationRatio);
            yield return null;
        }

        float dis;      // 거리
        float range = AtkRange;
        dis = (hostTrans.position - chTrans.position).sqrMagnitude;
        if (dis <= range * range)
        {
            host.isAttack = true;
            host.GiveCharacterDamage(PatternDamage());
            character.CharacterkHit(hostTrans.position);
        }
        nowSmashHit = objPool.ReturnSkillBulletPooling(storedSmashHit, smashHitEffect);
        if (nowSmashHit != null)
        {
            nowSmashHit.transform.localScale = new Vector3(3, 3, 3);
            nowSmashHit.transform.position = end_pos + Vector3.up;
        }

        Management.Instance.StageSoundManager.MonsterAttackSoundtPlay(0);

        yield return null;

        host.isAttack = false;
    }

    private IEnumerator SmashEffectWait()
    {
        var smashHit = nowSmashHit;
        nowSmashHit = null;
        yield return smashEffectWaitSeconds;
        smashHit.SetActive(false);
        foreach (var item in storedSmashHit)
        {
            item.gameObject.SetActive(false);
        }
    }

    public override void PatternExceptionStart()
    {
        base.PatternExceptionStart();

        switch (patternId)
        {
            case 1:
                if (nowIndicator != null) nowIndicator.gameObject.SetActive(false); nowIndicator = null;
                if (nowBulletHit != null) nowBulletHit.SetActive(false); nowBulletHit = null;
                if (nowRushEffect != null) nowRushEffect.SetActive(false); nowRushEffect = null;
                foreach (var item in storedSkillIndicators)
                {
                    item.gameObject.SetActive(false);
                }
                foreach (var item in storedBulletHit)
                {
                    item.SetActive(false);
                }
                foreach (var item in storedRushEffect)
                {
                    item.SetActive(false);
                }
                break;

            case 2:
                if (nowSmashArrive != null)
                {
                    nowSmashArrive.gameObject.SetActive(false);
                    nowSmashArrive.transform.localScale = smashArriveEffect.transform.localScale;
                    nowSmashArrive = null;
                }
                if (nowSmashHit != null) host.StartCoroutine(SmashEffectWait());
                else
                {
                    foreach (var item in storedSmashHit)
                    {
                        item.gameObject.SetActive(false);
                    }
                }
                foreach (var item in storedSmashArrives)
                {
                    item.gameObject.SetActive(false);
                }
                break;
        }
    }
}