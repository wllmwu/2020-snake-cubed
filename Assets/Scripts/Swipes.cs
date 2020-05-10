using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Swipes : MonoBehaviour {

    private Vector2 touchDownPosition;
    private Vector2 touchUpPosition;
    private static readonly float MINIMUM_DISTANCE = 20;
    private static readonly float MAXIMUM_TIME = 0.5f;
    private float elapsedTime = 0;
    ///<summary>A Unity event that takes a float (the angle of the swipe).</summary>
    public FloatEvent swipeEvent;

    void Update() {
        // test for swipes
        foreach (Touch touch in Input.touches) {
            if (touch.phase == TouchPhase.Began) {
                // set starting position
                this.touchDownPosition = touch.position;
                this.touchUpPosition = touch.position;
                this.elapsedTime = 0;
            }

            if (touch.phase == TouchPhase.Ended) {
                // set ending position and test whether the touch counts as a swipe
                this.touchUpPosition = touch.position;
                this.testForSwipe();
                break;
            }
        }
        this.elapsedTime += Time.deltaTime;
    }

    ///<summary>Checks whether a swipe has been detected, and invokes the event if so.
    /// The swipe must be longer than `MINIMUM_DISTANCE` and take less than `MAXIMUM_TIME`.</summary>
    private void testForSwipe() {
        float dx = this.touchDownPosition.x - this.touchUpPosition.x;
        float dy = this.touchDownPosition.y - this.touchUpPosition.y;
        float distance = Mathf.Sqrt((dx * dx) + (dy * dy));
        if (distance >= MINIMUM_DISTANCE && this.elapsedTime <= MAXIMUM_TIME) {
            float angle = angleBetweenPoints(this.touchDownPosition, this.touchUpPosition);
            this.swipeEvent.Invoke(angle);
            this.elapsedTime = 0;
        }
    }

    ///<summary>Returns the inverse tangent of the y difference over the x difference of the given `Vector2`s,
    /// in degrees. The angle will be between 0 and 360 degrees.</summary>
    public static float angleBetweenPoints(Vector2 a, Vector2 b) {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        float angle = Mathf.Atan(dy / dx) * Mathf.Rad2Deg;
        // fix angle so it is in range [0, 360)
        if (dx < 0) {
            angle += 180;
        }
        else if (dy < 0) {
            angle += 360;
        }
        return angle;
    }

}

[System.Serializable]
public class FloatEvent : UnityEvent<float> {}
