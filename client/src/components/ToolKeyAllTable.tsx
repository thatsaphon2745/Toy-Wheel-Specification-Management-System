import React, { useEffect, useMemo, useRef, useState } from "react";
import {
  useReactTable,
  getCoreRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  flexRender,
} from "@tanstack/react-table";
import type { ColumnDef, VisibilityState } from "@tanstack/react-table";
import Papa from "papaparse";

import ToolKeyOriginalPhotoModal from "./ToolKeyOriginalPhotoModal";

interface ToolKeyAllRow {
  tool_type: string;
  tool_name: string;
  type_ref: string;
  tool_ref: string;
  size_ref: string;
  original_spec: string;
  ref_spec: string;
  knurling: string;

  tool_key_id?: number; // ✅ ต้องให้ API ส่งมาด้วย

  image_url?: string | null;
  pic_before_hst_file_name?: string | null;

  [key: string]: string | undefined | null | number;
}
import ExcelJS from "exceljs";
import { saveAs } from "file-saver";

// ⬇️ แก้ ImageCell เดิมให้เป็นแบบนี้
const ImageCell: React.FC<{
  url?: string | null;
  fileName?: string | null;
  title?: string;
  onOpen?: () => void; // ✅ เพิ่ม
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
    if (onOpen) {
      onOpen(); // ✅ เปิดโมดัลจัดการรูป
    } else if (url && !failed) {
      window.open(url, "_blank", "noopener,noreferrer"); // fallback zoom
    }
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
      role="button" // ✅ เข้าถึงด้วยคีย์บอร์ด
      tabIndex={0}
      onClick={handleClick}
      onKeyDown={handleKey}
      title={title ?? fileName ?? ""}
    >
      <img
        src={src}
        alt={fileName ?? "after-hst"}
        loading="lazy"
        onError={() => setFailed(true)}
        className="w-full h-full object-cover"
      />
      {/* ✅ Hover overlay บอกว่าแก้ไขได้ */}
      <div className="absolute inset-0 bg-black/0 group-hover:bg-black/30 transition-colors" />
      <div className="absolute bottom-1 left-1 right-1 opacity-0 group-hover:opacity-100 transition-opacity">
        <div className="text-[10px] leading-tight text-white bg-black/60 rounded px-1 py-0.5 text-center">
          {url && !failed ? "Edit / Replace Photo" : "Upload Photo"}
        </div>
      </div>
    </div>
  );
};

const ToolKeyAllTable: React.FC = () => {
  const [data, setData] = useState<ToolKeyAllRow[]>([]);
  const [filterOptions, setFilterOptions] = useState<{
    [key: string]: string[];
  }>({});
  const [filterValues, setFilterValues] = useState<{ [key: string]: string[] }>(
    {}
  );
  const [pagination, setPagination] = useState({ pageIndex: 0, pageSize: 15 });
  const [dropdownOpen, setDropdownOpen] = useState<{ [key: string]: boolean }>(
    {}
  );
  const [columnVisibility, setColumnVisibility] = useState<VisibilityState>({});
  const [showColumnDropdown, setShowColumnDropdown] = useState(false);
  const [search, setSearch] = useState("");

  const dropdownRefs = useRef<HTMLDivElement>(null);

  const [showPhotoModal, setShowPhotoModal] = useState(false);
  const [photoContext, setPhotoContext] = useState<{
    tool_key_id: number;
    url?: string | null;
    name?: string | null;
  } | null>(null);

  const handleExportXLSX = async () => {
    const workbook = new ExcelJS.Workbook();
    const sheet = workbook.addWorksheet("ToolKeyAlls");

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

    // ✅ Data rows (ไม่มีสี)
    const rows = table.getFilteredRowModel().rows;
    rows.forEach((row, rowIdx) => {
      const excelRowIdx = rowIdx + 2;

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
        };
        cell.alignment = { vertical: "middle", horizontal: "center" };
        cell.border = {
          top: { style: "thin" },
          left: { style: "thin" },
          bottom: { style: "thin" },
          right: { style: "thin" },
        };
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
    saveAs(blob, "ToolKeyAlls.xlsx");
  };

  const fetchData = React.useCallback(() => {
    fetch("/api/ToolKeyAlls")
      .then((res) => res.json())
      .then((result: ToolKeyAllRow[]) => {
        setData(result);
        const keys = Object.keys(result[0] || {});
        const newOptions: { [key: string]: string[] } = {};
        keys.forEach((key) => {
          newOptions[key] = Array.from(
            new Set(result.map((item) => String(item[key] ?? "")))
          ).sort();
        });
        setFilterOptions(newOptions);
      });
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  // useEffect(() => {
  //   fetch("/api/ToolKeyAlls")
  //     .then((res) => res.json())
  //     .then((result: ToolKeyAllRow[]) => {
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
  // }, []);

  const filteredData = useMemo(() => {
    return data.filter((row) =>
      Object.entries(filterValues).every(([key, selected]) => {
        if (!selected.length) return true;
        const val = String(row[key] ?? "");
        return selected.includes(val);
      })
    );
  }, [data, filterValues]);

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
      const stringified = val ?? "null"; // ← ใช้ string 'null' แทนค่า null
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
          className="ml-1 px-1 py-0.5 border text-xs bg-white rounded shadow"
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

  // const columns = useMemo<ColumnDef<ToolKeyAllRow, any>[]>(() => {
  //   const keys = Object.keys(data[0] || {});
  //   return keys.map((key) => ({
  //     accessorKey: key,
  //     header: () => (
  //       <div className="flex items-center justify-center">
  //         {key}
  //         {renderDropdownFilter(key)}
  //       </div>
  //     ),
  //   }));
  // }, [data, filterOptions, filterValues, dropdownOpen]);

  // const columns = useMemo<ColumnDef<ToolKeyAllRow, any>[]>(() => {
  //   // ❌ ตัดคอลัมน์ pic_before_hst_file_name ออกไปเลย
  //   const EXCLUDED_KEYS = new Set(["pic_before_hst_file_name", "tool_key_id"]);
  //   const keys = Object.keys(data[0] || {}).filter(
  //     (k) => !EXCLUDED_KEYS.has(k)
  //   );
  //   return keys.map((key) => {
  //     const isImage = key === "image_url";

  //     const column: ColumnDef<ToolKeyAllRow, any> = {
  //       accessorKey: key,
  //       header: () => (
  //         <div className="flex items-center justify-center">
  //           {isImage ? "Photo" : key}
  //           {renderDropdownFilter(key)}
  //         </div>
  //       ),
  //     };

  //     // ✅ เรนเดอร์รูปด้วย ImageCell เฉพาะคอลัมน์ image_url
  //     if (isImage) {
  //       column.cell = ({ row }) => (
  //         <ImageCell
  //           url={row.original.image_url as string | null}
  //           fileName={row.original.pic_before_hst_file_name as string | null} // ใช้เป็น tooltip ได้ แม้ไม่แสดงคอลัมน์
  //           title={`${row.original.tool_name ?? ""} ${
  //             row.original.size_ref ?? ""
  //           }`}
  //           onOpen={() => {
  //             setPhotoContext({
  //               map_id: (row.original as any).map_id, // ถ้ามีในชุดข้อมูล
  //               url: row.original.image_url,
  //               name: row.original.pic_before_hst_file_name,
  //             });
  //             setShowPhotoModal(true);
  //           }}
  //         />
  //       );
  //       // ปกติคอลัมน์รูปไม่ต้อง sort
  //       (column as any).enableSorting = false;
  //     }

  //     return column;
  //   });
  // }, [data, filterOptions, filterValues, dropdownOpen]);

  const columns = useMemo<ColumnDef<ToolKeyAllRow, any>[]>(() => {
    // ตัดคอลัมน์ที่ไม่อยากโชว์ แต่ยังใช้ค่าภายในได้
    const EXCLUDED_KEYS = new Set(["pic_before_hst_file_name", "tool_key_id"]);
    const keys = Object.keys(data[0] || {}).filter(
      (k) => !EXCLUDED_KEYS.has(k)
    );

    return keys.map((key) => {
      const isImage = key === "image_url";

      const column: ColumnDef<ToolKeyAllRow, any> = {
        accessorKey: key,
        header: () => (
          <div className="flex items-center justify-center">
            {isImage ? "Photo" : key}
            {/* คอลัมน์รูปไม่ต้องมี filter */}
            {!isImage && renderDropdownFilter(key)}
          </div>
        ),
      };

      if (isImage) {
        column.cell = ({ row }) => (
          <ImageCell
            url={row.original.image_url as string | null}
            fileName={row.original.pic_before_hst_file_name as string | null}
            title={`${row.original.tool_name ?? ""} ${
              row.original.size_ref ?? ""
            }`}
            onOpen={() => {
              const originalKeyId = (row.original.tool_key_id ??
                (row.original as any).tool_key_id) as number | undefined;
              if (!originalKeyId) return;

              setPhotoContext({
                tool_key_id: row.original.tool_key_id as number,
                url: row.original.image_url,
                name: row.original.pic_before_hst_file_name,
              });
              setShowPhotoModal(true);
            }}
          />
        );
        (column as any).enableSorting = false;
      }

      return column;
    });
  }, [data, filterOptions, filterValues, dropdownOpen]);

  const table = useReactTable({
    data: filteredData,
    columns,
    state: {
      pagination,
      columnVisibility,
    },
    onPaginationChange: setPagination,
    onColumnVisibilityChange: setColumnVisibility,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
  });

  return (
    <div className="space-y-4 relative">
      {/* <div className="flex justify-between items-center px-4">
        <button
          className="border px-3 py-1 text-sm rounded bg-white shadow"
          onClick={() => {
            const csv = Papa.unparse(filteredData);
            const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
            const url = URL.createObjectURL(blob);
            const link = document.createElement("a");
            link.href = url;
            link.download = "ToolKeyAll.csv";
            link.click();
          }}
        >
          Export CSV
        </button>

        <div className="relative">
          <button
            className="border px-3 py-1 text-sm rounded bg-white shadow"
            onClick={() => setShowColumnDropdown((prev) => !prev)}
          >
            Toggle Columns
          </button>
          {showColumnDropdown && (
            <div className="absolute top-full mt-2 right-0 w-64 border max-h-80 overflow-auto rounded bg-white shadow z-50">
              <input
                type="text"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Search columns..."
                className="border-b px-2 py-1 w-full text-sm"
              />
              <div className="max-h-64 overflow-y-auto">
                {table
                  .getAllLeafColumns()
                  .filter((col) =>
                    col.id?.toLowerCase().includes(search.toLowerCase())
                  )
                  .map((column) => (
                    <label
                      key={column.id}
                      className="flex items-center px-2 py-1 text-sm"
                    >
                      <input
                        type="checkbox"
                        checked={column.getIsVisible()}
                        onChange={() => column.toggleVisibility()}
                        className="mr-2"
                      />
                      {column.columnDef.header?.toString()}
                    </label>
                  ))}
              </div>
            </div>
          )}
        </div>
      </div> */}

      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4">
        <div className="flex flex-wrap gap-3 items-center justify-between">
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
          </div>
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
                className="hover:bg-blue-50 transition-colors duration-100 even:bg-gray-50"
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
            const total = filteredData.length;
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
      {showPhotoModal && photoContext && (
        <ToolKeyOriginalPhotoModal
          originalKeyId={photoContext.tool_key_id}
          phase="before-hst" // ✅ ช่องรูปฝั่ง Original
          currentUrl={photoContext.url ?? undefined}
          currentName={photoContext.name ?? undefined}
          onClose={() => setShowPhotoModal(false)}
          onSuccess={() => {
            setShowPhotoModal(false);
            fetchData(); // ✅ รีเฟรชตารางหลังอัปเดต/ลบรูป
          }}
        />
      )}
    </div>
  );
};

export default ToolKeyAllTable;
