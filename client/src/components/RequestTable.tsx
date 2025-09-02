import React, { useEffect, useMemo, useState, useRef } from "react";
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

import Swal from "sweetalert2";
import withReactContent from "sweetalert2-react-content";

// ✅ Added XLSX Export logic for RequestTable
import ExcelJS from "exceljs";
import { saveAs } from "file-saver";

import { createPortal } from "react-dom";

import EnhancedDateTimeFilter from "./EnhancedDateTimeFilter";

import { PlusCircle, Edit3, Trash2 } from "lucide-react";

// import EnhancedDateTimeModal from "./EnhancedDateTimeModal";

const MySwal = withReactContent(Swal);
const swalCustomClasses = {
  confirmButton:
    "bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded text-sm focus:outline-none",
  cancelButton:
    "bg-gray-300 hover:bg-gray-400 text-gray-800 px-4 py-2 rounded text-sm ml-2 focus:outline-none",
};

export interface RequestRow {
  request_id: number;
  request_type: string;
  request_status: string;
  target_table: string;
  target_pk_id: number | null;
  old_data: string | null;
  new_data: string | null;
  requested_by: string;
  requested_at: string;
  approved_by: string | null;
  approved_at: string | null;
  note: string | null;
}

const tableFieldMap: Record<string, string[]> = {
  toolMachineMap: [
    "tool_key_id",
    "type_id",
    "tool_id",
    "type_ref_id",
    "tool_ref_id",
    "size_ref_id",
    "machine_id",
  ],
  toolPadMap: [
    "tool_key_id",
    "type_id",
    "tool_id",
    "type_ref_id",
    "tool_ref_id",
    "size_ref_id",
    "pad_id",
    "hst_type_id",
  ],
  // ✅ เพิ่ม table อื่นๆ ได้ตาม schema จริง
};

const RequestTable: React.FC = () => {
  const [data, setData] = useState<RequestRow[]>([]);
  const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([]);
  const [columnVisibility, setColumnVisibility] = useState<VisibilityState>({});
  const [search, setSearch] = useState("");
  const [showColumnDropdown, setShowColumnDropdown] = useState(false);
  const [pagination, setPagination] = useState({ pageIndex: 0, pageSize: 15 });

  const [showModal, setShowModal] = useState(false);
  const [modalOldData, setModalOldData] = useState<any>(null);
  const [modalNewData, setModalNewData] = useState<any>(null);

  const [selectedRequestType, setSelectedRequestType] = useState<string>("");

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

  const [toolsMap, setToolsMap] = useState<Record<number, string>>({});
  const [typesMap, setTypesMap] = useState<Record<number, string>>({});
  const [sizeRefsMap, setSizeRefsMap] = useState<Record<number, string>>({});
  const [axleTypesMap, setAxleTypesMap] = useState<Record<number, string>>({});
  const [hstTypesMap, setHstTypesMap] = useState<Record<number, string>>({});
  const [padsMap, setPadsMap] = useState<Record<number, string>>({});
  const [machinesMap, setMachinesMap] = useState<Record<number, string>>({});

  const columnDropdownRef = useRef<HTMLDivElement>(null);
  const columnButtonRef = useRef<HTMLButtonElement>(null);
  const [columnDropdownPosition, setColumnDropdownPosition] = useState<{
    top: number;
    left: number;
    width: number;
  } | null>(null);

  useEffect(() => {
    if (showColumnDropdown && columnButtonRef.current) {
      const rect = columnButtonRef.current.getBoundingClientRect();
      setColumnDropdownPosition({
        top: rect.bottom + window.scrollY + 4,
        left: rect.left + window.scrollX,
        width: rect.width,
      });
    }
  }, [showColumnDropdown]);

  const [dateRange, setDateRange] = useState<{
    requested_at_start: string;
    requested_at_end: string;
    approved_at_start: string;
    approved_at_end: string;
  }>({
    requested_at_start: "",
    requested_at_end: "",
    approved_at_start: "",
    approved_at_end: "",
  });

  const [dateFilterOpen, setDateFilterOpen] = useState(false);
  const dateFilterButtonRef = useRef<HTMLButtonElement>(null);
  const [dateFilterPosition, setDateFilterPosition] = useState<{
    top: number;
    left: number;
    width: number;
  } | null>(null);

  const [showDateFilterDropdown, setShowDateFilterDropdown] = useState(false);
  const dateFilterBtnRef = useRef<HTMLButtonElement>(null);

  const [filterMode, setFilterMode] = useState<"datetime" | "date" | "time">(
    "datetime"
  );

  const [toolKeyDetailsMap, setToolKeyDetailsMap] = useState<
    Record<number, any>
  >({});
  const [selectedTargetTable, setSelectedTargetTable] = useState<string>("");

  // ✅ ฟังก์ชัน fetch API per ID
  const fetchToolKeyAllIdsById = async (id: number) => {
    if (toolKeyDetailsMap[id]) {
      return toolKeyDetailsMap[id];
    }

    const token = localStorage.getItem("token");

    const res = await fetch(`/api/ToolKeyAlls/${id}/ids`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (res.ok) {
      const json = await res.json();
      setToolKeyDetailsMap((prev) => ({
        ...prev,
        [id]: json,
      }));
      return json;
    } else {
      console.error("Cannot load ToolKeyAll IDs:", id);
      return null;
    }
  };

  useEffect(() => {
    fetchRequests();
  }, [filterValues]);

  // const fetchRequests = () => {
  //   const token = localStorage.getItem("token");

  //   if (!token) {
  //     console.error("No token found, redirecting to login.");
  //     window.location.href = "/login";
  //     return;
  //   }

  //   fetch("/api/requests", {
  //     headers: {
  //       "Content-Type": "application/json",
  //       Authorization: `Bearer ${token}`,
  //     },
  //   })
  //     .then((res) => {
  //       if (!res.ok) {
  //         if (res.status === 401) {
  //           console.error("Unauthorized - redirecting to login.");
  //           window.location.href = "/login";
  //         }
  //         throw new Error("Server error");
  //       }
  //       return res.json();
  //     })
  //     .then((result: RequestRow[]) => {
  //       setData(result);
  //     })
  //     .catch((err) => console.error("Error fetching requests:", err));
  // };

  // Inside RequestTable component
  // const handleExportXLSX = async () => {
  //   const ignoredIds = ["actions", "details"];

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

  //   let visibleIds = table
  //     .getVisibleLeafColumns()
  //     .map((col) => col.id)
  //     .filter((id) => !ignoredIds.includes(id));

  //   const exportData = data.map((row) => {
  //     const obj: Record<string, string> = {};

  //     visibleIds.forEach((id) => {
  //       const accessor = idToAccessorMap[id];
  //       let val: any = "";

  //       if (accessor) {
  //         val = (row as any)[accessor];
  //         if (val === undefined || val === null) val = "";
  //         if (typeof val === "boolean") val = val ? "✔" : "-";
  //         if (typeof val === "string" && accessor.endsWith("_at"))
  //           val = formatDateTime(val);
  //       }

  //       if (accessor) {
  //         obj[id] = String(val); // ✅ ใช้ id เป็น key เพื่อ match กับ visibleIds
  //       }
  //     });

  //     return obj;
  //   });

  //   const workbook = new ExcelJS.Workbook();
  //   const sheet = workbook.addWorksheet("Requests");

  //   const groupHeaders: string[] = [];
  //   const subHeaders: string[] = [];

  //   let prevGroup = "";
  //   visibleIds.forEach((id) => {
  //     const group = idToGroupMap[id] ?? "Other";
  //     groupHeaders.push(group === prevGroup ? "" : group);
  //     subHeaders.push(id);
  //     prevGroup = group;
  //   });

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

  //   exportData.forEach((dataRow, rowIdx) => {
  //     const excelRowIdx = rowIdx + 3;
  //     const pending = (data[rowIdx] as any)?.request_type;

  //     let fillColor: string | undefined;
  //     let fontColor: string | undefined;

  //     if (pending === "UPDATE") {
  //       fillColor = "FEFCE8"; // yellow
  //       fontColor = "854D0E";
  //     } else if (pending === "DELETE") {
  //       fillColor = "FEF2F2"; // red
  //       fontColor = "991B1B";
  //     } else if (pending === "INSERT") {
  //       fillColor = "ECFDF5"; // green
  //       fontColor = "065F46";
  //     }

  //     visibleIds.forEach((id, colIdx) => {
  //       const cell = sheet.getCell(excelRowIdx, colIdx + 1);
  //       cell.value = dataRow[id];
  //       cell.font = {
  //         name: "Arial",
  //         size: 10,
  //         color: fontColor ? { argb: fontColor } : undefined,
  //       };
  //       cell.alignment = { vertical: "middle", horizontal: "center" };
  //       cell.border = {
  //         top: { style: "thin" },
  //         left: { style: "thin" },
  //         bottom: { style: "thin" },
  //         right: { style: "thin" },
  //       };
  //       if (fillColor) {
  //         cell.fill = {
  //           type: "pattern",
  //           pattern: "solid",
  //           fgColor: { argb: fillColor },
  //         };
  //       }
  //     });
  //   });

  //   sheet.columns.forEach((col) => {
  //     col.width = 15;
  //   });

  //   const buffer = await workbook.xlsx.writeBuffer();
  //   const blob = new Blob([buffer], {
  //     type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
  //   });
  //   saveAs(blob, "Requests.xlsx");
  // };

  // ✅ Added XLSX Export logic for RequestTable

  const handleExportXLSX = async () => {
    const workbook = new ExcelJS.Workbook();
    const sheet = workbook.addWorksheet("Requests");

    const visibleColumns = table
      .getVisibleLeafColumns()
      .filter((col) => col.id !== "actions" ); // ❌ ตัด Action และ Detail columns

    const subHeaders: string[] = visibleColumns.map((col) => col.id);

    // ✅ เขียนแค่ Sub Header → แถวที่ 1
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

    // ✅ เขียนข้อมูลจริง เริ่มที่ row 2
    const rows = table.getFilteredRowModel().rows;
    rows.forEach((row, rowIdx) => {
      const excelRowIdx = rowIdx + 2;
      const pending = (row.original as any)?.request_status;

      let fillColor: string | undefined;
      let fontColor: string | undefined;

      if (pending === "UPDATE") {
        fillColor = "FEFCE8";
        fontColor = "854D0E";
      } else if (pending === "DELETE") {
        fillColor = "FEF2F2";
        fontColor = "991B1B";
      } else if (pending === "INSERT") {
        fillColor = "ECFDF5";
        fontColor = "065F46";
      }

      visibleColumns.forEach((col, colIdx) => {
        const accessor = col.id;
        const value = (row.original as any)[accessor];
        const cell = sheet.getCell(excelRowIdx, colIdx + 1);

        let val: ExcelJS.CellValue = "";
        if (typeof value === "boolean") {
          val = value ? "✔" : "-";
        } else if (value !== undefined && value !== null) {
          val = accessor.endsWith("_at")
            ? formatDateTime(String(value))
            : String(value);
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

    // ✅ ปรับ column width
    sheet.columns.forEach((col) => {
      col.width = 15;
    });

    const buffer = await workbook.xlsx.writeBuffer();
    const blob = new Blob([buffer], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    });
    saveAs(blob, "Requests.xlsx");
  };

  const fetchRequests = () => {
    const token = localStorage.getItem("token");
    if (!token) {
      console.error("No token found, redirecting to login.");
      window.location.href = "/login";
      return;
    }

    // 🔁 สร้าง query string จาก filterValues
    const query = new URLSearchParams();
    Object.entries(filterValues).forEach(([key, values]) => {
      if (values.length > 0) query.append(key, values.join(","));
    });

    fetch(`/api/requests?${query.toString()}`, {
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
    })
      .then((res) => {
        if (!res.ok) {
          if (res.status === 401) {
            console.error("Unauthorized - redirecting to login.");
            window.location.href = "/login";
          }
          throw new Error("Server error");
        }
        return res.json();
      })
      .then((result: RequestRow[]) => {
        setData(result);
      })
      .catch((err) => console.error("Error fetching requests:", err));
  };

  // useEffect(() => {
  //   const token = localStorage.getItem("token");

  //   Promise.all([
  //     fetch("/api/tools", {
  //       headers: { Authorization: `Bearer ${token}` },
  //     }).then((res) => res.json()),

  //     fetch("/api/typeModels", {
  //       headers: { Authorization: `Bearer ${token}` },
  //     }).then((res) => res.json()),

  //     fetch("/api/sizeRefs", {
  //       headers: { Authorization: `Bearer ${token}` },
  //     }).then((res) => res.json()),

  //     fetch("/api/axleTypes", {
  //       headers: { Authorization: `Bearer ${token}` },
  //     }).then((res) => res.json()),
  //   ]).then(([tools, types, sizes, axles]) => {
  //     setToolsMap(
  //       Object.fromEntries(tools.map((t: any) => [t.tool_id, t.tool_name]))
  //     );
  //     setTypesMap(
  //       Object.fromEntries(types.map((t: any) => [t.type_id, t.type_name]))
  //     );
  //     setSizeRefsMap(
  //       Object.fromEntries(sizes.map((t: any) => [t.size_ref_id, t.size_ref]))
  //     );
  //     setAxleTypesMap(
  //       Object.fromEntries(axles.map((t: any) => [t.axle_type_id, t.axle_type]))
  //     );
  //   });
  // }, []);

  useEffect(() => {
    const options: Record<string, string[]> = {};

    if (data.length > 0) {
      Object.keys(data[0]).forEach((key) => {
        const values = Array.from(
          new Set(data.map((row) => String((row as any)[key] ?? "")))
        ).sort();

        options[key] = values;
      });
    }

    setFilterOptions(options);
  }, [data]);

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

        {dropdownOpen[columnId] &&
          createPortal(
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
                    (filterOptions[columnId]?.map((v) => v ?? "null").length ||
                      0)
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
                    const updatedFilters = {
                      ...filterValues,
                      [columnId]: tempFilterValues[columnId] || [],
                    };

                    console.log(
                      "📤 Sending filters to backend:",
                      updatedFilters
                    );

                    setFilterValues(updatedFilters);
                    setDropdownOpen((prev) => ({ ...prev, [columnId]: false }));
                  }}
                >
                  OK
                </button>
              </div>
            </div>,
            document.body // 👈 dropdown ถูก render แยกออกจาก parent ไปไว้ใน <body>
          )}
      </>
    );
  };

  useEffect(() => {
    const token = localStorage.getItem("token");

    Promise.all([
      fetch("/api/tools", {
        headers: { Authorization: `Bearer ${token}` },
      }).then((res) => res.json()),

      fetch("/api/typeModels", {
        headers: { Authorization: `Bearer ${token}` },
      }).then((res) => res.json()),

      fetch("/api/sizeRefs", {
        headers: { Authorization: `Bearer ${token}` },
      }).then((res) => res.json()),

      fetch("/api/axleTypes", {
        headers: { Authorization: `Bearer ${token}` },
      }).then((res) => res.json()),

      fetch("/api/hstTypes", {
        headers: { Authorization: `Bearer ${token}` },
      }).then((res) => res.json()),

      fetch("/api/pads", {
        headers: { Authorization: `Bearer ${token}` },
      }).then((res) => res.json()),

      fetch("/api/machines", {
        headers: { Authorization: `Bearer ${token}` },
      }).then((res) => res.json()),
    ]).then(([tools, types, sizes, axles, hsts, pads, machines]) => {
      setToolsMap(
        Object.fromEntries(tools.map((t: any) => [t.tool_id, t.tool_name]))
      );
      setTypesMap(
        Object.fromEntries(types.map((t: any) => [t.type_id, t.tool_type]))
      );
      setSizeRefsMap(
        Object.fromEntries(sizes.map((t: any) => [t.size_ref_id, t.size_ref]))
      );
      setAxleTypesMap(
        Object.fromEntries(axles.map((t: any) => [t.axle_type_id, t.axle_type]))
      );
      setHstTypesMap(
        Object.fromEntries(hsts.map((t: any) => [t.hst_type_id, t.hst_type]))
      );
      setPadsMap(
        Object.fromEntries(pads.map((t: any) => [t.pad_id, t.pad_name]))
      );
      setMachinesMap(
        Object.fromEntries(
          machines.map((t: any) => [t.machine_id, t.machine_no])
        )
      );
    });
  }, []);

  const renderName = (id: number | null, map: Record<number, string>) => {
    return id != null ? map[id] || id : "-";
  };

  const handleExportCSV = () => {
    const csv = Papa.unparse(data);
    const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = "Requests.csv";
    link.click();
  };

  const handleApprove = async (id: number) => {
    const result = await MySwal.fire({
      title: "Approve Request",
      text: "Are you sure you want to approve this request?",
      icon: "question",
      showCancelButton: true,
      confirmButtonText: "Yes, Approve",
      cancelButtonText: "Cancel",
      customClass: swalCustomClasses,
    });

    if (result.isConfirmed) {
      const token = localStorage.getItem("token");
      console.log("token before approve:", token);

      const res = await fetch(
        `/api/requests/approve/${id}`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
          },
        }
      );

      if (res.ok) {
        MySwal.fire({
          icon: "success",
          title: "Approved!",
          text: "Request approved successfully.",
          confirmButtonText: "OK",
          customClass: {
            confirmButton: swalCustomClasses.confirmButton,
          },
        }).then(() => {
          // ✅ refresh data instead of reload page
          fetchRequests();
        });
      } else {
        const text = await res.text();
        console.error("Error approving:", text);

        MySwal.fire({
          icon: "error",
          title: "Error",
          text: "Failed to approve request.",
        });
      }
    }
  };

  const handleReject = async (id: number) => {
    const result = await MySwal.fire({
      title: "Reject Request",
      text: "Are you sure you want to reject this request?",
      icon: "warning",
      showCancelButton: true,
      confirmButtonText: "Yes, Reject",
      cancelButtonText: "Cancel",
      customClass: {
        confirmButton:
          "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded text-sm focus:outline-none",
        cancelButton: swalCustomClasses.cancelButton,
      },
    });

    if (result.isConfirmed) {
      const token = localStorage.getItem("token");
      console.log("token before reject:", token);

      const res = await fetch(
        `/api/requests/reject/${id}`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
          },
        }
      );

      if (res.ok) {
        MySwal.fire({
          icon: "success",
          title: "Rejected!",
          text: "Request rejected successfully.",
          confirmButtonText: "OK",
          customClass: {
            confirmButton:
              "bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded text-sm focus:outline-none",
          },
        }).then(() => {
          // ✅ ไม่ reload → fetch data ใหม่แทน
          fetchRequests();
        });
      } else {
        const text = await res.text();
        console.error("Error rejecting:", text);

        MySwal.fire({
          icon: "error",
          title: "Error",
          text: "Failed to reject request.",
        });
      }
    }
  };

  // const handleViewDetails = (
  //   oldData: string | null,
  //   newData: string | null
  // ) => {
  //   let oldJson = null;
  //   let newJson = null;

  //   try {
  //     oldJson = oldData ? JSON.parse(oldData) : null;
  //   } catch (e) {
  //     console.error("Failed to parse old_data:", e);
  //   }

  //   try {
  //     newJson = newData ? JSON.parse(newData) : null;
  //   } catch (e) {
  //     console.error("Failed to parse new_data:", e);
  //   }

  //   setModalOldData(oldJson);
  //   setModalNewData(newJson);
  //   setShowModal(true);
  // };

  // const handleViewDetails = (
  //   oldData: string | null,
  //   newData: string | null
  // ) => {
  //   let oldJson = null;
  //   let newJson = null;

  //   try {
  //     oldJson = oldData ? JSON.parse(oldData) : null;
  //   } catch (e) {
  //     console.error("Failed to parse old_data:", e);
  //   }

  //   try {
  //     newJson = newData ? JSON.parse(newData) : null;
  //   } catch (e) {
  //     console.error("Failed to parse new_data:", e);
  //   }

  //   setModalOldData(oldJson);
  //   setModalNewData(newJson);
  //   setShowModal(true);

  //   // ✅ รวม key จาก old & new
  //   const allToolKeyIds = new Set<number>();
  //   for (const d of [oldJson, newJson]) {
  //     if (d?.tool_key_id) {
  //       allToolKeyIds.add(Number(d.tool_key_id));
  //     }
  //   }

  //   // ✅ fetch รายตัว
  //   allToolKeyIds.forEach((id) => {
  //     fetchToolKeyAllIdsById(id).then((details) => {
  //       if (details) {
  //         setToolKeyDetailsMap((prev) => ({
  //           ...prev,
  //           [id]: details,
  //         }));
  //       }
  //     });
  //   });
  // };

  const handleViewDetails = (
    oldData: string | null,
    newData: string | null,
    targetTable: string
  ) => {
    let oldJson = null;
    let newJson = null;

    try {
      oldJson = oldData ? JSON.parse(oldData) : null;
    } catch (e) {
      console.error("Failed to parse old_data:", e);
    }

    try {
      newJson = newData ? JSON.parse(newData) : null;
    } catch (e) {
      console.error("Failed to parse new_data:", e);
    }

    setModalOldData(oldJson);
    setModalNewData(newJson);
    setShowModal(true);
    setSelectedTargetTable(targetTable);

    const toolKeyIds = new Set<number>();

    [oldJson, newJson].forEach((data) => {
      if (data?.tool_key_id) {
        toolKeyIds.add(Number(data.tool_key_id));
      }
    });

    toolKeyIds.forEach((id) => {
      fetchToolKeyAllIdsById(id);
    });
  };

  // const columns = useMemo<ColumnDef<RequestRow, any>[]>(
  //   () => [
  //     {
  //       accessorKey: "request_id",
  //       header: "ID",
  //       cell: ({ getValue }) => (
  //         <span className="font-semibold">{getValue()}</span>
  //       ),
  //     },
  //     {
  //       accessorKey: "request_type",
  //       header: "Type",
  //     },
  //     {
  //       accessorKey: "request_status",
  //       header: "Status",
  //       cell: ({ getValue }) => {
  //         const value = getValue();

  //         let baseClasses =
  //           "px-2 py-0.5 rounded text-xs font-medium inline-flex items-center gap-1";

  //         let colorClasses = "";
  //         let animateClasses = "";

  //         if (value === "Pending") {
  //           colorClasses = `
  //     text-yellow-900
  //     border border-yellow-300
  //     bg-gradient-to-r from-yellow-100 to-yellow-200
  //     shadow-sm
  //   `;
  //           animateClasses = "animate-pulse";
  //         } else if (value === "Completed") {
  //           colorClasses =
  //             "bg-green-100 text-green-700 border border-green-300";
  //         } else if (value === "Rejected") {
  //           colorClasses = "bg-red-100 text-red-700 border border-red-300";
  //         } else {
  //           colorClasses = "bg-gray-100 text-gray-700 border border-gray-300";
  //         }

  //         const icon =
  //           value === "Pending"
  //             ? "⏳"
  //             : value === "Completed"
  //             ? "✅"
  //             : value === "Rejected"
  //             ? "❌"
  //             : "";

  //         return (
  //           <span
  //             className={`${baseClasses} ${colorClasses} ${animateClasses} transition-all duration-300`}
  //           >
  //             {icon && <span>{icon}</span>}
  //             {value}
  //           </span>
  //         );
  //       },
  //     },
  //     {
  //       accessorKey: "target_table",
  //       header: "Target Table",
  //     },
  //     {
  //       accessorKey: "target_pk_id",
  //       header: "Target ID",
  //     },
  //     {
  //       accessorKey: "requested_by",
  //       header: "Requested By",
  //     },
  //     {
  //       accessorKey: "requested_at",
  //       header: "Requested At",
  //       cell: ({ getValue }) => new Date(getValue()).toLocaleString(),
  //     },
  //     {
  //       accessorKey: "approved_by",
  //       header: "Approved By",
  //     },
  //     {
  //       accessorKey: "approved_at",
  //       header: "Approved At",
  //       cell: ({ getValue }) =>
  //         getValue() ? new Date(getValue()).toLocaleString() : "-",
  //     },
  //     {
  //       id: "details",
  //       header: "Details",
  //       cell: ({ row }) => (
  //         <button
  //           onClick={() => {
  //             setSelectedRequestType(row.original.request_type);
  //             handleViewDetails(
  //               row.original.old_data,
  //               row.original.new_data,
  //               row.original.target_table // ✅ ส่ง table ไปด้วย
  //             );
  //           }}
  //           className="
  //       inline-flex items-center gap-1
  //       border border-gray-300
  //       text-gray-700
  //       hover:bg-gray-100
  //       px-2 py-1
  //       rounded
  //       text-xs
  //       transition-colors
  //     "
  //         >
  //           <svg
  //             xmlns="http://www.w3.org/2000/svg"
  //             className="h-3 w-3 text-gray-500"
  //             fill="none"
  //             viewBox="0 0 24 24"
  //             stroke="currentColor"
  //             strokeWidth={2}
  //           >
  //             <path
  //               strokeLinecap="round"
  //               strokeLinejoin="round"
  //               d="M15 12H9m12 0a9 9 0 11-18 0 9 9 0 0118 0z"
  //             />
  //           </svg>
  //           View Details
  //         </button>
  //       ),
  //     },
  //     {
  //       accessorKey: "note",
  //       header: "Note",
  //     },
  //     {
  //       id: "actions",
  //       header: "Actions",
  //       cell: ({ row }) => {
  //         if (row.original.request_status === "Pending") {
  //           return (
  //             <div className="flex gap-2 justify-center">
  //               <button
  //                 onClick={() => handleApprove(row.original.request_id)}
  //                 className="text-xs bg-green-500 hover:bg-green-600 text-white rounded px-2 py-1"
  //               >
  //                 Approve
  //               </button>
  //               <button
  //                 onClick={() => handleReject(row.original.request_id)}
  //                 className="text-xs bg-red-500 hover:bg-red-600 text-white rounded px-2 py-1"
  //               >
  //                 Reject
  //               </button>
  //             </div>
  //           );
  //         } else {
  //           return <span className="text-gray-500 text-xs">-</span>;
  //         }
  //       },
  //     },
  //   ],
  //   []
  // );
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

  const columns = useMemo<ColumnDef<RequestRow, any>[]>(
    () => [
      {
        accessorKey: "request_id",
        header: () => (
          <div className="flex items-center justify-center">
            ID
            {renderDropdownFilter("request_id")}
          </div>
        ),
        cell: ({ getValue }) => (
          <span className="font-semibold">{getValue()}</span>
        ),
      },
      {
        accessorKey: "request_type",
        header: () => (
          <div className="flex items-center justify-center">
            Type
            {renderDropdownFilter("request_type")}
          </div>
        ),
      },
      {
        accessorKey: "request_status",
        header: () => (
          <div className="flex items-center justify-center">
            Status
            {renderDropdownFilter("request_status")}
          </div>
        ),
        cell: ({ getValue }) => {
          const value = getValue();

          let baseClasses =
            "px-2 py-0.5 rounded text-xs font-medium inline-flex items-center gap-1";

          let colorClasses = "";
          let animateClasses = "";

          if (value === "Pending") {
            colorClasses = `
            text-yellow-900 
            border border-yellow-300 
            bg-gradient-to-r from-yellow-100 to-yellow-200
            shadow-sm
          `;
            animateClasses = "animate-pulse";
          } else if (value === "Completed") {
            colorClasses =
              "bg-green-100 text-green-700 border border-green-300";
          } else if (value === "Rejected") {
            colorClasses = "bg-red-100 text-red-700 border border-red-300";
          } else {
            colorClasses = "bg-gray-100 text-gray-700 border border-gray-300";
          }

          const icon =
            value === "Pending"
              ? "\u23F3"
              : value === "Completed"
              ? "\u2705"
              : value === "Rejected"
              ? "\u274C"
              : "";

          return (
            <span
              className={`${baseClasses} ${colorClasses} ${animateClasses} transition-all duration-300`}
            >
              {icon && <span>{icon}</span>}
              {value}
            </span>
          );
        },
      },
      {
        accessorKey: "target_table",
        header: () => (
          <div className="flex items-center justify-center">
            Target Table
            {renderDropdownFilter("target_table")}
          </div>
        ),
      },
      // {
      //   accessorKey: "target_pk_id",
      //   header: () => (
      //     <div className="flex items-center justify-center">
      //       Target ID
      //       {renderDropdownFilter("target_pk_id")}
      //     </div>
      //   ),
      // },
      {
        accessorKey: "requested_by",
        header: () => (
          <div className="flex items-center justify-center">
            Requested By
            {renderDropdownFilter("requested_by")}
          </div>
        ),
      },
      {
        accessorKey: "requested_at",
        header: () => (
          <div className="flex items-center justify-center">Requested At</div>
        ),
        cell: ({ getValue }) => formatDateTime(getValue()),
      },
      {
        accessorKey: "approved_by",
        header: () => (
          <div className="flex items-center justify-center">
            Approved By
            {renderDropdownFilter("approved_by")}
          </div>
        ),
      },
      {
        accessorKey: "approved_at",
        header: () => (
          <div className="flex items-center justify-center">Approved At</div>
        ),
        cell: ({ getValue }) => formatDateTime(getValue()),
      },
      {
        id: "details",
        header: "Details",
        cell: ({ row }) => (
          <button
            onClick={() => {
              setSelectedRequestType(row.original.request_type);
              handleViewDetails(
                row.original.old_data,
                row.original.new_data,
                row.original.target_table
              );
            }}
            className="inline-flex items-center gap-1 border border-gray-300 text-gray-700 hover:bg-gray-100 px-2 py-1 rounded text-xs transition-colors"
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
        ),
      },
      {
        accessorKey: "note",
        header: () => (
          <div className="flex items-center justify-center">
            Note
            {renderDropdownFilter("note")}
          </div>
        ),
      },
      {
        id: "actions",
        header: "Actions",
        cell: ({ row }) => {
          if (row.original.request_status === "Pending") {
            return (
              <div className="flex gap-2 justify-center">
                <button
                  onClick={() => handleApprove(row.original.request_id)}
                  className="text-xs bg-green-500 hover:bg-green-600 text-white rounded px-2 py-1"
                >
                  Approve
                </button>
                <button
                  onClick={() => handleReject(row.original.request_id)}
                  className="text-xs bg-red-500 hover:bg-red-600 text-white rounded px-2 py-1"
                >
                  Reject
                </button>
              </div>
            );
          } else {
            return <span className="text-gray-500 text-xs">-</span>;
          }
        },
      },
    ],
    [renderDropdownFilter]
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
    <div className="space-y-4">
      {/* ✅ 3 Cards: Pending by Request Type */}
      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
        {/* INSERT */}
        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-200">
          <div className="flex items-center">
            <div className="p-2 bg-blue-100 rounded-lg">
              <PlusCircle className="w-6 h-6 text-blue-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">
                Pending INSERT
              </p>
              <p className="text-2xl font-bold text-gray-900">
                {
                  data.filter(
                    (r) =>
                      r.request_status === "Pending" &&
                      r.request_type.toUpperCase() === "INSERT"
                  ).length
                }
              </p>
            </div>
          </div>
        </div>

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
                    (r) =>
                      r.request_status === "Pending" &&
                      r.request_type.toUpperCase() === "UPDATE"
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
                    (r) =>
                      r.request_status === "Pending" &&
                      r.request_type.toUpperCase() === "DELETE"
                  ).length
                }
              </p>
            </div>
          </div>
        </div>
      </div>

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

            <div ref={dropdownRefs} className="relative">
              <button
                ref={columnButtonRef}
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

              {/* {showColumnDropdown && (
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
              )} */}
              {showColumnDropdown &&
                createPortal(
                  <div
                    ref={columnDropdownRef}
                    className="fixed z-[9999] w-64 bg-white border border-gray-200 rounded-lg shadow-lg animate-in fade-in-0 zoom-in-95 duration-200 max-h-80 overflow-hidden"
                    style={
                      columnDropdownPosition
                        ? {
                            top: columnDropdownPosition.top,
                            left: columnDropdownPosition.left,
                            minWidth: columnDropdownPosition.width,
                          }
                        : { visibility: "hidden" }
                    }
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
          </div>
          <EnhancedDateTimeFilter
            label="Filter Requests"
            filterMode="datetime"
            initialFilters={{
              requested_at_start: dateRange.requested_at_start,
              requested_at_end: dateRange.requested_at_end,
              approved_at_start: dateRange.approved_at_start,
              approved_at_end: dateRange.approved_at_end,
            }}
            onFilterApply={(filters) => {
              setDateRange(filters); // ✅ อัปเดต dateRange state (ยังเก็บไว้ใช้ในหน้า)

              // ✅ รวมเข้า filterValues เพื่อยิง backend ใหม่
              const updated = {
                ...filterValues,

                ...(filters.requested_at_start
                  ? { requested_at_start: [filters.requested_at_start] }
                  : { requested_at_start: [] }),

                ...(filters.requested_at_end
                  ? { requested_at_end: [filters.requested_at_end] }
                  : { requested_at_end: [] }),

                ...(filters.approved_at_start
                  ? { approved_at_start: [filters.approved_at_start] }
                  : { approved_at_start: [] }),

                ...(filters.approved_at_end
                  ? { approved_at_end: [filters.approved_at_end] }
                  : { approved_at_end: [] }),
              };

              setFilterValues(updated); // ✅ ไปกระตุ้น useEffect(fetchRequests)
            }}
          />
        </div>
      </div>

      <div className="overflow-auto max-h-[70vh] border rounded">
        <table className="min-w-full text-sm text-center">
          <thead className="sticky top-0 bg-gray-100">
            {table.getHeaderGroups().map((headerGroup) => (
              <tr key={headerGroup.id}>
                {headerGroup.headers.map((header) => (
                  <th
                    key={header.id}
                    colSpan={header.colSpan}
                    className="px-2 py-2 border font-semibold text-xs text-gray-700 whitespace-nowrap"
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
              <tr key={row.id} className="hover:bg-blue-50 even:bg-gray-50">
                {row.getVisibleCells().map((cell) => (
                  <td
                    key={cell.id}
                    className="px-2 py-2 border whitespace-nowrap"
                  >
                    {flexRender(cell.column.columnDef.cell, cell.getContext())}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="flex justify-between items-center border-t pt-2 text-sm">
        <div className="flex items-center">
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
        <div>
          {(() => {
            const { pageIndex, pageSize } = table.getState().pagination;
            const total = data.length;
            const start = pageIndex * pageSize + 1;
            const end = Math.min(start + pageSize - 1, total);
            return `${start}–${end} of ${total}`;
          })()}
        </div>
        <div className="flex items-center gap-1">
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
        {showModal && (
          <div className="fixed inset-0 z-[9999] bg-black bg-opacity-50 flex justify-center items-center p-4">
            <div className="bg-white rounded-lg shadow-lg max-w-5xl w-full max-h-[90vh] overflow-y-auto p-6">
              <div className="flex justify-between items-center mb-4">
                <h2 className="text-lg font-bold">Request Details</h2>
                <button
                  onClick={() => setShowModal(false)}
                  className="text-gray-500 hover:text-gray-700"
                >
                  ✕
                </button>
              </div>

              <table className="w-full text-sm border">
                <thead className="bg-gray-100">
                  <tr>
                    <th className="text-left p-2">Field</th>
                    <th className="text-left p-2">Old Value</th>
                    <th className="text-left p-2">New Value</th>
                  </tr>
                </thead>
                <tbody>
                  {(() => {
                    const desiredOrder =
                      tableFieldMap[selectedTargetTable] || [];

                    const allKeysSet = new Set([
                      ...Object.keys(modalOldData || {}),
                      ...Object.keys(modalNewData || {}),
                    ]);

                    // ✅ แทรก fields จาก tool_key_id ถ้ามี
                    const toolKeyIds = new Set<number>();

                    [modalOldData, modalNewData].forEach((data) => {
                      if (data?.tool_key_id) {
                        toolKeyIds.add(Number(data.tool_key_id));
                      }
                    });

                    toolKeyIds.forEach((id) => {
                      const details = toolKeyDetailsMap[id];
                      if (details) {
                        Object.keys(details).forEach((key) => {
                          if (key !== "tool_key_id") {
                            allKeysSet.add(key);
                          }
                        });
                      }
                    });

                    const hiddenFields = new Set([
                      "create_by",
                      "create_at",
                      "update_by",
                      "update_at",
                      "createByUser",
                      "updateByUser",
                      "refKey",
                      "toolRefSpec",
                      "pending_request",
                      "axleType",
                      "positionType",
                    ]);

                    const allKeys = [
                      ...desiredOrder.filter(
                        (k) =>
                          allKeysSet.has(k) &&
                          !k.toLowerCase().endsWith("id") &&
                          !hiddenFields.has(k)
                      ),
                      ...Array.from(allKeysSet).filter(
                        (k) =>
                          !desiredOrder.includes(k) &&
                          !k.toLowerCase().endsWith("id") &&
                          !hiddenFields.has(k)
                      ),
                    ];

                    const isChanged = (oldVal: any, newVal: any) => {
                      return String(oldVal ?? "") !== String(newVal ?? "");
                    };

                    const getDisplayValue = (key: string, value: any) => {
                      if (value == null || value === "") return "";

                      switch (key) {
                        case "tool_id":
                          return toolsMap[Number(value)] || value;
                        case "type_id":
                          return typesMap[Number(value)] || value;
                        case "size_ref_id":
                          return sizeRefsMap[Number(value)] || value;
                        case "axle_type_id":
                          return axleTypesMap[Number(value)] || value;
                        // case "hst_type_id":
                        //   return hstTypesMap[Number(value)] || value;
                        // case "pad_id":
                        //   return padsMap[Number(value)] || value;
                        default:
                          return String(value);
                      }
                    };

                    return allKeys.map((key) => {
                      let oldVal = modalOldData?.[key] ?? "";
                      let newVal = modalNewData?.[key];

                      // ✅ lookup จาก toolKeyDetailsMap → Old
                      if (key !== "tool_key_id" && modalOldData?.tool_key_id) {
                        const details =
                          toolKeyDetailsMap[modalOldData.tool_key_id];
                        if (details && details.hasOwnProperty(key)) {
                          oldVal = details[key];
                        }
                      }

                      // ✅ lookup จาก toolKeyDetailsMap → New
                      if (key !== "tool_key_id" && modalNewData?.tool_key_id) {
                        const details =
                          toolKeyDetailsMap[modalNewData.tool_key_id];
                        if (details && details.hasOwnProperty(key)) {
                          newVal = details[key];
                        }
                      }

                      let changed = false;
                      let shouldHighlight = false;

                      // ✅ compare ค่าเสมอ (ไม่ต้องสนว่ามี key อยู่ใน newData หรือไม่)
                      changed = isChanged(oldVal, newVal);
                      shouldHighlight =
                        selectedRequestType?.toUpperCase() === "UPDATE" &&
                        changed;

                      return (
                        <tr
                          key={key}
                          className={
                            shouldHighlight
                              ? "bg-yellow-50 transition-colors duration-300"
                              : ""
                          }
                        >
                          <td className="p-2 font-medium text-gray-700 border-b">
                            {key}
                          </td>
                          <td className="p-2 text-gray-800 border-b">
                            {getDisplayValue(key, oldVal)}
                          </td>
                          <td
                            className={`p-2 border-b ${
                              shouldHighlight
                                ? "text-yellow-700 font-semibold"
                                : "text-gray-800"
                            }`}
                          >
                            {getDisplayValue(key, newVal)}
                          </td>
                        </tr>
                      );
                    });
                  })()}
                </tbody>
              </table>

              <div className="mt-6 flex justify-end">
                <button
                  onClick={() => setShowModal(false)}
                  className="bg-blue-600 hover:bg-blue-700 text-white text-sm px-4 py-2 rounded"
                >
                  Close
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default RequestTable;
