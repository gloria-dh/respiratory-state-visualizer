import os
import glob

LOGS_DIR = os.path.join(
    os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
    "Respiratory_State_Visualizer_V0", "bin", "Debug", "logs"
)


def wipe_logs():
    if not os.path.isdir(LOGS_DIR):
        print(f"Logs directory not found: {LOGS_DIR}")
        return

    csv_files = glob.glob(os.path.join(LOGS_DIR, "session_*.csv"))

    if not csv_files:
        print("No log files to delete.")
        return

    for f in csv_files:
        os.remove(f)
        print(f"Deleted: {os.path.basename(f)}")

    print(f"\n{len(csv_files)} log file(s) deleted.")


if __name__ == "__main__":
    wipe_logs()
