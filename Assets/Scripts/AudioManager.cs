using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {

    public static readonly string SOUND_MENU_MUSIC = "menu";
    public static readonly string SOUND_BACKGROUND_MUSIC = "background";
    public static readonly string SOUND_BUTTON = "button";
    public static readonly string SOUND_APPLE = "apple";
    public static readonly string SOUND_GOLD = "gold";
    public static readonly string SOUND_BAD = "bad";

    public Sound[] sounds;

    private static bool musicEnabled;
    private static bool soundsEnabled;

    void Awake() {
        foreach (Sound s in sounds) {
            s.source = this.gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
        }
        musicEnabled = DataAndSettingsManager.getMusicEnabledState();
        soundsEnabled = DataAndSettingsManager.getSoundsEnabledState();
    }

    /* * * * Public methods * * * */

    public static void setMusicEnabled(bool isEnabled) {
        musicEnabled = isEnabled;
        // TODO: start or stop music depending on setting.
        // Array.Find the Sound with the name, do sound.source.Stop();
        // or start it
        // probably need to have a flag for which scene we are currently in so we know which music to start
    }

    public static void setSoundsEnabled(bool isEnabled) {
        soundsEnabled = isEnabled;
    }

    public void playMusic(string name) {
        if (musicEnabled) {
            playAudio(name);
        }
    }

    public void playSound(string name) {
        if (soundsEnabled) {
            playAudio(name);
        }
    }

    public void playButtonSound() {
        this.playSound(SOUND_BUTTON);
    }

    /* * * * Private methods * * * */

    private void playAudio(string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) {
            Debug.LogWarning("Sound \"" + name + "\" doesn't exist");
            return;
        }
        s.source.Play();
    }

}
