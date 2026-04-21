'use client';

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  Calendar,
  Clock,
  MapPin,
  User,
  CheckCircle2,
  XCircle,
  Clock3,
  GraduationCap,
} from 'lucide-react';
import type { Registration, TrainingProgram } from '@/lib/types';

interface MyRegistrationsProps {
  registrations: Registration[];
  programs: TrainingProgram[];
}

export function MyRegistrations({ registrations, programs }: MyRegistrationsProps) {
  const getStatusInfo = (status: Registration['status']) => {
    switch (status) {
      case 'pending':
        return {
          badge: (
            <Badge variant="outline" className="border-amber-500 text-amber-600">
              بانتظار الموافقة
            </Badge>
          ),
          icon: <Clock3 className="h-5 w-5 text-amber-500" />,
          color: 'border-amber-200 bg-amber-50/50',
        };
      case 'approved':
        return {
          badge: <Badge className="bg-emerald-500">موافق عليه</Badge>,
          icon: <CheckCircle2 className="h-5 w-5 text-emerald-500" />,
          color: 'border-emerald-200 bg-emerald-50/50',
        };
      case 'rejected':
        return {
          badge: <Badge variant="destructive">مرفوض</Badge>,
          icon: <XCircle className="h-5 w-5 text-red-500" />,
          color: 'border-red-200 bg-red-50/50',
        };
      case 'completed':
        return {
          badge: <Badge variant="secondary">مكتمل</Badge>,
          icon: <GraduationCap className="h-5 w-5 text-violet-500" />,
          color: 'border-violet-200 bg-violet-50/50',
        };
    }
  };

  const getProgramDetails = (programId: string) => {
    return programs.find((p) => p.id === programId);
  };

  const groupedRegistrations = {
    active: registrations.filter(
      (r) => r.status === 'pending' || r.status === 'approved'
    ),
    completed: registrations.filter((r) => r.status === 'completed'),
    rejected: registrations.filter((r) => r.status === 'rejected'),
  };

  if (registrations.length === 0) {
    return (
      <div className="space-y-6">
        <div>
          <h2 className="text-2xl font-bold text-foreground">تسجيلاتي</h2>
          <p className="text-muted-foreground">
            عرض جميع البرامج التي سجلت فيها
          </p>
        </div>

        <Card className="border-none shadow-sm">
          <CardContent className="flex flex-col items-center justify-center py-16">
            <div className="rounded-full bg-muted p-4">
              <GraduationCap className="h-12 w-12 text-muted-foreground" />
            </div>
            <h3 className="mt-4 text-lg font-medium text-foreground">
              لا توجد تسجيلات
            </h3>
            <p className="mt-2 text-center text-muted-foreground">
              لم تقم بالتسجيل في أي برنامج تدريبي بعد
            </p>
            <Button className="mt-4">تصفح البرامج التدريبية</Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold text-foreground">تسجيلاتي</h2>
        <p className="text-muted-foreground">
          عرض جميع البرامج التي سجلت فيها ({registrations.length} تسجيل)
        </p>
      </div>

      {/* Active Registrations */}
      {groupedRegistrations.active.length > 0 && (
        <div className="space-y-4">
          <h3 className="text-lg font-semibold text-foreground">
            التسجيلات النشطة ({groupedRegistrations.active.length})
          </h3>
          <div className="grid gap-4 md:grid-cols-2">
            {groupedRegistrations.active.map((reg) => {
              const statusInfo = getStatusInfo(reg.status);
              const program = getProgramDetails(reg.programId);

              return (
                <Card
                  key={reg.id}
                  className={`overflow-hidden border ${statusInfo.color} shadow-sm`}
                >
                  <CardHeader className="pb-3">
                    <div className="flex items-start justify-between gap-2">
                      <div className="flex items-start gap-3">
                        {statusInfo.icon}
                        <div>
                          <CardTitle className="text-lg">{reg.programTitle}</CardTitle>
                          <p className="mt-1 text-sm text-muted-foreground">
                            {reg.batchName}
                          </p>
                        </div>
                      </div>
                      {statusInfo.badge}
                    </div>
                  </CardHeader>
                  <CardContent className="space-y-3">
                    {program && (
                      <>
                        <div className="flex items-center gap-2 text-sm text-muted-foreground">
                          <Clock className="h-4 w-4" />
                          <span>{program.duration}</span>
                        </div>
                        <div className="flex items-center gap-2 text-sm text-muted-foreground">
                          <User className="h-4 w-4" />
                          <span>{program.instructor}</span>
                        </div>
                        <div className="flex items-center gap-2 text-sm text-muted-foreground">
                          <MapPin className="h-4 w-4" />
                          <span>{program.location}</span>
                        </div>
                      </>
                    )}
                    <div className="flex items-center gap-2 text-sm text-muted-foreground">
                      <Calendar className="h-4 w-4" />
                      <span>تاريخ التسجيل: {reg.registeredAt}</span>
                    </div>
                  </CardContent>
                </Card>
              );
            })}
          </div>
        </div>
      )}

      {/* Completed Registrations */}
      {groupedRegistrations.completed.length > 0 && (
        <div className="space-y-4">
          <h3 className="text-lg font-semibold text-foreground">
            التدريبات المكتملة ({groupedRegistrations.completed.length})
          </h3>
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {groupedRegistrations.completed.map((reg) => {
              const statusInfo = getStatusInfo(reg.status);

              return (
                <Card
                  key={reg.id}
                  className={`overflow-hidden border ${statusInfo.color} shadow-sm`}
                >
                  <CardContent className="flex items-center gap-3 p-4">
                    {statusInfo.icon}
                    <div className="flex-1">
                      <p className="font-medium text-foreground">
                        {reg.programTitle}
                      </p>
                      <p className="text-sm text-muted-foreground">
                        {reg.batchName}
                      </p>
                    </div>
                    {statusInfo.badge}
                  </CardContent>
                </Card>
              );
            })}
          </div>
        </div>
      )}

      {/* Rejected Registrations */}
      {groupedRegistrations.rejected.length > 0 && (
        <div className="space-y-4">
          <h3 className="text-lg font-semibold text-foreground">
            الطلبات المرفوضة ({groupedRegistrations.rejected.length})
          </h3>
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {groupedRegistrations.rejected.map((reg) => {
              const statusInfo = getStatusInfo(reg.status);

              return (
                <Card
                  key={reg.id}
                  className={`overflow-hidden border ${statusInfo.color} shadow-sm`}
                >
                  <CardContent className="flex items-center gap-3 p-4">
                    {statusInfo.icon}
                    <div className="flex-1">
                      <p className="font-medium text-foreground">
                        {reg.programTitle}
                      </p>
                      <p className="text-sm text-muted-foreground">
                        {reg.batchName}
                      </p>
                    </div>
                    <Button variant="outline" size="sm">
                      إعادة التسجيل
                    </Button>
                  </CardContent>
                </Card>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}
