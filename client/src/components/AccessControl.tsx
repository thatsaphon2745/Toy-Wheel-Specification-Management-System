import React, { useEffect, useState } from "react";
import axios from "axios";
import { Switch } from "@headlessui/react";
import Swal from "sweetalert2";
import { BadgeCheck } from "lucide-react";

export interface UserDto {
  user_id: number;
  first_name: string;
  last_name: string;
  role_name?: string;
  role_id?: number;
  status_name?: string;
  status_id?: number;
  department_name?: string;
  employee_id?: string;
  email?: string;
}

export interface UserStatus {
  status_id: number;
  status_name: string;
}

export interface Role {
  role_id: number;
  role_name: string;
}

const AccessControl: React.FC = () => {
  const [users, setUsers] = useState<UserDto[]>([]);
  const [filteredUsers, setFilteredUsers] = useState<UserDto[]>([]);
  const [statuses, setStatuses] = useState<UserStatus[]>([]);
  const [roles, setRoles] = useState<Role[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingUser, setEditingUser] = useState<UserDto | null>(null);
  const [saving, setSaving] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");
  const [roleFilter, setRoleFilter] = useState("all");
  const [sortBy, setSortBy] = useState<
    "name" | "role" | "department" | "status"
  >("name");
  const [sortOrder, setSortOrder] = useState<"asc" | "desc">("asc");

  const userRole = getUserRoleFromToken();
  const canEdit = userRole === "admin" || userRole === "editor";

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    filterAndSortUsers();
  }, [users, searchTerm, statusFilter, roleFilter, sortBy, sortOrder]);

  const loadData = async () => {
    try {
      setLoading(true);
      const [userRes, statusRes, roleRes] = await Promise.all([
        axios.get<UserDto[]>("/api/Users"),
        axios.get<UserStatus[]>("/api/UserStatuses"),
        axios.get<Role[]>("/api/Roles"),
      ]);

      setUsers(userRes.data);
      setStatuses(statusRes.data);
      setRoles(roleRes.data);
    } catch (err) {
      console.error("Error loading data:", err);
      Swal.fire({
        icon: "error",
        title: "Error Loading Data",
        text: "Failed to load user data. Please refresh the page.",
        customClass: {
          confirmButton:
            "bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700",
          popup: "rounded-lg shadow-xl",
        },
        buttonsStyling: false,
      });
    } finally {
      setLoading(false);
    }
  };

  const filterAndSortUsers = () => {
    let filtered = users.filter((user) => {
      const matchesSearch =
        `${user.first_name} ${user.last_name}`
          .toLowerCase()
          .includes(searchTerm.toLowerCase()) ||
        user.email?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        user.employee_id?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        user.department_name?.toLowerCase().includes(searchTerm.toLowerCase());

      const matchesStatus =
        statusFilter === "all" ||
        (statusFilter === "active" && user.status_id === 1) ||
        (statusFilter === "inactive" && user.status_id !== 1);

      const matchesRole =
        roleFilter === "all" || user.role_id?.toString() === roleFilter;

      return matchesSearch && matchesStatus && matchesRole;
    });

    filtered.sort((a, b) => {
      let aValue: string, bValue: string;

      switch (sortBy) {
        case "name":
          aValue = `${a.first_name} ${a.last_name}`;
          bValue = `${b.first_name} ${b.last_name}`;
          break;
        case "role":
          aValue = a.role_name || "";
          bValue = b.role_name || "";
          break;
        case "department":
          aValue = a.department_name || "";
          bValue = b.department_name || "";
          break;
        case "status":
          aValue = a.status_name || "";
          bValue = b.status_name || "";
          break;
        default:
          aValue = `${a.first_name} ${a.last_name}`;
          bValue = `${b.first_name} ${b.last_name}`;
      }

      const comparison = aValue.localeCompare(bValue);
      return sortOrder === "asc" ? comparison : -comparison;
    });

    setFilteredUsers(filtered);
  };

  const handleSort = (column: typeof sortBy) => {
    if (sortBy === column) {
      setSortOrder(sortOrder === "asc" ? "desc" : "asc");
    } else {
      setSortBy(column);
      setSortOrder("asc");
    }
  };

  const handleSave = async () => {
    if (!editingUser) return;

    const result = await Swal.fire({
      title: "Confirm Role Update",
      text: `Update ${editingUser.first_name} ${editingUser.last_name}'s role?`,
      icon: "question",
      showCancelButton: true,
      confirmButtonText: "Update Role",
      cancelButtonText: "Cancel",
      customClass: {
        confirmButton:
          "bg-blue-600 text-white px-6 py-2 rounded-lg hover:bg-blue-700 transition-colors",
        cancelButton:
          "bg-gray-300 text-gray-800 px-6 py-2 rounded-lg hover:bg-gray-400 ml-3 transition-colors",
        popup: "rounded-xl shadow-2xl",
      },
      buttonsStyling: false,
    });

    if (!result.isConfirmed) return;

    if (!editingUser.role_id) {
      await Swal.fire({
        icon: "error",
        title: "Role Required",
        text: "Please select a role before saving.",
        customClass: {
          confirmButton:
            "bg-blue-600 text-white px-6 py-2 rounded-lg hover:bg-blue-700",
          popup: "rounded-xl shadow-2xl",
        },
        buttonsStyling: false,
      });
      return;
    }

    try {
      setSaving(true);

      await axios.patch(
        `/api/Users/${editingUser.user_id}/Role`,
        { role_id: editingUser.role_id },
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem("token")}`,
          },
        }
      );

      await Swal.fire({
        icon: "success",
        title: "Role Updated!",
        text: "User role has been updated successfully.",
        timer: 2000,
        showConfirmButton: false,
        customClass: {
          popup: "rounded-xl shadow-2xl",
        },
      });

      setEditingUser(null);
      await loadData();
    } catch (err: any) {
      console.error("Error updating role:", err);
      await Swal.fire({
        icon: "error",
        title: "Update Failed",
        text:
          err.response?.data?.message ||
          "Failed to update user role. Please try again.",
        customClass: {
          confirmButton:
            "bg-blue-600 text-white px-6 py-2 rounded-lg hover:bg-blue-700",
          popup: "rounded-xl shadow-2xl",
        },
        buttonsStyling: false,
      });
    } finally {
      setSaving(false);
    }
  };

  const toggleStatus = async (user: UserDto) => {
    try {
      const newStatusId = user.status_id === 1 ? 4 : 1;
      const newStatusName = user.status_id === 1 ? "Deactive" : "Active";

      // Optimistic update
      setUsers((prev) =>
        prev.map((u) =>
          u.user_id === user.user_id
            ? { ...u, status_id: newStatusId, status_name: newStatusName }
            : u
        )
      );

      await axios.patch(
        `/api/Users/${user.user_id}/ToggleStatus`,
        {},
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem("token")}`,
          },
        }
      );

      // Refresh data to ensure sync
      await loadData();
    } catch (err) {
      console.error("Error toggling status:", err);
      // Revert optimistic update on error
      await loadData();

      Swal.fire({
        icon: "error",
        title: "Status Update Failed",
        text: "Failed to update user status. Please try again.",
        customClass: {
          confirmButton:
            "bg-blue-600 text-white px-6 py-2 rounded-lg hover:bg-blue-700",
          popup: "rounded-xl shadow-2xl",
        },
        buttonsStyling: false,
      });
    }
  };

  const getInitials = (firstName: string, lastName: string) => {
    return `${firstName?.charAt(0) || ""}${
      lastName?.charAt(0) || ""
    }`.toUpperCase();
  };

  const getAvatarColor = (userId: number) => {
    const colors = [
      "bg-blue-600",
      "bg-green-600",
      "bg-purple-600",
      "bg-pink-600",
      "bg-indigo-600",
      "bg-red-600",
      "bg-yellow-600",
      "bg-teal-600",
    ];
    return colors[userId % colors.length];
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600 text-lg">Loading user data...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen ">
      <div className="w-full px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        {/* <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Access Control</h1>
          <p className="text-gray-600">Manage user roles and permissions across your organization</p>
        </div> */}

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
          <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-200">
            <div className="flex items-center">
              <div className="p-2 bg-blue-100 rounded-lg">
                <svg
                  className="w-6 h-6 text-blue-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z"
                  />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Total Users</p>
                <p className="text-2xl font-bold text-gray-900">
                  {users.length}
                </p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-200">
            <div className="flex items-center">
              <div className="p-2 bg-green-100 rounded-lg">
                <svg
                  className="w-6 h-6 text-green-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">
                  Active Users
                </p>
                <p className="text-2xl font-bold text-gray-900">
                  {users.filter((u) => u.status_id === 1).length}
                </p>
              </div>
            </div>
          </div>

          {/* <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-200">
            <div className="flex items-center">
              <div className="p-2 bg-purple-100 rounded-lg">
                <svg className="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Departments</p>
                <p className="text-2xl font-bold text-gray-900">{new Set(users.map(u => u.department_name).filter(Boolean)).size}</p>
              </div>
            </div>
          </div> */}

          {/* <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-200">
            <div className="flex items-center">
              <div className="p-2 bg-orange-100 rounded-lg">
                <svg className="w-6 h-6 text-orange-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Roles</p>
                <p className="text-2xl font-bold text-gray-900">{roles.length}</p>
              </div>
            </div>
          </div> */}
        </div>

        {/* Filters and Search */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6 mb-6">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            {/* Search */}
            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Search Users
              </label>
              <div className="relative">
                <svg
                  className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
                  />
                </svg>
                <input
                  type="text"
                  placeholder="Search by Name, Email, , Employee ID or Department..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full pl-10 pr-4 py-2.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
            </div>

            {/* Status Filter */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Status
              </label>
              <div className="relative">
                <select
                  value={statusFilter}
                  onChange={(e) => setStatusFilter(e.target.value)}
                  className="w-full appearance-none bg-white border border-gray-300 text-sm rounded-lg py-2.5 pl-3 pr-10 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                >
                  <option value="all">All Statuses</option>
                  <option value="active">Active Only</option>
                  <option value="inactive">Inactive Only</option>
                </select>
                <div className="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3 text-gray-400">
                  <svg
                    className="w-4 h-4"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth={2}
                    viewBox="0 0 24 24"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      d="M19 9l-7 7-7-7"
                    />
                  </svg>
                </div>
              </div>
            </div>

            {/* Role Filter */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Role
              </label>
              <div className="relative">
                <select
                  value={roleFilter}
                  onChange={(e) => setRoleFilter(e.target.value)}
                  className="w-full appearance-none bg-white border border-gray-300 text-sm rounded-lg py-2.5 pl-3 pr-10 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                >
                  <option value="all">All Roles</option>
                  {roles.map((role) => (
                    <option key={role.role_id} value={role.role_id.toString()}>
                      {role.role_name}
                    </option>
                  ))}
                </select>
                <div className="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3 text-gray-400">
                  <svg
                    className="w-4 h-4"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth={2}
                    viewBox="0 0 24 24"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      d="M19 9l-7 7-7-7"
                    />
                  </svg>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Users Table */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-200">
            <div className="flex items-center justify-between">
              <h3 className="text-lg font-semibold text-gray-900">
                Users ({filteredUsers.length})
              </h3>
              <div className="text-sm text-gray-500">
                Showing {filteredUsers.length} of {users.length} users
              </div>
            </div>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    User
                  </th>
                  <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Email
                  </th>
                  <th
                    className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100 transition-colors"
                    onClick={() => handleSort("department")}
                  >
                    <div className="flex items-center space-x-1">
                      <span>Department</span>
                      {sortBy === "department" && (
                        <svg
                          className={`w-4 h-4 ${
                            sortOrder === "asc" ? "transform rotate-180" : ""
                          }`}
                          fill="none"
                          stroke="currentColor"
                          viewBox="0 0 24 24"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M19 9l-7 7-7-7"
                          />
                        </svg>
                      )}
                    </div>
                  </th>
                  <th
                    className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100 transition-colors"
                    onClick={() => handleSort("role")}
                  >
                    <div className="flex items-center space-x-1">
                      <span>Role</span>
                      {sortBy === "role" && (
                        <svg
                          className={`w-4 h-4 ${
                            sortOrder === "asc" ? "transform rotate-180" : ""
                          }`}
                          fill="none"
                          stroke="currentColor"
                          viewBox="0 0 24 24"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M19 9l-7 7-7-7"
                          />
                        </svg>
                      )}
                    </div>
                  </th>
                  <th className="px-6 py-4 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-6 py-4 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredUsers.map((user) => (
                  <tr
                    key={user.user_id}
                    className="hover:bg-gray-50 transition-colors"
                  >
                    {/* User Info */}
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <div
                          className={`flex-shrink-0 h-12 w-12 ${getAvatarColor(
                            user.user_id
                          )} rounded-full flex items-center justify-center text-white font-semibold text-sm shadow-md`}
                        >
                          {getInitials(user.first_name, user.last_name)}
                        </div>
                        <div className="ml-4">
                          <div className="text-sm font-semibold text-gray-900">
                            {user.first_name} {user.last_name}
                          </div>
                          <div className="text-sm text-gray-500">
                            Employee ID: {user.employee_id || "N/A"}
                          </div>
                        </div>
                      </div>
                    </td>

                    {/* Contact */}
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-gray-900">
                        {user.email || "No email"}
                      </div>
                    </td>

                    {/* Department */}
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-gray-900">
                        {user.department_name || "N/A"}
                      </div>
                    </td>

                    {/* Role */}
                    <td className="px-6 py-4 whitespace-nowrap">
                      {user.role_name ? (
                        <span
                          className={`inline-flex items-center px-2 py-0.5 text-xs rounded-full font-medium
                          ${
                            user.role_name === "admin"
                              ? "bg-purple-100 text-purple-800"
                              : user.role_name === "editor"
                              ? "bg-blue-100 text-blue-800"
                              : "bg-gray-100 text-gray-800"
                          }`}
                        >
                          <BadgeCheck className="w-3 h-3 mr-1" />
                          {user.role_name}
                        </span>
                      ) : (
                        <span className="text-gray-400 text-sm">No Role</span>
                      )}
                    </td>

                    {/* Status */}
                    <td className="px-6 py-4 whitespace-nowrap text-center">
                      {canEdit ? (
                        <Switch
                          checked={user.status_id === 1}
                          onChange={() => toggleStatus(user)}
                          className={`${
                            user.status_id === 1
                              ? "bg-green-600"
                              : "bg-gray-300"
                          } relative inline-flex h-6 w-11 items-center rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2`}
                        >
                          <span className="sr-only">Toggle user status</span>
                          <span
                            className={`${
                              user.status_id === 1
                                ? "translate-x-6"
                                : "translate-x-1"
                            } inline-block h-4 w-4 transform rounded-full bg-white transition-transform shadow-sm`}
                          />
                        </Switch>
                      ) : (
                        <span
                          className={`inline-flex items-center px-3 py-1 rounded-full text-xs font-medium ${
                            user.status_id === 1
                              ? "bg-green-100 text-green-800"
                              : "bg-red-100 text-red-800"
                          }`}
                        >
                          {user.status_id === 1 ? "Active" : "Inactive"}
                        </span>
                      )}
                    </td>

                    {/* Actions */}
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      {canEdit ? (
                        <button
                          onClick={() => setEditingUser(user)}
                          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-lg text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
                        >
                          <svg
                            className="w-4 h-4 mr-2"
                            fill="none"
                            stroke="currentColor"
                            viewBox="0 0 24 24"
                          >
                            <path
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              strokeWidth={2}
                              d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"
                            />
                          </svg>
                          Edit Role
                        </button>
                      ) : (
                        <span className="text-gray-400 text-sm">No access</span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            {filteredUsers.length === 0 && (
              <div className="text-center py-12">
                <svg
                  className="mx-auto h-12 w-12 text-gray-400"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4"
                  />
                </svg>
                <h3 className="mt-4 text-lg font-medium text-gray-900">
                  No users found
                </h3>
                <p className="mt-2 text-gray-500">
                  Try adjusting your search or filter criteria.
                </p>
              </div>
            )}
          </div>
        </div>

        {/* Edit User Modal */}
        {editingUser && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-xl shadow-2xl w-full max-w-md relative">
              {saving && (
                <div className="absolute inset-0 bg-white bg-opacity-90 flex items-center justify-center rounded-xl">
                  <div className="text-center">
                    <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600 mx-auto mb-3"></div>
                    <p className="text-gray-600">Updating role...</p>
                  </div>
                </div>
              )}

              <div className="p-6">
                <div className="flex items-center mb-6">
                  <div
                    className={`flex-shrink-0 h-12 w-12 ${getAvatarColor(
                      editingUser.user_id
                    )} rounded-full flex items-center justify-center text-white font-semibold text-sm`}
                  >
                    {getInitials(editingUser.first_name, editingUser.last_name)}
                  </div>
                  <div className="ml-4">
                    <h3 className="text-xl font-semibold text-gray-900">
                      Edit User Role
                    </h3>
                    <p className="text-gray-600">
                      {editingUser.first_name} {editingUser.last_name}
                    </p>
                  </div>
                </div>

                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Current Role
                    </label>
                    <p className="text-sm text-gray-600 bg-gray-50 p-3 rounded-lg">
                      {editingUser.role_name || "No role assigned"}
                    </p>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      New Role *
                    </label>
                    <select
                      value={editingUser.role_id ?? ""}
                      onChange={(e) =>
                        setEditingUser({
                          ...editingUser,
                          role_id: Number(e.target.value),
                        })
                      }
                      className="w-full py-3 px-4 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
                      disabled={saving}
                    >
                      <option value="">Select a role...</option>
                      {roles.map((role) => (
                        <option key={role.role_id} value={role.role_id}>
                          {role.role_name}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>

                <div className="flex justify-end space-x-3 mt-8 pt-6 border-t border-gray-200">
                  <button
                    onClick={() => setEditingUser(null)}
                    disabled={saving}
                    className="px-6 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500 transition-colors disabled:opacity-50"
                  >
                    Cancel
                  </button>
                  <button
                    onClick={handleSave}
                    disabled={saving || !editingUser.role_id}
                    className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {saving ? "Updating..." : "Update Role"}
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default AccessControl;

function getUserRoleFromToken(): string | undefined {
  try {
    const token = localStorage.getItem("token");
    if (!token) return undefined;
    const payloadBase64 = token.split(".")[1];
    const payloadJson = atob(payloadBase64);
    const payload = JSON.parse(payloadJson);
    return payload.role;
  } catch {
    return undefined;
  }
}
