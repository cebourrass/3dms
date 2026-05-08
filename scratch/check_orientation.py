import os
import struct

directory = r'Map/Circuits/France'
values = {}

for filename in os.listdir(directory):
    if filename.endswith('.map'):
        filepath = os.path.join(directory, filename)
        with open(filepath, 'rb') as f:
            f.seek(40)
            data = f.read(4)
            if len(data) == 4:
                val = struct.unpack('<i', data)[0] # Using signed int just in case
                values[val] = values.get(val, 0) + 1

print(f'{"Value":<10} | {"Frequency":<10}')
print("-" * 25)
for val in sorted(values.keys()):
    print(f'{val:<10} | {values[val]:<10}')
