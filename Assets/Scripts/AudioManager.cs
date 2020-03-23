using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {

    public static readonly string MUSIC_MENU = "menu";
    public static readonly string MUSIC_BACKGROUND = "background";
    public static readonly string SOUND_BUTTON = "button";
    public static readonly string SOUND_APPLE = "apple";
    public static readonly string SOUND_GOLD = "gold";
    public static readonly string SOUND_BAD = "bad";

    public Sound[] sounds;
    public bool isOnMenu;

    private static bool musicEnabled;
    private static bool soundsEnabled;
    private static AudioManager instance;

    void Awake() {
        foreach (Sound s in sounds) {
            s.source = this.gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
        }
        musicEnabled = DataAndSettingsManager.getMusicEnabledState();
        soundsEnabled = DataAndSettingsManager.getSoundsEnabledState();
        instance = this;
    }

    /* * * * Public methods * * * */

    public static void setMusicEnabled(bool isEnabled) {
        musicEnabled = isEnabled;
        if (isEnabled) {
            // start music
            if (instance.isOnMenu) {
                instance.playAudio(MUSIC_MENU);
            }
            else {
                instance.playAudio(MUSIC_BACKGROUND);
            }
        }
        else {
            // stop music
            if (instance.isOnMenu) {
                Array.Find(instance.sounds, sound => sound.name == MUSIC_MENU).source.Stop();
            }
            else {
                Array.Find(instance.sounds, sound => sound.name == MUSIC_BACKGROUND).source.Stop();
            }
        }
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
