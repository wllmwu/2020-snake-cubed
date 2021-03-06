﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Experimental.XR;

public class GameStarter : StateChangeListener {

    private ARSessionOrigin arOrigin;
    private ARRaycastManager arRaycaster;
    private Pose placementPose;
    private bool isValidPlacement = false;
    public GameObject potentialBoundingBoxPrefab;
    private GameObject potentialBoundingBox;
    private GameObject gameOrigin;
    public GameObject hardModeRect;
    public Toggle hardModeToggle;
    public RectTransform helpText;

    /* * * * Lifecycle methods * * * */

    void Awake() {
        this.gameOrigin = GameObject.FindWithTag("GameOrigin");
        this.arOrigin = FindObjectOfType<ARSessionOrigin>();
        this.arRaycaster = FindObjectOfType<ARRaycastManager>();
    }

    void Start() {
        FindObjectOfType<AudioManager>().playMusic(AudioManager.MUSIC_BACKGROUND);
    }

    void OnEnable() {
        this.potentialBoundingBox = Instantiate(this.potentialBoundingBoxPrefab);
    }

    void Update() {
        // move the potential bounding box around
        this.updatePotentialPlacement();
    }

    /* * * * StateChangeListener delegate * * * */

    public override void respondToStateChange(GameState newState) {
        if (newState == GameState.SettingPosition) {
            this.enabled = true;
            this.showHardModeToggleIfNecessary();
        }
        else {
            this.enabled = false;
        }
    }

    /* * * * UI setup * * * */

    private void showHardModeToggleIfNecessary() {
        this.hardModeRect.SetActive(DataAndSettingsManager.getNumBoughtForStoreItem(StoreManager.ITEM_KEY_HARD_MODE) > 0);
        this.hardModeToggle.isOn = DataAndSettingsManager.getHardModeState();
        if (this.hardModeRect.activeSelf) { // need to adjust text position to make room for the toggle
            this.helpText.anchoredPosition = new Vector2(0f, 400f);
        }
        else {
            this.helpText.anchoredPosition = new Vector2(0f, 300f);
        }
    }

    /* * * * Setting game position * * * */

    ///<summary>Projects a raycast from the center of the screen and moves the bounding box to where it hits.</summary>
    private void updatePotentialPlacement() {
        Vector3 screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        this.arRaycaster.Raycast(screenCenter, hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes); // looking for flat planes

        this.isValidPlacement = hits.Count > 0;
        if (this.isValidPlacement) {
            this.placementPose = hits[0].pose; // pose tracks position and rotation

            // keep the box facing the same direction relative to the camera
            Vector3 cameraForward = Camera.main.transform.forward; // orthogonal to the screen
            Vector3 cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized; // project onto xz plane (horizontal)
            this.placementPose.rotation = Quaternion.LookRotation(cameraBearing); // box "looks" in that direction

            this.potentialBoundingBox.SetActive(true);
            this.potentialBoundingBox.transform.SetPositionAndRotation(this.placementPose.position, this.placementPose.rotation);
        }
        else {
            this.potentialBoundingBox.SetActive(false);
        }
    }

    /* * * * UI actions * * * */

    public void toggleHardModeAction() {
        DataAndSettingsManager.setHardModeState(this.hardModeToggle.isOn);
    }

    public void onScaleChanged(float value) {
        float newScale = 1 / value;
        this.arOrigin.transform.localScale = new Vector3(newScale, newScale, newScale);
    }

    public void setPositionAction() {
        if (this.isValidPlacement) {
            // calculate game origin position
            Transform boxTransform = this.potentialBoundingBox.transform;
            Vector3 offset = (-boxTransform.right * 0.45f) + (boxTransform.up * 0.05f) + (-boxTransform.forward * 0.45f);
            this.gameOrigin.transform.position = boxTransform.position + offset;
            this.gameOrigin.transform.rotation = boxTransform.rotation;
            Destroy(this.potentialBoundingBox);
            GameStateManager.onPositionSet();
        }
    }

    public void backToMenuAction() {
        GameStateManager.quitGame();
    }

}
