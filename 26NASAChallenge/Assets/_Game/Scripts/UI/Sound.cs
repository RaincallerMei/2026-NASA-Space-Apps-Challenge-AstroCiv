using System.Collections.Generic;
using Mapmakers.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mapmakers.UI
{
    public enum Sfx
    {
        Click, Back, LineNext, LineBack, WindowOpen, WindowClose, Speed, Pause, Resume, Inspect, PowerOn, PowerOff,
        PickCard, Remove, Invalid, Repair, GoodNews, Objective, Warning, Place, Alert, MissionDone, MissionStopped
    }

    public enum MusicMode { None, Menu, Cutscene, Game }

    // level: the file's typical loudness in LUFS (see AudioDirector). gain brings every song to the menu song's level.
    public sealed class Track
    {
        public string file, title, artist;
        public float gain, level;
        public Track(string file, string title, string artist, float gain, float level) { this.file = file; this.title = title; this.artist = artist; this.gain = gain; this.level = level; }
    }

    // One music source and one effects source that live across scenes.
    // The menu and story screens loop one track each; gameplay and its report shuffle three tracks with a short silence between them.
    // Only the most important effect requested in a frame plays, and repeated effects are spaced out.
    public sealed class AudioDirector : MonoBehaviour
    {
        // Every song plays at the menu song's loudness, about -31.7 LUFS after volume and gain. A song's level is the mean
        // 3-second K-weighted loudness, averaged over the full file and a 150 Hz high-passed copy (small laptop speakers drop
        // bass). Measured with tools/measure_music.py (pyloudnorm) on November 15, 2026.
        // Effect volumes put clicks near the music level and key actions about 9 dB above it (measured RMS of each file).
        // No sharp sounds: every cue the player triggers has its spectral centroid under 4 kHz and under 10% of its energy
        // above 4 kHz (measured on the Kenney files). Every window opens with one sound and closes with one sound.
        public const float MusicVolume = 0.35f;
        public const float GapSeconds = 5f;
        const float FadeSeconds = 0.8f;

        public static readonly Track MenuTrack = new Track("menu_dream_culture", "Dream Culture", "Kevin MacLeod", 0.73f, -19.80f);
        public static readonly Track CutsceneTrack = new Track("cutscene_space_jazz", "Space Jazz", "Kevin MacLeod", 1.05f, -22.93f);
        public static readonly Track[] GameTracks =
        {
            new Track("game_cat_caffe", "Cat caffe", "TAD", 0.90f, -21.66f),
            new Track("game_cipher", "Cipher", "Kevin MacLeod", 0.75f, -20.06f),
            new Track("game_cue", "Cue", "TAD", 0.57f, -17.72f)
        };

        sealed class Cue
        {
            public string file, layer;
            public float volume, layerVolume, spacing;
            public int priority;
        }

        static readonly Dictionary<Sfx, Cue> cues = new Dictionary<Sfx, Cue>
        {
            { Sfx.Click, C("click_002", 0.17f, 0) },
            { Sfx.Back, C("back_002", 0.17f, 0) },
            { Sfx.LineNext, C("click_003", 0.18f, 1) },
            { Sfx.LineBack, C("back_004", 0.18f, 1) },
            { Sfx.WindowOpen, C("maximize_009", 0.13f, 1) },
            { Sfx.WindowClose, C("minimize_009", 0.12f, 1) },
            { Sfx.Speed, C("click_005", 0.16f, 1) },
            { Sfx.Pause, C("pluck_001", 0.42f, 2) },
            { Sfx.Resume, C("pluck_002", 0.49f, 2) },
            { Sfx.Inspect, C("select_004", 0.45f, 2) },
            { Sfx.PowerOn, C("switch_005", 0.57f, 2) },
            { Sfx.PowerOff, C("switch_002", 0.67f, 2) },
            { Sfx.PickCard, C("select_005", 0.54f, 3) },
            { Sfx.Remove, C("minimize_008", 0.27f, 3) },
            { Sfx.Invalid, C("error_008", 0.47f, 3, 0.25f) },
            { Sfx.Repair, C("drop_004", 0.68f, 3) },
            { Sfx.GoodNews, C("glass_001", 0.51f, 3, 1f) },
            { Sfx.Objective, C("confirmation_001", 0.23f, 4, 0.5f) },
            { Sfx.Warning, C("error_006", 0.45f, 4, 2f) },
            { Sfx.Place, C("drop_002", 0.87f, 5, 0.05f, "glass_006", 0.34f) },
            { Sfx.Alert, C("bong_001", 0.58f, 6, 2f) },
            { Sfx.MissionDone, C("confirmation_004", 0.34f, 6) },
            { Sfx.MissionStopped, C("minimize_006", 0.29f, 6) }
        };

        static Cue C(string file, float volume, int priority, float spacing = 0.06f, string layer = null, float layerVolume = 0)
            => new Cue { file = file, volume = volume, priority = priority, spacing = spacing, layer = layer, layerVolume = layerVolume };

        public static IEnumerable<string> FilesOf(Sfx cue)
        {
            yield return cues[cue].file;
            if (cues[cue].layer != null) yield return cues[cue].layer;
        }

        static AudioDirector instance;

        AudioSource music, effects;
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        readonly Dictionary<Sfx, float> lastPlayed = new Dictionary<Sfx, float>();
        readonly List<Track> order = new List<Track>();
        Sfx? pending;
        int index = -1;
        Track switching;
        float gapEnds = -1, startedAt;
        bool held;

        public MusicMode Mode { get; private set; }
        public Track Current { get; private set; }
        public bool UserPaused { get; private set; }
        public float GapLeft => gapEnds < 0 ? 0 : Mathf.Max(0, gapEnds - Time.unscaledTime);
        public bool Waiting => gapEnds >= 0;
        public Sfx? LastEffect { get; private set; }

        public static AudioDirector Instance
        {
            get
            {
                if (instance) return instance;
                if (!Application.isPlaying) return null;
                instance = new GameObject("Audio Director").AddComponent<AudioDirector>();
                DontDestroyOnLoad(instance.gameObject);
                return instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot() => _ = Instance;

        public static void Play(Sfx cue)
        {
            var director = Instance;
            if (director == null || !Progress.SoundsOn) return;
            if (director.pending.HasValue && cues[director.pending.Value].priority >= cues[cue].priority) return;
            director.pending = cue;
        }

        void Awake()
        {
            music = gameObject.AddComponent<AudioSource>();
            music.playOnAwake = false;
            music.volume = 0;
            effects = gameObject.AddComponent<AudioSource>();
            effects.playOnAwake = false;
            Shuffle(null);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) => SetMode(ModeFor(scene.name));

        public static MusicMode ModeFor(string scene)
        {
            if (scene == Flow.GameScene || scene == Flow.ResultsScene) return MusicMode.Game;
            if (scene == Flow.CutsceneScene) return MusicMode.Cutscene;
            return MusicMode.Menu;
        }

        public void SetMode(MusicMode next)
        {
            if (next == Mode) return;
            Mode = next;
            gapEnds = -1;
            if (next == MusicMode.Game) Step(1);
            else SwitchTo(next == MusicMode.Cutscene ? CutsceneTrack : MenuTrack);
        }

        // ---------- the in-game music player ----------

        public void Next()
        {
            if (Mode != MusicMode.Game) return;
            UserPaused = false;
            Step(1);
        }

        public void Previous()
        {
            if (Mode != MusicMode.Game) return;
            UserPaused = false;
            Step(-1);
        }

        public void TogglePause() => UserPaused = !UserPaused;

        void Step(int direction)
        {
            index += direction;
            if (index >= order.Count) { Shuffle(order[order.Count - 1]); index = 0; }
            if (index < 0) index = order.Count - 1;
            SwitchTo(order[index]);
        }

        // A new random order that never starts with the song that just ended.
        void Shuffle(Track last)
        {
            order.Clear();
            order.AddRange(GameTracks);
            for (int i = order.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (order[i], order[j]) = (order[j], order[i]);
            }
            if (last != null && order[0] == last) (order[0], order[order.Count - 1]) = (order[order.Count - 1], order[0]);
        }

        void SwitchTo(Track track)
        {
            gapEnds = -1;
            if (music.isPlaying && music.volume > 0.01f) { switching = track; return; }
            Begin(track);
        }

        void Begin(Track track)
        {
            switching = null;
            Current = track;
            music.Stop();
            music.clip = Clip("Music/" + track.file);
            music.loop = Mode != MusicMode.Game;
            music.volume = 0;
            startedAt = Time.unscaledTime;
            if (music.clip != null) music.Play();
        }

        bool Silent => !Progress.MusicOn || (Mode == MusicMode.Game && UserPaused);

        void Update()
        {
            PlayPending();
            if (Silent)
            {
                if (music.isPlaying) { music.Pause(); held = true; }
                return;
            }
            if (held) { held = false; music.UnPause(); }
            float goal = switching != null || Current == null ? 0 : MusicVolume * Current.gain;
            music.volume = Mathf.MoveTowards(music.volume, goal, Time.unscaledDeltaTime * MusicVolume / FadeSeconds);
            if (switching != null)
            {
                if (music.volume <= 0.001f) Begin(switching);
                return;
            }
            if (Mode != MusicMode.Game || music.clip == null) return;
            if (music.isPlaying || music.clip.loadState == AudioDataLoadState.Loading || Time.unscaledTime - startedAt < 1.5f || !Application.isFocused) return;
            if (gapEnds < 0) gapEnds = Time.unscaledTime + GapSeconds;
            else if (Time.unscaledTime >= gapEnds) Step(1);
        }

        void PlayPending()
        {
            if (!pending.HasValue) return;
            var id = pending.Value;
            pending = null;
            var cue = cues[id];
            float now = Time.unscaledTime;
            if (lastPlayed.TryGetValue(id, out float last) && now - last < cue.spacing) return;
            lastPlayed[id] = now;
            LastEffect = id;
            var clip = Clip("Sfx/" + cue.file);
            if (clip != null) effects.PlayOneShot(clip, cue.volume);
            if (cue.layer == null) return;
            var layer = Clip("Sfx/" + cue.layer);
            if (layer != null) effects.PlayOneShot(layer, cue.layerVolume);
        }

        AudioClip Clip(string path)
        {
            if (clips.TryGetValue(path, out var clip) && clip) return clip;
            return clips[path] = Resources.Load<AudioClip>("Audio/" + path);
        }
    }

    // A small music player for gameplay: previous, play/pause, next, and the song's name and creator.
    public sealed class MusicPlayer : MonoBehaviour
    {
        public const float Height = 64;

        Text title;
        Image playIcon;
        Button playButton;
        float nextRefresh;

        public static MusicPlayer Build(Transform parent, Vector2 anchoredTopRight, float width)
        {
            var panel = UIKit.Panel(parent, Palette.Alpha(Palette.Panel, 0.95f), "Music Player");
            panel.rectTransform.Anchor(new Vector2(1, 1), new Vector2(1, 1), anchoredTopRight, new Vector2(width, Height));
            var player = panel.gameObject.AddComponent<MusicPlayer>();
            var note = UIKit.Icon(panel.transform, "music", 30);
            note.color = Palette.Cyan;
            note.TopLeftOf(12, 17);
            player.title = UIKit.Label(panel.transform, "", 20, Palette.Hull, TextAnchor.MiddleLeft, FontStyle.Bold);
            player.title.rectTransform.TopLeft(50, 0, width - 50 - 3 * 52 - 12, Height);
            player.title.resizeTextForBestFit = true;
            player.title.resizeTextMinSize = 14;
            player.title.resizeTextMaxSize = 20;
            player.title.verticalOverflow = VerticalWrapMode.Truncate;

            float x = width - 3 * 52 - 8;
            var previous = Control(panel.transform, x, "skip", () => AudioDirector.Instance?.Previous());
            previous.transform.GetChild(0).localScale = new Vector3(-1, 1, 1);
            player.playButton = Control(panel.transform, x + 52, "pause", () => AudioDirector.Instance?.TogglePause());
            player.playIcon = player.playButton.transform.GetChild(0).GetComponent<Image>();
            Control(panel.transform, x + 104, "skip", () => AudioDirector.Instance?.Next());
            Tip.Add(previous, () => new TipContent("Previous song", "music", "Go back to the song before."), false);
            Tip.Add(player.playButton, () => new TipContent("Music", "music", "Pause or play the music. The game keeps going."), false);
            Tip.Add(panel, () => new TipContent("Music player", "music", "Three songs play in a random order. Credits are in Settings → Data & Credits."), false);
            return player;
        }

        static Button Control(Transform parent, float x, string icon, UnityEngine.Events.UnityAction action)
        {
            var button = UIKit.Button(parent, "", action, Palette.PanelLight, Palette.Hull, 20);
            button.GetComponent<RectTransform>().TopLeft(x, 8, 48, 48);
            var image = UIKit.Icon(button.transform, icon, 28);
            image.rectTransform.anchorMin = image.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            image.rectTransform.anchoredPosition = Vector2.zero;
            image.transform.SetAsFirstSibling();
            return button;
        }

        void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.25f;
            var director = AudioDirector.Instance;
            if (director == null) return;
            bool off = !Progress.MusicOn;
            bool paused = director.UserPaused || off;
            playIcon.sprite = Icons.Get(paused ? "play" : "pause");
            playButton.image.color = paused ? Palette.Amber : Palette.PanelLight;
            var track = director.Current;
            if (off) title.text = "Music is off (Settings)";
            else if (director.Waiting) title.text = "Next song in " + Mathf.CeilToInt(director.GapLeft) + " s";
            else title.text = track == null ? "" : track.title + "  <color=#8A9BB5>· " + track.artist + "</color>";
        }
    }
}
