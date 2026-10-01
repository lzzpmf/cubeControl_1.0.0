"""Offline structural chorus model, not a recording or bit-exact firmware emulator.

Assumptions: 48 kHz, normalized cosine LFO for selector 0, zero initial phases,
identity biquads (or their isolated b0 attenuations), Python float arithmetic.
Selector 1 uses the triangle equation demonstrated by captured ARM code.
"""
import argparse
import json
import math
from pathlib import Path


def simulate(depth=0.4, step=0.000031, mix=0.85, feedback=0, waveform=0, wet_gain=1, combined_gain=1, impulse=False):
    ring = [0.0] * 900
    index = 0
    phase = 0
    peak = energy = 0.0
    minimum, maximum = 900.0, 0.0
    output = []
    for n in range(48000):
        x = float(n == 0) if impulse else 0.2 * math.sin(2 * math.pi * 440 * n / 48000)
        phase += step
        if phase > 1: phase -= 1
        lfo = (2 * phase if phase < 0.5 else 2 - 2 * phase) if waveform == 1 else 0.5 - 0.5 * math.cos(2 * math.pi * phase)
        delay = 400 + 499 * depth * lfo
        minimum, maximum = min(minimum, delay), max(maximum, delay)
        position = index - delay - 1
        if position < 0: position += 900
        first = int(position)
        second = 0 if first + 1 >= 900 else first + 1
        assert 0 <= first < 900 and 0 <= second < 900
        fraction = position - first
        delayed = ring[first] * (1 - fraction) + ring[second] * fraction
        # Both firmware phases start equal and receive the same step.
        processed = combined_gain * 0.5 * (x + wet_gain * delayed)
        ring[index] = x + feedback * processed
        index = (index + 1) % 900
        y = (1 - mix) * x + mix * processed
        assert math.isfinite(y)
        peak = max(peak, abs(y))
        energy += y * y
        output.append(y)
    return {'peak': peak, 'rms': math.sqrt(energy / len(output)), 'delayArgumentMin': minimum,
            'delayArgumentMax': maximum, 'effectiveDelayIncludesExtraSample': True}, output


def verify():
    # Exercise the entire oscillator envelope and every write index, independently
    # of whether a one-second waveform has completed a full modulation cycle.
    for index in range(900):
        for depth in (0, 0.2, 0.4):
            for tick in range(129):
                position = index - (400 + 499 * depth * tick / 128) - 1
                if position < 0: position += 900
                assert 0 <= int(position) < 900
    results = {}
    for name, parameters in [('factory', {}), ('depth-half', {'depth': 0.2}), ('depth-zero', {'depth': 0}),
            ('rate-low', {'step': 0.000008}), ('rate-high', {'step': 0.000063}),
            ('mix-zero', {'mix': 0}), ('mix-half', {'mix': 0.425}), ('feedback-small', {'feedback': 0.1}),
            ('triangle', {'waveform': 1}), ('wet-gain', {'wet_gain': 0.5}), ('combined-gain', {'combined_gain': 0.5})]:
        summary, _ = simulate(**parameters)
        assert summary['peak'] <= (0.2 / (1 - parameters.get('feedback', 0))) + 1e-12
        results[name] = summary
    _, bypass = simulate(mix=0)
    assert all(y == 0.2 * math.sin(2 * math.pi * 440 * n / 48000) for n, y in enumerate(bypass))
    _, impulse = simulate(depth=0, impulse=True)
    assert abs(impulse[0] - 0.575) < 1e-12 and abs(impulse[401] - 0.425) < 1e-12
    assert sum(abs(v) for i, v in enumerate(impulse) if i not in (0, 401)) == 0
    return {'format': 'cubecontrol-chorus-structural-model-v1', 'audioMeasured': False,
            'assumptions': __doc__, 'checks': ['ring indices bounded', 'finite outputs', 'conservative feedback gain bound',
            'mix zero equals input', 'depth zero impulse has taps at 0 and 401 samples'], 'scenarios': results}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output')
    args = parser.parse_args()
    text = json.dumps(verify(), indent=2) + '\n'
    if args.output: Path(args.output).write_text(text, encoding='utf-8')
    else: print(text)
