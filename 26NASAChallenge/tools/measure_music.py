#!/usr/bin/env python3
"""Measure each music file's typical loudness and the gain that matches it to the menu song.

Typical loudness = mean of 3-second K-weighted loudness windows (1 s hop, ITU-R BS.1770 via pyloudnorm),
averaged over the full file and a copy high-passed at 150 Hz (small laptop speakers drop bass).
Copy the printed level and gain into AudioDirector's Track list in Assets/_Game/Scripts/UI/Sound.cs.

Usage:  pip install soundfile pyloudnorm numpy scipy
        python tools/measure_music.py
"""
import os
import numpy as np
import soundfile as sf
import pyloudnorm as pyln
from scipy.signal import butter, sosfilt

ROOT = os.path.join(os.path.dirname(__file__), "..", "Assets", "_Game", "Audio", "Resources", "Audio", "Music")
REFERENCE = "menu_dream_culture"
REFERENCE_GAIN = 0.73
MUSIC_VOLUME = 0.35


def typical(data, rate):
    meter = pyln.Meter(rate)
    n = 3 * rate
    values = [meter.integrated_loudness(data[s:s + n]) for s in range(0, len(data) - n, rate)]
    return float(np.mean([v for v in values if np.isfinite(v) and v > -60]))


def level(path):
    data, rate = sf.read(path)
    if data.ndim == 1:
        data = data[:, None]
    bass_cut = sosfilt(butter(2, 150, "highpass", fs=rate, output="sos"), data, axis=0)
    return (typical(data, rate) + typical(bass_cut, rate)) / 2


levels = {f[:-4]: level(os.path.join(ROOT, f)) for f in sorted(os.listdir(ROOT)) if f.endswith(".mp3")}
target = levels[REFERENCE] + 20 * np.log10(MUSIC_VOLUME * REFERENCE_GAIN)
print(f"target output {target:.2f} LUFS")
for name, lv in levels.items():
    gain = 10 ** ((target - lv) / 20) / MUSIC_VOLUME
    print(f"{name:22} level {lv:7.2f}  gain {gain:.2f}")
