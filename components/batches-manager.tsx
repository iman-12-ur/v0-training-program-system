'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
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
  Calendar,
  Users,
  Plus,
  MoreHorizontal,
  Edit,
  Trash2,
  Play,
  CheckCircle,
} from 'lucide-react';
import type { TrainingProgram, Batch } from '@/lib/types';

interface BatchesManagerProps {
  programs: TrainingProgram[];
  onAddBatch: (programId: string, batch: Omit<Batch, 'id'>) => void;
  onUpdateBatch: (programId: string, batchId: string, batch: Partial<Batch>) => void;
  onDeleteBatch: (programId: string, batchId: string) => void;
  onUpdateBatchStatus: (
    programId: string,
    batchId: string,
    status: Batch['status']
  ) => void;
}

export function BatchesManager({
  programs,
  onAddBatch,
  onUpdateBatch,
  onDeleteBatch,
  onUpdateBatchStatus,
}: BatchesManagerProps) {
  const [selectedProgram, setSelectedProgram] = useState<string>('');
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [editingBatch, setEditingBatch] = useState<Batch | null>(null);
  const [deletingBatch, setDeletingBatch] = useState<Batch | null>(null);
  const [newBatch, setNewBatch] = useState({
    name: '',
    startDate: '',
    endDate: '',
    maxParticipants: 20,
  });

  const activePrograms = programs.filter((p) => p.status === 'active');
  const selectedProgramData = programs.find((p) => p.id === selectedProgram);

  const handleAddBatch = () => {
    if (
      !selectedProgram ||
      !newBatch.name ||
      !newBatch.startDate ||
      !newBatch.endDate
    )
      return;

    onAddBatch(selectedProgram, {
      name: newBatch.name,
      startDate: newBatch.startDate,
      endDate: newBatch.endDate,
      maxParticipants: newBatch.maxParticipants,
      currentParticipants: 0,
      status: 'upcoming',
    });

    setIsAddModalOpen(false);
    setNewBatch({
      name: '',
      startDate: '',
      endDate: '',
      maxParticipants: 20,
    });
  };

  const handleEditBatch = (batch: Batch) => {
    setEditingBatch(batch);
    setIsEditModalOpen(true);
  };

  const handleSaveEdit = () => {
    if (!editingBatch || !selectedProgram) return;
    
    onUpdateBatch(selectedProgram, editingBatch.id, {
      name: editingBatch.name,
      startDate: editingBatch.startDate,
      endDate: editingBatch.endDate,
      maxParticipants: editingBatch.maxParticipants,
    });
    
    setIsEditModalOpen(false);
    setEditingBatch(null);
  };

  const handleDeleteBatch = (batch: Batch) => {
    setDeletingBatch(batch);
    setIsDeleteDialogOpen(true);
  };

  const handleConfirmDelete = () => {
    if (!deletingBatch || !selectedProgram) return;
    
    onDeleteBatch(selectedProgram, deletingBatch.id);
    setIsDeleteDialogOpen(false);
    setDeletingBatch(null);
  };

  const getBatchStatusBadge = (status: Batch['status']) => {
    switch (status) {
      case 'upcoming':
        return <Badge className="bg-blue-500">قادم</Badge>;
      case 'ongoing':
        return <Badge className="bg-emerald-500">جاري</Badge>;
      case 'completed':
        return <Badge variant="secondary">مكتمل</Badge>;
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-2xl font-bold text-foreground">إدارة الدفعات</h2>
          <p className="text-muted-foreground">
            إضافة وإدارة دفعات البرامج التدريبية
          </p>
        </div>
        <Button onClick={() => setIsAddModalOpen(true)}>
          <Plus className="ml-2 h-4 w-4" />
          إضافة دفعة جديدة
        </Button>
      </div>

      {/* Program Selector */}
      <Card className="border-none shadow-sm">
        <CardHeader>
          <CardTitle className="text-lg">اختر البرنامج</CardTitle>
        </CardHeader>
        <CardContent>
          <Select value={selectedProgram} onValueChange={setSelectedProgram}>
            <SelectTrigger className="w-full sm:w-96">
              <SelectValue placeholder="اختر برنامجاً لعرض الدفعات" />
            </SelectTrigger>
            <SelectContent>
              {activePrograms.map((program) => (
                <SelectItem key={program.id} value={program.id}>
                  {program.title} ({program.batches.length} دفعات)
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </CardContent>
      </Card>

      {/* Batches Table */}
      {selectedProgramData && (
        <Card className="border-none shadow-sm">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Calendar className="h-5 w-5 text-primary" />
              دفعات برنامج: {selectedProgramData.title}
            </CardTitle>
          </CardHeader>
          <CardContent>
            {selectedProgramData.batches.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-12 text-center">
                <div className="rounded-full bg-muted p-4">
                  <Calendar className="h-8 w-8 text-muted-foreground" />
                </div>
                <h3 className="mt-4 font-medium text-foreground">
                  لا توجد دفعات
                </h3>
                <p className="mt-1 text-sm text-muted-foreground">
                  لم يتم إضافة أي دفعات لهذا البرنامج بعد
                </p>
                <Button
                  className="mt-4"
                  onClick={() => setIsAddModalOpen(true)}
                >
                  <Plus className="ml-2 h-4 w-4" />
                  إضافة أول دفعة
                </Button>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead className="text-right">اسم الدفعة</TableHead>
                      <TableHead className="text-right">تاريخ البدء</TableHead>
                      <TableHead className="text-right">تاريخ الانتهاء</TableHead>
                      <TableHead className="text-right">المشاركون</TableHead>
                      <TableHead className="text-right">الحالة</TableHead>
                      <TableHead className="text-right">الإجراءات</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {selectedProgramData.batches.map((batch) => (
                      <TableRow key={batch.id}>
                        <TableCell className="font-medium">
                          {batch.name}
                        </TableCell>
                        <TableCell>{batch.startDate}</TableCell>
                        <TableCell>{batch.endDate}</TableCell>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <Users className="h-4 w-4 text-muted-foreground" />
                            <span>
                              {batch.currentParticipants}/{batch.maxParticipants}
                            </span>
                          </div>
                        </TableCell>
                        <TableCell>{getBatchStatusBadge(batch.status)}</TableCell>
                        <TableCell>
                          <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                              <Button variant="ghost" size="icon" className="h-8 w-8">
                                <MoreHorizontal className="h-4 w-4" />
                              </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="end">
                              {batch.status === 'upcoming' && (
                                <DropdownMenuItem
                                  onClick={() =>
                                    onUpdateBatchStatus(
                                      selectedProgram,
                                      batch.id,
                                      'ongoing'
                                    )
                                  }
                                >
                                  <Play className="ml-2 h-4 w-4" />
                                  بدء الدفعة
                                </DropdownMenuItem>
                              )}
                              {batch.status === 'ongoing' && (
                                <DropdownMenuItem
                                  onClick={() =>
                                    onUpdateBatchStatus(
                                      selectedProgram,
                                      batch.id,
                                      'completed'
                                    )
                                  }
                                >
                                  <CheckCircle className="ml-2 h-4 w-4" />
                                  إنهاء الدفعة
                                </DropdownMenuItem>
                              )}
                              <DropdownMenuItem onClick={() => handleEditBatch(batch)}>
                                <Edit className="ml-2 h-4 w-4" />
                                تعديل
                              </DropdownMenuItem>
                              <DropdownMenuItem 
                                className="text-red-600"
                                onClick={() => handleDeleteBatch(batch)}
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
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Add Batch Modal */}
      <Dialog open={isAddModalOpen} onOpenChange={setIsAddModalOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>إضافة دفعة جديدة</DialogTitle>
          </DialogHeader>

          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="batch-program">البرنامج</Label>
              <Select value={selectedProgram} onValueChange={setSelectedProgram}>
                <SelectTrigger id="batch-program">
                  <SelectValue placeholder="اختر البرنامج" />
                </SelectTrigger>
                <SelectContent>
                  {activePrograms.map((program) => (
                    <SelectItem key={program.id} value={program.id}>
                      {program.title}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="batch-name">اسم الدفعة</Label>
              <Input
                id="batch-name"
                placeholder="مثال: الدفعة الأولى"
                value={newBatch.name}
                onChange={(e) =>
                  setNewBatch((prev) => ({ ...prev, name: e.target.value }))
                }
              />
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="start-date">تاريخ البدء</Label>
                <Input
                  id="start-date"
                  type="date"
                  value={newBatch.startDate}
                  onChange={(e) =>
                    setNewBatch((prev) => ({ ...prev, startDate: e.target.value }))
                  }
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="end-date">تاريخ الانتهاء</Label>
                <Input
                  id="end-date"
                  type="date"
                  value={newBatch.endDate}
                  onChange={(e) =>
                    setNewBatch((prev) => ({ ...prev, endDate: e.target.value }))
                  }
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="max-participants">الحد الأقصى للمشاركين</Label>
              <Input
                id="max-participants"
                type="number"
                min="1"
                value={newBatch.maxParticipants}
                onChange={(e) =>
                  setNewBatch((prev) => ({
                    ...prev,
                    maxParticipants: parseInt(e.target.value) || 20,
                  }))
                }
              />
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setIsAddModalOpen(false)}>
              إلغاء
            </Button>
            <Button
              onClick={handleAddBatch}
              disabled={
                !selectedProgram ||
                !newBatch.name ||
                !newBatch.startDate ||
                !newBatch.endDate
              }
            >
              إضافة الدفعة
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Edit Batch Modal */}
      <Dialog open={isEditModalOpen} onOpenChange={setIsEditModalOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>تعديل الدفعة</DialogTitle>
          </DialogHeader>

          {editingBatch && (
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="edit-batch-name">اسم الدفعة</Label>
                <Input
                  id="edit-batch-name"
                  value={editingBatch.name}
                  onChange={(e) =>
                    setEditingBatch((prev) =>
                      prev ? { ...prev, name: e.target.value } : null
                    )
                  }
                />
              </div>

              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="edit-start-date">تاريخ البدء</Label>
                  <Input
                    id="edit-start-date"
                    type="date"
                    value={editingBatch.startDate}
                    onChange={(e) =>
                      setEditingBatch((prev) =>
                        prev ? { ...prev, startDate: e.target.value } : null
                      )
                    }
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="edit-end-date">تاريخ الانتهاء</Label>
                  <Input
                    id="edit-end-date"
                    type="date"
                    value={editingBatch.endDate}
                    onChange={(e) =>
                      setEditingBatch((prev) =>
                        prev ? { ...prev, endDate: e.target.value } : null
                      )
                    }
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="edit-max-participants">الحد الأقصى للمشاركين</Label>
                <Input
                  id="edit-max-participants"
                  type="number"
                  min="1"
                  value={editingBatch.maxParticipants}
                  onChange={(e) =>
                    setEditingBatch((prev) =>
                      prev
                        ? { ...prev, maxParticipants: parseInt(e.target.value) || 20 }
                        : null
                    )
                  }
                />
              </div>
            </div>
          )}

          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => {
                setIsEditModalOpen(false);
                setEditingBatch(null);
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
              هل أنت متأكد من حذف الدفعة{' '}
              <span className="font-semibold text-foreground">
                {deletingBatch?.name}
              </span>
              ؟
            </p>
            <p className="mt-2 text-sm text-red-600">
              سيتم حذف جميع التسجيلات المرتبطة بهذه الدفعة. هذا الإجراء لا يمكن
              التراجع عنه.
            </p>
          </div>

          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => {
                setIsDeleteDialogOpen(false);
                setDeletingBatch(null);
              }}
            >
              إلغاء
            </Button>
            <Button variant="destructive" onClick={handleConfirmDelete}>
              حذف الدفعة
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
