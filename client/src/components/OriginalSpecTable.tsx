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

import OriginalSpecAddToolModal from "./OriginalSpecAddToolModal";
import OriginalSpecEditToolModal from "./OriginalSpecEditToolModal";

import { PlusCircle, Edit3, Trash2 } from "lucide-react";

import Swal from "sweetalert2";
import withReactContent from "sweetalert2-react-content";
const MySwal = withReactContent(Swal);

import definitions1 from "../assets/definitions1.png";
import definitions2 from "../assets/definitions2.png";
import ToolDetailModal from "./ToolDetailModal";

import DefModal from "./DefModal";

import type { Option } from "./OriginalSpecAddToolModal";

import ExcelJS from "exceljs";
import { saveAs } from "file-saver"; // ใช้ช่วยดาวน์โหลดไฟล์ blob

import DateTimeFilter from "./DateTimeFilter"; // ✅ เพิ่ม

import ToolKeyOriginalPhotoModal from "./ToolKeyOriginalPhotoModal";

export interface OriginalSpecRow {
  [key: string]: string | number | boolean | null | undefined;
  tool_key_id: number; // (optional, if already provided by backend)
  tool_ref_spec_id: number;
  tool_key_id_original: number; // (optional, if already provided by backend)
  tool_type: string;
  tool_name: string;
  size_ref: string;
  axle_type: string;
  overall_a: number | string;
  overall_b: number | string;
  overall_c: number | string;
  tolerance_a: number | string;
  tolerance_b: number | string;
  tolerance_c: number | string;
  f_shank_min: number | string;
  f_shank_max: number | string;
  b2b_min: number | string;
  b2b_max: number | string;
  h2h_min: number | string;
  h2h_max: number | string;
  chassis_span: string;
  chassis_span1: number | string;
  chassis_span2: number | string;
  HST_pad: string;
  RIM_pad: string;
  INNER_pad: string;
  EXTRA_RIM_pad: string;
  HST_brass: string;
  RIM_brass: string;
  INNER_brass: string;
  EXTRA_RIM_brass: string;
  pad_source_key: string;
  machine_no: string;
  machine_source_key: string;
  source: string;
  description?: string; // ✅ เพิ่ม
  is_original_spec: boolean;
  // pending_status?: "UPDATE" | "DELETE" | null;

  image_url?: string | null;
  pic_after_hst_file_name?: string | null;

  pending_request: string;
  create_by: string;
  create_at: string;
  update_by?: string;
  update_at?: string;
}

export interface OriginalSpecRowInput {
  type_id: number | null;
  tool_id: number | null;
  size_ref_id: number | null;
  axle_type_id: number | null;
  knurling_type: number | null;
  is_original_spec: number;
  overall_a: string | number;
  overall_b: string | number;
  overall_c: string | number;
  tolerance_a: string | number;
  tolerance_b: string | number;
  tolerance_c: string | number;
  f_shank_min: string | number;
  f_shank_max: string | number;
  b2b_min: string | number;
  b2b_max: string | number;
  h2h_min: string | number;
  h2h_max: string | number;
  chassis_span1: string | number;
  chassis_span2: string | number;
  source: string;
  description: string | null; // ✅ เพิ่ม
}

// const ImageCell: React.FC<{
//   url?: string | null;
//   fileName?: string | null;
//   title?: string;
//   onOpen?: () => void; // ✅ เพิ่ม
// }> = ({ url, fileName, title, onOpen }) => {
//   const [failed, setFailed] = React.useState(false);

//   const NO_IMAGE = `data:image/svg+xml;utf8,${encodeURIComponent(
//     `<svg xmlns="http://www.w3.org/2000/svg" width="96" height="96">
//       <rect width="100%" height="100%" fill="#f3f4f6"/>
//       <text x="50%" y="50%" dominant-baseline="middle" text-anchor="middle"
//         font-family="Arial" font-size="12" fill="#9ca3af">No photo</text>
//     </svg>`
//   )}`;

//   const src = !failed && url ? url : NO_IMAGE;

//   const handleClick = () => {
//     if (onOpen) {
//       onOpen(); // ✅ เปิดโมดัลจัดการรูป
//     } else if (url && !failed) {
//       window.open(url, "_blank", "noopener,noreferrer"); // fallback zoom
//     }
//   };

//   const handleKey = (e: React.KeyboardEvent) => {
//     if (e.key === "Enter" || e.key === " ") {
//       e.preventDefault();
//       handleClick();
//     }
//   };

//   return (
//     <div
//       className="group relative w-24 h-24 mx-auto rounded-md overflow-hidden border bg-gray-50 flex items-center justify-center"
//       role="button" // ✅ เข้าถึงด้วยคีย์บอร์ด
//       tabIndex={0}
//       onClick={handleClick}
//       onKeyDown={handleKey}
//       title={title ?? fileName ?? ""}
//     >
//       <img
//         src={src}
//         alt={fileName ?? "after-hst"}
//         loading="lazy"
//         onError={() => setFailed(true)}
//         className="w-full h-full object-cover"
//       />
//       {/* ✅ Hover overlay บอกว่าแก้ไขได้ */}
//       <div className="absolute inset-0 bg-black/0 group-hover:bg-black/30 transition-colors" />
//       <div className="absolute bottom-1 left-1 right-1 opacity-0 group-hover:opacity-100 transition-opacity">
//         <div className="text-[10px] leading-tight text-white bg-black/60 rounded px-1 py-0.5 text-center">
//           {url && !failed ? "Edit / Replace Photo" : "Upload Photo"}
//         </div>
//       </div>
//     </div>
//   );
// };

const ImageCell: React.FC<{
  url?: string | null;
  fileName?: string | null;
  title?: string;
  onOpen?: () => void;
}> = ({ url, fileName, title, onOpen }) => {
  const [failed, setFailed] = React.useState(false);

  const NO_IMAGE = `data:image/svg+xml;utf8,${encodeURIComponent(
    `<svg xmlns="http://www.w3.org/2000/svg" width="96" height="96">
      <rect width="100%" height="100%" fill="#f3f4f6"/>
      <text x="50%" y="50%" dominant-baseline="middle" text-anchor="middle"
        font-family="Arial" font-size="12" fill="#9ca3af">No photo</text>
    </svg>`
  )}`;

  const src = !failed && url ? url : NO_IMAGE;

  const handleClick = () => {
    if (onOpen) onOpen();
    else if (url && !failed) window.open(url, "_blank", "noopener,noreferrer");
  };

  const handleKey = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" || e.key === " ") {
      e.preventDefault();
      handleClick();
    }
  };

  return (
    <div
      className="group relative w-24 h-24 mx-auto rounded-md overflow-hidden border bg-gray-50 flex items-center justify-center"
      role="button"
      tabIndex={0}
      onClick={handleClick}
      onKeyDown={handleKey}
      title={title ?? fileName ?? ""}
    >
      <img
        src={src}
        alt={fileName ?? "before-hst"}
        loading="lazy"
        onError={() => setFailed(true)}
        className="w-full h-full object-cover"
      />
      <div className="absolute inset-0 bg-black/0 group-hover:bg-black/30 transition-colors" />
      <div className="absolute bottom-1 left-1 right-1 opacity-0 group-hover:opacity-100 transition-opacity">
        <div className="text-[10px] leading-tight text-white bg-black/60 rounded px-1 py-0.5 text-center">
          {url && !failed ? "Photo" : "Photo"}
        </div>
      </div>
    </div>
  );
};

const OriginalSpecTable: React.FC = () => {
  const [data, setData] = useState<OriginalSpecRow[]>([]);
  const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([]);
  // const [columnVisibility, setColumnVisibility] = useState<VisibilityState>({});
  const [columnVisibility, setColumnVisibility] = useState<VisibilityState>({
    tolerance_a: false,
    tolerance_b: false,
    tolerance_c: false,
  });

  const [pagination, setPagination] = useState({ pageIndex: 0, pageSize: 15 });

  const [search, setSearch] = useState("");
  const [showColumnDropdown, setShowColumnDropdown] = useState(false);

  const dropdownRefs = useRef<HTMLDivElement>(null);
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

  const [editingRow, setEditingRow] = useState<OriginalSpecRow | null>(null);
  const [showEditModal, setShowEditModal] = useState(false);

  const [showDefModal, setShowDefModal] = useState(false);

  const [showDetailModal, setShowDetailModal] = useState(false);
  const [detailData, setDetailData] = useState<OriginalSpecRow | null>(null);

  const [cloneRow, setCloneRow] = useState<OriginalSpecRowInput | null>(null);
  const [fromClone, setFromClone] = useState(false);

  const [types, setTypes] = useState<Option[]>([]);
  const [tools, setTools] = useState<Option[]>([]);
  const [sizes, setSizes] = useState<Option[]>([]);
  const [axles, setAxles] = useState<Option[]>([]);

  const [reloadFlag, setReloadFlag] = useState(0);

  const [datetimeFilters, setDatetimeFilters] = useState({
    created_at_start: "",
    created_at_end: "",
    updated_at_start: "",
    updated_at_end: "",
  });

  const [userRole, setUserRole] = useState<string | null>(null);

  const [showPhotoModal, setShowPhotoModal] = useState(false);
  const [photoCtx, setPhotoCtx] = useState<{
    open: boolean;
    originalKeyId: number | null;
    url: string | null;
    fileName: string | null;
  }>(() => ({ open: false, originalKeyId: null, url: null, fileName: null }));

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
  //       "/api/Requests/pending?target_table=OriginalSpec",
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

  // const prevQueryRef = useRef("");
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

    const queryString = query.toString();

    //   if (prevQueryRef.current === queryString) return; // ❌ ไม่ fetch ถ้า query เหมือนเดิม
    // prevQueryRef.current = queryString;

    // ✅ Log ทั้งแบบ string และ object
    console.log("🔍 filterValues object:", filterValues);
    console.log("🌐 Query string sent to API:", queryString);

    fetch(`/api/ResolvedOriginals/spec?${queryString}`)
      .then((res) => res.json())
      .then((result: OriginalSpecRow[]) => {
        setData(result);
        const keys = Object.keys(result[0] || {});
        const newOptions: { [key: string]: string[] } = {};
        keys.forEach((key) => {
          newOptions[key] = Array.from(
            new Set(result.map((item) => String(item[key] ?? "")))
          ).sort();
        });
        setFilterOptions(newOptions);
      })
      .catch((err) => console.error("Error fetching data:", err));
  }, [filterValues, datetimeFilters, reloadFlag]);

  const hasFetchedOption = useRef(false);

  useEffect(() => {
    if (hasFetchedOption.current) return; // ❌ block รอบที่ 2
    hasFetchedOption.current = true;
    Promise.all([
      fetch("/api/TypeModels").then((r) => r.json()),
      fetch("/api/Tools").then((r) => r.json()),
      fetch("/api/SizeRefs").then((r) => r.json()),
      fetch("/api/AxleTypes").then((r) => r.json()),
    ]).then(([typesData, toolsData, sizesData, axlesData]) => {
      setTypes(
        typesData.map((d: any) => ({
          value: d.type_id ?? null,
          label: d.type_name ?? "(Blanks)",
        }))
      );
      setTools(
        toolsData.map((d: any) => ({
          value: d.tool_id ?? null,
          label: d.tool_name ?? "(Blanks)",
        }))
      );
      setSizes(
        sizesData.map((d: any) => ({
          value: d.size_ref_id ?? null,
          label: d.size_ref ?? "(Blanks)",
        }))
      );
      setAxles(
        axlesData.map((d: any) => ({
          value: d.axle_type_id ?? null,
          label: d.axle_type ?? "(Blanks)",
        }))
      );
    });
  }, []);

  // useEffect(() => {
  //   const query = new URLSearchParams();
  //   Object.entries(filterValues).forEach(([key, values]) => {
  //     if (values.length > 0) query.append(key, values.join(","));
  //   });

  //   fetch(
  //     `/api/ResolvedOriginals/spec?${query.toString()}`
  //   )
  //     .then((res) => res.json())
  //     .then((result: OriginalSpecRow[]) => {
  //       const merged = result.map((row) => {
  //         const rawStatus = pendingMap.get(Number(row.tool_ref_spec_id));
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
  //           new Set(result.map((item) => String(item[key] ?? "")))
  //         ).sort();
  //       });
  //       setFilterOptions(newOptions);
  //     })
  //     .catch((err) => console.error("Error fetching data:", err));
  // }, [filterValues, pendingMap]);

  // useEffect(() => {
  //   fetchPendingRequests();
  // }, []);

  // const handleDelete = async (row: OriginalSpecRow) => {
  //   console.log("🗑️ Deleting original spec row:", row);

  //   const escapeHTML = (str: string | null | undefined) =>
  //     (str ?? "-").replace(/</g, "&lt;").replace(/>/g, "&gt;");

  //   const result = await MySwal.fire({
  //     title: "Are you sure?",
  //     html: `
  //     <table style="text-align:left; font-size:18px; line-height:1.6;">
  //       <tr><td><b>Tool Type:</b></td><td>${escapeHTML(row.tool_type)}</td></tr>
  //       <tr><td><b>Tool Name:</b></td><td>${escapeHTML(row.tool_name)}</td></tr>
  //       <tr><td><b>Size Ref:</b></td><td>${escapeHTML(row.size_ref)}</td></tr>
  //       <tr><td><b>Axle Type:</b></td><td>${escapeHTML(row.axle_type)}</td></tr>
  //       <tr><td><b>Original Spec:</b></td><td>${
  //         row.is_original_spec ? "✔" : "-"
  //       }</td></tr>
  //       <tr><td><b>Source:</b></td><td>${escapeHTML(row.source)}</td></tr>
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

  //   if (!result.isConfirmed) {
  //     console.log("❌ User cancelled deletion.");
  //     return;
  //   }

  //   const { tool_key_id_original, tool_ref_spec_id } = row;

  //   if (!tool_key_id_original || !tool_ref_spec_id) {
  //     await MySwal.fire({
  //       icon: "error",
  //       title: "Missing IDs",
  //       text: "tool_key_id or tool_ref_spec_id is missing.",
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
  //     // ✅ Step 0: Check usage
  //     const checkUrl = `/api/ToolRefSpecs/check-original-usage?tool_ref_spec_id=${tool_ref_spec_id}`;
  //     const usageCheck = await fetch(checkUrl, {
  //       headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  //     });

  //     if (usageCheck.ok) {
  //       const text = await usageCheck.text();
  //       let json = null;

  //       try {
  //         json = JSON.parse(text);
  //       } catch {
  //         console.error("⚠️ Response not JSON:", text);
  //       }

  //       if (json?.used) {
  //         await MySwal.fire({
  //           title: "Cannot Delete",
  //           text: "This original tool is still being used in Tool Reference or Tool Specs.",
  //           icon: "warning",
  //           confirmButtonText: "OK",
  //           customClass: {
  //             confirmButton:
  //               "bg-yellow-500 hover:bg-yellow-600 text-black font-bold px-4 py-2 rounded",
  //             popup: "text-sm",
  //           },
  //         });
  //         return;
  //       }
  //     }

  //     // ✅ Step 1: Delete ToolRefSpecs
  //     const refRes = await fetch(
  //       `/api/ToolRefSpecs/${tool_ref_spec_id}`,
  //       {
  //         method: "DELETE",
  //         headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  //       }
  //     );

  //     if (!refRes.ok) {
  //       const text = await refRes.text();
  //       let errorMsg = text;

  //       try {
  //         const json = JSON.parse(text);
  //         errorMsg = json?.message || text;
  //       } catch {
  //         console.error("⚠️ Response not JSON:", text);
  //       }

  //       if (refRes.status === 409) {
  //         await MySwal.fire({
  //           title: "Cannot Delete",
  //           text: errorMsg,
  //           icon: "warning",
  //           confirmButtonText: "OK",
  //           customClass: {
  //             confirmButton:
  //               "bg-yellow-500 hover:bg-yellow-600 text-black font-bold px-4 py-2 rounded",
  //             popup: "text-sm",
  //           },
  //         });
  //       } else {
  //         await MySwal.fire({
  //           icon: "error",
  //           title: "Delete Failed",
  //           text: errorMsg,
  //           confirmButtonText: "OK",
  //           customClass: {
  //             confirmButton:
  //               "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
  //             popup: "text-sm",
  //           },
  //         });
  //       }
  //       return;
  //     }

  //     // ✅ Step 2: Check for other ToolRefSpecs
  //     const refListRes = await fetch(
  //       `/api/ToolRefSpecs/byToolKeyId/${tool_key_id_original}`,
  //       {
  //         headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  //       }
  //     );

  //     if (refListRes.ok) {
  //       const text = await refListRes.text();
  //       let refList = [];

  //       try {
  //         refList = JSON.parse(text);
  //       } catch {
  //         console.error("⚠️ Response not JSON:", text);
  //       }

  //       const otherRefs = refList.filter(
  //         (r: any) => r.tool_ref_spec_id !== tool_ref_spec_id
  //       );

  //       if (otherRefs.length === 0) {
  //         // ✅ Step 3: Delete ToolKeyAlls
  //         await fetch(
  //           `/api/ToolKeyAlls/byOriginalKey/${tool_key_id_original}`,
  //           {
  //             method: "DELETE",
  //             headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  //           }
  //         );

  //         // ✅ Step 4: Delete ToolKeyOriginals
  //         const originRes = await fetch(
  //           `/api/ToolKeyOriginals/${tool_key_id_original}`,
  //           {
  //             method: "DELETE",
  //             headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  //           }
  //         );

  //         if (!originRes.ok) {
  //           const text = await originRes.text();
  //           let errorMsg = text;

  //           try {
  //             const json = JSON.parse(text);
  //             errorMsg = json?.message || text;
  //           } catch {
  //             console.error("⚠️ Response not JSON:", text);
  //           }

  //           await MySwal.fire({
  //             icon: "error",
  //             title: "Delete Failed",
  //             text: errorMsg,
  //             confirmButtonText: "OK",
  //             customClass: {
  //               confirmButton:
  //                 "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
  //               popup: "text-sm",
  //             },
  //           });
  //           return;
  //         }
  //       }
  //     }

  //     await MySwal.fire({
  //       icon: "success",
  //       title: "Deleted!",
  //       text: "The original spec and related records have been deleted.",
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
  //         popup: "text-sm",
  //       },
  //     });

  //     setFilterValues({});
  //   } catch (err: unknown) {
  //     console.error("🔥 Exception during deletion:", err);

  //     let errorMsg = "An unexpected error occurred while deleting.";
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

  // const handleDelete = async (row: OriginalSpecRow) => {
  //   console.log("🗑️ Deleting original spec row:", row);

  //   const escapeHTML = (str: string | null | undefined) =>
  //     (str ?? "-").replace(/</g, "&lt;").replace(/>/g, "&gt;");

  //   const result = await MySwal.fire({
  //     title: "Are you sure?",
  //     html: `
  //     <table style="text-align:left; font-size:18px; line-height:1.6;">
  //       <tr><td><b>Tool Type:</b></td><td>${escapeHTML(row.tool_type)}</td></tr>
  //       <tr><td><b>Tool Name:</b></td><td>${escapeHTML(row.tool_name)}</td></tr>
  //       <tr><td><b>Size Ref:</b></td><td>${escapeHTML(row.size_ref)}</td></tr>
  //       <tr><td><b>Axle Type:</b></td><td>${escapeHTML(row.axle_type)}</td></tr>
  //       <tr><td><b>Original Spec:</b></td><td>${
  //         row.is_original_spec ? "✔" : "-"
  //       }</td></tr>
  //       <tr><td><b>Source:</b></td><td>${escapeHTML(row.source)}</td></tr>
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

  //   const { tool_ref_spec_id } = row;

  //   if (!tool_ref_spec_id) {
  //     await MySwal.fire({
  //       icon: "error",
  //       title: "Missing ID",
  //       text: "tool_ref_spec_id is missing.",
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
  //     // ✅ STEP 1: Check usage
  //     const checkUrl = `/api/ToolRefSpecs/check-original-usage?tool_ref_spec_id=${tool_ref_spec_id}`;
  //     const usageCheck = await fetch(checkUrl, {
  //       headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  //     });

  //     if (usageCheck.ok) {
  //       const text = await usageCheck.text();
  //       let json = null;

  //       try {
  //         json = JSON.parse(text);
  //       } catch {
  //         console.error("⚠️ Response not JSON:", text);
  //       }

  //       if (json?.used) {
  //         await MySwal.fire({
  //           title: "Cannot Delete",
  //           text: "This original tool is still being used in Tool Reference or Tool Specs.",
  //           icon: "warning",
  //           confirmButtonText: "OK",
  //           customClass: {
  //             confirmButton:
  //               "bg-yellow-500 hover:bg-yellow-600 text-black font-bold px-4 py-2 rounded",
  //             popup: "text-sm",
  //           },
  //         });
  //         return;
  //       }
  //     }

  //     // ✅ STEP 2: Create Request payload
  //     const url = "/api/Requests";

  //     const requestBody = {
  //       request_type: "DELETE",
  //       target_table: "OriginalSpec",
  //       target_pk_id: tool_ref_spec_id,
  //       old_data: row,
  //       new_data: null,
  //       note: "Deleted from frontend",
  //     };

  //     const request = new Request(url, {
  //       method: "POST",
  //       headers: {
  //         "Content-Type": "application/json",
  //         Authorization: `Bearer ${token}`,
  //       },
  //       body: JSON.stringify(requestBody),
  //     });

  //     const response = await fetch(request);

  //     if (!response.ok) {
  //       const errorData = await response.json();
  //       console.error("Error:", errorData);
  //       await MySwal.fire({
  //         icon: "error",
  //         title: "Error",
  //         text:
  //           errorData.message ||
  //           "Failed to submit delete request for OriginalSpec.",
  //         confirmButtonText: "OK",
  //         customClass: {
  //           confirmButton:
  //             "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
  //           popup: "text-sm",
  //         },
  //       });
  //       return;
  //     }

  //     const resultData = await response.json();

  //     await MySwal.fire({
  //       icon: "info",
  //       title: "Request Submitted",
  //       html: `Your delete request has been submitted.<br>Request ID: <b>${resultData.request_id}</b><br>It will be effective after admin approval.`,
  //       confirmButtonText: "OK",
  //       customClass: {
  //         confirmButton:
  //           "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
  //         popup: "text-sm",
  //       },
  //     });

  //     setFilterValues({});
  //   } catch (err: unknown) {
  //     console.error("🔥 Exception during deletion:", err);

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

  const handleDelete = async (row: OriginalSpecRow) => {
    console.log("🗑️ Deleting original spec row:", row);
    console.log("🗑️ Deleting tool_key_id:", row.tool_key_id);

    const escapeHTML = (str: string | null | undefined) =>
      (str ?? "-").replace(/</g, "&lt;").replace(/>/g, "&gt;");

    const result = await MySwal.fire({
      title: "Are you sure?",
      html: `
      <table style="text-align:left; font-size:18px; line-height:1.6;">
        <tr><td><b>Tool Type:</b></td><td>${escapeHTML(row.tool_type)}</td></tr>
        <tr><td><b>Tool Name:</b></td><td>${escapeHTML(row.tool_name)}</td></tr>
        <tr><td><b>Size Ref:</b></td><td>${escapeHTML(row.size_ref)}</td></tr>
        <tr><td><b>Axle Type:</b></td><td>${escapeHTML(row.axle_type)}</td></tr>
        <tr><td><b>Original Spec:</b></td><td>${
          row.is_original_spec ? "✔" : "-"
        }</td></tr>
        <tr><td><b>Source:</b></td><td>${escapeHTML(row.source)}</td></tr>
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

    const { tool_ref_spec_id, tool_key_id } = row;

    if (!tool_ref_spec_id || !tool_key_id || tool_key_id <= 0) {
      await MySwal.fire({
        icon: "error",
        title: "Missing ID",
        text: "tool_ref_spec_id or tool_key_id is missing or invalid.",
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
      // ✅ STEP 1: Check usage
      const endpoints = [
        {
          url: `/api/ToolRefSpecs/check-toolspec?tool_ref_spec_id=${tool_ref_spec_id}`,
          label: "ToolSpecs",
        },
        {
          url: `/api/ToolRefSpecs/check-toolmachinemap?original_tool_key_id=${tool_key_id}`,
          label: "ToolMachineMap",
        },
        {
          url: `/api/ToolRefSpecs/check-toolpadmap?original_tool_key_id=${tool_key_id}`,
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

      console.log("USAGE CHECK RESPONSES:", responses);

      const reasons = responses
        .filter((usage) => usage.used)
        .map((usage) => `- Used in ${usage.label}`);

      if (reasons.length > 0) {
        await MySwal.fire({
          title: "Cannot Delete",
          html: `
          <p>This OriginalSpec cannot be deleted because:</p>
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

      // ✅ STEP 2: Create Request payload
      const url = "/api/Requests";

      const requestBody = {
        request_type: "DELETE",
        target_table: "OriginalSpec",
        target_pk_id: tool_ref_spec_id,
        old_data: row,
        new_data: null,
        note: "Deleted from frontend",
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
        await MySwal.fire({
          icon: "error",
          title: "Error",
          text:
            errorData.message ||
            "Failed to submit delete request for OriginalSpec.",
          confirmButtonText: "OK",
          customClass: {
            confirmButton:
              "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded",
            popup: "text-sm",
          },
        });
        return;
      }

      const resultData = await response.json();

      await MySwal.fire({
        icon: "info",
        title: "Request Submitted",
        html: `Your delete request has been submitted.<br>Request ID: <b>${resultData.request_id}</b><br>It will be effective after admin approval.`,
        confirmButtonText: "OK",
        customClass: {
          confirmButton:
            "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded",
          popup: "text-sm",
        },
      });
      // // ✅ Force refetch pending request (to update yellow/red rows)
      // await fetchPendingRequests(); // 👈 เพิ่มบรรทัดนี้

      // setFilterValues({});
      setReloadFlag((prev) => prev + 1);
    } catch (err: unknown) {
      console.error("🔥 Exception during deletion:", err);

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

  // const handleExportCSV = () => {
  //   const csv = Papa.unparse(data);
  //   const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
  //   const url = URL.createObjectURL(blob);
  //   const link = document.createElement("a");
  //   link.setAttribute("href", url);
  //   link.setAttribute("download", "OriginalSpec.csv");
  //   document.body.appendChild(link);
  //   link.click();
  //   document.body.removeChild(link);
  // };

  // const handleExportCSV = () => {
  //   const visibleColumns = table.getVisibleLeafColumns();

  //   // ✅ header name → ใช้ชื่อ header หรือ fallback เป็น id
  //   const headers = visibleColumns.map(
  //     (col) => col.columnDef.header?.toString() || col.id
  //   );

  //   // ✅ ใช้ rows ที่ผ่าน filter ทั้งหมด (ไม่จำกัดแค่หน้านั้น)
  //   const visibleRows = table.getFilteredRowModel().rows;

  //   const exportData = visibleRows.map((row) => {
  //     const obj: Record<string, any> = {};
  //     visibleColumns.forEach((col) => {
  //       obj[col.columnDef.header?.toString() || col.id] = row.getValue(col.id);
  //     });
  //     return obj;
  //   });

  //   const csv = Papa.unparse({
  //     fields: headers,
  //     data: exportData.map((r) => Object.values(r)),
  //   });

  //   const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
  //   const url = URL.createObjectURL(blob);
  //   const link = document.createElement("a");
  //   link.setAttribute("href", url);
  //   link.setAttribute("download", "OriginalSpec.csv");
  //   document.body.appendChild(link);
  //   link.click();
  //   document.body.removeChild(link);
  // };

  // const handleExportCSV = () => {
  //   const visibleColumns = table.getVisibleLeafColumns();

  //   const headers = visibleColumns.map((col) => {
  //     if (typeof col.columnDef.header === "function") {
  //       return col.id;
  //     }
  //     return col.columnDef.header?.toString() || col.id;
  //   });

  //   const visibleRows = table.getFilteredRowModel().rows;

  //   const exportData = visibleRows.map((row) => {
  //     const obj: Record<string, any> = {};
  //     visibleColumns.forEach((col, i) => {
  //       const header =
  //         typeof col.columnDef.header === "function"
  //           ? col.id
  //           : col.columnDef.header?.toString() || col.id;

  //       obj[header] = row.getValue(col.id);
  //     });
  //     return obj;
  //   });

  //   const csv = Papa.unparse({
  //     fields: headers,
  //     data: exportData.map((r) => Object.values(r)),
  //   });

  //   const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
  //   const url = URL.createObjectURL(blob);
  //   const link = document.createElement("a");
  //   link.setAttribute("href", url);
  //   link.setAttribute("download", "OriginalSpec.csv");
  //   document.body.appendChild(link);
  //   link.click();
  //   document.body.removeChild(link);
  // };

  const handleExportCSV = () => {
    const exportDependencies: Record<string, string[]> = {
      overall_a: ["tolerance_a"],
      overall_b: ["tolerance_b"],
      overall_c: ["tolerance_c"],
    };

    const headerMap: Record<string, string> = {
      tool_type: "Tool Type",
      tool_name: "Tool Name",
      size_ref: "Size Ref",
      axle_type: "Axle Type",
      overall_a: "A",
      tolerance_a: "Tol. A",
      overall_b: "B",
      tolerance_b: "Tol. B",
      overall_c: "C",
      tolerance_c: "Tol. C",
      f_shank_min: "F-Shank min",
      f_shank_max: "F-Shank max",
      b2b_min: "B2B min",
      b2b_max: "B2B max",
      h2h_min: "H2H min",
      h2h_max: "H2H max",
      chassis_span1: "Chassis Span 1",
      chassis_span2: "Chassis Span 2",
      HST_pad: "HST Pad",
      RIM_pad: "RIM Pad",
      INNER_pad: "INNER Pad",
      EXTRA_RIM_pad: "EXTRA RIM Pad",
      HST_brass: "HST Brass",
      RIM_brass: "RIM Brass",
      INNER_brass: "INNER Brass",
      EXTRA_RIM_brass: "EXTRA RIM Brass",
      pad_source_key: "Pad Source Key",
      machine_no: "Machine No",
      machine_source_key: "Machine Source Key",
      source: "Source",
      is_original_spec: "Original?",
    };

    let visibleColumns = table.getVisibleLeafColumns().map((col) => col.id);

    // เพิ่ม dependencies ถ้า overall_x ถูกเลือก
    Object.entries(exportDependencies).forEach(([key, deps]) => {
      if (visibleColumns.includes(key)) {
        deps.forEach((dep) => {
          if (!visibleColumns.includes(dep)) {
            visibleColumns.push(dep);
          }
        });
      }
    });

    const headers = visibleColumns.map((id) => headerMap[id] || id);

    const visibleRows = table.getFilteredRowModel().rows;

    const exportData = visibleRows.map((row) => {
      const obj: Record<string, any> = {};

      visibleColumns.forEach((colId) => {
        if (["overall_a", "overall_b", "overall_c"].includes(colId)) {
          const toleranceCol = "tolerance_" + colId.split("_")[1];

          // เอา main value
          let val = row.getValue(colId);
          if (val === undefined || val === null) val = "";
          if (typeof val === "boolean") val = val ? "✔" : "-";

          obj[headerMap[colId] || colId] = val;

          // บังคับ export tolerance
          if (exportDependencies[colId]?.includes(toleranceCol)) {
            let tolVal = row.getValue(toleranceCol);
            if (tolVal === undefined || tolVal === null) tolVal = "";
            if (typeof tolVal === "boolean") tolVal = tolVal ? "✔" : "-";

            obj[headerMap[toleranceCol] || toleranceCol] = tolVal;
          }
        }
        // ข้าม tolerance fields ใน loop (กัน export ซ้ำ)
        else if (
          ["tolerance_a", "tolerance_b", "tolerance_c"].includes(colId)
        ) {
          // do nothing
        } else {
          let val = row.getValue(colId);
          if (val === undefined || val === null) val = "";
          if (typeof val === "boolean") val = val ? "✔" : "-";

          obj[headerMap[colId] || colId] = val;
        }
      });

      return obj;
    });

    const csv = Papa.unparse({
      fields: headers,
      data: exportData.map((r) => Object.values(r)),
    });

    const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.setAttribute("href", url);
    link.setAttribute("download", "OriginalSpec.csv");
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  // const handleExportXLSX = async () => {
  //   const exportDependencies: Record<string, string[]> = {
  //     overall_a: ["tolerance_a"],
  //     overall_b: ["tolerance_b"],
  //     overall_c: ["tolerance_c"],
  //   };

  //   const headerMap: Record<string, string> = {
  //     tool_type: "Tool Type",
  //     tool_name: "Tool Name",
  //     size_ref: "Size Ref",
  //     axle_type: "Axle Type",
  //     overall_a: "A",
  //     tolerance_a: "Tol. A",
  //     overall_b: "B",
  //     tolerance_b: "Tol. B",
  //     overall_c: "C",
  //     tolerance_c: "Tol. C",
  //     f_shank_min: "F-Shank min",
  //     f_shank_max: "F-Shank max",
  //     b2b_min: "B2B min",
  //     b2b_max: "B2B max",
  //     h2h_min: "H2H min",
  //     h2h_max: "H2H max",
  //     chassis_span1: "Chassis Span 1",
  //     chassis_span2: "Chassis Span 2",
  //     HST_pad: "HST Pad",
  //     RIM_pad: "RIM Pad",
  //     INNER_pad: "INNER Pad",
  //     EXTRA_RIM_pad: "EXTRA RIM Pad",
  //     HST_brass: "HST Brass",
  //     RIM_brass: "RIM Brass",
  //     INNER_brass: "INNER Brass",
  //     EXTRA_RIM_brass: "EXTRA RIM Brass",
  //     pad_source_key: "Pad Source Key",
  //     machine_no: "Machine No",
  //     machine_source_key: "Machine Source Key",
  //     source: "Source",
  //     is_original_spec: "Original?",
  //     knurling_type: "Knurling Type",
  //   };

  //   const groupHeaderMap: Record<string, string> = {
  //     tool_type: "Tool Info",
  //     tool_name: "Tool Info",
  //     size_ref: "Tool Info",
  //     axle_type: "Tool Info",
  //     overall_a: "Wheel: Over All Dimension (mm)",
  //     tolerance_a: "Wheel: Over All Dimension (mm)",
  //     overall_b: "Wheel: Over All Dimension (mm)",
  //     tolerance_b: "Wheel: Over All Dimension (mm)",
  //     overall_c: "Wheel: Over All Dimension (mm)",
  //     tolerance_c: "Wheel: Over All Dimension (mm)",
  //     f_shank_min: "Wheel: F-Shank (Inch)",
  //     f_shank_max: "Wheel: F-Shank (Inch)",
  //     b2b_min: "Bump to Bump (Inch)",
  //     b2b_max: "Bump to Bump (Inch)",
  //     h2h_min: "Head to Head (Inch)",
  //     h2h_max: "Head to Head (Inch)",
  //     chassis_span1: "Chassis Span (Inch)",
  //     chassis_span2: "Chassis Span (Inch)",
  //     HST_pad: "Pad",
  //     RIM_pad: "Pad",
  //     INNER_pad: "Pad",
  //     EXTRA_RIM_pad: "Pad",
  //     HST_brass: "Brass",
  //     RIM_brass: "Brass",
  //     INNER_brass: "Brass",
  //     EXTRA_RIM_brass: "Brass",
  //     pad_source_key: "Pad Source",
  //     machine_no: "Machine",
  //     machine_source_key: "Machine Source",
  //     source: "Specification Info",
  //     is_original_spec: "Specification Info",
  //     knurling_type: "Specification Info",
  //   };

  //   // ✅ get visible columns
  //   let visibleColumns = table.getVisibleLeafColumns().map((col) => col.id);

  //   Object.entries(exportDependencies).forEach(([key, deps]) => {
  //     if (visibleColumns.includes(key)) {
  //       deps.forEach((dep) => {
  //         if (!visibleColumns.includes(dep)) {
  //           visibleColumns.push(dep);
  //         }
  //       });
  //     }
  //   });

  //   const visibleRows = table.getFilteredRowModel().rows;

  //   // ✅ Prepare exportData
  //   const exportData: Record<string, string>[] = visibleRows.map((row) => {
  //     const obj: Record<string, string> = {};
  //     visibleColumns.forEach((colId) => {
  //       let val = row.getValue(colId);
  //       if (val === undefined || val === null) val = "";
  //       if (typeof val === "boolean") val = val ? "✔" : "-";
  //       obj[colId] = String(val);
  //     });
  //     return obj;
  //   });

  //   const workbook = new ExcelJS.Workbook();
  //   const sheet = workbook.addWorksheet("OriginalSpec");

  //   // ✅ Build group header + sub header
  //   const groupHeaders: string[] = [];
  //   const subHeaders: string[] = [];

  //   let prevGroup = "";
  //   visibleColumns.forEach((colId) => {
  //     const group = groupHeaderMap[colId] || "";
  //     if (group === prevGroup) {
  //       groupHeaders.push("");
  //     } else {
  //       groupHeaders.push(group);
  //       prevGroup = group;
  //     }
  //     subHeaders.push(headerMap[colId] || colId);
  //   });

  //   // ✅ Write group header row
  //   groupHeaders.forEach((header, colIdx) => {
  //     const cell = sheet.getCell(1, colIdx + 1);
  //     cell.value = header || null; // ต้องเป็น null ไม่ใช่ undefined
  //     cell.font = { name: "Arial", size: 10, bold: true };
  //     cell.alignment = { vertical: "middle", horizontal: "center" };
  //     cell.border = {
  //       top: { style: "thin" },
  //       left: { style: "thin" },
  //       bottom: { style: "thin" },
  //       right: { style: "thin" },
  //     };
  //   });

  //   // ✅ Write sub header row
  //   subHeaders.forEach((header, colIdx) => {
  //     const cell = sheet.getCell(2, colIdx + 1);
  //     cell.value = header;
  //     cell.font = { name: "Arial", size: 10, bold: true };
  //     cell.alignment = { vertical: "middle", horizontal: "center" };
  //     cell.border = {
  //       top: { style: "thin" },
  //       left: { style: "thin" },
  //       bottom: { style: "thin" },
  //       right: { style: "thin" },
  //     };
  //   });

  //   // ✅ Merge cells for same group headers
  //   let startIdx = 0;
  //   while (startIdx < groupHeaders.length) {
  //     let endIdx = startIdx;
  //     while (
  //       endIdx + 1 < groupHeaders.length &&
  //       groupHeaders[endIdx + 1] === ""
  //     ) {
  //       endIdx++;
  //     }
  //     if (endIdx > startIdx && groupHeaders[startIdx] !== "") {
  //       sheet.mergeCells(1, startIdx + 1, 1, endIdx + 1);
  //     }
  //     startIdx = endIdx + 1;
  //   }

  //   // ✅ Write data rows
  //   exportData.forEach((dataRow, rowIdx) => {
  //     visibleColumns.forEach((colId, colIdx) => {
  //       const cell = sheet.getCell(rowIdx + 3, colIdx + 1);
  //       cell.value = dataRow[colId] || "";
  //       cell.font = { name: "Arial", size: 10 };
  //       cell.alignment = { vertical: "middle", horizontal: "center" };
  //       cell.border = {
  //         top: { style: "thin" },
  //         left: { style: "thin" },
  //         bottom: { style: "thin" },
  //         right: { style: "thin" },
  //       };
  //     });
  //   });

  //   // ✅ Column width
  //   sheet.columns.forEach((col) => {
  //     col.width = 15;
  //   });

  //   const buffer = await workbook.xlsx.writeBuffer();
  //   const blob = new Blob([buffer], {
  //     type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
  //   });
  //   saveAs(blob, "OriginalSpec.xlsx");
  // };

  // const handleExportXLSX = async () => {
  //   const exportDependencies: Record<string, string[]> = {
  //     overall_a: ["tolerance_a"],
  //     overall_b: ["tolerance_b"],
  //     overall_c: ["tolerance_c"],
  //   };

  //   // ✅ สร้าง mapping จาก id → group และ id → accessorKey
  //   const idToGroupMap: Record<string, string> = {};
  //   const idToAccessorMap: Record<string, string> = {};

  //   const buildMaps = (cols: any[], parentHeader: string | null = null) => {
  //     cols.forEach((col) => {
  //       if (col.columns) {
  //         buildMaps(col.columns, col.header);
  //       } else {
  //         if (col.id && col.accessorKey) {
  //           idToGroupMap[col.id] = parentHeader ?? "Other";
  //           idToAccessorMap[col.id] = col.accessorKey;
  //         }
  //       }
  //     });
  //   };

  //   buildMaps(table.getAllColumns().map((col) => col.columnDef));

  //   // ✅ รายชื่อ column id ที่ไม่ต้อง export
  //   const ignoredIds = ["Other", "Actions"];

  //   // ✅ เก็บ column ID ที่เปิดอยู่ (และไม่ถูก ignore)
  //   let visibleIds = table
  //     .getVisibleLeafColumns()
  //     .map((col) => col.id)
  //     .filter((id) => !ignoredIds.includes(id));

  //   // ✅ Add dependencies (tolerance) ถ้ายังไม่มี
  //   Object.entries(exportDependencies).forEach(([main, deps]) => {
  //     const mainId = Object.entries(idToAccessorMap).find(
  //       ([id, acc]) => acc === main
  //     )?.[0];
  //     if (mainId && visibleIds.includes(mainId)) {
  //       deps.forEach((dep) => {
  //         const depId = Object.entries(idToAccessorMap).find(
  //           ([id, acc]) => acc === dep
  //         )?.[0];
  //         if (depId && !visibleIds.includes(depId)) {
  //           visibleIds.push(depId);
  //         }
  //       });
  //     }
  //   });

  //   const visibleRows = table.getFilteredRowModel().rows;

  //   // ✅ Prepare export data
  //   const exportData: Record<string, string>[] = visibleRows.map((row) => {
  //     const obj: Record<string, string> = {};
  //     visibleIds.forEach((id) => {
  //       const accessor = idToAccessorMap[id];
  //       let val = row.original[accessor];
  //       if (val === undefined || val === null) val = "";
  //       if (typeof val === "boolean") val = val ? "✔" : "-";
  //       obj[id] = String(val);
  //     });
  //     return obj;
  //   });

  //   const workbook = new ExcelJS.Workbook();
  //   const sheet = workbook.addWorksheet("OriginalSpec");

  //   // ✅ Group Header / Sub Header
  //   const groupHeaders: string[] = [];
  //   const subHeaders: string[] = [];

  //   let prevGroup = "";
  //   visibleIds.forEach((id) => {
  //     const group = idToGroupMap[id] ?? "Other";
  //     groupHeaders.push(group === prevGroup ? "" : group);
  //     subHeaders.push(id);
  //     prevGroup = group;
  //   });

  //   // ✅ Write group header (Row 1)
  //   groupHeaders.forEach((header, colIdx) => {
  //     const cell = sheet.getCell(1, colIdx + 1);
  //     cell.value = header || null;
  //     cell.font = { name: "Arial", size: 10, bold: true };
  //     cell.alignment = { vertical: "middle", horizontal: "center" };
  //     cell.border = {
  //       top: { style: "thin" },
  //       left: { style: "thin" },
  //       bottom: { style: "thin" },
  //       right: { style: "thin" },
  //     };
  //   });

  //   // ✅ Write sub header (Row 2)
  //   subHeaders.forEach((header, colIdx) => {
  //     const cell = sheet.getCell(2, colIdx + 1);
  //     cell.value = header;
  //     cell.font = { name: "Arial", size: 10, bold: true };
  //     cell.alignment = { vertical: "middle", horizontal: "center" };
  //     cell.border = {
  //       top: { style: "thin" },
  //       left: { style: "thin" },
  //       bottom: { style: "thin" },
  //       right: { style: "thin" },
  //     };
  //   });

  //   // ✅ Merge group headers
  //   let startIdx = 0;
  //   while (startIdx < groupHeaders.length) {
  //     let endIdx = startIdx;
  //     while (
  //       endIdx + 1 < groupHeaders.length &&
  //       groupHeaders[endIdx + 1] === ""
  //     ) {
  //       endIdx++;
  //     }
  //     if (endIdx > startIdx && groupHeaders[startIdx]) {
  //       sheet.mergeCells(1, startIdx + 1, 1, endIdx + 1);
  //     }
  //     startIdx = endIdx + 1;
  //   }

  //   // ✅ Write data rows
  //   exportData.forEach((dataRow, rowIdx) => {
  //     visibleIds.forEach((id, colIdx) => {
  //       const cell = sheet.getCell(rowIdx + 3, colIdx + 1);
  //       cell.value = dataRow[id];
  //       cell.font = { name: "Arial", size: 10 };
  //       cell.alignment = { vertical: "middle", horizontal: "center" };
  //       cell.border = {
  //         top: { style: "thin" },
  //         left: { style: "thin" },
  //         bottom: { style: "thin" },
  //         right: { style: "thin" },
  //       };
  //     });
  //   });

  //   // ✅ Column width
  //   sheet.columns.forEach((col) => {
  //     col.width = 15;
  //   });

  //   // ✅ Save file
  //   const buffer = await workbook.xlsx.writeBuffer();
  //   const blob = new Blob([buffer], {
  //     type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
  //   });
  //   saveAs(blob, "OriginalSpec.xlsx");
  // };

  const handleExportXLSX = async () => {
    const exportDependencies: Record<string, string[]> = {
      overall_a: ["tolerance_a"],
      overall_b: ["tolerance_b"],
      overall_c: ["tolerance_c"],
    };

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

    const ignoredIds = ["Other", "Actions"];

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

    const visibleRows = table.getFilteredRowModel().rows;

    // ✅ ✨ Combine A/B/C with tolerance
    const exportData: Record<string, string>[] = visibleRows.map((row) => {
      const obj: Record<string, string> = {};

      visibleIds.forEach((id) => {
        let val: any = "";
        if (["A", "B", "C"].includes(id)) {
          const mainKey = idToAccessorMap[id];
          const tolKey = exportDependencies[mainKey]?.[0];
          const mainVal = row.original[mainKey];
          const tolVal = row.original[tolKey];
          if (mainVal !== undefined && tolVal !== undefined) {
            val = `${mainVal} ± ${tolVal}`;
          } else {
            val = mainVal ?? "";
          }
        } else {
          const accessor = idToAccessorMap[id];
          val = row.original[accessor];
        }

        if (val === undefined || val === null) val = "";
        if (typeof val === "boolean") val = val ? "✔" : "-";
        obj[id] = String(val);
      });

      return obj;
    });

    const workbook = new ExcelJS.Workbook();
    const sheet = workbook.addWorksheet("OriginalSpec");

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
      const pendingRequest = (visibleRows[rowIdx]?.original as any)
        ?.pending_request;

      let fillColor = undefined;
      let fontColor = undefined;

      if (pendingRequest === "UPDATE") {
        fillColor = "FEFCE8"; // bg-yellow-50
        fontColor = "854D0E"; // text-yellow-900
      } else if (pendingRequest === "DELETE") {
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
    saveAs(blob, "OriginalSpec.xlsx");
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

  const handleOpenDetail = (rowData: OriginalSpecRow) => {
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

  const columns = useMemo<ColumnDef<OriginalSpecRow, any>[]>(
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
            accessorKey: "tolerance_a",
            header: "Tol. A",
            enableSorting: false,
            enableColumnFilter: false,
            meta: { hidden: false },
            visible: false,
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
            accessorKey: "tolerance_b",
            header: "Tol. B",
            enableSorting: false,
            enableColumnFilter: false,
            meta: { hidden: true },
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
          {
            accessorKey: "tolerance_c",
            header: "Tol. C",
            enableSorting: false,
            enableColumnFilter: false,
            meta: { hidden: true },
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
            id: "Chassis Span1",
            accessorKey: "chassis_span1",
            header: () => (
              <div className="flex items-center justify-center">
                Chassis Span1{renderDropdownFilter("chassis_span1")}
              </div>
            ),
          },
          {
            id: "Chassis Span2",
            accessorKey: "chassis_span2",
            header: () => (
              <div className="flex items-center justify-center">
                Chassis Span2{renderDropdownFilter("chassis_span2")}
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
            id: "Original Spec",
            accessorKey: "is_original_spec",
            header: () => (
              <div className="flex items-center justify-center">
                Original Spec{renderDropdownFilter("is_original_spec")}
              </div>
            ),
          },
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
        header: "Wheel Picture",
        columns: [
          {
            id: "Wheel Before HST",
            accessorKey: "image_url",
            header: () => (
              <div className="flex items-center justify-center">
                Wheel Before HST
                {renderDropdownFilter("image_url")}
              </div>
            ),
            cell: ({ row }) => {
              const r = row.original as OriginalSpecRow;
              const url = (r as any).image_url ?? null;
              const fileName = (r as any).pic_before_hst_file_name ?? null;
              const originalKeyId = (r as any).tool_key_id as
                | number
                | undefined;

              return (
                <ImageCell
                  url={url}
                  fileName={fileName ?? undefined}
                  title={`${(r as any).tool_name} ${(r as any).size_ref}`}
                  onOpen={() => {
                    if (!originalKeyId) return;
                    setPhotoCtx({
                      open: true,
                      originalKeyId,
                      url,
                      fileName: fileName ?? null,
                    });
                  }}
                />
              );
            },
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
            id: "Source",
            accessorKey: "source",
            header: () => (
              <div className="flex items-center justify-center">
                Source{renderDropdownFilter("source")}
              </div>
            ),
          },
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
                        size_ref_id:
                          sizes.find((t) => t.label === row.original.size_ref)
                            ?.value ?? 12,
                        axle_type_id:
                          axles.find((t) => t.label === row.original.axle_type)
                            ?.value ?? 56,
                        knurling_type:
                          row.original.knurling_type === true
                            ? 1
                            : row.original.knurling_type === false
                            ? 0
                            : typeof row.original.knurling_type === "string"
                            ? Number(row.original.knurling_type) || 0
                            : typeof row.original.knurling_type === "number"
                            ? row.original.knurling_type
                            : 0,
                        is_original_spec: row.original.is_original_spec ? 1 : 0,
                        overall_a: row.original.overall_a ?? "",
                        overall_b: row.original.overall_b ?? "",
                        overall_c: row.original.overall_c ?? "",
                        tolerance_a: row.original.tolerance_a ?? "",
                        tolerance_b: row.original.tolerance_b ?? "",
                        tolerance_c: row.original.tolerance_c ?? "",
                        f_shank_min: row.original.f_shank_min ?? "",
                        f_shank_max: row.original.f_shank_max ?? "",
                        b2b_min: row.original.b2b_min ?? "",
                        b2b_max: row.original.b2b_max ?? "",
                        h2h_min: row.original.h2h_min ?? "",
                        h2h_max: row.original.h2h_max ?? "",
                        chassis_span1: row.original.chassis_span1 ?? "",
                        chassis_span2: row.original.chassis_span2 ?? "",
                        source: row.original.source ?? "",
                        description: row.original.description ?? "",
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
    onColumnFiltersChange: setColumnFilters,
    onColumnVisibilityChange: setColumnVisibility,
    onPaginationChange: setPagination,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
  });

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

            <div ref={dropdownRefs} className="relative">
              <button
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

              {showColumnDropdown && (
                <div className="absolute top-full mt-2 right-0 w-64 bg-white border border-gray-200 rounded-lg shadow-lg z-50 animate-in fade-in-0 zoom-in-95 duration-200 max-h-80 overflow-hidden">
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
                </div>
              )}
            </div>
            <DateTimeFilter
              filterMode="datetime"
              onFilterApply={setDatetimeFilters}
              initialFilters={datetimeFilters}
            />
          </div>

          {/* Right side - Primary action */}
          {["admin", "editor"].includes(userRole || "") && (
            <button
              className="inline-flex items-center px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-lg shadow-sm hover:bg-blue-700 hover:shadow-md active:bg-blue-800 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-all duration-200"
              onClick={() => {
                setCloneRow(null);
                setFromClone(false);
                setShowAddModal(true);
              }}
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
        <OriginalSpecAddToolModal
          onClose={() => setShowAddModal(false)}
          onSubmitSuccess={() => {
            setFilterValues({}); // trigger reload
          }}
        />
      )} */}
      {showAddModal && (
        <OriginalSpecAddToolModal
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
        <OriginalSpecEditToolModal
          initialData={{
            tool_ref_spec_id: editingRow.tool_ref_spec_id,
            overall_a: editingRow.overall_a ?? "",
            overall_b: editingRow.overall_b ?? "",
            overall_c: editingRow.overall_c ?? "",
            tolerance_a: editingRow.tolerance_a ?? "",
            tolerance_b: editingRow.tolerance_b ?? "",
            tolerance_c: editingRow.tolerance_c ?? "",
            f_shank_min: editingRow.f_shank_min ?? "",
            f_shank_max: editingRow.f_shank_max ?? "",
            b2b_min: editingRow.b2b_min ?? "",
            b2b_max: editingRow.b2b_max ?? "",
            h2h_min: editingRow.h2h_min ?? "",
            h2h_max: editingRow.h2h_max ?? "",
            chassis_span1: editingRow.chassis_span1 ?? "",
            chassis_span2: editingRow.chassis_span2 ?? "",
            source: editingRow.source ?? "",
            description: editingRow.description ?? "",

            // ✅ แสดงค่า (Blanks) ถ้าไม่มี
            tool_type: editingRow.tool_type || "(Blanks)",
            tool_name: editingRow.tool_name || "(Blanks)",
            size_ref: editingRow.size_ref || "(Blanks)",
            axle_type: editingRow.axle_type || "(Blanks)",
            is_original_spec: editingRow.is_original_spec ? 1 : 0,
            knurling_type: editingRow.knurling_type ? 1 : 0,
          }}
          onClose={() => setShowEditModal(false)}
          onSubmitSuccess={() => {
            setShowEditModal(false);
            // ให้รีโหลด data ใหม่
            // fetchPendingRequests(); // re render
            setFilterValues({ ...filterValues }); // trick: trigger useEffect
            setReloadFlag((prev) => prev + 1); // 👈 trigger reload
          }}
        />
      )}

      {/* {photoCtx.open && photoCtx.originalKeyId && (
        <ToolKeyOriginalPhotoModal
          originalKeyId={photoCtx.originalKeyId}
          phase="before-hst" // ✅ ช่องนี้คือ before-HST ของ original
          currentUrl={photoCtx.url}
          currentName={photoCtx.fileName}
          onClose={() => setPhotoCtx((s) => ({ ...s, open: false }))}
          onSuccess={() => {
            // ✅ โหลดข้อมูลใหม่หลังอัปเดตหรือลบรูป
            // วิธีง่ายสุด: trigger useEffect เดิม เช่น setFilterValues({ ...filterValues })
            setFilterValues((prev) => ({ ...prev }));
          }}
        />
      )} */}

      <ToolDetailModal
        showDetailModal={showDetailModal}
        setShowDetailModal={setShowDetailModal}
        detailData={detailData}
        definitions1={definitions1}
        definitions2={definitions2}
      />

      <DefModal
        showDefModal={showDefModal}
        setShowDefModal={setShowDefModal}
        definitions1={definitions1}
        definitions2={definitions2}
      />
    </div>
  );
};

export default OriginalSpecTable;
