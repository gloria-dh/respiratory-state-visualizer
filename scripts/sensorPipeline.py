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

VITALS_STRUCT_FORMAT = '<2H33f'
VITALS_STRUCT_SIZE = struct.calcsize(VITALS_STRUCT_FORMAT)

BREATH_DEVIATION_THRESHOLD = 0.02
ALERT_BREATH_RATE_THRESHOLD = 20
MAX_GRACE_PACKETS = 1


def emit(prefix, message):
    """Sends a prefixed line to stdout for the C# host to read."""
    print(f"{prefix}|{message}", flush=True)


def send_config(cli_port_name, config_file_path):
    emit("STATUS", f"Opening config file: {config_file_path}")

    try:
        with open(config_file_path, 'r') as f:
            config_lines = f.readlines()
    except FileNotFoundError:
        emit("ERROR", f"Config file not found: {config_file_path}")
        return False
    except Exception as e:
        emit("ERROR", f"Could not read config file: {e}")
        return False

    emit("STATUS", f"Config file loaded ({len(config_lines)} lines).")

    try:
        cli_ser = serial.Serial(cli_port_name, CLI_BAUD_RATE, timeout=1)
    except serial.SerialException as e:
        emit("ERROR", f"Could not open CLI port {cli_port_name}: {e}")
        return False

    emit("STATUS", f"Connected to CLI port {cli_port_name}. Sending config...")

    for line in config_lines:
        line = line.strip()
        if not line or line.startswith('%'):
            continue

        emit("STATUS", f"Sending: {line}")
        line_bytes = (line + '\n').encode('utf-8')
        try:
            cli_ser.write(line_bytes)
            time.sleep(0.03)
            ack1 = cli_ser.readline()
            ack2 = cli_ser.readline()
        except serial.SerialException as e:
            emit("ERROR", f"Failed during write to CLI port: {e}")
            cli_ser.close()
            return False

    has_sensor_start = 'sensorStart' in line
    emit("STATUS", f"All config lines sent. Last command: '{line}'")

    # Give the sensor time to process sensorStart before closing the CLI port
    time.sleep(0.5)
    cli_ser.close()
    emit("STATUS", "CLI port closed.")

    if has_sensor_start:
        emit("STATUS", "sensorStart sent. Data should be streaming.")
        return True
    else:
        emit("ERROR", "sensorStart was not the last command in the config.")
        return False


def parse_vitals_tlv(tlv_data):
    try:
        if len(tlv_data) < VITALS_STRUCT_SIZE:
            return None

        unpacked_data = struct.unpack('<2H3f', tlv_data[:struct.calcsize('<2H3f')])

        vitals = {
            'id': unpacked_data[0],
            'rangeBin': unpacked_data[1],
            'breathDeviation': unpacked_data[2],
            'heart_rate': unpacked_data[3],
            'breath_rate': unpacked_data[4]
        }
        return vitals
    except struct.error:
        return None


def parse_frame(frame_data):
    try:
        num_tlvs = struct.unpack('<I', frame_data[16:20])[0]
    except struct.error:
        return None

    ptr = 24
    for _ in range(num_tlvs):
        if ptr + 8 > len(frame_data):
            return None
        try:
            tlv_type, tlv_len = struct.unpack('<II', frame_data[ptr: ptr + 8])
        except struct.error:
            return None
        ptr += 8
        if ptr + tlv_len > len(frame_data):
            return None
        if tlv_type == VITALS_TLV_TYPE:
            tlv_data = frame_data[ptr: ptr + tlv_len]
            return parse_vitals_tlv(tlv_data)
        else:
            ptr += tlv_len
    return None


def find_magic_word(ser):
    magic_index = 0
    while True:
        try:
            byte = ser.read(1)
            if not byte:
                continue
            if byte[0] == MAGIC_WORD[magic_index]:
                magic_index += 1
                if magic_index == len(MAGIC_WORD):
                    return True
            else:
                magic_index = 0
        except Exception as e:
            return False


def listen_for_vitals(data_port_name):
    emit("STATUS", f"Connecting to DATA port {data_port_name} at {DATA_BAUD_RATE} baud...")

    try:
        ser = serial.Serial(data_port_name, DATA_BAUD_RATE, timeout=1)
    except serial.SerialException as e:
        emit("ERROR", f"Could not open DATA port {data_port_name}: {e}")
        return

    emit("STATUS", f"Listening on {data_port_name}.")

    previous_status = "Neutral"
    neutral_grace_counter = 0

    try:
        while True:
            if find_magic_word(ser):
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

                vitals = parse_frame(frame_data)

                if vitals:
                    raw_heart_rate = vitals['heart_rate']
                    raw_breath_rate = vitals['breath_rate']
                    breath_dev = vitals['breathDeviation']

                    # Add 30bpm offset to heart rate
                    # raw_heart_rate += 30.0

                    is_low = (breath_dev < BREATH_DEVIATION_THRESHOLD)
                    is_alert = (raw_breath_rate > ALERT_BREATH_RATE_THRESHOLD)

                    if is_alert:
                        current_status = "Alert"
                        neutral_grace_counter = 0
                    elif is_low:
                        if previous_status in ["Strained", "HoldingBreath", "Recovering"]:
                            current_status = "HoldingBreath"
                        else:
                            current_status = "Strained"
                        neutral_grace_counter = 0
                    else:
                        if previous_status == "HoldingBreath" and neutral_grace_counter < MAX_GRACE_PACKETS:
                            current_status = "Recovering"
                            neutral_grace_counter += 1
                        else:
                            current_status = "Neutral"
                            neutral_grace_counter = 0

                    if current_status == "HoldingBreath":
                        raw_breath_rate = 0.0

                    previous_status = current_status

                    # Send vitals to C# via stdout pipe
                    emit("VITALS", f"{raw_heart_rate:.2f}|{raw_breath_rate:.2f}|{breath_dev:.4f}|{current_status}")

    except KeyboardInterrupt:
        emit("STATUS", "Stopping script...")
    except Exception as e:
        emit("ERROR", f"Unexpected error: {e}")
    finally:
        ser.close()
        emit("STATUS", "Serial port closed.")


def main():
    parser = argparse.ArgumentParser(description="Breathing state sensor reader")
    parser.add_argument('--cli-port', required=True, help='CLI COM port (e.g. COM3)')
    parser.add_argument('--data-port', required=True, help='Data COM port (e.g. COM4)')
    parser.add_argument('--config-file', required=True, help='Path to .cfg config file')
    args = parser.parse_args()

    emit("STATUS", "Python script started.")

    try:
        config_success = send_config(args.cli_port, args.config_file)
        if not config_success:
            emit("ERROR", "Config send failed. Exiting.")
            sys.exit(1)

        emit("STATUS", "Config sent. Starting data listener in 1 seconds...")
        time.sleep(1)

        listen_for_vitals(args.data_port)
    except Exception as e:
        emit("ERROR", f"Fatal error: {e}")

if __name__ == "__main__":
    main()
