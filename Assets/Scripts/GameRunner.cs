using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Experimental.XR;

public class GameRunner : StateChangeListener {

    public static readonly int SPACE_EMPTY = 0;
    public static readonly int SPACE_SNAKE = 1;
    public static readonly int SPACE_APPLE = 2;
    public static readonly int SPACE_GOLD = 3;
    public static readonly int SPACE_BAD = 4;
    private const int SIZE = 10;
    private int[,,] space = new int[SIZE,SIZE,SIZE];

    private GameObject gameOrigin;

    public GameObject boundingBoxPrefab;
    public Snake snakePrefab;
    public Cube applePrefab;
    public Cube goldPrefab;
    public Cube badPrefab;

    public GameObject plus1Prefab;
    public GameObject plus3Prefab;
    public GameObject plusGoldPrefab;
    public GameObject minus2Prefab;

    private GameObject boundingBox;
    private Snake snake;
    private int score;
    private int applesCollected;
    private int goldAmount;
    private bool isHardMode;
    private static readonly float DEFAULT_TIME_INTERVAL = 0.4f;
    private static readonly float SLOW_TIME_INTERVAL = 1f;
    private static readonly float FAST_TIME_INTERVAL = 0.25f;
    private bool isPaused;
    private bool isReviving;
    private bool isTutorial;
    private Cube apple;
    private Cube gold;
    private List<Cube> bads;
    private List<IEnumerator> badCoroutines;

    public GameObject mainCamera;
    public AlertPrompt tutorialAlertPanel;
    public Text scoreLabel;
    public Text goldLabel;
    public DirectionCube directionCube;
    public AudioManager audioManager;

    /* * * * Lifecycle methods * * * */

    void Awake() {
        this.gameOrigin = GameObject.FindWithTag("GameOrigin");
    }

    // Start is called before the first frame update
    void Start() {
    }

    void OnEnable() {
        //Debug.Log("on enable");
        if (!this.isReviving) {
            this.setupGame();
        }
    }

    /* * * * Public getters * * * */

    public int getScore() { return this.score; }
    public int getApples() { return this.applesCollected; }
    public int getGoldAmount() { return this.goldAmount; }

    public bool canRevive() {
        int[] headCoordinates = this.snake.getHeadCoordinates();
        int headX = headCoordinates[0], headY = headCoordinates[1], headZ = headCoordinates[2];
        List<int> openDirections = new List<int>();
        if (this.inBounds(headX + 1, headY, headZ) &&
            this.valueAtCoordinates(headX + 1, headY, headZ) != SPACE_SNAKE) {
            openDirections.Add(Snake.DIRECTION_POS_X);
        }
        if (this.inBounds(headX, headY + 1, headZ) &&
            this.valueAtCoordinates(headX, headY + 1, headZ) != SPACE_SNAKE) {
            openDirections.Add(Snake.DIRECTION_POS_Y);
        }
        if (this.inBounds(headX, headY, headZ + 1) &&
            this.valueAtCoordinates(headX, headY, headZ + 1) != SPACE_SNAKE) {
            openDirections.Add(Snake.DIRECTION_POS_Z);
        }
        if (this.inBounds(headX - 1, headY, headZ) &&
            this.valueAtCoordinates(headX - 1, headY, headZ) != SPACE_SNAKE) {
            openDirections.Add(Snake.DIRECTION_NEG_X);
        }
        if (this.inBounds(headX, headY - 1, headZ) &&
            this.valueAtCoordinates(headX, headY - 1, headZ) != SPACE_SNAKE) {
            openDirections.Add(Snake.DIRECTION_NEG_Y);
        }
        if (this.inBounds(headX, headY, headZ - 1) &&
            this.valueAtCoordinates(headX, headY, headZ - 1) != SPACE_SNAKE) {
            openDirections.Add(Snake.DIRECTION_NEG_Z);
        }

        if (openDirections.Count > 0) {
            // choose an available direction for the snake to move if it does get revived
            int i = Random.Range(0, openDirections.Count);
            this.snake.setNextDirection(openDirections[i]);
            return true;
        }
        return false;
    }

    /* * * * Private setters * * * */

    private void setPaused(bool paused) {
        this.isPaused = paused;
        this.snake.setPaused(paused);
    }

    /* * * * StateChangeListener delegate * * * */

    public override void respondToStateChange(GameState newState) {
        //Debug.Log("respond to state change");
        if (newState == GameState.SettingPosition || newState == GameState.GameOver) {
            this.enabled = false;
        }
        else {
            this.enabled = true;
            if (newState == GameState.GameRunning || newState == GameState.GamePaused) {
                this.setPaused(newState == GameState.GamePaused);
            }
        }
    }

    /* * * * Game pathway steps * * * */

    ///<summary>Initializes the objects used by the game from their prefabs and the `space` 3D array and sets them up to begin the game, then activates the starting canvases.</summary>
    private void setupGame() {
        //Debug.Log("setup game");
        this.destroyObjects();
        this.initializeObjects();

        this.space = new int[SIZE,SIZE,SIZE];
        this.space[2,2,2] = SPACE_SNAKE;
        this.space[3,2,2] = SPACE_SNAKE;
        this.space[4,2,2] = SPACE_SNAKE;
        this.score = 0;
        this.applesCollected = 0;
        this.goldAmount = DataAndSettingsManager.getGoldAmount();
        this.isHardMode = DataAndSettingsManager.getHardModeState();
        this.isPaused = false;

        this.generateApple();

        this.updateScoreLabel();
        this.updateGoldLabel();
        DataAndSettingsManager.updateColorblindModeListeners();
    }

    ///<summary>Undoes the actions of `this.setupGame()` and reenables the GameStarter. This method is linked to a button on the canvas.</summary>
    public void changePositionAction() {
        // undo the setup and go back to GameStarter
        this.destroyObjects();
        GameStateManager.onPositionCancel();
    }

    public void startTutorialAction() {
        GameStateManager.onTutorialStart();
        this.isTutorial = true;
        this.snake.enabled = true;
        StartCoroutine("runGame");
        StartCoroutine("runTutorial");
    }

    private IEnumerator runTutorial() {
        float timeToMove = this.timeInterval();
        yield return StartCoroutine(this.pausableWait(timeToMove * 1.5f));
        yield return StartCoroutine(this.displayTutorialMessage("Welcome to s\u00b3, a 3D extension of the classic snake game!\n\n" +
            "Our little snake has already started moving. Swipe on the screen to change its direction!"));
        yield return StartCoroutine(this.pausableWait(timeToMove * 10));
        yield return StartCoroutine(this.displayTutorialMessage("No matter what angle you're looking from, " +
            "the game will figure out what horizontal direction you meant by swiping.\n\n" +
            "But to move upwards or downwards, press the arrow buttons below.\n\n" +
            "Swipe to continue!"));
        yield return StartCoroutine(this.pausableWait(timeToMove * 10));
        yield return StartCoroutine(this.displayTutorialMessage("Your goal is to eat as many apples (red cubes) as you can.\n\n" +
            "Try to grab this apple\u2014swipe to continue!"));
        int s = this.score;
        while (this.score == s) {
            yield return null;
        }
        yield return StartCoroutine(this.displayTutorialMessage("Nice! Apples and other things that appear will increase " +
            "(or decrease) your score.\n\n" +
            "It may be tough now, but this game will really train your depth perception and spatial awareness. " +
            "And once you get the hang of it, it's also really satisfying.\n\n" +
            "Continue this tutorial for as long as you want!"));
        yield break;
    }

    private IEnumerator displayTutorialMessage(string message) {
        this.isPaused = true;
        this.tutorialAlertPanel.showMessage(message);
        int direction = this.snake.getNextDirection();
        yield return new WaitForSeconds(0.2f);
        while (this.snake.getNextDirection() == direction && this.isTutorial) {
            yield return null;
        }
        this.tutorialAlertPanel.closeAction();
        this.isPaused = false;
        yield break;
    }

    private void stopTutorial() {
        StartCoroutine("reviveTutorial");
    }

    private IEnumerator reviveTutorial() {
        yield return StartCoroutine(this.displayTutorialMessage("Oh no, the snake hit something! This would normally end the game, " +
            "but in this tutorial, you may continue.\n\nSwipe to change direction and resume."));
        StartCoroutine("runGame");
        yield break;
    }

    public void quitTutorialAction() {
        StopCoroutine("runGame");
        StopCoroutine("runTutorial");
        StopCoroutine("reviveTutorial");
        this.isTutorial = false;
        GameStateManager.onPositionSet();
        this.setupGame();
    }

    ///<summary>Activates objects that should be active during the game and starts the game coroutines. This method is linked to a button on the canvas.</summary>
    public void actuallyStartGameAction() {
        //Debug.Log("actually start game");
        GameStateManager.onGameStart();
        this.snake.enabled = true;
        StartCoroutine("runGame");
        StartCoroutine("randomlyGenerateGold");
        foreach (Cube bad in this.bads) {
            IEnumerator coroutine = this.randomlyGenerateBad(bad);
            this.badCoroutines.Add(coroutine);
            StartCoroutine(coroutine);
        }
    }

    ///<summary>The main game coroutine - repeatedly moves the snake and checks for apples until the snake can't move.</summary>
    private IEnumerator runGame() {
        // keep moving the snake and checking for apples until the snake hits the wall or itself
        int[] next = this.snake.nextMove();
        while (this.validMove(next)) {
            float timeToMove = this.timeInterval();
            if (this.aboutToEatApple(next) && this.shouldGrow()) {
                this.snake.grow(timeToMove, DataAndSettingsManager.getSmoothMovementState());
            }
            else {
                int[] old = this.snake.move(timeToMove, DataAndSettingsManager.getSmoothMovementState());
                this.setValueAtCoordinates(old, SPACE_EMPTY);
            }
            yield return StartCoroutine(this.pausableWait(timeToMove)); // the player may pause during this time

            this.lookForApples(next);
            this.setValueAtCoordinates(next, SPACE_SNAKE);
            next = this.snake.nextMove();
        }
        if (this.isTutorial) {
            this.stopTutorial();
        }
        else {
            this.stopGame();
        }
        yield break;
    }

    ///<summary>Pauses the game coroutines and switches the canvas. This method is linked to the pause button on the canvas.</summary>
    public void pauseAction() {
        GameStateManager.onGamePause();
    }

    private void stopGame() {
        //Debug.Log("stop game");
        StopCoroutine("randomlyGenerateGold");
        foreach (IEnumerator coroutine in this.badCoroutines) {
            StopCoroutine(coroutine);
        }
        GameStateManager.onGameEnd();
        this.snake.enabled = false;
    }

    public void reviveGame() {
        //Debug.Log("revive game");
        this.removeGold();
        foreach (Cube bad in this.bads) {
            this.removeBad(bad);
        }
        this.goldAmount = DataAndSettingsManager.getGoldAmount();
        this.updateGoldLabel();
        this.isReviving = true;
        this.actuallyStartGameAction();
        this.isReviving = false;
    }

    /* * * * Initializing, generating, and destroying objects * * * */

    private void initializeObjects() {
        //Debug.Log("initialize objects");
        // all snake nodes and apples are children of the gameOrigin object so we can refer to their localPositions when moving them
        Transform originTransform = this.gameOrigin.transform;
        Vector3 originPosition = originTransform.position;
        Quaternion originRotation = originTransform.rotation;

        // initialize a bunch of things
        Vector3 boxOffset = (originTransform.right * 0.45f) + (-originTransform.up * 0.05f) + (originTransform.forward * 0.45f);
        this.boundingBox = Instantiate(this.boundingBoxPrefab, originPosition + boxOffset, originRotation);
        this.snake = Instantiate(this.snakePrefab, originPosition, originRotation, originTransform) as Snake;
        this.snake.setGameOrigin(this.gameOrigin);

        // these game objects will be reused repeatedly instead of recreated
        this.apple = Instantiate(this.applePrefab, originPosition + new Vector3(-1, -1, -1), originRotation, originTransform) as Cube;
        this.apple.gameObject.SetActive(false);
        this.gold = Instantiate(this.goldPrefab, originPosition + new Vector3(-1, -1, -1), originRotation, originTransform) as Cube;
        this.gold.gameObject.SetActive(false);
        this.bads = new List<Cube>();
        int numBads = this.isHardMode ? 10 : 5;
        for (int i = 0; i < numBads; i++) {
            Cube bad = Instantiate(this.badPrefab, originPosition + new Vector3(-1, -1, -1), originRotation, originTransform) as Cube;
            this.bads.Add(bad);
            bad.gameObject.SetActive(false);
        }
        this.badCoroutines = new List<IEnumerator>();
    }

    private void generateApple() {
        // each time the snake eats an apple, another one appears
        int[] coordinates = this.findRandomEmptySpace();
        this.setValueAtCoordinates(coordinates, SPACE_APPLE);
        this.apple.goToCoordinates(coordinates);
        this.apple.gameObject.SetActive(true);
    }

    private IEnumerator randomlyGenerateGold() {
        // gold appears and disappears after random time intervals
        while (true) { // will run until coroutine is stopped
            float delayTime = (float) Random.Range(10, 20);
            yield return StartCoroutine(this.pausableWait(delayTime)); // the player may pause during the delay
            this.generateGold();
            yield return StartCoroutine(this.pausableWait(delayTime));
            this.removeGold();
        }
    }

    private void generateGold() {
        //Debug.Log("generating gold");
        int[] coordinates = this.findRandomEmptySpace();
        this.setValueAtCoordinates(coordinates, SPACE_GOLD);
        this.gold.goToCoordinates(coordinates);
        this.gold.gameObject.SetActive(true);
    }

    private void removeGold() {
        //Debug.Log("removing gold");
        this.gold.gameObject.SetActive(false);
        if (this.valueAtCoordinates(this.gold.getCoordinates()) == SPACE_GOLD) {
            this.setValueAtCoordinates(this.gold.getCoordinates(), SPACE_EMPTY);
        }
    }

    private IEnumerator randomlyGenerateBad(Cube bad) {
        // bad appears and disappears after random time intervals
        while (true) { // will run until coroutine is stopped
            float delayTime = (float) Random.Range(10, 30);
            yield return StartCoroutine(this.pausableWait(delayTime)); // the player may pause during the delay
            this.generateBad(bad);
            yield return StartCoroutine(this.pausableWait(delayTime));
            this.removeBad(bad);
        }
    }

    private void generateBad(Cube bad) {
        //Debug.Log("generating bad");
        int[] coordinates = this.findRandomEmptySpace();
        this.setValueAtCoordinates(coordinates, SPACE_BAD);
        bad.goToCoordinates(coordinates);
        bad.gameObject.SetActive(true);
    }

    private void removeBad(Cube bad) {
        //Debug.Log("removing bad");
        bad.gameObject.SetActive(false);
        if (this.valueAtCoordinates(bad.getCoordinates()) == SPACE_BAD) {
            this.setValueAtCoordinates(bad.getCoordinates(), SPACE_EMPTY);
        }
    }

    private void destroyObjects() {
        //Debug.Log("destroy objects");
        if (this.gameOrigin.transform.childCount > 0) {
            //Debug.Log("1");
            Destroy(this.boundingBox);
            //Debug.Log("2");
            Destroy(this.snake); // calls the script's OnDestroy method, so it can destroy its child nodes
            //Debug.Log("3");
            Destroy(this.snake.gameObject);
            //Debug.Log("4");
            Destroy(this.apple.gameObject);
            //Debug.Log("5");
            Destroy(this.gold.gameObject);
            //Debug.Log("6");
            foreach (Cube bad in this.bads) {
                Destroy(bad.gameObject);
                //Debug.Log("7");
            }
            this.bads.Clear();
            this.badCoroutines.Clear();
        }
    }

    /* * * * Player controls * * * */

    ///<summary>Converts the angle of a swipe into a direction for the snake to move, then instructs the snake to change its direction. This method is triggered by Swipes' `swipeEvent`.</summary>
    public void handleSwipe(float angle) {
        int direction = this.directionCube.directionForSwipe(angle);
        if (direction != -1) {
            this.snake.setNextDirection(direction);
        }
    }

    ///<summary>Instructs the snake to change its direction to up. This method is linked to a button on the canvas.</summary>
    public void handleUp() {
        this.snake.setNextDirection(Snake.DIRECTION_POS_Y);
    }

    ///<summary>Instructs the snake to change its direction to down. This method is linked to a button on the canvas.</summary>
    public void handleDown() {
        this.snake.setNextDirection(Snake.DIRECTION_NEG_Y);
    }

    /* * * * Eating and animating feedback * * * */

    ///<summary>Checks for apples at the given coordinates. If there is anything there, it will be eaten and the score will be updated.</summary>
    private void lookForApples(int[] coordinates) {
        // eat any apple/gold/bad that is there
        int value = this.valueAtCoordinates(coordinates);
        this.setValueAtCoordinates(coordinates, SPACE_EMPTY);
        if (value == SPACE_APPLE) {
            this.showPointsFeedback(this.apple.gameObject.transform.position, SPACE_APPLE);
            this.apple.gameObject.SetActive(false);
            this.score += 1;
            this.applesCollected += 1;
            this.generateApple(); // will be regenerated immediately
        }
        else if (value == SPACE_GOLD) {
            this.showPointsFeedback(this.gold.gameObject.transform.position, SPACE_GOLD);
            this.gold.gameObject.SetActive(false); // will be regenerated eventually
            this.score += 3;
            this.goldAmount += 1;
            this.updateGoldLabel();
        }
        else if (value == SPACE_BAD) {
            Cube bad = null;
            foreach (Cube b in this.bads) {
                if (b.getX() == coordinates[0] && b.getY() == coordinates[1] && b.getZ() == coordinates[2]) {
                    bad = b;
                    break;
                }
            }
            if (bad != null) {
                this.showPointsFeedback(bad.gameObject.transform.position, SPACE_BAD);
                bad.gameObject.SetActive(false); // will be regenerated eventually
                this.score -= 2;
                this.applesCollected -= 2;
            }
        }

        if (this.score < 0) {
            this.score = 0;
        }
        if (this.applesCollected < 0) {
            this.applesCollected = 0;
        }
        this.updateScoreLabel();
    }

    ///<summary>Generates an animated +1, +3, etc. at the given position.</summary>
    private void showPointsFeedback(Vector3 position, int type) {
        if (type == SPACE_APPLE) {
            StartCoroutine("animatePointsFeedback", Instantiate(this.plus1Prefab, position, new Quaternion()));
            this.audioManager.playSound(AudioManager.SOUND_APPLE);
        }
        else if (type == SPACE_GOLD) {
            StartCoroutine("animatePointsFeedback", Instantiate(this.plus3Prefab, position, new Quaternion()));
            StartCoroutine("animatePointsFeedback", Instantiate(this.plusGoldPrefab, position + new Vector3(0f, -0.1f, 0f), new Quaternion()));
            this.audioManager.playSound(AudioManager.SOUND_GOLD);
        }
        else if (type == SPACE_BAD) {
            StartCoroutine("animatePointsFeedback", Instantiate(this.minus2Prefab, position, new Quaternion()));
            this.audioManager.playSound(AudioManager.SOUND_BAD);
        }
    }

    private IEnumerator animatePointsFeedback(GameObject obj) {
        Vector3 currentPosition = obj.transform.position;
        Vector3 destination = currentPosition + new Vector3(0f, 0.08f, 0f);
        float t = 0f, timeToMove = 0.4f;
        while (t < 1) {
            t += Time.deltaTime / timeToMove;
            obj.transform.position = Vector3.Lerp(currentPosition, destination, t);
            obj.transform.LookAt(this.mainCamera.transform);
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);
        Destroy(obj);
        yield break;
    }

    /* * * * Helper methods * * * */

    ///<param name="coordinates">An integer array containing, in order, the x, y, and z coordinates in game space to retrieve a value from.</param>
    private int valueAtCoordinates(int[] coordinates) {
        return this.valueAtCoordinates(coordinates[0], coordinates[1], coordinates[2]);
    }
    private int valueAtCoordinates(int x, int y, int z) {
        if (this.inBounds(x, y, z)) {
            return this.space[x,y,z];
        }
        return 0;
    }

    ///<param name="coordinates">An integer array containing, in order, the x, y, and z coordinates in game space to set a value at.</param>
    ///<param name="value">The value to set - should be one of the SPACE constants.</param>
    private void setValueAtCoordinates(int[] coordinates, int value) {
        this.setValueAtCoordinates(coordinates[0], coordinates[1], coordinates[2], value);
    }
    private void setValueAtCoordinates(int x, int y, int z, int value) {
        if (this.inBounds(x, y, z) && value >= SPACE_EMPTY && value <= SPACE_BAD) {
            this.space[x,y,z] = value;
        }
    }

    ///<summary>Indicates whether the given coordinates are within the bounds of the game.</summary>
    ///<param name="coordinates">An integer array containing, in order, the x, y, and z coordinates in game space to examine.</param>
    private bool inBounds(int[] coordinates) {
        return this.inBounds(coordinates[0], coordinates[1], coordinates[2]);
    }
    public bool inBounds(int x, int y, int z) {
        return x >= 0 && x < SIZE && y >= 0 && y < SIZE && z >= 0 && z < SIZE;
    }

    private bool validMove(int[] nextMove) {
        return this.inBounds(nextMove) && this.valueAtCoordinates(nextMove) != SPACE_SNAKE;
    }

    private bool aboutToEatApple(int[] nextMove) {
        return this.valueAtCoordinates(nextMove) == SPACE_APPLE;
    }

    private bool shouldGrow() {
        int length = this.snake.getLength();
        return (length <= 12) || (Random.Range(1, 41) > length);
    }

    private float timeInterval() {
        if (this.isTutorial) {
            return DEFAULT_TIME_INTERVAL + 0.1f;
        }
        else if (this.isReviving) {
            return SLOW_TIME_INTERVAL;
        }
        else if (this.isHardMode) {
            return FAST_TIME_INTERVAL;
        }
        else if (this.score < 10) {
            return DEFAULT_TIME_INTERVAL;
        }
        else if (this.score < 40) {
            return DEFAULT_TIME_INTERVAL - ((this.score - 7) / 3 * 0.01f);
        }
        else {
            return DEFAULT_TIME_INTERVAL - 0.1f;
        }
    }

    ///<summary>A helper coroutine that functions the same as `yield return new WaitForSeconds(waitTime);`, but also respects the game's pause state. Usage: `yield return StartCoroutine(this.pausableWait(waitTime));`.</summary>
    private IEnumerator pausableWait(float waitTime) {
        float elapsedTime = 0f;
        while (elapsedTime < waitTime) {
            while (this.isPaused) { yield return null; }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        yield break;
    }

    private void updateScoreLabel() {
        this.scoreLabel.text = "" + this.score;
    }

    private void updateGoldLabel() {
        this.goldLabel.text = "" + this.goldAmount;
    }

    private int[] findRandomEmptySpace() {
        int x, y, z;
        do {
            x = Random.Range(0, SIZE);
            y = Random.Range(0, SIZE);
            z = Random.Range(0, SIZE);
        } while (this.space[x,y,z] != SPACE_EMPTY);
        return new int[3] { x, y, z };
    }

}
