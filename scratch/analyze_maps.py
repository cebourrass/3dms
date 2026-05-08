import os
import struct

def analyze_map(f_path):
    if not os.path.exists(f_path):
        return f"{os.path.basename(f_path)}: File not found"
    
    with open(f_path, 'rb') as f:
        # Offset 40
        f.seek(40)
        p_count = struct.unpack('<I', f.read(4))[0]
        m_count = struct.unpack('<I', f.read(4))[0]
        
        # Skip markers
        for _ in range(m_count):
            name_len_data = f.read(1)
            if not name_len_data: break
            name_len = name_len_data[0]
            f.read(name_len + 16) # name + lon + lat
            
        # Skip TrackWidth (8 bytes)
        f.read(8)
        
        # Read Real Trajectory Count (4 bytes)
        t_count_data = f.read(4)
        if t_count_data:
            t_count = struct.unpack('<I', t_count_data)[0]
        else:
            t_count = "N/A"
            
        return (os.path.basename(f_path), p_count, m_count, t_count)

files = [
    r'Map/Circuits/France/Lédenon.map',
    r'Map/Circuits/France/Alès (Sens horaire).map',
    r'Map/Circuits/France/Nogaro.map',
    r'Map/Circuits/France/Pau Arnos.map'
]

print(f'{"Circuit":<35} | {"Partial":<8} | {"Markers":<8} | {"Total":<8}')
print("-" * 65)
for f_path in files:
    res = analyze_map(f_path)
    if isinstance(res, tuple):
        print(f'{res[0]:<35} | {res[1]:<8} | {res[2]:<8} | {res[3]:<8}')
    else:
        print(res)
