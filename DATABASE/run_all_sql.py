import os
import subprocess
import sys

if sys.stdout.encoding.lower() != 'utf-8':
    sys.stdout.reconfigure(encoding='utf-8')


scripts = [
    r"DATABASE\load_do_to_db.sql",
    r"FUNCTION\query_DC.sql",
    r"FUNCTION\query_AM.sql",
    r"FUNCTION\query_KT.sql",
    r"FUNCTION\query_QL.sql",
    r"FUNCTION\query_LT.sql",
    r"FUNCTION\query_TN.sql",
    r"FUNCTION\query_KH.sql",
    r"DATABASE\Refactor_Fix_Concurrency.sql"
]

base_dir = r"d:\Dai_Hoc\Viet_Sport_refactor"
server = "."
database = "VIET_SPORT"

print(f"=== BẮT ĐẦU THỰC THI TẤT CẢ CÁC FILE SQL TRÊN {database} ===")

failed = False
for s in scripts:
    full_path = os.path.join(base_dir, s)
    print(f"\n[EXEC] Đang chạy: {s} ...")
    cmd = ["sqlcmd", "-S", server, "-E", "-d", database, "-i", full_path, "-b", "-f", "65001"]
    res = subprocess.run(cmd, capture_output=True, text=True, encoding="utf-8", errors="replace")
    if res.returncode != 0:
        print(f"[-] LỖI KHI CHẠY: {s}")
        print(res.stderr if res.stderr else res.stdout)
        failed = True
        break
    else:
        print(f"[+] THÀNH CÔNG: {s}")
        if res.stdout and res.stdout.strip():
            lines = [line for line in res.stdout.strip().splitlines() if not line.startswith("Changed database context")]
            if lines:
                print("    " + "\n    ".join(lines[:5]))

if not failed:
    print("\n========================================================")
    print(">>> TẤT CẢ CÁC FILE SQL ĐÃ ĐƯỢC CHẠY THÀNH CÔNG 100%! <<<")
    print("========================================================")
else:
    sys.exit(1)
