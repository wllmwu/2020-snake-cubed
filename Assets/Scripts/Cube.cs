using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour {

    private int x;
    private int y;
    private int z;

    ///<summary>Returns the `Cube`'s x coordinate in game space.</summary>
    public int getX() { return this.x; }
    ///<summary>Returns the `Cube`'s y coordinate in game space.</summary>
    public int getY() { return this.y; }
    ///<summary>Returns the `Cube`'s z coordinate in game space.</summary>
    public int getZ() { return this.z; }

    ///<summary>Sets the `Cube`'s coordinates in game space.</summary>
    public void setCoordinates(int _x, int _y, int _z) {
        this.x = _x;
        this.y = _y;
        this.z = _z;
    }
    ///<summary>Sets the `Cube`'s coordinates in game space.
    /// `coordinates` should contain the x, y, and z coordinates in order.</summary>
    public void setCoordinates(int[] coordinates) {
        this.x = coordinates[0];
        this.y = coordinates[1];
        this.z = coordinates[2];
    }
    ///<summary>Returns an integer array containing, in order, the `Cube`'s x, y, and z coordinates in game space.</summary>
    public int[] getCoordinates() {
        return new int[3] { this.x, this.y, this.z };
    }

    ///<summary>Sets the `Cube`'s local position to the given `Vector3`.</summary>
    private void goToPosition(Vector3 destination) {
        this.transform.localPosition = destination;
    }
    ///<summary>Sets the `Cube`'s coordinates in game space to those given and sets its position accordingly.</summary>
    public void goToCoordinates(int x, int y, int z) {
        this.setCoordinates(x, y, z);
        this.goToPosition(new Vector3(x * 0.1f, y * 0.1f, z * 0.1f));
    }
    ///<summary>Sets the `Cube`'s coordinates in game space to those given and sets its position accordingly.
    /// `coordinates` should contain the x, y, and z coordinates in order.</summary>
    public void goToCoordinates(int[] coordinates) {
        this.goToCoordinates(coordinates[0], coordinates[1], coordinates[2]);
    }

    /* * * * Colorblind indicators * * * */

    ///<summary>Icons that mark what type of cube this is.</summary>
    public GameObject colorblindIndicators; // should be set in the editor

    void OnEnable() {
        DataAndSettingsManager.OnToggleColorblindMode += this.setColorblindMode;
        this.setColorblindMode(DataAndSettingsManager.getColorblindModeState());
    }

    void OnDisable() {
        DataAndSettingsManager.OnToggleColorblindMode -= this.setColorblindMode;
    }

    ///<summary>The delegate method for `SettingsManager.OnToggleColorblindMode`.</summary>
    public void setColorblindMode(bool isOn) {
        if (this.colorblindIndicators != null) {
            this.colorblindIndicators.SetActive(isOn);
        }
    }

}
