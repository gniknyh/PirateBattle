using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyManager {

    //当前场景敌人数量
    public static List<GameObject> enemyList = new List<GameObject>(); 
    //当前攻击敌人数量
    public static List<GameObject> enemiesAttackingPlayer = new List<GameObject>(); 
    //当前还有生命值的敌人
    public static List<GameObject> activeEnemies = new List<GameObject>();

    public static void RemoveEnemyFromList(GameObject g)
    {
        enemyList.Remove(g);
    }

    //Disables all enemy AI's
    public static void DisableAllAIControllers()
    {
        getActiveEnemies();
        if (activeEnemies.Count > 0)
        {
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                activeEnemies[i].GetComponent<AIController>().enabled = false;
            }
        }
    }

    //Returns a list of enemies that are currently active
    public static void getActiveEnemies()
    {
        activeEnemies.Clear();
        foreach (GameObject enemy in enemyList)
        {
            if (enemy != null && enemy.activeSelf)
                activeEnemies.Add(enemy);
        }
    }

    //Player has died
    public static void PlayerHasDied()
    {
        DisableAllAIControllers();
        enemyList.Clear();
    }

}
