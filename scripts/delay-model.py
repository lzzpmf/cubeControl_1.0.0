"""Structural delay model, not pedal audio. Python floats do not emulate ARM FMA.

48 kHz host rate is assumed; the captured worker processes half as many samples.
The normalized sinusoidal ROM helper is an inference inherited from the chorus
investigation, not a full reconstruction of ROM address 0x5FC50.
Two identical interpolated reads of one ring are each scaled by 0.45, then two
biquads feed both the output and feedback. The previous wet sample is stored as
int16 AFTER unscaled float-to-int truncation; this unusual firmware detail is
preserved. No codec, analog noise, concurrent writes or startup fades modeled.
"""
import argparse
import cmath
import hashlib
import json
import math
from pathlib import Path
import struct
import zipfile


def load(source):
    p = Path(source)
    if p.is_dir():
        read = lambda name: (p / name).read_bytes()
        return read('filters.bin'), read('input-filter.bin')
    with zipfile.ZipFile(p) as z:
        return z.read('map-02/filters.bin'), z.read('map-02/input-filter.bin')


def simulate(coefficients, input_coefficients, *, signal='impulse', time=9600,
             initial_time=None, initial_phase=0., feedback=.25, depth=.1, rate=.01, mix=.25, count=96000,
             sine_amplitude=.02):
    ring = [0] * 16384; index = 0; phase = initial_phase; previous_wet = 0
    current_time = time if initial_time is None else initial_time
    states = [[0.] * 4 for _ in coefficients]
    previous_input = branch_a = branch_b = 0.
    a, b = input_coefficients
    dry, wet = (1., 2*mix) if mix < .5 else (2-2*mix, 1.)
    output = []; saturation = 0; low = 16384.; high = 0.; ring_min = 0; ring_max = 0
    # Block size is a model assumption, not a measured DMA size. Phase increment
    # is recomputed once per worker block from the then-current smoothed time.
    for block in range(0, count, 256):
        increment = rate/current_time
        for n in range(block, min(block+256,count), 2):
            def sample(i):
                if signal == 'silence': return 0.
                if signal == 'impulse': return .1 if i == 0 else 0.
                return sine_amplitude*math.sin(2*math.pi*440*i/48000)
            x0,x1 = sample(n), sample(n+1)
            t0,t1 = x0-a*branch_a, previous_input-b*branch_b
            decimated = .5*(branch_a+a*t0+branch_b+b*t1)
            branch_a,branch_b,previous_input = t0,t1,x1
            current_time += (time > current_time) - (time < current_time)
            phase += increment
            if phase > 1: phase -= 1
            lfo = .5*(1+math.sin(2*math.pi*phase+1.5*math.pi))
            delay = current_time*(1-depth+depth*lfo)
            low,high = min(low,delay),max(high,delay)
            position = index-delay
            if position < 0: position += 16384
            first = math.trunc(position); second = (first+1)%16384; frac = position-first
            assert 0 <= first < 16384 and 0 <= second < 16384
            y = .9*((1-frac)*ring[first]+frac*ring[second])/32768
            for c,state in zip(coefficients,states):
                value = y
                y = c[0]*value+c[1]*state[0]+c[2]*state[1]-c[3]*state[2]-c[4]*state[3]
                state[:] = [value,state[0],y,state[2]]
            value = math.trunc((decimated+feedback*y)*32768)
            saturation += int(value < -32768 or value > 32767)
            ring[index] = max(-32768,min(32767,value))
            ring_min,ring_max = min(ring_min,ring[index]),max(ring_max,ring[index])
            index = (index+1)%16384
            output.extend([dry*x0+wet*.5*(y+previous_wet), dry*x1+wet*y])
            previous_wet = ((math.trunc(y)+32768)%65536)-32768
    assert all(math.isfinite(x) for x in output)
    return {'peak':max(map(abs,output)), 'rms':math.sqrt(sum(x*x for x in output)/len(output)),
            'saturations':saturation,'delaySamplesMin':low,'delaySamplesMax':high,
            'finalSmoothedTime':current_time,'ringWrittenMin':ring_min,'ringWrittenMax':ring_max}, output


def verify(source):
    filters, raw = load(source)
    coefficients = [struct.unpack_from('<5f', filters, i*28) for i in range(2)]
    input_coefficients = struct.unpack_from('<2f',raw,0x64)
    poles = [[abs((-c[3]+sign*cmath.sqrt(c[3]**2-4*c[4]))/2) for sign in (-1,1)] for c in coefficients]
    assert all(p < 1 for pair in poles for p in pair)
    # Sufficient infinity-norm bound for the feedback operator, including
    # time-varying convex ring interpolation and the fixed 0.9 tap sum.
    state = [[0.]*4 for _ in coefficients]; impulse = []
    for n in range(16384):
        y = 1. if n == 0 else 0.
        for c,s in zip(coefficients,state):
            x=y; y=c[0]*x+c[1]*s[0]+c[2]*s[1]-c[3]*s[2]-c[4]*s[3];s[:]=[x,s[0],y,s[2]]
        impulse.append(y)
    l1 = sum(map(abs,impulse))
    peak_frequency_gain = max(abs(math.prod((c[0]+c[1]*cmath.exp(-1j*w)+c[2]*cmath.exp(-2j*w))/(1+c[3]*cmath.exp(-1j*w)+c[4]*cmath.exp(-2j*w)) for c in coefficients)) for w in [math.pi*i/8192 for i in range(8193)])
    scenarios={}
    for name,params in [('factory',{}),('silence',{'signal':'silence'}),('sine',{'signal':'sine'}),
        ('depth-zero',{'depth':0}),('depth-half',{'depth':.05}),('rate-zero',{'rate':0}),('rate-half',{'rate':.005}),
        ('rate-zero-preserves-phase',{'rate':0,'initial_phase':.37}),
        ('full-lfo-cycle',{'count':1920000}),
        ('time-low',{'time':2400}),('time-high',{'time':16350}),('time-fine',{'time':9601}),
        ('time-ramp-up',{'time':16350,'initial_time':2400}),('time-ramp-down',{'time':2400,'initial_time':16350}),
        ('feedback-zero',{'feedback':0}),('feedback-high',{'feedback':127/128}),('feedback-fine',{'feedback':.123}),
        ('repeat-decay',{'time':2400,'depth':0,'feedback':.5,'mix':.5}),
        ('saturation-stress',{'signal':'sine','sine_amplitude':2,'feedback':127/128}),
        ('mix-zero',{'mix':0}),('mix-center',{'mix':.5}),('mix-high',{'mix':118/128}),('mix-fine',{'mix':.321})]:
        summary,out=simulate(coefficients,input_coefficients,**params);scenarios[name]=summary
        assert 2160 <= summary['delaySamplesMin'] <= summary['delaySamplesMax'] <= 16350
        if name=='silence': assert all(x==0 for x in out)
        if name=='mix-zero': assert out[0]==.1 and all(x==0 for x in out[1:])
        if name=='depth-zero': assert summary['delaySamplesMin']==summary['delaySamplesMax']==9600
        if name=='rate-zero-preserves-phase': assert summary['delaySamplesMin']==summary['delaySamplesMax']
        if name=='full-lfo-cycle': assert summary['delaySamplesMin'] < 8640.01 and summary['delaySamplesMax'] > 9599.99
        if name=='saturation-stress':
            assert summary['saturations'] > 0 and summary['ringWrittenMin']==-32768 and summary['ringWrittenMax']==32767
        if name=='repeat-decay':
            energies=[math.sqrt(sum(x*x for x in out[4800*n-128:4800*n+128])) for n in (1,2,3)]
            assert energies[0] > energies[1] > energies[2] > 0
            summary['repeatWindowsHostSamples']=[4800,9600,14400]
            summary['repeatWindowL2']=energies
            summary['successiveWindowRatios']=[energies[i+1]/energies[i] for i in (0,1)]
    return {'format':'cubecontrol-delay-model-v1','audioMeasured':False,'assumptions':__doc__,
        'archiveSha256':hashlib.sha256(Path(source).read_bytes()).hexdigest() if Path(source).is_file() else None,
        'filtersSha256':hashlib.sha256(filters).hexdigest(),'inputFilterSha256':hashlib.sha256(raw).hexdigest(),
        'hostRateAssumed':48000,'networkRateInferred':24000,
        'ringSamples':16384,'biquadPoleMagnitudes':poles,'filterImpulseL1':l1,
        'sampledFilterPeakMagnitude':peak_frequency_gain,'maximumStaticLoopMagnitude':.9*127/128*peak_frequency_gain,
        'timeVaryingL1Bound':.9*127/128*l1,
        'stabilityNote':'Frequency-grid gain and finite simulations are not a proof of arbitrary time-varying stability; L1 bound >= 1 is inconclusive.',
        'scenarios':scenarios}


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('source');parser.add_argument('--output')
    args=parser.parse_args();result=json.dumps(verify(args.source),indent=2)+'\n'
    if args.output:
        Path(args.output).parent.mkdir(parents=True,exist_ok=True)
        Path(args.output).write_text(result,encoding='utf-8')
    else: print(result)
