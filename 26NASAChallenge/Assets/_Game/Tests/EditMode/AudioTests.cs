using System;
using Mapmakers.Game;
using Mapmakers.UI;
using NUnit.Framework;
using UnityEngine;

namespace Mapmakers.Tests
{
    public class AudioTests
    {
        static Track[] AllTracks()
        {
            var all = new Track[AudioDirector.GameTracks.Length + 2];
            AudioDirector.GameTracks.CopyTo(all, 0);
            all[all.Length - 2] = AudioDirector.MenuTrack;
            all[all.Length - 1] = AudioDirector.CutsceneTrack;
            return all;
        }

        [Test]
        public void EverySongAndSoundLoads()
        {
            foreach (var track in AllTracks())
                Assert.IsNotNull(Resources.Load<AudioClip>("Audio/Music/" + track.file), track.file);
            foreach (Sfx cue in Enum.GetValues(typeof(Sfx)))
                foreach (var file in AudioDirector.FilesOf(cue))
                    Assert.IsNotNull(Resources.Load<AudioClip>("Audio/Sfx/" + file), cue + ": " + file);
        }

        [Test]
        public void EachSceneHasItsMusic()
        {
            Assert.AreEqual(MusicMode.Menu, AudioDirector.ModeFor("Boot"));
            Assert.AreEqual(MusicMode.Menu, AudioDirector.ModeFor(Flow.MenuScene));
            Assert.AreEqual(MusicMode.Cutscene, AudioDirector.ModeFor(Flow.CutsceneScene));
            Assert.AreEqual(MusicMode.Game, AudioDirector.ModeFor(Flow.GameScene));
            Assert.AreEqual(MusicMode.Game, AudioDirector.ModeFor(Flow.ResultsScene));
            Assert.AreEqual(3, AudioDirector.GameTracks.Length);
        }

        [Test]
        public void CreditsNameEverySongCreatorAndLicense()
        {
            foreach (var track in AllTracks())
            {
                StringAssert.Contains("\"" + track.title + "\"", MenuScreen.AudioCreditsText);
                StringAssert.Contains(track.artist, MenuScreen.AudioCreditsText);
            }
            StringAssert.Contains("creativecommons.org/licenses/by/4.0", MenuScreen.AudioCreditsText);
            StringAssert.Contains("Kenney", MenuScreen.AudioCreditsText);
        }
    }
}
