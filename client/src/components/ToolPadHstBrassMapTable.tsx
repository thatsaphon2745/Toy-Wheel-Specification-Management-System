// ✅ ToolPadHstBrassMapTable.tsx (Only Pivot View, No Toggle)
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

interface ToolPadHstBrassRow {
  tool_type: string;
  tool_name: string;
  type_ref: string;
  tool_ref: string;
  size_ref: string;
  HST_pad?: string;
  RIM_pad?: string;
  INNER_pad?: string;
  EXTRA_RIM_pad?: string;
  HST_brass?: string;
  RIM_brass?: string;
  INNER_brass?: string;
  EXTRA_RIM_brass?: string;
  [key: string]: string | undefined;
}

import ExcelJS from "exceljs";
import { saveAs } from "file-saver";

const ToolPadHstBrassMapTable: React.FC = () => {
  const [data, setData] = useState<ToolPadHstBrassRow[]>([]);
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

  const handleExportXLSX = async () => {
    const workbook = new ExcelJS.Workbook();
    const sheet = workbook.addWorksheet("ToolPadHstBrassMap");

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
    saveAs(blob, "ToolPadHstBrassMap.xlsx");
  };

  useEffect(() => {
    fetch("/api/ToolPadHstBrassMap/pivot")
      .then((res) => res.json())
      .then((result: ToolPadHstBrassRow[]) => {
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

  const columns = useMemo<ColumnDef<ToolPadHstBrassRow, any>[]>(() => {
    const keys = Object.keys(data[0] || {});
    return keys.map((key) => ({
      accessorKey: key,
      header: () => (
        <div className="flex items-center justify-center">
          {key}
          {renderDropdownFilter(key)}
        </div>
      ),
    }));
  }, [data, filterOptions, filterValues, dropdownOpen]);

  const table = useReactTable({
    data: filteredData,
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
      {/* <div className="flex justify-between items-center px-4">
        <button
          className="border px-3 py-1 text-sm rounded bg-white shadow"
          onClick={() => {
            const csv = Papa.unparse(filteredData); // ✅ ใช้ filteredData แทน
            const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
            const url = URL.createObjectURL(blob);
            const link = document.createElement("a");
            link.href = url;
            link.download = "ToolPadHstBrassMap.csv";
            link.click();
          }}
        >
          Export CSV
        </button>
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
    </div>
  );
};

export default ToolPadHstBrassMapTable;
