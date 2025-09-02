// ✅ ToolMachineMapTable.tsx with Pivot Toggle, Modal Add, and Dropdown Filter
import React, { useEffect, useMemo, useRef, useState } from "react";
import {
  useReactTable,
  getCoreRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  flexRender,
} from "@tanstack/react-table";
import type {
  ColumnDef,
  ColumnFiltersState,
  VisibilityState,
} from "@tanstack/react-table";
import Papa from "papaparse";

import ToolMachineMapAddNewMachineModal from "./ToolMachineMapAddNewMachineModal";
import { ToolMachineMapEditModal } from "./ToolMachineMapEditModal";

import { PlusCircle, Edit3, Trash2 } from "lucide-react";

import Swal from "sweetalert2";
import withReactContent from "sweetalert2-react-content";
const MySwal = withReactContent(Swal);

import ExcelJS from "exceljs";
import { saveAs } from "file-saver";

import type { Option } from "./ToolMachineMapAddNewMachineModal";

import DateTimeFilter from "./DateTimeFilter"; // ✅ เพิ่ม

import { createPortal } from "react-dom";

export interface ToolMachineRow {
  map_id: number;
  tool_type: string;
  tool_name: string;
  type_ref: string;
  tool_ref: string;
  size_ref: string;
  machine_no: string;
  create_by: string;
  create_at: string;
  update_by?: string;
  update_at?: string;
  description?: string; // ✅ เพิ่มตรงนี้
  // pending_status?: "UPDATE" | "DELETE" | null; // ✅ เพิ่ม field นี้ไว้รองรับ
  [key: string]: string | undefined | number | null;
}

export interface ToolMachineMapRowInput {
  type_id: number | null;
  tool_id: number | null;
  type_ref_id: number | null;
  tool_ref_id: number | null;
  size_ref_id: number | null;
  machine_id: number | null;
  description?: string; // ✅ เพิ่มตรงนี้ด้วย
}

const ToolMachineMapTable: React.FC = () => {
  const [data, setData] = useState<ToolMachineRow[]>([]);
  const [filterOptions, setFilterOptions] = useState<{
    [key: string]: string[];
  }>({});
  const [filterValues, setFilterValues] = useState<{ [key: string]: string[] }>(
    {}
  );
  const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([]);
  const [columnVisibility, setColumnVisibility] = useState<VisibilityState>({});
  const [pagination, setPagination] = useState({ pageIndex: 0, pageSize: 15 });
  const [dropdownOpen, setDropdownOpen] = useState<{ [key: string]: boolean }>(
    {}
  );
  const [isPivot, setIsPivot] = useState(false);

  const [showAddModal, setShowAddModal] = useState(false); // ✅ ย้ายมาไว้ตรงนี้

  const [cloneRow, setCloneRow] = useState<ToolMachineMapRowInput | null>(null);
  const [fromClone, setFromClone] = useState(false);

  const [types, setTypes] = useState<Option[]>([]);
  const [tools, setTools] = useState<Option[]>([]);
  const [typeRefs, setTypeRefs] = useState<Option[]>([]);
  const [toolRefs, setToolRefs] = useState<Option[]>([]);
  const [sizeRefs, setSizeRefs] = useState<Option[]>([]);
  const [machines, setMachines] = useState<Option[]>([]);

  const [datetimeFilters, setDatetimeFilters] = useState({
    created_at_start: "",
    created_at_end: "",
    updated_at_start: "",
    updated_at_end: "",
  });

  // const [columnVisibility, setColumnVisibility] = useState<VisibilityState>({});
  const [search, setSearch] = useState("");
  const columnsButtonRef = useRef<HTMLButtonElement>(null);
  const [columnsDropdownPos, setColumnsDropdownPos] = useState<{
    top: number;
    left: number;
  } | null>(null);
  const [showColumnDropdown, setShowColumnDropdown] = useState(false);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      const dropdownEl = document.getElementById("columns-dropdown-portal");

      if (
        dropdownEl &&
        !dropdownEl.contains(event.target as Node) &&
        !columnsButtonRef.current?.contains(event.target as Node)
      ) {
        setShowColumnDropdown(false);
      }
    };

    if (showColumnDropdown && columnsButtonRef.current) {
      const raf = requestAnimationFrame(() => {
        const rect = columnsButtonRef.current!.getBoundingClientRect();
        setColumnsDropdownPos({
          top: rect.bottom + window.scrollY + 8,
          left: rect.right + window.scrollX - 256,
        });
      });

      document.addEventListener("mousedown", handleClickOutside);

      return () => {
        cancelAnimationFrame(raf);
        document.removeEventListener("mousedown", handleClickOutside);
      };
    }
  }, [showColumnDropdown]);

  const handleExportXLSX = async () => {
    const workbook = new ExcelJS.Workbook();
    const sheet = workbook.addWorksheet("ToolMachineMap");

    const visibleColumns = table
      .getVisibleLeafColumns()
      .filter((col) => col.id !== "actions"); // ❌ ตัด Action column

    const subHeaders: string[] = visibleColumns.map((col) => col.id);

    // ✅ Header row
    subHeaders.forEach((header, colIdx) => {
      const cell = sheet.getCell(1, colIdx + 1);
      cell.value = header;
      cell.font = { name: "Arial", size: 10, bold: true };
      cell.alignment = { vertical: "middle", horizontal: "center" };
      cell.border = {
        top: { style: "thin" },
        left: { style: "thin" },
        bottom: { style: "thin" },
        right: { style: "thin" },
      };
    });

    // ✅ Data rows
    const rows = table.getFilteredRowModel().rows;
    rows.forEach((row, rowIdx) => {
      const excelRowIdx = rowIdx + 2;
      const pending = (row.original as any).pending_request;

      let fillColor: string | undefined;
      let fontColor: string | undefined;

      if (pending === "UPDATE") {
        fillColor = "FEFCE8";
        fontColor = "854D0E";
      } else if (pending === "DELETE") {
        fillColor = "FEF2F2";
        fontColor = "991B1B";
      }

      visibleColumns.forEach((col, colIdx) => {
        const accessor = col.id;
        const value = row.original[accessor];
        const cell = sheet.getCell(excelRowIdx, colIdx + 1);

        let val: ExcelJS.CellValue = "";
        if (typeof value === "boolean") {
          val = value ? "✔" : "-";
        } else if (value !== undefined && value !== null) {
          val = String(value);
        }

        cell.value = val;
        cell.font = {
          name: "Arial",
          size: 10,
          color: fontColor ? { argb: fontColor } : undefined,
        };
        cell.alignment = { vertical: "middle", horizontal: "center" };
        cell.border = {
          top: { style: "thin" },
          left: { style: "thin" },
          bottom: { style: "thin" },
          right: { style: "thin" },
        };
        if (fillColor) {
          cell.fill = {
            type: "pattern",
            pattern: "solid",
            fgColor: { argb: fillColor },
          };
        }
      });
    });

    // ✅ Set column width
    sheet.columns.forEach((col) => {
      col.width = 15;
    });

    const buffer = await workbook.xlsx.writeBuffer();
    const blob = new Blob([buffer], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    });
    saveAs(blob, "ToolMachineMap.xlsx");
  };

  const [userRole, setUserRole] = useState<string | null>(null);

  useEffect(() => {
    const token = localStorage.getItem("token");
    if (token) {
      try {
        const payloadBase64 = token.split(".")[1];
        const payloadJson = atob(payloadBase64);
        const payload = JSON.parse(payloadJson);
        setUserRole(payload.role ?? null);
      } catch (error) {
        console.error("Invalid token format", error);
      }
    }
  }, []);

  // const [pendingMap, setPendingMap] = useState<Map<number, string>>(new Map());

  // const fetchPendingRequests = async () => {
  //   const token = localStorage.getItem("token");

  //   try {
  //     const res = await fetch(
  //       "/api/Requests/pending?target_table=ToolMachineMap",
  //       {
  //         headers: token ? { Authorization: `Bearer ${token}` } : {},
  //       }
  //     );

  //     if (!res.ok) {
  //       console.error(
  //         "❌ Pending requests fetch failed:",
  //         res.status,
  //         res.statusText
  //       );
  //       setPendingMap(new Map());
  //       return;
  //     }

  //     const pendingData: {
  //       request_id: number;
  //       request_type: "UPDATE" | "DELETE";
  //       target_pk_id: number;
  //     }[] = await res.json();

  //     const map = new Map<number, "UPDATE" | "DELETE">();
  //     pendingData.forEach((item) => {
  //       if (item.request_type === "UPDATE" || item.request_type === "DELETE") {
  //         map.set(Number(item.target_pk_id), item.request_type);
  //       }
  //     });

  //     setPendingMap(map);
  //   } catch (err) {
  //     console.error("Error fetching pending requests:", err);
  //     setPendingMap(new Map());
  //   }
  // };

  useEffect(() => {
    Promise.all([
      fetch("/api/TypeModels").then((r) => r.json()),
      fetch("/api/Tools").then((r) => r.json()),
      fetch("/api/TypeModels").then((r) => r.json()),
      fetch("/api/Tools").then((r) => r.json()),
      fetch("/api/SizeRefs").then((r) => r.json()),
      fetch("/api/Machines").then((r) => r.json()),
    ]).then(
      ([
        typesData,
        toolsData,
        typeRefsData,
        toolRefsData,
        sizeRefsData,
        machinesData,
      ]) => {
        const toOption = (arr: any[], label: string, value: string) =>
          arr.map((d) => ({
            value: d[value],
            label: d[label] ?? "(Blanks)",
          }));

        setTypes(toOption(typesData, "type_name", "type_id"));
        setTools(toOption(toolsData, "tool_name", "tool_id"));
        setTypeRefs(toOption(typeRefsData, "type_name", "type_id"));
        setToolRefs(toOption(toolRefsData, "tool_name", "tool_id"));
        setSizeRefs(toOption(sizeRefsData, "size_ref", "size_ref_id"));
        setMachines(toOption(machinesData, "machine_no", "machine_id"));
      }
    );
  }, []);

  // const fetchData = () => {
  //   const url = isPivot
  //     ? "/api/ToolMachineMap/pivot"
  //     : "/api/ToolMachineMap";
  //   fetch(url)
  //     .then((res) => res.json())
  //     .then((result: ToolMachineRow[]) => {
  //       setData(result);
  //       const keys = Object.keys(result[0] || {});
  //       const newOptions: { [key: string]: string[] } = {};
  //       keys.forEach((key) => {
  //         newOptions[key] = Array.from(
  //           new Set(result.map((item) => String(item[key] ?? "")))
  //         ).sort();
  //       });
  //       setFilterOptions(newOptions);
  //     });
  // };
  const fetchData = () => {
    const query = new URLSearchParams();

    Object.entries(filterValues).forEach(([key, values]) => {
      // if (values.length > 0) {
      //   // ✅ Fix: แปลง '(Blanks)' เป็น 'null' ชัดๆ ก่อน query.append
      //   const normalized = values.map((v) =>
      //     v.trim() === "(Blanks)" ? "null" : v.trim()
      //   );
      //   query.append(key, normalized.join(","));
      // }
      values.forEach((v) => query.append(key, v));
    });

    // 🔹 ใส่ filter datetime
    Object.entries(datetimeFilters).forEach(([key, value]) => {
      if (value) query.append(key, value); // ไม่ส่งค่าว่าง
    });

    const url = isPivot
      ? `/api/ToolMachineMap/pivot?${query.toString()}`
      : `/api/ToolMachineMap?${query.toString()}`;

    console.log("📤 Sending filterValues:", filterValues);
    console.log("🌐 Final API URL:", url);

    fetch(url)
      .then((res) => res.json())
      .then((result: ToolMachineRow[]) => {
        setData(result);

        const keys = Object.keys(result[0] || {});
        const newOptions: { [key: string]: string[] } = {};
        keys.forEach((key) => {
          newOptions[key] = Array.from(
            new Set(
              result.map((item) =>
                item[key] == null ? "(Blanks)" : String(item[key])
              )
            )
          ).sort();
        });
        setFilterOptions(newOptions);
      })
      .catch((err) => console.error("Error fetching data:", err));
  };

  // const fetchData = async () => {
  //   const url = isPivot
  //     ? "/api/ToolMachineMap/pivot"
  //     : "/api/ToolMachineMap";

  //   try {
  //     const token = localStorage.getItem("token");

  //     // 🔶 1. ดึง pending request ของ ToolMachineMap
  //     const pendingRes = await fetch(
  //       "/api/Requests/pending?target_table=ToolMachineMap",
  //       {
  //         headers: token ? { Authorization: `Bearer ${token}` } : {},
  //       }
  //     );

  //     const pendingMap = new Map<number, "UPDATE" | "DELETE">();
  //     if (pendingRes.ok) {
  //       const pendingData: {
  //         request_id: number;
  //         request_type: "UPDATE" | "DELETE";
  //         target_pk_id: number;
  //       }[] = await pendingRes.json();

  //       pendingData.forEach((item) => {
  //         if (
  //           item.request_type === "UPDATE" ||
  //           item.request_type === "DELETE"
  //         ) {
  //           pendingMap.set(item.target_pk_id, item.request_type);
  //         }
  //       });
  //     }

  //     // 🔷 2. ดึงข้อมูล ToolMachineMap
  //     const res = await fetch(url);
  //     const result: ToolMachineRow[] = await res.json();

  //     // 🔷 3. Merge pending_status เข้ากับ row
  //     const merged = result.map((row) => {
  //       const status = pendingMap.get(Number(row.map_id)) ?? null;
  //       return { ...row, pending_status: status };
  //     });

  //     setData(merged);

  //     // 🔷 4. Set Filter Options
  //     const keys = Object.keys(result[0] || {});
  //     const newOptions: { [key: string]: string[] } = {};
  //     keys.forEach((key) => {
  //       newOptions[key] = Array.from(
  //         new Set(result.map((item) => String(item[key] ?? "")))
  //       ).sort();
  //     });
  //     setFilterOptions(newOptions);
  //   } catch (err) {
  //     console.error("❌ Error fetching ToolMachineMap:", err);
  //   }
  // };

  // ✅ Fetch data and pending requests on mount
  // useEffect(() => {
  //   fetchData();
  //   // fetchPendingRequests();
  // }, []);

  useEffect(() => {
    fetchData();
    // setFilterValues({});
  }, [filterValues, datetimeFilters, isPivot]);

  // const handleDelete = async (row: ToolMachineRow) => {
  //   console.log("🔧 Deleting row:", row);
  //   console.log("🗑️ map_id to delete:", row.map_id);

  //   const escapeHTML = (str: string | null) =>
  //     (str ?? "–").replace(/</g, "&lt;").replace(/>/g, "&gt;");

  //   const result = await MySwal.fire({
  //     title: "Are you sure?",
  //     html: `
  //     <table style="text-align:left; font-size:18px; line-height:1.6;">
  //       <tr><td><b>Tool Type:</b></td><td>${escapeHTML(row.tool_type)}</td></tr>
  //       <tr><td><b>Tool Name:</b></td><td>${escapeHTML(row.tool_name)}</td></tr>
  //       <tr><td><b>Type Ref:</b></td><td>${escapeHTML(row.type_ref)}</td></tr>
  //       <tr><td><b>Tool Ref:</b></td><td>${escapeHTML(row.tool_ref)}</td></tr>
  //       <tr><td><b>Size Ref:</b></td><td>${escapeHTML(row.size_ref)}</td></tr>
  //       <tr><td><b>Machine No:</b></td><td>${escapeHTML(
  //         row.machine_no
  //       )}</td></tr>
  //     </table>
  //   `,
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

  //   if (!result.isConfirmed) return;

  //   const token = localStorage.getItem("token");
  //   console.log("TOKEN:", token);

  //   if (token) {
  //     const payloadBase64 = token.split(".")[1];
  //     const payloadJson = atob(payloadBase64);
  //     const payload = JSON.parse(payloadJson);
  //     console.log("Decoded Payload:", payload);
  //     console.log("ROLE FROM TOKEN:", payload.role);
  //   }

  //   try {
  //     const res = await fetch(
  //       `/api/ToolMachineMap/${row.map_id}`,
  //       {
  //         method: "DELETE",
  //         headers: {
  //           Authorization: `Bearer ${token}`,
  //         },
  //       }
  //     );

  //     if (!res.ok) {
  //       const error = await res.text();
  //       console.error("❌ Delete failed:", error);
  //       await MySwal.fire({
  //         icon: "error",
  //         title: "Delete Failed",
  //         text: error || "Server error",
  //         confirmButtonText: "OK",
  //         customClass: {
  //           confirmButton:
  //             "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
  //         },
  //       });
  //       return;
  //     }

  //     console.log("✅ Delete successful!");
  //     await MySwal.fire({
  //       icon: "success",
  //       title: "Deleted!",
  //       text: "The mapping has been removed successfully.",
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
  //       },
  //     });

  //     fetchData();
  //   } catch (err: any) {
  //     console.error("🔥 Exception during deletion:", err);
  //     await MySwal.fire({
  //       icon: "error",
  //       title: "Error",
  //       text: err?.message || "An unexpected error occurred while deleting.",
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
  //       },
  //     });
  //   }
  // };

  // const handleDelete = async (row: ToolMachineRow) => {
  //   console.log("🔧 Deleting row:", row);
  //   console.log("🗑️ map_id to delete:", row.map_id);

  //   const escapeHTML = (str: string | null) =>
  //     (str ?? "–").replace(/</g, "&lt;").replace(/>/g, "&gt;");

  //   const result = await MySwal.fire({
  //     title: "Are you sure?",
  //     html: `
  //     <table style="text-align:left; font-size:18px; line-height:1.6;">
  //       <tr><td><b>Tool Type:</b></td><td>${escapeHTML(row.tool_type)}</td></tr>
  //       <tr><td><b>Tool Name:</b></td><td>${escapeHTML(row.tool_name)}</td></tr>
  //       <tr><td><b>Type Ref:</b></td><td>${escapeHTML(row.type_ref)}</td></tr>
  //       <tr><td><b>Tool Ref:</b></td><td>${escapeHTML(row.tool_ref)}</td></tr>
  //       <tr><td><b>Size Ref:</b></td><td>${escapeHTML(row.size_ref)}</td></tr>
  //       <tr><td><b>Machine No:</b></td><td>${escapeHTML(
  //         row.machine_no
  //       )}</td></tr>
  //     </table>
  //   `,
  //     icon: "warning",
  //     showCancelButton: true,
  //     confirmButtonText: "Yes, submit request",
  //     cancelButtonText: "Cancel",
  //     customClass: {
  //       popup: "text-sm",
  //       confirmButton:
  //         "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
  //       cancelButton:
  //         "bg-gray-200 hover:bg-gray-300 text-black px-4 py-2 rounded",
  //     },
  //   });

  //   if (!result.isConfirmed) return;

  //   const token = localStorage.getItem("token");
  //   console.log("TOKEN:", token);

  //   try {
  //     const res = await fetch("/api/Requests", {
  //       method: "POST",
  //       headers: {
  //         "Content-Type": "application/json",
  //         Authorization: `Bearer ${token}`,
  //       },
  //       body: JSON.stringify({
  //         request_type: "DELETE",
  //         target_table: "ToolMachineMap",
  //         target_pk_id: row.map_id,
  //         old_data: null,
  //         new_data: null,
  //         note: null,
  //       }),
  //     });

  //     if (!res.ok) {
  //       const error = await res.text();
  //       console.error("❌ Delete request failed:", error);
  //       await MySwal.fire({
  //         icon: "error",
  //         title: "Submission Failed",
  //         text: error || "Server error",
  //         confirmButtonText: "OK",
  //         customClass: {
  //           confirmButton:
  //             "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
  //         },
  //       });
  //       return;
  //     }

  //     console.log("✅ Delete request submitted!");
  //     await MySwal.fire({
  //       icon: "success",
  //       title: "Request Submitted",
  //       text: "Your delete request has been submitted successfully.",
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
  //       },
  //     });

  //     fetchData();
  //   } catch (err: any) {
  //     console.error("🔥 Exception during deletion:", err);
  //     await MySwal.fire({
  //       icon: "error",
  //       title: "Error",
  //       text: err?.message || "An unexpected error occurred while deleting.",
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
  //       },
  //     });
  //   }
  // };

  const handleDelete = async (row: ToolMachineRow) => {
    console.log("🔧 Deleting row:", row);
    console.log("🗑️ map_id to delete:", row.map_id);

    const escapeHTML = (str: string | null) =>
      (str ?? "–").replace(/</g, "&lt;").replace(/>/g, "&gt;");

    const result = await MySwal.fire({
      title: "Are you sure?",
      html: `
      <table style="text-align:left; font-size:18px; line-height:1.6;">
        <tr><td><b>Tool Type:</b></td><td>${escapeHTML(row.tool_type)}</td></tr>
        <tr><td><b>Tool Name:</b></td><td>${escapeHTML(row.tool_name)}</td></tr>
        <tr><td><b>Type Ref:</b></td><td>${escapeHTML(row.type_ref)}</td></tr>
        <tr><td><b>Tool Ref:</b></td><td>${escapeHTML(row.tool_ref)}</td></tr>
        <tr><td><b>Size Ref:</b></td><td>${escapeHTML(row.size_ref)}</td></tr>
        <tr><td><b>Machine No:</b></td><td>${escapeHTML(
          row.machine_no
        )}</td></tr>
      </table>
    `,
      icon: "warning",
      showCancelButton: true,
      confirmButtonText: "Yes, submit request",
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

    try {
      // ✅ Prepare old_data snapshot
      const oldData = {
        map_id: row.map_id,
        tool_key_id: row.tool_key_id,
        tool_type: row.tool_type,
        tool_name: row.tool_name,
        type_ref: row.type_ref,
        tool_ref: row.tool_ref,
        size_ref: row.size_ref,
        machine_no: row.machine_no,
        description: row.description ?? null, // ✅ เพิ่มตรงนี้
      };

      const res = await fetch("/api/Requests", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          request_type: "DELETE",
          target_table: "ToolMachineMap",
          target_pk_id: row.map_id,
          old_data: oldData,
          new_data: null,
          note: null,
        }),
      });

      if (!res.ok) {
        const error = await res.text();
        console.error("❌ Delete request failed:", error);
        await MySwal.fire({
          icon: "error",
          title: "Submission Failed",
          text: error || "Server error",
          confirmButtonText: "OK",
          customClass: {
            confirmButton:
              "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
          },
        });
        return;
      }

      console.log("✅ Delete request submitted!");
      await MySwal.fire({
        icon: "success",
        title: "Request Submitted",
        text: "Your delete request has been submitted successfully.",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
        },
      });

      fetchData();
    } catch (err: any) {
      console.error("🔥 Exception during deletion:", err);
      await MySwal.fire({
        icon: "error",
        title: "Error",
        text: err?.message || "An unexpected error occurred while deleting.",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
        },
      });
    }
  };

  // const filteredData = useMemo(() => {
  //   return data.filter((row) =>
  //     Object.entries(filterValues).every(([key, selected]) => {
  //       if (!selected.length) return true;

  //       const val = row[key];
  //       const displayVal = val == null || val === "" ? "(Blanks)" : String(val);
  //       return selected.includes(displayVal);
  //     })
  //   );
  // }, [data, filterValues]);

  const renderDropdownFilter = (columnId: string) => {
    const isActive = (filterValues[columnId] ?? []).length > 0;
    const [searchText, setSearchText] = useState("");
    const buttonRef = useRef<HTMLButtonElement>(null);
    const dropdownRef = useRef<HTMLDivElement>(null);
    const [dropdownPosition, setDropdownPosition] = useState<{
      top: number;
      left: number;
      width: number;
    } | null>(null);

    const [tempFilterValues, setTempFilterValues] = useState<{
      [key: string]: string[];
    }>({});

    // ⏳ Copy ค่า filter เดิมเข้า temp
    useEffect(() => {
      if (filterValues[columnId]) {
        setTempFilterValues((prev) => ({
          ...prev,
          [columnId]: [...filterValues[columnId]],
        }));
      }
    }, [dropdownOpen[columnId]]);

    // 🔐 ปิด dropdown เมื่อคลิกนอก
    useEffect(() => {
      const handleClickOutside = (event: MouseEvent) => {
        if (
          dropdownRef.current &&
          !dropdownRef.current.contains(event.target as Node) &&
          !buttonRef.current?.contains(event.target as Node)
        ) {
          setDropdownOpen((prev) => ({ ...prev, [columnId]: false }));
        }
      };
      document.addEventListener("mousedown", handleClickOutside);
      return () =>
        document.removeEventListener("mousedown", handleClickOutside);
    }, [columnId]);

    // 📌 กำหนด position dropdown
    useEffect(() => {
      if (dropdownOpen[columnId] && buttonRef.current) {
        const rect = buttonRef.current.getBoundingClientRect();
        const scrollX = window.scrollX || document.documentElement.scrollLeft;
        const scrollY = window.scrollY || document.documentElement.scrollTop;

        setDropdownPosition({
          top: rect.bottom + scrollY + 4,
          left: rect.left + scrollX,
          width: rect.width,
        });
      }
    }, [dropdownOpen[columnId]]);

    const handleOpenDropdown = () => {
      setDropdownOpen((prev) => ({ ...prev, [columnId]: !prev[columnId] }));
    };

    // ✅ เพิ่ม/ลบค่า checkbox ทีละตัว
    const handleTempToggle = (val: string | null) => {
      const stringified = val == null || val === "" ? "null" : val; // ✅ แก้ตรงนี้ให้สอดคล้อง
      setTempFilterValues((prev) => {
        const current = new Set(prev[columnId] || []);
        if (current.has(stringified)) current.delete(stringified);
        else current.add(stringified);
        return { ...prev, [columnId]: Array.from(current) };
      });
    };

    return (
      <>
        <button
          ref={buttonRef}
          onClick={handleOpenDropdown}
          className={`ml-1 px-2 py-0.5 border text-xs rounded shadow transition-colors duration-150 ${
            isActive
              ? "bg-blue-600 border-blue-700 text-white hover:bg-blue-700"
              : "bg-white border-gray-300 text-gray-700 hover:bg-gray-100"
          }`}
        >
          ▼
        </button>

        {dropdownOpen[columnId] && (
          <div
            ref={dropdownRef}
            className="fixed z-[9999] rounded-md border bg-white shadow-lg p-2"
            style={
              dropdownPosition
                ? {
                    top: dropdownPosition.top,
                    left: dropdownPosition.left,
                    minWidth: dropdownPosition.width,
                    visibility: "visible",
                  }
                : { visibility: "hidden" }
            }
          >
            {/* 🔍 Search */}
            <input
              type="text"
              value={searchText}
              placeholder="Search..."
              className="w-full mb-2 px-2 py-1 border rounded text-sm"
              onChange={(e) => setSearchText(e.target.value.toLowerCase())}
            />

            {/* ✅ Select All */}
            <label className="block text-xs font-medium px-1 py-0.5 text-left">
              <input
                type="checkbox"
                className="mr-2"
                checked={
                  tempFilterValues[columnId]?.length ===
                  (filterOptions[columnId]?.map((v) => v ?? "null").length || 0)
                }
                onChange={() => {
                  const all =
                    filterOptions[columnId]?.map((v) => v ?? "null") || [];
                  const curr = tempFilterValues[columnId] || [];
                  setTempFilterValues((prev) => ({
                    ...prev,
                    [columnId]: curr.length === all.length ? [] : all,
                  }));
                }}
              />
              (Select All)
            </label>

            {/* ✅ Scrollable list only */}
            <div className="max-h-48 overflow-y-auto mt-1">
              {filterOptions[columnId]
                ?.filter((val) =>
                  (val ?? "(Blanks)").toLowerCase().includes(searchText)
                )
                .map((val) => {
                  const valueToSend = val == null || val === "" ? "null" : val;

                  return (
                    <label
                      key={valueToSend}
                      className="block px-1 py-0.5 text-sm text-left"
                    >
                      <input
                        type="checkbox"
                        className="mr-2"
                        checked={tempFilterValues[columnId]?.includes(
                          valueToSend
                        )}
                        onChange={() => handleTempToggle(val)}
                      />
                      {val || "(Blanks)"}
                    </label>
                  );
                })}
            </div>

            {/* ✅ OK / Cancel Fixed at bottom */}
            <div className="flex justify-end gap-2 mt-2 pt-2 border-t">
              <button
                className="text-xs border px-2 py-1 rounded bg-gray-100"
                onClick={() =>
                  setDropdownOpen((prev) => ({ ...prev, [columnId]: false }))
                }
              >
                Cancel
              </button>
              <button
                className="text-xs border px-2 py-1 rounded bg-blue-600 text-white"
                onClick={() => {
                  setFilterValues((prev) => ({
                    ...prev,
                    [columnId]: (tempFilterValues[columnId] || []).map((v) =>
                      v.trim()
                    ),
                  }));
                  setDropdownOpen((prev) => ({ ...prev, [columnId]: false }));
                }}
              >
                OK
              </button>
            </div>
          </div>
        )}
      </>
    );
  };

  // const columns = useMemo<ColumnDef<ToolMachineRow, any>[]>(() => {
  //   const keys = Object.keys(data[0] || {});

  //   return [
  //     ...keys
  //       .filter((key) => key !== "map_id")
  //       .map((key) => ({
  //         accessorKey: key,
  //         header: () => (
  //           <div className="flex items-center justify-center">
  //             {key}
  //             {renderDropdownFilter(key)}
  //           </div>
  //         ),
  //       })),
  //     // 👇 เงื่อนไขแสดง Actions เฉพาะตอน isPivot === false
  //     ...(!isPivot
  //       ? [
  //           {
  //             id: "actions",
  //             header: () => "Actions",
  //             cell: ({ row }: { row: any }) => (
  //               <div className="flex gap-2 justify-center">
  //                 <button
  //                   onClick={() =>
  //                     ToolMachineMapEditModal(row.original, fetchData)
  //                   }
  //                   className="px-2 py-1 text-xs border rounded text-blue-600 border-blue-600 hover:bg-blue-50"
  //                 >
  //                   Edit
  //                 </button>
  //                 <button
  //                   onClick={() => {
  //                     const clone = {
  //                       type_id:
  //                         types.find((t) => t.label === row.original.tool_type)
  //                           ?.value ?? 2002,
  //                       tool_id:
  //                         tools.find((t) => t.label === row.original.tool_name)
  //                           ?.value ?? 1002,
  //                       type_ref_id:
  //                         typeRefs.find(
  //                           (t) => t.label === row.original.type_ref
  //                         )?.value ?? 2002,
  //                       tool_ref_id:
  //                         toolRefs.find(
  //                           (t) => t.label === row.original.tool_ref
  //                         )?.value ?? 1002,
  //                       size_ref_id:
  //                         sizeRefs.find(
  //                           (t) => t.label === row.original.size_ref
  //                         )?.value ?? 9,
  //                       machine_id:
  //                         machines.find(
  //                           (m) => m.label === row.original.machine_no
  //                         )?.value ?? null,
  //                     };

  //                     setCloneRow(clone);
  //                     setShowAddModal(true);
  //                     setFromClone(true);
  //                   }}
  //                   className="px-2 py-1 text-xs border rounded text-green-600 border-green-600 hover:bg-green-50"
  //                 >
  //                   Clone
  //                 </button>

  //                 <button
  //                   onClick={() => handleDelete(row.original)}
  //                   className="px-2 py-1 text-xs border rounded text-red-600 border-red-600 hover:bg-red-50"
  //                 >
  //                   Delete
  //                 </button>
  //               </div>
  //             ),
  //           },
  //         ]
  //       : []),
  //   ];
  // }, [data, filterOptions, filterValues, dropdownOpen]);

  // const columns = useMemo<ColumnDef<ToolMachineRow, any>[]>(() => {
  //   const formatHeader = (key: string) =>
  //     key.replace(/_/g, " ").replace(/\b\w/g, (c) => c.toUpperCase());

  //   if (isPivot) {
  //     return [
  //       {
  //         accessorKey: "tool_type",
  //         header: () => (
  //           <div className="flex items-center justify-center">
  //             Tool Type {renderDropdownFilter("tool_type")}
  //           </div>
  //         ),
  //       },
  //       {
  //         accessorKey: "tool_name",
  //         header: () => (
  //           <div className="flex items-center justify-center">
  //             Tool Name {renderDropdownFilter("tool_name")}
  //           </div>
  //         ),
  //       },
  //       {
  //         accessorKey: "type_ref",
  //         header: () => (
  //           <div className="flex items-center justify-center">
  //             Type Ref {renderDropdownFilter("type_ref")}
  //           </div>
  //         ),
  //       },
  //       {
  //         accessorKey: "tool_ref",
  //         header: () => (
  //           <div className="flex items-center justify-center">
  //             Tool Ref {renderDropdownFilter("tool_ref")}
  //           </div>
  //         ),
  //       },
  //       {
  //         accessorKey: "size_ref",
  //         header: () => (
  //           <div className="flex items-center justify-center">
  //             Size Ref {renderDropdownFilter("size_ref")}
  //           </div>
  //         ),
  //       },
  //       {
  //         accessorKey: "machine_no",
  //         header: () => (
  //           <div className="flex items-center justify-center">
  //             Machine No {renderDropdownFilter("machine_no")}
  //           </div>
  //         ),
  //       },
  //     ];
  //   }

  //   const keys = Object.keys(data[0] || {}).filter((key) => key !== "map_id");

  //   return [
  //     ...keys.map((key) => ({
  //       accessorKey: key,
  //       header: () => (
  //         <div className="flex items-center justify-center">
  //           {formatHeader(key)} {renderDropdownFilter(key)}
  //         </div>
  //       ),
  //     })),
  //     {
  //       id: "actions",
  //       header: "Actions",
  //       cell: ({ row }: { row: any }) => (
  //         <div className="flex gap-2 justify-center">
  //           <button
  //             onClick={() => ToolMachineMapEditModal(row.original, fetchData)}
  //             className="px-2 py-1 text-xs border rounded text-blue-600 border-blue-600 hover:bg-blue-50"
  //           >
  //             Edit
  //           </button>
  //           <button
  //             onClick={() => {
  //               const clone = {
  //                 type_id:
  //                   types.find((t) => t.label === row.original.tool_type)
  //                     ?.value ?? 2002,
  //                 tool_id:
  //                   tools.find((t) => t.label === row.original.tool_name)
  //                     ?.value ?? 1002,
  //                 type_ref_id:
  //                   typeRefs.find((t) => t.label === row.original.type_ref)
  //                     ?.value ?? 2002,
  //                 tool_ref_id:
  //                   toolRefs.find((t) => t.label === row.original.tool_ref)
  //                     ?.value ?? 1002,
  //                 size_ref_id:
  //                   sizeRefs.find((t) => t.label === row.original.size_ref)
  //                     ?.value ?? 9,
  //                 machine_id:
  //                   machines.find((m) => m.label === row.original.machine_no)
  //                     ?.value ?? null,
  //               };

  //               setCloneRow(clone);
  //               setShowAddModal(true);
  //               setFromClone(true);
  //             }}
  //             className="px-2 py-1 text-xs border rounded text-green-600 border-green-600 hover:bg-green-50"
  //           >
  //             Clone
  //           </button>
  //           <button
  //             onClick={() => handleDelete(row.original)}
  //             className="px-2 py-1 text-xs border rounded text-red-600 border-red-600 hover:bg-red-50"
  //           >
  //             Delete
  //           </button>
  //         </div>
  //       ),
  //     },
  //   ];
  // }, [data, filterOptions, filterValues, dropdownOpen, isPivot]);

  const formatDateTime = (value: string | undefined) => {
    if (!value) return "-";
    const date = new Date(value);
    return date.toLocaleString("en-US", {
      year: "numeric",
      month: "short",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
    }); // ตัวอย่าง: 25 Jul 2025, 14:30
  };
  const columns = useMemo<ColumnDef<ToolMachineRow, any>[]>(() => {
    const formatHeader = (key: string) =>
      key.replace(/_/g, " ").replace(/\b\w/g, (c) => c.toUpperCase());

    if (isPivot) {
      return [
        {
          accessorKey: "tool_type",
          header: () => (
            <div className="flex items-center justify-center">
              Tool Type {renderDropdownFilter("tool_type")}
            </div>
          ),
        },
        {
          accessorKey: "tool_name",
          header: () => (
            <div className="flex items-center justify-center">
              Tool Name {renderDropdownFilter("tool_name")}
            </div>
          ),
        },
        {
          accessorKey: "type_ref",
          header: () => (
            <div className="flex items-center justify-center">
              Type Ref {renderDropdownFilter("type_ref")}
            </div>
          ),
        },
        {
          accessorKey: "tool_ref",
          header: () => (
            <div className="flex items-center justify-center">
              Tool Ref {renderDropdownFilter("tool_ref")}
            </div>
          ),
        },
        {
          accessorKey: "size_ref",
          header: () => (
            <div className="flex items-center justify-center">
              Size Ref {renderDropdownFilter("size_ref")}
            </div>
          ),
        },
        {
          accessorKey: "machine_no",
          header: () => (
            <div className="flex items-center justify-center">
              Machine No {renderDropdownFilter("machine_no")}
            </div>
          ),
        },
      ];
    }

    const keys = Object.keys(data[0] || {}).filter((key) => key !== "map_id");

    const manualOrder = [
      "tool_type",
      "tool_name",
      "type_ref",
      "tool_ref",
      "size_ref",
      "machine_no",
      "description",
      "create_by",
      "create_at",
      "update_by",
      "update_at",
      "pending_request",
    ];

    const reorderedKeys = [
      ...manualOrder.filter((key) => keys.includes(key)),
      ...keys.filter((key) => !manualOrder.includes(key)),
    ];

    return [
      ...reorderedKeys.map((key) => {
        const noFilter = key === "create_at" || key === "update_at";
        return {
          accessorKey: key,
          header: () => (
            <div className="flex items-center justify-center">
              {formatHeader(key)} {!noFilter && renderDropdownFilter(key)}
            </div>
          ),
          cell: ({ row }: any) =>
            key === "create_at" || key === "update_at"
              ? formatDateTime(row.original[key])
              : row.original[key] ?? "", // ✅ กำหนดให้ทุก field render ปกติ
        };
      }),
      // {
      //   id: "actions",
      //   header: "Actions",
      //   cell: ({ row }: { row: any }) => (
      //     <div className="flex gap-2 justify-center">
      //       <button
      //         onClick={() => ToolMachineMapEditModal(row.original, fetchData)}
      //         className="px-2 py-1 text-xs border rounded text-blue-600 border-blue-600 hover:bg-blue-50"
      //       >
      //         Edit
      //       </button>
      //       <button
      //         onClick={() => {
      //           const clone = {
      //             type_id:
      //               types.find((t) => t.label === row.original.tool_type)
      //                 ?.value ?? 13,
      //             tool_id:
      //               tools.find((t) => t.label === row.original.tool_name)
      //                 ?.value ?? 2,
      //             type_ref_id:
      //               typeRefs.find((t) => t.label === row.original.type_ref)
      //                 ?.value ?? 13,
      //             tool_ref_id:
      //               toolRefs.find((t) => t.label === row.original.tool_ref)
      //                 ?.value ?? 2,
      //             size_ref_id:
      //               sizeRefs.find((t) => t.label === row.original.size_ref)
      //                 ?.value ?? 12,
      //             machine_id:
      //               machines.find((m) => m.label === row.original.machine_no)
      //                 ?.value ?? null,
      //             description: row.original.description ?? "", // ✅ เพิ่มตรงนี้
      //           };

      //           setCloneRow(clone);
      //           setShowAddModal(true);
      //           setFromClone(true);
      //         }}
      //         className="px-2 py-1 text-xs border rounded text-green-600 border-green-600 hover:bg-green-50"
      //       >
      //         Clone
      //       </button>
      //       <button
      //         onClick={() => handleDelete(row.original)}
      //         className="px-2 py-1 text-xs border rounded text-red-600 border-red-600 hover:bg-red-50"
      //       >
      //         Delete
      //       </button>
      //     </div>
      //   ),
      // },
      {
        id: "actions",
        header: "Actions",
        cell: ({ row }: { row: any }) => {
          const canEdit = ["admin", "editor"].includes(userRole || "");

          return canEdit ? (
            <div className="flex gap-2 justify-center">
              <button
                onClick={() => ToolMachineMapEditModal(row.original, fetchData)}
                className="px-2 py-1 text-xs border rounded text-blue-600 border-blue-600 hover:bg-blue-50"
              >
                Edit
              </button>
              <button
                onClick={() => {
                  const clone = {
                    type_id:
                      types.find((t) => t.label === row.original.tool_type)
                        ?.value ?? 13,
                    tool_id:
                      tools.find((t) => t.label === row.original.tool_name)
                        ?.value ?? 2,
                    type_ref_id:
                      typeRefs.find((t) => t.label === row.original.type_ref)
                        ?.value ?? 13,
                    tool_ref_id:
                      toolRefs.find((t) => t.label === row.original.tool_ref)
                        ?.value ?? 2,
                    size_ref_id:
                      sizeRefs.find((t) => t.label === row.original.size_ref)
                        ?.value ?? 12,
                    machine_id:
                      machines.find((m) => m.label === row.original.machine_no)
                        ?.value ?? null,
                    description: row.original.description ?? "",
                  };

                  setCloneRow(clone);
                  setShowAddModal(true);
                  setFromClone(true);
                }}
                className="px-2 py-1 text-xs border rounded text-green-600 border-green-600 hover:bg-green-50"
              >
                Clone
              </button>
              <button
                onClick={() => handleDelete(row.original)}
                className="px-2 py-1 text-xs border rounded text-red-600 border-red-600 hover:bg-red-50"
              >
                Delete
              </button>
            </div>
          ) : null; // ❌ ถ้าไม่ใช่ admin/editor ไม่ต้องแสดงอะไรเลย
        },
      },
    ];
  }, [data, filterOptions, filterValues, dropdownOpen, isPivot]);

  const table = useReactTable({
    data: data,
    columns,
    state: { columnFilters, columnVisibility, pagination },
    onColumnFiltersChange: setColumnFilters,
    onColumnVisibilityChange: setColumnVisibility,
    onPaginationChange: setPagination,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
  });

  return (
    <div className="space-y-4 relative">
      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
        {/* UPDATE */}
        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-200">
          <div className="flex items-center">
            <div className="p-2 bg-yellow-100 rounded-lg">
              <Edit3 className="w-6 h-6 text-yellow-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">
                Pending UPDATE
              </p>
              <p className="text-2xl font-bold text-gray-900">
                {
                  data.filter(
                    (r) => String(r.pending_request).toUpperCase() === "UPDATE"
                  ).length
                }
              </p>
            </div>
          </div>
        </div>

        {/* DELETE */}
        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-200">
          <div className="flex items-center">
            <div className="p-2 bg-red-100 rounded-lg">
              <Trash2 className="w-6 h-6 text-red-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">
                Pending DELETE
              </p>
              <p className="text-2xl font-bold text-gray-900">
                {
                  data.filter(
                    (r) => String(r.pending_request).toUpperCase() === "DELETE"
                  ).length
                }
              </p>
            </div>
          </div>
        </div>
      </div>
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4">
        <div className="flex justify-between items-center px-4">
          <div className="flex items-center gap-2">
            <button
              className="inline-flex items-center px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg shadow-sm hover:bg-gray-50 hover:shadow-md active:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-all duration-200"
              onClick={handleExportXLSX}
            >
              <svg
                className="w-4 h-4 mr-2"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                />
              </svg>
              Export XLSX
            </button>

            <button
              className={`inline-flex items-center px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg shadow-sm hover:bg-gray-50 hover:shadow-md active:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-all duration-200 ${
                isPivot ? "bg-blue-100 text-blue-800" : "bg-white"
              }`}
              onClick={() => setIsPivot((prev) => !prev)}
            >
              View: {isPivot ? "Group By Pad Name" : "Raw"}
            </button>
            <div className="relative">
              <button
                ref={columnsButtonRef}
                onClick={() => setShowColumnDropdown((prev) => !prev)}
                className="inline-flex items-center px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg shadow-sm hover:bg-gray-50 hover:shadow-md active:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-all duration-200"
              >
                <svg
                  className="w-4 h-4 mr-2"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    d="M4 6h16M4 12h16M4 18h16M4 6v12M12 6v12M20 6v12"
                  />
                </svg>
                Columns
                <svg
                  className={`w-4 h-4 ml-2 transition-transform duration-200 ${
                    showColumnDropdown ? "rotate-180" : ""
                  }`}
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    d="M19 9l-7 7-7-7"
                  />
                </svg>
              </button>

              {showColumnDropdown &&
                columnsDropdownPos &&
                createPortal(
                  <div
                    id="columns-dropdown-portal" // ✅ ใช้ตรวจจับคลิกนอก
                    className="fixed z-[9999] w-64 bg-white border border-gray-200 rounded-lg shadow-lg max-h-80 overflow-hidden animate-in fade-in-0 zoom-in-95 duration-200"
                    style={{
                      top: columnsDropdownPos?.top ?? 0,
                      left: columnsDropdownPos?.left ?? 0,
                    }}
                  >
                    <div className="p-3 border-b border-gray-200">
                      <input
                        type="text"
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                        placeholder="Search columns..."
                        className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      />
                    </div>
                    <div className="max-h-60 overflow-y-auto">
                      {table
                        .getAllLeafColumns()
                        .filter((col) =>
                          col.id?.toLowerCase().includes(search.toLowerCase())
                        )
                        .map((column) => (
                          <label
                            key={column.id}
                            className="flex items-center px-3 py-2 text-sm hover:bg-gray-50 cursor-pointer transition-colors duration-150"
                          >
                            <input
                              type="checkbox"
                              checked={column.getIsVisible()}
                              onChange={() => column.toggleVisibility()}
                              className="mr-3 w-4 h-4 text-blue-600 rounded focus:ring-blue-500"
                            />
                            <span className="text-gray-700">{column.id}</span>
                          </label>
                        ))}
                    </div>
                  </div>,
                  document.body
                )}
            </div>
            <DateTimeFilter
              filterMode="datetime"
              onFilterApply={setDatetimeFilters}
              initialFilters={datetimeFilters}
            />
          </div>
          {["admin", "editor"].includes(userRole || "") && (
            <button
              className="inline-flex items-center px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-lg shadow-sm hover:bg-blue-700 hover:shadow-md active:bg-blue-800 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-all duration-200"
              onClick={() => setShowAddModal(true)}
            >
              <svg
                className="w-4 h-4 mr-2"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M12 6v6m0 0v6m0-6h6m-6 0H6"
                />
              </svg>
              Add Mapping
            </button>
          )}
        </div>
      </div>

      <div className="overflow-auto max-h-[70vh] border rounded-lg">
        <table className="min-w-full text-sm text-center">
          <thead className="bg-gray-100 border-b sticky top-0 z-10">
            {table.getHeaderGroups().map((headerGroup) => (
              <tr key={headerGroup.id}>
                {headerGroup.headers.map((header) => (
                  <th
                    key={header.id}
                    colSpan={header.colSpan}
                    className="px-2 py-2 border text-center font-semibold text-xs text-gray-700 whitespace-nowrap"
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
              // <tr
              //   key={row.id}
              //   className="hover:bg-blue-50 transition-colors duration-100 even:bg-gray-50"
              // >
              <tr
                key={row.id}
                className={`transition-colors duration-100 ${
                  row.original.pending_request === "UPDATE"
                    ? "bg-yellow-50 text-yellow-900 border-l-4 border-yellow-400"
                    : row.original.pending_request === "DELETE"
                    ? "bg-red-50 text-red-800 border-l-4 border-red-500"
                    : "even:bg-gray-50"
                } hover:bg-blue-50`}
              >
                {row.getVisibleCells().map((cell) => (
                  <td
                    key={cell.id}
                    className="px-2 py-2 border text-sm text-gray-800 whitespace-nowrap"
                  >
                    {flexRender(cell.column.columnDef.cell, cell.getContext())}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="flex justify-between items-center px-4 py-2 mt-2 border-t bg-white">
        <div className="flex items-center text-sm">
          <span className="mr-2">Results per page:</span>
          <select
            value={table.getState().pagination.pageSize}
            onChange={(e) => table.setPageSize(Number(e.target.value))}
            className="border rounded px-2 py-1 text-sm"
          >
            {[10, 15, 25, 50, 100].map((size) => (
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
      {/* {showAddModal && (
        <ToolMachineMapAddNewMachineModal
          onClose={() => setShowAddModal(false)}
          onSubmitSuccess={() => {
            setFilterValues({}); // trigger reload
          }}
        />
      )} */}
      {/* {showAddModal && (
        <ToolMachineMapAddNewMachineModal
          initialData={cloneRow}
          fromClone={fromClone}
          onClose={() => {
            setShowAddModal(false);
            setCloneRow(null);
            setFromClone(false);
          }}
          onSubmitSuccess={() => {
            setShowAddModal(false);
            setCloneRow(null);
            setFromClone(false);
            fetchData();
          }}
        />
      )} */}
      {/* {showAddModal && (
        <div className="fixed inset-0 z-[9999] flex items-center justify-center bg-black bg-opacity-50">
          <div className="bg-white p-4 rounded-lg shadow-lg w-[600px]">
            <ToolMachineMapAddNewMachineModal
              initialData={cloneRow}
              fromClone={fromClone}
              onClose={() => {
                setShowAddModal(false);
                setCloneRow(null);
                setFromClone(false);
              }}
              onSubmitSuccess={() => {
                setShowAddModal(false);
                setCloneRow(null);
                setFromClone(false);
                fetchData();
              }}
            />
          </div>
        </div>
      )} */}
      {showAddModal && (
        <ToolMachineMapAddNewMachineModal
          initialData={cloneRow}
          fromClone={fromClone}
          onClose={() => {
            setShowAddModal(false);
            setCloneRow(null);
            setFromClone(false);
          }}
          onSubmitSuccess={() => {
            setShowAddModal(false);
            setCloneRow(null);
            setFromClone(false);
            fetchData();
          }}
        />
      )}
    </div>
  );
};

export default ToolMachineMapTable;
