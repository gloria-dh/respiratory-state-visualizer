import serial
import struct
import time
import sys
import argparse
from datetime import datetime

CLI_BAUD_RATE = 115200
DATA_BAUD_RATE = 921600

MAGIC_WORD = b'\x02\x01\x04\x03\x06\x05\x08\x07'
VITALS_TLV_TYPE = 1040


def send_config(cli_port_name, config_file_path):
    print(f"[CONFIG] Opening: {config_file_path}")

    with open(config_file_path, 'r') as f:
        config_lines = f.readlines()

    print(f"[CONFIG] Loaded {len(config_lines)} lines")

    cli_ser = serial.Serial(cli_port_name, CLI_BAUD_RATE, timeout=1)
    print(f"[CONFIG] Sending to {cli_port_name}...")

    for line in config_lines:
        line = line.strip()
        if not line or line.startswith('%'):
            continue
        cli_ser.write((line + '\n').encode('utf-8'))
        time.sleep(0.03)
        cli_ser.readline()
        cli_ser.readline()

    time.sleep(0.5)
    cli_ser.close()
    print("[CONFIG] Done. CLI port closed.\n")


def parse_vitals_tlv(tlv_data):
    fmt = '<2H3f'
    size = struct.calcsize(fmt)
    if len(tlv_data) < size:
        return None
    try:
        unpacked = struct.unpack(fmt, tlv_data[:size])
        return {
            'id': unpacked[0],
            'rangeBin': unpacked[1],
            'breathDeviation': unpacked[2],
            'heart_rate': unpacked[3],
            'breath_rate': unpacked[4],
        }
    except struct.error:
        return None


def parse_all_vitals_fields(tlv_data):
    fmt = '<2H33f'
    size = struct.calcsize(fmt)
    if len(tlv_data) < size:
        return None
    try:
        return struct.unpack(fmt, tlv_data[:size])
    except struct.error:
        return None


def parse_frame(frame_data):
    try:
        num_tlvs = struct.unpack('<I', frame_data[16:20])[0]
    except struct.error:
        return None, None

    ptr = 24
    vitals = None
    raw_fields = None

    for _ in range(num_tlvs):
        if ptr + 8 > len(frame_data):
            break
        try:
            tlv_type, tlv_len = struct.unpack('<II', frame_data[ptr:ptr + 8])
        except struct.error:
            break
        ptr += 8
        if ptr + tlv_len > len(frame_data):
            break
        if tlv_type == VITALS_TLV_TYPE:
            tlv_payload = frame_data[ptr:ptr + tlv_len]
            vitals = parse_vitals_tlv(tlv_payload)
            raw_fields = parse_all_vitals_fields(tlv_payload)
        ptr += tlv_len

    return vitals, raw_fields


def find_magic_word(ser):
    magic_index = 0
    while True:
        byte = ser.read(1)
        if not byte:
            return False
        if byte[0] == MAGIC_WORD[magic_index]:
            magic_index += 1
            if magic_index == len(MAGIC_WORD):
                return True
        else:
            magic_index = 0


def listen(data_port_name, verbose):
    print(f"[DATA] Connecting to {data_port_name} at {DATA_BAUD_RATE} baud...")
    ser = serial.Serial(data_port_name, DATA_BAUD_RATE, timeout=1)
    print(f"[DATA] Listening...\n")

    packet_num = 0
    last_time = None

    if verbose:
        print(f"{'#':>5s}  {'dt_ms':>7s}  {'ID':>4s}  {'RngBin':>6s}  "
              f"{'HeartRate':>9s}  {'BreathRate':>10s}  {'BreathDev':>10s}  "
              f"{'AllFields'}")
    else:
        print(f"{'#':>5s}  {'dt_ms':>7s}  {'ID':>4s}  {'RngBin':>6s}  "
              f"{'HeartRate':>9s}  {'BreathRate':>10s}  {'BreathDev':>10s}")
    print("-" * 80)

    try:
        while True:
            if not find_magic_word(ser):
                continue

            header_bytes = ser.read(8)
            if len(header_bytes) < 8:
                continue
            version, total_packet_len = struct.unpack('<II', header_bytes)

            bytes_to_read = total_packet_len - 16
            if bytes_to_read <= 0:
                continue

            frame_data = ser.read(bytes_to_read)
            if len(frame_data) < bytes_to_read:
                continue

            vitals, raw_fields = parse_frame(frame_data)

            if vitals:
                now = time.time()
                packet_num += 1

                dt_ms = (now - last_time) * 1000 if last_time else 0.0
                last_time = now

                line = (f"{packet_num:5d}  {dt_ms:7.1f}  "
                        f"{vitals['id']:4d}  {vitals['rangeBin']:6d}  "
                        f"{vitals['heart_rate']:9.2f}  "
                        f"{vitals['breath_rate']:10.2f}  "
                        f"{vitals['breathDeviation']:10.4f}")

                if verbose and raw_fields:
                    line += "  " + str(list(raw_fields))

                print(line)

    except KeyboardInterrupt:
        print(f"\n[DATA] Stopped. {packet_num} packets received.")
    finally:
        ser.close()
        print("[DATA] Port closed.")


def main():
    parser = argparse.ArgumentParser(description="Raw vital-sign sensor dump (no state mapping)")
    parser.add_argument('--cli-port', default='COM3', help='CLI COM port (default: COM3)')
    parser.add_argument('--data-port', default='COM4', help='Data COM port (default: COM4)')
    parser.add_argument('--config-file', required=True, help='Path to .cfg config file')
    parser.add_argument('--verbose', '-v', action='store_true',
                        help='Show all 35 unpacked TLV fields per packet')
    args = parser.parse_args()

    print("=" * 80)
    print("  RAW SENSOR DUMP")
    print(f"  {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    print(f"  CLI: {args.cli_port}  DATA: {args.data_port}")
    print(f"  Config: {args.config_file}")
    print("=" * 80)

    send_config(args.cli_port, args.config_file)

    print("Waiting 2 seconds for sensor to start streaming...\n")
    time.sleep(2)

    listen(args.data_port, args.verbose)


if __name__ == "__main__":
    main()
