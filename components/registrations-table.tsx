'use client';

import { useState } from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Search,
  Filter,
  MoreHorizontal,
  CheckCircle,
  XCircle,
  Eye,
  Download,
  FileSpreadsheet,
  Mail,
  Phone,
  Trash2,
  ArrowUpDown,
  ArrowUp,
  ArrowDown,
} from 'lucide-react';
import type { Registration, TrainingProgram } from '@/lib/types';
import * as XLSX from 'xlsx';

interface RegistrationsTableProps {
  registrations: Registration[];
  programs?: TrainingProgram[];
  onApprove?: (id: string) => void;
  onReject?: (id: string) => void;
  onDelete?: (id: string) => void;
  showActions?: boolean;
}

export function RegistrationsTable({
  registrations,
  programs = [],
  onApprove,
  onReject,
  onDelete,
  showActions = true,
}: RegistrationsTableProps) {
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [programFilter, setProgramFilter] = useState<string>('all');
  const [sortField, setSortField] = useState<string>('registeredAt');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('desc');

  const handleSort = (field: string) => {
    if (sortField === field) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortDirection('desc');
    }
  };

  const getSortIcon = (field: string) => {
    if (sortField !== field) {
      return <ArrowUpDown className="h-4 w-4 mr-1 opacity-50" />;
    }
    return sortDirection === 'asc' ? (
      <ArrowUp className="h-4 w-4 mr-1" />
    ) : (
      <ArrowDown className="h-4 w-4 mr-1" />
    );
  };

  const getStatusBadge = (status: Registration['status']) => {
    switch (status) {
      case 'pending':
        return (
          <Badge variant="outline" className="border-amber-500 text-amber-600">
            بانتظار الموافقة
          </Badge>
        );
      case 'approved':
        return <Badge className="bg-emerald-500">مقبول</Badge>;
      case 'rejected':
        return <Badge variant="destructive">مرفوض</Badge>;
      case 'completed':
        return <Badge variant="secondary">مكتمل</Badge>;
    }
  };

  const filteredRegistrations = registrations
    .filter((reg) => {
      const matchesSearch =
        reg.visitorName.toLowerCase().includes(searchQuery.toLowerCase()) ||
        reg.programTitle.toLowerCase().includes(searchQuery.toLowerCase()) ||
        reg.employeeId.toLowerCase().includes(searchQuery.toLowerCase()) ||
        reg.email.toLowerCase().includes(searchQuery.toLowerCase());
      const matchesStatus =
        statusFilter === 'all' || reg.status === statusFilter;
      const matchesProgram =
        programFilter === 'all' || reg.programId === programFilter;
      return matchesSearch && matchesStatus && matchesProgram;
    })
    .sort((a, b) => {
      let valueA: string | number;
      let valueB: string | number;
      
      switch (sortField) {
        case 'visitorName':
          valueA = a.visitorName.toLowerCase();
          valueB = b.visitorName.toLowerCase();
          break;
        case 'employeeId':
          valueA = a.employeeId;
          valueB = b.employeeId;
          break;
        case 'court':
          valueA = (a.court || '').toLowerCase();
          valueB = (b.court || '').toLowerCase();
          break;
        case 'programTitle':
          valueA = a.programTitle.toLowerCase();
          valueB = b.programTitle.toLowerCase();
          break;
        case 'batchName':
          valueA = a.batchName.toLowerCase();
          valueB = b.batchName.toLowerCase();
          break;
        case 'registeredAt':
          valueA = new Date(a.registeredAt).getTime();
          valueB = new Date(b.registeredAt).getTime();
          break;
        case 'status':
          const statusOrder = { pending: 0, approved: 1, rejected: 2, completed: 3 };
          valueA = statusOrder[a.status];
          valueB = statusOrder[b.status];
          break;
        default:
          valueA = new Date(a.registeredAt).getTime();
          valueB = new Date(b.registeredAt).getTime();
      }
      
      if (valueA < valueB) return sortDirection === 'asc' ? -1 : 1;
      if (valueA > valueB) return sortDirection === 'asc' ? 1 : -1;
      return 0;
    });

  // Get approved registrations for export (filtered by selected program)
  const approvedRegistrations = filteredRegistrations.filter(
    (reg) => reg.status === 'approved'
  );

  // Get approved registrations grouped by program for export
  const approvedByProgram = registrations
    .filter((reg) => reg.status === 'approved')
    .reduce((acc, reg) => {
      if (!acc[reg.programId]) {
        acc[reg.programId] = {
          programTitle: reg.programTitle,
          registrations: [],
        };
      }
      acc[reg.programId].registrations.push(reg);
      return acc;
    }, {} as Record<string, { programTitle: string; registrations: Registration[] }>);

  // Export to Excel for selected program
  const handleExportExcel = (programId?: string) => {
    let dataToExport: Registration[];
    let fileName: string;

    if (programId && programId !== 'all') {
      // Export for specific program
      const programData = approvedByProgram[programId];
      if (!programData || programData.registrations.length === 0) return;
      
      dataToExport = programData.registrations;
      fileName = `المقبولين_${programData.programTitle}`;
    } else if (programFilter !== 'all') {
      // Export based on current filter
      dataToExport = approvedRegistrations;
      const programTitle = uniquePrograms.find(p => p.id === programFilter)?.title || '';
      fileName = `المقبولين_${programTitle}`;
    } else {
      // Should not happen, but fallback
      return;
    }

    const excelData = dataToExport.map((reg) => ({
      'الاسم': reg.visitorName,
      'الرقم الوظيفي': reg.employeeId,
      'المسمى الوظيفي': reg.jobTitle || '-',
      'الدائرة/المحكمة': reg.court || '-',
      'القسم': reg.department,
      'البريد الإلكتروني': reg.email,
      'رقم الهاتف': reg.phone || '-',
      'البرنامج': reg.programTitle,
      'الدفعة': reg.batchName,
      'تاريخ التسجيل': reg.registeredAt,
      'تاريخ الموافقة': reg.approvedAt || '-',
    }));

    const ws = XLSX.utils.json_to_sheet(excelData);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'المرشحين المقبولين');
    
    // Generate filename with date
    const date = new Date().toLocaleDateString('ar-SA').replace(/\//g, '-');
    XLSX.writeFile(wb, `${fileName}_${date}.xlsx`);
  };

  // Get unique programs for filter
  const uniquePrograms = Array.from(
    new Set(registrations.map((r) => r.programId))
  ).map((id) => ({
    id,
    title: registrations.find((r) => r.programId === id)?.programTitle || '',
  }));

  return (
    <Card className="border-none shadow-sm">
      <CardHeader className="pb-4">
        <div className="flex flex-col gap-4">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <CardTitle>طلبات الترشيح</CardTitle>
            {showActions && Object.keys(approvedByProgram).length > 0 && (
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="outline" className="gap-2">
                    <FileSpreadsheet className="h-4 w-4" />
                    تصدير المقبولين حسب البرنامج
                    <Download className="h-4 w-4" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-64">
                  {Object.entries(approvedByProgram).map(([progId, data]) => (
                    <DropdownMenuItem
                      key={progId}
                      onClick={() => handleExportExcel(progId)}
                      className="flex justify-between"
                    >
                      <span className="truncate">{data.programTitle}</span>
                      <Badge variant="secondary" className="mr-2">
                        {data.registrations.length}
                      </Badge>
                    </DropdownMenuItem>
                  ))}
                </DropdownMenuContent>
              </DropdownMenu>
            )}
          </div>
          
          <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
            <div className="relative flex-1">
              <Search className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder="بحث بالاسم أو الرقم الوظيفي أو البريد..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pr-9"
              />
            </div>
            <Select value={programFilter} onValueChange={setProgramFilter}>
              <SelectTrigger className="w-full sm:w-48">
                <SelectValue placeholder="البرنامج" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">جميع البرامج</SelectItem>
                {uniquePrograms.map((prog) => (
                  <SelectItem key={prog.id} value={prog.id}>
                    {prog.title}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select value={statusFilter} onValueChange={setStatusFilter}>
              <SelectTrigger className="w-full sm:w-40">
                <Filter className="ml-2 h-4 w-4" />
                <SelectValue placeholder="الحالة" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">جميع الحالات</SelectItem>
                <SelectItem value="pending">بانتظار الموافقة</SelectItem>
                <SelectItem value="approved">مقبول</SelectItem>
                <SelectItem value="rejected">مرفوض</SelectItem>
                <SelectItem value="completed">مكتمل</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="text-right">
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-8 px-2 hover:bg-muted"
                    onClick={() => handleSort('visitorName')}
                  >
                    الموظف
                    {getSortIcon('visitorName')}
                  </Button>
                </TableHead>
                <TableHead className="text-right">
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-8 px-2 hover:bg-muted"
                    onClick={() => handleSort('employeeId')}
                  >
                    الرقم الوظيفي
                    {getSortIcon('employeeId')}
                  </Button>
                </TableHead>
                <TableHead className="text-right">
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-8 px-2 hover:bg-muted"
                    onClick={() => handleSort('court')}
                  >
                    الدائرة/المحكمة
                    {getSortIcon('court')}
                  </Button>
                </TableHead>
                <TableHead className="text-right">التواصل</TableHead>
                <TableHead className="text-right">
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-8 px-2 hover:bg-muted"
                    onClick={() => handleSort('programTitle')}
                  >
                    البرنامج
                    {getSortIcon('programTitle')}
                  </Button>
                </TableHead>
                <TableHead className="text-right">
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-8 px-2 hover:bg-muted"
                    onClick={() => handleSort('batchName')}
                  >
                    الدفعة
                    {getSortIcon('batchName')}
                  </Button>
                </TableHead>
                <TableHead className="text-right">
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-8 px-2 hover:bg-muted"
                    onClick={() => handleSort('registeredAt')}
                  >
                    تاريخ التسجيل
                    {getSortIcon('registeredAt')}
                  </Button>
                </TableHead>
                <TableHead className="text-right">
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-8 px-2 hover:bg-muted"
                    onClick={() => handleSort('status')}
                  >
                    الحالة
                    {getSortIcon('status')}
                  </Button>
                </TableHead>
                {showActions && (
                  <TableHead className="text-right">الإجراءات</TableHead>
                )}
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredRegistrations.length === 0 ? (
                <TableRow>
                  <TableCell
                    colSpan={showActions ? 9 : 8}
                    className="h-32 text-center text-muted-foreground"
                  >
                    لا توجد تسجيلات
                  </TableCell>
                </TableRow>
              ) : (
                filteredRegistrations.map((reg) => (
                  <TableRow key={reg.id}>
                    <TableCell>
                      <div>
                        <p className="font-medium">{reg.visitorName}</p>
                        <p className="text-sm text-muted-foreground">
                          {reg.department}
                        </p>
                      </div>
                    </TableCell>
                    <TableCell>
                      <span className="font-mono text-sm">{reg.employeeId}</span>
                      {reg.jobTitle && (
                        <p className="text-xs text-muted-foreground mt-1">{reg.jobTitle}</p>
                      )}
                    </TableCell>
                    <TableCell>
                      <span className="text-sm">{reg.court || '-'}</span>
                    </TableCell>
                    <TableCell>
                      <div className="flex flex-col gap-1">
                        <a
                          href={`mailto:${reg.email}`}
                          className="flex items-center gap-1 text-sm text-primary hover:underline"
                        >
                          <Mail className="h-3 w-3" />
                          {reg.email}
                        </a>
                        {reg.phone && (
                          <a
                            href={`tel:${reg.phone}`}
                            className="flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
                          >
                            <Phone className="h-3 w-3" />
                            {reg.phone}
                          </a>
                        )}
                      </div>
                    </TableCell>
                    <TableCell className="font-medium">
                      {reg.programTitle}
                    </TableCell>
                    <TableCell>{reg.batchName}</TableCell>
                    <TableCell>{reg.registeredAt}</TableCell>
                    <TableCell>{getStatusBadge(reg.status)}</TableCell>
                    {showActions && (
                      <TableCell>
                        {reg.status === 'pending' ? (
                          <div className="flex items-center gap-1">
                            <Button
                              size="icon"
                              variant="ghost"
                              className="h-8 w-8 text-emerald-600 hover:bg-emerald-50 hover:text-emerald-700"
                              onClick={() => onApprove?.(reg.id)}
                              title="قبول"
                            >
                              <CheckCircle className="h-4 w-4" />
                            </Button>
                            <Button
                              size="icon"
                              variant="ghost"
                              className="h-8 w-8 text-red-600 hover:bg-red-50 hover:text-red-700"
                              onClick={() => onReject?.(reg.id)}
                              title="رفض"
                            >
                              <XCircle className="h-4 w-4" />
                            </Button>
                            <DropdownMenu>
                              <DropdownMenuTrigger asChild>
                                <Button variant="ghost" size="icon" className="h-8 w-8">
                                  <MoreHorizontal className="h-4 w-4" />
                                </Button>
                              </DropdownMenuTrigger>
                              <DropdownMenuContent align="end">
                                <DropdownMenuItem
                                  className="text-red-600"
                                  onClick={() => onDelete?.(reg.id)}
                                >
                                  <Trash2 className="ml-2 h-4 w-4" />
                                  حذف
                                </DropdownMenuItem>
                              </DropdownMenuContent>
                            </DropdownMenu>
                          </div>
                        ) : (
                          <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                              <Button variant="ghost" size="icon" className="h-8 w-8">
                                <MoreHorizontal className="h-4 w-4" />
                              </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="end">
                              <DropdownMenuItem>
                                <Eye className="ml-2 h-4 w-4" />
                                عرض التفاصيل
                              </DropdownMenuItem>
                              <DropdownMenuItem>
                                <Mail className="ml-2 h-4 w-4" />
                                إرسال بريد
                              </DropdownMenuItem>
                              <DropdownMenuItem
                                className="text-red-600"
                                onClick={() => onDelete?.(reg.id)}
                              >
                                <Trash2 className="ml-2 h-4 w-4" />
                                حذف
                              </DropdownMenuItem>
                            </DropdownMenuContent>
                          </DropdownMenu>
                        )}
                      </TableCell>
                    )}
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>

        {/* Summary */}
        <div className="mt-4 flex flex-wrap gap-4 text-sm text-muted-foreground">
          <span>
            الإجمالي: <strong className="text-foreground">{filteredRegistrations.length}</strong>
          </span>
          <span>
            ��انتظار الموافقة:{' '}
            <strong className="text-amber-600">
              {filteredRegistrations.filter((r) => r.status === 'pending').length}
            </strong>
          </span>
          <span>
            مقبول:{' '}
            <strong className="text-emerald-600">
              {filteredRegistrations.filter((r) => r.status === 'approved').length}
            </strong>
          </span>
          <span>
            مرفوض:{' '}
            <strong className="text-red-600">
              {filteredRegistrations.filter((r) => r.status === 'rejected').length}
            </strong>
          </span>
        </div>
      </CardContent>
    </Card>
  );
}
