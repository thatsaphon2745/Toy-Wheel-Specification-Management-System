import React, { useEffect, useState } from "react";
import axios from "axios";

export interface Log {
  log_id: number;
  user_id?: number;
  username_snapshot?: string;
  action: string;
  target_table?: string;
  target_id?: string;
  old_data?: string;
  new_data?: string;
  created_at: string;
}

const LogTable: React.FC = () => {
  const [logs, setLogs] = useState<Log[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  // Pagination states
  const [page, setPage] = useState<number>(1);
  const pageSize = 10;

  // Modal state
  const [modalLog, setModalLog] = useState<Log | null>(null);

  useEffect(() => {
    const fetchLogs = async () => {
      try {
        const res = await axios.get<Log[]>("/api/Logs");
        setLogs(res.data);
        setLoading(false);
      } catch (err) {
        console.error(err);
        setError("Failed to load logs.");
        setLoading(false);
      }
    };

    fetchLogs();
  }, []);

  const pagedLogs = logs.slice((page - 1) * pageSize, page * pageSize);

  const handleViewDetails = (log: Log) => {
    setModalLog(log);
  };

  if (loading) {
    return <div className="p-4">Loading logs...</div>;
  }

  if (error) {
    return <div className="p-4 text-red-600">{error}</div>;
  }

  /**
   * ✅ ฟังก์ชันใหม่สำหรับ log model ปัจจุบัน:
   * - แสดงทุก field
   * - ถ้า field ไม่อยู่ใน new_data → ดึง old value มาใส่ new value
   * - highlight เฉพาะ field ที่เปลี่ยน
   */
  const getDiffRows = (
    oldStr?: string,
    newStr?: string,
    action?: string
  ) => {
    let oldObj: any = {};
    let newObj: any = {};

    try {
      if (oldStr) {
        oldObj = JSON.parse(oldStr);
      }
      if (newStr) {
        newObj = JSON.parse(newStr);
      }
    } catch (e) {
      console.error("Invalid JSON:", e);
      return [];
    }

    // รวมทุก key
    const allKeys = Array.from(
      new Set([...Object.keys(oldObj), ...Object.keys(newObj)])
    );

    return allKeys.map((key) => {
      const oldVal = oldObj[key];

      let newVal;
      if (newObj.hasOwnProperty(key)) {
        newVal = newObj[key];
      } else {
        newVal = oldVal;
      }

      const changed = oldVal !== newVal;

      return {
        key,
        oldVal,
        newVal,
        changed,
      };
    });
  };

  return (
    <div className="p-4 overflow-auto">
      <h2 className="text-2xl font-bold mb-4">System Logs</h2>

      <table className="min-w-full text-xs border-collapse border border-gray-300">
        <thead className="bg-gray-100">
          <tr>
            <th className="border px-2 py-2">#</th>
            <th className="border px-2 py-2">Date</th>
            <th className="border px-2 py-2">Time</th>
            <th className="border px-2 py-2">User</th>
            <th className="border px-2 py-2">Action</th>
            <th className="border px-2 py-2">Table</th>
            <th className="border px-2 py-2">Target ID</th>
            <th className="border px-2 py-2">Details</th>
          </tr>
        </thead>
        <tbody>
          {pagedLogs.map((log) => {
            const dateObj = new Date(log.created_at);
            const dateStr = dateObj.toLocaleDateString();
            const timeStr = dateObj.toLocaleTimeString("en-US", {
              hour12: false,
            });

            return (
              <tr key={log.log_id} className="hover:bg-gray-50">
                <td className="border px-2 py-2 text-center">{log.log_id}</td>
                <td className="border px-2 py-2 text-center">{dateStr}</td>
                <td className="border px-2 py-2 text-center">{timeStr}</td>
                <td className="border px-2 py-2">
                  {log.username_snapshot || "-"}
                </td>
                <td className="border px-2 py-2">{log.action}</td>
                <td className="border px-2 py-2">{log.target_table}</td>
                <td className="border px-2 py-2">{log.target_id}</td>
                <td className="border px-2 py-2 text-center">
                  <button
                    onClick={() => handleViewDetails(log)}
                    className="
                      inline-flex items-center gap-1
                      border border-gray-300
                      text-gray-700
                      hover:bg-gray-100
                      px-2 py-1
                      rounded
                      text-xs
                      transition-colors
                    "
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      className="h-3 w-3 text-gray-500"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                      strokeWidth={2}
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M15 12H9m12 0a9 9 0 11-18 0 9 9 0 0118 0z"
                      />
                    </svg>
                    View Details
                  </button>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>

      {/* Pagination */}
      <div className="mt-4 flex justify-between items-center">
        <span className="text-sm">
          Showing {(page - 1) * pageSize + 1} -{" "}
          {Math.min(page * pageSize, logs.length)} of {logs.length}
        </span>

        <div className="space-x-2">
          <button
            className="px-3 py-1 border rounded text-sm"
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
          >
            Prev
          </button>
          <button
            className="px-3 py-1 border rounded text-sm"
            onClick={() =>
              setPage((p) => (p * pageSize < logs.length ? p + 1 : p))
            }
            disabled={page * pageSize >= logs.length}
          >
            Next
          </button>
        </div>
      </div>

      {/* Modal */}
      {modalLog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center z-50">
          <div className="bg-white p-6 rounded max-w-3xl w-full overflow-auto max-h-[80vh]">
            <h3 className="text-xl font-bold mb-4">Log Details</h3>

            <table className="min-w-full text-xs border-collapse border border-gray-300">
              <thead className="bg-gray-100">
                <tr>
                  <th className="border px-2 py-2">Field</th>
                  <th className="border px-2 py-2">Old Value</th>
                  <th className="border px-2 py-2">New Value</th>
                </tr>
              </thead>
              <tbody>
                {getDiffRows(
                  modalLog.old_data,
                  modalLog.new_data,
                  modalLog.action
                ).map((row) => (
                  <tr
                    key={row.key}
                    className={row.changed ? "bg-yellow-100" : ""}
                  >
                    <td className="border px-2 py-1 font-semibold">
                      {row.key}
                    </td>
                    <td className="border px-2 py-1">
                      {row.oldVal !== undefined ? String(row.oldVal) : "-"}
                    </td>
                    <td className="border px-2 py-1">
                      {row.newVal !== undefined ? String(row.newVal) : "-"}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            <div className="text-right mt-4">
              <button
                className="px-3 py-1 text-sm border rounded bg-gray-100 hover:bg-gray-200"
                onClick={() => setModalLog(null)}
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default LogTable;
