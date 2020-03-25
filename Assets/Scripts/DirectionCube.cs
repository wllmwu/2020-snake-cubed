using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionCube : MonoBehaviour {

    public Camera mainCamera;
    public GameObject gameOrigin;
    public GameObject cameraHolder;
    public Camera smallCamera;
    public GameObject xAxisArrows;
    public GameObject zAxisArrows;
    public Transform center;
    public Transform posXposZ;
    public Transform negXposZ;
    public Transform negXnegZ;
    public Transform posXnegZ;

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        this.moveCamera();
        this.rotateArrows();
    }

    private void moveCamera() {
        this.cameraHolder.transform.rotation = Quaternion.Inverse(this.gameOrigin.transform.rotation) * this.mainCamera.transform.rotation;

        // prevent camera from going close to 0 degrees or 180 degrees
        Vector3 rotation = this.cameraHolder.transform.eulerAngles;
        if (rotation.x < 30) {
            rotation.x = 30;
        }
        else if (rotation.x > 150 && rotation.x <= 180) {
            rotation.x = 150;
        }
        else if (rotation.x > 180 && rotation.x < 210) {
            rotation.x = 210;
        }
        else if (rotation.x > 330) {
            rotation.x = 330;
        }
        this.cameraHolder.transform.eulerAngles = rotation;
    }

    private void rotateArrows() {
        Vector3 cameraPosition = this.smallCamera.transform.position;
        this.xAxisArrows.transform.LookAt(new Vector3(0, cameraPosition.y, cameraPosition.z)); // rotates only on x axis
        this.zAxisArrows.transform.LookAt(new Vector3(cameraPosition.x, cameraPosition.y, 0)); // rotates only on z axis
    }

    public int directionForSwipe(float swipeAngle) {
        Vector2 centerScreenPoint = this.smallCamera.WorldToScreenPoint(this.center.position); // (implicit conversion from Vector3)
        Vector2 posXposZScreenPoint = this.smallCamera.WorldToScreenPoint(this.posXposZ.position);
        Vector2 negXposZScreenPoint = this.smallCamera.WorldToScreenPoint(this.negXposZ.position);
        Vector2 negXnegZScreenPoint = this.smallCamera.WorldToScreenPoint(this.negXnegZ.position);
        Vector2 posXnegZScreenPoint = this.smallCamera.WorldToScreenPoint(this.posXnegZ.position);
        float posXposZAngle = Swipes.angleBetweenPoints(centerScreenPoint, posXposZScreenPoint);
        float negXposZAngle = Swipes.angleBetweenPoints(centerScreenPoint, negXposZScreenPoint);
        float negXnegZAngle = Swipes.angleBetweenPoints(centerScreenPoint, negXnegZScreenPoint);
        float posXnegZAngle = Swipes.angleBetweenPoints(centerScreenPoint, posXnegZScreenPoint);
        //Debug.Log("+x+z: " + posXposZAngle + ", -x+z: " + negXposZAngle + ", -x-z: " + negXnegZAngle + ", +x-z: " + posXnegZAngle);
        if (this.angleIsInRange(swipeAngle, posXposZAngle, negXposZAngle)) {
            return Snake.DIRECTION_POS_Z;
        }
        else if (this.angleIsInRange(swipeAngle, negXposZAngle, negXnegZAngle)) {
            return Snake.DIRECTION_NEG_X;
        }
        else if (this.angleIsInRange(swipeAngle, negXnegZAngle, posXnegZAngle)) {
            return Snake.DIRECTION_NEG_Z;
        }
        else if (this.angleIsInRange(swipeAngle, posXnegZAngle, posXposZAngle)) {
            return Snake.DIRECTION_POS_X;
        }
        return -1;
    }

    private bool angleIsInRange(float angle, float low, float high) {
        if (low > high) {
            return angle >= low || angle <= high;
        }
        return low <= angle && angle <= high;
    }

}
