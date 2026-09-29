"""Deterministic original Area 1 prototype loops; requires NumPy."""
from pathlib import Path
import wave
import numpy as np

RATE = 22050
OUTPUT = Path(__file__).resolve().parents[1] / 'Assets/Audio/BGM/Area1'
OUTPUT.mkdir(parents=True, exist_ok=True)
RNG = np.random.default_rng(20260929)


def pitch(midi):
    return 440.0 * 2 ** ((midi - 69) / 12)


def render(name, bpm, bars, chords, melody, style):
    beat = round(RATE * 60 / bpm)
    length = bars * 4 * beat
    audio = np.zeros(length, dtype=np.float64)

    def add(start, data, gain):
        first = int(start) % length
        data = data * gain
        while len(data):
            count = min(len(data), length - first)
            audio[first:first + count] += data[:count]
            data = data[count:]
            first = 0

    def tone(note, duration, kind):
        n = int(RATE * duration)
        t = np.arange(n) / RATE
        f = pitch(note)
        if kind == 'pluck':
            body = (np.sin(2*np.pi*f*t) + .31*np.sin(2*np.pi*2*f*t)
                    + .12*np.sin(2*np.pi*3*f*t))
            env = (1 - np.exp(-t*850)) * np.exp(-t*5.2)
        elif kind == 'bell':
            body = (np.sin(2*np.pi*f*t) + .22*np.sin(2*np.pi*2.01*f*t)
                    + .10*np.sin(2*np.pi*3.95*f*t))
            env = (1 - np.exp(-t*600)) * np.exp(-t*3.0)
        elif kind == 'bass':
            body = np.sin(2*np.pi*f*t) + .23*np.sin(2*np.pi*2*f*t)
            env = (1 - np.exp(-t*280)) * np.exp(-t*4.0)
        else:
            body = np.sin(2*np.pi*f*t) + .18*np.sin(2*np.pi*2*f*t)
            env = (1 - np.exp(-t*24)) * np.exp(-t*1.1)
        return body * env

    def drum(kind):
        duration = {'kick': .25, 'snare': .17, 'hat': .065}[kind]
        t = np.arange(int(RATE*duration)) / RATE
        if kind == 'kick':
            phase = 2*np.pi*(62*t + 37*(1-np.exp(-t*35))/35)
            return np.sin(phase) * np.exp(-t*20) * (1-np.exp(-t*500))
        noise = RNG.standard_normal(len(t))
        high = noise - np.convolve(noise, np.ones(25)/25, mode='same')
        return high * np.exp(-t*(35 if kind == 'hat' else 18)) * (1-np.exp(-t*700))

    for bar in range(bars):
        root, third, fifth = chords[(bar // 4) % len(chords)]
        pos = bar * 4 * beat
        b = beat / RATE
        # Soft sustained harmony leaves space for warning and tool effects.
        for note in (root+12, third+12, fifth+12):
            add(pos, tone(note, 1.8*b, 'pad'), .018 if style == 'normal' else .012)
            add(pos+2*beat, tone(note, 1.8*b, 'pad'), .014 if style == 'normal' else .010)
        for step in (0, 2) if style == 'normal' else (0, 1.5, 2, 3.5):
            add(pos+int(step*beat), tone(root-12, .55*b, 'bass'),
                .13 if style == 'normal' else .17)
        phrase = melody[(bar // 4) % len(melody)]
        for i, note in enumerate(phrase):
            if note is not None:
                add(pos+int(i*.5*beat), tone(note, .48*b, 'bell' if style == 'normal' else 'pluck'),
                    .085 if style == 'normal' else (.077 if style == 'mini' else .075))
        # A sparse offbeat arpeggio gives the long normal loop motion.
        if style == 'normal':
            for step, note in ((1.5, third+24), (3.5, fifth+24)):
                add(pos+int(step*beat), tone(note, .32*b, 'pluck'), .038)
        else:
            for step in range(8):
                if step % 2 or style == 'boss':
                    add(pos+int(step*.5*beat), tone((root, fifth, third, fifth)[step % 4]+12,
                                                  .22*b, 'pluck'), .033 if style == 'mini' else .039)
        kicks = (0, 2) if style == 'normal' else ((0, 2, 3) if style == 'mini' else (0, 1.5, 2, 3.5))
        for step in kicks:
            add(pos+int(step*beat), drum('kick'), .14 if style == 'normal' else .18)
        for step in (1, 3):
            add(pos+step*beat, drum('snare'), .025 if style == 'normal' else .043)
        for step in range(8):
            if style != 'normal' or step % 2 == 1:
                add(pos+int(step*.5*beat), drum('hat'), .015 if style == 'normal' else .019)

    # Keep the loop's first attack and wrapped decay continuous at the seam.
    audio = np.tanh(audio * 1.3)
    peak = np.max(np.abs(audio))
    audio = audio / max(peak, 1e-9) * .66
    pcm = np.round(audio * 32767).astype('<i2')
    with wave.open(str(OUTPUT / name), 'wb') as output:
        output.setnchannels(1)
        output.setsampwidth(2)
        output.setframerate(RATE)
        output.writeframes(pcm.tobytes())
    print(name, f'{length/RATE:.2f}s', f'peak={peak:.3f}',
          f'loop_jump={abs(int(pcm[0])-int(pcm[-1]))}')


if __name__ == '__main__':
    render('Area1_Normal.wav', 100, 64,
           [(48,52,55), (53,57,60), (45,48,52), (55,59,62)],
           [(72,None,76,79,76,None,72,None), (69,None,72,76,72,None,69,None),
            (69,None,72,76,72,69,None,None), (71,None,74,79,74,None,71,None)], 'normal')
    render('Area1_MiniBoss.wav', 120, 12,
           [(45,48,52), (43,47,50), (48,52,55)],
           [(69,None,72,69,67,None,64,None), (67,None,71,74,71,None,67,None),
            (72,None,76,72,69,None,67,None)], 'mini')
    render('Area1_Boss.wav', 140, 48,
           [(40,43,47), (38,41,45), (36,40,43), (43,47,50)],
           [(64,None,64,67,64,None,59,62), (62,None,65,62,57,None,60,None),
            (60,None,64,67,64,None,60,None), (62,None,67,71,67,62,59,None)], 'boss')
