using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public delegate void FightInterrupt();
public class FightSetup
{
    // The middle spawn location
    static Vector2 MIDDLE_LOCATION = new Vector2(0.5f, 0.5f);

    // The data of the player [Where heroes are position, the heroes bonuses, etc..
    PlayerLineupData playerData;

    // Indicates where the player will be spawned
    // Stores points in the form of
    // Lower left, top left, top right, bottom right
    Vector3[] playerSpawnCorners;
    Vector3[][] playerBossSpawnCorners;

    // Indicates where the cooresponding waves of enemies will be spawned
    // Stores points of the the form
    // Lower left, top left, top right, bottom right
    Vector3[][] waveSpawnCorners;
    Vector3[][] bossSpawnLocationCorners;

    // Arrays storing all currently active enemies and player characters
    static LinkedList<Character> enemies;
    static LinkedList<Character> bosses; // If there's any bosses in this fight, then they're stored here
    static LinkedList<Character> playerHeroes;
    static LinkedList<Character> playerPets;
    static FightSetup singleTonSetup;

    Transform skillsUI; // UI that shows the heroes skills the player can use
    Transform worldCanvas; // The canvas that will display any realtime world-position UI

    LevelData levelData;
    int currentWaveIndex;
    int currentBossIndex;
    Character nextBoss;

    static bool completedWaves;
    Image bossUIHP;

    // Have we spaned a boss yet?
    bool hasSpawnedBoss = false;
    

    // Called before spawning any data
    public void SetupFight(LevelManager manager, RectTransform spawnLocation, RectTransform[] waveSpawns, int numWaves, Transform worldCanvas, Transform skillsUI, 
                           RectTransform[] playerBossSpawnLocation, RectTransform[] enemyBossSpawnLocation, Image bossHPBarsUI
        )
    {
        if (numWaves != waveSpawns.Length)
            throw new FightConfigError("Error, incorrect number of waves");

        // Setup new fight instance
        singleTonSetup = this;

        // Initialize data structures
        enemies = new LinkedList<Character> ();
        bosses = new LinkedList<Character> ();
        playerHeroes = new LinkedList<Character> ();
        playerPets = new LinkedList<Character>();
        this.worldCanvas = worldCanvas;
        this.skillsUI = skillsUI;
        this.bossUIHP = bossHPBarsUI;

        // Spawns
        this.bossSpawnLocationCorners = new Vector3[enemyBossSpawnLocation.Length][];
        this.playerBossSpawnCorners = new Vector3[playerBossSpawnLocation.Length][];

        if (enemyBossSpawnLocation.Length != playerBossSpawnLocation.Length)
            throw new FightConfigError("Error, Must be a spawn location for each boss");

        playerSpawnCorners = new Vector3[4];
        playerBossSpawnCorners = new Vector3[enemyBossSpawnLocation.Length][];

        spawnLocation.GetWorldCorners(playerSpawnCorners);

        // Initializing waves
        waveSpawnCorners = new Vector3[numWaves][];
        for (int i = 0; i < numWaves; i++)
        {
            waveSpawnCorners[i] = new Vector3[4];
            waveSpawns[i].GetWorldCorners(waveSpawnCorners[i]);
        }

        // Initializing bosses
        for (int i = 0; i < enemyBossSpawnLocation.Length; i++)
        {
            playerBossSpawnCorners[i] = new Vector3[4];
            bossSpawnLocationCorners[i] = new Vector3[4];
            enemyBossSpawnLocation[i].GetWorldCorners(bossSpawnLocationCorners[i]);
            playerBossSpawnLocation[i].GetWorldCorners(playerBossSpawnCorners[i]);
        }

        skillsUI.gameObject.SetActive (true); // FIXME remove this or keep it?
    }

    // Spawn the players information into the game
    void SpawnPlayerCharacters (CharacterPosition[] heroes, CharacterPosition[] pets)
    {
        if (heroes == null || pets == null || heroes.Length == 0 || pets.Length == 0)
        {
            throw new FightConfigError("Error, The fight was not properly setup before starting.");
        }

        int manualSkillsIndex = 1; // The current index of which heroes have manual skills

        // Spawn the heroes
        for (int i = 0; i < heroes.Length; i++)
        {
            CharacterBonuses heroInfo = heroes[i].CharacterStats;
            
            // Obtain resource to be instantiated
            GameObject obj = (Resources.Load<GameObject>(FileSystem.GetHeroModelPathByName(heroInfo.CharacterName)));

            if (obj == null)
            {
                throw new SpawnFailure("Error, no hero model found.");
            }
            obj = GameObject.Instantiate(obj);

            // Parent the world canvas, so UI will be displayed [such as hp]
            obj.transform.SetParent(this.worldCanvas);

            Character playerHero = obj.GetComponent<Hero>();

            if (playerHero == null)
                throw new ComponentNotFound("Error, Hero not found");

            // Configure the hero data
            playerHero.Init(enemies, bosses, playerHeroes, playerPets, true, heroInfo, heroes[i].CharPosition, true);

            playerHeroes.AddFirst(playerHero); // Add heroes to list of characters in game
            ConfigurePlayerUI(playerHero, manualSkillsIndex++);
            MoveToSpawnLocation(obj, heroes[i].CharPosition, playerSpawnCorners [0], playerSpawnCorners[2]);
        }

        // Spawn the pets
        for (int i = 0; i < pets.Length; i++)
        {
            CharacterBonuses petInfo = pets[i].CharacterStats;

            // Obtain resource to be instantiated
            GameObject obj = Resources.Load<GameObject>(FileSystem.GetPetModelPathByName(petInfo.CharacterName));

            if (obj == null)
            {
                throw new SpawnFailure("Error, no hero model found.");
            }
            obj = GameObject.Instantiate(obj);

            // Parent the world canvas, so UI will be displayed [such as hp]
            obj.transform.SetParent(this.worldCanvas);

            Character playerPet = obj.GetComponent<Pet>();

            if (playerPet == null)
                throw new ComponentNotFound("Error, Pet not found");

            // Configure the pet data
            playerPet.Init(enemies, bosses, playerHeroes, playerPets, true, petInfo, pets[i].CharPosition, true);
            
            playerPets.AddLast(playerPet); // Add pets to list of characters in game
            ConfigurePlayerUI(playerPet, manualSkillsIndex++);
            MoveToSpawnLocation(obj, pets[i].CharPosition, playerSpawnCorners[0], playerSpawnCorners[2]);
        }
    }

    // Spawn the cooresponding wave of enemies
    void SpawnEnemyWave (int waveNumber, CharacterPosition[] heroes)
    {
        if (heroes == null || heroes.Length == 0)
        {
            throw new FightConfigError("Error, The fight was not properly setup before starting.");
        }

        // Spawn them within the enemy spawning region template
        for (int i = 0; i < heroes.Length; i++)
        {
            CharacterBonuses heroInfo = heroes[i].CharacterStats;

            // Obtain resource to be instantiated
            GameObject obj = (Resources.Load<GameObject>(FileSystem.GetHeroModelPathByName(heroInfo.CharacterName)));

            if (obj == null)
            {
                throw new SpawnFailure("Error, no hero model found.");
            }
            obj = GameObject.Instantiate(obj);

            // Parent the world canvas, so UI will be displayed [such as hp]
            obj.transform.SetParent(this.worldCanvas);

            Character enemHero = obj.GetComponent<Hero>();

            if (enemHero == null)
                throw new ComponentNotFound("Error, Hero not found");

            // Configure the enemy data
            // Note that, to an enemy, the players heroes are bosses and the enemy bosses are ally heroes
            // The enemies are ally pets
            enemHero.Init(playerPets, playerHeroes, bosses, enemies, false, heroInfo, heroes[i].CharPosition, true);
            enemies.AddFirst(enemHero); // Add enemies to list of enemies in game
            ConfigureEnemyUI(enemHero);
            MoveToSpawnLocation(obj, heroes[i].CharPosition, waveSpawnCorners [waveNumber][0], waveSpawnCorners[waveNumber][2]);
        }
    }

    // Spawn the cooresponding bosses of enemies
    void SpawnEnemyBoss (int bossNumber, CharacterBonuses bossStats)
    {
        if (bossStats == null)
            throw new FightConfigError("Error, the fight was not properly setup.");

        GameObject obj = Resources.Load<GameObject>(FileSystem.GetBossModelPathByName(bossStats.CharacterName));

        if (obj == null)
        {
            throw new SpawnFailure("Error, no boss model found.");
        }
        obj = GameObject.Instantiate(obj);

        obj.transform.SetParent(this.worldCanvas);

        Character bossChar = obj.GetComponent<Boss>();

        if (bossChar == null)
            throw new ComponentNotFound("Error, boss not found");

        // The spawn location of the boss
        Vector2 spawnLocation = MIDDLE_LOCATION;

        // Configure the boss data
        bossChar.Init(playerPets, playerHeroes, bosses, enemies, false, bossStats, MIDDLE_LOCATION, false);
        MoveToSpawnLocation(obj, spawnLocation, bossSpawnLocationCorners[bossNumber][0], bossSpawnLocationCorners[bossNumber][2]);
        nextBoss = bossChar;

        SetupPlayerCharactersForBosses(bossNumber, spawnLocation);
        hasSpawnedBoss = true;
    }

    /// <summary>
    /// Prepares characters for boss fights
    /// Before we allow the boss to be fought, lets move all the players heroes to the correct location
    /// Then, we will also set an increased attack range for the heroes
    /// </summary>
    void SetupPlayerCharactersForBosses (int bossNumber, Vector2 spawnLocation)
    {
        foreach (Character i in playerHeroes)
        {
            MovePlayerHeroesToPositionBeforeAction(i, playerBossSpawnCorners[bossNumber][0], playerBossSpawnCorners[bossNumber][2]);

            // Add the extra attack range to each character based on the distance to the closest point of the center spawning location
            // TODO: Double check to make sure that these are the correct corners to use for the extra boss attack range
            // It's possible it might be the left corners, HOW do i define a consistent set of corners if they depend on the orientation of the spite???
            i.ExtraBossAttackRange = Vector2.Distance(spawnLocation, new Vector2(
                (playerBossSpawnCorners[bossNumber][1].x + playerBossSpawnCorners[bossNumber][2].x) / 2f,
                (playerBossSpawnCorners[bossNumber][1].z + playerBossSpawnCorners[bossNumber][2].z) / 2f
                ));
        }

        foreach (Character i in playerPets)
        {
            MovePlayerHeroesToPositionBeforeAction(i, playerBossSpawnCorners[bossNumber][0], playerBossSpawnCorners[bossNumber][2]);

            // Add the extra attack range to each character based on the distance to the closest point of the center spawning location
            // TODO: Double check to make sure that these are the correct corners to use for the extra boss attack range
            // It's possible it might be the left corners, HOW do i define a consistent set of corners if they depend on the orientation of the spite???
            i.ExtraBossAttackRange = Vector2.Distance(spawnLocation, new Vector2(
                (playerBossSpawnCorners[bossNumber][1].x + playerBossSpawnCorners[bossNumber][2].x) / 2f,
                (playerBossSpawnCorners[bossNumber][1].z + playerBossSpawnCorners[bossNumber][2].z) / 2f
                ));
        }
    }

    void MovePlayerHeroesToPositionBeforeAction (Character chararacter, Vector3 lowerLeftPos, Vector3 upperRightPos)
    {
        Vector3 heroLoc = chararacter.GetBossSpawnLocation();

        // Spawn locations
        float xOffset = (upperRightPos.x - lowerLeftPos.x) * heroLoc.x;
        float yOffset = (upperRightPos.z - lowerLeftPos.z) * heroLoc.y;

        chararacter.MoveToPos(new Vector3(lowerLeftPos.x + xOffset, lowerLeftPos.y, lowerLeftPos.z + yOffset), MoveToInterrupt, true);
    }

    /// <summary>
    /// The number of characters that have reached the target
    /// Once this number is how many characters were moved, we can perform the action we initially desired
    /// </summary>
    int numCharactersReachedDestination = 0;
    void MoveToInterrupt ()
    {
        numCharactersReachedDestination++;

        if (numCharactersReachedDestination >= (playerPets.Count + playerHeroes.Count))
        {
            // All characters have reached their respsective destination
            // Now, we can setup the boss fully
            BossSetup(this.nextBoss);
        }
    }

    void BossSetup (Character bossChar)
    {
        // Switch to the boss fight camera, if it's not done already
        // Note that, another wave fight cannot come after a boss.
        if (!hasSpawnedBoss)
            Camera.main.GetComponent<CameraBehaviour>().SwitchCameras();

        // Enabled player movement, so they can move heroes
        PlayerMovement.EnabledPlayerMovement (new Vector2 (bossChar.transform.position.x, bossChar.transform.position.z), playerHeroes, playerPets);
        
        bosses.AddFirst(bossChar);
        ConfigureBossUI(bossChar);
        this.nextBoss = null;
    }

    // Move the game object to the correct spawn location on the map
    // The location stores PERCENTAGES along the valid location
    // 0 on X axis = Leftmost side
    // 1.00 on Y axis = top side
    // 0.5 on x and Y axis = middle of spawn location
    // Arguments include, the object being moved
    // The location that the hero will be placed
    // And the corners of the spawn region where the hero will be spawned [Location of this region]
    void MoveToSpawnLocation (GameObject obj, Vector2 heroLoc, Vector3 lowerLeftSpawnRegion, Vector3 upperRightSpawnRegion)
    {
        // Spawn locations
        float xOffset = (upperRightSpawnRegion.x - lowerLeftSpawnRegion.x) * heroLoc.x;
        float yOffset = (upperRightSpawnRegion.z - lowerLeftSpawnRegion.z) * heroLoc.y;

        obj.transform.position = new Vector3(lowerLeftSpawnRegion.x + xOffset, lowerLeftSpawnRegion.y, lowerLeftSpawnRegion.z + yOffset);
    }

    // Configures any necessary hero UI before the fight begins
    // For example, the players characters have GREEN hp bars, whereas the default is red
    void ConfigurePlayerUI (Character player, int heroSkillIndex)
    {
        Image hpUI = player.gameObject.GetComponentInChildren<Image>();

        if (hpUI == null)
            throw new ComponentNotFound("Error, Image not found");

        // Add Hp bars to the hero models
        hpUI.sprite = Resources.Load<Sprite>(FileSystem.GetAllyHpImagePath ());
        player.HpBar = hpUI;

        // Initialize the component for the Characters skills
        // If the character has any that the player can use, then we display it to the player
        if (player.HasManualSkills())
        {
            Transform childObj = skillsUI.Find(HierarchyDependencies.GetSelectableSkillsName (heroSkillIndex));
            player.ManSkills = childObj; // Store a reference to the skills UI, so it can be disabled when the character dies
            if (childObj == null)
                throw new NoObjectExists("Error, No skill by that name exists");
            else
                SetupManualSkillUI(player, childObj);
        }

        // Setup tag for the player object
        player.IsTargetableAlly();
    }

    void SetupManualSkillUI (Character character, Transform skillUI)
    {
        if (character == null || skillsUI == null)
            throw new ElementNotDefined("Error, args do not exist");

        Dictionary<ManualSkills, string> skillNames = character.GetManualSkillNames();

        if (skillNames == null)
            throw new ElementNotDefined("Error, no skills found for this hero");
        else
        {
            // Set the skill UI for the hero based on their cooresponding skills
            // NOTE that the skill name MUST match the picture in the file system
            if (skillNames.TryGetValue(ManualSkills.PRIMARY, out string val) && val != null)
            {
                Image img = skillUI.Find(ManualSkills.PRIMARY.ToString()).GetComponent<Image>();
                img.enabled = true; // Enable the image for the user to see
                img.sprite
                    = Resources.Load<Sprite>(FileSystem.GetSkillImageByCharSkillName(character.GetCharacterName(), val));

                img.GetComponent<SkillPress>().SkillToRun = character.GetPrimarySkill();
            }
            if (skillNames.TryGetValue(ManualSkills.SECONDARY, out val) && val != null)
            {
                Image img = skillUI.Find(ManualSkills.SECONDARY.ToString()).GetComponent<Image>();
                img.enabled = true; // Enable the image for the user to see
                img.sprite
                    = Resources.Load<Sprite>(FileSystem.GetSkillImageByCharSkillName(character.GetCharacterName(), val));
                img.GetComponent<SkillPress>().SkillToRun = character.GetSecondarySkill();
            }
            if (skillNames.TryGetValue(ManualSkills.ULTIMATE, out val) && val != null)
            {
                Image img = skillUI.Find(ManualSkills.ULTIMATE.ToString()).GetComponent<Image>();
                img.enabled = true; // Enable the image for the user to see
                img.sprite
                    = Resources.Load<Sprite>(FileSystem.GetSkillImageByCharSkillName(character.GetCharacterName(), val));
                img.GetComponent<SkillPress>().SkillToRun = character.GetUltimateSkill();
            }
        }
    }

    // Setup enemy UI
    // Such as, RED hp bars, and gameobject tags
    void ConfigureEnemyUI (Character enemy)
    {
        Image hpUI = enemy.gameObject.GetComponentInChildren<Image>();

        // Add RED hp bars to hero models
        hpUI.sprite = Resources.Load<Sprite>(FileSystem.GetEnemHpImagePath());
        enemy.HpBar = hpUI;

        // If the enemy is targettable, [Player can target them with skills]
        // Then, we set their tag as such
        enemy.IsTargetableEnemy();
    }

    // Setup boss UI
    void ConfigureBossUI (Character boss)
    {
        this.bossUIHP.gameObject.SetActive(true);
        boss.HpBar = this.bossUIHP;

        // Setup boss as a targettable enemy
        boss.IsTargetableEnemy();
    }

    // Begin the level
    // Sets up all necessary configurations to start the fight
    public void BeginLevel (PlayerLineupData playerData, LevelData levelData)
    {
        if (playerSpawnCorners == null || playerSpawnCorners.Length == 0 || levelData == null || (levelData.WaveData.Length == 0 && levelData.BossData.Length == 0))
        {
            throw new FightConfigError("Error, The fight was not properly setup before starting.");
        }
        this.playerData = playerData;
        SpawnPlayerCharacters (playerData.GetHeroWaveData(), playerData.GetPetWaveData());
        this.levelData = levelData;
        currentWaveIndex = 0;
        currentBossIndex = 0;
        SpawnNextWave();
    }

    // The character was killed
    public static void Died (Character character)
    {
        if (enemies.Remove(character) || bosses.Remove(character))
        {
            // An opponent is dieing, all of our characters must check to see if it was our target, and move to a new target
            foreach (Character i in playerHeroes)
                i.DeadTargetCheck(character);
            foreach (Character i in playerPets)
                i.DeadTargetCheck(character);
        }
        else if (playerHeroes.Remove(character) || playerPets.Remove(character))
        {
            // Our hero is dieing, all enemies must check to see if it was their target, and move to a new target
            foreach (Character i in enemies)
                i.DeadTargetCheck(character);
            foreach (Character i in bosses)
                i.DeadTargetCheck(character);
        }
        else
        {
            throw new ElementNotDefined("Error, this character was not defined as targetable, yet died.");
        }

        // Check to see if the player completed the wave
        // If there's no more enemies or bosses, then we spawn the next wave
        if (!completedWaves && enemies.Count == 0)
        {
            singleTonSetup.SpawnNextWave();
        }
        else if (completedWaves && bosses.Count == 0)
        {
            CompletedFight();
        }
    }

    public LinkedList<Character> GetPlayerHeroes ()
    {
        return playerHeroes;
    }

    void SpawnNextWave()
    {
        if (currentWaveIndex < levelData.WaveData.Length)
        {
            SpawnEnemyWave(currentWaveIndex, levelData.WaveData[currentWaveIndex]);
            currentWaveIndex++;
        }
        else
        {
            BeginBossFight();
        }
    }

    void SpawnNextBoss()
    {
        if (currentBossIndex < levelData.BossData.Length)
        {
            // Spawn the next boss
            SpawnEnemyBoss(currentBossIndex, levelData.BossData[currentBossIndex]);
            currentBossIndex++;
        }
        else
        {
            CompletedFight();
        }
    }

    /// <summary>
    /// Setup the boss fight
    /// If there were no waves, then this will run directly 
    /// Move players to the boss spawn region
    /// Creates the bosses in their cooresponding enemey boss spawn region
    /// </summary>
    void BeginBossFight()
    {
        // We have completed the round!!!
        completedWaves = true;

        // Check if there even is any bosses
        if (levelData.BossData.Length == 0)
        {
            CompletedFight();
        }
        else
        {
            // There are bosses!
            currentBossIndex = 0;
            SpawnNextBoss();
        }
    }

    /// <summary>
    /// The player has completed the fight!
    /// Give them the rewards, and completion menu
    /// </summary>
    static void CompletedFight ()
    {
        Debug.Log("Fight completed!");
    }
}
