import serial
import struct
import time
import sys
import csv
import os
import socket
import argparse
from datetime import datetime

# --- SENSOR & PARSING CONSTANTS ---

CLI_BAUD_RATE = 115200
DATA_BAUD_RATE = 921600

MAGIC_WORD = b'\x02\x01\x04\x03\x06\x05\x08\x07'
VITALS_TLV_TYPE = 1040

VITALS_STRUCT_FORMAT = '<2H33f'
VITALS_STRUCT_SIZE = struct.calcsize(VITALS_STRUCT_FORMAT)

# --- State Logic Parameters ---
BREATH_DEVIATION_THRESHOLD = 0.02
ALERT_BREATH_RATE_THRESHOLD = 23
MAX_GRACE_PACKETS = 1


# --- FUNCTIONS ---

def emit(sock, prefix, message):
    """Send a prefixed line over the TCP socket so C# reads it immediately."""
    line = f"{prefix}|{message}\n"
    try:
        sock.sendall(line.encode('utf-8'))
    except Exception:
        # Also print to console as fallback for debugging
        print(f"{prefix}|{message}", flush=True)


def send_config(sock, cli_port_name, config_file_path):
    emit(sock, "STATUS", f"Opening config file: {config_file_path}")

    try:
        with open(config_file_path, 'r') as f:
            config_lines = f.readlines()
    except FileNotFoundError:
        emit(sock, "ERROR", f"Config file not found: {config_file_path}")
        return False
    except Exception as e:
        emit(sock, "ERROR", f"Could not read config file: {e}")
        return False

    emit(sock, "STATUS", f"Config file loaded ({len(config_lines)} lines).")

    try:
        cli_ser = serial.Serial(cli_port_name, CLI_BAUD_RATE, timeout=1)
    except serial.SerialException as e:
        emit(sock, "ERROR", f"Could not open CLI port {cli_port_name}: {e}")
        return False

    emit(sock, "STATUS", f"Connected to CLI port {cli_port_name}. Sending config...")

    for line in config_lines:
        line = line.strip()
        if not line or line.startswith('%'):
            continue

        emit(sock, "STATUS", f"Sending: {line}")
        line_bytes = (line + '\n').encode('utf-8')
        try:
            cli_ser.write(line_bytes)
            time.sleep(0.03)
            ack1 = cli_ser.readline()
            ack2 = cli_ser.readline()
        except serial.SerialException as e:
            emit(sock, "ERROR", f"Failed during write to CLI port: {e}")
            cli_ser.close()
            return False

    has_sensor_start = 'sensorStart' in line
    emit(sock, "STATUS", f"All config lines sent. Last command: '{line}'")

    # Give the sensor time to process sensorStart before closing the CLI port
    time.sleep(0.5)
    cli_ser.close()
    emit(sock, "STATUS", "CLI port closed.")

    if has_sensor_start:
        emit(sock, "STATUS", "sensorStart sent. Data should be streaming.")
        return True
    else:
        emit(sock, "ERROR", "sensorStart was not the last command in the config.")
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


def listen_for_vitals(sock, data_port_name, csv_writer):
    emit(sock, "STATUS", f"Connecting to DATA port {data_port_name} at {DATA_BAUD_RATE} baud...")

    try:
        ser = serial.Serial(data_port_name, DATA_BAUD_RATE, timeout=1)
    except serial.SerialException as e:
        emit(sock, "ERROR", f"Could not open DATA port {data_port_name}: {e}")
        return

    emit(sock, "STATUS", f"Listening on {data_port_name}.")

    packet_counter = 0
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
                    packet_counter += 1

                    raw_heart_rate = vitals['heart_rate']
                    raw_breath_rate = vitals['breath_rate']
                    breath_dev = vitals['breathDeviation']

                    # Add 30bpm offset to heart rate
                    raw_heart_rate += 30.0

                    # --- State Logic ---
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

                    # Send vitals over TCP
                    emit(sock, "VITALS", f"{raw_heart_rate:.2f}|{raw_breath_rate:.2f}|{breath_dev:.4f}|{current_status}")

                    # Write to CSV log
                    csv_writer.writerow([
                        datetime.now().isoformat(),
                        packet_counter,
                        f"{raw_heart_rate:.2f}",
                        f"{raw_breath_rate:.2f}",
                        f"{breath_dev:.4f}",
                        current_status
                    ])

    except KeyboardInterrupt:
        emit(sock, "STATUS", "Stopping script...")
    except Exception as e:
        emit(sock, "ERROR", f"Unexpected error: {e}")
    finally:
        ser.close()
        emit(sock, "STATUS", "Serial port closed.")


def main():
    parser = argparse.ArgumentParser(description="Breathing state sensor reader")
    parser.add_argument('--cli-port', required=True, help='CLI COM port (e.g. COM3)')
    parser.add_argument('--data-port', required=True, help='Data COM port (e.g. COM4)')
    parser.add_argument('--config-file', required=True, help='Path to .cfg config file')
    parser.add_argument('--log-dir', default='logs', help='Directory for CSV log files')
    parser.add_argument('--tcp-port', type=int, required=True, help='TCP port to connect to for sending data to C# app')
    args = parser.parse_args()

    # Connect to the C# TCP listener
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)
    try:
        sock.connect(('127.0.0.1', args.tcp_port))
    except Exception as e:
        print(f"ERROR|Could not connect to C# app on port {args.tcp_port}: {e}", flush=True)
        sys.exit(1)

    emit(sock, "STATUS", "Python connected to C# app via TCP.")

    # Create log directory and open CSV file
    os.makedirs(args.log_dir, exist_ok=True)
    log_filename = f"session_{datetime.now().strftime('%Y%m%d_%H%M%S')}.csv"
    log_path = os.path.join(args.log_dir, log_filename)

    csv_file = open(log_path, 'w', newline='', encoding='utf-8', buffering=1)
    csv_writer = csv.writer(csv_file)
    csv_writer.writerow(['Timestamp', 'PacketNumber', 'HeartRate', 'BreathRate', 'BreathDeviation', 'State'])

    emit(sock, "STATUS", f"Logging to {log_path}")

    try:
        config_success = send_config(sock, args.cli_port, args.config_file)
        if not config_success:
            emit(sock, "ERROR", "Config send failed. Exiting.")
            sys.exit(1)

        emit(sock, "STATUS", "Config sent. Starting data listener in 2 seconds...")
        time.sleep(2)

        listen_for_vitals(sock, args.data_port, csv_writer)
    finally:
        csv_file.close()
        sock.close()


if __name__ == "__main__":
    main()
