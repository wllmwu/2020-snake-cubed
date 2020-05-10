using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {

    // keys for the Sound objects in the sounds array - should match what is set in the editor
    public static readonly string MUSIC_MENU = "menu";
    public static readonly string MUSIC_BACKGROUND = "background";
    public static readonly string SOUND_BUTTON = "button";
    public static readonly string SOUND_APPLE = "apple";
    public static readonly string SOUND_GOLD = "gold";
    public static readonly string SOUND_BAD = "bad";

    public Sound[] sounds; // should be set in the editor
    public bool isOnMenu; // determines what music to play/pause

    private bool musicEnabled;
    private bool soundsEnabled;

    void Awake() {
        foreach (Sound s in sounds) {
            s.source = this.gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
        }
        this.musicEnabled = DataAndSettingsManager.getMusicEnabledState();
        this.soundsEnabled = DataAndSettingsManager.getSoundsEnabledState();
    }

    /* * * * Public methods * * * */

    public void setMusicEnabled(bool isEnabled) {
        this.musicEnabled = isEnabled;
        if (isEnabled) {
            // start music
            if (this.isOnMenu) {
                this.playAudio(MUSIC_MENU);
            }
            else {
                this.playAudio(MUSIC_BACKGROUND);
            }
        }
        else {
            // stop music
            if (this.isOnMenu) {
                this.findSound(MUSIC_MENU).source.Stop();
            }
            else {
                this.findSound(MUSIC_BACKGROUND).source.Stop();
            }
        }
    }

    public void setSoundsEnabled(bool isEnabled) {
        this.soundsEnabled = isEnabled;
    }

    ///<summary>`name` should be a `MUSIC_*` constant.</summary>
    public void playMusic(string name) {
        if (musicEnabled) {
            playAudio(name);
        }
    }

    ///<summary>`name` should be a `SOUND_*` constant.</summary>
    public void playSound(string name) {
        if (soundsEnabled) {
            playAudio(name);
        }
    }

    ///<summary>Convenience method - attach to buttons in the editor.</summary>
    public void playButtonSound() {
        this.playSound(SOUND_BUTTON);
    }

    public void pauseMusic(string name) {
        this.findSound(name).source.Pause();
    }

    /* * * * Private methods * * * */

    ///<summary>Plays the given sound if it exists.</summary>
    private void playAudio(string name) {
        Sound s = this.findSound(name);
        if (s == null) {
            Debug.LogWarning("Sound \"" + name + "\" doesn't exist");
            return;
        }
        s.source.Play();
    }

    private Sound findSound(string name) {
        return Array.Find(this.sounds, sound => sound.name == name);
    }

}
