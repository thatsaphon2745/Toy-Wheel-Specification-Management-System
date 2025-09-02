// ✅ SizeRefTable.tsx แบบ production
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

export interface SizeRefRow {
  size_ref_id: number;
  size_ref: string;
}

const SizeRefTable: React.FC = () => {
  const [data, setData] = useState<SizeRefRow[]>([]);
  const [pagination, setPagination] = useState({ pageIndex: 0, pageSize: 15 });
  const [search, setSearch] = useState("");
  const debounceTimeout = useRef<NodeJS.Timeout | null>(null);

  const fetchSizeRefs = () => {
    const query = new URLSearchParams();
    if (search.trim()) query.append("search", search.trim());

    fetch(`/api/SizeRefs?${query.toString()}`)
      .then((res) => res.json())
      .then(setData)
      .catch((err) => console.error("Error fetching size refs:", err));
  };

  useEffect(() => {
    if (debounceTimeout.current) clearTimeout(debounceTimeout.current);
    debounceTimeout.current = setTimeout(() => {
      fetchSizeRefs();
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

  // const handleAddSizeRef = async (sizeRef: string) => {
  //   if (!sizeRef.trim()) return;

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
  //       "/api/SizeRefs",
  //       {
  //         size_ref: sizeRef,
  //       },
  //       {
  //         headers: {
  //           Authorization: `Bearer ${token}`,
  //         },
  //       }
  //     );

  //     setSearch("");
  //     fetchSizeRefs();
  //     Swal.fire({
  //       icon: "success",
  //       title: "Success",
  //       text: "SizeRef added successfully",
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
  //       },
  //     });
  //   } catch (err) {
  //     console.error(err);
  //     Swal.fire("Error", "Failed to add size ref", "error");
  //   }
  // };

  const handleAddSizeRef = async (sizeRef: string) => {
    if (!sizeRef.trim()) return;

    const token = localStorage.getItem("token");
    console.log("TOKEN:", token);

    if (token) {
      // แยก payload จาก JWT
      const payloadBase64 = token.split(".")[1];
      const payloadJson = atob(payloadBase64);
      const payload = JSON.parse(payloadJson);
      console.log("Decoded Payload:", payload);

      // ✅ log เฉพาะ role
      console.log("ROLE FROM TOKEN:", payload.role);
    }

    try {
      const url = "/api/Requests";

      const body = {
        request_type: "INSERT",
        target_table: "sizeRefs",
        target_pk_id: null,
        old_data: null,
        new_data: {
          size_ref: sizeRef,
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
        icon: "info",
        title: "Request Submitted",
        text: `Your request to add a size ref has been submitted. It will be effective after admin approval.`,
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
        },
      });

      setSearch("");
      fetchSizeRefs();
    } catch (err) {
      console.error(err);
      Swal.fire("Error", "Failed to submit request.", "error");
    }
  };

  // const handleEditSizeRef = async (row: SizeRefRow) => {
  //   const { value: newRef } = await Swal.fire({
  //     title: "Edit Size Ref",
  //     input: "text",
  //     inputValue: row.size_ref,
  //     inputLabel: "Size Ref",
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
  //         Swal.showValidationMessage("SizeRef cannot be empty");
  //         return false;
  //       }
  //       if (trimmed.toLowerCase() === row.size_ref.toLowerCase()) {
  //         // ไม่ต้องเปลี่ยน ถ้าเหมือนเดิม
  //         return false;
  //       }
  //       try {
  //         const res = await axios.get(
  //           `/api/SizeRefs?search=${encodeURIComponent(
  //             trimmed
  //           )}`
  //         );
  //         if (
  //           Array.isArray(res.data) &&
  //           res.data.some(
  //             (r: any) => r.size_ref.toLowerCase() === trimmed.toLowerCase()
  //           )
  //         ) {
  //           Swal.showValidationMessage("This size ref already exists");
  //           return false;
  //         }
  //       } catch {
  //         Swal.showValidationMessage("Error checking for duplicates");
  //         return false;
  //       }
  //       return trimmed;
  //     },
  //   });

  //   if (newRef && newRef.trim() !== row.size_ref) {
  //     const token = localStorage.getItem("token");
  //     console.log("TOKEN:", token);

  //     if (token) {
  //       // แยก payload จาก JWT (optional)
  //       const payloadBase64 = token.split(".")[1];
  //       const payloadJson = atob(payloadBase64);
  //       const payload = JSON.parse(payloadJson);
  //       console.log("Decoded Payload:", payload);
  //       console.log("ROLE FROM TOKEN:", payload.role);
  //     }

  //     try {
  //       await axios.put(
  //         `/api/SizeRefs/${row.size_ref_id}`,
  //         {
  //           size_ref_id: row.size_ref_id,
  //           size_ref: newRef.trim(),
  //         },
  //         {
  //           headers: {
  //             Authorization: `Bearer ${token}`,
  //           },
  //         }
  //       );
  //       fetchSizeRefs();
  //       Swal.fire({
  //         icon: "success",
  //         title: "Success",
  //         text: "SizeRef updated successfully",
  //         confirmButtonText: "OK",
  //         customClass: {
  //           confirmButton:
  //             "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
  //         },
  //       });
  //     } catch {
  //       Swal.fire("Error", "Failed to update size ref", "error");
  //     }
  //   }
  // };

  const handleEditSizeRef = async (row: SizeRefRow) => {
    const { value: newRef } = await Swal.fire({
      title: "Edit Size Ref",
      input: "text",
      inputValue: row.size_ref,
      inputLabel: "Size Ref",
      showCancelButton: true,
      confirmButtonText: "Submit Update Request",
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
          Swal.showValidationMessage("SizeRef cannot be empty");
          return false;
        }
        if (trimmed.toLowerCase() === row.size_ref.toLowerCase()) {
          // ไม่ต้องเปลี่ยน ถ้าเหมือนเดิม
          return false;
        }
        try {
          const res = await axios.get(
            `/api/SizeRefs?search=${encodeURIComponent(
              trimmed
            )}`
          );
          if (
            Array.isArray(res.data) &&
            res.data.some(
              (r: any) =>
                r.size_ref.toLowerCase() === trimmed.toLowerCase() &&
                r.size_ref_id !== row.size_ref_id
            )
          ) {
            Swal.showValidationMessage("This size ref already exists");
            return false;
          }
        } catch {
          Swal.showValidationMessage("Error checking for duplicates");
          return false;
        }
        return trimmed;
      },
    });

    if (newRef && newRef.trim() !== row.size_ref) {
      const token = localStorage.getItem("token");
      console.log("TOKEN:", token);

      if (token) {
        // แยก payload จาก JWT (optional)
        const payloadBase64 = token.split(".")[1];
        const payloadJson = atob(payloadBase64);
        const payload = JSON.parse(payloadJson);
        console.log("Decoded Payload:", payload);
        console.log("ROLE FROM TOKEN:", payload.role);
      }

      try {
        const url = "/api/Requests";

        const requestBody = {
          request_type: "UPDATE",
          target_table: "sizeRefs",
          target_pk_id: row.size_ref_id,
          old_data: {
            size_ref_id: row.size_ref_id,
            size_ref: row.size_ref,
          },
          new_data: {
            size_ref_id: row.size_ref_id,
            size_ref: newRef.trim(),
          },
          note: "Edited from frontend",
        };

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
          icon: "info",
          title: "Request Submitted",
          html: `Your update request has been submitted.<br>Request ID: <b>${result.request_id}</b>`,
          confirmButtonText: "OK",
          customClass: {
            confirmButton:
              "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
          },
        });

        fetchSizeRefs();
      } catch (err) {
        console.error(err);
        Swal.fire("Error", "Failed to submit update request.", "error");
      }
    }
  };

  // const handleDelete = async (id?: number) => {
  //   if (!id) return;

  //   const result = await Swal.fire({
  //     title: "Are you sure?",
  //     text: "Do you really want to delete this size ref?",
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
  //     const token = localStorage.getItem("token");
  //     console.log("TOKEN:", token);

  //     if (token) {
  //       // Optional: decode JWT
  //       const payloadBase64 = token.split(".")[1];
  //       const payloadJson = atob(payloadBase64);
  //       const payload = JSON.parse(payloadJson);
  //       console.log("Decoded Payload:", payload);
  //       console.log("ROLE FROM TOKEN:", payload.role);
  //     }

  //     const res = await fetch(`/api/SizeRefs/${id}`, {
  //       method: "DELETE",
  //       headers: {
  //         Authorization: `Bearer ${token}`,
  //       },
  //     });

  //     if (res.ok) {
  //       fetchSizeRefs();
  //       Swal.fire({
  //         icon: "success",
  //         title: "Deleted!",
  //         text: "SizeRef has been deleted.",
  //         confirmButtonText: "OK",
  //         customClass: {
  //           confirmButton:
  //             "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
  //         },
  //       });
  //     } else {
  //       Swal.fire({
  //         icon: "error",
  //         title: "Error",
  //         text: "Failed to delete size ref.",
  //         confirmButtonText: "OK",
  //         customClass: {
  //           confirmButton:
  //             "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
  //         },
  //       });
  //     }
  //   }
  // };

  const handleDelete = async (id?: number) => {
    if (!id) return;

    const result = await Swal.fire({
      title: "Are you sure?",
      text: "Do you really want to delete this size ref?",
      icon: "warning",
      showCancelButton: true,
      confirmButtonText: "Submit Delete Request",
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

    const token = localStorage.getItem("token");
    console.log("TOKEN:", token);

    if (token) {
      // Optional: decode JWT
      const payloadBase64 = token.split(".")[1];
      const payloadJson = atob(payloadBase64);
      const payload = JSON.parse(payloadJson);
      console.log("Decoded Payload:", payload);
      console.log("ROLE FROM TOKEN:", payload.role);
    }

    try {
      // ✅ Load old data for snapshot
      const oldDataRes = await fetch(
        `/api/SizeRefs/${id}`,
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
          "Cannot load size ref data for delete request.",
          "error"
        );
        return;
      }

      const oldData = await oldDataRes.json();

      const requestBody = {
        request_type: "DELETE",
        target_table: "sizeRefs",
        target_pk_id: id,
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
        Swal.fire({
          icon: "error",
          title: "Error",
          text: errorData.message || "Failed to submit delete request.",
          confirmButtonText: "OK",
          customClass: {
            confirmButton:
              "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
          },
        });
        return;
      }

      const resultData = await response.json();

      Swal.fire({
        icon: "info",
        title: "Request Submitted",
        html: `Your delete request has been submitted.<br>Request ID: <b>${resultData.request_id}</b>`,
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
        },
      });

      fetchSizeRefs();
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
    link.setAttribute("download", "size_refs.csv");
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const columns = useMemo<ColumnDef<SizeRefRow, any>[]>(
    () => [
      {
        header: "#",
        cell: ({ row }) => row.index + 1,
      },
      { accessorKey: "size_ref", header: "Size Ref" },
      // {
      //   header: "Actions",
      //   cell: ({ row }) => (
      //     <div className="flex justify-center space-x-2">
      //       <button
      //         onClick={() => handleEditSizeRef(row.original)}
      //         className="text-blue-500 hover:text-blue-700"
      //       >
      //         <FiEdit2 />
      //       </button>
      //       <button
      //         onClick={() => handleDelete(row.original.size_ref_id)}
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
                onClick={() => handleEditSizeRef(row.original)}
                className="text-blue-500 hover:text-blue-700"
              >
                <FiEdit2 />
              </button>
              <button
                onClick={() => handleDelete(row.original.size_ref_id)}
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
  // 2. sortedData
  const sortedData = useMemo(() => {
    return [...data]
      .filter((d) => d.size_ref_id !== undefined && d.size_ref_id !== null)
      .sort((a, b) => a.size_ref_id - b.size_ref_id);
  }, [data]);
  const table = useReactTable({
    data: sortedData,
    columns,
    state: { pagination },
    onPaginationChange: setPagination,
    getCoreRowModel: getCoreRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
  });

  return (
    <div className="space-y-4 relative">
      <div className="bg-yellow-100 border border-yellow-300 text-yellow-800 text-sm p-2 rounded mb-2 mx-4">
        <strong>Note:</strong> Changes to size ref will be pending until
        approved by an admin.
      </div>
      <div className="flex justify-between px-4 items-center">
        <div className="flex space-x-2">
          <button
            className="bg-blue-600 hover:bg-blue-700 text-white text-sm px-4 py-1 rounded shadow"
            onClick={() => {
              Swal.fire({
                title: "Add New SizeRef",
                input: "text",
                inputLabel: "Size Ref",
                showCancelButton: true,
                confirmButtonText: "Add",
                cancelButtonText: "Cancel",
                customClass: {
                  popup: "text-sm",
                  input: "text-sm px-3 py-2 border border-blue-300 rounded",
                  confirmButton:
                    "bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded",
                  cancelButton:
                    "bg-gray-200 hover:bg-gray-300 text-black px-4 py-2 rounded",
                },
                preConfirm: async (value) => {
                  const trimmed = value.trim();
                  if (!trimmed) {
                    Swal.showValidationMessage("SizeRef cannot be empty");
                    return false;
                  }
                  try {
                    const res = await axios.get(
                      `/api/SizeRefs?search=${encodeURIComponent(
                        trimmed
                      )}`
                    );
                    if (
                      Array.isArray(res.data) &&
                      res.data.some(
                        (r: any) =>
                          r.size_ref.toLowerCase() === trimmed.toLowerCase()
                      )
                    ) {
                      Swal.showValidationMessage(
                        "This size ref already exists"
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
                  handleAddSizeRef(result.value);
                }
              });
            }}
          >
            + Add SizeRef
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
          placeholder="Search size ref..."
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

export default SizeRefTable;
