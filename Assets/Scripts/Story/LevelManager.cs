using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Begins the level the player has selected
// (Initiated the necessary scripts)
public class LevelManager : MonoBehaviour
{
    [SerializeField]
    RectTransform spawnLocation = null;

    [SerializeField]
    RectTransform[] enemySpawns = null;

    [SerializeField]
    Transform worldCanvas = null;

    [SerializeField]
    Transform skillsUI = null;

    [SerializeField]
    RectTransform[] playerBossSpawnLocation = null;

    [SerializeField]
    RectTransform[] enemyBossSpawns = null;

    [SerializeField]
    Image bossHPBarUI = null;

    readonly FightSetup fight = new FightSetup();

    Vector3[] playerPos;

    CameraBehaviour cam;

    private void Awake()
    {
        if (spawnLocation == null || enemySpawns == null || enemySpawns.Length == 0 || 
            worldCanvas == null || skillsUI == null || playerBossSpawnLocation == null || enemyBossSpawns == null || bossHPBarUI == null)
        {
            throw new ElementNotDefined("Error, spawns not initialized.");
        }
    }

    // Starts the fight!
    void Start()
    {
        LevelData data = StoryData.GetLevelDataByChapterIndex(1, 1);
        fight.SetupFight(this, spawnLocation, enemySpawns, data.WaveData.Length, worldCanvas, skillsUI, playerBossSpawnLocation, enemyBossSpawns, bossHPBarUI); // Fights MUST always be setup before beginning the level FIXME dependency ??
        fight.BeginLevel(Player.GetPlayerLineup (), data);
        playerPos = new Vector3 [Player.GetPlayerLineup().GetHeroWaveData ().Length];
        cam = Camera.main.GetComponent<CameraBehaviour>();
    }

    private void Update()
    {
        // Make the camera center move based on the positions of the characters in the fight/level
        if (cam != null)
            cam.MoveCenterByPos(GetPlayerPositions());
        else if (!cam.enabled)
            cam = Camera.main.GetComponent<CameraBehaviour>(); // We may have switched cameras
        else
            throw new ElementNotDefined("Error, cam not defined.");
    }

    // Gets the positions of the players characters in the fight
    // [Casts the linked list into an array]
    // FIXME, can this be optimized / improved??
    Vector3[] GetPlayerPositions ()
    {
        System.Collections.Generic.LinkedList<Character> chars = fight.GetPlayerHeroes();

        int index = 0;
        foreach (Character i in chars)
        {
            if (i != null)
                playerPos[index++] = i.transform.position;
        }

        return playerPos;
    }
}
