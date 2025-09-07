using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyAdrObjPool : AddressableObjectPool<Enemy>
{
    private SpawnManager spawnManager;      // SpawnManager 인스턴스 참조
    public int indexNum;                    // 몬스터 소환시 인덱스
    public void Start()
    {
        spawnManager = Management.Instance.SpawnManager;        
    }
    public async Task GetUnitToStage(int enemyId, Vector3 pos, Transform targetTrans, Vector4 targetingPos)
    {
        Enemy enemy = await GetObject(enemyId);

        if (enemy != null)
        {            
            enemy.transform.position = pos;
            enemy.IsDeath = false;
            enemy.transform.SetParent(null);
            enemy.Target = targetTrans;
            enemy.IndexNum = indexNum;

            enemy.HitPointNow = enemy.HitPointMAX;
            ++indexNum;

            spawnManager.Enemies.Add(enemy);

            enemy.gameObject.SetActive(true);
            if (enemy.EnemyType == Enemy.eEnmeyType.Normal || enemy.EnemyType == Enemy.eEnmeyType.Creature)
            {
                enemy.MaxTargetPos = targetingPos;
                spawnManager.ObjectPooling.GetHpbar(enemy);
            }
            else if(enemy.EnemyType == Enemy.eEnmeyType.Boss)
            {
                ((Boss)enemy).BossStart();
            }
        }
        else
        {
            CappuDebug.LogError("Failed to spawn an object.");
        }
    }
    public async Task<Enemy> GetEnemy(int enemyId)
    {
        return await GetObject(enemyId);
    }
    public async Task<Enemy> GetEnemyInfo(int enemyId)
    {
        return await GetObjectInfo(enemyId);
    }
    public void SetEnemy(Enemy enemy, Vector3 pos, Transform targetTrans, Vector4 targetingPos)
    {
        if (enemy != null)
        {            
            enemy.transform.position = pos;
            enemy.SpawnedPos = pos;
            enemy.IsDeath = false;
            enemy.transform.SetParent(null);
            enemy.Target = targetTrans;
            enemy.IndexNum = indexNum;

            enemy.HitPointNow = enemy.HitPointMAX;
            ++indexNum;

            spawnManager.Enemies.Add(enemy);

            enemy.gameObject.SetActive(true);
            if (enemy.EnemyType == Enemy.eEnmeyType.Normal || enemy.EnemyType == Enemy.eEnmeyType.Creature)
            {
                enemy.MaxTargetPos = targetingPos;
                spawnManager.ObjectPooling.GetHpbar(enemy);
            }
            else if (enemy.EnemyType == Enemy.eEnmeyType.Boss)
            {
                ((Boss)enemy).BossStart();
            }
        }
        else
        {
            CappuDebug.LogError("Failed to spawn an object.");
        }
    }
    public void ReturnEnemy(int EnemyId, Enemy enemy)
    {
        enemy.IsDying = false;
        enemy.transform.position = new Vector3(999, 999, 999);
        enemy.transform.SetParent(spawnManager.EnemyParent.transform);
        enemy.IndexNum = 0;
        
        Management.Instance.EventActionManager.TriggerPoolEnemy(enemy);

        enemy.HitPointNow = enemy.HitPointMAX;
        enemy.gameObject.SetActive(false);
        ReturnObject(EnemyId, enemy);
    }
}
