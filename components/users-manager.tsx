'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { Switch } from '@/components/ui/switch';
import { Checkbox } from '@/components/ui/checkbox';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Plus,
  MoreVertical,
  Edit,
  Trash2,
  Shield,
  UserCheck,
  UserX,
} from 'lucide-react';
import type { AdminUser } from '@/lib/types';

interface UsersManagerProps {
  users: AdminUser[];
  onAddUser: (user: Omit<AdminUser, 'id' | 'createdAt'>) => void;
  onUpdateUser: (userId: string, updates: Partial<AdminUser>) => void;
  onDeleteUser: (userId: string) => void;
}

export function UsersManager({
  users,
  onAddUser,
  onUpdateUser,
  onDeleteUser,
}: UsersManagerProps) {
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<AdminUser | null>(null);
  const [deletingUser, setDeletingUser] = useState<AdminUser | null>(null);

  const [newUser, setNewUser] = useState({
    username: '',
    name: '',
    email: '',
    role: 'supervisor' as AdminUser['role'],
    permissions: {
      managePrograms: false,
      manageBatches: false,
      approveRegistrations: true,
      viewReports: true,
      manageUsers: false,
    },
    isActive: true,
  });

  const handleAddUser = () => {
    if (!newUser.username || !newUser.name || !newUser.email) return;

    onAddUser(newUser);
    setNewUser({
      username: '',
      name: '',
      email: '',
      role: 'supervisor',
      permissions: {
        managePrograms: false,
        manageBatches: false,
        approveRegistrations: true,
        viewReports: true,
        manageUsers: false,
      },
      isActive: true,
    });
    setIsAddModalOpen(false);
  };

  const handleEditUser = (user: AdminUser) => {
    setEditingUser(user);
    setIsEditModalOpen(true);
  };

  const handleSaveEdit = () => {
    if (!editingUser) return;

    onUpdateUser(editingUser.id, {
      name: editingUser.name,
      email: editingUser.email,
      role: editingUser.role,
      permissions: editingUser.permissions,
      isActive: editingUser.isActive,
    });

    setIsEditModalOpen(false);
    setEditingUser(null);
  };

  const handleDeleteUser = (user: AdminUser) => {
    setDeletingUser(user);
    setIsDeleteDialogOpen(true);
  };

  const handleConfirmDelete = () => {
    if (!deletingUser) return;

    onDeleteUser(deletingUser.id);
    setIsDeleteDialogOpen(false);
    setDeletingUser(null);
  };

  const handleToggleActive = (userId: string, isActive: boolean) => {
    onUpdateUser(userId, { isActive });
  };

  const permissionLabels = {
    managePrograms: 'إدارة البرامج',
    manageBatches: 'إدارة الدفعات',
    approveRegistrations: 'الموافقة على الترشيحات',
    viewReports: 'عرض التقارير',
    manageUsers: 'إدارة المستخدمين',
  };

  return (
    <div className="space-y-6">
      <Card className="border-none shadow-sm">
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="flex items-center gap-2">
            <Shield className="h-5 w-5" />
            إدارة المستخدمين والصلاحيات
          </CardTitle>
          <Button onClick={() => setIsAddModalOpen(true)} className="gap-2">
            <Plus className="h-4 w-4" />
            إضافة مستخدم
          </Button>
        </CardHeader>
        <CardContent>
          {users.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <Shield className="h-12 w-12 text-muted-foreground" />
              <h3 className="mt-4 text-lg font-medium">لا يوجد مستخدمين</h3>
              <p className="mt-2 text-muted-foreground">
                أضف مستخدمين جدد لمنحهم صلاحيات الوصول للنظام
              </p>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>المستخدم</TableHead>
                  <TableHead>اسم الدخول</TableHead>
                  <TableHead>الدور</TableHead>
                  <TableHead>الصلاحيات</TableHead>
                  <TableHead>الحالة</TableHead>
                  <TableHead className="w-[80px]">الإجراءات</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {users.map((user) => (
                  <TableRow key={user.id}>
                    <TableCell>
                      <div>
                        <p className="font-medium">{user.name}</p>
                        <p className="text-sm text-muted-foreground">
                          {user.email}
                        </p>
                      </div>
                    </TableCell>
                    <TableCell className="font-mono text-sm">
                      {user.username}
                    </TableCell>
                    <TableCell>
                      <Badge
                        variant={user.role === 'admin' ? 'default' : 'secondary'}
                      >
                        {user.role === 'admin' ? 'مدير' : 'مشرف'}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <div className="flex flex-wrap gap-1">
                        {Object.entries(user.permissions)
                          .filter(([, value]) => value)
                          .map(([key]) => (
                            <Badge
                              key={key}
                              variant="outline"
                              className="text-xs"
                            >
                              {permissionLabels[key as keyof typeof permissionLabels]}
                            </Badge>
                          ))}
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <Switch
                          checked={user.isActive}
                          onCheckedChange={(checked) =>
                            handleToggleActive(user.id, checked)
                          }
                          disabled={user.id === 'admin'}
                        />
                        {user.isActive ? (
                          <UserCheck className="h-4 w-4 text-emerald-600" />
                        ) : (
                          <UserX className="h-4 w-4 text-muted-foreground" />
                        )}
                      </div>
                    </TableCell>
                    <TableCell>
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button
                            variant="ghost"
                            size="icon"
                            disabled={user.id === 'admin'}
                          >
                            <MoreVertical className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuItem onClick={() => handleEditUser(user)}>
                            <Edit className="ml-2 h-4 w-4" />
                            تعديل
                          </DropdownMenuItem>
                          <DropdownMenuItem
                            className="text-red-600"
                            onClick={() => handleDeleteUser(user)}
                          >
                            <Trash2 className="ml-2 h-4 w-4" />
                            حذف
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Add User Modal */}
      <Dialog open={isAddModalOpen} onOpenChange={setIsAddModalOpen}>
        <DialogContent className="sm:max-w-lg">
          <DialogHeader>
            <DialogTitle>إضافة مستخدم جديد</DialogTitle>
          </DialogHeader>

          <div className="space-y-4 py-4">
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="new-username">اسم الدخول</Label>
                <Input
                  id="new-username"
                  value={newUser.username}
                  onChange={(e) =>
                    setNewUser((prev) => ({ ...prev, username: e.target.value }))
                  }
                  placeholder="مثال: ahmed.ali"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="new-name">الاسم الكامل</Label>
                <Input
                  id="new-name"
                  value={newUser.name}
                  onChange={(e) =>
                    setNewUser((prev) => ({ ...prev, name: e.target.value }))
                  }
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="new-email">البريد الإلكتروني</Label>
              <Input
                id="new-email"
                type="email"
                value={newUser.email}
                onChange={(e) =>
                  setNewUser((prev) => ({ ...prev, email: e.target.value }))
                }
              />
            </div>

            <div className="space-y-2">
              <Label>الدور</Label>
              <Select
                value={newUser.role}
                onValueChange={(value: AdminUser['role']) =>
                  setNewUser((prev) => ({
                    ...prev,
                    role: value,
                    permissions:
                      value === 'admin'
                        ? {
                            managePrograms: true,
                            manageBatches: true,
                            approveRegistrations: true,
                            viewReports: true,
                            manageUsers: true,
                          }
                        : prev.permissions,
                  }))
                }
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="supervisor">مشرف</SelectItem>
                  <SelectItem value="admin">مدير</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-3">
              <Label>الصلاحيات</Label>
              <div className="space-y-2 rounded-lg border p-4">
                {Object.entries(permissionLabels).map(([key, label]) => (
                  <div key={key} className="flex items-center gap-3">
                    <Checkbox
                      id={`new-perm-${key}`}
                      checked={
                        newUser.permissions[key as keyof typeof newUser.permissions]
                      }
                      onCheckedChange={(checked) =>
                        setNewUser((prev) => ({
                          ...prev,
                          permissions: {
                            ...prev.permissions,
                            [key]: checked,
                          },
                        }))
                      }
                      disabled={newUser.role === 'admin'}
                    />
                    <Label
                      htmlFor={`new-perm-${key}`}
                      className="cursor-pointer"
                    >
                      {label}
                    </Label>
                  </div>
                ))}
              </div>
              {newUser.role === 'admin' && (
                <p className="text-sm text-muted-foreground">
                  المدير لديه جميع الصلاحيات تلقائياً
                </p>
              )}
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setIsAddModalOpen(false)}>
              إلغاء
            </Button>
            <Button
              onClick={handleAddUser}
              disabled={!newUser.username || !newUser.name || !newUser.email}
            >
              إضافة المستخدم
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Edit User Modal */}
      <Dialog open={isEditModalOpen} onOpenChange={setIsEditModalOpen}>
        <DialogContent className="sm:max-w-lg">
          <DialogHeader>
            <DialogTitle>تعديل المستخدم</DialogTitle>
          </DialogHeader>

          {editingUser && (
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="edit-name">الاسم الكامل</Label>
                <Input
                  id="edit-name"
                  value={editingUser.name}
                  onChange={(e) =>
                    setEditingUser((prev) =>
                      prev ? { ...prev, name: e.target.value } : null
                    )
                  }
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="edit-email">البريد الإلكتروني</Label>
                <Input
                  id="edit-email"
                  type="email"
                  value={editingUser.email}
                  onChange={(e) =>
                    setEditingUser((prev) =>
                      prev ? { ...prev, email: e.target.value } : null
                    )
                  }
                />
              </div>

              <div className="space-y-2">
                <Label>الدور</Label>
                <Select
                  value={editingUser.role}
                  onValueChange={(value: AdminUser['role']) =>
                    setEditingUser((prev) =>
                      prev
                        ? {
                            ...prev,
                            role: value,
                            permissions:
                              value === 'admin'
                                ? {
                                    managePrograms: true,
                                    manageBatches: true,
                                    approveRegistrations: true,
                                    viewReports: true,
                                    manageUsers: true,
                                  }
                                : prev.permissions,
                          }
                        : null
                    )
                  }
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="supervisor">مشرف</SelectItem>
                    <SelectItem value="admin">مدير</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-3">
                <Label>الصلاحيات</Label>
                <div className="space-y-2 rounded-lg border p-4">
                  {Object.entries(permissionLabels).map(([key, label]) => (
                    <div key={key} className="flex items-center gap-3">
                      <Checkbox
                        id={`edit-perm-${key}`}
                        checked={
                          editingUser.permissions[
                            key as keyof typeof editingUser.permissions
                          ]
                        }
                        onCheckedChange={(checked) =>
                          setEditingUser((prev) =>
                            prev
                              ? {
                                  ...prev,
                                  permissions: {
                                    ...prev.permissions,
                                    [key]: checked,
                                  },
                                }
                              : null
                          )
                        }
                        disabled={editingUser.role === 'admin'}
                      />
                      <Label
                        htmlFor={`edit-perm-${key}`}
                        className="cursor-pointer"
                      >
                        {label}
                      </Label>
                    </div>
                  ))}
                </div>
              </div>

              <div className="flex items-center gap-3">
                <Switch
                  checked={editingUser.isActive}
                  onCheckedChange={(checked) =>
                    setEditingUser((prev) =>
                      prev ? { ...prev, isActive: checked } : null
                    )
                  }
                />
                <Label>الحساب نشط</Label>
              </div>
            </div>
          )}

          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => {
                setIsEditModalOpen(false);
                setEditingUser(null);
              }}
            >
              إلغاء
            </Button>
            <Button onClick={handleSaveEdit}>حفظ التغييرات</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>تأكيد الحذف</DialogTitle>
          </DialogHeader>

          <div className="py-4">
            <p className="text-muted-foreground">
              هل أنت متأكد من حذف المستخدم{' '}
              <span className="font-semibold text-foreground">
                {deletingUser?.name}
              </span>
              ؟
            </p>
            <p className="mt-2 text-sm text-red-600">
              سيفقد هذا المستخدم إمكانية الوصول للنظام بشكل نهائي.
            </p>
          </div>

          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => {
                setIsDeleteDialogOpen(false);
                setDeletingUser(null);
              }}
            >
              إلغاء
            </Button>
            <Button variant="destructive" onClick={handleConfirmDelete}>
              حذف المستخدم
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
