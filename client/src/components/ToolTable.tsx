import React, { useEffect, useMemo, useRef, useState } from "react";
import axios from "axios";
import Swal from "sweetalert2";
import {
  useReactTable,
  getCoreRowModel,
  getPaginationRowModel,
  flexRender,
} from "@tanstack/react-table";
import type { ColumnDef } from "@tanstack/react-table";
import { FiEdit2, FiTrash2 } from "react-icons/fi";
import Papa from "papaparse";

import "sweetalert2/src/sweetalert2.scss";

interface ToolRow {
  tool_id: number;
  tool_name: string;
}

const ToolTable: React.FC = () => {
  const [data, setData] = useState<ToolRow[]>([]);
  const [pagination, setPagination] = useState({ pageIndex: 0, pageSize: 15 });
  const [search, setSearch] = useState("");
  const debounceTimeout = useRef<NodeJS.Timeout | null>(null);

  const fetchTools = () => {
    const query = new URLSearchParams();
    if (search.trim()) query.append("search", search.trim());

    fetch(`/api/Tools?${query.toString()}`)
      .then((res) => res.json())
      .then(setData)
      .catch((err) => console.error("Error fetching tools:", err));
  };

  useEffect(() => {
    if (debounceTimeout.current) clearTimeout(debounceTimeout.current);
    debounceTimeout.current = setTimeout(() => {
      fetchTools();
    }, 300);
  }, [search]);

  const [userRole, setUserRole] = useState<string | null>(null);

  useEffect(() => {
    const token = localStorage.getItem("token");
    if (token) {
      try {
        const payloadBase64 = token.split(".")[1];
        const payloadJson = atob(payloadBase64);
        const payload = JSON.parse(payloadJson);
        console.log("✅ ROLE IN PAYLOAD:", payload.role); // 👈 เพิ่มตรงนี้
        setUserRole(payload.role ?? null);
      } catch (error) {
        console.error("Invalid token format", error);
      }
    }
  }, []);

  // const handleAddTool = async (toolName: string) => {
  //   if (!toolName.trim()) return;

  //   const token = localStorage.getItem("token");
  //   console.log("TOKEN:", token);

  //   if (token) {
  //     // แยก payload จาก JWT
  //     const payloadBase64 = token.split(".")[1];
  //     const payloadJson = atob(payloadBase64);
  //     const payload = JSON.parse(payloadJson);
  //     console.log("Decoded Payload:", payload);

  //     // ✅ log เฉพาะ role
  //     console.log("ROLE FROM TOKEN:", payload.role);
  //   }

  //   try {
  //     await axios.post(
  //       "/api/Tools",
  //       {
  //         tool_name: toolName,
  //       },
  //       {
  //         headers: {
  //           Authorization: Bearer ${token},
  //         },
  //       }
  //     );
  //     setSearch("");
  //     fetchTools();
  //     Swal.fire({
  //       icon: "success",
  //       title: "Success",
  //       text: "Tool added successfully",
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
  //       },
  //     });
  //   } catch (err) {
  //     Swal.fire("Error", "Failed to add tool", "error");
  //   }
  // };

  const handleAddTool = async (toolName: string) => {
    if (!toolName.trim()) return;

    const token = localStorage.getItem("token");
    console.log("TOKEN:", token);

    if (token) {
      // แยก payload จาก JWT
      const payloadBase64 = token.split(".")[1];
      const payloadJson = atob(payloadBase64);
      const payload = JSON.parse(payloadJson);
      console.log("Decoded Payload:", payload);

      console.log("ROLE FROM TOKEN:", payload.role);
    }

    try {
      const url = "/api/Requests";

      const body = {
        request_type: "INSERT",
        target_table: "tools",
        target_pk_id: null,
        old_data: null,
        new_data: {
          tool_name: toolName,
        },
        note: "Created via frontend",
      };

      const request = new Request(url, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(body),
      });

      const response = await fetch(request);

      if (!response.ok) {
        const errorData = await response.json();
        console.error("Error:", errorData);
        Swal.fire(
          "Error",
          errorData.message || "Failed to submit request.",
          "error"
        );
        return;
      }

      const result = await response.json();

      Swal.fire({
        icon: "success",
        title: "Request Submitted",
        text: `Your request to add a tool has been submitted. It will be effective after admin approval.`,
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
        },
      });

      setSearch("");
      fetchTools();
    } catch (err) {
      console.error(err);
      Swal.fire("Error", "Failed to submit request.", "error");
    }
  };

  // const handleEditTool = async (tool: ToolRow) => {
  //   const { value: newName } = await Swal.fire({
  //     title: "Edit Tool Name",
  //     input: "text",
  //     inputValue: tool.tool_name,
  //     inputLabel: "Tool Name",
  //     showCancelButton: true,
  //     confirmButtonText: "Update",
  //     cancelButtonText: "Cancel",
  //     customClass: {
  //       popup: "text-sm",
  //       input:
  //         "text-sm px-3 py-2 border border-blue-300 rounded focus:ring focus:ring-blue-300 focus:outline-none",
  //       confirmButton:
  //         "bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded",
  //       cancelButton:
  //         "bg-gray-200 hover:bg-gray-300 text-black px-4 py-2 rounded",
  //     },
  //     preConfirm: async (value) => {
  //       const trimmed = value.trim();
  //       if (!trimmed) {
  //         Swal.showValidationMessage("Tool name cannot be empty");
  //         return false;
  //       }
  //       if (trimmed.toLowerCase() === tool.tool_name.toLowerCase()) {
  //         // ไม่ต้องเปลี่ยน ถ้าเหมือนเดิม
  //         return false;
  //       }
  //       try {
  //         const res = await axios.get(
  //           `/api/Tools?search=${encodeURIComponent(
  //             trimmed
  //           )}`
  //         );
  //         if (
  //           Array.isArray(res.data) &&
  //           res.data.some(
  //             (t: any) => t.tool_name.toLowerCase() === trimmed.toLowerCase()
  //           )
  //         ) {
  //           Swal.showValidationMessage("This tool name already exists");
  //           return false;
  //         }
  //       } catch {
  //         Swal.showValidationMessage("Error checking for duplicates");
  //         return false;
  //       }
  //       return trimmed;
  //     },
  //   });

  //   if (newName && newName.trim() !== tool.tool_name) {
  //     try {
  //       await axios.put(
  //         `/api/Tools/${tool.tool_id}`,
  //         {
  //           tool_id: tool.tool_id,
  //           tool_name: newName.trim(),
  //         },
  //         {
  //           headers: {
  //             Authorization: `Bearer ${localStorage.getItem("token")}`,
  //           },
  //         }
  //       );
  //       fetchTools();
  //       Swal.fire({
  //         icon: "success",
  //         title: "Success",
  //         text: "Tool updated successfully",
  //         confirmButtonText: "OK",
  //         customClass: {
  //           confirmButton:
  //             "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
  //         },
  //       });
  //     } catch {
  //       Swal.fire("Error", "Failed to update tool", "error");
  //     }
  //   }
  // };

  const handleEditTool = async (tool: ToolRow) => {
    const { value: newName } = await Swal.fire({
      title: "Edit Tool Name",
      input: "text",
      inputValue: tool.tool_name,
      inputLabel: "Tool Name",
      showCancelButton: true,
      confirmButtonText: "Update",
      cancelButtonText: "Cancel",
      customClass: {
        popup: "text-sm",
        input:
          "text-sm px-3 py-2 border border-blue-300 rounded focus:ring focus:ring-blue-300 focus:outline-none",
        confirmButton:
          "bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded",
        cancelButton:
          "bg-gray-200 hover:bg-gray-300 text-black px-4 py-2 rounded",
      },
      preConfirm: async (value) => {
        const trimmed = value.trim();
        if (!trimmed) {
          Swal.showValidationMessage("Tool name cannot be empty");
          return false;
        }
        if (trimmed.toLowerCase() === tool.tool_name.toLowerCase()) {
          // ไม่ต้องเปลี่ยน ถ้าเหมือนเดิม
          return false;
        }
        try {
          // ✅ เช็ค duplicate ผ่าน DynamicService โดย query api/Tools
          const res = await axios.get(
            `/api/Tools?search=${encodeURIComponent(
              trimmed
            )}`
          );
          if (
            Array.isArray(res.data) &&
            res.data.some(
              (t: any) =>
                t.tool_name.toLowerCase() === trimmed.toLowerCase() &&
                t.tool_id !== tool.tool_id
            )
          ) {
            Swal.showValidationMessage("This tool name already exists");
            return false;
          }
        } catch {
          Swal.showValidationMessage("Error checking for duplicates");
          return false;
        }
        return trimmed;
      },
    });

    if (newName && newName.trim() !== tool.tool_name) {
      try {
        const token = localStorage.getItem("token");

        const requestBody = {
          request_type: "UPDATE",
          target_table: "tools",
          target_pk_id: tool.tool_id,
          old_data: {
            tool_id: tool.tool_id,
            tool_name: tool.tool_name,
          },
          new_data: {
            tool_id: tool.tool_id,
            tool_name: newName.trim(),
          },
          note: "Edited from frontend",
        };

        const url = "/api/Requests";

        const request = new Request(url, {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
          },
          body: JSON.stringify(requestBody),
        });

        const response = await fetch(request);

        if (!response.ok) {
          const errorData = await response.json();
          console.error("Error:", errorData);
          Swal.fire(
            "Error",
            errorData.message || "Failed to submit update request.",
            "error"
          );
          return;
        }

        const result = await response.json();

        Swal.fire({
          icon: "success",
          title: "Request Submitted",
          text: `Update request submitted successfully.`,
          confirmButtonText: "OK",
          customClass: {
            confirmButton:
              "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
          },
        });

        fetchTools();
      } catch (err) {
        console.error(err);
        Swal.fire("Error", "Failed to submit update request.", "error");
      }
    }
  };

  // const handleDelete = async (toolId?: number) => {
  //   if (!toolId) return;
  //   const result = await Swal.fire({
  //     title: "Are you sure?",
  //     text: "Do you really want to delete this tool?",
  //     icon: "warning",
  //     showCancelButton: true,
  //     confirmButtonText: "Yes, delete it!",
  //     cancelButtonText: "Cancel",
  //     customClass: {
  //       popup: "text-sm",
  //       confirmButton:
  //         "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
  //       cancelButton:
  //         "bg-gray-200 hover:bg-gray-300 text-black px-4 py-2 rounded",
  //     },
  //   });
  //   if (result.isConfirmed) {
  //     const res = await fetch(`/api/Tools/${toolId}`, {
  //       method: "DELETE",
  //       headers: {
  //         Authorization: `Bearer ${localStorage.getItem("token")}`,
  //       },
  //     });

  //     if (res.ok) {
  //       fetchTools();
  //       Swal.fire({
  //         icon: "success",
  //         title: "Deleted!",
  //         text: "Tool has been deleted.",
  //         confirmButtonText: "OK",
  //         customClass: {
  //           confirmButton:
  //             "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
  //         },
  //       });
  //     }
  //   }
  // };

  const handleDelete = async (toolId?: number) => {
    if (!toolId) return;

    const result = await Swal.fire({
      title: "Are you sure?",
      text: "Do you really want to delete this tool?",
      icon: "warning",
      showCancelButton: true,
      confirmButtonText: "Yes, delete it!",
      cancelButtonText: "Cancel",
      customClass: {
        popup: "text-sm",
        confirmButton:
          "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
        cancelButton:
          "bg-gray-200 hover:bg-gray-300 text-black px-4 py-2 rounded",
      },
    });

    if (!result.isConfirmed) return;

    try {
      const token = localStorage.getItem("token");

      // ✅ Load old data for snapshot
      const oldDataRes = await fetch(
        `/api/Tools/${toolId}`,
        {
          method: "GET",
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      if (!oldDataRes.ok) {
        Swal.fire(
          "Error",
          "Cannot load tool data for delete request.",
          "error"
        );
        return;
      }

      const oldData = await oldDataRes.json();

      const requestBody = {
        request_type: "DELETE",
        target_table: "tools",
        target_pk_id: toolId,
        old_data: oldData,
        new_data: null,
        note: "Deleted from frontend",
      };

      const url = "/api/Requests";

      const request = new Request(url, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(requestBody),
      });

      const response = await fetch(request);

      if (!response.ok) {
        const errorData = await response.json();
        console.error("Error:", errorData);
        Swal.fire(
          "Error",
          errorData.message || "Failed to submit delete request.",
          "error"
        );
        return;
      }

      const resultData = await response.json();

      Swal.fire({
        icon: "success",
        title: "Request Submitted",
        text: `Delete request submitted successfully. Request ID: ${resultData.request_id}`,
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
        },
      });

      fetchTools();
    } catch (err) {
      console.error(err);
      Swal.fire("Error", "Failed to submit delete request.", "error");
    }
  };

  const handleExportCSV = () => {
    const csv = Papa.unparse(data);
    const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.setAttribute("download", "tools.csv");
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const columns = useMemo<ColumnDef<ToolRow, any>[]>(
    () => [
      {
        header: "#",
        cell: ({ row }) => row.index + 1,
      },
      { accessorKey: "tool_name", header: "Tool Name" },
      // {
      //   header: "Actions",
      //   cell: ({ row }) => (
      //     <div className="flex justify-center space-x-2">
      //       <button
      //         onClick={() => handleEditTool(row.original)}
      //         className="text-blue-500 hover:text-blue-700"
      //       >
      //         <FiEdit2 />
      //       </button>
      //       <button
      //         onClick={() => handleDelete(row.original.tool_id)}
      //         className="text-red-500 hover:text-red-700"
      //       >
      //         <FiTrash2 />
      //       </button>
      //     </div>
      //   ),
      // },
      {
        header: "Actions",
        cell: ({ row }) => {
          const canEdit = ["admin", "editor"].includes(userRole || "");

          return canEdit ? (
            <div className="flex justify-center space-x-2">
              <button
                onClick={() => handleEditTool(row.original)}
                className="text-blue-500 hover:text-blue-700"
              >
                <FiEdit2 />
              </button>
              <button
                onClick={() => handleDelete(row.original.tool_id)}
                className="text-red-500 hover:text-red-700"
              >
                <FiTrash2 />
              </button>
            </div>
          ) : null;
        },
      },
    ],
    [userRole]
  );

  const table = useReactTable({
    data,
    columns,
    state: { pagination },
    onPaginationChange: setPagination,
    getCoreRowModel: getCoreRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
  });

  return (
    <div className="space-y-4 relative">
      {/* ✅ Banner เตือนผู้ใช้ว่ากำลังใช้ระบบ Request */}
      <div className="bg-yellow-100 border border-yellow-300 text-yellow-800 text-sm p-2 rounded mb-2 mx-4">
        <strong>Note:</strong> Changes to tools will be pending until approved
        by an admin.
      </div>
      <div className="flex justify-between px-4 items-center">
        <div className="flex space-x-2">
          <button
            className="bg-blue-600 hover:bg-blue-700 text-white text-sm px-4 py-1 rounded shadow"
            onClick={() => {
              Swal.fire({
                title: "Add New Tool",
                input: "text",
                inputLabel: "Tool Name",
                showCancelButton: true,
                confirmButtonText: "Add",
                confirmButtonColor: "#2563eb",
                cancelButtonColor: "#d1d5db",
                customClass: {
                  popup: "text-sm",
                  input:
                    "text-sm px-3 py-2 border border-blue-300 rounded focus:ring focus:ring-blue-300 focus:outline-none",
                  confirmButton:
                    "bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded",
                  cancelButton: "text-black px-4 py-2 rounded",
                },
                preConfirm: async (value) => {
                  const trimmed = value.trim();
                  if (!trimmed) {
                    Swal.showValidationMessage("Tool name cannot be empty");
                    return false;
                  }
                  try {
                    const res = await axios.get(
                      `/api/Tools?search=${encodeURIComponent(
                        trimmed
                      )}`
                    );
                    if (
                      Array.isArray(res.data) &&
                      res.data.some(
                        (t: any) =>
                          t.tool_name.toLowerCase() === trimmed.toLowerCase()
                      )
                    ) {
                      Swal.showValidationMessage(
                        "This tool name already exists"
                      );
                      return false;
                    }
                  } catch (err) {
                    Swal.showValidationMessage("Error checking for duplicates");
                    return false;
                  }
                  return trimmed;
                },
              }).then((result) => {
                if (result.isConfirmed && result.value) {
                  handleAddTool(result.value);
                }
              });
            }}
          >
            + Add Tool
          </button>

          <button
            className="border border-blue-600 text-blue-600 text-sm px-4 py-1 rounded shadow hover:bg-blue-50"
            onClick={handleExportCSV}
          >
            Export CSV
          </button>
        </div>

        <input
          type="text"
          placeholder="Search tool..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="border rounded px-3 py-1 text-sm shadow"
        />
      </div>

      <div className="overflow-auto max-h-[70vh] border rounded-lg">
        <table className="min-w-full text-sm text-left">
          <thead className="bg-gray-100 border-b">
            {table.getHeaderGroups().map((headerGroup) => (
              <tr key={headerGroup.id}>
                {headerGroup.headers.map((header) => (
                  <th
                    key={header.id}
                    className="px-2 py-2 border text-xs font-semibold text-gray-700 whitespace-nowrap text-center"
                  >
                    {flexRender(
                      header.column.columnDef.header,
                      header.getContext()
                    )}
                  </th>
                ))}
              </tr>
            ))}
          </thead>
          <tbody>
            {table.getRowModel().rows.map((row) => (
              <tr
                key={row.id}
                className="hover:bg-blue-50 transition-colors duration-100 even:bg-gray-50"
              >
                {row.getVisibleCells().map((cell) => (
                  <td
                    key={cell.id}
                    className="px-2 py-2 border text-sm text-gray-800 whitespace-nowrap text-left"
                  >
                    {flexRender(cell.column.columnDef.cell, cell.getContext())}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="flex justify-between items-center px-4 py-2 border-t bg-white">
        <div className="flex items-center text-sm">
          <span className="mr-2">Results per page:</span>
          <select
            value={table.getState().pagination.pageSize}
            onChange={(e) => table.setPageSize(Number(e.target.value))}
            className="border rounded px-2 py-1 text-sm"
          >
            {[5, 10, 15, 25, 50, 100].map((size) => (
              <option key={size} value={size}>
                {size}
              </option>
            ))}
          </select>
        </div>

        <div className="text-sm">
          {(() => {
            const { pageIndex, pageSize } = table.getState().pagination;
            const total = data.length;
            const start = pageIndex * pageSize + 1;
            const end = Math.min(start + pageSize - 1, total);
            return `${start} – ${end} of ${total}`;
          })()}
        </div>

        <div className="flex items-center space-x-1 text-sm">
          <button
            onClick={() => table.setPageIndex(0)}
            disabled={!table.getCanPreviousPage()}
            className="px-2 py-1 border rounded disabled:opacity-40"
          >
            ⏮
          </button>
          <button
            onClick={() => table.previousPage()}
            disabled={!table.getCanPreviousPage()}
            className="px-2 py-1 border rounded disabled:opacity-40"
          >
            ◀
          </button>
          <button
            onClick={() => table.nextPage()}
            disabled={!table.getCanNextPage()}
            className="px-2 py-1 border rounded disabled:opacity-40"
          >
            ▶
          </button>
          <button
            onClick={() => table.setPageIndex(table.getPageCount() - 1)}
            disabled={!table.getCanNextPage()}
            className="px-2 py-1 border rounded disabled:opacity-40"
          >
            ⏭
          </button>
        </div>
      </div>
    </div>
  );
};

export default ToolTable;
