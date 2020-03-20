using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Swipes : MonoBehaviour {

    private Vector2 touchDownPosition;
    private Vector2 touchUpPosition;
    private float minimumDistance = 20;
    private float maximumTime = 0.5f;
    private float elapsedTime = 0;
    public FloatEvent swipeEvent;

    void Update() {
        foreach (Touch touch in Input.touches) {
            if (touch.phase == TouchPhase.Began) {
                this.touchDownPosition = touch.position;
                this.touchUpPosition = touch.position;
                this.elapsedTime = 0;
            }

            if (touch.phase == TouchPhase.Ended) {
                this.touchUpPosition = touch.position;
                this.testForSwipe();
                break;
            }
        }
        this.elapsedTime += Time.deltaTime;
    }

    private void testForSwipe() {
        float dx = this.touchDownPosition.x - this.touchUpPosition.x;
        float dy = this.touchDownPosition.y - this.touchUpPosition.y;
        float distance = Mathf.Sqrt((dx * dx) + (dy * dy));
        if (distance >= this.minimumDistance && this.elapsedTime <= this.maximumTime) {
            float angle = angleBetweenPoints(this.touchDownPosition, this.touchUpPosition);
            this.swipeEvent.Invoke(angle);
            this.elapsedTime = 0;
        }
    }

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
