using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeNode : Cube {

    private int direction;
    private SnakeNode nodeBefore;
    private bool isPaused;

    public void setDirection(int dir) { this.direction = dir; }
    public int getDirection() { return this.direction; }
    public void setNodeBefore(SnakeNode node) { this.nodeBefore = node; }
    public SnakeNode getNodeBefore() { return this.nodeBefore; }
    public void setPaused(bool paused) { this.isPaused = paused; }

    public int[] coordinatesOfNextNode() {
        int[] offset = Snake.DIRECTION_COORDINATE_OFFSETS[this.direction];
        return new int[3] { this.getX() + offset[0], this.getY() + offset[1], this.getZ() + offset[2] };
    }

    public IEnumerator moveToCoordinates(int[] coordinates, float timeToMove, bool shouldMoveSmoothly) {
        int newX = coordinates[0];
        int newY = coordinates[1];
        int newZ = coordinates[2];
        this.setCoordinates(newX, newY, newZ);
        Vector3 currentPosition = this.transform.localPosition;
        Vector3 destination = new Vector3(newX * 0.1f, newY * 0.1f, newZ * 0.1f);
        if (!shouldMoveSmoothly) {
            this.transform.localPosition = destination;
        }
        float t = 0f;
        while (t < 1) { // whether or not it should move smoothly, still need to count down
            while (this.isPaused) { yield return null; }
            t += Time.deltaTime / timeToMove;
            if (shouldMoveSmoothly) {
                this.transform.localPosition = Vector3.Lerp(currentPosition, destination, t);
            }
            yield return null;
        }
        yield break;
    }

}
