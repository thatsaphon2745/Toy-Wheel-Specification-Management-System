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
import DdcSpecAddToolModal from "./DdcSpecAddToolModal";
import DdcSpecEditToolModal from "./DdcSpecEditToolModal"; // Assuming this is the correct import path

import { PlusCircle, Edit3, Trash2 } from "lucide-react";

import Swal from "sweetalert2";
import withReactContent from "sweetalert2-react-content";
const MySwal = withReactContent(Swal);

import DefDdcModal from "./DefDdcModal";

import definitions1 from "../assets/definitions1.png";
import definitions2 from "../assets/definitions2.png";
import ToolDdcDetailModal from "./ToolDdcDetailModal";

import { createPortal } from "react-dom";

import type { Option } from "./DdcSpecAddToolModal";

import ExcelJS from "exceljs";
import { saveAs } from "file-saver"; // ใช้ช่วยดาวน์โหลดไฟล์ blob

import DateTimeFilter from "./DateTimeFilter"; // ✅ เพิ่ม

interface SummaryDdcRow {
  ref_key_id: number;
  tool_spec_id: number;
  tool_type: string;
  tool_name: string;
  position_type: string;
  type_ref: string;
  tool_ref: string;
  size_ref: string;
  axle_type: string;
  overall_a: number;
  overall_b: number;
  overall_c: number;
  tolerance_a: number;
  tolerance_b: number;
  tolerance_c: number;
  f_shank_min: number;
  f_shank_max: number;
  b2b_min: number;
  b2b_max: number;
  h2h_min: number;
  h2h_max: number;
  chassis_span: number;
  knurling_type: number;
  HST_pad: string;
  RIM_pad: string;
  INNER_pad: string;
  EXTRA_RIM_pad: string;
  HST_brass: string;
  RIM_brass: string;
  INNER_brass: string;
  EXTRA_RIM_brass: string;
  pad_source: string;
  pad_source_key: string;
  machine_no: string;
  machine_source: string;
  machine_source_key: string;
  pending_status?: "UPDATE" | "DELETE" | null;
  pending_request?: string;
  create_by: string;
  create_at: string;
  update_by?: string;
  update_at?: string;
  description?: string; // ✅ ใหม่
  [key: string]: any;
}

interface MyColumnMeta {
  label?: string;
}

const SummaryDdcTable: React.FC = () => {
  const [data, setData] = useState<SummaryDdcRow[]>([]);
  const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([]);
  const [columnVisibility, setColumnVisibility] = useState<VisibilityState>({});
  const [search, setSearch] = useState("");
  const [showColumnDropdown, setShowColumnDropdown] = useState(false);
  const [pagination, setPagination] = useState({ pageIndex: 0, pageSize: 15 });
  const dropdownRef = useRef<HTMLDivElement>(null);

  const [dropdownOpen, setDropdownOpen] = useState<{ [key: string]: boolean }>(
    {}
  );
  const [filterOptions, setFilterOptions] = useState<{
    [key: string]: string[];
  }>({});
  const [filterValues, setFilterValues] = useState<{ [key: string]: string[] }>(
    {}
  );

  const [showAddModal, setShowAddModal] = useState(false); // ✅ ย้ายมาไว้ตรงนี้

  const [editingRow, setEditingRow] = useState<SummaryDdcRow | null>(null);
  const [showEditModal, setShowEditModal] = useState(false);

  const [showDefModal, setShowDefModal] = useState(false);

  const [showDetailModal, setShowDetailModal] = useState(false);
  const [detailData, setDetailData] = useState<SummaryDdcRow | null>(null);

  const columnsButtonRef = useRef<HTMLButtonElement>(null);
  const [columnsDropdownPos, setColumnsDropdownPos] = useState<{
    top: number;
    left: number;
  } | null>(null);

  const [cloneRow, setCloneRow] = useState<any>(null);
  const [fromClone, setFromClone] = useState(false);

  const [types, setTypes] = useState<Option[]>([]);
  const [tools, setTools] = useState<Option[]>([]);
  const [positions, setPositions] = useState<Option[]>([]);
  const [typeRefs, setTypeRefs] = useState<Option[]>([]);
  const [toolRefs, setToolRefs] = useState<Option[]>([]);
  const [sizeRefs, setSizeRefs] = useState<Option[]>([]);
  const [axles, setAxles] = useState<Option[]>([]);

  const [reloadFlag, setReloadFlag] = useState(0);

  const [datetimeFilters, setDatetimeFilters] = useState({
    created_at_start: "",
    created_at_end: "",
    updated_at_start: "",
    updated_at_end: "",
  });

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
  //       "/api/Requests/pending?target_table=DdcSpec",
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

  // useEffect(() => {
  //   fetchPendingRequests();
  // }, []);

  useEffect(() => {
    if (showColumnDropdown && columnsButtonRef.current) {
      const rect = columnsButtonRef.current.getBoundingClientRect();
      setColumnsDropdownPos({
        top: rect.bottom + window.scrollY + 8,
        left: rect.right + window.scrollX - 256,
      });
    }
  }, [showColumnDropdown]);

  // const hasFetchedRef = useRef(false);

  useEffect(() => {
    // if (hasFetchedRef.current) return; // ❌ block รอบที่ 2
    // hasFetchedRef.current = true;

    const query = new URLSearchParams();
    Object.entries(filterValues).forEach(([key, values]) => {
      values.forEach((v) => query.append(key, v));
    });
    // 🔹 ใส่ filter datetime
    Object.entries(datetimeFilters).forEach(([key, value]) => {
      if (value) query.append(key, value); // ไม่ส่งค่าว่าง
    });
    fetch(
      `/api/ResolvedToolSpecsRaw/raw?${query.toString()}`
    )
      .then((res) => res.json())
      .then((result: SummaryDdcRow[]) => {
        setData(result);
        const keys = Object.keys(result[0] || {});
        const newOptions: { [key: string]: string[] } = {};
        keys.forEach((key) => {
          newOptions[key] = Array.from(
            new Set(
              result.map((item) =>
                String((item as Record<string, any>)[key] ?? "")
              )
            )
          ).sort();
        });
        setFilterOptions(newOptions);
      })
      .catch((err) => console.error("Error fetching data:", err));
  }, [filterValues, datetimeFilters, reloadFlag]);

  const hasFetchedOption = useRef(false);

  // useEffect(() => {
  //   if (hasFetchedOption.current) return; // ❌ block รอบที่ 2
  //   hasFetchedOption.current = true;
  //   Promise.all([
  //     fetch("/api/TypeModels").then((r) => r.json()),
  //     fetch("/api/Tools").then((r) => r.json()),
  //     fetch("/api/PositionTypes").then((r) => r.json()),
  //     fetch("/api/TypeModels").then((r) => r.json()),
  //     fetch("/api/Tools").then((r) => r.json()),
  //     fetch("/api/SizeRefs").then((r) => r.json()),
  //     fetch("/api/AxleTypes").then((r) => r.json()),
  //   ]).then(
  //     ([
  //       typesData,
  //       toolsData,
  //       positionsData,
  //       typeRefsData,
  //       toolRefsData,
  //       sizeRefsData,
  //       axlesData,
  //     ]) => {
  //       const toOption = (arr: any[], label: string, value: string) =>
  //         arr.map((d) => ({
  //           value: d[value],
  //           label: d[label] ?? "(Blanks)",
  //         }));

  //       setTypes(toOption(typesData, "type_name", "type_id"));
  //       setTools(toOption(toolsData, "tool_name", "tool_id"));
  //       setPositions(
  //         toOption(positionsData, "position_type", "position_type_id")
  //       );
  //       setTypeRefs(toOption(typeRefsData, "type_name", "type_id"));
  //       setToolRefs(toOption(toolRefsData, "tool_name", "tool_id"));
  //       setSizeRefs(toOption(sizeRefsData, "size_ref", "size_ref_id"));
  //       setAxles(toOption(axlesData, "axle_type", "axle_type_id"));
  //     }
  //   );
  // }, []);

  useEffect(() => {
    if (hasFetchedOption.current) return;
    hasFetchedOption.current = true;

    Promise.all([
      fetch("/api/TypeModels").then((r) => r.json()),
      fetch("/api/Tools").then((r) => r.json()),
      fetch("/api/PositionTypes").then((r) => r.json()),
      fetch("/api/SizeRefs").then((r) => r.json()),
      fetch("/api/AxleTypes").then((r) => r.json()),
    ]).then(
      ([typesData, toolsData, positionsData, sizeRefsData, axlesData]) => {
        const toOption = (arr: any[], label: string, value: string) =>
          arr.map((d) => ({
            value: d[value],
            label: d[label] ?? "(Blanks)",
          }));

        setTypes(toOption(typesData, "type_name", "type_id"));
        setTypeRefs(toOption(typesData, "type_name", "type_id")); // ✅ reuse
        setTools(toOption(toolsData, "tool_name", "tool_id"));
        setToolRefs(toOption(toolsData, "tool_name", "tool_id")); // ✅ reuse
        setPositions(
          toOption(positionsData, "position_type", "position_type_id")
        );
        setSizeRefs(toOption(sizeRefsData, "size_ref", "size_ref_id"));
        setAxles(toOption(axlesData, "axle_type", "axle_type_id"));
      }
    );
  }, []);

  // useEffect(() => {
  //   const query = new URLSearchParams();
  //   Object.entries(filterValues).forEach(([key, values]) => {
  //     if (values.length > 0) query.append(key, values.join(","));
  //   });

  //   fetch(
  //     `/api/ResolvedToolSpecsRaw/raw?${query.toString()}`
  //   )
  //     .then((res) => res.json())
  //     .then((result: SummaryDdcRow[]) => {
  //       const merged = result.map((row) => {
  //         const rawStatus = pendingMap.get(Number(row.tool_spec_id));
  //         let pending_status: "UPDATE" | "DELETE" | null = null;

  //         if (rawStatus === "UPDATE" || rawStatus === "DELETE") {
  //           pending_status = rawStatus;
  //         }

  //         return {
  //           ...row,
  //           pending_status,
  //         };
  //       });

  //       setData(merged);

  //       const keys = Object.keys(result[0] || {});
  //       const newOptions: { [key: string]: string[] } = {};
  //       keys.forEach((key) => {
  //         newOptions[key] = Array.from(
  //           new Set(
  //             result.map((item) =>
  //               String((item as Record<string, any>)[key] ?? "")
  //             )
  //           )
  //         ).sort();
  //       });
  //       setFilterOptions(newOptions);
  //     })
  //     .catch((err) => console.error("Error fetching data:", err));
  // }, [filterValues, pendingMap]);

  // const handleDelete = async (row: SummaryDdcRow) => {
  //   console.log("🗑️ Deleting DDC spec row:", row);

  //   const escapeHTML = (str: string | null | undefined) =>
  //     (str ?? "-").replace(/</g, "&lt;").replace(/>/g, "&gt;");

  //   const result = await MySwal.fire({
  //     title: "Are you sure?",
  //     html: `
  //     <table style="text-align:left; font-size:18px; line-height:1.6;">
  //       <tr><td><b>Tool Type:</b></td><td>${escapeHTML(row.tool_type)}</td></tr>
  //       <tr><td><b>Tool Name:</b></td><td>${escapeHTML(row.tool_name)}</td></tr>
  //       <tr><td><b>Position Type:</b></td><td>${escapeHTML(
  //         row.position_type
  //       )}</td></tr>
  //       <tr><td><b>Type Ref:</b></td><td>${escapeHTML(row.type_ref)}</td></tr>
  //       <tr><td><b>Tool Ref:</b></td><td>${escapeHTML(row.tool_ref)}</td></tr>
  //       <tr><td><b>Size Ref:</b></td><td>${escapeHTML(row.size_ref)}</td></tr>
  //       <tr><td><b>Axle Type:</b></td><td>${escapeHTML(row.axle_type)}</td></tr>
  //     </table>
  //   `,
  //     icon: "warning",
  //     showCancelButton: true,
  //     confirmButtonText: "Submit Delete Request",
  //     cancelButtonText: "Cancel",
  //     customClass: {
  //       popup: "text-sm",
  //       confirmButton:
  //         "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
  //       cancelButton:
  //         "bg-gray-200 hover:bg-gray-300 text-black px-4 py-2 rounded",
  //     },
  //   });

  //   if (!result.isConfirmed) {
  //     console.log("❌ User cancelled deletion.");
  //     return;
  //   }

  //   const { tool_spec_id } = row as any;

  //   if (!tool_spec_id) {
  //     await MySwal.fire({
  //       icon: "error",
  //       title: "Missing ID",
  //       text: "tool_spec_id is missing.",
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
  //         popup: "text-sm",
  //       },
  //     });
  //     return;
  //   }

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
  //     const url = "/api/Requests";

  //     const requestBody = {
  //       request_type: "DELETE",
  //       target_table: "DdcSpec",
  //       target_pk_id: tool_spec_id,
  //       old_data: row,
  //       new_data: null,
  //       note: "Deleted from frontend",
  //     };

  //     console.log("🚀 Submitting DELETE request payload:", requestBody);

  //     const res = await fetch(url, {
  //       method: "POST",
  //       headers: {
  //         "Content-Type": "application/json",
  //         ...(token && { Authorization: `Bearer ${token}` }),
  //       },
  //       body: JSON.stringify(requestBody),
  //     });

  //     const text = await res.text();
  //     let json: any = null;

  //     try {
  //       json = JSON.parse(text);
  //     } catch {
  //       console.error("⚠️ Response not JSON:", text);
  //     }

  //     if (!res.ok) {
  //       const message =
  //         json?.message || text || "Failed to submit delete request.";
  //       console.error("❌ Delete request failed:", message);
  //       await MySwal.fire({
  //         icon: "error",
  //         title: "Delete Failed",
  //         text: message,
  //         confirmButtonText: "OK",
  //         customClass: {
  //           confirmButton:
  //             "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
  //           popup: "text-sm",
  //         },
  //       });
  //       return;
  //     }

  //     console.log("✅ Delete request submitted:", json);

  //     await MySwal.fire({
  //       icon: "info",
  //       title: "Request Submitted",
  //       html: `Your delete request has been submitted.<br>Request ID: <b>${json.request_id}</b><br>It will be effective after admin approval.`,
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
  //         popup: "text-sm",
  //       },
  //     });

  //     setColumnFilters([]);
  //   } catch (err: unknown) {
  //     console.error("🔥 Exception during delete request:", err);

  //     let errorMsg =
  //       "An unexpected error occurred while submitting delete request.";
  //     if (err && typeof err === "object" && "message" in err) {
  //       errorMsg = (err as any).message;
  //     }

  //     await MySwal.fire({
  //       icon: "error",
  //       title: "Error",
  //       text: errorMsg,
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
  //         popup: "text-sm",
  //       },
  //     });
  //   }
  // };

  const handleDelete = async (row: SummaryDdcRow) => {
    console.log("🗑️ Deleting DDC spec row:", row);

    const escapeHTML = (str: string | null | undefined) =>
      (str ?? "-").replace(/</g, "&lt;").replace(/>/g, "&gt;");

    const result = await MySwal.fire({
      title: "Are you sure?",
      html: `
      <table style="text-align:left; font-size:18px; line-height:1.6;">
        <tr><td><b>Tool Type:</b></td><td>${escapeHTML(row.tool_type)}</td></tr>
        <tr><td><b>Tool Name:</b></td><td>${escapeHTML(row.tool_name)}</td></tr>
        <tr><td><b>Position Type:</b></td><td>${escapeHTML(
          row.position_type
        )}</td></tr>
        <tr><td><b>Type Ref:</b></td><td>${escapeHTML(row.type_ref)}</td></tr>
        <tr><td><b>Tool Ref:</b></td><td>${escapeHTML(row.tool_ref)}</td></tr>
        <tr><td><b>Size Ref:</b></td><td>${escapeHTML(row.size_ref)}</td></tr>
        <tr><td><b>Axle Type:</b></td><td>${escapeHTML(row.axle_type)}</td></tr>
      </table>
    `,
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

    if (!result.isConfirmed) {
      console.log("❌ User cancelled deletion.");
      return;
    }

    const { tool_spec_id, tool_key_id } = row as any;

    if (!tool_spec_id || !tool_key_id) {
      await MySwal.fire({
        icon: "error",
        title: "Missing ID",
        text: "tool_spec_id or tool_key_id is missing.",
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
          popup: "text-sm",
        },
      });
      return;
    }

    const token = localStorage.getItem("token");

    try {
      // ✅ STEP 1: Check ToolMachineMap and ToolPadMap only
      const endpoints = [
        {
          url: `/api/ToolSpecs/check-toolmachinemap-ref?tool_key_id=${tool_key_id}`,
          label: "ToolMachineMap",
        },
        {
          url: `/api/ToolSpecs/check-toolpadmap-ref?tool_key_id=${tool_key_id}`,
          label: "ToolPadMap",
        },
      ];

      const responses = await Promise.all(
        endpoints.map((ep) =>
          fetch(ep.url, {
            headers: token ? { Authorization: `Bearer ${token}` } : undefined,
          }).then((res) =>
            res.json().then((data) => ({ ...data, label: ep.label }))
          )
        )
      );

      console.log("✅ Usage check responses:", responses);

      const reasons = responses
        .filter((usage) => usage.used)
        .map((usage) => `- Used in ${usage.label}`);

      if (reasons.length > 0) {
        await MySwal.fire({
          title: "Cannot Delete",
          html: `
          <p>This DDC Spec cannot be deleted because:</p>
          <pre style="text-align:left; background:#f3f3f3; padding:10px; border-radius:5px;">${reasons.join(
            "\n"
          )}</pre>
        `,
          icon: "warning",
          confirmButtonText: "OK",
          customClass: {
            confirmButton:
              "bg-yellow-500 hover:bg-yellow-600 text-black font-bold px-4 py-2 rounded",
            popup: "text-sm",
          },
        });
        return;
      }

      // ✅ STEP 2: Submit Delete Request
      const url = "/api/Requests";

      const requestBody = {
        request_type: "DELETE",
        target_table: "DdcSpec",
        target_pk_id: tool_spec_id,
        old_data: row,
        new_data: null,
        note: "Deleted from frontend",
      };

      console.log("🚀 Submitting DELETE request payload:", requestBody);

      const res = await fetch(url, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          ...(token && { Authorization: `Bearer ${token}` }),
        },
        body: JSON.stringify(requestBody),
      });

      const text = await res.text();
      let json: any = null;

      try {
        json = JSON.parse(text);
      } catch {
        console.error("⚠️ Response not JSON:", text);
      }

      if (!res.ok) {
        const message =
          json?.message || text || "Failed to submit delete request.";
        console.error("❌ Delete request failed:", message);
        await MySwal.fire({
          icon: "error",
          title: "Delete Failed",
          text: message,
          confirmButtonText: "OK",
          customClass: {
            confirmButton:
              "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
            popup: "text-sm",
          },
        });
        return;
      }

      console.log("✅ Delete request submitted:", json);

      await MySwal.fire({
        icon: "info",
        title: "Request Submitted",
        html: `Your delete request has been submitted.<br>Request ID: <b>${json.request_id}</b><br>It will be effective after admin approval.`,
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
          popup: "text-sm",
        },
      });

      // ✅ fetch pending ใหม่ หลังยิง request
      // await fetchPendingRequests();
      setReloadFlag((prev) => prev + 1);
    } catch (err: unknown) {
      console.error("🔥 Exception during delete request:", err);

      let errorMsg =
        "An unexpected error occurred while submitting delete request.";
      if (err && typeof err === "object" && "message" in err) {
        errorMsg = (err as any).message;
      }

      await MySwal.fire({
        icon: "error",
        title: "Error",
        text: errorMsg,
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
          popup: "text-sm",
        },
      });
    }
  };

  const handleOpenDetail = (rowData: SummaryDdcRow) => {
    setDetailData(rowData);
    setShowDetailModal(true);
  };

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

  const columns = useMemo<ColumnDef<SummaryDdcRow, any>[]>(
    () => [
      {
        header: "Tool Info",
        columns: [
          {
            id: "Tool Type",
            accessorKey: "tool_type",
            header: () => (
              <div className="flex items-center justify-center">
                Tool Type
                {renderDropdownFilter("tool_type")}
              </div>
            ),
          },
          {
            id: "Tool Name",
            accessorKey: "tool_name",
            header: () => (
              <div className="flex items-center justify-center">
                Tool Name
                {renderDropdownFilter("tool_name")}
              </div>
            ),
          },
          {
            id: "Position Type",
            accessorKey: "position_type",
            header: () => (
              <div className="flex items-center justify-center">
                Position Type
                {renderDropdownFilter("position_type")}
              </div>
            ),
          },
          {
            id: "Type Ref",
            accessorKey: "type_ref",
            header: () => (
              <div className="flex items-center justify-center">
                Type Ref
                {renderDropdownFilter("type_ref")}
              </div>
            ),
          },
          {
            id: "Tool Ref",
            accessorKey: "tool_ref",
            header: () => (
              <div className="flex items-center justify-center">
                Tool Ref
                {renderDropdownFilter("tool_ref")}
              </div>
            ),
          },
          {
            id: "Size Ref",
            accessorKey: "size_ref",
            header: () => (
              <div className="flex items-center justify-center">
                Size Ref
                {renderDropdownFilter("size_ref")}
              </div>
            ),
          },
          {
            id: "Axle Type",
            accessorKey: "axle_type",
            header: () => (
              <div className="flex items-center justify-center">
                Axle Type
                {renderDropdownFilter("axle_type")}
              </div>
            ),
          },
        ],
      },
      {
        header: "Wheel: Over All Dimension (mm)",
        columns: [
          {
            id: "A",
            accessorKey: "overall_a",
            header: () => (
              <div className="flex items-center justify-center gap-1">
                <span>A</span>
                {renderDropdownFilterSingle("overall_a", "Ov.")}
                {renderDropdownFilterSingle("tolerance_a", "Tol.")}
              </div>
            ),
            cell: ({ row }) =>
              `${row.original.overall_a} ± ${row.original.tolerance_a}`,
          },
          {
            id: "B",
            accessorKey: "overall_b",
            header: () => (
              <div className="flex items-center justify-center gap-1">
                <span>B</span>
                {renderDropdownFilterSingle("overall_b", "Ov.")}
                {renderDropdownFilterSingle("tolerance_b", "Tol.")}
              </div>
            ),
            cell: ({ row }) =>
              `${row.original.overall_b} ± ${row.original.tolerance_b}`,
          },
          {
            id: "C",
            accessorKey: "overall_c",
            header: () => (
              <div className="flex items-center justify-center gap-1">
                <span>C</span>
                {renderDropdownFilterSingle("overall_c", "Ov.")}
                {renderDropdownFilterSingle("tolerance_c", "Tol.")}
              </div>
            ),
            cell: ({ row }) =>
              `${row.original.overall_c} ± ${row.original.tolerance_c}`,
          },
        ],
      },
      {
        header: "Wheel: F-Shank (Inch)",
        columns: [
          {
            id: "F-Shank min",
            accessorKey: "f_shank_min",
            header: () => (
              <div className="flex items-center justify-center">
                Min{renderDropdownFilter("f_shank_min")}
              </div>
            ),
          },
          {
            id: "F-Shank max",
            accessorKey: "f_shank_max",
            header: () => (
              <div className="flex items-center justify-center">
                Max{renderDropdownFilter("f_shank_max")}
              </div>
            ),
          },
        ],
      },
      {
        header: "Bump to Bump (Inch)",
        columns: [
          {
            id: "B2B min",
            accessorKey: "b2b_min",
            header: () => (
              <div className="flex items-center justify-center">
                Min{renderDropdownFilter("b2b_min")}
              </div>
            ),
          },
          {
            id: "B2B max",
            accessorKey: "b2b_max",
            header: () => (
              <div className="flex items-center justify-center">
                Max{renderDropdownFilter("b2b_max")}
              </div>
            ),
          },
        ],
      },
      {
        header: "Head to Head (Inch)",
        columns: [
          {
            id: "H2H min",
            accessorKey: "h2h_min",
            header: () => (
              <div className="flex items-center justify-center">
                Max{renderDropdownFilter("h2h_min")}
              </div>
            ),
          },
          {
            id: "H2H max",
            accessorKey: "h2h_max",
            header: () => (
              <div className="flex items-center justify-center">
                Max{renderDropdownFilter("h2h_max")}
              </div>
            ),
          },
        ],
      },
      {
        header: "Chassis Span (Inch)",
        columns: [
          {
            id: "Chassis Span",
            accessorKey: "chassis_span",
            header: () => (
              <div className="flex items-center justify-center">
                Chassis Span{renderDropdownFilter("chassis_span")}
              </div>
            ),
          },
        ],
      },
      {
        header: "Pad",
        columns: [
          {
            id: "HST Pad",
            accessorKey: "hsT_pad",
            header: () => (
              <div className="flex items-center justify-center">
                HST{renderDropdownFilter("hsT_pad")}
              </div>
            ),
          },
          {
            id: "RIM Pad",
            accessorKey: "riM_pad",
            header: () => (
              <div className="flex items-center justify-center">
                RIM{renderDropdownFilter("riM_pad")}
              </div>
            ),
          },
          {
            id: "INNER Pad",
            accessorKey: "inneR_pad",
            header: () => (
              <div className="flex items-center justify-center">
                INNER{renderDropdownFilter("inneR_pad")}
              </div>
            ),
          },
          {
            id: "EXTRA RIM Pad",
            accessorKey: "extrA_RIM_pad",
            header: () => (
              <div className="flex items-center justify-center">
                EXTRA RIM{renderDropdownFilter("extrA_RIM_pad")}
              </div>
            ),
          },
        ],
      },
      {
        header: "Brass",
        columns: [
          {
            id: "HST Brass",
            accessorKey: "hsT_brass",
            header: () => (
              <div className="flex items-center justify-center">
                HST{renderDropdownFilter("hsT_brass")}
              </div>
            ),
          },
          {
            id: "RIM Brass",
            accessorKey: "riM_brass",
            header: () => (
              <div className="flex items-center justify-center">
                RIM{renderDropdownFilter("riM_brass")}
              </div>
            ),
          },
          {
            id: "INNER Brass",
            accessorKey: "inneR_brass",
            header: () => (
              <div className="flex items-center justify-center">
                INNER{renderDropdownFilter("inneR_brass")}
              </div>
            ),
          },
          {
            id: "EXTRA RIM Brass",
            accessorKey: "extrA_RIM_brass",
            header: () => (
              <div className="flex items-center justify-center">
                EXTRA RIM{renderDropdownFilter("extrA_RIM_brass")}
              </div>
            ),
          },
        ],
      },
      {
        header: "Pad Source",
        columns: [
          {
            id: "Pad Source Type",
            accessorKey: "pad_source",
            header: () => (
              <div className="flex items-center justify-center">
                Pad Source Type{renderDropdownFilter("pad_source")}
              </div>
            ),
          },
          {
            id: "Pad Source Key",
            accessorKey: "pad_source_key",
            header: () => (
              <div className="flex items-center justify-center">
                Pad Source Key{renderDropdownFilter("pad_source_key")}
              </div>
            ),
          },
        ],
      },
      {
        header: "Machine",
        columns: [
          {
            id: "Machine No",
            accessorKey: "machine_no",
            header: () => (
              <div className="flex items-center justify-center">
                Machine No{renderDropdownFilter("machine_no")}
              </div>
            ),
          },
          {
            id: "Machine Source Type",
            accessorKey: "machine_source",
            header: () => (
              <div className="flex items-center justify-center">
                Machine Source Type{renderDropdownFilter("machine_source")}
              </div>
            ),
          },
          {
            id: "Machine Source Key",
            accessorKey: "machine_source_key",
            header: () => (
              <div className="flex items-center justify-center">
                Machine Source Key{renderDropdownFilter("machine_source_key")}
              </div>
            ),
          },
        ],
      },
      {
        header: "Specification Info",
        columns: [
          {
            id: "Knurling Type",
            accessorKey: "knurling_type",
            header: () => (
              <div className="flex items-center justify-center">
                Knurling Type{renderDropdownFilter("knurling_type")}
              </div>
            ),
          },
        ],
      },
      {
        header: "Create",
        columns: [
          {
            id: "Create By",
            accessorKey: "create_by",
            header: () => (
              <div className="flex items-center justify-center">
                Create By{renderDropdownFilter("create_by")}
              </div>
            ),
          },
          {
            id: "Create At",
            accessorKey: "create_at",
            header: () => (
              <div className="flex items-center justify-center">Create At</div>
            ),
            cell: ({ getValue }) => formatDateTime(getValue()),
          },
        ],
      },
      {
        header: "Update",
        columns: [
          {
            id: "Update By",
            accessorKey: "update_by",
            header: () => (
              <div className="flex items-center justify-center">
                Update By{renderDropdownFilter("update_by")}
              </div>
            ),
          },
          {
            id: "Update At",
            accessorKey: "update_at",
            header: () => (
              <div className="flex items-center justify-center">Update At</div>
            ),
            cell: ({ getValue }) => formatDateTime(getValue()),
          },
        ],
      },
      {
        header: "Pending",
        columns: [
          {
            id: "Request Type",
            accessorKey: "pending_request",
            header: () => (
              <div className="flex items-center justify-center">
                Request Type{renderDropdownFilter("pending_request")}
              </div>
            ),
          },
        ],
      },
      {
        header: "Note",
        columns: [
          {
            id: "Description",
            accessorKey: "description",
            header: () => (
              <div className="flex items-center justify-center">
                Description{renderDropdownFilter("description")}
              </div>
            ),
          },
        ],
      },

      // {
      //   id: "Actions",
      //   header: "Actions",
      //   cell: ({ row }) => (
      //     <div className="flex gap-2 justify-center">
      //       <button
      //         onClick={() => handleOpenDetail(row.original)}
      //         className="px-2 py-1 text-xs border rounded text-gray-600 border-gray-600 hover:bg-gray-100"
      //       >
      //         Detail
      //       </button>
      //       <button
      //         onClick={() => {
      //           setEditingRow(row.original);
      //           setShowEditModal(true);
      //         }}
      //         className="px-2 py-1 text-xs border rounded text-blue-600 border-blue-600 hover:bg-blue-50"
      //       >
      //         Edit
      //       </button>
      //       <button
      //         onClick={() => {
      //           const cloned = {
      //             type_id:
      //               types.find((t) => t.label === row.original.tool_type)
      //                 ?.value ?? 13, //edit this row now static code.     //on prem db use 2002 , local db use 13
      //             tool_id:
      //               tools.find((t) => t.label === row.original.tool_name)
      //                 ?.value ?? 2, //edit this row now static code.     //on prem db use 1002 , local db use 2
      //             position_type_id:
      //               positions.find(
      //                 (t) => t.label === row.original.position_type
      //               )?.value ?? null,
      //             type_ref_id:
      //               typeRefs.find((t) => t.label === row.original.type_ref)
      //                 ?.value ?? 13, //edit this row now static code.     //on prem db use 2002 , local db use 13
      //             tool_ref_id:
      //               toolRefs.find((t) => t.label === row.original.tool_ref)
      //                 ?.value ?? 2, //edit this row now static code.     //on prem db use 1002 , local db use 2
      //             size_ref_id:
      //               sizeRefs.find((t) => t.label === row.original.size_ref)
      //                 ?.value ?? 12, //edit this row now static code.        //on prem db use 9 , local db use 12
      //             axle_type_id:
      //               axles.find((t) => t.label === row.original.axle_type)
      //                 ?.value ?? 56, //edit this row now static code.       //on prem db use 17 , local db use 56
      //             knurling_type:
      //               typeof row.original.knurling_type === "number"
      //                 ? row.original.knurling_type
      //                 : typeof row.original.knurling_type === "string"
      //                 ? Number(row.original.knurling_type) || 0
      //                 : 0,
      //             chassis_span_override: row.original.chassis_span ?? "",
      //           };

      //           console.log("🔥 Clone converted:", cloned);
      //           setCloneRow(cloned);
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
        id: "Actions",
        header: "Actions",
        cell: ({ row }) => {
          const canEdit = ["admin", "editor"].includes(userRole || "");

          return (
            <div className="flex gap-2 justify-center">
              {/* Always show Detail */}
              <button
                onClick={() => handleOpenDetail(row.original)}
                className="px-2 py-1 text-xs border rounded text-gray-600 border-gray-600 hover:bg-gray-100"
              >
                Detail
              </button>

              {canEdit && (
                <>
                  <button
                    onClick={() => {
                      setEditingRow(row.original);
                      setShowEditModal(true);
                    }}
                    className="px-2 py-1 text-xs border rounded text-blue-600 border-blue-600 hover:bg-blue-50"
                  >
                    Edit
                  </button>

                  <button
                    onClick={() => {
                      const cloned = {
                        type_id:
                          types.find((t) => t.label === row.original.tool_type)
                            ?.value ?? 13,
                        tool_id:
                          tools.find((t) => t.label === row.original.tool_name)
                            ?.value ?? 2,
                        position_type_id:
                          positions.find(
                            (t) => t.label === row.original.position_type
                          )?.value ?? null,
                        type_ref_id:
                          typeRefs.find(
                            (t) => t.label === row.original.type_ref
                          )?.value ?? 13,
                        tool_ref_id:
                          toolRefs.find(
                            (t) => t.label === row.original.tool_ref
                          )?.value ?? 2,
                        size_ref_id:
                          sizeRefs.find(
                            (t) => t.label === row.original.size_ref
                          )?.value ?? 12,
                        axle_type_id:
                          axles.find((t) => t.label === row.original.axle_type)
                            ?.value ?? 56,
                        knurling_type:
                          typeof row.original.knurling_type === "number"
                            ? row.original.knurling_type
                            : typeof row.original.knurling_type === "string"
                            ? Number(row.original.knurling_type) || 0
                            : 0,
                        chassis_span_override: row.original.chassis_span ?? "",
                      };

                      console.log("🔥 Clone converted:", cloned);
                      setCloneRow(cloned);
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
                </>
              )}
            </div>
          );
        },
      },
    ],
    [filterOptions, filterValues, dropdownOpen, data]
  );

  const table = useReactTable({
    data,
    columns,
    state: {
      columnFilters,
      columnVisibility,
      pagination,
    },
    onPaginationChange: setPagination,
    onColumnFiltersChange: setColumnFilters,
    onColumnVisibilityChange: setColumnVisibility,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
  });

  // const handleExportCSV = () => {
  //   const csv = Papa.unparse(data);
  //   const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
  //   const url = URL.createObjectURL(blob);
  //   const link = document.createElement("a");
  //   link.href = url;
  //   link.download = "SummaryDdc.csv";
  //   link.click();
  // };

  const handleExportXLSX = async () => {
    const exportDependencies: Record<string, string[]> = {
      overall_a: ["tolerance_a"],
      overall_b: ["tolerance_b"],
      overall_c: ["tolerance_c"],
    };

    const ignoredIds = ["Actions", "Other"];

    const idToGroupMap: Record<string, string> = {};
    const idToAccessorMap: Record<string, string> = {};

    const buildMaps = (cols: any[], parentHeader: string | null = null) => {
      cols.forEach((col) => {
        if (col.columns) {
          buildMaps(col.columns, col.header);
        } else {
          if (col.id && col.accessorKey) {
            idToGroupMap[col.id] = parentHeader ?? "Other";
            idToAccessorMap[col.id] = col.accessorKey;
          }
        }
      });
    };

    buildMaps(table.getAllColumns().map((col) => col.columnDef));

    let visibleIds = table
      .getVisibleLeafColumns()
      .map((col) => col.id)
      .filter((id) => !ignoredIds.includes(id));

    Object.entries(exportDependencies).forEach(([main, deps]) => {
      const mainId = Object.entries(idToAccessorMap).find(
        ([id, acc]) => acc === main
      )?.[0];
      if (mainId && visibleIds.includes(mainId)) {
        deps.forEach((dep) => {
          const depId = Object.entries(idToAccessorMap).find(
            ([id, acc]) => acc === dep
          )?.[0];
          if (depId && !visibleIds.includes(depId)) {
            visibleIds.push(depId);
          }
        });
      }
    });

    const exportData: Record<string, string>[] = data.map((row) => {
      const obj: Record<string, string> = {};

      visibleIds.forEach((id) => {
        let val: any = "";

        if (["A", "B", "C"].includes(id)) {
          const mainKey = idToAccessorMap[id];
          const tolKey = exportDependencies[mainKey]?.[0];
          const mainVal = row[mainKey];
          const tolVal = row[tolKey];
          if (mainVal !== undefined && tolVal !== undefined) {
            val = `${mainVal} ± ${tolVal}`;
          } else {
            val = mainVal ?? "";
          }
        } else {
          const accessor = idToAccessorMap[id];
          val = row[accessor];
        }

        if (val === undefined || val === null) val = "";
        if (typeof val === "boolean") val = val ? "✔" : "-";
        obj[id] = String(val);
      });

      return obj;
    });

    const workbook = new ExcelJS.Workbook();
    const sheet = workbook.addWorksheet("SummaryDdc");

    const groupHeaders: string[] = [];
    const subHeaders: string[] = [];

    let prevGroup = "";
    visibleIds.forEach((id) => {
      const group = idToGroupMap[id] ?? "Other";
      groupHeaders.push(group === prevGroup ? "" : group);
      subHeaders.push(id);
      prevGroup = group;
    });

    groupHeaders.forEach((header, colIdx) => {
      const cell = sheet.getCell(1, colIdx + 1);
      cell.value = header || null;
      cell.font = { name: "Arial", size: 10, bold: true };
      cell.alignment = { vertical: "middle", horizontal: "center" };
      cell.border = {
        top: { style: "thin" },
        left: { style: "thin" },
        bottom: { style: "thin" },
        right: { style: "thin" },
      };
    });

    subHeaders.forEach((header, colIdx) => {
      const cell = sheet.getCell(2, colIdx + 1);
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

    let startIdx = 0;
    while (startIdx < groupHeaders.length) {
      let endIdx = startIdx;
      while (
        endIdx + 1 < groupHeaders.length &&
        groupHeaders[endIdx + 1] === ""
      ) {
        endIdx++;
      }
      if (endIdx > startIdx && groupHeaders[startIdx]) {
        sheet.mergeCells(1, startIdx + 1, 1, endIdx + 1);
      }
      startIdx = endIdx + 1;
    }

    exportData.forEach((dataRow, rowIdx) => {
      const excelRowIdx = rowIdx + 3;
      const pending = (data[rowIdx] as any)?.pending_request;

      let fillColor: string | undefined;
      let fontColor: string | undefined;

      if (pending === "UPDATE") {
        fillColor = "FEFCE8"; // bg-yellow-50
        fontColor = "854D0E"; // text-yellow-900
      } else if (pending === "DELETE") {
        fillColor = "FEF2F2"; // bg-red-50
        fontColor = "991B1B"; // text-red-800
      }

      visibleIds.forEach((id, colIdx) => {
        const cell = sheet.getCell(excelRowIdx, colIdx + 1);
        cell.value = dataRow[id];
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

    sheet.columns.forEach((col) => {
      col.width = 15;
    });

    const buffer = await workbook.xlsx.writeBuffer();
    const blob = new Blob([buffer], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    });
    saveAs(blob, "SummaryDdc.xlsx");
  };

  // 👇 อยู่ใน React Component นะ อย่าลืม
  const renderDropdownFilter = (columnId: string) => {
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

    // ✅ เช็คว่า filter ใช้งานอยู่หรือไม่
    const isActive = (filterValues[columnId] ?? []).length > 0;

    useEffect(() => {
      if (filterValues[columnId]) {
        setTempFilterValues((prev) => ({
          ...prev,
          [columnId]: [...filterValues[columnId]],
        }));
      }
    }, [dropdownOpen[columnId]]);

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

    const handleTempToggle = (val: string | null) => {
      const stringified = val ?? "null";
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

            {/* ✅ Scrollable list */}
            <div className="max-h-48 overflow-y-auto mt-1">
              {filterOptions[columnId]
                ?.filter((val) =>
                  (val ?? "(Blanks)").toLowerCase().includes(searchText)
                )
                .map((val) => {
                  const valueToSend = val ?? "null";
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

            {/* ✅ OK / Cancel */}
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
                    [columnId]: tempFilterValues[columnId] || [],
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
  const renderDropdownFilterSingle = (columnId: string, label?: string) => {
    const [searchText, setSearchText] = useState("");
    const buttonRef = useRef<HTMLButtonElement>(null);
    const dropdownRef = useRef<HTMLDivElement>(null);
    const [dropdownPosition, setDropdownPosition] = useState<{
      top: number;
      left: number;
      width: number;
    } | null>(null);

    const [tempFilterValues, setTempFilterValues] = useState<string[]>([]);

    const options = filterOptions[columnId] || [];

    // ✅ เช็คว่า filter ใช้งานอยู่หรือไม่
    const isActive = (filterValues[columnId] ?? []).length > 0;

    useEffect(() => {
      if (filterValues[columnId]) {
        setTempFilterValues([...filterValues[columnId]]);
      }
    }, [dropdownOpen[columnId]]);

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
      return () => {
        document.removeEventListener("mousedown", handleClickOutside);
      };
    }, [columnId]);

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

    const handleTempToggle = (val: string | null) => {
      const stringified = val ?? "null";
      setTempFilterValues((prev) => {
        const current = new Set(prev);
        if (current.has(stringified)) current.delete(stringified);
        else current.add(stringified);
        return Array.from(current);
      });
    };

    const handleSelectAll = () => {
      if (tempFilterValues.length === options.length) {
        setTempFilterValues([]);
      } else {
        setTempFilterValues([...options.map((v) => v ?? "null")]);
      }
    };

    const handleOK = () => {
      setFilterValues((prev) => ({
        ...prev,
        [columnId]: tempFilterValues,
      }));
      setDropdownOpen((prev) => ({ ...prev, [columnId]: false }));
    };

    return (
      <>
        <button
          ref={buttonRef}
          onClick={handleOpenDropdown}
          className={`ml-1 px-2 py-0.5 border text-xs rounded shadow flex items-center gap-1 transition-colors duration-150 ${
            isActive
              ? "bg-blue-600 border-blue-700 text-white hover:bg-blue-700"
              : "bg-white border-gray-300 text-gray-700 hover:bg-gray-100"
          }`}
        >
          {label && <span>{label}</span>}▼
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
            <input
              type="text"
              value={searchText}
              placeholder="Search..."
              className="w-full mb-2 px-2 py-1 border rounded text-sm"
              onChange={(e) => setSearchText(e.target.value.toLowerCase())}
            />

            <label className="block text-xs font-medium px-1 py-0.5 text-left">
              <input
                type="checkbox"
                className="mr-2"
                checked={tempFilterValues.length === options.length}
                onChange={handleSelectAll}
              />
              (Select All)
            </label>

            <div className="max-h-48 overflow-y-auto mt-1">
              {options
                ?.filter((val) =>
                  (val ?? "(Blanks)").toLowerCase().includes(searchText)
                )
                .map((val) => {
                  const valueToSend = val ?? "null";
                  return (
                    <label
                      key={valueToSend}
                      className="block px-1 py-0.5 text-sm text-left"
                    >
                      <input
                        type="checkbox"
                        className="mr-2"
                        checked={tempFilterValues.includes(valueToSend)}
                        onChange={() => handleTempToggle(val)}
                      />
                      {val}
                    </label>
                  );
                })}
            </div>

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
                className="text-xs border px-2 py-1 rounded bg-blue-600 text-white hover:bg-blue-700"
                onClick={handleOK}
              >
                OK
              </button>
            </div>
          </div>
        )}
      </>
    );
  };

  return (
    <div className="space-y-4 relative">
      {/* ✅ 3 Cards: Pending by Request Type */}
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
                    (r) => r.pending_request?.toUpperCase() === "UPDATE"
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
                    (r) => r.pending_request?.toUpperCase() === "DELETE"
                  ).length
                }
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Header with Action Buttons */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4">
        <div className="flex flex-wrap gap-3 items-center justify-between">
          {/* Left side - Secondary actions */}
          <div className="flex flex-wrap gap-2">
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
              onClick={() => setShowDefModal(true)}
              className="inline-flex items-center px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg shadow-sm hover:bg-gray-50 hover:shadow-md active:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-all duration-200"
            >
              Show Definitions
            </button>

            <div ref={dropdownRef} className="relative">
              <button
                ref={columnsButtonRef}
                className="inline-flex items-center px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg shadow-sm hover:bg-gray-50 hover:shadow-md active:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-all duration-200"
                onClick={() => setShowColumnDropdown((prev) => !prev)}
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
                createPortal(
                  <div
                    className="fixed z-[9999] w-64 bg-white border border-gray-200 rounded-lg shadow-lg max-h-80 overflow-hidden transition-opacity duration-200"
                    style={{
                      top: columnsDropdownPos?.top ?? 0,
                      left: columnsDropdownPos?.left ?? 0,
                      opacity: columnsDropdownPos ? 1 : 0,
                      visibility: columnsDropdownPos ? "visible" : "hidden",
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

          {/* Right side - Primary action */}
          {/* <button
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
            Add New Tool
          </button> */}
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
              Add New Tool
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

      {/* Pagination UI */}
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
      {showAddModal && (
        <DdcSpecAddToolModal
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
            setFilterValues({});
            setReloadFlag((prev) => prev + 1); // 👈 trigger reload
          }}
        />
      )}

      {showEditModal && editingRow && (
        <DdcSpecEditToolModal
          tool_spec_id={editingRow.tool_spec_id}
          ref_key_id={editingRow.ref_key_id}
          initialChassisSpanOverride={editingRow.chassis_span ?? 0}
          initialDescription={editingRow.description} // ✅ ADD THIS
          displayInfo={{
            tool_type: editingRow.tool_type || "(Blanks)",
            tool_name: editingRow.tool_name || "(Blanks)",
            position_type: editingRow.position_type || "(Blanks)",
            type_ref: editingRow.type_ref || "(Blanks)",
            tool_ref: editingRow.tool_ref || "(Blanks)",
            size_ref: editingRow.size_ref || "(Blanks)",
            axle_type: editingRow.axle_type || "(Blanks)",
          }}
          onClose={() => setShowEditModal(false)}
          onSubmitSuccess={() => {
            // fetchPendingRequests();
            setShowEditModal(false);
            setColumnFilters([...columnFilters]); // ✅ trick: trigger useEffect reload
            setReloadFlag((prev) => prev + 1); // 👈 trigger reload
          }}
        />
      )}

      <ToolDdcDetailModal
        showDetailModal={showDetailModal}
        setShowDetailModal={setShowDetailModal}
        detailData={detailData}
        definitions1={definitions1}
        definitions2={definitions2}
      />
      <DefDdcModal
        showDefModal={showDefModal}
        setShowDefModal={setShowDefModal}
        // definitions1={definitions1}
        // definitions2={definitions2}
      />
    </div>
  );
};

export default SummaryDdcTable;
