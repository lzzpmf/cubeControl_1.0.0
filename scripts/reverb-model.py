"""Structural reconstruction of the captured reverb, not recorded pedal audio.

48 kHz host-rate assumption; the worker executes its delay network at half rate.
Implements the captured decimator, Q15 delays, Q14 feedback, repeated integer
damping-helper calls, two float allpasses, three biquads, and output interpolation.
Python floats do not reproduce ARM float32/FMA rounding or concurrent updates.
"""
import argparse
import cmath
import hashlib
import json
import math
from pathlib import Path
import struct
import zipfile


def sat16(x): return max(-32768, min(32767, x))


def load(archive):
    with zipfile.ZipFile(archive) as z:
        default = z.read('map-02/default.bin')
        return {'lengths': struct.unpack_from('<9h', default, 0x16),
                'filters': [struct.unpack_from('<5f', default, 0x30 + i * 28) for i in range(3)],
                'feedback': [struct.unpack_from('<6h', z.read('map-02/feedback-table.bin'), i * 12) for i in range(15)],
                'input': struct.unpack_from('<2f', z.read('map-02/input-filter.bin'), 0x64)}


def simulate(data, signal='impulse', dry=1, wet=.55, decay=8, return_level=80, damping=10, count=96000):
    lengths = data['lengths']; coeff = data['feedback'][decay-1]
    combs = [[0] * n for n in lengths[:6]]; positions = [0] * 6; history = [0] * 6
    allpasses = [[0.] * n for n in lengths[6:8]]; ap_positions = [0, 0]
    predelay = [0] * lengths[8]; pre_position = 0
    filter_states = [[0.] * 4 for _ in range(3)]
    a, b = data['input']; previous_input = branch_a = branch_b = previous_wet = 0.
    output = []; wet_output = []; saturation = 0
    for n in range(0, count, 2):
        def sample(i):
            if signal == 'silence': return 0.
            if signal == 'impulse': return .1 if i == 0 else 0.
            return .02 * math.sin(2 * math.pi * 440 * i / 48000)
        x0, x1 = sample(n), sample(n+1)
        t0, t1 = x0 - a * branch_a, previous_input - b * branch_b
        decimated = .5 * (branch_a + a * t0 + branch_b + b * t1)
        branch_a, branch_b, previous_input = t0, t1, x1
        excitation = predelay[pre_position]
        predelay[pre_position] = sat16(math.trunc(decimated * .25 * 32768))
        pre_position = (pre_position + 1) % lengths[8]
        summed = excitation
        for i in range(6):
            delayed = combs[i][positions[i]]
            summed += (return_level * delayed) >> 7
            damp = (40 + 2 * i) * damping // 10
            def helper():
                value = damp * history[i] + (512 - damp) * (delayed << 5)
                assert -(1 << 31) <= value < (1 << 31)
                history[i] = value >> 9
                return sat16(value >> 14)
            # The actual worker calls its state-mutating helper again for the
            # lower-bound check and a third time for the normal store path.
            candidate = excitation + ((coeff[i] * helper()) >> 14)
            if candidate >= 32768: value = 32767; saturation += 1
            else:
                candidate = excitation + ((coeff[i] * helper()) >> 14)
                if candidate < -32768: value = -32768; saturation += 1
                else: value = sat16(excitation + ((coeff[i] * helper()) >> 14))
            combs[i][positions[i]] = value
            positions[i] = (positions[i] + 1) % lengths[i]
        y = sat16(summed) / 32768
        g = struct.unpack('<f', struct.pack('<f', .7))[0]
        ap0, ap1 = ap_positions
        diffused = allpasses[1][ap1] - g * y
        middle = y + g * diffused
        inner = allpasses[0][ap0] - g * middle
        allpasses[0][ap0] = middle + g * inner
        allpasses[1][ap1] = inner
        ap_positions = [(ap0+1) % lengths[6], (ap1+1) % lengths[7]]
        y = diffused
        for c, state in zip(data['filters'], filter_states):
            x = y; x1s, x2s, y1s, y2s = state
            y = c[0]*x + c[1]*x1s + c[2]*x2s - c[3]*y1s - c[4]*y2s
            state[:] = [x, x1s, y, y1s]
        wet_output.extend([.5 * (y + previous_wet), y]); previous_wet = y
        output.extend([dry*x0 + wet*wet_output[-2], dry*x1 + wet*y])
    assert all(math.isfinite(v) for v in output)
    return {'peak': max(map(abs, output)), 'rms': math.sqrt(sum(v*v for v in output)/count),
            'wetTailRmsLastQuarter': math.sqrt(sum(v*v for v in wet_output[3*count//4:])/(count//4)),
            'combSaturations': saturation}, output


def verify(archive):
    data = load(archive); lengths = data['lengths']
    # Firmware allocates six int16 combs, aligns to a float boundary, then two
    # float allpasses and an int16 predelay in a shared 0x5800-byte pool.
    used_halfwords = ((sum(lengths[:6]) + 1) & ~1) + 2*sum(lengths[6:8]) + lengths[8]
    assert all(n > 0 for n in lengths) and used_halfwords <= 0x2C00
    poles = []
    for c in data['filters']:
        roots = [(-c[3] + sign*cmath.sqrt(c[3]**2 - 4*c[4]))/2 for sign in (-1, 1)]
        assert all(abs(r) < 1 for r in roots); poles.append([abs(r) for r in roots])
    assert all(0 <= v < 16384 for row in data['feedback'] for v in row)
    assert all(0 <= (40+2*i)*step//10 < 512 for i in range(6) for step in range(11))
    scenarios = {}
    for name, params in [('factory-impulse', {}), ('factory-sine', {'signal': 'sine'}),
                         ('silence', {'signal': 'silence'}), ('dry-zero', {'dry': 0}),
                         ('wet-zero', {'wet': 0}), ('decay-low', {'decay': 1}),
                         ('decay-high', {'decay': 15}), ('return-zero', {'return_level': 0}),
                         ('return-half', {'return_level': 40}), ('damping-zero', {'damping': 0}),
                         ('damping-half', {'damping': 5})]:
        summary, output = simulate(data, **params); scenarios[name] = summary
        if name == 'silence': assert all(v == 0 for v in output)
        if name == 'wet-zero': assert output[0] == .1 and all(v == 0 for v in output[1:])
    return {'format': 'cubecontrol-reverb-structural-model-v1', 'audioMeasured': False,
            'archiveSha256': hashlib.sha256(Path(archive).read_bytes()).hexdigest(), 'assumptions': __doc__,
            'hostRateAssumed': 48000, 'delayNetworkRateInferred': 24000,
            'lengthsSamplesAtHalfRate': lengths, 'poolUsedBytes': used_halfwords*2, 'poolCapacityBytes': 0x5800,
            'biquadPoleMagnitudes': poles, 'maximumFeedback': max(max(row) for row in data['feedback'])/16384,
            'checks': ['bounded fixed buffers', 'finite outputs', 'stable biquad poles',
                       'feedback magnitudes below one', 'convex damping', 'silence stays zero', 'wet zero equals dry input'],
            'scenarios': scenarios,
            'limitations': ['No measured audio, headroom or acoustic certification',
                            'No float32/FMA exact emulation', 'Zero initial audio states',
                            'No codec, analog noise, block-boundary race or preset-transition simulation',
                            'Return zero leaves the direct excitation in the reverberated branch; it is not wet zero']}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__); parser.add_argument('archive'); parser.add_argument('--output')
    args = parser.parse_args(); result = json.dumps(verify(args.archive), indent=2) + '\n'
    if args.output:
        Path(args.output).parent.mkdir(parents=True, exist_ok=True); Path(args.output).write_text(result, encoding='utf-8')
    else: print(result)
