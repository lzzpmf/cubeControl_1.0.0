import sys, asyncio, json
from pathlib import Path
from datetime import datetime, timezone
sys.path.insert(0, str(Path(__file__).resolve().parent / 'bluetooth-probe-deps'))
from bleak import BleakScanner

async def main():
    state = sys.argv[1]
    duration = int(sys.argv[2]) if len(sys.argv) > 2 else 30
    rows = {}
    def received(device, adv):
        row = rows.setdefault(device.address, {'address': device.address, 'names': [], 'services': [], 'manufacturer_data': {}, 'service_data': {}, 'count': 0, 'rssi_min': adv.rssi, 'rssi_max': adv.rssi})
        row['count'] += 1
        row['rssi_min'] = min(row['rssi_min'], adv.rssi)
        row['rssi_max'] = max(row['rssi_max'], adv.rssi)
        for name in [device.name, adv.local_name]:
            if name and name not in row['names']: row['names'].append(name)
        for uuid in adv.service_uuids:
            if uuid not in row['services']: row['services'].append(uuid)
        for key, value in adv.manufacturer_data.items():
            values = row['manufacturer_data'].setdefault(str(key), [])
            if value.hex() not in values: values.append(value.hex())
        for key, value in adv.service_data.items():
            values = row['service_data'].setdefault(key, [])
            if value.hex() not in values: values.append(value.hex())
    started = datetime.now(timezone.utc).isoformat()
    async with BleakScanner(detection_callback=received, scanning_mode='active'):
        await asyncio.sleep(duration)
    output = {'state': state, 'started_utc': started, 'duration_seconds': duration, 'devices': list(rows.values())}
    directory = Path(__file__).resolve().parent / 'bluetooth-comparison'
    directory.mkdir(exist_ok=True)
    path = directory / (state + '-' + datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%SZ') + '.json')
    path.write_text(json.dumps(output, indent=2), encoding='utf-8')
    print(json.dumps({'file':str(path), 'device_count':len(rows), 'devices':[{'names':r['names'], 'services':r['services'], 'packets':r['count'], 'rssi_range':[r['rssi_min'],r['rssi_max']]} for r in rows.values()]}, indent=2))
asyncio.run(main())
