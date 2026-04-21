'use client';

import { cn } from '@/lib/utils';
import {
  LayoutDashboard,
  BookOpen,
  Users,
  ClipboardList,
  BarChart3,
  Settings,
  GraduationCap,
  LogOut,
  Menu,
  X,
} from 'lucide-react';
import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import type { User, UserRole } from '@/lib/types';

interface SidebarProps {
  currentUser: User;
  activeTab: string;
  onTabChange: (tab: string) => void;
}

interface NavItem {
  id: string;
  label: string;
  icon: React.ReactNode;
  roles: UserRole[];
}

const navItems: NavItem[] = [
  {
    id: 'dashboard',
    label: 'لوحة التحكم',
    icon: <LayoutDashboard className="h-5 w-5" />,
    roles: ['admin'],
  },
  {
    id: 'programs',
    label: 'البرامج التدريبية',
    icon: <BookOpen className="h-5 w-5" />,
    roles: ['admin'],
  },
  {
    id: 'registrations',
    label: 'طلبات الترشيح',
    icon: <ClipboardList className="h-5 w-5" />,
    roles: ['admin'],
  },
  {
    id: 'batches',
    label: 'إدارة الدفعات',
    icon: <Users className="h-5 w-5" />,
    roles: ['admin'],
  },
  {
    id: 'reports',
    label: 'التقارير',
    icon: <BarChart3 className="h-5 w-5" />,
    roles: ['admin'],
  },
  {
    id: 'settings',
    label: 'الإعدادات',
    icon: <Settings className="h-5 w-5" />,
    roles: ['admin'],
  },
];

export function Sidebar({ currentUser, activeTab, onTabChange }: SidebarProps) {
  const [isMobileOpen, setIsMobileOpen] = useState(false);

  const filteredNavItems = navItems.filter((item) =>
    item.roles.includes(currentUser.role)
  );

  const getRoleName = (role: UserRole) => {
    switch (role) {
      case 'admin':
        return 'مدير النظام';
      case 'supervisor':
        return 'مشرف';
      case 'employee':
        return 'موظف';
    }
  };

  const SidebarContent = () => (
    <div className="flex h-full flex-col">
      {/* Logo */}
      <div className="flex items-center gap-3 border-b border-border/50 p-6">
        <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary">
          <GraduationCap className="h-6 w-6 text-primary-foreground" />
        </div>
        <div>
          <h1 className="text-lg font-bold text-foreground">نظام التدريب</h1>
          <p className="text-xs text-muted-foreground">إدارة البرامج التدريبية</p>
        </div>
      </div>

      {/* Navigation */}
      <nav className="flex-1 space-y-1 p-4">
        {filteredNavItems.map((item) => (
          <button
            key={item.id}
            onClick={() => {
              onTabChange(item.id);
              setIsMobileOpen(false);
            }}
            className={cn(
              'flex w-full items-center gap-3 rounded-lg px-4 py-3 text-sm font-medium transition-all',
              activeTab === item.id
                ? 'bg-primary text-primary-foreground shadow-md'
                : 'text-muted-foreground hover:bg-muted hover:text-foreground'
            )}
          >
            {item.icon}
            <span>{item.label}</span>
          </button>
        ))}
      </nav>

      {/* User Info */}
      <div className="border-t border-border/50 p-4">
        <div className="flex items-center gap-3 rounded-lg bg-muted/50 p-3">
          <Avatar className="h-10 w-10">
            <AvatarFallback className="bg-primary/10 text-primary">
              {currentUser.name.charAt(0)}
            </AvatarFallback>
          </Avatar>
          <div className="flex-1">
            <p className="text-sm font-medium text-foreground">{currentUser.name}</p>
            <p className="text-xs text-muted-foreground">{getRoleName(currentUser.role)}</p>
          </div>
          <Button variant="ghost" size="icon" className="h-8 w-8">
            <LogOut className="h-4 w-4" />
          </Button>
        </div>
      </div>
    </div>
  );

  return (
    <>
      {/* Mobile Menu Button */}
      <Button
        variant="ghost"
        size="icon"
        className="fixed right-4 top-4 z-50 lg:hidden"
        onClick={() => setIsMobileOpen(!isMobileOpen)}
      >
        {isMobileOpen ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
      </Button>

      {/* Mobile Overlay */}
      {isMobileOpen && (
        <div
          className="fixed inset-0 z-40 bg-background/80 backdrop-blur-sm lg:hidden"
          onClick={() => setIsMobileOpen(false)}
        />
      )}

      {/* Mobile Sidebar */}
      <aside
        className={cn(
          'fixed inset-y-0 right-0 z-40 w-72 transform border-l border-border bg-card transition-transform duration-300 lg:hidden',
          isMobileOpen ? 'translate-x-0' : 'translate-x-full'
        )}
      >
        <SidebarContent />
      </aside>

      {/* Desktop Sidebar */}
      <aside className="hidden h-screen w-72 border-l border-border bg-card lg:block">
        <SidebarContent />
      </aside>
    </>
  );
}
