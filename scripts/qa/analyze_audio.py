"""Independent PCM16 measurements of synthetic DSP fixtures. Python stdlib only.

This checks known sine fixtures, not arbitrary music or subjective sound quality.
Never opens an audio device. Relative paths are confined to the manifest directory.
"""
import argparse
import array
import json
import math
from pathlib import Path
import sys
import wave


def measure(item, base):
    path = (base / item['path']).resolve()
    if not path.is_relative_to(base.resolve()):
        raise ValueError('Fixture path escapes manifest directory')
    with wave.open(str(path), 'rb') as wav:
        if wav.getsampwidth() != 2 or wav.getcomptype() != 'NONE':
            raise ValueError('Expected uncompressed PCM16')
        rate, channels, frames = wav.getframerate(), wav.getnchannels(), wav.getnframes()
        # Bound memory: analyze at most five seconds, while checking full declared duration.
        raw = wav.readframes(min(frames, rate * 5))
    samples = array.array('h', raw)
    if sys.byteorder != 'little':
        samples.byteswap()
    left = [x / 32768 for x in samples[::channels]]
    trim = rate // 10
    window = left[trim:-trim] if len(left) > 2 * trim else left
    if len(window) < 100:
        raise ValueError('Fixture too short for frequency measurement')
    peak = max(abs(x) for x in left)
    rms = math.sqrt(sum(x * x for x in window) / len(window))
    # Hysteresis ignores PCM dither near zero. Periods use identical positive thresholds.
    threshold = max(0.01, peak * 0.1)
    edges, armed = [], False
    for i, value in enumerate(window):
        if value < -threshold:
            armed = True
        elif armed and value >= threshold:
            edges.append(i)
            armed = False
    if len(edges) < 2:
        raise ValueError('No measurable periodic signal')
    frequency = (len(edges) - 1) * rate / (edges[-1] - edges[0])
    checks = {
        'rate': rate == item['sampleRate'],
        'channels': channels == item['channels'],
        'duration': abs(frames - item['frames']) <= 1,
        'frequency': abs(frequency / item['frequency'] - 1) <= 0.015,
        'signal_present': rms > 0.05,
        'no_full_scale_clipping': peak < 0.999,
    }
    correlation = None
    if item.get('antiPhase'):
        if channels != 2:
            checks['anti_phase'] = False
        else:
            right = [x / 32768 for x in samples[1::channels]][trim:-trim]
            norm = math.sqrt(sum(x*x for x in window) * sum(x*x for x in right))
            correlation = sum(a*b for a, b in zip(window, right)) / norm if norm else 0
            checks['anti_phase'] = correlation < -0.95
    return dict(path=str(path), frames=frames, frequency=frequency, rms=rms, peak=peak,
                correlation=correlation, checks=checks, passed=all(checks.values()))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--manifest', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    results = []
    for item in json.loads(args.manifest.read_text(encoding='utf-8-sig')):
        try:
            results.append(measure(item, args.manifest.parent))
        except Exception as error:
            results.append(dict(path=item.get('path'), passed=False, error=str(error)))
    report = dict(schema=1, scope='Synthetic sine fixtures only', frequencyTolerance=0.015,
                  results=results, passed=bool(results) and all(x['passed'] for x in results))
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, indent=2), encoding='utf-8')
    print(f"{sum(x['passed'] for x in results)}/{len(results)} audio fixtures passed: {args.output}")
    return 0 if report['passed'] else 1


if __name__ == '__main__':
    raise SystemExit(main())
