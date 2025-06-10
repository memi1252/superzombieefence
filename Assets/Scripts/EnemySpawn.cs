using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EnemySpawn : MonoBehaviour
{
    IObjectPool<GameObject> enemyPool;
    public GameObject canvas;
    public Transform[] spawnPoints;
    public GameObject enemyPrefab;
    public GameObject enemyHpPrefab;
    public int enemyCount = 20;
    public float spawnTime;
    private int spawnCount = 0;
    
    
    private float currentTime = 0f;
    
    private void Awake()
    {
        // 풀 생성: 적 생성 및 반환 시 동작 정의
        enemyPool = new ObjectPool<GameObject>(
            CreateEnemy,
            OnEnemySpawned,
            OnEnemyReleased,
            DestroyEnemy,
            maxSize: enemyCount
        );
        
    }

    private void Start()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            GameObject enemy = enemyPool.Get(); 
            enemyPool.Release(enemy); 
        }
    }

    private void Update()
    {
        if (spawnCount <= enemyCount)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= spawnTime)
            {
                SpawnEnemy();
                currentTime = 0f; 
            }
        }
    }

    private GameObject CreateEnemy()
    {
        return Instantiate(enemyPrefab);
    }

    private void OnEnemySpawned(GameObject enemy)
    {
        enemy.SetActive(true);
    }

    private void OnEnemyReleased(GameObject enemy)
    {
        enemy.SetActive(false);
    }

    private void DestroyEnemy(GameObject enemy)
    {
        Destroy(enemy);
    }

    public void SpawnEnemy()
    {
        spawnCount++;
        GameObject enemy = enemyPool.Get();
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        agent.enabled = false;
        //enemy.transform.position = spawnPoints.position;
        enemy.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        agent.enabled = true;
        agent.ResetPath();
        agent.isStopped = false;
        GameObject enemyHp = Instantiate(enemyHpPrefab, enemy.GetComponent<EnemyFSM>().hpBarTransform);
        enemyHp.transform.SetParent(canvas.transform);
        enemy.GetComponent<NavMeshAgent>().enabled = true;
        enemyHp.GetComponent<Billboard>().target = Camera.main.transform;
        enemy.GetComponent<EnemyFSM>().hpSlider = enemyHp.transform.GetChild(2).GetComponent<Image>();
        enemy.GetComponent<EnemyFSM>().Init();
        enemy.GetComponent<EnemyFSM>().enemySpawn = this;
    }

    public void ReleaseEnemy(GameObject enemy)
    {
        // 적 반환
        enemyPool.Release(enemy);
    }
}
