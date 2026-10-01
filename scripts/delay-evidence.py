"""Verify delay captures against raw USB, including writes, restoration and firmware.

Offline; standard library only. --disassembly additionally uses capstone==5.0.9.
"""
import argparse
import base64
from collections import Counter
from datetime import datetime
import hashlib
import importlib.util
import json
from pathlib import Path
import struct
import zipfile

spec = importlib.util.spec_from_file_location('protocol', Path(__file__).with_name('phaser-evidence.py'))
protocol = importlib.util.module_from_spec(spec)
spec.loader.exec_module(protocol)

# Captured CPU addresses; never inferred using a uniform firmware delta.
BLOCKS = {'preset':0x831AC0,'caller':0x833724,'worker':0x832DA8,'initialize':0xC13C3C,
          'oscillator':0x833562,'filter':0x80A8BC,'filters':0xC488E0,'input-filter':0x8359A0,
          'runtime':0x817F86,'current-time':0x80FF84,'input-state':0x81823C}
FIELDS = [(0x696,2),(0x698,4),(0x69C,4),(0x6A0,4),(0x6DC,4)]


def require(condition, message):
    if not condition:
        raise ValueError(message)


def stable(a, b):
    return len(a) == len(b) == 0xF70 and all(x == y or 0xF6C <= i < 0xF70 for i, (x, y) in enumerate(zip(a, b)))


def packets(raw, timed=False):
    for line in raw.decode('utf-8-sig').splitlines():
        if ' TX ' in line or ' RX ' in line:
            stamp, direction, encoded = line.split(' ', 2)
            command, payload = protocol.packet(encoded)
            yield (stamp, direction, command, payload) if timed else (direction, command, payload)


def summarize(archive, mapping_only=False):
    with zipfile.ZipFile(archive) as z:
        names = z.namelist()
        def read(n): return z.read(n)
        def meta(n): return json.loads(read(n))['snapshot']
        def key(dsp, bank, physical): return (hashlib.sha256(dsp).hexdigest(), bank.hex(), physical.hex())
        result = {'format': 'cubecontrol-delay-evidence-v1', 'audioMeasured': False,
                  'completeInvestigation': False,
                  'archiveSha256': hashlib.sha256(Path(archive).read_bytes()).hexdigest(),
                  'sha256': {n: hashlib.sha256(read(n)).hexdigest() for n in sorted(names)},
                  'mapping': {}, 'traffic': {}, 'restoration': {}, 'table': [], 'trials': []}
        for series in ('map-01', 'map-02'):
            mapping = json.loads(read(series + '/mapping.json'))['mapping']
            chunks = {}
            for direction, command, payload in packets(read(series + '/transport.log')):
                if direction == 'TX': require(command in (0x11, 0x23), 'mapping is read only')
                if direction == 'RX' and command == 0x23:
                    chunks[(payload[0], int.from_bytes(payload[1:5], 'little'))] = payload[8:]
            for name, hexdata in mapping.items():
                b = read(series + '/' + name + '.bin')
                require(b.hex().upper() == hexdata, 'mapping JSON/binary mismatch')
                if name in BLOCKS:
                    address, region = (BLOCKS[name] - 0x8347F8) & 0xFFFFFFFF, 4
                else:
                    address, region = {'bank': (0, 5), 'dsp': (0, 4), 'physical': (0xFFFD8202, 4)}[name]
                actual = b''.join(chunks[(region, (address + i) & 0xFFFFFFFF)] for i in range(0, len(b), 113))
                require(actual == b, 'mapping does not match USB: ' + name)
            result['mapping'][series] = {'matchedToUsb': True}
        # Signatures are pinned independently in the protocol fixture shipped with these tests.
        fixture = json.loads((Path(__file__).parents[1] / 'CubeControl-1.1/Cube/windows/Cube.Tests/Fixtures/delay-live-code.json').read_text())
        for name, cpu in BLOCKS.items():
            if name in ('runtime','current-time','input-state'): continue
            b = read('map-02/' + name + '.bin')
            require(bytes.fromhex(fixture[str((cpu - 0x8347F8) & 0xFFFFFFFF)]) == b, 'firmware fixture mismatch: ' + name)
        result['mapping']['dspEqualAcrossMaps'] = read('map-01/dsp.bin') == read('map-02/dsp.bin')
        if mapping_only:
            return result
        for series in ('sweep-01', 'controls-01', 'transitions-01', 'modulation-01', 'runtime-01'):
            original, restored = read(series + '/original.bin'), read(series + '/restored.bin')
            first, last = meta(series + '/original.json'), meta(series + '/restored.json')
            require(stable(original, restored), 'stable restoration: ' + series)
            require(first['Bank'] == last['Bank'] and first['PhysicalState'] == last['PhysicalState'], 'context restoration')
            slot = base64.b64decode(first['PhysicalState'])[0]
            received, writes, counts = [], [], Counter()
            memory = {}
            assembled = None
            bank = physical = None
            pending = None
            external_time = None
            external_samples = []
            for stamp, direction, command, payload in packets(read(series + '/transport.log'), timed=True):
                counts[f'{direction}-{command:02X}'] += 1
                if direction == 'TX':
                    require(command in (0x11, 0x22, 0x23), 'unexpected command')
                    pending = (command, payload)
                elif command == 0x23:
                    require(pending is not None and pending[0] == 0x23 and payload[:8] == pending[1], 'read response/header mismatch')
                    region = payload[0]; address = int.from_bytes(payload[1:5], 'little')
                    length = int.from_bytes(payload[5:8], 'little')
                    require(len(payload) == length + 8, 'read length')
                    memory.update({(region, (address + i) & 0xFFFFFFFF): v for i, v in enumerate(payload[8:])})
                    if region == 4 and address == ((0x80FF84-0x8347F8)&0xFFFFFFFF): external_time = payload[8:]
                    if series == 'runtime-01' and region == 4 and address == ((0x817F86-0x8347F8)&0xFFFFFFFF):
                        external_samples.append((stamp, external_time, payload[8:]))
                    if region == 5 and address == 0 and length == 48: bank = bytes(payload[8:])
                    if region == 4 and address == 0xFFFD8202: physical = bytes(payload[8:])
                    if region == 4 and address == 0 and length == 113: assembled = bytearray()
                    if assembled is not None and region == 4 and address == len(assembled):
                        assembled.extend(payload[8:])
                        if len(assembled) == 0xF70:
                            received.append(key(assembled, bank, physical)); assembled = None
                if direction != 'TX' or command != 0x22: continue
                region = payload[0]; address = int.from_bytes(payload[1:5], 'little')
                length = int.from_bytes(payload[5:8], 'little')
                require(len(payload) == 8 + length, 'write length')
                minimum = 0x63C if series == 'modulation-01' else 0x694
                require((region == 4 and minimum <= address and address + length <= 0x6E0) or
                        (region == 5 and length == 1 and address in [0x80000000 + slot * 16 + n for n in (4,5,6,7,8,11)]), 'write outside allowlist')
                source = address - 0x80000000 if region == 5 else address
                before = bytes(memory[(region, source + i)] for i in range(length))
                writes.append({'region': region, 'address': address, 'data': bytes(payload[8:]).hex(), 'before': before.hex(),
                               'delayStateBefore': memory[(4,0x694)], 'modStateBefore': memory[(4,0x63C)],
                               'sectionBefore': memory[(5,slot*16+11)]})
            stored = []
            excluded_volume = {}
            for n in names:
                if n.startswith(series + '/') and n.endswith('.bin'):
                    context = meta(n[:-4] + '.json'); b = read(n)
                    require(base64.b64decode(context['Dsp']) == b, 'snapshot JSON/binary mismatch')
                    stored.append(key(b, base64.b64decode(context['Bank']), base64.b64decode(context['PhysicalState'])))
                    if b[0xF6C:0xF70] != original[0xF6C:0xF70]:
                        excluded_volume[n] = {'bytes': b[0xF6C:0xF70].hex(),
                            'value': struct.unpack_from('<f', b, 0xF6C)[0],
                            'offsets': [hex(i) for i in range(0xF6C,0xF70) if b[i] != original[i]]}
            # Guard-only snapshots are also read, but not separately saved. Every saved snapshot
            # must have its own matching occurrence in raw traffic, including its exact context.
            require(not (Counter(stored) - Counter(received)), 'snapshot not evidenced by USB')
            intents = [json.loads(read(n)) for n in sorted(names) if n.startswith(series + '/intent-')]
            require(len(intents) == len(writes), 'one intent per write')
            for intent, write in zip(intents, writes):
                require(intent['region'] == write['region'] and intent['address'] == write['address'] and
                        base64.b64decode(intent['after']).hex() == write['data'] and base64.b64decode(intent['before']).hex() == write['before'], 'intent/read/write mismatch')
                if write['region'] == 4 and intent['kind'] != 'inactive-restore':
                    require((write['address'], len(bytes.fromhex(write['data']))) in FIELDS, 'non-control active write')
                    require(write['delayStateBefore']==3 and write['sectionBefore']==1,'control write outside active delay')
                elif intent['kind']=='inactive-restore':
                    require(write['delayStateBefore']==write['modStateBefore']==write['sectionBefore']==0,'inactive restore while processing')
            result['traffic'][series] = {'counts': dict(counts), 'writes': writes, 'intentCount': len(intents),
                                         'snapshotsMatchedToUsb': len(stored), 'guardSnapshots': len(received) - len(stored), 'flashWrites': 0}
            result['restoration'][series] = {'bankExact': True, 'physicalExact': True, 'stableDspExact': True,
                                             'dspExact': original == restored, 'differences': [hex(i) for i, (a, b) in enumerate(zip(original, restored)) if a != b]}
            result['restoration'][series]['excludedVolumeDifferences'] = {
                'region': '04:0xF6C+4', 'originalBytes': original[0xF6C:0xF70].hex(),
                'originalValue': struct.unpack_from('<f', original, 0xF6C)[0],
                'snapshotsDifferingFromOriginal': excluded_volume}
            if series == 'runtime-01':
                require(len(external_samples)==30,'runtime sample count')
                result['runtime']={}
                for group,name in enumerate(('original','half','frozen')):
                    values=[]
                    for repeat in range(10):
                        sample=json.loads(read(f'runtime-01/runtime-{name}-{repeat}-external.json'))
                        stamp,time,state=external_samples[group*10+repeat]
                        require(base64.b64decode(sample['CurrentTime'])==time and base64.b64decode(sample['Runtime'])==state,'runtime sample not evidenced by USB')
                        require(datetime.fromisoformat(sample['Started']) <= datetime.fromisoformat(stamp) <= datetime.fromisoformat(sample['Finished']),'runtime timestamp not bracketed')
                        require(struct.unpack('<h',time)[0]==9600,'smoothed time not settled')
                        values.append({'receivedUtc':stamp,'phase':struct.unpack_from('<f',state,0x26)[0],'index':struct.unpack_from('<h',state)[0]})
                    elapsed=(datetime.fromisoformat(values[-1]['receivedUtc'])-datetime.fromisoformat(values[0]['receivedUtc'])).total_seconds()
                    slope=((values[-1]['phase']-values[0]['phase'])%1)/elapsed
                    require(len({v['index'] for v in values})>1,'ring did not advance')
                    require(all(0<=v['phase']<=1 and 0<=v['index']<16384 for v in values),'runtime state bounds')
                    if name=='frozen': require(len({v['phase'] for v in values})==1,'zero rate did not freeze phase')
                    else: require(slope>0,'nonzero rate did not advance phase')
                    result['runtime'][name]={'samples':values,'phaseCyclesPerWallSecond':slope,'audioMeasurement':False}
                result['runtime']['halfToOriginalSlopeRatio']=result['runtime']['half']['phaseCyclesPerWallSecond']/result['runtime']['original']['phaseCyclesPerWallSecond']
                require(0 < result['runtime']['halfToOriginalSlopeRatio'] < 1,'half-rate phase did not slow down')
        baseline = read('sweep-01/baseline.bin')
        for name,maximum,offset,fmt in [('time',31,0x696,'<h'),('feedback',127,0x698,'<f'),('mix',118,0x6DC,'<f')]:
            for raw in range(maximum+1):
                target=bytearray(baseline);value=2400+450*raw if name=='time' else raw/128
                struct.pack_into(fmt,target,offset,value)
                for repeat in range(3): require(stable(read(f'sweep-01/{name}-{raw:03}-{repeat}.bin'),target),'sweep mismatch')
                result['table'].append({'control':name,'raw':raw,'internal':value})
            require(stable(read(f'sweep-01/{name}-restored.bin'),baseline),'sweep family restore')
        baseline=read('controls-01/baseline.bin')
        trials={'depth-zero':(0x69C,'<f',0),'depth-half':(0x69C,'<f',.05),'rate-zero':(0x6A0,'<f',0),
                'rate-half':(0x6A0,'<f',.005),'time-low':(0x696,'<h',2400),'time-fine':(0x696,'<h',9601),
                'time-high':(0x696,'<h',16350),'feedback-zero':(0x698,'<f',0),'feedback-fine':(0x698,'<f',.123),
                'mix-zero':(0x6DC,'<f',0),'mix-fine':(0x6DC,'<f',.321)}
        for cycle in range(2):
            for name,(offset,fmt,value) in trials.items():
                target=bytearray(baseline);struct.pack_into(fmt,target,offset,value)
                for phase in ('checked','delayed','same','restore-checked','restored'):
                    require(stable(read(f'controls-01/{cycle}-{name}-{phase}.bin'),baseline if phase in ('restore-checked','restored') else target),'trial mismatch: '+name+'-'+phase)
                result['trials'].append({'cycle':cycle,'name':name,'offset':hex(offset),'isolatedPersistedRestored':True})
        baseline=read('transitions-01/baseline.bin');custom=bytearray(baseline)
        struct.pack_into('<hfff',custom,0x696,9601,.123,.05,.005)
        struct.pack_into('<f',custom,0x6DC,.321)
        for name,offset,fmt,value in [('time',0x696,'<h',10050),('feedback',0x698,'<f',33/128),('mix',0x6DC,'<f',33/128),('mod-off',None,None,None)]:
            require(stable(read(f'transitions-01/custom-{name}-checked.bin'),custom),'transition customization')
            target=bytearray(custom)
            if offset is not None: struct.pack_into(fmt,target,offset,value)
            require(stable(read(f'transitions-01/{name}-changed.bin'),target),'selective reload')
            require(stable(read(f'transitions-01/{name}-restore-checked.bin'),baseline),'transition restore')
        require(stable(read('transitions-01/custom-section-checked.bin'),custom),'section customization')
        require(read('transitions-01/section-off.bin')[0x694]==0,'section off')
        require(stable(read('transitions-01/section-on.bin'),baseline),'section reload')
        result['transitions']={'sameValuesPreserve':True,'changedControlReloadsOnlyItsField':True,'switchingBetweenMod7And8Preserves':True,'sectionResetsAllControls':True}
        baseline=read('modulation-01/custom-modulation-checked.bin')
        original=read('modulation-01/original.bin')
        for mod,algorithm in [(0,5),(9,2)]:
            for repeat in range(3):
                b=read(f'modulation-01/mod-{mod}-{repeat}.bin')
                require(b[0x63C]==3 and b[0x63D]==algorithm,'modulation algorithm')
                require(b[0x694:0x6E0]==baseline[0x694:0x6E0],'delay changed with modulation')
                protected=bytearray(b);protected[0x63C:0x6E0]=original[0x63C:0x6E0]
                require(stable(protected,original),'unrelated DSP changed during modulation')
            require(read(f'modulation-01/mod-{mod}-off.bin')[0x63C]==0,'modulation returns off')
        result['transitions']['chorusAndPhaserPreserveDelayControls']=True
        result['completeInvestigation']=True
        return result


def disassembly(archive):
    from capstone import Cs, CS_ARCH_ARM, CS_MODE_THUMB
    from capstone.arm import ARM_OP_MEM, ARM_REG_PC
    cs = Cs(CS_ARCH_ARM, CS_MODE_THUMB); cs.detail = True
    # Exclude literal pools from each contiguous routine; preset branches cross several pools.
    segments = {'preset':[(0,0x270),(0x298,0x1B0)],'caller':[(0,0x124)],'worker':[(0,0x306)],
                'initialize':[(0,0x3C)],'oscillator':[(0,0x28)],'filter':[(0,0x32)],'input-filter':[(0,0x64)]}
    lines = ['Delay live capture. Literal pools omitted; PC-relative constants annotated.']
    with zipfile.ZipFile(archive) as z:
        for name, spans in segments.items():
            b = z.read('map-02/' + name + '.bin'); base = BLOCKS[name]
            lines.append(f'\n{name} CPU={base:08X} SHA256={hashlib.sha256(b).hexdigest()}')
            for offset, length in spans:
                for ins in cs.disasm(b[offset:offset+length], base+offset):
                    extra = ''
                    for op in ins.operands:
                        if op.type == ARM_OP_MEM and op.mem.base == ARM_REG_PC:
                            literal = ((ins.address + 4) & ~3) + op.mem.disp
                            if base <= literal <= base + len(b) - 4:
                                extra = f' ; [{literal:08X}]={struct.unpack_from("<I", b, literal-base)[0]:08X}'
                    lines.append(f'{ins.address:08X} {ins.bytes.hex():10s} {ins.mnemonic:10s} {ins.op_str}{extra}')
    return '\n'.join(lines) + '\n'


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('archive'); parser.add_argument('--output'); parser.add_argument('--disassembly')
    parser.add_argument('--mapping-only',action='store_true',help='Verify only the two read-only mapping captures; never mark the investigation complete')
    args = parser.parse_args()
    summary = json.dumps(summarize(args.archive,args.mapping_only), indent=2) + '\n'
    if args.output:
        Path(args.output).parent.mkdir(parents=True, exist_ok=True); Path(args.output).write_text(summary, encoding='utf-8')
    else: print(summary)
    if args.disassembly: Path(args.disassembly).write_text(disassembly(args.archive), encoding='utf-8')
