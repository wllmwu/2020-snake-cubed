using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour {

    public static readonly int DIRECTION_POS_X = 0;
    public static readonly int DIRECTION_POS_Y = 1;
    public static readonly int DIRECTION_POS_Z = 2;
    public static readonly int DIRECTION_NEG_X = 3;
    public static readonly int DIRECTION_NEG_Y = 4;
    public static readonly int DIRECTION_NEG_Z = 5;
    public static readonly int[][] DIRECTION_COORDINATE_OFFSETS = {
        new int[3] { 1, 0, 0 }, // +x
        new int[3] { 0, 1, 0 }, // +y
        new int[3] { 0, 0, 1 }, // +z
        new int[3] { -1, 0, 0 }, // -x
        new int[3] { 0, -1, 0 }, // -y
        new int[3] { 0, 0, -1 } // -z
    };

    public List<SnakeNode> nodes;
    public SnakeNode snakeNodePrefab;

    private SnakeNode head;
    private SnakeNode tail;
    private int nextDirection;
    private GameObject gameOrigin;
    private SnakeNode ghostNode;

    /* * * * Public getters and setters * * * */

    public int getNextDirection() { return this.nextDirection; }
    public void setNextDirection(int direction) { this.nextDirection = direction; }
    public void setGameOrigin(GameObject origin) { this.gameOrigin = origin; }
    public void setPaused(bool paused) {
        this.ghostNode.setPaused(paused);
        foreach (SnakeNode node in this.nodes) {
            node.setPaused(paused);
        }
    }

    /* * * * Lifecycle methods * * * */

    void Awake() {
        // set up nodes' data
        this.head = nodes[0];
        this.head.setDirection(DIRECTION_POS_X);
        this.head.setCoordinates(4, 2, 2);
        nodes[1].setNodeBefore(nodes[0]);
        nodes[1].setDirection(DIRECTION_POS_X);
        nodes[1].setCoordinates(3, 2, 2);
        this.tail = nodes[2];
        this.tail.setNodeBefore(nodes[1]);
        this.tail.setDirection(DIRECTION_POS_X);
        this.tail.setCoordinates(2, 2, 2);
    }

    void Start() {
        // ghostNode serves only in animating the snake's movement - not actually part of the snake
        this.ghostNode = Instantiate(this.snakeNodePrefab, this.tail.transform.position, this.tail.transform.rotation, this.gameOrigin.transform) as SnakeNode;
    }

    void OnDestroy() {
        // destroy nodes
        Destroy(this.ghostNode.gameObject);
        foreach (SnakeNode node in this.nodes) {
            Destroy(node.gameObject);
        }
    }

    /* * * * Public methods * * * */

    public int[] nextMove() {
        this.attemptDirectionChange(this.nextDirection); // change intended direction if necessary
        this.nextDirection = -1;
        return this.head.coordinatesOfNextNode();
    }

    private void attemptDirectionChange(int newDirection) {
        if (newDirection >= 0 && newDirection != this.head.getDirection() - 3 && newDirection != this.head.getDirection() + 3) {
            this.head.setDirection(newDirection);
        }
    }

    public int[] move(float timeToMove, bool shouldMoveSmoothly) {
        int[] old = this.tail.getCoordinates();
        SnakeNode newTail = this.tail.getNodeBefore();
        SnakeNode newHead = this.tail; // the tail moves up to become the head
        this.ghostNode.goToCoordinates(this.tail.getCoordinates());
        newHead.goToCoordinates(this.head.getCoordinates());

        // slide ghostNode into the tail and newHead out of the current head to animate the snake's movement
        this.ghostNode.StartCoroutine(this.ghostNode.moveToCoordinates(newTail.getCoordinates(), timeToMove, shouldMoveSmoothly));
        newHead.StartCoroutine(newHead.moveToCoordinates(this.head.coordinatesOfNextNode(), timeToMove, shouldMoveSmoothly));
        newHead.setDirection(this.head.getDirection());

        // point to the new head and tail
        this.head.setNodeBefore(newHead);
        newHead.setNodeBefore(null);
        this.tail = newTail;
        this.head = newHead;
        return old;
    }

    public void grow(float timeToMove, bool shouldMoveSmoothly) {
        // like the move function, but instantiates a new node as the head instead of moving the tail up
        SnakeNode newHead = Instantiate(this.snakeNodePrefab, this.head.transform.position, this.head.transform.rotation, this.gameOrigin.transform) as SnakeNode;
        newHead.setDirection(this.head.getDirection());
        newHead.setCoordinates(this.head.getCoordinates());
        this.nodes.Add(newHead);

        newHead.StartCoroutine(newHead.moveToCoordinates(this.head.coordinatesOfNextNode(), timeToMove, shouldMoveSmoothly));

        this.head.setNodeBefore(newHead);
        this.head = newHead;
    }

    public int getLength() {
        return this.nodes.Count;
    }

    public int[] getHeadCoordinates() {
        return this.head.getCoordinates();
    }

}
